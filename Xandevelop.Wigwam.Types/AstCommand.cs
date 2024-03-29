﻿using System.Collections.Generic;

namespace Xandevelop.Wigwam.Ast
{ 
    public class AstCommand : IAstStatement
    {
        public string Command { get; set; }
        public string Target { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }

        public List<AstArgument> Arguments { get; set; }
        public AstCommandDefinition CommandDefinition { get; set; }

        public override IAstStatement CopyWithNewConditions(Dictionary<string, string> conditions)
        {
            return new AstCommand { Command = this.Command, Target = this.Target, Value = this.Value, Description = this.Description,
                
                SourceFile = this.SourceFile,
                SourceLine = this.SourceLine,
                SourceLineNumber = this.SourceLineNumber
            };
        }
    }
}
