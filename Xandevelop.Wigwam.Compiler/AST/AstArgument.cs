using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Xandevelop.Wigwam.Ast
{
    [DebuggerDisplay("Argument | {Name} | {Value}")]
    public class AstArgument : AstBase
    {
        public override string ToDebugString()
        {
            throw new NotImplementedException();
        }

        public string Name { get; set; }

        private string _value;
        public string Value { get { return _value; } set { _value = value; SetParts(value); } }

        [Obsolete("Use Value instead")]
        public string ValueString { get { return Value; }  set { Value = value; } }


        public List<AstArgumentPart> Parts { get; private set; }
        
        private void SetParts(string value)
        {
            Parts = new List<AstArgumentPart>();

            if (value == null) return;

            List<string> resultStrings = new List<string>();

            StringBuilder curStr = new StringBuilder();
            int ix = 0;
            while(true)
            {
                char? curChar = SafeGet(ix, value); 

                if (IsVariableStart(value, ix))
                {
                    // Maybe in a variable...
                    int? endVariable = SafeIndexOf(value, '}', ix + 1) + 1;
                    if(endVariable == null)
                    {
                        // No more variables
                        curStr.Append(value.Substring(ix + 1, value.Length - (ix+1)));
                        break;
                    }
                    else
                    {
                        if(curStr.Length > 0) resultStrings.Add(curStr.ToString());

                        resultStrings.Add(value.Substring(ix, endVariable.Value-ix));
                        ix = endVariable.Value;
                        curStr.Clear();
                    }
                }
                else
                {
                    curStr.Append(curChar);
                    ix++;
                }

                if (ix > value.Length) break;
            }
            if (curStr.Length > 0) resultStrings.Add(curStr.ToString());

            foreach(var p in resultStrings)
            {
                this.Parts.Add(new AstArgumentPart { Value = p });
            }
        }

        private bool IsVariableStart(string value, int index)
        {
            char? prevChar = SafeGet(index - 1, value);
            char? curChar = SafeGet(index, value);
            char? nextChar = SafeGet(index + 1, value);
            char? nextNextChar = SafeGet(index + 2, value); // Don't allow ${} as a variable
            return prevChar != '\\' && curChar == '$' && nextChar == '{' && nextNextChar != '}';
        }

        public string FillVariables(Dictionary<string, string> currentScopeVariables)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var p in this.Parts)
            {
                if(p.IsVariable)
                {
                    var trimmedP = p.Value.TrimStart('$', '{').TrimEnd('}');
                    string replace = null;
                    if(currentScopeVariables.ContainsKey(trimmedP))
                    {
                        replace = currentScopeVariables[trimmedP];
                    }
                    else if(currentScopeVariables.ContainsKey(p.Value))
                    {
                        replace = currentScopeVariables[p.Value];
                    }
                    if(replace != null)
                    {
                        sb.Append(replace);
                    }
                    else
                    {
                        sb.Append(p.Value);
                    }
                }
                else
                {
                    sb.Append(p.Value);
                }
            }
            return sb.ToString();
        }

        private char? SafeGet(int ix, string value)
        {
            if (ix >= value.Length || ix < 0) return null;
            else return value[ix];
        }
        private int? SafeIndexOf(string value, char c, int startIndex)
        {
            if (startIndex > value.Length) return null;
            int result = value.IndexOf(c, startIndex);
            if (result == -1) return null;
            return result;
        }

    }

    [DebuggerDisplay("{Value}")]
    public class AstArgumentPart
    {
        public bool IsVariable => Value.StartsWith("${") && Value.EndsWith("}") && !Value.StartsWith("\\${");
        public string Value { get; set; }
    }
}
