using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{ 
    public class AstFunction : AstBase, IAstMethod
    {
        public string Name { get; set; }
        public List<AstFormalParameter> FormalParameters { get; set; }
        public List<AstPreCondition> PreConditions { get; set; }
        public List<AstPostCondition> PostConditions { get; set; }

        public List<IAstStatement> Statements { get; set; }
        public string Description { get; set; }
        
    }
}
