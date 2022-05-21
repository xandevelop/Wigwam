using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    [DebuggerDisplay("Formal Parameter | {Name} | {DataType} | {DefaultValue}")]
    public class AstFormalParameter
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string DefaultValue { get; set; }
    }
}
