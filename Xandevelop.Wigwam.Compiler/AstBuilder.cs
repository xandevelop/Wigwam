using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private IAstMethod CurrentMethod { get; set; }

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
                SourceFile = method.SourceFile,
                SourceLine = method.SourceLine,
                SourceLineNumber = method.SourceLineNumber
            };

            // Can't do Statements = method.Statements - we need a value copy, not a reference copy.
            foreach(var s in method.Statements)
            {
                func.Statements.Add(s.CopyWithNewConditions(conditionsWhenCalled));
            }

            
            foreach (var x in conditionsWhenCalled)
            {
                func.PreConditions.Add(new AstPreCondition { Variable = x.Key, Value = x.Value, Comparison = PreConditionComparisonType.Equals,
                    SourceFile = method.SourceFile, SourceLine = "(Automatically Generated PreCondition)", SourceLineNumber = method.SourceLineNumber });
            }
            func.ConditionsWhenCompiled = AstFunctionCallNoContext.CopyConditionsWhenCalled(conditionsWhenCalled);

            Program.Functions.Add(func);
            AllMethods.Add(func);

            return func;
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
