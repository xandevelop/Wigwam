using System.Collections.Generic;
using System.Linq;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Extensions;
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
            var args = Scanner.ScanLineArguments(line, new System.Collections.Generic.List<AstFormalParameter> {
                    new AstFormalParameter { Name = "name" }});

            AstCommandDefinition astCmd = new AstCommandDefinition
            {
                SourceCode = line,

                Name = args["name"].Value,
                Description = line.CommentBlock,
            };

            astCmd.FormalParameters = new FormalParameterScanner().Scan(line);

            ast.AddCommandDefinition(astCmd);
        }
        ArgumentScanner Scanner { get; } = new ArgumentScanner {  AllowExtraArguments = true };




    }
}
