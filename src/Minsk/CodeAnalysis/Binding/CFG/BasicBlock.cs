using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Minsk.CodeAnalysis.Binding.SSA;

namespace Minsk.CodeAnalysis.Binding.CFG
{
    internal sealed partial class ControlFlowGraph
    {
        public sealed class BasicBlock
        {
            public BasicBlock()
            {
            }

            public BasicBlock(bool isStart)
            {
                IsStart = isStart;
                IsEnd = !isStart;
            }

            public bool IsStart { get; }
            public bool IsEnd { get; }
            public int Number { get; internal set; } = -1;
            public List<PhiNode> Phis { get; } = new();
            public List<BoundStatement> Statements { get; } = new List<BoundStatement>();
            public List<IR.Instruction> Instructions { get; set;} = new List<IR.Instruction>();
            public List<BasicBlockBranch> Incoming { get; } = new List<BasicBlockBranch>();
            public List<BasicBlockBranch> Outgoing { get; } = new List<BasicBlockBranch>();

            public override string ToString()
            {
                if (IsStart)
                    return "<Start>";

                if (IsEnd)
                    return "<End>";

                using (var writer = new StringWriter())
                using (var indentedWriter = new IndentedTextWriter(writer))
                {
                    foreach (var phi in Phis)
                        phi.WriteTo(indentedWriter);
                    
                    foreach (var statement in Statements)
                        statement.WriteTo(indentedWriter);

                    if(Instructions.Any())
                    {
                        indentedWriter.WriteLine();
                        foreach(var i in Instructions)
                        {
                            indentedWriter.Write(i.ToString());
                            indentedWriter.WriteLine();
                        }
                    }

                    return writer.ToString();
                }
            }

            internal bool EndsWithJump()
            {
                if(!Statements.Any())
                    return false;
                
                var lastStm = Statements.Last();

                return lastStm is BoundGotoStatement || lastStm is BoundConditionalGotoStatement;

            }

            internal BoundLabel? GetStartLabel()
            {
                if(!Statements.Any())
                    return null;
                
                var firstStm = Statements.First();
                if(firstStm is BoundLabelStatement boundLabelStatement)
                {
                    return boundLabelStatement.Label;
                }

                return null;
            }
        }
    }
}
