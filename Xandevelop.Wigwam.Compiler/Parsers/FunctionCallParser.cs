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


        // todo trace all uses and check if conditions needs to be mandatory
        public void SetFunctionResolved(AstFunction funcCall, Dictionary<string, string> conditions)
        {
            //if (Function != null) throw new Exception("Cannot set function multiple times");
            Function = funcCall;
            ConditionsWhenCalled = CopyConditionsWhenCalled(conditions);
        }

        internal void SetFunctionNotResolved(Dictionary<string, string> conditions)
        {
            Function = null;
            ConditionsWhenCalled = CopyConditionsWhenCalled(conditions);
        }

        // Note: Think this is needed, but location is terrible and should move to a util class later
        public static Dictionary<string, string> CopyConditionsWhenCalled(Dictionary<string, string> conditions)
        {
            Dictionary<string, string> copy = new Dictionary<string, string>();
            {
                foreach (var kvp in conditions)
                {
                    copy.Add(kvp.Key, kvp.Value);
                }
            }
            return copy;
        }



        public AstFunction Function { get; private set; } // Function after matching
        public Dictionary<string, string> ConditionsWhenCalled { get; internal set; } = null;

        public AstCommand Command { get; set; }// Command after matching (i.e. this isn't a full function, only a command, (sort of like a header)

        public override IAstStatement CopyWithNewConditions(Dictionary<string, string> conditions)
        {
            return new AstFunctionCallNoContext
            {
                FunctionName = this.FunctionName,
                ConditionsWhenCalled = CopyConditionsWhenCalled(conditions),
                ContextFreeArguments = this.ContextFreeArguments,
                Description = this.Description,
                Function = null,
                SourceFile = this.SourceFile,
                SourceLine = this.SourceLine,
                SourceLineForPatchup = this.SourceLineForPatchup,
                SourceLineNumber = this.SourceLineNumber
            };
        }

    }

    
}
