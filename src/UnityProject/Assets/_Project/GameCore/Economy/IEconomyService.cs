using System.Threading.Tasks;

namespace Game.Core.Economy
{
    /// <summary>
    /// Point d'extension économie P2P (ARCHITECTURE.md §5). Conformément au cadrage de
    /// mentalyas et à la classification SCOPE.md (IMPORTANT, pas CORE-jour-1), le système
    /// anti-fraude/trading n'est PAS conçu ici — uniquement le point d'extension, satisfait
    /// pour le MVP par <see cref="StubEconomyService"/>.
    /// </summary>
    public interface IEconomyService
    {
        Task<Balance> GetBalanceAsync(PlayerId playerId);
        Task<TransactionResult> ExecuteTradeAsync(TradeRequest request);
    }
}
