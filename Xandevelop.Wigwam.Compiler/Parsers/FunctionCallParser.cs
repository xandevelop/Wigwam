using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    public class FunctionCallParser : ILineParser
    {
        public string Name => "Function Call Parser";

        public int OrderNumber => 9999;

        public bool IsMatch(Line line)
        {
            return true; // Catch all.  Note: high order number means we shouldn't catch any more specific things.
        }

        ArgumentScanner argScanner = new ArgumentScanner();
        public void Parse(AstBuilder ast, Line line)
        {
            AstFunctionCallNoContext astFunctionCall = new AstFunctionCallNoContext
            {
                SourceFile = line.SourceFile,
                SourceLine = line.SourceLine,
                SourceLineNumber = line.SourceLineNumber,

                ContextFreeArguments = argScanner.ScanLineArgumentsWithoutContext(line),
                FunctionName = line.Command,
                SourceLineForPatchup = line,
                Description = line.CommentBlock
            };

            if (ast.HasCurrentMethod)
            {
                ast.AddStatementToCurrentMethod(astFunctionCall);
            }
            else
            {
                ast.AddError(StandardMessages.FunctionCallOutsideOfMethod(line));
            }
        }
    }

    // Note: First pass generates these, second pass removes them and replaces wiht AstFunctionCalls.
    public class AstFunctionCallNoContext : IAstStatement
    {
        public string FunctionName { get; set; } // For signature matching - note: we'll discard this in second pass in favor of a pointer to the actual matched function

        // Arguments BEFORE we match with a function signature, so these may not be valid, they're just what the scanner/parser read on first pass.
        public List<AstArgument> ContextFreeArguments { get; internal set; }

        public Line SourceLineForPatchup { get; set; }

        public string Description { get; set; }
        public List<AstFunctionCallTemp> FunctionResolved { get; internal set; } = new List<AstFunctionCallTemp>();

        public void SetFunctionResolved(AstFunctionCallTemp funcCall)
        {
            FunctionResolved.Add(funcCall);
        }
        public void SetFunctionResolved(List<AstFunctionCallTemp> funcCall)
        {
            FunctionResolved.AddRange(funcCall);
        }
    }

    // Different from AstFunctionCall because this keeps a link back to the caller.
    public class AstFunctionCallTemp : IAstStatement
    {

        public List<AstArgument> Arguments { get; set; }
        public string Description { get; set; }

        public AstFunction Function { get; set; } // Function after matching. Note: may still be multiple possible paths until inlining or execution because of how preconditions work.


        // Store what conditions were used when this function was called.
        // If we ever try to call the function again with different conditions, we'll duplicate it for easier use downstream.
        // This helps with pre/post propagation, so we know what possible paths we can take out of this func deterministically rahter than saying
        // "this func has n paths out" we can say with certainty exactly which path it'll follow because of the conditions when called.
        public Dictionary<string, string> ConditionsWhenCalled { get; internal set; } = null; // Must have null default to indicate "not set yet" (ie. the func hasn't been called)
    }
}
