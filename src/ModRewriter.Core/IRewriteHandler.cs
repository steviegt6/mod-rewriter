using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace ModRewriter.Core
{
    /// <summary>
    ///     Capable of handling <see cref="ISyntaxRewriter"/>s.
    /// </summary>
    public interface IRewriteHandler
    {
        /// <summary>
        ///     Installs a <see cref="ISyntaxRewriter"/> instance to this handler.
        /// </summary>
        /// <param name="rewriter">The rewriter instance to install, to be used later.</param>
        void InstallRewriter(ISyntaxRewriter rewriter);

        /// <summary>
        ///     Rewrites a <see cref="Microsoft.CodeAnalysis.Document"/> using all installed rewriters.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        Task RewriteDocument(Document doc);
    }
}