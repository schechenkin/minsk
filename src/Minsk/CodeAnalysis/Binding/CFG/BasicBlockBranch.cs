namespace Minsk.CodeAnalysis.Binding.CFG
{
    internal sealed partial class ControlFlowGraph
    {
        public sealed class BasicBlockBranch
        {
            public BasicBlockBranch(BasicBlock from, BasicBlock to, BoundExpression? condition = null)
            {
                From = from;
                To = to;
                Condition = condition;
            }

            public BasicBlock From { get; }
            public BasicBlock To { get; }
            public BoundExpression? Condition { get; }

            public override string ToString()
            {
                if (Condition == null)
                    return string.Empty;

                return Condition.ToString();
            }
        }
    }
}
