namespace Game.Core.Economy
{
    public sealed class TransactionResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
