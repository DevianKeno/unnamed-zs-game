using System;
using UnityEngine;

namespace URMG.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "URMG/Item")]
    [Serializable]
    public class ItemData : ScriptableObject
    {
        public string Id;
        public string Name;
        [TextArea] public string Description;
        public Sprite Sprite;
        public bool IsStackable;
        public int MaxStackSize;
    }
}