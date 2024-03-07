using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Minsk.CodeAnalysis.IR;
using Xunit;

namespace Minsk.Tests.RegisterAllocation
{
    public class RegisterAllocationTest
    {
        [Fact]
        public void Hello()
        {
            List<Instruction> instrutions = new List<Instruction>
            {
                Load.Create(VirtualRegister.Num(1), new Address("x")),
                Load.Create(VirtualRegister.Num(2), new Address("y")),
                Load.Create(VirtualRegister.Num(3), new Address("z")),
                Mul.Create(VirtualRegister.Num(4), VirtualRegister.Num(1), VirtualRegister.Num(3)),
                Add.Create(VirtualRegister.Num(5), VirtualRegister.Num(4), VirtualRegister.Num(2)),
                Add.Create(VirtualRegister.Num(6), VirtualRegister.Num(5), VirtualRegister.Num(1)),
                Store.Create(new Address("z"), VirtualRegister.Num(6))
            };

            int number = 1;
            foreach (var i in instrutions)
            {
                i.Id = number;
                number++;
            }

            RegisterAllocator allocator = new RegisterAllocator(2, instrutions.ToImmutableArray());

            var res = allocator.Allocate();

            {
                var load = res[0] as Load;
                Assert.True(load.Register == PhysicalRegister.Num(1));
            }

            {
                var load = res[1] as Load;
                Assert.True(load.Register == PhysicalRegister.Num(2));
            }

            {
                var store = res[2] as Store;
                Assert.True(store.Register == PhysicalRegister.Num(2));
                Assert.True(store.Address.VarName == "y");
            }


            {
                var load = res[3] as Load;
                Assert.True(load.Register == PhysicalRegister.Num(2));
                Assert.True(load.Source.AsT0.VarName == "z");
            }

            {
                var mul = res[4] as Mul;
                Assert.True(mul.Source1 == PhysicalRegister.Num(1));
                Assert.True(mul.Source2 == PhysicalRegister.Num(2));
                Assert.True(mul.Destination == PhysicalRegister.Num(2));
            }

            {
                var store = res[5] as Store;
                Assert.True(store.Register == PhysicalRegister.Num(1));
                Assert.True(store.Address.VarName == "x");
            }

            {
                var load = res[6] as Load;
                Assert.True(load.Register == PhysicalRegister.Num(1));
                Assert.True(load.Source.AsT0.VarName == "y");
            }

            {
                var add = res[7] as Add;
                Assert.True(add.Source1 == PhysicalRegister.Num(2));
                Assert.True(add.Source2 == PhysicalRegister.Num(1));
                Assert.True(add.Destination == PhysicalRegister.Num(2));
            }

            {
                var load = res[8] as Load;
                Assert.True(load.Register == PhysicalRegister.Num(1));
                Assert.True(load.Source.AsT0.VarName == "x");
            }

            {
                var add = res[9] as Add;
                Assert.True(add.Source1 == PhysicalRegister.Num(2));
                Assert.True(add.Source2 == PhysicalRegister.Num(1));
                Assert.True(add.Destination == PhysicalRegister.Num(2));
            }

            {
                var store = res[10] as Store;
                Assert.True(store.Register == PhysicalRegister.Num(2));
                Assert.True(store.Address.VarName == "z");
            }               
        }
    }

    internal class RegisterAllocator
    {
        internal List<PhysicalRegister> FreeList = new();

        internal VirtualRegistersDescriptor VirtualRegistersDescriptor = new();

        internal int CurrentInstructionNumber;

        internal PhysicalRegistersDescriptor PhysicalRegistersDescriptor = new();

        List<Instruction> output = new();

        public RegisterAllocator(int physicalRegCount, ImmutableArray<Instruction> instrutions)
        {
            PhysicalRegCount = physicalRegCount;
            Instrutions = instrutions;
        }

        internal List<Instruction> Allocate()
        {
            CreateFreeList();

            InitRegistersDescriptor(Instrutions);

            for (int i = 0; i < Instrutions.Length; i++)
            {
                CurrentInstructionNumber++;

                if(CurrentInstructionNumber == 5)
                    Debugger.Break();
                
                var op = Instrutions[i];

                if (op is BinaryInstruction binaryInstruction)
                {
                    VirtualRegister Rx = binaryInstruction.Source1 as VirtualRegister;
                    VirtualRegister Ry = binaryInstruction.Source2 as VirtualRegister;
                    VirtualRegister Rz = binaryInstruction.Destination as VirtualRegister;

                    Debug.Assert(Rx != null);
                    Debug.Assert(Ry != null);
                    Debug.Assert(Rz != null);

                    PhysicalRegister Px = Ensure(Rx);
                    PhysicalRegister Py = Ensure(Ry);

                    if (VirtualRegistersDescriptor.RegisterIsNotNeededAfter(Rx, CurrentInstructionNumber))
                        Free(Px);

                    if (VirtualRegistersDescriptor.RegisterIsNotNeededAfter(Ry, CurrentInstructionNumber))
                        Free(Py);

                    PhysicalRegister Pz = Alloc(Rz);

                    Rewrite(binaryInstruction, Px, Py, Pz);

                    PhysicalRegistersDescriptor.GetDescriptor(Px).NextUse = VirtualRegistersDescriptor.GetNextUseAfter(Rx, CurrentInstructionNumber);
                    PhysicalRegistersDescriptor.GetDescriptor(Py).NextUse = VirtualRegistersDescriptor.GetNextUseAfter(Ry, CurrentInstructionNumber);
                    PhysicalRegistersDescriptor.GetDescriptor(Pz).NextUse = VirtualRegistersDescriptor.GetNextUseAfter(Rz, CurrentInstructionNumber);

                }
                else
                {
                    if(op is Load load)
                    {
                        VirtualRegister Rz = load.Register as VirtualRegister;

                        Debug.Assert(Rz != null);
                        VirtualRegistersDescriptor.GetDescriptor(Rz).Address = load.Source.AsT0;

                        PhysicalRegister Pz = Alloc(Rz);
                        Emit(Load.Create(Pz, load.Source));
                        PhysicalRegistersDescriptor.GetDescriptor(Pz).NextUse = VirtualRegistersDescriptor.GetNextUseAfter(Rz, CurrentInstructionNumber);

                    }
                    else if(op is Store store)
                    {
                        VirtualRegister Rx = store.Register as VirtualRegister;
                        Debug.Assert(Rx != null);
                        PhysicalRegister Px = Ensure(Rx);
                        if (VirtualRegistersDescriptor.RegisterIsNotNeededAfter(Rx, CurrentInstructionNumber))
                            Free(Px);

                        Emit(Store.Create(store.Address, Px));

                        PhysicalRegistersDescriptor.GetDescriptor(Px).NextUse = VirtualRegistersDescriptor.GetNextUseAfter(Rx, CurrentInstructionNumber);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            return output;
        }

        private void Free(PhysicalRegister physicalRegister)
        {
            var descriptor = PhysicalRegistersDescriptor.GetDescriptor(physicalRegister);
            if(descriptor.Register != null)
            {
                var vRegDesc = VirtualRegistersDescriptor.GetDescriptor(descriptor.Register);
                vRegDesc.PhysicalRegister = null;
            }
            descriptor.Register = null;
            descriptor.NextUse = null;

            FreeList.Add(physicalRegister);
        }


        private PhysicalRegister Ensure(VirtualRegister virtualRegister)
        {
            PhysicalRegister p;

            var regDesc = VirtualRegistersDescriptor.GetDescriptor(virtualRegister);
            if (regDesc.PhysicalRegister != null)
                p = regDesc.PhysicalRegister;
            else
            {
                p = Alloc(virtualRegister);
                Emit(Load.Create(p, VirtualRegistersDescriptor.GetDescriptor(virtualRegister).Address));
            }

            return p;
        }

        private PhysicalRegister Alloc(VirtualRegister virtualRegister)
        {
            PhysicalRegister p;

            if (FreeList.Any())
            {
                p = FreeList.First();
                FreeList.Remove(p);
            }
            else
            {
                p = PhysicalRegistersDescriptor.GetRegisterWithMaxNextUse();
                VirtualRegister vReg = PhysicalRegistersDescriptor.GetDescriptor(p).Register!;
                var vRegDesc = VirtualRegistersDescriptor.GetDescriptor(vReg);
                vRegDesc.PhysicalRegister = null;
                Emit(Store.Create(vRegDesc.Address, p));
            }

            PhysicalRegistersDescriptor.GetDescriptor(p).NextUse = null;
            PhysicalRegistersDescriptor.GetDescriptor(p).Register = virtualRegister;
            VirtualRegistersDescriptor.GetDescriptor(virtualRegister).PhysicalRegister = p;

            return p;
        }

        private void Emit(Instruction instruction)
        {
            output.Add(instruction);
        }


        private void Rewrite(BinaryInstruction binaryInstruction, PhysicalRegister px, PhysicalRegister py, PhysicalRegister pz)
        {
            if(binaryInstruction is Add add)
            {
                Emit(Add.Create(pz, px, py));
            }
            else if(binaryInstruction is Sub sub)
            {
                Emit(Sub.Create(pz, px, py));
            }
            else if(binaryInstruction is Mul mul)
            {
                Emit(Mul.Create(pz, px, py));
            }
            else if(binaryInstruction is Div div)
            {
                Emit(Div.Create(pz, px, py));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private IEnumerable<Instruction> Rewrite(Load load)
        {
            Debug.Assert(load.Register is VirtualRegister);
            yield return Load.Create(FindPhysicalRegister(load.Register as VirtualRegister), load.Source);
        }

        private PhysicalRegister FindPhysicalRegister(VirtualRegister register)
        {
            var regDesc = VirtualRegistersDescriptor.GetDescriptor(register);
            if (regDesc.PhysicalRegister != null)
                return regDesc.PhysicalRegister;

            if (FreeList.Any())
            {
                var firstFree = FreeList.First();
                FreeList.Remove(firstFree);
                regDesc.PhysicalRegister = firstFree;
                return regDesc.PhysicalRegister;
            }

            throw new NotImplementedException();
        }

        private void InitRegistersDescriptor(ImmutableArray<Instruction> input)
        {
            foreach (var instrution in input)
            {
                foreach (VirtualRegister register in instrution.GetSources())
                {
                    var regDesc = VirtualRegistersDescriptor.GetDescriptor(register);
                    regDesc.Usages.Add(instrution.Id);
                }
            }
        }


        internal int PhysicalRegCount { get; }
        internal ImmutableArray<Instruction> Instrutions { get; }


        private void CreateFreeList()
        {
            for (int i = 0; i < PhysicalRegCount; i++)
            {
                FreeList.Add(PhysicalRegister.Num(i + 1));
            }
        }
    }
}