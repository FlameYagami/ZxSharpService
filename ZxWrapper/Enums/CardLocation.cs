namespace ZxWrapper.Enums
{
    public enum CardLocation
    {
        Square = 0x01, // 方阵
        Resource = 0x02, //资源
        Deck = 0x04, // 卡组
        Removed = 0x08, // 除外
        Trash = 0x10, // 废弃
        Charge = 0x20, // 充能
        Life = 0x40, // 生命 
        Dynamis = 0x80, // 额外
        Hand = 0xA0 // 手牌
    }
}