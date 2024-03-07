using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Minsk.CodeAnalysis;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Syntax;
using Minsk.IO;
using Mono.Options;

namespace Minsk
{
    public enum OutputFormat
    {
        IL,
        Native,
        IR
    }

    internal static class Program
    {
        private static int Main(string[] args)
        {
            var outputPath = (string?)null;
            var moduleName = (string?)null;
            var referencePaths = new List<string>();
            var sourcePaths = new List<string>();
            var helpRequested = false;
            var outputFormat = OutputFormat.IL;
            var showProgram = false;
            var showSyntax = false;

            var options = new OptionSet
            {
                "usage: msc <source-paths> [options]",
                { "r=", "The {path} of an assembly to reference", v => referencePaths.Add(v) },
                { "o=", "The output {path} of the assembly to create", v => outputPath = v },
                { "m=", "The {name} of the module", v => moduleName = v },
                { "sp=", "Show program", v => showProgram = bool.Parse(v) },
                { "st=", "Show syntax", v => showSyntax = bool.Parse(v) },
                { "f=", "The format {format} of the assembly to create", f => outputFormat = (OutputFormat) Enum.Parse(typeof(OutputFormat), f)},
                { "?|h|help", "Prints help", v => helpRequested = true },
                { "<>", v => sourcePaths.Add(v) }
            };

            options.Parse(args);

            if (helpRequested)
            {
                options.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            if (sourcePaths.Count == 0)
            {
                Console.Error.WriteLine("error: need at least one source file");
                return 1;
            }

            if (outputPath == null)
                outputPath = Path.ChangeExtension(sourcePaths[0], ".exe");

            if (moduleName == null)
                moduleName = Path.GetFileNameWithoutExtension(outputPath);

            var syntaxTrees = new List<SyntaxTree>();
            var hasErrors = false;

            foreach (var path in sourcePaths)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"error: file '{path}' doesn't exist");
                    hasErrors = true;
                    continue;
                }

                var syntaxTree = SyntaxTree.Load(path);
                syntaxTrees.Add(syntaxTree);

                if (showSyntax)
                {
                    Console.Out.WriteLine(path);
                    syntaxTree.Root.WriteTo(Console.Out);
                }
            }

            foreach (var path in referencePaths)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"error: file '{path}' doesn't exist");
                    hasErrors = true;
                    continue;
                }
            }

            if (hasErrors)
                return 1;

            var compilation = Compilation.Create(syntaxTrees.ToArray());

            if (showProgram)
                compilation.EmitTree(Console.Out);

            ImmutableArray<Diagnostic> diagnostics = ImmutableArray<Diagnostic>.Empty;

            switch (outputFormat)
            {
                case OutputFormat.IL:
                    diagnostics = compilation.EmitIL(moduleName, referencePaths.ToArray(), outputPath);
                    break;
                case OutputFormat.Native:
                    diagnostics = compilation.EmitAssembly(moduleName, outputPath);
                    break;
                case OutputFormat.IR:
                    diagnostics = compilation.EmitIR(moduleName, outputPath);
                    break;
                default:
                    throw new Exception($"Unknown format {outputFormat}");
            }

            if (diagnostics.Any())
            {
                Console.Error.WriteDiagnostics(diagnostics);
                return 1;
            }

            compilation.DumpControlFlowGraphs("tmp");

            return 0;
        }
    }
}