using System.Collections.Generic;

namespace Xandevelop.Wigwam.Ast
{
    public interface IAstMethod
    {
        string Name { get; set; }
        string Description { get; set; }
        List<IAstStatement> Statements { get; }
    }
}
