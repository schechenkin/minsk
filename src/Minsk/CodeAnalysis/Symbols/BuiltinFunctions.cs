using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Minsk.CodeAnalysis.Symbols
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol PrintInt = new FunctionSymbol("printInt", ImmutableArray.Create(new ParameterSymbol("number", TypeSymbol.Int, 0)), TypeSymbol.Void);
        public static readonly FunctionSymbol PrintString = new FunctionSymbol("print", ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.String, 0)), TypeSymbol.Void);
        public static readonly FunctionSymbol Input = new FunctionSymbol("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);
        public static readonly FunctionSymbol Rnd = new FunctionSymbol("rnd", ImmutableArray.Create(new ParameterSymbol("max", TypeSymbol.Int, 0)), TypeSymbol.Int);

        public static readonly FunctionSymbol StartTimer = new FunctionSymbol("start_timer", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void);
        public static readonly FunctionSymbol StopTimer = new FunctionSymbol("stop_timer", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Void);

        internal static IEnumerable<FunctionSymbol> GetAll()
            => typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                                       .Where(f => f.FieldType == typeof(FunctionSymbol))
                                       .Select(f => (FunctionSymbol)f.GetValue(null)!);
    }
}