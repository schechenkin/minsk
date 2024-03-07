using System;
using System.CodeDom.Compiler;
using System.IO;
using Minsk.CodeAnalysis.Symbols;
using Minsk.CodeAnalysis.Syntax;
using Minsk.IO;

namespace Minsk.CodeAnalysis.Binding
{
    internal class BoundNodeTreeViewPrinter : BoundNodeVisitor
    {
        readonly IndentedTextWriter _writer;
        
        public BoundNodeTreeViewPrinter(TextWriter writer)
        {
            if (writer is IndentedTextWriter iw)
                _writer = iw;
            else
                _writer = new IndentedTextWriter(writer);
        }

        public void Print(BoundNode node)
        {
            Traverse(node);
        }
        
        protected override void Visit(BoundNode node, int level)
        {
            _writer.Indent = level;
            _writer.WriteLine($"{node.Kind} {node.ToString()}");
        }
    }
}
