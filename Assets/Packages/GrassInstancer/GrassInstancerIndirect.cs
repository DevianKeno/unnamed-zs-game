using UnityEngine;
using System;
using System.Diagnostics;

namespace UZSG.Worlds
{
    struct GrassChunk
    {
        public Vector3 position; //World position of chunk
        public uint instanceStartIndex; //Start index inside trsBuffer
        public uint instanceCount; //End index inside trsBuffer

        public GrassChunk(Vector3 p)
        {
            position = p;
            instanceStartIndex = 0;
            instanceCount = 0;
        }
    }

    public class GrassInstancerIndirect : MonoBehaviour
    {

        [Header("Debugging")]
        [SerializeField] bool _drawGizmos;
        [Header("Instancing")]
        [SerializeField] int _instances;
        [SerializeField] int _trueInstanceCount;
        [SerializeField] int _visibleInstanceCount;
        [Header("Grass settings")]
        [SerializeField] Vector2 _range;
        [SerializeField] Vector3 _scaleMin = Vector3.one;
        [SerializeField] Vector3 _scaleMax = Vector3.one;
        [SerializeField] float _scaleNoiseScale = 100f;
        [SerializeField][Range(0f, 1f)] float _minGrassHeight = 0.15f;
        [Header("Rendering")]
        [SerializeField] Material _material;
        [SerializeField] Mesh _mesh;
        [SerializeField] bool _castShadows = false;
        [SerializeField] float _maxViewDistance = 512f;
        [SerializeField] ComputeShader _grassComputeShader;
        int _kernelChunkRender;
        int _kernelInitChunkInstanceCount;
        int _kernelInitInstanceTransforms;
        ComputeBuffer _trsBuffer; //Contains all TRS matrices
        ComputeBuffer _visibleBuffer; //Contains indices of visible TRS matrices from _trsBuffer
        ComputeBuffer _argsBuffer;
        ComputeBuffer _readBackArgsBuffer;
        ComputeBuffer _chunkBuffer;
        ComputeBuffer _instanceCounterBuffer; //Needed to atomically count the amount of instances
        Bounds _renderBounds;
        Camera _cam;
        [SerializeField] float _texNoiseLayer1 = 0.01f;
        [SerializeField] float _texNoiseLayer2 = 4.41f;
        [Header("Chunks")]
        [SerializeField] int _chunkSize = 8;
        GrassChunk[] _chunks;
        int _numChunks;
        [SerializeField] uint _threadsChunkRender = 32;
        [SerializeField] int _threadsChunkInit = 32;
        [SerializeField][Range(2, 8)] int _lodLevel1 = 4;
        [SerializeField][Range(2, 16)] int _lodLevel2 = 8;
        [Header("Occlusion")]
        Texture _cameraDepthTexture;
        [SerializeField][Range(0.0001f, 0.1f)] float _depthBias = 0.0001f;
        [Header("Terrain")]
        [SerializeField] Terrain _terrain;
        TerrainData _terrainData;
        [SerializeField][Range(0f, 1f)] float _grassThreshhold = 0.5f;

        void Start()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            _cam = Camera.main;
            _cam.depthTextureMode = DepthTextureMode.Depth;

            _terrainData = _terrain.terrainData;

            _trueInstanceCount = 0;
            _renderBounds = new Bounds(transform.position, _terrainData.size);

            InitializeGrassChunkPositions();
            InitializeGrassChunkInstances();
            stopwatch.Stop();
            print("Init time: " + stopwatch.ElapsedMilliseconds + " ms");
        }

        void Update()
        {
            if (_cameraDepthTexture == null)
            {
                _cameraDepthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");
                _grassComputeShader.SetTextureFromGlobal(_kernelChunkRender, "_DepthTexture", "_CameraDepthTexture");
                return;
            }
            RenderInstances();
        }

        void OnDestroy()
        {
            _argsBuffer?.Release();
            _argsBuffer?.Dispose();
            _trsBuffer?.Release();
            _trsBuffer?.Dispose();
            _visibleBuffer?.Release();
            _visibleBuffer?.Dispose();
            _readBackArgsBuffer?.Release();
            _readBackArgsBuffer?.Dispose();
            _chunkBuffer?.Release();
            _chunkBuffer?.Dispose();
            _instanceCounterBuffer?.Release();
            _instanceCounterBuffer?.Dispose();
        }

        void OnDrawGizmos()
        {
            if (!_drawGizmos) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(_range.x * 2, 5, _range.y * 2));

            if (_chunks == null) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < _chunks.Length; i++)
            {
                if (Vector3.Distance(_chunks[i].position, _cam.transform.position) > 300) continue;
                Gizmos.DrawWireCube(_chunks[i].position, _chunkSize * Vector3.one);
            }
        }


        void RenderInstances()
        {
            UpdateCameraViewProjectionMatrix();

            // if (_mesh == null) return;

            _grassComputeShader.SetVector("camPos", _cam.transform.position);
            _grassComputeShader.SetVectorArray("viewFrustumPlanes", GetViewFrustumPlaneNormals());

            _visibleBuffer.SetCounterValue(0);
            _grassComputeShader.Dispatch(_kernelChunkRender, Mathf.FloorToInt(_numChunks / _threadsChunkRender), 1, 1);

            SetVisibleInstanceCount();

            _argsBuffer.SetData(new uint[5] {
                _mesh.GetIndexCount(0), (uint)_visibleInstanceCount, 0, 0, 0
            });

            Graphics.DrawMeshInstancedIndirect(
            _mesh,
            0,
            _material,
            _renderBounds,
            _argsBuffer,
            0,
            null,
            _castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off
            );
        }

        /// <summary>
        /// Updates vpMatrix property in compute shader. Forced to make our own VP matrix since UNITY_MATRIX_VP does not contain the right values for us.
        /// </summary>
        void UpdateCameraViewProjectionMatrix()
        {
            Matrix4x4 mat = GL.GetGPUProjectionMatrix(_cam.projectionMatrix, false) * _cam.worldToCameraMatrix;
            _grassComputeShader.SetMatrix("vpMatrix", mat);
        }

        /// <summary>
        /// Fetches amount of elements inside _visibleBuffer with an additional buffer.
        /// Stores result in _visibleInstanceCount.
        /// </summary>
        void SetVisibleInstanceCount()
        {
            ComputeBuffer.CopyCount(_visibleBuffer, _readBackArgsBuffer, 0);
            int[] appendBufferCount = new int[1];
            _readBackArgsBuffer.GetData(appendBufferCount);
            _visibleInstanceCount = appendBufferCount[0];
        }

        /// <summary>
        /// Initializes grass chunk array with their x and z positions in a grid.
        /// </summary>
        void InitializeGrassChunkPositions()
        {
            //whole range from -range.x to range.x
            int wholeRangeX = (int)(_range.x * 2);
            int wholeRangeZ = (int)(_range.y * 2);

            _numChunks = Mathf.CeilToInt(wholeRangeX / _chunkSize) * Mathf.CeilToInt(wholeRangeZ / _chunkSize);
            _chunks = new GrassChunk[_numChunks];

            //Used for centering grid
            int chunkSizeHalf = Mathf.CeilToInt(_chunkSize / 2);

            int startOffsetX = Mathf.CeilToInt(_range.x - chunkSizeHalf);
            int startOffsetZ = Mathf.CeilToInt(_range.y - chunkSizeHalf);
            Vector3 gridStartPos = transform.position - new Vector3(startOffsetX, 0, startOffsetZ);

            int xOffset = 0;
            int zOffset = 0;

            int chunksPerRow = wholeRangeX / _chunkSize;

            for (int i = 0; i < _numChunks; i++)
            {
                Vector3 p = gridStartPos;
                p.x += _chunkSize * xOffset;
                p.z += _chunkSize * zOffset;

                bool isSameRow = (i % chunksPerRow) < chunksPerRow - 1;

                xOffset = isSameRow ? xOffset + 1 : 0; //if same row then continue forward, else start at 0 again
                zOffset = isSameRow ? zOffset : zOffset + 1; //if same row keep z else got to the right (next row)

                _chunks[i] = new GrassChunk(p);
            }
        }

        /// <summary>
        /// Initializes grass instances for each chunk via compute shader
        /// </summary>
        void InitializeGrassChunkInstances()
        {
            int instancesPerChunk = Mathf.CeilToInt(_instances / _numChunks);
            print("Num chunks : " + _numChunks);
            print("Instances per chunk : " + instancesPerChunk);

            _kernelChunkRender = _grassComputeShader.FindKernel("ChunkRender");
            _kernelInitChunkInstanceCount = _grassComputeShader.FindKernel("InitChunkInstanceCount");
            _kernelInitInstanceTransforms = _grassComputeShader.FindKernel("InitInstanceTransforms");

            _grassComputeShader.SetFloat("depthBias", _depthBias);
            _grassComputeShader.SetFloat("maxViewDistance", _maxViewDistance);

            _grassComputeShader.SetFloat("grassThreshhold", _grassThreshhold);
            _grassComputeShader.SetFloat("minGrassHeight", _minGrassHeight);
            _grassComputeShader.SetFloat("halfChunkSize", _chunkSize / 2f);

            _grassComputeShader.SetInt("lodLevel1", _lodLevel1);
            _grassComputeShader.SetInt("lodLevel2", _lodLevel2);
            _grassComputeShader.SetInt("numChunks", _numChunks);
            _grassComputeShader.SetInt("chunkSize", _chunkSize);
            _grassComputeShader.SetInt("instancesPerChunk", instancesPerChunk);

            _grassComputeShader.SetVectorArray("viewFrustumPlanes", GetViewFrustumPlaneNormals());

            _grassComputeShader.SetVector("scaleMin", _scaleMin);
            _grassComputeShader.SetVector("scaleMax", _scaleMax);
            _grassComputeShader.SetFloat("scaleNoiseScale", _scaleNoiseScale);

            _grassComputeShader.SetVector("terrainSize", _terrainData.size);
            _grassComputeShader.SetVector("terrainPos", _terrain.transform.position);
            _grassComputeShader.SetInt("terrainHeightmapResolution", _terrainData.heightmapResolution);

            _grassComputeShader.SetTexture(_kernelInitInstanceTransforms, "Heightmap", _terrainData.heightmapTexture);
            _grassComputeShader.SetTexture(_kernelInitInstanceTransforms, "Splatmap", _terrainData.alphamapTextures[0]);

            _grassComputeShader.SetTexture(_kernelInitChunkInstanceCount, "Heightmap", _terrainData.heightmapTexture);
            _grassComputeShader.SetTexture(_kernelInitChunkInstanceCount, "Splatmap", _terrainData.alphamapTextures[0]);


            _argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
            _readBackArgsBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

            _instanceCounterBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            _instanceCounterBuffer.SetData(new int[] { 0 });

            _chunkBuffer = new ComputeBuffer(_numChunks, 3 * sizeof(float) + 2 * sizeof(int));
            _chunkBuffer.SetData(_chunks);

            _grassComputeShader.SetBuffer(_kernelInitChunkInstanceCount, "chunkBuffer", _chunkBuffer);
            _grassComputeShader.SetBuffer(_kernelInitChunkInstanceCount, "instanceCounter", _instanceCounterBuffer);

            _grassComputeShader.SetBuffer(_kernelInitInstanceTransforms, "chunkBuffer", _chunkBuffer);
            _grassComputeShader.SetBuffer(_kernelInitInstanceTransforms, "instanceCounter", _instanceCounterBuffer);

            // First calculating instances per chunk
            _grassComputeShader.Dispatch(_kernelInitChunkInstanceCount, Mathf.CeilToInt(_numChunks / _threadsChunkInit), 1, 1);

            // _instanceCounterBuffer now contains sum of all instances of all chunks
            int[] cb = new int[1];
            _instanceCounterBuffer.GetData(cb);
            _trueInstanceCount = cb[0];

            // Now generate the TRS buffer for rendering
            _trsBuffer = new ComputeBuffer(_trueInstanceCount, sizeof(float) * 4 * 4);
            _grassComputeShader.SetBuffer(_kernelInitInstanceTransforms, "trsBuffer", _trsBuffer);
            // Create buffer for indexing the TRS buffer in the material
            _visibleBuffer = new ComputeBuffer(_trueInstanceCount, sizeof(uint), ComputeBufferType.Append);
            _grassComputeShader.SetBuffer(_kernelChunkRender, "visibleList", _visibleBuffer);
            _grassComputeShader.SetBuffer(_kernelChunkRender, "trsBuffer", _trsBuffer);
            _grassComputeShader.SetBuffer(_kernelChunkRender, "chunkBuffer", _chunkBuffer);

            // Fill buffer for rendering with chunk data
            _grassComputeShader.Dispatch(_kernelInitInstanceTransforms, Mathf.CeilToInt(_numChunks / _threadsChunkInit), 1, 1);

            _material.SetBuffer("visibleList", _visibleBuffer);
            _material.SetBuffer("trsBuffer", _trsBuffer);
            _material.SetFloat("_NoiseScale", _texNoiseLayer1);
            _material.SetFloat("_NoiseScale2", _texNoiseLayer2);

            _instanceCounterBuffer?.Release();

            if (!_drawGizmos)
            {
                _chunks = null;
                return;
            }
            _chunkBuffer.GetData(_chunks);
        }

        Vector4[] GetViewFrustumPlaneNormals()
        {
            Vector4[] planeNormals = new Vector4[6];
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_cam);

            for (int i = 0; i < 6; i++)
            {
                planeNormals[i] = new Vector4(planes[i].normal.x, planes[i].normal.y, planes[i].normal.z, planes[i].distance);
            }
            return planeNormals;
        }
    }
}