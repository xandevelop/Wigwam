using System.Diagnostics;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Ast
{
    [DebuggerDisplay("{SourceLine}")]
    public abstract class AstBase
    {
        public Line SourceCode { get; set; }

        public string SourceFile => SourceCode.SourceFile;
        public string SourceLine => SourceCode.SourceLine;
        public int SourceLineNumber => SourceCode.SourceLineNumber;

        public abstract string ToDebugString();
    }
}
