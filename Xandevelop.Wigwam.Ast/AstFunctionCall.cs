using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    [DebuggerDisplay("Function Call | {Function.Name}")]
    public class AstFunctionCall : IAstStatement
    {

        public List<AstArgument> Arguments { get; set; }
        public string Description { get; set; }

        public AstFunction Function { get; set; } // Function after matching.
    }
}
