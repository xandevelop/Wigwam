using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    public interface ILineParser
    {
        string Name { get; }
        int OrderNumber { get; } // Low = processed sooner, High = processed later (may not happen at all)
        bool IsMatch(Line line);
        void Parse(AstBuilder ast, Line line);
    }
}
