using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;

using UZSG.Attributes;
using UZSG.Items;

namespace UZSG.Data
{
    [Serializable]
    public struct ResourceInstances
    {
        public string Name;
        public AssetReference Model;
        public List<AttributeData> Attributes;
    }

    public enum HarvestType {
        PerAction, OnDestroy
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Resource Object Data", menuName = "UZSG/Objects/Resource Object Data")]
    public class ResourceData : ObjectData
    {
        [Header("Resource")]
        /// <summary>
        /// The best tool to harvest this resource.
        /// </summary>
        public ToolType ToolType;
        public HarvestType HarvestType;
        /// <summary>
        /// Items given to the Player harvesting this resource. Will depend on the HarvestType.
        /// </summary>
        public Item Yield;
        public List<Item> Yields; /// lmao
        public bool IsPickup;
        public float PickupDuration;
        public float MaxInteractDistance;
        public bool UseInstances;
        public List<ResourceInstances> Instances;
    }
}