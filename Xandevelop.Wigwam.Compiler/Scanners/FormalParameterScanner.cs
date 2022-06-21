using System;
using System.Collections.Generic;
using System.Linq;
using Xandevelop.Wigwam.Ast;

namespace Xandevelop.Wigwam.Compiler.Scanners
{
    public class FormalParameterScanner
    {
        public List<AstFormalParameter> Scan(Line line)
        {
            List<AstFormalParameter> result = new List<AstFormalParameter>();
            foreach (var b in line.Blocks.Skip(1))
            {
                result.Add(ScanSingleParameter(b));
            }
            return result;
        }

        public AstFormalParameter ScanSingleParameter(string parameterString)
        {
            var partsNameAndValue = SplitNameValue(parameterString);
            return new AstFormalParameter { Name = partsNameAndValue.Name, DefaultValue = partsNameAndValue.Value };
        }

        private (string Name, string Value) SplitNameValue(string s)
        {
            var split2 = Split2(s);
            if (split2.Part1 == null)
            {
                // No name was specified
                return (split2.Part0, null);
            }
            else
            {
                return (split2.Part0, split2.Part1);
            }
        }
        private (string Part0, string Part1) Split2(string s)
        {
            var parts = s.SplitWithEscape(new[] { ':', '=' }, 2).ToList();

            switch (parts.Count)
            {
                case 0: return (null, null);
                case 1: return (parts[0], null);
                case 2: return (parts[0], parts[1]);
                default: throw new Exception("Can't happen");
            }
        }
    }
}
