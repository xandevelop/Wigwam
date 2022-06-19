using System.Globalization;
using System.Linq;
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

        public static string PreConditionOutsideOfFunction(Line l) => "PreCondition must be inside a function";
        public static string PostConditionOutsideOfFunction(Line l) => "PostCondition must be inside a function";

        public static string IncludeMustSpecifyPath(Line l) => $"{l.Command.ToTitleCaseNotAcronym()} must specify a path"; // line command is include or import or using
        public static string IncludeFileNotFound(Line l) => $"Included file not found '{l.Blocks.First()}'";

        public static string UnrecognisedComparison(Line l, string comparison) => $"Comparison value '{comparison}' not recognised.  Options include equals, not equals, contains, not contains, regex.";

        // Function does not exist by name.  Different to function exists but you're not specifying correct args.
        public static string FunctionSignatureNotFound_NoFunctionWithName(Line l) => $"Function {l.Command} could not be found.  Make sure you define the function somewhere and that you've typed the name correctly.";
        public static string FunctionSignatureNotFound_SinglePossibility_ArgumentsIncorrect(Line l) => $"Function {l.Command} could not be matched.  Argument mismatch.  You seem to be trying to use this function, but the arguments you have specified are invalid.";
        public static string FunctionSignatureNotFound_MultiplePossibility_ArgumentsIncorrect(Line l) => $"Function {l.Command} could not be matched.  Ambiguous invocation.  There are multiple functions that might match, but the compiler cannot work out which one you mean.  Change the parameters or preconditions of the functions so it is clearer which one you are trying to use.";

        public static string FunctionSignatureNotFound_MultiplePossibility_PreConditionsNotMet(Line l) => $"Function {l.Command} could not be matched.  Ambiguous invocation.  There are multiple functions that might match, but the compiler cannot work out which one you mean.  Change the parameters or preconditions of the functions so it is clearer which one you are trying to use.";
        public static string FunctionSignatureNotFound_MultiplePossibility_TooManyPreconditionMatches(Line l) => "too many matches";
#warning write better error message
    }

    public static class StringExn
    {
        public static string ToTitleCaseNotAcronym(this string s)
        {
            var textInfo = new CultureInfo("en-us", false).TextInfo;
            return textInfo.ToTitleCase(s.ToLower());
        }
    }
}
