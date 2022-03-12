using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ModRewriter.Core
{
    /// <summary>
    ///     Extended <see cref="CSharpSyntaxRewriter"/> specialized for utilizing <see cref="ISyntaxRewriter"/>s. <br />
    ///     Instances should be supplied by a <see cref="IRewriteHandler"/>.
    /// </summary>
    public class DocumentRewriter : CSharpSyntaxRewriter
    {
        public SemanticModel Model { get; }

        public List<ISyntaxRewriter> Rewriters { get; }

        public readonly HashSet<(ISyntaxRewriter rewriter, SyntaxNode originalNode)> NodesToRewrite = new();
        public readonly HashSet<(ISyntaxRewriter rewriter, SyntaxToken originalToken)> TokensToRewrite = new();
        public readonly HashSet<(ISyntaxRewriter rewriter, SyntaxTrivia originalTrivia)> TriviaToRewrite = new();
        public readonly List<string> UsingsList = new();

        public DocumentRewriter(SemanticModel model, List<ISyntaxRewriter> rewriters)
        {
            Model = model;
            Rewriters = rewriters;
        }

        public virtual async Task<SyntaxNode> RewriteNodes(SyntaxNode treeRootNode)
        {
            Dictionary<SyntaxNode, SyntaxNode> nodeDict = new();

            foreach ((ISyntaxRewriter rewriter, SyntaxNode originalNode) in NodesToRewrite)
            {
                SyntaxNode newNode = await rewriter.RewriteNode(originalNode);
                nodeDict.Add(originalNode, newNode);
            }

            Dictionary<SyntaxToken, SyntaxToken> tokenDict = new();
            foreach ((ISyntaxRewriter rewriter, SyntaxToken originalToken) in TokensToRewrite)
            {
                SyntaxToken newToken = await rewriter.RewriteToken(originalToken);
                tokenDict.Add(originalToken, newToken);
            }

            Dictionary<SyntaxTrivia, SyntaxTrivia> triviaDict = new();
            foreach ((ISyntaxRewriter rewriter, SyntaxTrivia originalTrivia) in TriviaToRewrite)
            {
                SyntaxTrivia newTrivia = await rewriter.RewriteTrivia(originalTrivia);
                triviaDict.Add(originalTrivia, newTrivia);
            }

            return treeRootNode.ReplaceSyntax(
                nodeDict.Keys.AsEnumerable(),
                (orig, _) => nodeDict[orig],
                tokenDict.Keys.AsEnumerable(),
                (orig, _) => tokenDict[orig],
                triviaDict.Keys.AsEnumerable(),
                (orig, _) => triviaDict[orig]
            );
        }

        public CompilationUnitSyntax AddUsingDirectives(CompilationUnitSyntax syntax)
        {
            UsingDirectiveSyntax[] usingDirectives = UsingsList
                .Where(x => !syntax.Usings.Select(y => y.Name.ToString()).Contains(x))
                .Select(@using => SyntaxFactory
                    .UsingDirective(SyntaxFactory.IdentifierName(" " + @using))
                    .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed)
                )
                .ToArray();

            return syntax.AddUsings(usingDirectives);
        }

        public SyntaxNode VisitRewriters(SyntaxNode node, VisitorExpressionContext expressionContext)
        {
            foreach (ISyntaxRewriter rewriter in Rewriters)
                rewriter.VisitNode(node, expressionContext);

            return node;
        }


        public override SyntaxNode? VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) =>
            base.VisitAnonymousMethodExpression(
                (AnonymousMethodExpressionSyntax) VisitRewriters(node,
                    VisitorExpressionContext.AnonymousMethodExpression)
            );

        public override SyntaxNode? VisitAssignmentExpression(AssignmentExpressionSyntax node) =>
            base.VisitAssignmentExpression(
                (AssignmentExpressionSyntax) VisitRewriters(node, VisitorExpressionContext.AssignmentExpression)
            );

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node) =>
            base.VisitIdentifierName(
                (IdentifierNameSyntax) VisitRewriters(node, VisitorExpressionContext.IdentifierNameExpression)
            );

        public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node) =>
            base.VisitInvocationExpression(
                (InvocationExpressionSyntax) VisitRewriters(node, VisitorExpressionContext.InvocationExpression)
            );

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node) =>
            base.VisitMemberAccessExpression(
                (MemberAccessExpressionSyntax) VisitRewriters(node, VisitorExpressionContext.MemberAccessExpression)
            );

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node) =>
            base.VisitMethodDeclaration(
                (MethodDeclarationSyntax) VisitRewriters(node, VisitorExpressionContext.MethodDeclarationExpression)
            );

        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node) =>
            base.VisitUsingDirective(
                (UsingDirectiveSyntax) VisitRewriters(node, VisitorExpressionContext.UsingDirectiveExpression)
            );
    }
}