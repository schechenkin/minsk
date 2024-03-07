using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Emit;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk.CodeAnalysis.IR
{
    internal class IREmitter_old : IEmiiter
    {
        private DiagnosticBag _diagnostics = new DiagnosticBag();
        private int _currentReg = 0;

        private readonly Dictionary<VariableSymbol, StackPosition> _locals = new();

        public ImmutableArray<Diagnostic> Emit(BoundProgram program, string outputPath)
        {
            foreach (var functionWithBody in program.Functions)
                EmitFunction(functionWithBody.Key, functionWithBody.Value);

            return _diagnostics.ToImmutableArray();
        }

        private void EmitFunction(FunctionSymbol function, BoundBlockStatement body)
        {
            if (function.Name == "main")
            {
                _locals.Clear();

                var fucntionVariables = FunctionHelper.GetAllVariables(function.Parameters, body);
                foreach (var variable in fucntionVariables)
                {
                    var stackPosition = new StackPosition(Offset: (_locals.Count + 1) * 8);
                    _locals.Add(variable, stackPosition);
                }

                foreach (var statement in body.Statements)
                    EmitStatement(statement);
            }
        }

        private void EmitStatement(BoundStatement node)
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

        private void EmitExpressionStatement(BoundExpressionStatement node)
        {
            EmitExpression(node.Expression);
        }

        private int EmitExpression(BoundExpression node)
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
                default:
                    throw new Exception($"Unexpected node kind {node.Kind}");
            }
        }

        private int EmitBinaryExpression(BoundBinaryExpression node)
        {
            int t1 = EmitExpression(node.Left);
            int t2 = EmitExpression(node.Right);

            int res = GetNextRegister();

            switch (node.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    Emit(Operation.Add, t1, t2, res);
                    break;
                case BoundBinaryOperatorKind.Multiplication:
                    Emit(Operation.Mul, t1, t2, res);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return res;
        }


        private int EmitAssignmentExpression(BoundAssignmentExpression node)
        {
            throw new NotImplementedException();
        }


        private int EmitVariableExpression(BoundVariableExpression node)
        {
            var r0 = GetNextRegister();
            Emit(Operation.LoadI, _locals[node.Variable].Offset, null, r0);
            var r1 = GetNextRegister();
            Emit(Operation.LoadAO, 0, r0, r1);
            return r1;
        }


        private int EmitConstantExpression(BoundExpression node)
        {
            var value = (int)node.ConstantValue.Value;
            var t = GetNextRegister();
            Emit(Operation.LoadI, value, null, t);
            return t;
        }

        private void EmitReturnStatement(BoundReturnStatement node)
        {
            throw new NotImplementedException();
        }


        private void EmitConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            throw new NotImplementedException();
        }


        private void EmitGotoStatement(BoundGotoStatement node)
        {
            throw new NotImplementedException();
        }


        private void EmitLabelStatement(BoundLabelStatement node)
        {
            throw new NotImplementedException();
        }


        private void EmitVariableDeclaration(BoundVariableDeclaration node)
        {
            var r = EmitExpression(node.Initializer);
            Store(r, node.Variable);
        }

        private void EmitNopStatement(BoundNopStatement node)
        {

        }

        private void Emit(Operation op, int src1, int? src2, int dest)
        {
            switch(op)
            {
                case Operation.LoadI:
                    Console.WriteLine($"{op} {src1} R{dest}");
                    break;
                case Operation.LoadAO:
                    Debug.Assert(src2.HasValue);
                    Console.WriteLine($"{op} {src1} R{src2.Value} R{dest}");
                    break;
                case Operation.Add:
                case Operation.Mul:
                    Debug.Assert(src2.HasValue);
                    Console.WriteLine($"{op} R{src1} R{src2.Value} R{dest}");
                    break;
                default:
                    throw new NotImplementedException();
                
            }

        }

        private void Store(int reg, VariableSymbol var)
        {
            Console.WriteLine($"Store R{reg} -> {var.Name}");
        }

        private int GetNextRegister()
        {
            _currentReg++;
            return _currentReg;
        }
    }
}