using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                SourceFile = line.SourceFile,
                SourceLine = line.SourceLine,
                SourceLineNumber = line.SourceLineNumber,

                Name = line.Blocks.First(),
                Description = line.CommentBlock,
                Statements = new List<IAstStatement>()
            };
#warning todo read rest of signature
            ast.AddFunction(astFunction);
        }
    }
}
