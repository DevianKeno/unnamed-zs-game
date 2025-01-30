namespace UZSG.Worlds
{
    [System.Serializable]
    public class WorldManifest
    {
        public string WorldName;
        public string CreatedDate;
        public string LastPlayedDate;
        public string LevelId;
        public string OwnerId;
        
        [Newtonsoft.Json.JsonIgnore] public string WorldRootDirectory;
    }
}