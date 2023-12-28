using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Binding
{
    internal sealed class BoundArrayCreationExpression : BoundExpression
    {
        public BoundArrayCreationExpression(SyntaxNode syntax, TypeSymbol arrayElementType, BoundExpression sizeExpression)
            : base(syntax)
        {
            ArrayElementType = arrayElementType;
            SizeExpression = sizeExpression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ArrayCreationExpression;
        public override TypeSymbol Type => TypeSymbol.ArrayOf(ArrayElementType);

        public TypeSymbol ArrayElementType { get; }
        public BoundExpression SizeExpression { get; }
    }

    internal sealed class BoundArrayElementAccessExpression : BoundExpression
    {
        public BoundArrayElementAccessExpression(SyntaxNode syntax, VariableSymbol variableSymbol, BoundExpression elementIndexExpression)
            : base(syntax)
        {
            Variable = variableSymbol;
            ElementIndexExpression = elementIndexExpression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ArrayElementAccessExpression;

        public override TypeSymbol Type => Variable.Type.GetArrayElementType();

        public VariableSymbol Variable { get; }
        public BoundExpression ElementIndexExpression { get; }
    }
}
