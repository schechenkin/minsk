using System;
using Minsk.CodeAnalysis.Binding;

namespace Minsk.CodeAnalysis.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        internal VariableSymbol(string name, bool isReadOnly, TypeSymbol type, BoundConstant? constant)
            : base(name)
        {
            IsReadOnly = isReadOnly;
            Type = type;
            Constant = isReadOnly ? constant : null;
        }

        public bool IsReadOnly { get; }
        public TypeSymbol Type { get; }
        internal BoundConstant? Constant { get; }

        public override bool Equals(object? obj)
        {
            if(obj == null)
                return false;

            if(obj is VariableSymbol v)
            {
                return this.Name == v.Name;
            }
            
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}