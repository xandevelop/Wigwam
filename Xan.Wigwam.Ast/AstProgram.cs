using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xan.Wigwam.Ast
{
    
    

    public class AstProgram
    {
        public List<AstTest> Tests { get; set; } = new List<AstTest>();
        public List<AstFunction> Functions { get; set; } = new List<AstFunction>();
    }


    
    

   
}
