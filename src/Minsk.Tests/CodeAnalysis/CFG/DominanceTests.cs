using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Binding.CFG;
using Xunit;

namespace Minsk.Tests.CodeAnalysis.CFG
{
    public class DominanceTests
    {
        [Fact]
        public void Dominance_Build_DOM_Info()
        {
            var builder = new CFGBuilder(9);

            builder.Connect(0, 1);
            builder.Connect(1, 2, true);
            builder.Connect(1, 5, false);
            builder.Connect(2, 3);
            builder.Connect(5, 6, true);
            builder.Connect(5, 8, false);
            builder.Connect(6, 7);
            builder.Connect(8, 7);
            builder.Connect(7, 3);
            builder.Connect(3, 1, true);
            builder.Connect(3, 4, false);
            builder.ReturnFrom(4);

            ControlFlowGraph cfg = builder.BuildCFG();

            var cfgPath = $"cfg_Dominance_Build_DOM_Info.dot";
            using (var streamWriter = new StreamWriter(cfgPath))
                    cfg.WriteTo(streamWriter);

            var dominators = Dominance.Create(cfg);

            Assert.True(ImmutableArray.Create(0).SequenceEqual(dominators.Get(0)));
            Assert.True(ImmutableArray.Create(0, 1).SequenceEqual(dominators.Get(1)));
            Assert.True(ImmutableArray.Create(0, 1, 2).SequenceEqual(dominators.Get(2)));
            Assert.True(ImmutableArray.Create(0, 1, 2, 3).SequenceEqual(dominators.Get(3)));
            Assert.True(ImmutableArray.Create(0, 1, 2, 4).SequenceEqual(dominators.Get(4)));
            Assert.True(ImmutableArray.Create(0, 1, 5).SequenceEqual(dominators.Get(5)));
            Assert.True(ImmutableArray.Create(0, 1, 5, 6).SequenceEqual(dominators.Get(6)));
            Assert.True(ImmutableArray.Create(0, 1, 2, 7).SequenceEqual(dominators.Get(7)));
            Assert.True(ImmutableArray.Create(0, 1, 8).SequenceEqual(dominators.Get(8)));
        }
    }

    public class CFGBuilder
    {
        private List<ControlFlowGraph.BasicBlock> _blocks = new();

        public CFGBuilder(int blocksCount)
        {
            for(int i = 0; i < blocksCount; i++)
            {
                var block = new ControlFlowGraph.BasicBlock();

                block.Statements.Add(new BoundLabelStatement(null, Label(i)));
                block.Statements.Add(new BoundNopStatement(null));

                _blocks.Add(block);
            }
        }

        internal ControlFlowGraph BuildCFG()
        {
            var graphBuilder = new ControlFlowGraph.GraphBuilder();
            return graphBuilder.Build(_blocks);
        }

        internal void Connect(int sourceNode, int destNode)
        {
            _blocks[sourceNode].Statements.Add(new BoundGotoStatement(null, Label(destNode)));
        }

        internal void Connect(int sourceNode, int destNode, bool onTrue)
        {
            if(onTrue)
                _blocks[sourceNode].Statements.Add(new BoundConditionalGotoStatement(null, Label(destNode), new BoundLiteralExpression(null, true)));
            else
                _blocks[sourceNode].Statements.Add(new BoundGotoStatement(null, Label(destNode)));
        }


        internal void ReturnFrom(int blockNum)
        {
            _blocks[blockNum].Statements.Add(new BoundReturnStatement(null, null));
        }

        Dictionary<int, BoundLabel> _labels = new();

        private BoundLabel Label(int blockNum)
        {
            if(!_labels.ContainsKey(blockNum))
                _labels.Add(blockNum, new BoundLabel($"Label{blockNum}"));
            return _labels[blockNum];
        }
    }
}