namespace UZSG.EOS
{
    /// <summary>
    /// Class represents RTC State (Voice) of a Lobby
    /// </summary>
    public class LobbyRTCState
    {
        /// Is this person currently connected to the RTC room?
        public bool IsInRTCRoom = false;

        /// Is this person currently talking (audible sounds from their audio output)
        public bool IsTalking = false;

        /// We have locally muted this person (others can still hear them)
        public bool IsLocalMuted = false;

        /// Has this person muted their own audio output (nobody can hear them)
        public bool IsAudioOutputDisabled = false;

        /// Are we currently muting this person?
        public bool MuteActionInProgress = false;

        /// Has this person enabled press to talk
        public bool PressToTalkEnabled = false;
    }
}