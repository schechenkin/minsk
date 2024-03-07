using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;
using OneOf;
using OneOf.Types;

namespace Minsk.CodeAnalysis.IR
{
    //a - 2 * b
    //Y arp = activation record pointer
    internal enum Operation { LoadI, Add, Sub, Mul, Div, Store, LoadAO, Cmp_LT,Cmp_LE, Cmp_EQ, Cmp_GE, Cmp_GT, Cmp_NE, Cbr, Call, Label, Jump }
    internal abstract record class Instruction
    {
        public abstract Operation Op { get; }
        public int Id { get; set; }
        internal abstract ImmutableArray<Register> GetSources();
        internal abstract OneOf<Register, None> Defines { get; }
        internal virtual ImmutableArray<Register> GetUses() => GetSources();

    }

    internal abstract record class BinaryInstruction : Instruction
    {
        public BinaryInstruction(Register destination, Register source1, Register source2)
        {
            Destination = destination;
            Source1 = source1;
            Source2 = source2;
        }

        internal override OneOf<Register, None> Defines => Destination;
        internal override ImmutableArray<Register> GetSources() => [Source1, Source2];

        public Register Destination { get; }
        public Register Source1 { get; }
        public Register Source2 { get; }

        public override string ToString()
        {
            return $"{Destination} = {Op} {Source1} {Source2}";
        }
    }

    internal sealed record class Variable(string Name);
    internal sealed record class StackPosition(int Offset);
    internal sealed record class InstructionNumber(int Number)
    {
        public static implicit operator InstructionNumber(int number) => new InstructionNumber(number);
    }

    internal sealed class VirtualRegistersDescriptor
    {
        private Dictionary<VirtualRegister, Descriptor> _descriptors = new();

        internal record class Descriptor
        {
            public PhysicalRegister? PhysicalRegister { get; set; }
            public List<InstructionNumber> Usages { get; set; } = new();
            public Address Address { get; set; }
        }

        public Descriptor GetDescriptor(VirtualRegister register)
        {
            if (!_descriptors.ContainsKey(register))
                _descriptors.Add(register, new Descriptor());

            return _descriptors[register];
        }

        internal bool RegisterIsNotNeededAfter(VirtualRegister register, InstructionNumber instructionNumber)
        {
            return GetNextUseAfter(register, instructionNumber) == null;
        }

        internal InstructionNumber? GetNextUseAfter(VirtualRegister register, InstructionNumber instructionNumber)
        {
            var descriptor = GetDescriptor(register);
            return descriptor.Usages.FirstOrDefault(u => u.Number > instructionNumber.Number);
        }
    }

    internal sealed class PhysicalRegistersDescriptor
    {
        private Dictionary<PhysicalRegister, Descriptor> _descriptors = new();

        internal PhysicalRegister GetRegisterWithMaxNextUse()
        {
            PhysicalRegister? r = null;
            int maxNextUse = -1;

            foreach (var kvp in _descriptors)
            {
                var reg = kvp.Key;
                var nextUse = kvp.Value.NextUse;
                if (nextUse != null)
                {
                    if (nextUse.Number > maxNextUse)
                    {
                        r = reg;
                        maxNextUse = nextUse.Number;
                    }
                }
            }

            if (r is null)
                throw new NotImplementedException();

            return r;
        }

        public Descriptor GetDescriptor(PhysicalRegister register)
        {
            if (!_descriptors.ContainsKey(register))
                _descriptors.Add(register, new Descriptor());

            return _descriptors[register];
        }


        internal record class Descriptor
        {
            public VirtualRegister? Register { get; set; }
            public InstructionNumber? NextUse { get; set; }
        }
    }

    internal sealed class AddressDescriptor
    {
        private Dictionary<Variable, Descriptor> _descriptors = new();

        private record class Descriptor
        {
            public List<OneOf<Register, Address, StackPosition>> Values { get; } = new();
        }
    }

    internal sealed record class Jump : Instruction
    {
        public Jump(Label label)
        {
            Label = label;

        }


        public override Operation Op => Operation.Label;

        public Label Label { get; }


        internal override OneOf<Register, None> Defines => new None();

        internal static Instruction Create(Label label) => new Jump(label);

        internal override ImmutableArray<Register> GetSources() =>[];
    }

    internal sealed record class LabelInst : Instruction
    {
        public LabelInst(Label label)
        {
            Label = label;
        }


        public override Operation Op => Operation.Label;

        public Label Label { get; }


        internal override OneOf<Register, None> Defines  => new None();

        internal static Instruction Create(Label label) => new LabelInst(label);

        internal override ImmutableArray<Register> GetSources() =>[];

        public override string ToString()
        {
            return $"{Label.Name}:";
        }

    }

    internal sealed record class Call : Instruction
    {
        public Call(FunctionSymbol function, Register destination, List<Register> arguments)
        {
            Function = function;

            Destination = destination;
            Arguments = arguments;
        }
        public override Operation Op => Operation.Call;
        internal override OneOf<Register, None> Defines => Destination;
        public FunctionSymbol Function { get; }
        public Register Destination { get; }
        public List<Register> Arguments { get; }
        internal override ImmutableArray<Register> GetSources() => Arguments.ToImmutableArray();

        public static Call Create(FunctionSymbol function, Register destination, List<Register> arguments) => new Call(function, destination, arguments);

        public override string ToString()
        {
            return $"{Destination} = Call {Function.Name} ({PrintArgs()})";
        }

        private string PrintArgs()
        {
            return string.Join(',', Arguments);
        }

    }

    internal sealed record class Load : Instruction
    {
        public override Operation Op => Operation.LoadI;

        public Register Register { get; }
        public OneOf<Address, long> Source { get; }

        internal override OneOf<Register, None> Defines => Register;

        private Load(Register register, OneOf<Address, long> source)
        {
            Register = register;
            Source = source;
        }

        public static Load Create(Register destination, OneOf<Address, long> source) => new Load(destination, source);

        internal override ImmutableArray<Register> GetSources() => [];

        public override string ToString()
        {
            return Source.Match(
                (address) => $"{Register} = Load Addr({address.VarName})",
                (val) =>$"{Register} = Load {val}");
        }
    }

    internal sealed record class Store : Instruction
    {
        public override Operation Op => Operation.Store;

        public Register Register { get; }
        public Address Address { get; }

        internal override OneOf<Register, None> Defines => new None();


        private Store(Register register, Address address)
        {
            Register = register;
            Address = address;
        }
        public static Store Create(Address destination, Register source) => new Store(source, destination);

        internal override ImmutableArray<Register> GetSources() => [Register];

        public override string ToString()
        {
            return $"Store {Register} Addr({Address.VarName})";
        }
    }

    internal sealed record class Add : BinaryInstruction
    {
        public Add(Register destination, Register source1, Register source2)
            : base(destination, source1, source2)
        {
        }


        public override Operation Op => Operation.Add;

        public static Add Create(Register destination, Register source1, Register source2) => new Add(destination, source1, source2);
    }

    internal sealed record class Sub : BinaryInstruction
    {
        public Sub(Register destination, Register source1, Register source2)
            : base(destination, source1, source2)
        {
        }


        public override Operation Op => Operation.Sub;

        public static Sub Create(Register destination, Register source1, Register source2) => new Sub(destination, source1, source2);
    }

    internal sealed record class Mul : BinaryInstruction
    {
        public Mul(Register destination, Register source1, Register source2)
            : base(destination, source1, source2)
        {

        }

        public override Operation Op => Operation.Mul;

        public static Mul Create(Register destination, Register source1, Register source2) => new Mul(destination, source1, source2);
    }

    internal sealed record class Div : BinaryInstruction
    {
        public Div(Register destination, Register source1, Register source2)
            : base(destination, source1, source2)
        {
        }


        public override Operation Op => Operation.Div;

        public static Div Create(Register destination, Register source1, Register source2) => new Div(destination, source1, source2);
    }

    internal sealed record class Compare : BinaryInstruction
    {
        public Compare(Register destination, Register source1, Register source2, Operation operation)
            : base(destination, source1, source2)
        {
            Operation = operation;
        }
        public override Operation Op => Operation;

        public Operation Operation { get; }

        public override string ToString()
        {
            return $"{Destination} = {Op} {Source1} {Source2}";
        }

        public static Compare Create(Register destination, Register source1, Register source2, Operation operation) => new Compare(destination, source1, source2, operation);
    }

    internal sealed record class ConditionalBranch : Instruction
    {
        public override Operation Op => Operation.Cbr;

        public Register Register { get; }
        public Label Label { get; }
        public bool JumpIfTrue { get; }

        internal override OneOf<Register, None> Defines => new None();

        private ConditionalBranch(Register register, Label label, bool jumpIfTrue)
        {
            Register = register;
            Label = label;
            JumpIfTrue = jumpIfTrue;
        }

        public static ConditionalBranch Create(Register register, Label label, bool jumpIfTrue) => new ConditionalBranch(register, label, jumpIfTrue);

        internal override ImmutableArray<Register> GetSources() => [];

        public override string ToString()
        {
            if(JumpIfTrue)
                return $"Cbr {Register} {Label.Name} if true";
            else
                return $"Cbr {Register} {Label.Name} if false";
        }
    }

    internal abstract record class Register
    {
        public int Number { get; protected set; }

    }

    internal record class PhysicalRegister : Register
    {
        public static PhysicalRegister Num(int number)
        {
            return new PhysicalRegister(number);
        }

        private PhysicalRegister(int number)
        {
            Number = number;
        }

        public override string ToString()
        {
            return $"P{Number}";
        }

    }

    internal record class VirtualRegister : Register
    {
        public static VirtualRegister Num(int number)
        {
            return new VirtualRegister(number);
        }

        private VirtualRegister(int number)
        {
            Number = number;
        }

        public override string ToString()
        {
            return $"V{Number}";
        }

    }

    internal record class Address(string VarName)
    {
        public override string ToString() => $"Address of {VarName}";
    }
    internal record class Label(string Name);
}