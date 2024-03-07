using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Emit
{
    internal sealed class AsmEmitter : IEmiiter
    {
        private DiagnosticBag _diagnostics = new DiagnosticBag();

        private readonly Dictionary<VariableSymbol, StackPosition> _locals = new();
        private readonly Dictionary<string, string> _constStringToLabel = new();

        private readonly AssemblyGenerator gen = new();

        private int _labelNumber = 0;
        private int NextLabelNum() => _labelNumber++;

        public ImmutableArray<Diagnostic> Emit(BoundProgram program, string outputPath)
        {
            if (_diagnostics.Any())
                return _diagnostics.ToImmutableArray();

            gen.AddInstruction(".text");
            gen.AddInstruction(".globl	main");

            foreach (var functionWithBody in program.Functions)
                EmitFunction(functionWithBody.Key, functionWithBody.Value);

            gen.WriteTo(outputPath);

            return _diagnostics.ToImmutableArray();
        }

        private void EmitFunction(FunctionSymbol function, BoundBlockStatement body)
        {
            _constStringToLabel.Clear();

            var stringLiterals = GetAllStringConstants(body);
            foreach (var str in stringLiterals)
            {
                if (!_constStringToLabel.ContainsKey(str))
                {
                    var label = $".LC{NextLabelNum()}";
                    _constStringToLabel.Add(str, label);

                    gen.AddLabel($"{label}:");
                    gen.AddInstruction($".string \"{str}\"");
                }
            }

            gen.BeginFunction(function);

            _locals.Clear();

            var fucntionVariables = FunctionHelper.GetAllVariables(function.Parameters, body);
            var functionConstants = GetAllConstants(body);

            AllocateOnStack(fucntionVariables, functionConstants);
            CopyParamsIntoStack(function.Parameters);

            foreach (var statement in body.Statements)
                EmitStatement(statement);
        }

        private void AllocateOnStack(ImmutableArray<VariableSymbol> fucntionVariables, ImmutableArray<BoundConstant> functionConstants)
        {
            foreach (var variable in fucntionVariables)
            {
                var stackPosition = new StackPosition(Offset: (_locals.Count + 1) * 8);
                _locals.Add(variable, stackPosition);
            }

            //var bytesToAllocate = _locals.Count * 8 + (functionConstants.Length > 0 ? 1 * 8 : 0);
            var bytesToAllocate = Math.Max(_locals.Count * 8, 1 * 8);
            if(gen.CurrentFunction.Name == "main")
                bytesToAllocate += 8;

            gen.AddInstruction($"subq    ${bytesToAllocate}, %rsp   # Reserve {bytesToAllocate} bytes for local variables and constants");
        }

        private ImmutableArray<BoundConstant> GetAllConstants(BoundBlockStatement body)
        {
            var literals = ImmutableArray.CreateBuilder<BoundConstant>();

            body.Visit(node =>
            {
                if (node is BoundLiteralExpression litExp)
                    if (litExp.Type != TypeSymbol.String)
                        literals.Add(litExp.ConstantValue);
            });

            return literals.ToImmutableArray();

        }

        private ImmutableArray<string> GetAllStringConstants(BoundBlockStatement body)
        {
            var literals = ImmutableArray.CreateBuilder<string>();

            body.Visit(node =>
            {
                if (node is BoundLiteralExpression litExp)
                    if (litExp.Type == TypeSymbol.String)
                        literals.Add(litExp.ConstantValue.Value.ToString()!);
            });

            return literals.ToImmutableArray();
        }

        private void CopyParamsIntoStack(ImmutableArray<ParameterSymbol> parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                var stackPos = _locals[parameters[i]];
                switch (i)
                {
                    case 0:
                        gen.AddInstruction($"movq %rdi, -{stackPos.Offset}(%rbp)");
                        break;
                    case 1:
                        gen.AddInstruction($"movq %rsi, -{stackPos.Offset}(%rbp)");
                        break;
                    case 2:
                        gen.AddInstruction($"movq %rdx, -{stackPos.Offset}(%rbp)");
                        break;
                    case 3:
                        gen.AddInstruction($"movq %rcx, -{stackPos.Offset}(%rbp)");
                        break;
                    case 4:
                        gen.AddInstruction($"movq %r8, -{stackPos.Offset}(%rbp)");
                        break;
                    case 5:
                        gen.AddInstruction($"movq %r9, -{stackPos.Offset}(%rbp)");
                        break;
                }
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

        private void EmitExpression(BoundExpression node)
        {
            if (node.ConstantValue != null)
            {
                EmitConstantExpression(node);
                return;
            }

            switch (node.Kind)
            {
                case BoundNodeKind.VariableExpression:
                    EmitVariableExpression((BoundVariableExpression)node);
                    break;
                case BoundNodeKind.AssignmentExpression:
                    EmitAssignmentExpression((BoundAssignmentExpression)node);
                    break;
                case BoundNodeKind.UnaryExpression:
                    EmitUnaryExpression((BoundUnaryExpression)node);
                    break;
                case BoundNodeKind.BinaryExpression:
                    EmitBinaryExpression((BoundBinaryExpression)node);
                    break;
                case BoundNodeKind.CallExpression:
                    EmitCallExpression((BoundCallExpression)node);
                    break;
                case BoundNodeKind.ConversionExpression:
                    EmitConversionExpression((BoundConversionExpression)node);
                    break;
                case BoundNodeKind.EnumMemberAccessExpression:
                    EmitEnumMemberAccessExpression((BoundEnumMemberAccessExpression)node);
                    break;
                case BoundNodeKind.ArrayCreationExpression:
                    EmitArrayCreationExpression((BoundArrayCreationExpression)node);
                    break;
                case BoundNodeKind.ArrayElementAccessExpression:
                    EmitArrayElementAccessExpression((BoundArrayElementAccessExpression)node);
                    break;
                default:
                    throw new Exception($"Unexpected node kind {node.Kind}");
            }
        }

        private void EmitArrayCreationExpression(BoundArrayCreationExpression node)
        {
            EmitExpression(node.SizeExpression);

            gen.AddInstruction("popq %rdi");
            gen.AddInstruction("movq $8, %rax");
            gen.AddInstruction("imulq %rax, %rdi");
            gen.AddInstruction("call malloc");
            gen.AddInstruction("pushq %rax");
        }


        private void EmitEnumMemberAccessExpression(BoundEnumMemberAccessExpression node)
        {
            gen.AddInstruction($"pushq ${node.EnumMember.Ordinal}");
        }

        private void EmitConversionExpression(BoundConversionExpression node)
        {
            EmitExpression(node.Expression);

            //need to push converted value to node.Type on stack

            if (node.Type == TypeSymbol.Any)
            {
                // Done
            }
            else if (node.Type == TypeSymbol.Bool)
            {
                throw new NotImplementedException();
            }
            else if (node.Type == TypeSymbol.Int)
            {
                if (node.Expression.Type == TypeSymbol.String)
                {
                    gen.AddInstruction("popq %rdi");
                    gen.AddInstruction("call convert_string_to_int");
                    gen.AddInstruction("pushq %rax");
                }
                else
                    throw new NotImplementedException();
            }
            else if (node.Type == TypeSymbol.String)
            {
                if (node.Expression.Type == TypeSymbol.Bool)
                {
                    gen.AddInstruction("popq %rdi");
                    gen.AddInstruction("call convert_bool_to_string");
                    gen.AddInstruction("pushq %rax");
                }
                else if (node.Expression.Type == TypeSymbol.Int)
                {
                    gen.AddInstruction("popq %rdi");
                    gen.AddInstruction("call convert_int_to_string");
                    gen.AddInstruction("pushq %rax");
                }
                else if(node.Expression.Type.IsEnum())
                {
                    gen.AddInstruction("popq %rdi");
                    gen.AddInstruction("call convert_int_to_string");
                    gen.AddInstruction("pushq %rax");
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                throw new Exception($"Unexpected convertion from {node.Expression.Type} to {node.Type}");
            }
        }


        private void EmitCallExpression(BoundCallExpression node)
        {
            if (node.Arguments.Length > 6)
                throw new NotImplementedException("more then 6 params are not suppoted");

            foreach (var argument in node.Arguments)
                EmitExpression(argument);

            if (node.Function == BuiltinFunctions.Rnd)
            {
                gen.AddInstruction("popq %rdi");
                gen.AddInstruction("call minsk_rand");
                gen.AddInstruction("pushq %rax");
                return;
            }

            if (node.Function == BuiltinFunctions.Input)
            {
                gen.AddInstruction("call readInput");
                gen.AddInstruction("pushq %rax");

            }
            else if (node.Function == BuiltinFunctions.PrintInt)
            {
                gen.AddInstruction("popq %rdi");
                gen.AddInstruction("call printInt");

            }
            else if (node.Function == BuiltinFunctions.PrintString)
            {
                gen.AddInstruction("popq %rdi");
                gen.AddInstruction("call printText");

            }
            else
            {
                for (int i = node.Arguments.Length - 1; i >= 0; i--)
                {
                    switch (i)
                    {
                        case 0:
                            gen.AddInstruction("popq %rdi");
                            break;
                        case 1:
                            gen.AddInstruction("popq %rsi");
                            break;
                        case 2:
                            gen.AddInstruction("popq %rdx");
                            break;
                        case 3:
                            gen.AddInstruction("popq %rcx");
                            break;
                        case 4:
                            gen.AddInstruction("popq %r8");
                            break;
                        case 5:
                            gen.AddInstruction("popq %r9");
                            break;
                    }

                }

                gen.AddInstruction($"call {node.Function.Name}");
                if (node.Function.Type != TypeSymbol.Void)
                    gen.AddInstruction("pushq %rax");

            }
        }


        private void EmitBinaryExpression(BoundBinaryExpression node)
        {
            // +(string, string)

            if (node.Op.Kind == BoundBinaryOperatorKind.Addition)
            {
                if (node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
                {
                    throw new NotImplementedException();
                    return;
                }
            }

            EmitExpression(node.Left);
            EmitExpression(node.Right);

            // ==(any, any)
            // ==(string, string)

            if (node.Op.Kind == BoundBinaryOperatorKind.Equals)
            {
                if (node.Left.Type == TypeSymbol.Any && node.Right.Type == TypeSymbol.Any ||
                    node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
                {
                    throw new NotImplementedException();
                    return;
                }
            }

            // !=(any, any)
            // !=(string, string)

            if (node.Op.Kind == BoundBinaryOperatorKind.NotEquals)
            {
                if (node.Left.Type == TypeSymbol.Any && node.Right.Type == TypeSymbol.Any ||
                    node.Left.Type == TypeSymbol.String && node.Right.Type == TypeSymbol.String)
                {
                    throw new NotImplementedException();
                    return;
                }
            }

            gen.AddInstruction("popq %rdx");
            gen.AddInstruction("popq %rax");

            switch (node.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    gen.AddInstruction("addq %rdx, %rax");
                    break;
                case BoundBinaryOperatorKind.Subtraction:
                    gen.AddInstruction("subq %rdx, %rax");
                    break;
                case BoundBinaryOperatorKind.Multiplication:
                    gen.AddInstruction("imulq %rdx, %rax");
                    break;
                case BoundBinaryOperatorKind.Division:
                    gen.AddInstruction("movq %rdx, %rsi");
                    gen.AddInstruction("cqto");
                    gen.AddInstruction("idivq %rsi");
                    break;
                // TODO: Implement short-circuit evaluation #111
                /*case BoundBinaryOperatorKind.LogicalAnd:
                case BoundBinaryOperatorKind.BitwiseAnd:*/
                // TODO: Implement short-circuit evaluation #111
                /*case BoundBinaryOperatorKind.LogicalOr:
                case BoundBinaryOperatorKind.BitwiseOr:
                case BoundBinaryOperatorKind.BitwiseXor:*/
                case BoundBinaryOperatorKind.Equals:
                    gen.AddInstruction("cmpq %rdx, %rax");
                    gen.AddInstruction("sete %al");
                    gen.AddInstruction("movzbl %al, %eax");
                    break;
                case BoundBinaryOperatorKind.NotEquals:
                    gen.AddInstruction("cmpq %rdx, %rax");
                    gen.AddInstruction("setne %al");
                    gen.AddInstruction("movzbl %al, %eax");
                    break;
                case BoundBinaryOperatorKind.Less:
                    gen.AddInstruction("cmpq %rdx, %rax");
                    gen.AddInstruction("setl %al");
                    gen.AddInstruction("movzbl %al, %eax");
                    break;
                case BoundBinaryOperatorKind.LessOrEquals:
                    gen.AddInstruction("cmpq %rdx, %rax");
                    gen.AddInstruction("setle %al");
                    gen.AddInstruction("movzbl %al, %eax");
                    break;
                case BoundBinaryOperatorKind.Greater:
                    gen.AddInstruction("cmpq %rdx, %rax");
                    gen.AddInstruction("setg %al");
                    gen.AddInstruction("movzbl %al, %eax");
                    break;
                case BoundBinaryOperatorKind.GreaterOrEquals:
                    gen.AddInstruction("cmpq %rdx, %rax");
                    gen.AddInstruction("setge %al");
                    gen.AddInstruction("movzbl %al, %eax");
                    break;
                default:
                    throw new Exception($"Unexpected binary operator {SyntaxFacts.GetText(node.Op.SyntaxKind)}({node.Left.Type}, {node.Right.Type})");
            }

            gen.AddInstruction("pushq %rax");
        }


        private void EmitUnaryExpression(BoundUnaryExpression node)
        {
            EmitExpression(node.Operand);

            gen.AddInstruction("popq %rax");

            switch (node.Op.Kind)
            {
                case BoundUnaryOperatorKind.Identity:
                    gen.AddInstruction("inc %rax");
                    if(node.Operand is BoundVariableExpression ve)
                    {
                       gen.AddInstruction($"movq %rax, -{_locals[ve.Variable].Offset}(%rbp)"); 
                    }
                    break;
                case BoundUnaryOperatorKind.Negation:
                gen.AddInstruction("dec %rax");
                    if(node.Operand is BoundVariableExpression ve2)
                    {
                       gen.AddInstruction($"movq %rax, -{_locals[ve2.Variable].Offset}(%rbp)"); 
                    }
                    break;
                case BoundUnaryOperatorKind.LogicalNegation:
                case BoundUnaryOperatorKind.OnesComplement:
                throw new NotImplementedException();
            }

            gen.AddInstruction("pushq %rax");
        }

        private void EmitAssignmentExpression(BoundAssignmentExpression node)
        {
            if (node.ArrayElementIndexExpression != null)
            {
                EmitExpression(node.Expression);
                EmitExpression(node.ArrayElementIndexExpression);
                gen.AddInstruction($"movq -{_locals[node.Variable].Offset}(%rbp), %rdx  # place array being address into rdx");
                gen.AddInstruction($"popq %rcx # place array index into rcx");
                gen.AddInstruction($"popq %rax # place value into %rax");

                gen.AddInstruction($"movq %rax, (%rdx, %rcx, 8)");
            }
            else
            {
                EmitExpression(node.Expression);
                gen.AddInstruction($"popq %rax");
                gen.AddInstruction($"movq %rax, -{_locals[node.Variable].Offset}(%rbp)");
            }
        }

        private void EmitArrayElementAccessExpression(BoundArrayElementAccessExpression node)
        {
            EmitExpression(node.ElementIndexExpression);
            gen.AddInstruction($"movq -{_locals[node.Variable].Offset}(%rbp), %rdx  # place array being address into rdx");
            gen.AddInstruction($"popq %rcx # place array index into rcx");

            gen.AddInstruction($"pushq (%rdx, %rcx, 8) # push array value on stack");
        }


        private void EmitVariableExpression(BoundVariableExpression node)
        {
            if (node.Variable is ParameterSymbol parameter)
            {
                var stackPos = _locals[node.Variable];
                gen.AddInstruction($"pushq -{stackPos.Offset}(%rbp)");
            }
            else
            {
                var stackPos = _locals[node.Variable];
                gen.AddInstruction($"pushq -{stackPos.Offset}(%rbp)");
            }
        }


        private void EmitConstantExpression(BoundExpression node)
        {
            Debug.Assert(node.ConstantValue != null);

            if (node.Type == TypeSymbol.Bool)
            {
                if ((bool)node.ConstantValue.Value)
                    gen.AddInstruction($"pushq $1");
                else
                    gen.AddInstruction($"pushq $0");
            }
            else if (node.Type == TypeSymbol.Int)
            {
                var value = (int)node.ConstantValue.Value;
                gen.AddInstruction($"pushq ${value}");
            }
            else if (node.Type == TypeSymbol.String)
            {
                var value = (string)node.ConstantValue.Value;
                gen.AddInstruction($"leaq {_constStringToLabel[value]}(%rip), %rax");
                gen.AddInstruction("pushq %rax");
            }
            else
            {
                throw new Exception($"Unexpected constant expression type: {node.Type}");
            }
        }


        private void EmitReturnStatement(BoundReturnStatement node)
        {
            if (node.Expression != null)
            {
                EmitExpression(node.Expression);
                gen.AddInstruction("popq %rax   # ReturnStatement");
            }

            gen.AddInstruction("movq    %rbp, %rsp");

            Debug.Assert(gen.CurrentFunction != null);
            if (gen.CurrentFunction.Name != "main")
                gen.AddInstruction("popq %rbp");

            gen.AddInstruction("ret");
            gen.AddInstruction("");
        }


        private void EmitConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            EmitExpression(node.Condition);

            gen.AddInstruction("popq %rax");
            gen.AddInstruction("test %rax, %rax");

            Debug.Assert(gen.CurrentFunction != null);
            if (node.JumpIfTrue)
                gen.AddInstruction($"jne {gen.CurrentFunction.Name}_{node.Label.Name}");
            else
                gen.AddInstruction($"je {gen.CurrentFunction.Name}_{node.Label.Name}");
        }


        private void EmitGotoStatement(BoundGotoStatement node)
        {
            Debug.Assert(gen.CurrentFunction != null);
            gen.AddInstruction($"jmp {gen.CurrentFunction.Name}_{node.Label.Name}");
        }


        private void EmitLabelStatement(BoundLabelStatement node)
        {
            Debug.Assert(gen.CurrentFunction != null);
            gen.AddLabel($"{gen.CurrentFunction.Name}_{node.Label}:");
        }

        private void EmitVariableDeclaration(BoundVariableDeclaration node)
        {
            EmitExpression(node.Initializer);

            gen.AddInstruction("popq %rax");

            var stackPosition = _locals[node.Variable];
            gen.AddInstruction($"movq %rax, -{stackPosition.Offset}(%rbp)");
        }


        private void EmitNopStatement(BoundNopStatement node)
        {
            gen.AddInstruction("nop");
        }
    }

    internal record StackPosition(int Offset);
}