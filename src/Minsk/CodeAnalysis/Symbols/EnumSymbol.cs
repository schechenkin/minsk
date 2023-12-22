using System.Collections.Immutable;

namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class EnumSymbol : Symbol
    {
        internal EnumSymbol(string name, ImmutableArray<EnumMemberSymbol> members)
            : base(name)
        {
            Members = members;
        }

        public override SymbolKind Kind => SymbolKind.Enum;
        public ImmutableArray<EnumMemberSymbol> Members { get; }
    }
}