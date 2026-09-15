namespace Game.Core.Economy
{
    public readonly struct PlayerId
    {
        public string Value { get; }
        public PlayerId(string value) => Value = value;
        public override string ToString() => Value;
    }
}
