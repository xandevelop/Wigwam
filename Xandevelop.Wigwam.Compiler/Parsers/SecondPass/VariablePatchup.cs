using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Compiler.Parsers.SecondPass
{
    class VariablePatchup
    {
        public void Parse(AstBuilder ast)
        {
            ProgramVisitor programVisitor = new ProgramVisitor();
            
            var vars = new VariableTable();
            var curArgs = new VariableTable();

            programVisitor.Command += (object sender, Ast.AstCommand e) => VisitCommand(e, vars, curArgs);
            programVisitor.StartFunctionCall += (object s, Ast.AstFunctionCall f) => { curArgs = new VariableTable();
                foreach (var a in f.Arguments) curArgs.Add(a.Name, a.Value);
            };

            programVisitor.VisitDepthFirst(ast.Program);
        }

        private void VisitCommand(Ast.AstCommand e, VariableTable vars, VariableTable curArgs)
        {
            if(e.Arguments.Count == 0)
            {
                e.Arguments.Add(new Ast.AstArgument { Name = "target", Value = e.Target });
                e.Arguments.Add(new Ast.AstArgument { Name = "value", Value = e.Value });
            }
            foreach(var commandArg in e.Arguments.Collection)
            {
                foreach(var p in commandArg.Parts)
                {
                    if(p.IsVariable)
                    {
                        if(vars.ContainsNonEmpty(p.Value))
                        {
                            var val = vars.GetValue(p.Value);
                            if(val.StartsWith("${"))
                            {
                                // don't replace - reference
                            }
                            else
                            {
                                p.Value = val;
                            }
                        }
                        if (curArgs.ContainsNonEmpty(p.Value))
                        {
                            var val = curArgs.GetValue(p.Value);
                            if (val.StartsWith("${"))
                            {
                                // don't replace - reference
                            }
                            else
                            {
                                p.Value = val;
                            }
                        }
                    }
                }
            }

            
        }
    }

    class VariableTable
    {
        private Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        public bool Contains(string key)
        {
            return Values.Keys.Contains(key);
        }
        public string GetValue(string key)
        {
            if (Values.ContainsKey(key)) return Values[key];
            else return null;
        }

        public bool ContainsNonEmpty(string key)
        {
            var value = GetValue(key);
            if (String.IsNullOrEmpty(value)) return false;
            return true;
        }

        internal void Add(string name, string value)
        {
            if (name == null) return;
            Values.Add(name, value);
        }
    }
}
