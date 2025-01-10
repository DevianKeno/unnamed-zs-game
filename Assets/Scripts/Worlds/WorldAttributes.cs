using System;
using Epic.OnlineServices;

namespace UZSG.Worlds
{
    public enum WorldMultiplayerType {
        Internet, Friends, LAN,
    }

    [Serializable]
    public struct WorldAttributes
    {
        public const int DAY_START_HOUR = 6; /// 6:00 AM
        public const int NIGHT_START_HOUR = 21; /// 9:00 PM
        public const int DEFAULT_DAY_LENGTH = 2160; /// 36 minutes
        public const int MIN_DAY_LENGTH = 720; /// 12 minutes
        public const int MAX_DAY_LENGTH = 4320; /// 72 minutes
        public const int DEFAULT_NIGHT_LENGTH = 600; /// 10 minutes
        public const int MIN_NIGHT_LENGTH = 300; /// 5 minutes
        public const int MAX_NIGHT_LENGTH = 1440; /// 24 minutes

        public string LevelId;
        public string WorldName;
        public int DayLengthSeconds;
        public int MaxPlayers;
        public int DifficultyLevel;
        public bool IsMultiplayer;
        public WorldMultiplayerType WorldMultiplayerType;
    }
}