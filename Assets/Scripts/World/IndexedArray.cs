using System;
using UnityEngine;
using UZSG.Systems;

namespace UZSG.World
{
    [Serializable]
    public class IndexedArray<T> where T : struct
    {
        bool _isInitialized = false;

        [SerializeField] T[] _array;
        public T[] Array => _array;
        [SerializeField] Vector2Int _size;

        public IndexedArray()
        {
            Create(Game.World.Attributes.ChunkSize, Game.World.Attributes.WorldHeight);
        }

        public IndexedArray(int sizeX, int sizeY)
        {
            Create(sizeX, sizeY);
        }

        private void Create(int sizeX, int sizeY)
        {
            _size = new Vector2Int(sizeX + 3, sizeY + 1);
            _array = new T[Count];
            _isInitialized = true;
        }

        int IndexFromCoord(Vector3 idx)
        {
            return Mathf.RoundToInt(idx.x) + (Mathf.RoundToInt(idx.y) * _size.x) + (Mathf.RoundToInt(idx.z) * _size.x * _size.y);
        }


        public void Clear()
        {
            if (!_isInitialized)
                return;

            for (int x = 0; x < _size.x; x++)
                for (int y = 0; y < _size.y; y++)
                    for (int z = 0; z < _size.x; z++)
                        _array[x + (y * _size.x) + (z * _size.x * _size.y)] = default;
        }

        public int Count => _size.x * _size.y * _size.x;
        public T[] GetData => _array;

        public T this[Vector3 pos]
        {
            get
            {
                if (pos.x < 0 || pos.x > _size.x ||
                    pos.y < 0 || pos.y > _size.y ||
                    pos.z < 0 || pos.z > _size.x)
                {
                    Debug.LogError($"Coordinates out of bounds! {pos}");
                    return default;
                }
                return _array[IndexFromCoord(pos)];
            }
            set
            {
                if (pos.x < 0 || pos.x >= _size.x ||
                    pos.y < 0 || pos.y >= _size.y ||
                    pos.z < 0 || pos.z >= _size.x)
                {
                    Debug.LogError($"Coordinates out of bounds! {pos}");
                    return;
                }
                _array[IndexFromCoord(pos)] = value;
            }
        }
    }
}