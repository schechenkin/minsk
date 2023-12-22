using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundEnumMemberAccessExpression : BoundExpression
    {
        public BoundEnumMemberAccessExpression(SyntaxNode syntax, EnumSymbol enumType, EnumMemberSymbol enumMember)
            : base(syntax)
        {
            EnumSymbol = enumType;
            EnumMember = enumMember;
        }

        public override BoundNodeKind Kind => BoundNodeKind.EnumMemberAccessExpression;
        public override TypeSymbol Type => TypeSymbol.Enum(EnumSymbol.Name);
        public EnumSymbol EnumSymbol { get; }
        public EnumMemberSymbol EnumMember { get; }
    }
}
