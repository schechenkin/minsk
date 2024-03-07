using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Minsk.CodeAnalysis.Syntax;

namespace Minsk.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        protected BoundNode(SyntaxNode syntax)
        {
            Syntax = syntax;
        }

        public abstract BoundNodeKind Kind { get; }
        public SyntaxNode Syntax { get; }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                this.WriteTo(writer);
                return writer.ToString();
            }
        }

        public IEnumerable<BoundNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
                {
                    var child = (BoundNode)property.GetValue(this);
                    if (child != null)
                        yield return child;
                }
                else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                {
                    var children = (IEnumerable<BoundNode>)property.GetValue(this);
                    foreach (var child in children)
                    {
                        if (child != null)
                            yield return child;
                    }
                }
            }
        }

        private IEnumerable<(string Name, object Value)> GetProperties()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.Name == nameof(Kind) ||
                    property.Name == nameof(BoundBinaryExpression.Op))
                    continue;

                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType) ||
                    typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                    continue;

                var value = property.GetValue(this);
                if (value != null)
                    yield return (property.Name, value);
            }
        }
    }
}
