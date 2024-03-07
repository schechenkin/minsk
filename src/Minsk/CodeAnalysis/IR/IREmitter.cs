using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Binding.CFG;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.IR
{
    internal sealed class IREmitter
    {
        VirtualRegistersDescriptor RegistersDescriptor { get; } = new();
        AddressDescriptor AddressDescriptor { get; } = new();

        List<Instruction> _instrutions = new();

        VirtualRegister _virtualRegister;
        private readonly ControlFlowGraph.BasicBlock _basicBlock;


        public IREmitter(VirtualRegister current, ControlFlowGraph.BasicBlock basicBlock)
        {
            _virtualRegister = current;
            _basicBlock = basicBlock;

        }

        public ImmutableArray<Instruction> Emit(out VirtualRegister currentVirtualRegister)
        {
            foreach (BoundStatement node in _basicBlock.Statements)
            {
                switch (node.Kind)
                {
                    case BoundNodeKind.NopStatement:
                        EmitNopStatement((BoundNopStatement)node);
                        break;
                    case BoundNodeKind.VariableDeclaration:
                        EmitVariableDeclaration((BoundVariableDeclaration)node);
                        break;
                    case BoundNodeKind.LabelStatement:
                        EmitLabelStatement((BoundLabelStatement)node);
                        break;
                    case BoundNodeKind.GotoStatement:
                        EmitGotoStatement((BoundGotoStatement)node);
                        break;
                    case BoundNodeKind.ConditionalGotoStatement:
                        EmitConditionalGotoStatement((BoundConditionalGotoStatement)node);
                        break;
                    case BoundNodeKind.ReturnStatement:
                        EmitReturnStatement((BoundReturnStatement)node);
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        EmitExpressionStatement((BoundExpressionStatement)node);
                        break;
                    default:
                        throw new Exception($"Unexpected node kind {node.Kind}");
                }
            }

            currentVirtualRegister = _virtualRegister;

            return _instrutions.ToImmutableArray();
        }

        private void EmitNopStatement(BoundNopStatement node)
        {
            
        }

        private VirtualRegister EmitVariableDeclaration(BoundVariableDeclaration node)
        {
            var r = EmitExpression(node.Initializer);
            Emit(Store.Create(new Address(node.Variable.Name), r));
            return r;
        }


        private void EmitLabelStatement(BoundLabelStatement node)
        {
            Emit(LabelInst.Create(new Label(node.Label.Name)));
        }


        private void EmitGotoStatement(BoundGotoStatement node)
        {
            Emit(Jump.Create(new Label(node.Label.Name)));
        }

        private void EmitConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            var reg = EmitExpression(node.Condition);
            Emit(ConditionalBranch.Create(reg, new Label(node.Label.Name), node.JumpIfTrue));
        }


        private void EmitReturnStatement(BoundReturnStatement node)
        {
            
        }


        private VirtualRegister EmitExpressionStatement(BoundExpressionStatement node)
        {
            return EmitExpression(node.Expression);
        }

        private VirtualRegister EmitExpression(BoundExpression node)
        {
            if (node.ConstantValue != null)
            {
                return EmitConstantExpression(node);
            }

            switch (node.Kind)
            {
                case BoundNodeKind.VariableExpression:
                    return EmitVariableExpression((BoundVariableExpression)node);
                case BoundNodeKind.AssignmentExpression:
                    return EmitAssignmentExpression((BoundAssignmentExpression)node);
                case BoundNodeKind.BinaryExpression:
                    return EmitBinaryExpression((BoundBinaryExpression)node);
                case BoundNodeKind.CallExpression:
                    return EmitCallExpression((BoundCallExpression)node);
                default:
                    throw new Exception($"Unexpected node kind {node.Kind}");
            }
        }

        private VirtualRegister EmitCallExpression(BoundCallExpression node)
        {
            List<Register> args = new();
            foreach (var argument in node.Arguments)
                args.Add(EmitExpression(argument));

            var reg = GetNextRegister();
            Emit(Call.Create(node.Function, reg, args));

            return reg;
        }


        private VirtualRegister EmitConstantExpression(BoundExpression node)
        {
            Debug.Assert(node.ConstantValue != null);

            var reg = GetNextRegister();

            if (node.Type == TypeSymbol.Bool)
            {
                if ((bool)node.ConstantValue.Value)
                    Emit(Load.Create(reg, 1));
                else
                    Emit(Load.Create(reg, 0));
            }
            else if (node.Type == TypeSymbol.Int)
            {
                var value = (int)node.ConstantValue.Value;
                Emit(Load.Create(reg, value));
            }
            else if (node.Type == TypeSymbol.String)
            {
                var value = (string)node.ConstantValue.Value;
                /*

                TODO create address of contant string
                gen.AddInstruction($"leaq {_constStringToLabel[value]}(%rip), %rax");
                gen.AddInstruction("pushq %rax");
                
                */
                Emit(Load.Create(reg, new Address(value)));
            }
            else
            {
                throw new Exception($"Unexpected constant expression type: {node.Type}");
            }
            
            return reg;
        }


        private VirtualRegister EmitBinaryExpression(BoundBinaryExpression node)
        {
            var t1 = EmitExpression(node.Left);
            var t2 = EmitExpression(node.Right);

            var res = GetNextRegister();

            switch (node.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    Emit(Add.Create(res, t1, t2));
                    break;
                case BoundBinaryOperatorKind.Multiplication:
                    Emit(Mul.Create(res, t1, t2));
                    break;
                case BoundBinaryOperatorKind.Greater:
                    Emit(Compare.Create(res, t1, t2, Operation.Cmp_GE));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return res;
        }


        private VirtualRegister EmitAssignmentExpression(BoundAssignmentExpression node)
        {
            throw new NotImplementedException();
        }


        private VirtualRegister EmitVariableExpression(BoundVariableExpression node)
        {
            var r0 = GetNextRegister();
            Emit(Load.Create(r0, new Address(node.Variable.Name)));
            return r0;
            /*Emit(Operation.LoadI, _locals[node.Variable].Offset, null, r0);
            var r1 = GetNextRegister();
            Emit(Operation.LoadAO, 0, r0, r1);
            return r1;*/
        }

        private VirtualRegister GetNextRegister()
        {
            _virtualRegister = VirtualRegister.Num(_virtualRegister.Number + 1);
            return _virtualRegister;
        }

        private void Emit(Instruction instruction)
        {
            _instrutions.Add(instruction);
        }
    }
}