using System;

namespace UZSG.Worlds
{
    public enum WorldMultiplayerType {
        Internet, Friends, LAN,
    }

    [Serializable]
    public struct WorldAttributes
    {
        public string LevelId;
        public string WorldName;
        public int MaxPlayers;
        public int DifficultyLevel;
        public bool IsMultiplayer;
        public WorldMultiplayerType WorldMultiplayerType;
    }
}