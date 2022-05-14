using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    // Note: First pass generates these, second pass removes them and replaces wiht AstFunctionCalls.
    public class AstFunctionCallNoContext : IAstStatement
    {
        public string FunctionName { get; set; } // For signature matching - note: we'll discard this in second pass in favor of a pointer to the actual matched function

        // Arguments BEFORE we match with a function signature, so these may not be valid, they're just what the scanner/parser read on first pass.
        public List<string> ContextFreeArguments { get; internal set; }

        public string Description { get; set; }


    }
}
