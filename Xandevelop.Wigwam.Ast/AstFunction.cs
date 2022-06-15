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
        public List<AstFormalParameter> FormalParameters { get; set; } = new List<AstFormalParameter>();
        public List<AstPreCondition> PreConditions { get; set; } = new List<AstPreCondition>();
        public List<AstPostCondition> PostConditions { get; set; } = new List<AstPostCondition>();

        public List<IAstStatement> Statements { get; set; } = new List<IAstStatement>();
        public string Description { get; set; }
        
        /// <summary>
        /// Indicates the function was not literally in the source code, but rather was automatically generated to fulfil preconditions of downstream calls.
        /// </summary>
        public AstFunction OverloadGeneratedFrom { get; set; }
    }
}
