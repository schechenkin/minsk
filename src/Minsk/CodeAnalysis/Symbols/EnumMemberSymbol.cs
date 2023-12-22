namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class EnumMemberSymbol : Symbol
    {
        internal EnumMemberSymbol(string name, int ordinal)
            : base(name)
        {
            Ordinal = ordinal;
        }

        public override SymbolKind Kind => SymbolKind.EnumMember;
        public int Ordinal { get; }
    }
}