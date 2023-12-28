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
            var enumTypeKey = $"enum:{enumTypeName}";
            if (!_userDefinedTypes.ContainsKey(enumTypeKey))
            {
                _userDefinedTypes.Add(enumTypeKey, new TypeSymbol(enumTypeName, isEnum: true));
            }

            return _userDefinedTypes[enumTypeKey];
        }
        public static TypeSymbol ArrayOf(TypeSymbol elementType)
        {
            var arrayTypeKey = $"array:{elementType.Name}";
            if (!_userDefinedTypes.ContainsKey(arrayTypeKey))
            {
                _userDefinedTypes.Add(arrayTypeKey, new TypeSymbol(elementType.Name, isEnum: false, isArray: true));
            }

            return _userDefinedTypes[arrayTypeKey];
        }

        private bool _isEnum = false;
        private bool _isArray = false;

        public bool IsEnum() => _isEnum;
        public bool IsArray() => _isArray;

        internal TypeSymbol GetArrayElementType()
        {
            return TypeSymbol.Int;
        }

        private static Dictionary<string, TypeSymbol> _userDefinedTypes = new();

        private TypeSymbol(string name, bool isEnum = false, bool isArray = false)
            : base(name)
        {
            _isEnum = isEnum;
            _isArray = isArray;
        }

        public override SymbolKind Kind => SymbolKind.Type;
    }
}