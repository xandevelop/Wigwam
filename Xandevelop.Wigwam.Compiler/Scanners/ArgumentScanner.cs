using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Extensions;

namespace Xandevelop.Wigwam.Compiler.Scanners
{
    public class ArgumentDataOrError
    {
        public List<AstArgument> ArgumentData { get; set; }
        public List<ArgumentError> ArgumentErrors { get; set; }

        public bool IsError => ArgumentErrors != null;

        public static implicit operator ArgumentDataOrError(List<AstArgument> data) => new ArgumentDataOrError { ArgumentData = data };
        public static implicit operator ArgumentDataOrError(List<ArgumentError> errors) => new ArgumentDataOrError { ArgumentErrors = errors };
        
        public AstArgument this[string name]
        {
            get
            {
                return ArgumentData.FirstOrDefault(x => x.Name == name);
            }
        }
    }

    // Arg is like ArgName:Value ${somevar} XXX
    public class ArgumentScanner
    {
        public ArgumentDataOrError ScanLineArguments(Line line, List<Ast.AstFormalParameter> formalParameters)
        {
            List<AstFormalParameter> expargs = new List<AstFormalParameter>();
            foreach(var x in formalParameters)
            {
                expargs.Add(new AstFormalParameter { Name = x.Name, Matched = false, DefaultValue = x.DefaultValue, AllowNoValue = false });
            }
            return ScanLineArguments(line, expargs.ToArray());
        }

        // Consider making static?
        public ArgumentDataOrError ScanLineArguments(Line line, params AstFormalParameter[] expectedArguments)
        {
            var actualArgs = ScanArgumentsInLineOrder(line);

            var expectedArgumentsList = expectedArguments.ToList(); // Avoid multiple enumeration

            List<ArgumentError> errors = new List<ArgumentError>();

            List<AstArgument> result = new List<AstArgument>();
            int loopCount = Max(actualArgs.Count, expectedArgumentsList.Count);
            bool allowUnnamed = true; // Start from assumption args are in the correct order, but when they're not we trip this flag and then they must all be named

            for (int ix = 0; ix < loopCount; ix++)
            {
                var arg = actualArgs.SafeGet(ix);
                var exp = expectedArgumentsList.SafeGet(ix);

                if (arg == null && exp == null) break; // Can't happen logically

                if (arg == null && exp.IsRequired)
                {
                    errors.Add(new ArgumentError { Name = "Required argument not found" });
                    continue;
                }

                if (arg == null)
                    break; // No more args - so any others are defaults or missing required (so continue, not break)

                if (exp == null && arg != null)
                {
                    errors.Add(new ArgumentError { Name = "Too many arguments" });
                    continue;
                }

                if (arg.Name == null && !allowUnnamed)
                {
                    errors.Add(new ArgumentError { Name = "Argument must have a name" });
                    continue;
                }
                else if (arg.Name == null && allowUnnamed)
                {
                    // Ag is in correct position but isn't named - so derive it from param location instead
                    arg.Name = exp.Name;
                }

                if (arg.Name == exp.Name)
                {
                    // Correct location, no args have been out of sequence
                    result.Add(arg);
                    exp.Matched = true;
                }
                else
                {
                    // Incorrect location - may or may not be ok...
                    allowUnnamed = false; // From now on, it's not ok to rely only on argument location
                    exp = expectedArgumentsList.FirstOrDefault(q => q.Name == arg.Name && !q.Matched);

                    if (exp == null)
                    {
                        errors.Add(new ArgumentError { Name = "Expected argument not found" + arg.ValueString });
                        continue;
                    }
                    else
                    {
                        result.Add(arg);
                        exp.Matched = true;
                    }
                }
            }

            // If we got to here without exception, any remaining unmatched args will not be required and will have default values.
            foreach (var e in expectedArgumentsList.Where(x => !x.Matched))
            {
                if (e.IsRequired) errors.Add(new ArgumentError { Name = "Required arg not supplied" });
                else result.Add(e.GenerateDefaultArg());
            }

            if (errors.Count > 0) return errors;
            else return result;
        }

        public AstArgument ScanSingleArgument(string argumentString)
        {
            var partsNameAndValue = SplitNameValue(argumentString);
            return new AstArgument { Name = partsNameAndValue.Name, ValueString = partsNameAndValue.Value };
        }

        private List<AstArgument> ScanArgumentsInLineOrder(Line line)
        {
            List<AstArgument> result = new List<AstArgument>();
            foreach (var b in line.Blocks)
            {
                result.Add(ScanSingleArgument(b));
            }
            return result;
        }

        [Obsolete("Use List.SafeGet")]
        private T SafeGet<T>(List<T> list, int ix)
        {
            if (list.Count > ix) return list[ix];
            return default(T); // null, probably
        }
 

        private (string Name, string Value) SplitNameValue(string s)
        {
            var split2 = s.Split2(':', '=');
            if (split2.Part1 == null)
            {
                // No name was specified
                return (null, split2.Part0);
            }
            else
            {
                return (split2.Part0, split2.Part1);
            }
        }

        
        private int Max(int a, int b) => a > b ? a : b;

        public List<AstArgument> ScanLineArgumentsWithoutContext(Line line)
        {
            return ScanArgumentsInLineOrder(line)
                .Select(x => new AstArgument { Name = x.Name, Value = x.ValueString })
                .ToList();
        }
    }

    

    public class ArgumentError
    {
        public string Name { get; internal set; }
    }
}
