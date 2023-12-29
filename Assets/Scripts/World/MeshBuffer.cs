using UnityEngine;
using UZSG.Systems;

namespace UZSG.World
{
    public struct MeshBuffer
    {
        public ComputeBuffer VertexBuffer;
        public ComputeBuffer ColorBuffer;
        public ComputeBuffer IndexBuffer;
        public ComputeBuffer CountBuffer;

        public bool IsInitialized;
        public bool Cleared;

        public void InitializeBuffer()
        {
            if (IsInitialized) return;

            CountBuffer = new ComputeBuffer(2, 4, ComputeBufferType.Counter);
            CountBuffer.SetCounterValue(0);
            CountBuffer.SetData(new uint[] { 0, 0 });

            int maxTriangles = Game.World.Attributes.ChunkSize * Game.World.Attributes.WorldHeight * Game.World.Attributes.ChunkSize / 4;

            VertexBuffer ??= new ComputeBuffer(maxTriangles * 3, 12);
            ColorBuffer ??= new ComputeBuffer(maxTriangles * 3, 16);;
            IndexBuffer ??= new ComputeBuffer(maxTriangles * 3, 4);

            IsInitialized = true;
        }

        public void Dispose()
        {
            VertexBuffer?.Dispose();
            ColorBuffer?.Dispose();
            IndexBuffer?.Dispose();
            CountBuffer?.Dispose();

            IsInitialized = false;
        }
    }
}
