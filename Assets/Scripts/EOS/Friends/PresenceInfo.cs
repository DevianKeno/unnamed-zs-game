using Epic.OnlineServices.Presence;

namespace UZSG.EOS.Friends
{
    /// <summary>
    /// Stores Player Presence data.
    /// </summary>
    public class PresenceInfo
    {
        public Status Status = Status.Offline;
        public string RichText;
        public string Application;
        public string Platform;
        public string JoinInfo;
    }
}