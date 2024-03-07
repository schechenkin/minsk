using System;
using System.Collections.Generic;
using Minsk.CodeAnalysis.Binding.CFG;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.Binding.SSA
{
    internal sealed class PhiNode
    {
        public PhiNode(VariableSymbol destination)
        {
            Destination = destination;
        }

        public List<Argument> Arguments { get; } = new();
        public VariableSymbol Destination { get; private set; }

        internal void RenameDestinationWith(VariableSymbol variable)
        {
            Destination = variable;
        }

        internal class Argument
        {
            public Argument(VariableSymbol variable , ControlFlowGraph.BasicBlock from)
            {
                Variable = variable;
                From = from;
            }

            public VariableSymbol Variable { get; private set; }
            public ControlFlowGraph.BasicBlock From  { get; internal set; }
        }
    }
}
