using System.Collections.Generic;
using System.Linq;

namespace Xandevelop.Wigwam.Ast
{ 
    public class AstCommand : IAstStatement
    {
        public string Command { get; set; }
        public string Target => Arguments.FirstOrDefault(x => x.Name.ToLower().Trim() == "target")?.Value;
        public string Value => Arguments.FirstOrDefault(x => x.Name.ToLower().Trim() == "value")?.Value;
        public string Description { get; set; }

        public List<AstArgument> Arguments { get; set; } = new List<AstArgument>();
        public AstCommandDefinition CommandDefinition { get; set; }

        public override IAstStatement CopyWithNewConditions(Dictionary<string, string> conditions)
        {
            throw new System.Exception("Dont think this should be used?");
            return new AstCommand { Command = this.Command, Arguments = this.Arguments, Description = this.Description,
                SourceCode = this.SourceCode
            };
        }

        public AstArgument TargetArgument => Arguments.FirstOrDefault(x => x.Name.ToLower().Trim() == "target");
        public AstArgument ValueArgument => Arguments.FirstOrDefault(x => x.Name.ToLower().Trim() == "value");
    }
}
