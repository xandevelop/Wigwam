using System.Collections.Generic;
using System.Linq;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    public class FunctionDeclarationParser : ILineParser
    {
        public string Name => "Function Declaration";

        public int OrderNumber => 1;

        public bool IsMatch(Line line)
        {
            return line.Command == "func" || line.Command == "function";
        }

        public void Parse(AstBuilder ast, Line line)
        {
            AstFunction astFunction = new AstFunction
            {
                SourceCode = line,
                
                Name = line.Blocks.First(),
                Description = line.CommentBlock,
                Statements = new List<IAstStatement>()
            };

            astFunction.FormalParameters = new FormalParameterScanner().Scan(line);
            
            ast.AddFunction(astFunction);
        }
    }
}
