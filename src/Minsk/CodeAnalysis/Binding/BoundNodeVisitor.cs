using System;

namespace Minsk.CodeAnalysis.Binding
{
    internal static class BoundNodeVisitorExtensions
    {
        public static void Visit(this BoundNode node, Action<BoundNode> onEnter)
        {
            new BoundNodeVisitor().Visit(node, onEnter);
        }
    }
    
    internal class BoundNodeVisitor
    {
        int _level = 0;
        private Action<BoundNode>? _onEnter;

        public void Visit(BoundNode node, Action<BoundNode> onEnter)
        {
            _onEnter = onEnter;
            Traverse(node, level: 0);
        }

        public void Traverse(BoundNode node)
        {
            Traverse(node, level: 0);
        }
        
        private void Traverse(BoundNode node, int level)
        {
            Visit(node, level);

            if(_onEnter != null)
                _onEnter(node);

            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    Visit((BoundBlockStatement)node, level);
                    break;
                case BoundNodeKind.NopStatement:
                    Visit((BoundNopStatement)node, level);
                    break;
                case BoundNodeKind.VariableDeclaration:
                    Visit((BoundVariableDeclaration)node, level);
                    break;
                case BoundNodeKind.IfStatement:
                    Visit((BoundIfStatement)node, level);
                    break;
                case BoundNodeKind.WhileStatement:
                    Visit((BoundWhileStatement)node, level);
                    break;
                case BoundNodeKind.DoWhileStatement:
                    Visit((BoundDoWhileStatement)node, level);
                    break;
                case BoundNodeKind.ForStatement:
                    Visit((BoundForStatement)node, level);
                    break;
                case BoundNodeKind.LabelStatement:
                    Visit((BoundLabelStatement)node, level);
                    break;
                case BoundNodeKind.GotoStatement:
                    Visit((BoundGotoStatement)node, level);
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    Visit((BoundConditionalGotoStatement)node, level);
                    break;
                case BoundNodeKind.ReturnStatement:
                    Visit((BoundReturnStatement)node, level);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    Visit((BoundExpressionStatement)node, level);
                    break;
                case BoundNodeKind.ErrorExpression:
                    Visit((BoundErrorExpression)node, level);
                    break;
                case BoundNodeKind.LiteralExpression:
                    Visit((BoundLiteralExpression)node, level);
                    break;
                case BoundNodeKind.VariableExpression:
                    Visit((BoundVariableExpression)node, level);
                    break;
                case BoundNodeKind.AssignmentExpression:
                    Visit((BoundAssignmentExpression)node, level);
                    break;
                case BoundNodeKind.CompoundAssignmentExpression:
                    Visit((BoundCompoundAssignmentExpression)node, level);
                    break;
                case BoundNodeKind.UnaryExpression:
                    Visit((BoundUnaryExpression)node, level);
                    break;
                case BoundNodeKind.BinaryExpression:
                    Visit((BoundBinaryExpression)node, level);
                    break;
                case BoundNodeKind.CallExpression:
                    Visit((BoundCallExpression)node, level);
                    break;
                case BoundNodeKind.ConversionExpression:
                    Visit((BoundConversionExpression)node, level);
                    break;
                default:
                    break;
            }
            level++;
            foreach(var child in node.GetChildren())
            {
                Traverse(child, level);
            }
        }

        protected virtual void Visit(BoundNode node, int level)
        {
            
        }

        protected virtual void Visit(BoundBlockStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundNopStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundVariableDeclaration node, int level)
        {
            
        }

        protected virtual void Visit(BoundIfStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundWhileStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundDoWhileStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundForStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundLabelStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundGotoStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundConditionalGotoStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundReturnStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundExpressionStatement node, int level)
        {
            
        }

        protected virtual void Visit(BoundErrorExpression node, int level)
        {
            
        }

        protected virtual void Visit(BoundLiteralExpression node, int level)
        {
            
        }

        protected virtual void Visit(BoundVariableExpression node, int level)
        {
            
        }

        protected virtual void Visit(BoundAssignmentExpression node, int level)
        {
            
        }

        protected virtual void Visit(BoundCompoundAssignmentExpression node, int level)
        {
            
        }

        protected virtual void Visit(BoundUnaryExpression node, int level)
        {
            
        }

        protected virtual void Visit(BoundBinaryExpression node, int level)
        {
            
        }

        protected virtual void Visit(BoundCallExpression node, int level)
        {
            
        }

        protected virtual void Visit(BoundConversionExpression node, int level)
        {
            
        }
    }
}