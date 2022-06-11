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
        // What method is this statement in?
        public IAstMethod Method { get; set; }
    }
}
