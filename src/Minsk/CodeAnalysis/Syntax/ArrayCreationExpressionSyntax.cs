namespace Minsk.CodeAnalysis.Syntax
{
    public sealed partial class ArrayCreationExpressionSyntax : ExpressionSyntax
    {
        public ArrayCreationExpressionSyntax(SyntaxTree syntaxTree, TypeClauseSyntax typeClause, SyntaxToken arrayKeywordToken, SyntaxToken openParenthesisToken, ExpressionSyntax sizeExpression, SyntaxToken closeParenthesisToken)
            : base(syntaxTree)
        {
            TypeClause = typeClause;
            ArrayKeywordToken = arrayKeywordToken;
            OpenParenthesisToken = openParenthesisToken;
            SizeExpression = sizeExpression;
            CloseParenthesisToken = closeParenthesisToken;
        }

        public override SyntaxKind Kind => SyntaxKind.ArrayCreationExpression;

        public TypeClauseSyntax TypeClause { get; }
        public SyntaxToken ArrayKeywordToken { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax SizeExpression { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }
}