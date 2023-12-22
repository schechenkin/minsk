using System;
using System.Collections.Generic;

namespace Minsk.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol  : Symbol
    {
        public static readonly TypeSymbol Error = new TypeSymbol("?");
        public static readonly TypeSymbol Any = new TypeSymbol("any");
        public static readonly TypeSymbol Bool = new TypeSymbol("bool");
        public static readonly TypeSymbol Int = new TypeSymbol("int");
        public static readonly TypeSymbol String = new TypeSymbol("string");
        public static readonly TypeSymbol Void = new TypeSymbol("void");

        public static TypeSymbol Enum(string enumTypeName)
        {
            if (!_userDefinedTypes.ContainsKey(enumTypeName))
            {
                _userDefinedTypes.Add(enumTypeName, new TypeSymbol(enumTypeName, isEnum: true));
            }

            return _userDefinedTypes[enumTypeName];
        }

        private bool _isEnum = false;

        public bool IsEnum() => _isEnum;

        private static Dictionary<string, TypeSymbol> _userDefinedTypes = new();

        private TypeSymbol(string name, bool isEnum = false)
            : base(name)
        {
            _isEnum = isEnum;
        }

        public override SymbolKind Kind => SymbolKind.Type;
    }
}