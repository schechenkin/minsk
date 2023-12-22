namespace Minsk.CodeAnalysis.Syntax
{
    public sealed partial class EnumMemberAccessExpressionSyntax : ExpressionSyntax
    {
        internal EnumMemberAccessExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken enumTypeIdentifier, SyntaxToken dotToken, SyntaxToken enumMemberIdentifier)
            : base(syntaxTree)
        {
            EnumTypeIdentifier = enumTypeIdentifier;
            DotToken = dotToken;
            EnumMemberIdentifier = enumMemberIdentifier;
        }

        public override SyntaxKind Kind => SyntaxKind.EnumMemberAccessExpression;

        public SyntaxToken EnumTypeIdentifier { get; }
        public SyntaxToken DotToken { get; }
        public SyntaxToken EnumMemberIdentifier { get; }
    }
}