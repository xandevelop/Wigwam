using System.Linq;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    class PostConditionParser : ILineParser
    {
        public string Name => "PostCondition";

        public int OrderNumber => 1;

        public bool IsMatch(Line line)
        {
            return line.Command == "post" || line.Command == "postcondition" || line.Command == "post condition";
        }

        ArgumentScanner ArgumentScanner { get; } = new ArgumentScanner();

        public void Parse(AstBuilder ast, Line line)
        {
            #region Pre
            if (!ast.HasCurrentMethod || !ast.CurrentMethodIsFunction)
            {
                ast.AddError(StandardMessages.PostConditionOutsideOfFunction(line));
                return;
            }
            #endregion

            var args = ArgumentScanner.ScanLineArguments(line,
                new ExpectedArgument { Name = "variable" },
                new ExpectedArgument { Name = "value" }
            );

            if (args.ArgumentErrors?.Any()??false)
            {
                foreach (var x in args.ArgumentErrors)
                {
                    ast.AddError(x.Name);
                }
                return;
            }

            ArgumentData variableArgument = args["variable"];
            ArgumentData valueArgument = args["value"];
            
            Ast.AstPostCondition post = new Ast.AstPostCondition
            {
                SourceFile = line.SourceFile,
                SourceLine = line.SourceLine,
                SourceLineNumber = line.SourceLineNumber,
                Variable = variableArgument.ValueString,
                Value = valueArgument.ValueString,
                Description = line.CommentBlock
            };

            ast.AddPostCondition(post);
        }
    }
}
