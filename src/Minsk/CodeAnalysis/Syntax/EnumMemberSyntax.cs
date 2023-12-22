namespace Minsk.CodeAnalysis.Syntax
{
    public sealed partial class EnumMemberSyntax : SyntaxNode
    {
        internal EnumMemberSyntax(SyntaxTree syntaxTree, SyntaxToken identifier)
            : base(syntaxTree)
        {
            Identifier = identifier;
        }

        public override SyntaxKind Kind => SyntaxKind.EnumMember;
        public SyntaxToken Identifier { get; }
    }
}