﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    public interface IAstMethod
    {
        string Name { get; set; }
        string Description { get; set; }
        List<IAstStatement> Statements { get; }
    }
}