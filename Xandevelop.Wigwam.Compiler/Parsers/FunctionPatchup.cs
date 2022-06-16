﻿using System;
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
                ParseStatements(ast, null /*curr func null because this is a test not func*/, test.Statements, currentConditions);
            }
            // what about functions that haven't been called
        }
        private void ParseStatements(AstBuilder ast, AstFunction currentFunction, List<Ast.IAstStatement> statements, Dictionary<string, string> currentConditions)
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
                    callStatement.SetFunctionResolved(signature, currentConditions);

                    // We read the statement, then we "execute" it virtually
                    ParseStatements(ast, signature, signature.Statements, currentConditions);

                    



                    //                    if (signature.ConditionsWhenCompiled == null) // think this can't happen any more...
                    //                    {
                    //                        // The func has never been used - store the fact that the current method uses it with the current conditions set.
                    //#warning how do we do this - two different funcs may have same conditions and use this so we shouldn't generate it twice.

                    //                        signature.ConditionsWhenCalled = currentConditions;
                    //                        callStatement.SetFunctionResolved(signature.Function, currentConditions);
                    //                        //ast.Replace(callStatement, signature);
                    //                    }
                    //                    else if (signature.ConditionsWhenCompiled != null)
                    //                    {
                    //                        // If we're just reusing, don't do anything, but if we got here by a different path, dupliate the method.
                    //                        if (DictionariesSame(signature.ConditionsWhenCalled, currentConditions))
                    //#warning dodgy compare - need to do an order and case insensitive match above.
                    //                        {
                    //                            // do nothing - the method has already been coded to respond to these conditions ok.
                    //                            //ast.Replace(callStatement, signature);

                    //                            callStatement.SetFunctionResolved(signature.Function, currentConditions);
                    //                        }
                    //                        else
                    //                        {
                    //                            // Copy the function but with more specific preconditions - generate a new overload.
                    //                            var duplicatedFunc = ast.DuplicateFunction(signature, currentConditions); // Duplicate the signature for the current conditions
                    //                            // Replace the current line call with the overloaded version of the function.
                    //                            callStatement.SetFunctionResolved(duplicatedFunc.Function, currentConditions);

                    //                            //ast.Replace(callStatement, duplicatedFunc);
                    //                        }
                    //                    }
                }

                

                // recurse
                
            }

            // Apply post before pop out of the stack/recursion.  At bottom so not dependent on existence of other calls
            if (currentFunction != null)
            {
                foreach (var x in currentFunction.PostConditions)
                {
                    currentConditions[x.Variable] = x.Value;
                }
            }

        }

        private bool DictionariesSame(Dictionary<string, string> x, Dictionary<string, string> y)
        {
            if (x.Count != y.Count) return false;
            foreach(var xs in x)
            {
                if(y.Keys.Contains(xs.Key))
                {
                    if (y[xs.Key] != xs.Value) return false;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private AstFunction FindBestSignature(AstBuilder ast, AstFunctionCallNoContext callStatement, Dictionary<string, string> currentConditions)
        {
            ArgumentScanner s = new ArgumentScanner();
            List<AstFunction> matchedFuncs = new List<AstFunction>();

            // First, the function name must match
            List<AstFunction> functionsWithRightName = ast.Program.Functions.Where(x => x.Name.Trim().ToLower() == callStatement.FunctionName.Trim().ToLower()).ToList();

            //List<AstFunctionCallNoContext> allFunctionCalls = new List<AstFunctionCallNoContext>(); // Track all the function calls in the script, for use later when working out pre/post.

            if (functionsWithRightName.Count == 0)
            {
                // No match - error
                ast.AddError(StandardMessages.FunctionSignatureNotFound_NoFunctionWithName(callStatement.SourceLineForPatchup));
                return null;
            }
            if (functionsWithRightName.Count == 1)
            {
                var candidateFunction = functionsWithRightName.First();

                // We can only mean one possible function.  The args still need to make sense, but if they're a mismatch here we can give better error info.
                var argData = s.ScanLineArguments(callStatement.SourceLineForPatchup, candidateFunction.FormalParameters);
                if (argData.IsError)
                {
                    ast.AddError(StandardMessages.FunctionSignatureNotFound_SinglePossibility_ArgumentsIncorrect(callStatement.SourceLineForPatchup));
                    ast.AddArgumentErrors(argData.ArgumentErrors);
                    return null;
                }
                else
                {
                    // Args matched - also match preconditions.
                    bool preconditionsMet = new PreConditionChecker().PreConditionsMet(candidateFunction.PreConditions, currentConditions);
                    if(!preconditionsMet)
                    {
                        ast.AddError("todo better error here");
                    }

                    // Pre conditions are met... but do we need to generate an overload?
                    if(candidateFunction.ConditionsWhenCompiled != null)
                    {
                        // This func has been called before... check if this is the same override or not?
                        if(DictionariesSame(candidateFunction.ConditionsWhenCompiled, currentConditions))
                        {
                            // Dictionary is same, no changes needed
                            ;
                        }
                        else
                        {
                            // This is a differnt overload but with new preconditions for matching purposes.
                            candidateFunction = ast.DuplicateFunction(candidateFunction, currentConditions);
                        }
                    }
                    else
                    {
                        // Func has never been called before so record that we have called it here now before we reutnr it
                        candidateFunction.ConditionsWhenCompiled = currentConditions;
                    }

                    //callStatement.SetFunctionResolved(functionsWithRightName.First(), currentConditions);
                    return candidateFunction;
                    //return BuildFuncCall(functionsWithRightName.First(), callStatement);
                }
            }
            else // Multiple functions with same name
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
                        var candidateFunction = matchedFuncsSubset.First();
                        // Pre conditions are met... but do we need to generate an overload?
                        if (candidateFunction.ConditionsWhenCompiled != null)
                        {
                            // This func has been called before... check if this is the same override or not?
                            if (DictionariesSame(candidateFunction.ConditionsWhenCompiled, currentConditions))
                            {
                                // Dictionary is same, no changes needed
                                ;
                            }
                            else
                            {
                                // This is a differnt overload but with new preconditions for matching purposes.
                                candidateFunction = ast.DuplicateFunction(candidateFunction, currentConditions);
                            }
                        }
                        else
                        {
                            // Func has never been called before so record that we have called it here now before we reutnr it
                            candidateFunction.ConditionsWhenCompiled = currentConditions;
                        }

                        return candidateFunction;
                    }
                    if(matchedFuncsSubset.Count > 1)
                    {
                        ast.AddError(StandardMessages.FunctionSignatureNotFound_MultiplePossibility_TooManyPreconditionMatches(callStatement.SourceLineForPatchup));
                        return null;
                    }
                }
            }
            return null;
        }

        
        private bool PreConditionsMet(List<AstPreCondition> preConditions, Dictionary<string, string> currentConditions)
        {
            return new PreConditionChecker().PreConditionsMet(preConditions, currentConditions);
        }
        
        }
    }