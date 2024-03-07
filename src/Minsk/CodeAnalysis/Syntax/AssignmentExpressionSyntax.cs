namespace Minsk.CodeAnalysis.Syntax
{
    public sealed partial class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken assignmentToken, ExpressionSyntax right)
            : base(syntaxTree)
        {
            Left = left;
            AssignmentToken = assignmentToken;
            Right = right;
        }

        public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
        public ExpressionSyntax Left { get; }
        public SyntaxToken AssignmentToken { get; }
        public ExpressionSyntax Right { get; }
    }

    public sealed partial class ArrayElementAccessExpressionSyntax : ExpressionSyntax
    {
        public ArrayElementAccessExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken, SyntaxToken openBracketToken, ExpressionSyntax elementIndexExpression, SyntaxToken closeBracketToken)
            : base(syntaxTree)
        {
            IdentifierToken = identifierToken;
            OpenBracketToken = openBracketToken;
            ElementIndexExpression = elementIndexExpression;
            CloseBracketToken = closeBracketToken;
        }

        public override SyntaxKind Kind => SyntaxKind.ArrayElementAccessExpression;
        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken OpenBracketToken { get; }
        public ExpressionSyntax ElementIndexExpression { get; }
        public SyntaxToken CloseBracketToken { get; }
    }
}