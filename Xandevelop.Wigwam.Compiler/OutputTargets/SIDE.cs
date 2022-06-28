using System;
using System.Collections.Generic;
using System.Text;
using Xandevelop.Wigwam.Compiler;
using Xandevelop.Wigwam.Ast;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;


namespace Xandevelop.Wigwam.Compiler.OutputTargets
{
    public class SIDE
    {
        public SideTest ToSide(Ast.AstProgram program, string name = "testsuite", string url = "http://www.example.com")
        {
            ProgramVisitor programVisitor = new ProgramVisitor();


            //programVisitor.CommandDefinition += (object sender, AstCommandDefinition d) => { Console.WriteLine(d.Name); };

            CurrentScope scope = new CurrentScope(name, url);

            programVisitor.Control += (object sender, AstControlDeclaration x) => { Control(scope, x); };

            programVisitor.StartFunction += (object sender, AstFunction x) => { StartFunction(scope, x); };
            programVisitor.EndFunction += (object sender, AstFunction x) => { EndFunction(scope, x); };
            programVisitor.StartFunctionCall += (object sender, AstFunctionCall x) => { StartFunctionCall(scope, x); };
            programVisitor.EndFunctionCall += (object sender, AstFunctionCall x) => { EndFunctionCall(scope, x); };

            programVisitor.StartTest += (object sender, AstTest x) => { StartTest(scope, x); };
            programVisitor.EndTest += (object sender, AstTest x) => { EndTest(scope, x); };

            programVisitor.Command += (object s, AstCommand x) => { Command(scope, x); };


            programVisitor.VisitDepthFirst(program);


            return scope.TestFile;
        }

        private void StartTest(CurrentScope s, AstTest t)
        {
            var testId = Guid.NewGuid();
            s.SetCurrentTest(new Test { Name = t.Name, Id = testId, Commands = new List<Command>() });
            s.TestFile.Suites.First().Tests.Add(testId);
        }
        private void EndTest(CurrentScope s, AstTest t)
        {
            // No further processing needed on tests
        }

        private void Command(CurrentScope s, AstCommand cmd)
        {
            var newcmd = new OutputTargets.Command
            {
                CommandCommand = cmd.Command,
                Id = Guid.NewGuid(),
                Target = cmd.Target,
                Value = cmd.Value
            };

            newcmd.Target = cmd.TargetArgument?.FillVariables(s.CurrentScopeVariables);
            newcmd.Value = cmd.ValueArgument?.FillVariables(s.CurrentScopeVariables);

            s.CurrentTest.Commands.Add(newcmd);

        }

        private void Control(CurrentScope s, AstControlDeclaration d)
        {
            s.Controls.Add(d);
        }

        private void StartFunctionCall(CurrentScope s, AstFunctionCall fc)
        {
            // Reading a line just before we jump into function...
            // Increase func scope, pass args




            s.IncreaseScope(fc);
        }
        private void StartFunction(CurrentScope s, AstFunction f)
        {

        }
        private void EndFunction(CurrentScope s, AstFunction f)
        {

        }
        private void EndFunctionCall(CurrentScope s, AstFunctionCall fc)
        {
            // Work out return values from function we just called, and pop the scope

            s.DecreaseScope();
        }
    }

    class CurrentScope
    {
        public CurrentScope(string name, string url)
        {
            var suite = new Suite { Id = Guid.NewGuid(), Name = "DefaultSuite", Tests = new List<Guid>() };
            TestFile = new SideTest { Id = Guid.NewGuid(), Name = name, Url = url, Urls = new List<string> { url }, Suites = new List<Suite> { suite }, Version = "2.0" };
        }


        private Stack<Dictionary<string, string>> ScopedVariables { get; set; } = new Stack<Dictionary<string, string>>();
        public Dictionary<string, string> CurrentScopeVariables { get; set; }

        // Indicates that we're starting a new method call with its own variable scope.
        public void IncreaseScope()
        {
            CurrentScopeVariables = new Dictionary<string, string>();
            ScopedVariables.Push(CurrentScopeVariables);
        }
        // We're exiting a method call and getting rid of it's scoped variables
        public void DecreaseScope()
        {
            ScopedVariables.Pop();
            foreach (var trackedVar in TrackedArgs)
            {
                SetCurrentScopeVariable(trackedVar.Name, trackedVar.Value);
            }
        }

        public List<AstControlDeclaration> Controls { get; set; } = new List<AstControlDeclaration>();

        public SideTest TestFile { get; set; }

        public Test CurrentTest { get; set; }

        public void SetCurrentTest(Test t)
        {
            ScopedVariables = new Stack<Dictionary<string, string>>(); // Don't keep vars in scope between tests
            CurrentTest = t;
            TestFile.Tests.Add(t);

        }

        // We just entered a method and need to apply its default parameters, but ONLY if they aren't already in scope (this happens if they were specified as args)
        internal void ApplyDefaultParametersToScope(IEnumerable<AstFormalParameter> defaultParams)
        {
            foreach (var d in defaultParams)
            {
                if (!CurrentScopeVariables.ContainsKey(d.Name))
                {
                    CurrentScopeVariables.Add(d.Name, d.DefaultValue);
                }
            }
        }


        // Get the return values from a function (i.e. which were passed as variables)
        public Dictionary<string, string> GetReturnValues()
        {
            return TrackedArgs.ToDictionary(x => x.Name, x => x.Value);
        }

        internal void IncreaseScope(AstFunctionCall fc)
        {
            var callerScope = CurrentScopeVariables;
            //var arguments = fc.Arguments;

            TrackedArgs = new List<AstArgument>();

            var parameters = fc.Function.FormalParameters;
            var defaultParameters = parameters.Where(x => x.HasDefault);
            var outParameters = parameters.Where(x => x.IsOutParam);

            // Resolve args unless they're also out params
            List<AstArgument> arguments = new List<AstArgument>();

            #region Prepare args / REsolve known variable values
            foreach (var arg in fc.Arguments)
            {
                if (arg.Parts.Count == 0)
                {
                    // Can't happen I think
                    throw new Exception();
                }
                if (arg.Parts.Count == 1)
                {
                    // String or var
                    if (arg.Parts[0].IsVariable)
                    {
                        // Could be a return value
                        var p = parameters.First(x => x.Name == arg.Name);
                        if (p.IsOutParam)
                        {
                            TrackedArgs.Add(arg);
                            arguments.Add(arg);
                        }
                        else
                        {
                            // Try to resolve if poss
                            if (callerScope.ContainsKey(arg.Name))
                            {
                                arg.Value = callerScope[arg.Name];
                            }
                            arguments.Add(arg);
                        }
                    }
                    else
                    {
                        // string
                        arguments.Add(arg);
                    }
                }
                else
                {
                    // Multiple parts in arg - cannot be a return value.
                    throw new NotImplementedException("todo multi part args");
                }
            }
            #endregion

            IncreaseScope();

            AddVariablesToScope(defaultParameters);
            AddVariablesToScope(arguments);


            //s.IncreaseScope();
            //foreach (var f in fc.Arguments)
            //{
            //    s.AddArgumentToScope(f);
            //}

            //var f = fc.Function;

            //// Any args not set that have defaults should be added now
            //var defaultParams = f.FormalParameters.Where(x => !String.IsNullOrEmpty(x.DefaultValue));
            //s.ApplyDefaultParametersToScope(defaultParams);

            //var returnedParams = f.FormalParameters.Where(x => x.IsOutParam);
            //s.TrackReturnParams(returnedParams);
        }

        List<AstArgument> TrackedArgs { get; set; } // Args that are returned out of the function back to the caller

        private void AddVariablesToScope(List<AstArgument> arguments)
        {
            foreach (var a in arguments)
            {
                SetCurrentScopeVariable(a.Name, a.Value);
            }
        }

        //private void AddVariablesToScope(Dictionary<string, string> vars)
        //{

        //}
        private void AddVariablesToScope(IEnumerable<AstFormalParameter> vars)
        {
            foreach (var v in vars)
            {
                SetCurrentScopeVariable(v.Name, v.DefaultValue);
            }
        }

        private void SetCurrentScopeVariable(string key, string value)
        {
            if (key == null) key = "p0";
#warning error here where key wasn't set earlier in parser
            if (CurrentScopeVariables.ContainsKey(key))
            {
                CurrentScopeVariables[key] = value;
            }
            else
            {
                CurrentScopeVariables.Add(key, value);
            }
        }
    }




    public partial class SideTest
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("tests")]
        public List<Test> Tests { get; set; } = new List<Test>();

        [JsonProperty("suites")]
        public List<Suite> Suites { get; set; }

        [JsonProperty("urls")]
        public List<string> Urls { get; set; }

        [JsonProperty("plugins")]
        public List<object> Plugins { get; set; }
    }

    public partial class Suite
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("persistSession")]
        public bool PersistSession { get; set; }

        [JsonProperty("parallel")]
        public bool Parallel { get; set; }

        [JsonProperty("timeout")]
        public long Timeout { get; set; }

        [JsonProperty("tests")]
        public List<Guid> Tests { get; set; }
    }

    public partial class Test
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("commands")]
        public List<Command> Commands { get; set; } = new List<Command>();
    }

    public partial class Command
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("command")]
        public string CommandCommand { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("targets")]
        public List<List<string>> Targets { get; set; } = new List<List<string>>();

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
