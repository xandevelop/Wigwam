using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    public class FunctionPatchupParser : ISecondPassParser
    {
        public string Name => "Function Patchup";

        public int OrderNumber => 1;

        public void Parse(AstBuilder ast)
        {
            foreach(var method in ast.AllMethods)
            {
                var functionCallsWithoutContext = method.Statements.Where(x => x is AstFunctionCallNoContext).Cast<AstFunctionCallNoContext>().ToList();
                foreach(var fc in functionCallsWithoutContext)
                {
                    ast.CurrentLine = fc.SourceLineForPatchup; // Ensure error reports go against correct location
                    FindBestMatchingSignature(ast, fc, ast.Program.Functions);
                }
            }
        }

        private void FindBestMatchingSignature(AstBuilder ast, AstFunctionCallNoContext fc, List<AstFunction> functions)
        {
            ArgumentScanner s = new ArgumentScanner();
            List<AstFunction> matchedFuncs = new List<AstFunction>();
            
            // First, the function name must match
            List<AstFunction> functionsWithRightName = functions.Where(x => x.Name.Trim().ToLower() == fc.FunctionName.Trim().ToLower()).ToList();

            List<AstFunctionCallNoContext> allFunctionCalls = new List<AstFunctionCallNoContext>(); // Track all the function calls in the script, for use later when working out pre/post.

            if(functionsWithRightName.Count == 0)
            {
                // No match - error
                ast.AddError(StandardMessages.FunctionSignatureNotFound_NoFunctionWithName(fc.SourceLineForPatchup));
                return;
            }
            if (functionsWithRightName.Count == 1)
            {
                // We can only mean one possible function.  The args still need to make sense, but if they're a mismatch here we can give better error info.
                var argData = s.ScanLineArguments(fc.SourceLineForPatchup, functionsWithRightName.First().FormalParameters);
                if(argData.IsError)
                {
                    ast.AddError(StandardMessages.FunctionSignatureNotFound_SinglePossibility_ArgumentsIncorrect(fc.SourceLineForPatchup));
                    ast.AddArgumentErrors(argData.ArgumentErrors);
                    return;
                }
                else
                {
                    var funcCall = BuildFuncCall(functionsWithRightName.First(), fc);
                    fc.SetFunctionResolved(funcCall);
                    allFunctionCalls.Add(fc);
                    //ast.Replace(fc, funcCall);
                }
            }
            else
            {
                // The function is overloaded, so we need to work out which overload we're talking about.
                // So, we look for a function with matching arguments and hope they don't all have the same arg signature (we resolve later if they do)
                foreach (var possibleFunc in functionsWithRightName)
                {
                    ArgumentDataOrError argData = s.ScanLineArguments(fc.SourceLineForPatchup, possibleFunc.FormalParameters);
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
                    ast.AddError(StandardMessages.FunctionSignatureNotFound_MultiplePossibility_ArgumentsIncorrect(fc.SourceLineForPatchup));
                    return;
                }
                else
                {
                    // At least one match.  May need to work out which one by looking at pre/post conditions, which can't happen until inlining in case of multiple call stack routes.
                    // e.g. 
                    // test a -> post a; call func x;
                    // test b -> post b; call func x;
                    // func x-> call func y;
                    // func y -> pre a; echo a
                    // func y -> pre b; echo b
                    //
                    // Which overload of func y func x refers to is set from test a or test b, not from func x.
                    // Essentially, this compiles to:
                    // test a -> post a; call func x_a;
                    // test b -> post b; call func x_b;
                    // func x_a-> call func y_a;
                    // func x_b-> call func y_b;
                    // func y_a -> pre a; echo a
                    // func y_b -> pre b; echo b
                    // 
                    // We probably don't know which path we're following until we try to inline.

                    // Traverse up the call stack to work out the conditions currently in play to determine which function we're actually talking about here.
                    //if(fc.Method.

                    var funcCall = BuildFuncCall(matchedFuncs, fc);
                    fc.SetFunctionResolved(funcCall);
                    allFunctionCalls.Add(fc);
                    //ast.Replace(fc, funcCall);
                }
            }

            // Next, some links were probably unresolved, so start working them out...  (Items only distinguished by pre/post conditions)
            while(allFunctionCalls.Any(x => x.FunctionResolved.Count > 1))
            {
                List<AstFunctionCallNoContext> functionCallsToProcess = allFunctionCalls.Where(x => x.FunctionResolved.Count > 1).ToList();
                for(int i = 0; i < functionCallsToProcess.Count; i++)
                {
                    var curFuncCall = functionCallsToProcess[i];

                    //curFuncCall.FunctionResolved.First().
                }
            }

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

        private List<AstFunctionCallTemp> BuildFuncCall(List<AstFunction> astFunctions, AstFunctionCallNoContext fc)
        {
            var result = new List<AstFunctionCallTemp>();
            foreach(var a in astFunctions)
            {
                result.Add(BuildFuncCall(a, fc));
            }
            return result;
        }

        private List<AstArgument> ConvertArgTypes(List<ArgumentData> argData)
        {
            List<AstArgument> result = new List<AstArgument>();
            foreach(var x in argData)
            {
                result.Add(new AstArgument
                {
                    Name = x.Name,
                    Value = x.ValueString
                });
            }
            return result;
        }
    }

    // public class ControlDeclarationInliner - controls which are declared should be inlined where they're referenced in instructions
    
}
