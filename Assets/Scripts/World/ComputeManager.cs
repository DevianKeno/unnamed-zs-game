using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UZSG.Systems;

namespace UZSG.World
{
    public class ComputeManager : MonoBehaviour
    {
        public ComputeShader noiseShader;
        public ComputeShader voxelShader;

        private List<MeshBuffer> allMeshComputeBuffers = new List<MeshBuffer>();
        private Queue<MeshBuffer> availableMeshComputeBuffers = new Queue<MeshBuffer>();

        private List<NoiseBuffer> allNoiseComputeBuffers = new List<NoiseBuffer>();
        private Queue<NoiseBuffer> availableNoiseComputeBuffers = new Queue<NoiseBuffer>();

        ComputeBuffer noiseLayersArray;
        ComputeBuffer voxelColorsArray;

        private int xThreads;
        private int yThreads;
        public int numberMeshBuffers = 0;

        [Header("Noise Settings")]
        public int seed;
        public NoiseLayers[] noiseLayers;

        static float ColorfTo32(Color32 c)
        {
            if (c.r == 0) c.r = 1;
            if (c.g == 0) c.g = 1;
            if (c.b == 0) c.b = 1;
            if (c.a == 0) c.a = 1;
            return (c.r << 24) | (c.g << 16) | (c.b << 8) | (c.a);
        }

        public void Initialize(int count)
        {
            xThreads = Game.World.Attributes.ChunkSize / 8 + 1;
            yThreads = Game.World.Attributes.WorldHeight / 8;
        
            noiseLayersArray = new ComputeBuffer(noiseLayers.Length, 36);
            noiseLayersArray.SetData(noiseLayers);

            noiseShader.SetInt("containerSizeX", Game.World.Attributes.ChunkSize);
            noiseShader.SetInt("containerSizeY", Game.World.Attributes.WorldHeight);

            noiseShader.SetBool("generateCaves", true);
            noiseShader.SetBool("forceFloor", true);

            noiseShader.SetInt("maxHeight", Game.World.Attributes.WorldHeight);
            noiseShader.SetInt("oceanHeight", 42);
            noiseShader.SetInt("seed", seed);

            noiseShader.SetBuffer(0, "noiseArray", noiseLayersArray);
            noiseShader.SetInt("noiseCount", noiseLayers.Length);

            VoxelColor32[] converted = new VoxelColor32[Game.World.WorldColors.Length];
            int cCount = 0;

            foreach (VoxelColor c in Game.World.WorldColors)
            {
                VoxelColor32 c32 = new()
                {
                    Color = ColorfTo32(c.Color)
                };

                converted[cCount++] = c32;
            }

            voxelColorsArray = new ComputeBuffer(converted.Length, 8);
            voxelColorsArray.SetData(converted);

            voxelShader.SetBuffer(0, "voxelColors", voxelColorsArray);
            voxelShader.SetInt("containerSizeX", Game.World.Attributes.ChunkSize);
            voxelShader.SetInt("containerSizeY", Game.World.Attributes.WorldHeight);

            for (int i = 0; i < count; i++)
            {
                CreateNewNoiseBuffer();
                CreateNewMeshBuffer();
            }
        }

        public void GenerateVoxelData(Container cont, Vector3 pos)
        {

            NoiseBuffer noiseBuffer = GetNoiseBuffer();
            noiseBuffer.countBuffer.SetCounterValue(0);
            noiseBuffer.countBuffer.SetData(new uint[] { 0 });
            noiseShader.SetBuffer(0, "voxelArray", noiseBuffer.noiseBuffer);
            noiseShader.SetBuffer(0, "count", noiseBuffer.countBuffer);

            noiseShader.SetVector("chunkPosition", cont.Position);
            noiseShader.SetVector("seedOffset", Vector3.zero);

            noiseShader.Dispatch(0, xThreads, yThreads, xThreads);

            MeshBuffer meshBuffer = GetMeshBuffer();
            meshBuffer.CountBuffer.SetCounterValue(0);
            meshBuffer.CountBuffer.SetData(new uint[] { 0, 0 });
            voxelShader.SetVector("chunkPosition", cont.Position);

            voxelShader.SetBuffer(0, "voxelArray", noiseBuffer.noiseBuffer);
            voxelShader.SetBuffer(0, "counter", meshBuffer.CountBuffer);
            voxelShader.SetBuffer(0, "vertexBuffer", meshBuffer.VertexBuffer);
            voxelShader.SetBuffer(0, "colorBuffer", meshBuffer.ColorBuffer);
            voxelShader.SetBuffer(0, "indexBuffer", meshBuffer.IndexBuffer);
            voxelShader.Dispatch(0, xThreads, yThreads, xThreads);

            AsyncGPUReadback.Request(meshBuffer.CountBuffer, (callback) =>
            {
                if (Game.World.activeContainers.ContainsKey(pos))
                {
                    Game.World.activeContainers[pos].UploadMesh(meshBuffer);
                }
                ClearAndRequeueBuffer(noiseBuffer);
                ClearAndRequeueBuffer(meshBuffer);

            });
        }

        private void ClearVoxelData(NoiseBuffer buffer)
        {
            buffer.countBuffer.SetData(new int[] { 0 });
            noiseShader.SetBuffer(1, "voxelArray", buffer.noiseBuffer);
            noiseShader.Dispatch(1, xThreads, yThreads, xThreads);
        }

        #region MeshBuffer Pooling
        public MeshBuffer GetMeshBuffer()
        {
            if (availableMeshComputeBuffers.Count > 0)
            {
                return availableMeshComputeBuffers.Dequeue();
            }
            else
            {
                Debug.Log("Generate container");
                return CreateNewMeshBuffer(false);
            }
        }

        public MeshBuffer CreateNewMeshBuffer(bool enqueue = true)
        {
            MeshBuffer buffer = new MeshBuffer();
            buffer.InitializeBuffer();
            
            allMeshComputeBuffers.Add(buffer);
            
            if (enqueue)
                availableMeshComputeBuffers.Enqueue(buffer);
            
            numberMeshBuffers++;

            return buffer;
        }

        public void ClearAndRequeueBuffer(MeshBuffer buffer)
        {
            availableMeshComputeBuffers.Enqueue(buffer);
        }
        #endregion

        #region NoiseBuffer Pooling
        public NoiseBuffer GetNoiseBuffer()
        {
            if (availableNoiseComputeBuffers.Count > 0)
            {
                return availableNoiseComputeBuffers.Dequeue();
            }
            else
            {
                return CreateNewNoiseBuffer(false);
            }
        }

        public NoiseBuffer CreateNewNoiseBuffer(bool enqueue = true)
        {
            NoiseBuffer buffer = new NoiseBuffer();
            buffer.InitializeBuffer();
            allNoiseComputeBuffers.Add(buffer);

            if (enqueue)
                availableNoiseComputeBuffers.Enqueue(buffer);

            return buffer;
        }

        public void ClearAndRequeueBuffer(NoiseBuffer buffer)
        {
            ClearVoxelData(buffer);
            availableNoiseComputeBuffers.Enqueue(buffer);
        }
        #endregion

        private void OnApplicationQuit()
        {
            DisposeAllBuffers();
        }

        public void DisposeAllBuffers()
        {
            noiseLayersArray?.Dispose();
            voxelColorsArray?.Dispose();
            foreach (NoiseBuffer buffer in allNoiseComputeBuffers)
                buffer.Dispose();
            foreach (MeshBuffer buffer in allMeshComputeBuffers)
                buffer.Dispose();
        }
    }

    public struct NoiseBuffer
    {
        public ComputeBuffer noiseBuffer;
        public ComputeBuffer countBuffer;
        public bool Initialized;
        public bool Cleared;

        public void InitializeBuffer()
        {
            countBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Counter);
            countBuffer.SetCounterValue(0);
            countBuffer.SetData(new uint[] { 0 });

            //voxelArray = new IndexedArray<Voxel>();
            noiseBuffer = new ComputeBuffer(Game.World.Attributes.ChunkCount, 4);
            Initialized = true;
        }

        public void Dispose()
        {
            countBuffer?.Dispose();
            noiseBuffer?.Dispose();

            Initialized = false;
        }
    }
}
