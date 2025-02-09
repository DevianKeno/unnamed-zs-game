using System;
using System.Collections.Generic;

namespace UZSG.Data
{
    /// <summary>
    /// Audio data for equipment.
    /// Values are set in Inspector; <b>Do not write</b>.
    /// </summary>
    [Serializable]
    public struct EquipmentAudioData
    {
        public List<AudioAssetId> AudioAssetIds;
    }
}
