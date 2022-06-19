using System.Collections.Generic;

namespace Xandevelop.Wigwam.Ast
{ 
    public class AstTest : AstBase, IAstMethod
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public List<IAstStatement> Statements { get; set; }
        
    }
}
