using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Minsk.CodeAnalysis.Binding.CFG;
using Minsk.CodeAnalysis.Symbols;
using static Minsk.CodeAnalysis.Binding.CFG.ControlFlowGraph;

namespace Minsk.CodeAnalysis.Binding.SSA
{
    internal sealed class SSARewriter : BoundTreeRewriter
    {
        FunctionSymbol function;
        ControlFlowGraph cfg;
        Dominance dom;
        Dictionary<int, int?> idom = new();
        Dictionary<int, List<int>> dtree = new();
        Dictionary<int, List<int>> df = new();
        Dictionary<string, List<int>> varBlocks = new();
        Dictionary<int, BasicBlock> bbMap = new();
        Dictionary<string, int> counter = new();
        Dictionary<string, Stack<int>> stack = new();
        Dictionary<string, string> varToBaseVarMap = new();

        public static BoundBlockStatement Transform(FunctionSymbol function, BoundBlockStatement node)
        {
            var cfg = ControlFlowGraph.Create(node);
            var ssaRewriter = new SSARewriter();
            ssaRewriter.Transform(function, cfg);

            var statements = cfg.Blocks.SelectMany(b => b.Statements);
            return new BoundBlockStatement(node.Syntax, statements.ToImmutableArray());
        }

        private SSARewriter()
        {
        }

        protected override BoundStatement RewriteVariableDeclaration(BoundVariableDeclaration node)
        {
            return new BoundVariableDeclaration(node.Syntax, IsGlobal(node.Variable.Name) ? NewVarName(node.Variable) : node.Variable, RewriteExpression(node.Initializer));
        }

        protected override BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression node)
        {
            if (node.ArrayElementIndexExpression == null)
            {
                var expression = RewriteExpression(node.Expression);
                var variable = NewVarName(node.Variable);
                return new BoundAssignmentExpression(node.Syntax, variable, expression);
            }
            else
                return new BoundAssignmentExpression(node.Syntax, node.Variable, RewriteExpression(node.Expression), RewriteExpression(node.ArrayElementIndexExpression));
        }

        protected override BoundExpression RewriteVariableExpression(BoundVariableExpression node)
        {
            return new BoundVariableExpression(node.Syntax, CurrentVarName(node.Variable));
        }

        private void Transform(FunctionSymbol function, ControlFlowGraph cfg)
        {
            this.cfg = cfg;
            this.function = function;

            CalculateBasicBlocksMap();
            CalculateDominators();
            CalculateDiminatorsTree();
            CalculateDominanceFrontier();
            CalculateBlockForVariables();
            //cfg.Dump("start.dot", function.Name);
            InsertPhiFunctions();
            //cfg.Dump("after_phi.dot", function.Name);
            RenameVars();
            //cfg.Dump("after_RenameVars.dot", function.Name);
            SplitCriticalEdges();
            //cfg.Dump("after_SplitCriticalEdges.dot", function.Name);
            RemovePhiFunctions();
            //cfg.Dump("after_RemovePhiFunctions.dot", function.Name);
        }

        private void SplitCriticalEdges()
        {
        SeachCriticalEdgesAgain:
            foreach (var block in cfg.Blocks)
            {
                foreach (var phi in block.Phis)
                {
                    foreach (var arg in phi.Arguments)
                    {
                        if (arg.From.Outgoing.Count > 1)
                        {
                            var edge = arg.From.Outgoing.First(b => b.To == block);
                            SplitCriticalEdge(edge);
                            goto SeachCriticalEdgesAgain;
                        }
                    }
                }
            }
        }

        private void SplitCriticalEdge(BasicBlockBranch edge)
        {
            // sb - source block
            // bb - dest block
            var sb = edge.From;
            var db = edge.To;

            cfg.RemoveBranch(sb, db);

            //create new bb: add start label, add goto jump to db
            var bb = new BasicBlock();
            bb.Number = bbMap.Keys.Max() + 1;
            var bbLabel = new BoundLabel($"Label{bb.Number}");
            bb.Statements.Add(new BoundLabelStatement(null, bbLabel));
            BoundLabel? db_start_label = db.GetStartLabel();
            if (db_start_label is null)
                throw new Exception($"start label of block {db.Number} not found");
            bb.Statements.Add(new BoundGotoStatement(null, db_start_label));
            cfg.Blocks.Add(bb);

            cfg.AddBranch(sb, bb, edge.Condition);
            cfg.AddBranch(bb, db);

            //rewrite phi func arguments in db: change source block from sb to bb
            foreach (var phi in db.Phis)
            {
                foreach (var arg in phi.Arguments)
                {
                    if (arg.From == sb)
                    {
                        arg.From = bb;
                    }
                }
            }
            //change jump operation in sb: change label to start label of bb
            if (sb.EndsWithJump())
            {
                var jump = sb.Statements.Last();
                sb.Statements.Remove(jump);
                if (jump is BoundGotoStatement)
                {
                    sb.Statements.Add(new BoundGotoStatement(jump.Syntax, bbLabel));
                }
                else if (jump is BoundConditionalGotoStatement condGotoStm)
                {
                    sb.Statements.Add(new BoundConditionalGotoStatement(jump.Syntax, bbLabel, condGotoStm.Condition, condGotoStm.JumpIfTrue));
                }
            }
        }

        private void RemovePhiFunctions()
        {
            foreach (var block in cfg.Blocks)
            {
                foreach (var phi in block.Phis)
                {
                    var dest = phi.Destination;
                    foreach (var arg in phi.Arguments)
                    {
                        var blockToAddAssigments = arg.From;
                        var variable = arg.Variable;

                        bool jumpIsLast = blockToAddAssigments.EndsWithJump();
                        var assigmentStm = new BoundExpressionStatement(null,
                                                new BoundAssignmentExpression(null, dest,
                                                    new BoundVariableExpression(null, variable)));
                        if (!jumpIsLast)
                        {
                            blockToAddAssigments.Statements.Add(assigmentStm);
                        }
                        else
                        {
                            blockToAddAssigments.Statements.Insert(blockToAddAssigments.Statements.Count - 1, assigmentStm);
                        }
                    }
                }

                block.Phis.Clear();
            }
        }

        private void RenameVars()
        {           
            foreach (var kvp in varBlocks)
            {
                string varName = kvp.Key;
                counter.Add(varName, 0);
                stack.Add(varName, new Stack<int>());
                varToBaseVarMap.Add(varName, varName);
            }

            RewriteBlock(cfg.Start.Outgoing.Select(b => b.To).First());
        }

        private void RewriteBlock(BasicBlock block)
        {
            foreach (PhiNode phi in block.Phis)
            {
                phi.RenameDestinationWith(NewVarName(phi.Destination));
            }

            for (int i = 0; i < block.Statements.Count; i++)
            {
                block.Statements[i] = RewriteStatement(block.Statements[i]);
            }

            foreach (var succ in block.Outgoing.Select(b => b.To))
            {
                //fill in φ-function parameters 
                foreach (var phi in succ.Phis)
                {
                    var dest = phi.Destination;
                    phi.Arguments.Add(new PhiNode.Argument(CurrentVarName(dest), block));
                }
            }
            foreach (var succ in dtree[block.Number].Select(GetBlock))
            {
                RewriteBlock(succ);
            }

            foreach (var stm in block.Statements)
            {
                if (stm is BoundExpressionStatement expStm)
                {
                    if (expStm.Expression is BoundAssignmentExpression asExp)
                    {
                        var baseVarName = varToBaseVarMap[asExp.Variable.Name];
                        if (stack[baseVarName].Any())
                            stack[baseVarName].Pop();
                    }
                }
            }

            foreach (var phi in block.Phis)
            {
                var baseVarName = varToBaseVarMap[phi.Destination.Name];
                if (stack[baseVarName].Any())
                    stack[baseVarName].Pop();
            }
        }

        private VariableSymbol NewVarName(VariableSymbol variable)
        {
            var baseVarName = varToBaseVarMap[variable.Name];
            int i = counter[baseVarName];
            counter[baseVarName]++;
            stack[baseVarName].Push(i);
            var newName = $"{baseVarName}{i}";
            varToBaseVarMap.Add(newName, baseVarName);
            return new LocalVariableSymbol(newName, true, variable.Type, variable.Constant);
        }

        private VariableSymbol CurrentVarName(VariableSymbol variable)
        {
            var baseVarName = varToBaseVarMap[variable.Name];
            if (IsGlobal(baseVarName))
            {
                var currIndex = -1;
                if (stack[baseVarName].TryPeek(out currIndex))
                {
                    var currVarName = $"{baseVarName}{currIndex}";
                    return new LocalVariableSymbol(currVarName, true, variable.Type, variable.Constant);
                }
            }

            return variable;
        }


        private void InsertPhiFunctions()
        {
            Queue<int> work_list = new();
            Dictionary<int, string> inserted = new();

            foreach (var kvp in varBlocks)
            {
                var varName = kvp.Key;

                var blocks = kvp.Value;

                if (blocks.Count < 2)
                    continue;

                foreach (var block in blocks)
                {
                    work_list.Enqueue(block);
                }

                while (work_list.Any())
                {
                    var block = GetBlock(work_list.Dequeue());

                    foreach (var df_block_num in df[block.Number])
                    {
                        BasicBlock df_block = GetBlock(df_block_num);
                        if (!inserted.ContainsKey(df_block_num) || inserted[df_block_num] != varName)
                        {
                            inserted[df_block_num] = varName;

                            VariableSymbol variableSymbol = GetVariableSymbol(varName);
                            var phiNode = new PhiNode(variableSymbol);

                            df_block.Phis.Add(phiNode);

                            work_list.Enqueue(df_block_num);
                        }
                    }
                }
            }
        }

        private VariableSymbol GetVariableSymbol(string varName)
        {
            foreach (var p in function.Parameters)
            {
                if (p.Name == varName)
                    return p;
            }

            foreach (var block in cfg.Blocks)
            {
                foreach (var stm in block.Statements)
                {
                    if (stm is BoundVariableDeclaration varDecl && varDecl.Variable.Name == varName)
                    {
                        return varDecl.Variable;
                    }

                    if (stm is BoundExpressionStatement exp && exp.Expression is BoundAssignmentExpression assExp && assExp.Variable.Name == varName)
                    {
                        return assExp.Variable;
                    }
                }
            }

            throw new Exception("VariableDeclaration not found");
        }

        private BasicBlock GetBlock(int blockNumber)
        {
            return bbMap[blockNumber];
        }

        private void CalculateBlockForVariables()
        {
            foreach(var p in function.Parameters)
            {
                varBlocks.Add(p.Name, new List<int>());
            }
            
            foreach (var block in cfg.Blocks.Where(b => !b.IsStart && !b.IsEnd))
            {
                List<string> variables = GetBlockVariablesAssigments(block);
                foreach (string varName in variables)
                {
                    if (varBlocks.ContainsKey(varName) == false)
                        varBlocks.Add(varName, new List<int>());

                    varBlocks[varName].Add(block.Number);
                }
            }
        }

        private List<string> GetBlockVariablesAssigments(BasicBlock block)
        {
            HashSet<string> variables = new();
            foreach (var stm in block.Statements)
            {
                stm.Visit((node) =>
                {
                    if (node is BoundVariableDeclaration varDecl)
                        if (!variables.Contains(varDecl.Variable.Name))
                            variables.Add(varDecl.Variable.Name);

                    if (node is BoundAssignmentExpression boundExp && boundExp.ArrayElementIndexExpression is null)
                        if (!variables.Contains(boundExp.Variable.Name))
                            variables.Add(boundExp.Variable.Name);

                });
            }

            return variables.ToList();
        }


        private bool IsGlobal(string variableName)
        {
            return true;
            return varBlocks[variableName].Count > 1;
        }


        private void CalculateDominanceFrontier()
        {
            foreach (var kvp in bbMap)
                df.Add(kvp.Key, []);

            foreach (var block in cfg.Blocks)
            {
                if (block.Incoming.Count < 2 || block.IsEnd)
                    continue;

                foreach (var pre in block.Incoming.Select(b => b.From))
                {
                    var runner = pre;
                    while (runner != null && runner.Number != idom[block.Number])
                    {
                        df[runner.Number].Add(block.Number);

                        if (idom[runner.Number].HasValue)
                            runner = GetBlock(idom[runner.Number].Value);
                        else
                            runner = null;
                    }
                }
            }
        }

        private void CalculateDiminatorsTree()
        {
            foreach (var kvp in bbMap)
            {
                dtree.Add(kvp.Key, []);
            }

            foreach (var kvp in bbMap)
            {
                var blockNumber = kvp.Key;
                var dom = this.dom.Get(blockNumber);
                var indexOfBlockNumber = dom.IndexOf(blockNumber);
                if (indexOfBlockNumber > 0)
                {
                    var parentId = dom[indexOfBlockNumber - 1];
                    dtree[parentId].Add(blockNumber);
                }
            }

            idom.Add(0, null);
            foreach (var kvp in dtree)
            {
                int parent = kvp.Key;
                var children = kvp.Value;
                foreach (var child in children)
                {
                    idom.Add(child, parent);
                }
            }
        }


        private void CalculateDominators()
        {
            dom = Dominance.Create(cfg);
        }

        private void CalculateBasicBlocksMap()
        {
            new BasicBlockDFSVisitor().Visit(cfg.Start, (block) =>
                {
                    if (block != cfg.Start && block != cfg.End)
                    {
                        bbMap.Add(block.Number, block);
                    }
                });
        }
    }
}