using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    public class CommandDefinitionParser : ILineParser
    {
        public string Name => "Command Definition";

        public int OrderNumber => 1;

        public bool IsMatch(Line line)
        {
            return line.Command == "cmd" || line.Command == "command";
        }

        public void Parse(AstBuilder ast, Line line)
        {
            AstCommandDefinition astCmd = new AstCommandDefinition
            {
                SourceFile = line.SourceFile,
                SourceLine = line.SourceLine,
                SourceLineNumber = line.SourceLineNumber,

                Name = line.Blocks.First(),
                Description = line.CommentBlock,
            };

            astCmd.FormalParameters = new FormalParameterScanner().Scan(line);

            ast.AddCommandDefinition(astCmd);
        }
    }
}
