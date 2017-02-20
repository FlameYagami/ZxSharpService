namespace ZxSharpService.Game.Enums
{
    public enum StocMessage
    {
        CreateGame = 41,
        JoinGame = 42,
        LeaveGame = 43,


        GameMsg = 0x1,
        ErrorMsg = 0x2,
        SelectHand = 0x3,
        SelectTp = 0x4,
        HandResult = 0x5,
        TpResult = 0x6,
        ChangeSide = 0x7,
        WaitingSide = 0x8,
        TypeChange = 0x13,
        DuelStart = 0x15,
        DuelEnd = 0x16,
        Replay = 0x17,
        TimeLimit = 0x18,
        Chat = 0x19,
        HsPlayerEnter = 0x20,
        HsPlayerChange = 0x21,
        HsWatchChange = 0x22
    }
}