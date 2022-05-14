using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler
{
    public enum CompileMessageType
    {
        Info,
        Warning,
        Error
    }

    public class CompileMessage
    {
        public string SourceFile { get; set; }
        public int SourceLineNumber { get; set; }
        public string SourceLine { get; set; }

        public CompileMessageType MessageType { get; set; }
        public string Text { get; set; }
    }

    public class StandardMessages
    {
        public static string CommandOutsideOfMethod(Line l) => "Command must be inside a test or function";
        public static string NoParserFound(Line l) => $"Invalid command '{l.Command}'.  No matching parser found.  Make sure the command on this line is valid and that the compiler has been correctly configured.  This is normally caused by a compiler not using the function call parser, or similar.";
        public static string FunctionCallOutsideOfMethod(Line l) => "Function call must be inside a test or a function";
    }
}
