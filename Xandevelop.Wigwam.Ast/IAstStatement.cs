using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
#warning todo rename - abstract class not interface so method doesn't need a default impl everywhere
    public abstract class IAstStatement : AstBase
    {
        // When we clone a method, we also clone the statements in it, but they then might get modified so we need a value copy, not a reference copy.
        public abstract IAstStatement CopyWithNewConditions(Dictionary<string, string> conditions);
    }
}
