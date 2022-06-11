using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{


    [DebuggerDisplay("Program ( {Tests.Count} test(s), {Functions.Count} function(s), {Controls.Count} control(s) )")]
    public class AstProgram 
    {
        public string SourceFile { get; set; }

        public List<AstTest> Tests { get; set; } = new List<AstTest>();
        public List<AstFunction> Functions { get; set; } = new List<AstFunction>();

        public List<AstControlDeclaration> Controls { get; set; } = new List<AstControlDeclaration>();
    }


    
    

   
}
