using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Compiler
{
    public enum ParamType
    {
        None,
        Selector,
        Url,
        String,
        Variable
    }

    public class BuiltInCommandSignature
    {
        public string CommandName { get; set; }
        public ParamType TargetParameterType { get; set; } // "target"
        public ParamType ValueParameterType { get; set; } // "value"

        public static implicit operator BuiltInCommandSignature(string sig)
        {
            return (sig, ParamType.None, ParamType.None);
        }

        public static implicit operator BuiltInCommandSignature((string, ParamType) sig)
        {
            return (sig.Item1, sig.Item2, ParamType.None);
        }

        public static implicit operator BuiltInCommandSignature((string, ParamType, ParamType) sig)
        {
            BuiltInCommandSignature result = new BuiltInCommandSignature();
            result.CommandName = sig.Item1;
            result.TargetParameterType = sig.Item2;
            result.ValueParameterType = sig.Item3;
            return result;
        }
    }
}
