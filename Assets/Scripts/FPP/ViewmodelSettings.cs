using System;
using System.Collections.Generic;
using UnityEngine;

namespace UZSG.FPP
{
    [Serializable]
    public class ViewmodelBobPreset
    {
        public string Id;
        
        [Header("Bobbing")]
        public BobSettings BobSettings;

        [Header("Transform")]
        /// <summary>
        /// The position of the viewmodel in this bob state.
        /// Typically a "low ready side" position. idk
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// The position of the viewmodel in this bob state.
        /// Typically a "low ready side" position. idk
        /// </summary>
        public Vector3 Rotation;
        public float Damping;
    }

    [Serializable]
    public class ViewmodelSettings
    {
        public bool UseOffsets;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
        public List<ViewmodelBobPreset> BobbingPresets;
    }
}
