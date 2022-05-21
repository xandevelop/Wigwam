﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    public abstract class AstBase
    {
        public string SourceFile { get; set; }
        public string SourceLine { get; set; }
        public int SourceLineNumber { get; set; }
    }
}
