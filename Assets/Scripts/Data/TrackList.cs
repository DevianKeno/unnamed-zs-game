using System;
using System.Collections.Generic;

using UnityEngine;

namespace UZSG.Data
{
    /// <summary>
    /// A list of tracks (audio clips) stored as a scriptable object.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New Track List", menuName = "UZSG/Audio/Track List")]
    public class TrackList : BaseData
    {
        public List<AudioClip> AudioClips;
    }
}