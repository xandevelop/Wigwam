using System;
using System.Collections.Generic;
using System.Linq;

namespace Xandevelop.Wigwam.Ast
{ 
    public class AstCommand : IAstStatement
    {
        public string Command { get; set; }
        public string Target => TargetArgument?.Value;
        public string Value => ValueArgument?.Value;

        public override string ToDebugString()
        {
            return $"Command: Command={Command} | {Arguments.ToDebugString()} | Description={Description}";
        }

        public string Description { get; set; }

        public AstArgumentCollection Arguments { get; set; } = new AstArgumentCollection();
        public AstCommandDefinition CommandDefinition { get; set; }

        public override IAstStatement CopyWithNewConditions(Dictionary<string, string> conditions)
        {
            //throw new System.Exception("Dont think this should be used?");
#warning seems this IS used but still probably shouldn't be...
            return new AstCommand { Command = this.Command, Arguments = this.Arguments, Description = this.Description,
                SourceCode = this.SourceCode
            };
        }

        public AstArgument TargetArgument => Arguments["target"];
        public AstArgument ValueArgument => Arguments["value"];
    }

    
}
