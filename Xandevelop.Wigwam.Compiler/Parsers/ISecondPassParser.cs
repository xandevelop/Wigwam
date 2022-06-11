using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    public interface ISecondPassParser
    {
        string Name { get; }
        int OrderNumber { get; }  // Low = processed sooner, High = processed later (will definitley happen, unlike first pass parsers)
        void Parse(AstBuilder ast);
    }
}
