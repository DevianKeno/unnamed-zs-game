using Epic.OnlineServices;
using Epic.OnlineServices.Friends;

namespace UZSG.EOS.Friends
{
    /// <summary>
    /// Stores Friend data.
    /// </summary>
    public class FriendData
    {
        public EpicAccountId LocalUserId;
        public EpicAccountId UserId;
        public ProductUserId UserProductUserId;
        public string Name;
        public FriendsStatus Status = FriendsStatus.NotFriends;
        public PresenceInfo Presence;

        public bool IsFriend()
        {
            return Status == FriendsStatus.Friends;
        }

        public bool IsOnline()
        {
            return Presence?.Status == Epic.OnlineServices.Presence.Status.Online;
        }
    }
}