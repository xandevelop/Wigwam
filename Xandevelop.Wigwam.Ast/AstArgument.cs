using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    [DebuggerDisplay("Argument | {Name} | {Value}")]
    public class AstArgument
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
