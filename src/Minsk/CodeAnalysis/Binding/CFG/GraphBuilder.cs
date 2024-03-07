using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Minsk.CodeAnalysis.Binding.CFG.ControlFlowGraph;

namespace Minsk.CodeAnalysis.Binding.CFG
{
    internal sealed partial class ControlFlowGraph
    {
        public sealed class GraphBuilder
        {
            private Dictionary<BoundStatement, BasicBlock> _blockFromStatement = new Dictionary<BoundStatement, BasicBlock>();
            private Dictionary<BoundLabel, BasicBlock> _blockFromLabel = new Dictionary<BoundLabel, BasicBlock>();
            private List<BasicBlockBranch> _branches = new List<BasicBlockBranch>();
            private BasicBlock _start = new BasicBlock(isStart: true);
            private BasicBlock _end = new BasicBlock(isStart: false);

            public ControlFlowGraph Build(List<BasicBlock> blocks)
            {
                if (!blocks.Any())
                    Connect(_start, _end);
                else
                    Connect(_start, blocks.First());

                foreach (var block in blocks)
                {
                    foreach (var statement in block.Statements)
                    {
                        _blockFromStatement.Add(statement, block);
                        if (statement is BoundLabelStatement labelStatement)
                            _blockFromLabel.Add(labelStatement.Label, block);
                    }
                }

                for (int i = 0; i < blocks.Count; i++)
                {
                    var current = blocks[i];
                    var next = i == blocks.Count - 1 ? _end : blocks[i + 1];

                    foreach (var statement in current.Statements)
                    {
                        var isLastStatementInBlock = statement == current.Statements.Last();
                        switch (statement.Kind)
                        {
                            case BoundNodeKind.GotoStatement:
                                var gs = (BoundGotoStatement)statement;
                                var toBlock = _blockFromLabel[gs.Label];
                                Connect(current, toBlock);
                                break;
                            case BoundNodeKind.ConditionalGotoStatement:
                                var cgs = (BoundConditionalGotoStatement)statement;
                                var thenBlock = _blockFromLabel[cgs.Label];
                                var elseBlock = next;
                                var negatedCondition = Negate(cgs.Condition);
                                var thenCondition = cgs.JumpIfTrue ? cgs.Condition : negatedCondition;
                                var elseCondition = cgs.JumpIfTrue ? negatedCondition : cgs.Condition;
                                Connect(current, thenBlock, thenCondition);
                                Connect(current, elseBlock, elseCondition);
                                break;
                            case BoundNodeKind.ReturnStatement:
                                Connect(current, _end);
                                break;
                            case BoundNodeKind.NopStatement:
                            case BoundNodeKind.VariableDeclaration:
                            case BoundNodeKind.LabelStatement:
                            case BoundNodeKind.ExpressionStatement:
                                if (isLastStatementInBlock)
                                    Connect(current, next);
                                break;
                            default:
                                throw new Exception($"Unexpected statement: {statement.Kind}");
                        }
                    }
                }

            ScanAgain:
                foreach (var block in blocks)
                {
                    if (!block.Incoming.Any())
                    {
                        RemoveBlock(blocks, block);
                        goto ScanAgain;
                    }
                }


                blocks.Insert(0, _start);
                blocks.Add(_end);

                AssignBlockNumbers();

                return new ControlFlowGraph(_start, _end, blocks, _branches);
            }

            private void AssignBlockNumbers()
            {
                int blockNumber = 0;
                new BasicBlockDFSVisitor().Visit(_start, (block) =>
                {
                    if (block != _start && block != _end)
                    {
                        block.Number = blockNumber;
                        blockNumber++;
                    }
                });
            }


            private void Connect(BasicBlock from, BasicBlock to, BoundExpression? condition = null)
            {
                if (condition is BoundLiteralExpression l)
                {
                    var value = (bool)l.Value;
                    if (value)
                        condition = null;
                    else
                        return;
                }

                var branch = new BasicBlockBranch(from, to, condition);
                from.Outgoing.Add(branch);
                to.Incoming.Add(branch);
                _branches.Add(branch);
            }

            private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
            {
                foreach (var branch in block.Incoming)
                {
                    branch.From.Outgoing.Remove(branch);
                    _branches.Remove(branch);
                }

                foreach (var branch in block.Outgoing)
                {
                    branch.To.Incoming.Remove(branch);
                    _branches.Remove(branch);
                }

                blocks.Remove(block);
            }

            private BoundExpression Negate(BoundExpression condition)
            {
                var negated = BoundNodeFactory.Not(condition.Syntax, condition);
                if (negated.ConstantValue != null)
                    return new BoundLiteralExpression(condition.Syntax, negated.ConstantValue.Value);

                return negated;
            }
        }
    }

    internal class BasicBlockDFSVisitor
    {
        List<BasicBlock> _visited = new();
        Action<BasicBlock> _onEnter;
        
        public void Visit(BasicBlock block, Action<BasicBlock> onEnter)
        {
            _onEnter = onEnter;
            Visit(block);
        }

        private void Visit(BasicBlock block)
        {
            _onEnter(block);
            _visited.Add(block);

            foreach(var outBlock in block.Outgoing.Select(b => b.To).Reverse())
            {
                if(!_visited.Contains(outBlock))
                    Visit(outBlock);
            }
        }
    }
}
