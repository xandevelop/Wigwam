using System;
using System.Collections.Generic;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    // Function calls and commands
    public class InstructionParser : ILineParser
    {
        public string Name => "Instruction Parser";

        public int OrderNumber => 9999;

        public bool IsMatch(Line line)
        {
            return true; // Catch all.  Note: high order number means we shouldn't catch any more specific things.
        }

        ArgumentScanner argScanner = new ArgumentScanner();
        public void Parse(AstBuilder ast, Line line)
        {
            if (ast.HasCurrentMethod)
            {
                AstUnresolvedCall astFunctionCall = new AstUnresolvedCall(ast.CurrentMethod, line);
                ast.AddStatementToCurrentMethod(astFunctionCall);
            }
            else
            {
                ast.AddError(StandardMessages.FunctionCallOutsideOfMethod(line));
            }
        }
    }

    // Note: First pass generates these, second pass removes them and replaces wiht AstFunctionCalls.
    public class AstUnresolvedCall : IAstStatement
    {
        public override string ToDebugString()
        {
            throw new NotImplementedException();
        }
        public IAstMethod Method { get; set; }

        public AstUnresolvedCall(IAstMethod method, Line sourceLineForPatchup)
        {
            this.Method = method;
            this.SourceCode = sourceLineForPatchup;
        }
        public AstUnresolvedCall(IAstMethod method, Line sourceLineForPatchup, Dictionary<string, string> conditionsWhenCalled)
        {
            this.Method = method;
            this.SourceCode = sourceLineForPatchup;
            this.ConditionsWhenCalled = conditionsWhenCalled;
            
        }

        public AstFunctionCall ToFunctionCall()
            => new AstFunctionCall
            {
                Arguments = this.ArgumentsWithContext,
                Description = this.SourceLineForPatchup.CommentBlock,
                Function = this.Function,
                SourceCode = this.SourceCode
            };
        public AstCommand ToCommand() =>
            new AstCommand
            {
                Arguments = this.ArgumentsWithContext,
                Description = this.SourceLineForPatchup.CommentBlock,
                Command = this.Command.Command,
                SourceCode = this.SourceCode
            };

        
        // Arguments BEFORE we match with a function signature, so these may not be valid, they're just what the scanner/parser read on first pass.
        [Obsolete("We capture the line so this shouldn't be needed?", true)]
        public List<AstArgument> ContextFreeArguments => argScanner.ScanLineArgumentsWithoutContext(SourceLineForPatchup); //{ get; internal set; }
        ArgumentScanner argScanner = new ArgumentScanner();

        [Obsolete("Use SourceCode instead")]
        public Line SourceLineForPatchup => SourceCode;

        [Obsolete("We capture the line so this shouldn't be needed?")]
        public string Description => SourceLineForPatchup.CommentBlock; //{ get; set; }


        // todo trace all uses and check if conditions needs to be mandatory
        public void SetFunctionResolved(AstFunction funcCall, Dictionary<string, string> conditions)
        {
            //if (Function != null) throw new Exception("Cannot set function multiple times");
            Function = funcCall;
            ConditionsWhenCalled = CopyConditionsWhenCalled(conditions);

            ArgumentScanner scanner = new ArgumentScanner();
            var argsWithContext = scanner.ScanLineArguments(SourceCode, funcCall.FormalParameters);
            ArgumentsWithContext = argsWithContext.ArgumentData;


        }

#warning todo replace with AstArgumentCollection
        public List<AstArgument> ArgumentsWithContext { get; set; }

        //internal void SetFunctionNotResolved(Dictionary<string, string> conditions)
        //{
        //    Function = null;
        //    ConditionsWhenCalled = CopyConditionsWhenCalled(conditions);
        //}

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
        public bool IsFunction => Function != null;
        public bool IsCommand => Command != null;

        public override IAstStatement CopyWithNewConditions(Dictionary<string, string> conditions)
        {
            var condCopy = CopyConditionsWhenCalled(conditions);
#warning possibly bad use of this.Method
            return new AstUnresolvedCall(this.Method, this.SourceCode, condCopy);

            //return new AstUnresolvedCall
            //{
            //    ConditionsWhenCalled = CopyConditionsWhenCalled(conditions),
            //    //ContextFreeArguments = this.ContextFreeArguments,
            //    //Description = this.Description,
            //    Function = null,
            //    SourceFile = this.SourceFile,
            //    SourceLine = this.SourceLine,
            //    SourceLineForPatchup = this.SourceLineForPatchup,
            //    SourceLineNumber = this.SourceLineNumber
            //};
        }

    }

    
}
