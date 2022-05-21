using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                SourceFile = line.SourceFile,
                SourceLine = line.SourceLine,
                SourceLineNumber = line.SourceLineNumber,
                
                Name = line.Blocks.First(),
                Description = line.CommentBlock,
                Statements = new List<IAstStatement>()
            };
            ast.AddTest(astTest);
        }
    }
}
