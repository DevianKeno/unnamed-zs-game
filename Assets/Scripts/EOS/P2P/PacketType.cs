namespace UZSG.EOS.P2P
{
    public enum PacketType {
        UNDEFINED,
        ChatMessage,
        PlayerAction,
        WorldDataRequest,
        WorldDataHeading,
        WorldDataChunk,
        WorldDataFooter,
        PlayerSaveDataRequest,
        PlayerSaveDataReceived,
        ConnectionRequest,
    }
}