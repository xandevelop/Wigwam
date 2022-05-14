﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    public class AstTest : IAstMethod
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public List<IAstStatement> Statements { get; set; }
    }
}