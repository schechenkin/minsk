using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Minsk.CodeAnalysis.Binding;
using Minsk.CodeAnalysis.Binding.CFG;
using Minsk.CodeAnalysis.Binding.SSA;
using Minsk.CodeAnalysis.Emit;
using Minsk.CodeAnalysis.IR;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope? _globalScope;

        private Compilation(bool isScript, Compilation? previous, params SyntaxTree[] syntaxTrees)
        {
            IsScript = isScript;
            Previous = previous;
            SyntaxTrees = syntaxTrees.ToImmutableArray();
        }

        public static Compilation Create(params SyntaxTree[] syntaxTrees)
        {
            return new Compilation(isScript: false, previous: null, syntaxTrees);
        }

        public static Compilation CreateScript(Compilation? previous, params SyntaxTree[] syntaxTrees)
        {
            return new Compilation(isScript: true, previous, syntaxTrees);
        }

        public bool IsScript { get; }
        public Compilation? Previous { get; }
        public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
        public FunctionSymbol? MainFunction => GlobalScope.MainFunction;
        public ImmutableArray<FunctionSymbol> Functions => GlobalScope.Functions;
        public ImmutableArray<VariableSymbol> Variables => GlobalScope.Variables;
        public ImmutableArray<EnumSymbol> Enums => GlobalScope.Enums;

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(IsScript, Previous?.GlobalScope, SyntaxTrees);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }

        public IEnumerable<Symbol> GetSymbols()
        {
            var submission = this;
            var seenSymbolNames = new HashSet<string>();

            var builtinFunctions = BuiltinFunctions.GetAll().ToList();

            while (submission != null)
            {
                foreach (var function in submission.Functions)
                    if (seenSymbolNames.Add(function.Name))
                        yield return function;

                foreach (var enumType in submission.Enums)
                    if (seenSymbolNames.Add(enumType.Name))
                        yield return enumType;

                foreach (var variable in submission.Variables)
                    if (seenSymbolNames.Add(variable.Name))
                        yield return variable;

                foreach (var builtin in builtinFunctions)
                    if (seenSymbolNames.Add(builtin.Name))
                        yield return builtin;

                submission = submission.Previous;
            }
        }

        private BoundProgram GetProgram()
        {
            var previous = Previous == null ? null : Previous.GetProgram();
            return Binder.BindProgram(IsScript, previous, GlobalScope);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            if (GlobalScope.Diagnostics.Any())
                return new EvaluationResult(GlobalScope.Diagnostics, null);

            var program = GetProgram();

            if (program.Diagnostics.HasErrors())
                return new EvaluationResult(program.Diagnostics, null);

            var evaluator = new Evaluator(program, variables);
            var value = evaluator.Evaluate();

            return new EvaluationResult(program.Diagnostics, value);
        }

        public void EmitTree(TextWriter writer)
        {
            EmitEnumDeclarations(GlobalScope.Enums, writer);
            
            if (GlobalScope.MainFunction != null)
            {
                foreach(var f in GlobalScope.Functions)
                {
                    EmitTree(f, writer);
                    writer.WriteLine();
                }
            }
            else if (GlobalScope.ScriptFunction != null)
                EmitTree(GlobalScope.ScriptFunction, writer);
        }

        public void EmitTree(FunctionSymbol symbol, TextWriter writer)
        {
            var program = GetProgram();
            symbol.WriteTo(writer);
            writer.WriteLine();
            if (!program.Functions.TryGetValue(symbol, out var body))
                return;
            body.WriteTo(writer);

            /*writer.WriteLine("Tree view");
            var printer = new BoundNodeTreeViewPrinter(writer);
            printer.Print(body);*/
        }

        public void EmitEnumDeclarations(ImmutableArray<EnumSymbol> enumSymbols, TextWriter writer)
        {
            foreach(var enumSymbol in enumSymbols)
            {
                enumSymbol.WriteTo(writer);
                writer.WriteLine();
            }
            writer.WriteLine();
        }

        // TODO: References should be part of the compilation, not arguments for Emit
        public ImmutableArray<Diagnostic> EmitIL(string moduleName, string[] references, string outputPath)
        {
            var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);

            var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.HasErrors())
                return diagnostics;

            var program = GetProgram();

            if (program.Diagnostics.HasErrors())
                return program.Diagnostics;

            return ILEmitter.Emit(program, moduleName, references, outputPath);
        }

        public ImmutableArray<Diagnostic> EmitAssembly(string moduleName, string outputPath)
        {
            var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);

            var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.HasErrors())
                return diagnostics;

            var program = GetProgram();

            if (program.Diagnostics.HasErrors())
                return program.Diagnostics;

            RecreateFolder("tmp");

            var asmEmitter = new AsmEmitter();
            
            var asmFile = Path.Combine("tmp", $"{moduleName}.s");
            diagnostics = asmEmitter.Emit(program, asmFile);
            
            if (diagnostics.HasErrors())
                return diagnostics;

            CreateExecutable(asmFile, outputPath);

            return diagnostics;
        }

        public ImmutableArray<Diagnostic> EmitIR(string moduleName, string outputPath)
        {
            var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);

            var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.HasErrors())
                return diagnostics;

            var program = GetProgram();

            //TODO
            //lets take main func
            //build cfg for main
            //fill graph nodes with corresponding IR instuctions

            BoundBlockStatement body = program.Functions.Where(f => f.Key == program.MainFunction).Select(kvp => kvp.Value).First();
            var cfg = ControlFlowGraph.Create(body);
            VirtualRegister register = VirtualRegister.Num(1);
            cfg.Traverse((block) => {
                var emitter = new IREmitter(register, block);
                var instructions = emitter.Emit(out register);
                block.Instructions = instructions.ToList();
            });
            
            cfg.Dump("cfg_ir.dot", "main");

            /*if (program.Diagnostics.HasErrors())
                return program.Diagnostics;

            RecreateFolder("tmp");

            var irEmitter = new IREmitter_old();
            var irFile = Path.Combine("tmp", $"{moduleName}.ir");

            diagnostics = irEmitter.Emit(program, irFile);*/

            return diagnostics;
        }

        private void RecreateFolder(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }

        private void CreateExecutable(string source, string output)
        {                    
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"gcc {source} minsklib.c -o {output} -g\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
        }

        public void DumpControlFlowGraphs(string folderPth)
        {
            var program = GetProgram();
            
            foreach (var kvp in program.Functions)
            {
                var cfgPath = Path.Combine(folderPth, $"cfg_{kvp.Key.Name}.dot");
                var cfgStatement = kvp.Value;
                var cfg = ControlFlowGraph.Create(cfgStatement);
                using (var streamWriter = new StreamWriter(cfgPath))
                    cfg.WriteTo(streamWriter);
            }
        }
    }
}
