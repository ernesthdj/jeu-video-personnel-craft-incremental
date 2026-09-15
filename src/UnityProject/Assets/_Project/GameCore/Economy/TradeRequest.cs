namespace Game.Core.Economy
{
    /// <summary>Requête d'échange marché P2P — non implémentée en MVP (SCOPE.md : IMPORTANT, pas CORE-jour-1).</summary>
    public sealed class TradeRequest
    {
        public PlayerId BuyerId { get; init; }
        public PlayerId SellerId { get; init; }
        public string ItemDefinitionId { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public int PricePremiumCurrency { get; init; }
    }
}
