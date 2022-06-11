using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    public class PreConditionParser : ILineParser
    {
        public string Name => "PreCondition";

        public int OrderNumber => 1;

        public bool IsMatch(Line line)
        {
            return line.Command == "pre" || line.Command == "precondition";
        }

        ArgumentScanner ArgumentScanner { get; } = new ArgumentScanner();

        // pre | var=x | value=something | comparison=equals (could be not equal, contains, not contains)
        public void Parse(AstBuilder ast, Line line)
        {
            #region Pre
            if (!ast.HasCurrentMethod || !ast.CurrentMethodIsFunction)
            {
                ast.AddError(StandardMessages.PreConditionOutsideOfFunction(line));
                return;
            }
            #endregion

            var args = ArgumentScanner.ScanLineArguments(line,
                new ExpectedArgument { Name = "variable" },
                new ExpectedArgument { Name = "value" },
                new ExpectedArgument { Name = "comparison", DefaultValue = "equals" }
            );

            // todo tidy up code like this with better abstractions in error class
            if(args.ArgumentErrors?.Any()??false)
            {
                foreach(var x in args.ArgumentErrors)
                {
                    ast.AddError(x.Name);
                }
                return;
            }

            ArgumentData variableArgument = args["variable"];
            ArgumentData valueArgument = args["value"];
            ArgumentData comparisonArgument = args["comparison"];

            PreConditionComparisonType comparison = PreConditionComparisonType.Equals;
            #region Map Comparison strings to comparison
            switch(comparisonArgument.ValueString.ToLower())
            {
                case "equals":
                case "=":
                case "==":
                case "equal to":
                case "is":
                case "is equal to":
                    comparison = PreConditionComparisonType.Equals; break;
                case "not equals":
                case "!=":
                case "<>":
                case "not equal to":
                case "is not":
                case "is not equal to":
                    comparison = PreConditionComparisonType.NotEquals; break;
                case "contains":
                case "contain":
                case "substring":
                case "in":
                case "like":
                case "is like":
                case "%": // discouraged - not readable
                case "~": // discouraged - not readable
                    comparison = PreConditionComparisonType.Contains; break;
                case "not contains":
                case "not contain":
                case "does not contain":
                case "not substring":
                case "not in":
                case "not like":
                case "is not like":
                case "!%": // discouraged - not readable
                case "!~": // discouraged - not readable
                    comparison = PreConditionComparisonType.NotContains; break; 
                case "regex":
                    comparison = PreConditionComparisonType.Regex; break;
                default:
                    ast.AddError(StandardMessages.UnrecognisedComparison(line, comparisonArgument.ValueString));
                    return;
            }
            #endregion

            Ast.AstPreCondition pre = new Ast.AstPreCondition
            {
                SourceFile = line.SourceFile,
                SourceLine = line.SourceLine,
                SourceLineNumber = line.SourceLineNumber,
                Variable = variableArgument.ValueString,
                Value = valueArgument.ValueString,
                Comparison = comparison,
                Description = line.CommentBlock
            };

            ast.AddPreCondition(pre);
        }
    }
}
