﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Ast
{
    public enum PreConditionComparisonType
    {
        Equals,
        NotEquals,
        Contains,
        NotContains,
        Regex
    }
     
    public class AstPreCondition : AstBase {
        public string Variable { get; set; }
        public string Value { get; set; }
        public PreConditionComparisonType Comparison { get; set; }
        public string Description { get; set; }

    }
}
