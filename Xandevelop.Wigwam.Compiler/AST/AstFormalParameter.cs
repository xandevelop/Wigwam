using System;
using System.Diagnostics;

namespace Xandevelop.Wigwam.Ast
{
    [DebuggerDisplay("Formal Parameter | {Name} | {DataType} | {DefaultValue}")]
    public class AstFormalParameter : AstBase
    {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string DefaultValue { get; set; }

        // Do we return a value?  Note: not sure on this impl - could be that we should use DefaultValue instead of name here.
        public bool IsOutParam => Name.StartsWith("${") && Name.EndsWith("}");

        public bool HasDefault => !String.IsNullOrEmpty(DefaultValue);

        public bool IsRequired => DefaultValue == null && !AllowNoValue;


        /// <summary>
        /// Allow the arg to not be specified, even when there's no default
        /// </summary>
        internal bool AllowNoValue { get; set; }

        internal AstArgument GenerateDefaultArg()
        {
            return new AstArgument { Name = Name, Value = DefaultValue };
        }

        internal bool Matched { get; set; }
    }
}
