using System.Diagnostics;

namespace Xandevelop.Wigwam.Ast
{
    [DebuggerDisplay("Formal Parameter | {Name} | {DataType} | {DefaultValue}")]
    public class AstFormalParameter : AstBase
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string DefaultValue { get; set; }
    }
}
