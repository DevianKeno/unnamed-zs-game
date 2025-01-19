using Epic.OnlineServices.UserInfo;

namespace UZSG.Entities
{
    /// <summary>
    /// Player network entity.
    /// </summary>
    public class NetworkPlayerEntity : Entity
    {
        public UserInfoData UserInfo { get; set; }
    }
}