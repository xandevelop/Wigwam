using System.Diagnostics;

namespace Xandevelop.Wigwam.Ast
{
    [DebuggerDisplay("Argument | {Name} | {Value}")]
    public class AstArgument : AstBase
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
