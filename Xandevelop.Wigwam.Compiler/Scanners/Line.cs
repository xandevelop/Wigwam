using System.Collections.Generic;

namespace Xandevelop.Wigwam.Compiler.Scanners
{
    public class Line
    {
        public string SourceFile { get; set; }
        public string SourceLine { get; set; }
        public int SourceLineNumber { get; set; }


        public string Command { get; set; }
        public List<string> Blocks { get; set; } = new List<string>(); // Strings after command.
        public string CommentBlock { get; set; }
    }
}
