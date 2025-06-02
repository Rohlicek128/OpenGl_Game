namespace OpenGl_Game.Game.Upgrading;

public struct UpgradeLevel
{
    public uint Level { get; set; }
    public float Price { get; set; }
    public float Amount { get; set; }

    public UpgradeLevel(uint level, float price, float amount = 0f)
    {
        Level = level;
        Price = price;
        Amount = amount;
    }
}