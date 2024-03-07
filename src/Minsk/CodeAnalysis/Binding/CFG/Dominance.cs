using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Minsk.CodeAnalysis.Binding.CFG
{
    internal class Dominance
    {
        private Dictionary<int, ImmutableArray<int>> _dominators = new();

        public static Dominance Create(ControlFlowGraph cfg)
        {
            return new Dominance(cfg);
        }

        public ImmutableArray<int> Get(int blockNumber)
        {
            return _dominators[blockNumber];
        }

        private Dominance(ControlFlowGraph cfg)
        {          
            foreach (var block in cfg.Blocks.Where(b => !b.IsStart && !b.IsEnd))
            {
                var allWaysFromBlockToStart = new AllPathsFinder().findAllPaths(block, cfg.Blocks.First(b => b.IsStart));
                FillDominators(block.Number, allWaysFromBlockToStart);
            }
        }

        private void FillDominators(int blockNumber, List<Stack<int>> allWaysFromBlockToStart)
        {
            var allPaths = allWaysFromBlockToStart.Select(stack => stack.ToList()).ToList();
            var first = allPaths.First();
            var dominators = new List<int>();
            dominators.Add(blockNumber);
            dominators.AddRange(first);
            dominators.Reverse();

            foreach(var blockNum in first)
            {
                foreach(var path in allPaths)
                {
                    if(!path.Contains(blockNum))
                    {
                        dominators.Remove(blockNum);
                        break;
                    }
                }
            }

            _dominators[blockNumber] = dominators.ToImmutableArray();
        }

        private class AllPathsFinder
        {
            Stack<int> _connectionPath = new();
            List<Stack<int>> _allPaths = new();
            public List<Stack<int>> findAllPaths(ControlFlowGraph.BasicBlock from, ControlFlowGraph.BasicBlock to)
            {
                foreach (var nextNode in from.Incoming.Select(b => b.From))
                {
                    if (nextNode.Equals(to))
                    {
                        Stack<int> temp = new Stack<int>();
                        foreach (var node1 in _connectionPath)
                            temp.Push(node1);
                        _allPaths.Add(temp);
                    }
                    else if (!_connectionPath.Contains(nextNode.Number))
                    {
                        _connectionPath.Push(nextNode.Number);
                        findAllPaths(nextNode, to);
                        _connectionPath.Pop();
                    }
                }

                return _allPaths;
            }
        }
    }
}