using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    [DebuggerDisplay("Test | {Name}")]
    public class AstTest : AstBase, IAstMethod
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public List<IAstStatement> Statements { get; set; }
        
    }
}
