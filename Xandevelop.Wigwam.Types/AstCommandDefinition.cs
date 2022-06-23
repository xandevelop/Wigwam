using System.Collections.Generic;

namespace Xandevelop.Wigwam.Ast
{
    public class AstCommandDefinition : AstBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AstFormalParameter> FormalParameters { get; set; } = new List<AstFormalParameter>();
    }
}
