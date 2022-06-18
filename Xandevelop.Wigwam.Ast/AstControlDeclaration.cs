using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    public enum SelectorStrategy
    {
        Id,
        XPath,
        Css
    }
    
    public class AstControlDeclaration : AstBase
    {
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string Selector { get; set; }
        public SelectorStrategy Strategy { get; set; }
        public string Description { get; set; }
    }

    
}
