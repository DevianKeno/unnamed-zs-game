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
        public const int MIN_SEED = int.MinValue;
        public const int MAX_SEED = int.MaxValue;
        public const int MIN_MAX_NUM_PLAYERS = 1;
        public const int MAX_MAX_NUM_PLAYERS = 64;
        public const int DEFAULT_MAX_NUM_PLAYERS = 8;
        public const int DAY_START_HOUR = 6; /// 6:00 AM
        public const int NIGHT_START_HOUR = 21; /// 9:00 PM
        public const int DEFAULT_DAY_LENGTH = 2400; /// 40 minutes
        public const int MIN_DAY_LENGTH = 900; /// 15 minutes
        public const int MAX_DAY_LENGTH = 5400; /// 90 minutes
        public const int DEFAULT_NIGHT_LENGTH = 600; /// 10 minutes
        public const int MIN_NIGHT_LENGTH = 300; /// 5 minutes
        public const int MAX_NIGHT_LENGTH = 1800; /// 30 minutes


        #region Generation Settings

        public const float TREE_DENSITY = 1f;

        #endregion

        public string LevelId;
        public string WorldName;


        #region World

        public int DayLengthSeconds;
        public int NightLengthSeconds;
        public bool LootRespawns;

        #endregion


        #region Gameplay

        public bool DropItemsOnDeath;
        public int DifficultyLevel;

        #endregion


        #region Multiplayer

        public int MaxPlayers;
        public string Password;
        public bool IsMultiplayer;
        public WorldMultiplayerType WorldMultiplayerType;

        #endregion
        

        public static void Validate(ref WorldAttributes attributes)
        {
            attributes.MaxPlayers = Math.Clamp(attributes.MaxPlayers, MIN_MAX_NUM_PLAYERS, MAX_MAX_NUM_PLAYERS);
            attributes.DayLengthSeconds = Math.Clamp(attributes.DayLengthSeconds, MIN_DAY_LENGTH, MAX_DAY_LENGTH);
            /// TODO: few for testing, soon add the rest
        }
    }
}