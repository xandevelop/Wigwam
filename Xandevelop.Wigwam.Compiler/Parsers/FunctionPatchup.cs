using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    // Attempt 2.  For each test, do depth first search for signatures, tracking post conditions as we go.
    // This works a bit like an interpreter, but ignoring lines that do stuff other than calls.
    class FunctionPatchup
    {
        public void Parse(AstBuilder ast)
        {
            foreach (var test in ast.Program.Tests)
            {
                Dictionary<string, string> currentConditions = new Dictionary<string, string>();
                ParseStatements(ast, test.Statements, currentConditions);
            }
            // what about functions that haven't been called
        }
        private void ParseStatements(AstBuilder ast, List<Ast.IAstStatement> statements, Dictionary<string, string> currentConditions)
        {
            foreach (var callStatement in statements.Where(x => x is AstFunctionCallNoContext).Cast<AstFunctionCallNoContext>().ToList())
            {
                var signature = FindBestSignature(ast, callStatement, currentConditions);
                if (signature == null)
                {
                    // error - could not find
                    continue; // try the next one, don't throw 
                              // note: we may end up with spurious errors because the func you meant to call had a postcondition
                              // that when missed causes subsequent calls to fail so we may want to consider halting execution here until the next test
                              // ie. not continue but break instead.
                }
                else
                {
                    if (signature.ConditionsWhenCalled == null)
                    {
                        // The func has never been used - store the fact that the current method uses it with the current conditions set.
#warning how do we do this - two different funcs may have same conditions and use this so we shouldn't generate it twice.

                        signature.ConditionsWhenCalled = currentConditions;
                        ast.Replace(callStatement, signature);
                    }
                    else if (signature.ConditionsWhenCalled != null)
                    {
                        // If we're just reusing, don't do anything, but if we got here by a different path, dupliate the method.
                        if (signature.ConditionsWhenCalled == currentConditions)
#warning dodgy compare - need to do an order and case insensitive match above.
                        {
                            // do nothing - the method has already been coded to respond to these conditions ok.
                            ast.Replace(callStatement, signature);
                        }
                        else
                        {
                            // Copy the function but with more specific preconditions - generate a new overload.
                            var duplicatedFunc = ast.DuplicateFunction(signature, currentConditions); // Duplicate the signature for the current conditions
                            // Replace the current line call with the overloaded version of the function.
                            ast.Replace(callStatement, duplicatedFunc);
                        }
                    }

                    // recurse
                    ParseStatements(ast, signature.Function.Statements, currentConditions);
                }
            }
        }

        private AstFunctionCallTemp FindBestSignature(AstBuilder ast, AstFunctionCallNoContext callStatement, Dictionary<string, string> currentConditions)
        {
            ArgumentScanner s = new ArgumentScanner();
            List<AstFunction> matchedFuncs = new List<AstFunction>();

            // First, the function name must match
            List<AstFunction> functionsWithRightName = ast.Program.Functions.Where(x => x.Name.Trim().ToLower() == callStatement.FunctionName.Trim().ToLower()).ToList();

            List<AstFunctionCallNoContext> allFunctionCalls = new List<AstFunctionCallNoContext>(); // Track all the function calls in the script, for use later when working out pre/post.

            if (functionsWithRightName.Count == 0)
            {
                // No match - error
                ast.AddError(StandardMessages.FunctionSignatureNotFound_NoFunctionWithName(callStatement.SourceLineForPatchup));
                return null;
            }
            if (functionsWithRightName.Count == 1)
            {
                // We can only mean one possible function.  The args still need to make sense, but if they're a mismatch here we can give better error info.
                var argData = s.ScanLineArguments(callStatement.SourceLineForPatchup, functionsWithRightName.First().FormalParameters);
                if (argData.IsError)
                {
                    ast.AddError(StandardMessages.FunctionSignatureNotFound_SinglePossibility_ArgumentsIncorrect(callStatement.SourceLineForPatchup));
                    ast.AddArgumentErrors(argData.ArgumentErrors);
                    return null;
                }
                else
                {
                    return BuildFuncCall(functionsWithRightName.First(), callStatement);
                }
            }
            else
            {
                // The function is overloaded, so we need to work out which overload we're talking about.
                // So, we look for a function with matching arguments and hope they don't all have the same arg signature (we resolve later if they do)
                foreach (var possibleFunc in functionsWithRightName)
                {
                    ArgumentDataOrError argData = s.ScanLineArguments(callStatement.SourceLineForPatchup, possibleFunc.FormalParameters);
                    if (argData.IsError)
                    {
                        // No match
                    }
                    else
                    {
                        matchedFuncs.Add(possibleFunc);
                    }
                }

                if (matchedFuncs.Count == 0)
                {
                    // No match - error
                    ast.AddError(StandardMessages.FunctionSignatureNotFound_MultiplePossibility_ArgumentsIncorrect(callStatement.SourceLineForPatchup));
                    return null;
                }
                else
                {
                    // match on preconditions

                    var matchedFuncsSubset = matchedFuncs.Where(x => PreConditionsMet(x.PreConditions, currentConditions)).ToList();

                    if(matchedFuncsSubset.Count == 0)
                    {
                        ast.AddError(StandardMessages.FunctionSignatureNotFound_MultiplePossibility_PreConditionsNotMet(callStatement.SourceLineForPatchup));
                        return null;
                    }
                    if(matchedFuncsSubset.Count == 1)
                    {
                        return BuildFuncCall(matchedFuncsSubset.First(), callStatement);
                    }
                    if(matchedFuncsSubset.Count > 1)
                    {
                        ast.AddError(StandardMessages.FunctionSignatureNotFound_MultiplePossibility_TooManyPreconditionMatches(callStatement.SourceLineForPatchup));
                        return null;
                    }

                    //var funcCall = BuildFuncCall(matchedFuncs, callStatement);
                    //throw new NotImplementedException();

                    //ast.Replace(fc, funcCall);
                }
            }
            return null;
        }

        static bool p = true;
        private bool PreConditionsMet(List<AstPreCondition> preConditions, Dictionary<string, string> currentConditions)
        {
#warning todo
            var pprev = p;
            p = false;
            return pprev;
        }

        private AstFunctionCallTemp BuildFuncCall(AstFunction astFunction, AstFunctionCallNoContext fc)
        {
            return new AstFunctionCallTemp
            {
                SourceFile = fc.SourceFile,
                SourceLine = fc.SourceLine,
                SourceLineNumber = fc.SourceLineNumber,
                Arguments = fc.ContextFreeArguments,
                Description = fc.Description,
                Function = astFunction,
                //CallerMethod = fc
            };
        }
        }
    }
