using System;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression(SyntaxNode syntax, VariableSymbol variable, BoundExpression expression)
            : base(syntax)
        {
            Variable = variable;
            Expression = expression;
        }

        public BoundAssignmentExpression(SyntaxNode syntax, VariableSymbol variable, BoundExpression expression, BoundExpression arrayElementIndexExpression)
            : this(syntax, variable, expression)
        {
            ArrayElementIndexExpression = arrayElementIndexExpression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
        public override TypeSymbol Type => Expression.Type;
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
        public BoundExpression? ArrayElementIndexExpression { get; }
    }
}
