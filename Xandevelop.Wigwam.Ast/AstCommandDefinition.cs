using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    public class AstCommandDefinition : AstBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AstFormalParameter> FormalParameters { get; set; } = new List<AstFormalParameter>();
    }
}
