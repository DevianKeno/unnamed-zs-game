using System;
using System.Collections.Generic;

using UnityEngine;

namespace UZSG.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Track List", menuName = "UZSG/Audio/Track List")]
    public class TrackList : BaseData
    {
        public List<AudioClip> AudioClips;
    }
}