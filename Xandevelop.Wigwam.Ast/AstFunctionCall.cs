using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{ 
    public class AstFunctionCall : IAstStatement
    {

        public List<AstArgument> Arguments { get; set; }
        public string Description { get; set; }

        public AstFunction Function { get; set; } // Function after matching. Note: may still be multiple possible paths until inlining or execution because of how preconditions work.


        public override IAstStatement CopyWithNewConditions(Dictionary<string, string> conditions)
        {
            // todo - review whether func also needs copy?
            return new AstFunctionCall { Arguments = this.Arguments, Description = this.Description,
                Method = this.Method,
                SourceFile = this.SourceFile,
                SourceLine = this.SourceLine,
                SourceLineNumber = this.SourceLineNumber
            };
        }
    }
}
