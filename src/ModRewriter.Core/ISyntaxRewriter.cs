using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace ModRewriter.Core
{
    /// <summary>
    ///     The bare minimum for a <see cref="Document"/> syntax rewriter. Must be installed to a <see cref="IRewriteHandler"/>.
    /// </summary>
    public interface ISyntaxRewriter
    {
        void VisitNode(SyntaxNode nodeToVisit, VisitorExpressionContext expressionContext);

        Task<SyntaxNode> RewriteNode(SyntaxNode nodeToRewrite);

        Task<SyntaxToken> RewriteToken(SyntaxToken tokenToRewrite);

        Task<SyntaxTrivia> RewriteTrivia(SyntaxTrivia triviaToRewrite);
    }
}