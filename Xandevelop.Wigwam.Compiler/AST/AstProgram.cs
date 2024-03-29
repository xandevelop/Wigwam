﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xandevelop.Wigwam.Ast
{


    [DebuggerDisplay("Program ( {Tests.Count} test(s), {Functions.Count} function(s), {Controls.Count} control(s) )")]
    public class AstProgram 
    {
        public string SourceFile { get; set; }

        public List<AstTest> Tests { get; set; } = new List<AstTest>();
        public List<AstFunction> Functions { get; set; } = new List<AstFunction>();

        public List<AstControlDeclaration> Controls { get; set; } = new List<AstControlDeclaration>();

        // Extra "plugin" commands (easier than writing a real plugin)
        public List<AstCommandDefinition> CommandDefinitions { get; set; } = new List<AstCommandDefinition>();

        internal AstCommandDefinition FindCommandDefinition(string name)
        {
            return CommandDefinitions.FirstOrDefault(x => x.Name.ToLower().Trim() == name.ToLower().Trim());
        }
    }


    
    

   
}
