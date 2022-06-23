using System.Diagnostics;

namespace Xandevelop.Wigwam.Ast
{
    [DebuggerDisplay("{SourceLine}")]
    public abstract class AstBase
    {
        public string SourceFile { get; set; }
        public string SourceLine { get; set; }
        public int SourceLineNumber { get; set; }
    }
}
