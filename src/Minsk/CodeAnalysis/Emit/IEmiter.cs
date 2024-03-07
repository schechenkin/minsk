using System.Collections.Immutable;
using Minsk.CodeAnalysis.Binding;

namespace Minsk.CodeAnalysis.Emit
{
    internal interface IEmiiter
    {
        ImmutableArray<Diagnostic> Emit(BoundProgram program, string outputPath);
    }
}