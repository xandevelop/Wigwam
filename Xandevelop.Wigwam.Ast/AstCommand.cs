using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{ 
    public class AstCommand : IAstStatement
    {
        public string Command { get; set; }
        public string Target { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }

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
