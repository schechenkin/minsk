using System;
using System.IO;
using static Minsk.CodeAnalysis.Binding.CFG.ControlFlowGraph;

namespace Minsk.CodeAnalysis.Binding.CFG
{
    internal static class ControlFlowGraphExtensions
    {
        public static void Dump(this ControlFlowGraph cfg, string fileName, string functionName)
        {
            var cfgPath = Path.Combine("tmp", $"{functionName}_{fileName}");
            using (var streamWriter = new StreamWriter(cfgPath))
                cfg.WriteTo(streamWriter);
        }

        public static void Traverse(this ControlFlowGraph cfg, Action<BasicBlock> action)
        {
            new BasicBlockDFSVisitor().Visit(cfg.Start, (block) =>
                {
                    if (block != cfg.Start && block != cfg.End)
                    {
                        action(block);
                    }
                });
        }
    }
}