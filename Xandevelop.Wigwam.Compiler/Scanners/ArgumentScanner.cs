using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xandevelop.Wigwam.Ast;

namespace Xandevelop.Wigwam.Compiler.Scanners
{
    public class ArgumentDataOrError
    {
        public List<ArgumentData> ArgumentData { get; set; }
        public List<ArgumentError> ArgumentErrors { get; set; }

        public bool IsError => ArgumentErrors != null;

        public static implicit operator ArgumentDataOrError(List<ArgumentData> data) => new ArgumentDataOrError { ArgumentData = data };
        public static implicit operator ArgumentDataOrError(List<ArgumentError> errors) => new ArgumentDataOrError { ArgumentErrors = errors };
        
        public ArgumentData this[string name]
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
            List<ExpectedArgument> expargs = new List<ExpectedArgument>();
            foreach(var x in formalParameters)
            {
                expargs.Add(new ExpectedArgument { Name = x.Name, Matched = false, DefaultValue = x.DefaultValue, AllowNoValue = false });
            }
            return ScanLineArguments(line, expargs.ToArray());
        }

        // Consider making static?
        public ArgumentDataOrError ScanLineArguments(Line line, params ExpectedArgument[] expectedArguments)
        {
            var actualArgs = ScanArgumentsInLineOrder(line);

            List<ArgumentError> errors = new List<ArgumentError>();

            List<ArgumentData> result = new List<ArgumentData>();
            int loopCount = Max(actualArgs.Count, expectedArguments.ToList().Count);
            bool allowUnnamed = true; // Start from assumption args are in the correct order, but when they're not we trip this flag and then they must all be named

            for (int ix = 0; ix < loopCount; ix++)
            {
                var arg = SafeGet(actualArgs, ix);
                var exp = SafeGet(expectedArguments.ToList(), ix);

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
                    exp = expectedArguments.FirstOrDefault(q => q.Name == arg.Name && !q.Matched);

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
            foreach (var e in expectedArguments.Where(x => !x.Matched))
            {
                if (e.IsRequired) errors.Add(new ArgumentError { Name = "Required arg not supplied" });
                else result.Add(e.GenerateDefaultArg());
            }

            if (errors.Count > 0) return errors;
            else return result;
        }

        public ArgumentData ScanSingleArgument(string argumentString)
        {
            var partsNameAndValue = SplitNameValue(argumentString);
            return new ArgumentData { Name = partsNameAndValue.Name, ValueString = partsNameAndValue.Value };
        }

        private List<ArgumentData> ScanArgumentsInLineOrder(Line line)
        {
            List<ArgumentData> result = new List<ArgumentData>();
            foreach (var b in line.Blocks)
            {
                result.Add(ScanSingleArgument(b));
            }
            return result;
        }

        private T SafeGet<T>(List<T> list, int ix)
        {
            if (list.Count > ix) return list[ix];
            return default(T); // null, probably
        }

        private (string Part0, string Part1) Split2(string s, char splitChar)
        {
            var parts = s.SplitWithEscape(new[] { splitChar }, 2).ToList();
            switch (parts.Count)
            {
                case 0: return (null, null);
                case 1: return (parts[0], null);
                case 2: return (parts[0], parts[1]);
                default: throw new Exception("Can't happen");
            }
        }
        private (string Name, string Value) SplitNameValue(string s)
        {
            var split2 = Split2(s, ':');
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

    public class ArgumentData
    {
        public string Name { get; set; }
        public string ValueString { get; set; } // Whole string, ignoring variable possibility until later.

        public List<ArgumentPartsData> Parts
        {
            get
            {
                string variableregex = @"(?=(\${))|(?<=})"; // see https://regex101.com/
                return Regex.Split(ValueString, variableregex).Select(x => new ArgumentPartsData { Value = x }).ToList();
            }
        }

        public bool PossiblyPassedByReference
        {
            get
            {
                if (Parts.Count != 1) return false;
                if (Parts.First().IsString) return false;
                return true;
            }
        }
    }
    public class ArgumentPartsData
    {
        public bool IsVariable => Value.Trim().StartsWith("${") && Value.Trim().EndsWith("}");
        public bool IsString => !IsVariable;

        public string Value { get; set; }
    }

    public class ExpectedArgument
    {
        public bool IsRequired => DefaultValue == null && !AllowNoValue;
        public string Name { get; set; }
        public bool Matched { get; set; }

        // Only if not required
        public string DefaultValue { get; set; }

        /// <summary>
        /// Allow the arg to not be specified, even when there's no default
        /// </summary>
        public bool AllowNoValue { get; set; }

        public ArgumentData GenerateDefaultArg()
        {
            return new ArgumentData { Name = Name, ValueString = DefaultValue };
        }
    }

    public class ArgumentError
    {
        public string Name { get; internal set; }
    }
}
