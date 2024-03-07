using System.Collections.Immutable;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Emit
{
    internal static class FunctionHelper
    {
        public static ImmutableArray<VariableSymbol> GetAllVariables(ImmutableArray<ParameterSymbol> parameters, BoundBlockStatement body)
        {
            var variables = ImmutableArray.CreateBuilder<VariableSymbol>();
            foreach (var parameter in parameters)
                variables.Add(parameter);


            body.Visit(node =>
            {
                if (node is BoundVariableDeclaration varDecl)
                    variables.Add(varDecl.Variable);
                if(node is BoundAssignmentExpression assExp)
                    if(!variables.Contains(assExp.Variable))
                        variables.Add(assExp.Variable);
            });

            return variables.ToImmutableArray();
        }
    }
}