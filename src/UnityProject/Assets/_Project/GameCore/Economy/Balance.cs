namespace Game.Core.Economy
{
    public readonly struct Balance
    {
        public int SoftCurrency { get; }
        public int PremiumCurrency { get; }

        public Balance(int softCurrency, int premiumCurrency)
        {
            SoftCurrency = softCurrency;
            PremiumCurrency = premiumCurrency;
        }
    }
}
