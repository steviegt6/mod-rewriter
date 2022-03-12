using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModRewriter.Core.Exceptions;
using UtfUnknown;

namespace ModRewriter.Core.Impl
{
    public class RewriteHandler : IRewriteHandler
    {
        public List<ISyntaxRewriter> InstalledRewriters { get; } = new();

        public void InstallRewriter(ISyntaxRewriter rewriter) => InstalledRewriters.Add(rewriter);

        public async Task RewriteDocument(Document doc)
        {
            SyntaxTree treeRoot = await doc.GetSyntaxTreeAsync() ??
                                  throw new SyntaxRootNotFoundException("No syntax root in document: " + doc.FilePath);

            SyntaxNode treeRootNode = await treeRoot.GetRootAsync();

            await Rewrite(
                treeRootNode,
                await doc.GetSemanticModelAsync() ?? throw new SemanticModelNotFoundException(nameof(doc)),
                doc
            );
        }

        public async Task Rewrite(SyntaxNode treeRootNode, SemanticModel model, Document doc)
        {
            DocumentRewriter docRewriter = new(model, InstalledRewriters);
            docRewriter.Visit(treeRootNode);

            if (await docRewriter.RewriteNodes(treeRootNode) is not CompilationUnitSyntax result)
                throw new InvalidOperationException(
                    $"Rewritten node was not of type \"{nameof(CompilationUnitSyntax)}\""
                );

            result = docRewriter.AddUsingDirectives(result);

            if (!result.IsEquivalentTo(treeRootNode) && doc.FilePath is not null)
            {
                Encoding encoding;

                await using (Stream stream = new FileStream(doc.FilePath, FileMode.Open, FileAccess.Read))
                {
                    DetectionResult res = CharsetDetector.DetectFromStream(stream);
                    encoding = res.Detected.Encoding;
                }

                await File.WriteAllTextAsync(doc.FilePath, result.ToFullString(), encoding);
            }
        }
    }
}