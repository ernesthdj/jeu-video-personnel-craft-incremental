using System.Threading.Tasks;

namespace Game.Core.Economy
{
    /// <summary>
    /// Implémentation stub (ARCHITECTURE.md §5) : monnaie soft gérée localement à zéro
    /// fixe (le MVP ne relie pas encore cette interface à l'inventaire réel), toute
    /// opération de marché retourne "non disponible". Aucune logique anti-fraude, aucun
    /// appel réseau — exactement ce que demande le cadrage de cette phase.
    /// </summary>
    public sealed class StubEconomyService : IEconomyService
    {
        public Task<Balance> GetBalanceAsync(PlayerId playerId) =>
            Task.FromResult(new Balance(softCurrency: 0, premiumCurrency: 0));

        public Task<TransactionResult> ExecuteTradeAsync(TradeRequest request) =>
            Task.FromResult(new TransactionResult
            {
                Success = false,
                Message = "Marché P2P non disponible (StubEconomyService — voir ARCHITECTURE.md §5, SCOPE.md).",
            });
    }
}
