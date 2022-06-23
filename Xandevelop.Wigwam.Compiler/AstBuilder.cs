using System;
using System.Collections.Generic;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Parsers;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler
{
    public class AstBuilder
    {
        public AstBuilder(string initialFilePath)
        {
            Program = new AstProgram();
            Program.SourceFile = initialFilePath;
        }

        public AstProgram Program { get; }

        #region Current Method Tracking

        public bool HasCurrentMethod => CurrentMethod != null;
        public IAstMethod CurrentMethod { get; set; }

        public bool CurrentMethodIsFunction => CurrentMethod is AstFunction;

        public List<IAstMethod> AllMethods { get; set; } = new List<IAstMethod>();
        
        #endregion

        #region Build/Add Methods

        public void AddPreCondition(AstPreCondition pre)
        {
            (CurrentMethod as AstFunction).PreConditions.Add(pre);
        }

        

        public void AddPostCondition(AstPostCondition post)
        {
            (CurrentMethod as AstFunction).PostConditions.Add(post);
        }

        public void AddStatementToCurrentMethod(IAstStatement statement)
        {
            if (CurrentMethod != null)
            {
                CurrentMethod.Statements.Add(statement);
            }

            else throw new Exception("Precondition not met - must handle this in caller");
        }

        
        public void AddTest(AstTest astTest)
        {
            // Note: we don't add line tracking info here because we *may* add a test or function or whatever from a line other than the one we're currently on
            // So there's more boilerplate code living in the ILineParsers, but it's more flexible for future.
            CurrentMethod = astTest;
            Program.Tests.Add(astTest);
            AllMethods.Add(astTest);
        }

        public AstFunction DuplicateFunction(AstFunction method, Dictionary<string, string> conditionsWhenCalled)
        {
            AstFunction func = new AstFunction()
            {
                Name = method.Name,
                FormalParameters = method.FormalParameters,
                PostConditions = method.PostConditions,
                Description = method.Description,
                OverloadGeneratedFrom = method,
                SourceCode = method.SourceCode
            };

            // Can't do Statements = method.Statements - we need a value copy, not a reference copy.
            foreach(var s in method.Statements)
            {
                if (s is AstFunctionCall)
                {
                    func.Statements.Add(new AstUnresolvedCall(method, s.SourceCode, conditionsWhenCalled));
                }
                else
                {
                    // old code
                    func.Statements.Add(s.CopyWithNewConditions(conditionsWhenCalled));
                }
            }

            
            foreach (var x in conditionsWhenCalled)
            {
                var newpre = new AstPreCondition
                {
                    Variable = x.Key,
                    Value = x.Value,
                    Comparison = PreConditionComparisonType.Equals,
                    SourceCode = method.SourceCode
                };
                newpre.SourceCode.SourceLine = "(Automatically Generated PreCondition)";
                func.PreConditions.Add(newpre);
            }
            func.ConditionsWhenCompiled = AstUnresolvedCall.CopyConditionsWhenCalled(conditionsWhenCalled);

            Program.Functions.Add(func);
            AllMethods.Add(func);

            return func;
        }

        internal void AddCommandDefinitions(List<BuiltInCommandSignature> builtInCommandList)
        {
            foreach (var c in builtInCommandList)
            {
                AddCommandDefinition(c.ToCommandDefinition());
            }
        }

        public void AddFunction(AstFunction astFunction)
        {
            CurrentMethod = astFunction;
            Program.Functions.Add(astFunction);
            AllMethods.Add(astFunction);
        }

        internal void AddControlDeclaration(AstControlDeclaration control)
        {
            CurrentMethod = null;
            Program.Controls.Add(control);
            
        }
        internal void AddCommandDefinition(AstCommandDefinition astCmd)
        {
            CurrentMethod = null;
            Program.CommandDefinitions.Add(astCmd);
        }

        public void Replace(AstUnresolvedCall unresolvedCall, AstFunctionCall functionCall)
        {
            var method = unresolvedCall.Method;

            int ix = method.Statements.IndexOf(unresolvedCall);
            method.Statements.RemoveAt(ix);
            method.Statements.Insert(ix, functionCall);
        }
        public void Replace(AstUnresolvedCall unresolvedCall, AstCommand command)
        {
            var method = unresolvedCall.Method;

            int ix = method.Statements.IndexOf(unresolvedCall);
            method.Statements.RemoveAt(ix);
            method.Statements.Insert(ix, command);
        }

        #endregion

        #region Error Handling

        public bool BreakOnError { get; set; }

        public Line CurrentLine { get; set; }
        private List<CompileMessage> _compileMessages { get; } = new List<CompileMessage>();
        public IEnumerable<CompileMessage> CompileMessages => _compileMessages;

        

        public void AddError(string error)
        {
            if(BreakOnError)
            {
                System.Diagnostics.Debugger.Break();
            }

            _compileMessages.Add(new CompileMessage
            {
                MessageType = CompileMessageType.Error,
                SourceLineNumber = CurrentLine.SourceLineNumber,
                SourceFile = CurrentLine.SourceFile,
                SourceLine = CurrentLine.SourceLine,
                Text = error
            });
        }

        public void AddArgumentErrors(List<ArgumentError> errors)
        {
            foreach(var err in errors)
            {
                AddError(err.Name);
            }
        }


        #endregion
    }
}
