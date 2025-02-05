using System;

namespace UZSG.Systems
{
    public struct CreateWorldOptions
    {
        public string WorldName { get; set; }
        public string MapId { get; set; }
        public int Seed { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string OwnerId { get; set; }
    }
}