using System.Collections.Generic;
using System.Linq;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    public class TestDeclarationParser : ILineParser
    {
        public string Name => "Test Declaration";

        public int OrderNumber => 1;

        public bool IsMatch(Line line)
        {
            return line.Command == "test";
        }

        public void Parse(AstBuilder ast, Line line)
        {
            AstTest astTest = new AstTest
            {
                SourceCode = line,
                                
                Name = line.Blocks.First(),
                Description = line.CommentBlock,
                Statements = new List<IAstStatement>()
            };
            ast.AddTest(astTest);
        }
    }
}
