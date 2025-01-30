namespace UZSG.EOS.P2P
{
    public enum PacketSubtype {
        RequestWorldData,
        ReceiveWorldDataHeading,
        ReceiveWorldDataChunk,
        ReceiveWorldDataFooter,
        RequestPlayerSaveData,
        ReceivePlayerSaveData,
        SpawnMyPlayer,
        SpawnMyPlayerCompleted,
    }
}