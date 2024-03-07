namespace Minsk.CodeAnalysis.Syntax
{
    public sealed partial class TypeClauseSyntax : SyntaxNode
    {
        internal TypeClauseSyntax(SyntaxTree syntaxTree, SyntaxToken colonToken, SyntaxToken identifier, SyntaxToken? openBraceToken, SyntaxToken? closeBraceToken)
            : base(syntaxTree)
        {
            ColonToken = colonToken;
            Identifier = identifier;
            OpenBraceToken = openBraceToken;
            CloseBraceToken = closeBraceToken;
        }

        internal TypeClauseSyntax(SyntaxTree syntaxTree, SyntaxToken colonToken, SyntaxToken identifier)
            : base(syntaxTree)
        {
            ColonToken = colonToken;
            Identifier = identifier;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeClause;
        public SyntaxToken ColonToken { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken? OpenBraceToken { get; }
        public SyntaxToken? CloseBraceToken { get; }
        public bool IsArray => OpenBraceToken != null;
    }
}