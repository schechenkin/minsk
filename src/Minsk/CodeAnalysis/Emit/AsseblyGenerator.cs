using System;
using System.IO;
using System.Text;
using Minsk.CodeAnalysis.Symbols;

namespace Minsk;

public class AssemblyGenerator
{
    StringBuilder _sb = new StringBuilder();
    FunctionSymbol? _currentFunction;

    public FunctionSymbol? CurrentFunction => _currentFunction;

    public void AddInstruction(string text)
    {
        _sb.AppendLine($"    {text}");
    }

    public void AddLabel(string text)
    {
        _sb.AppendLine($"{text}");
    }


    public void WriteTo(string outputPath)
    {
        File.WriteAllText(outputPath, _sb.ToString());
    }

    public void BeginFunction(FunctionSymbol function)
    {
        _currentFunction = function;
        AddInstruction(".text");
        AddInstruction($".type	{function.Name}, @function");
        AddLabel($"{function.Name}:");

        if (function.Name != "main")
            AddInstruction("pushq	%rbp");

        AddInstruction("movq	%rsp, %rbp");
    }
}
