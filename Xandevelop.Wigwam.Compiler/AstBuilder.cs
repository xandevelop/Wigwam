using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler
{
    public class AstBuilder
    {
        public AstProgram Program { get; } = new AstProgram();

        #region Current Method Tracking

        public bool HasCurrentMethod => CurrentMethod != null;
        private IAstMethod CurrentMethod { get; set; }

        #endregion

        #region Build/Add Methods

        public void AddStatementToCurrentMethod(IAstStatement statement)
        {
            if (CurrentMethod != null) CurrentMethod.Statements.Add(statement);
            else throw new Exception("Precondition not met - must handle this in caller");
        }
        
        public void AddTest(AstTest astTest)
        {
            CurrentMethod = astTest;
            Program.Tests.Add(astTest);
        }

        public void AddFunction(AstFunction astFunction)
        {
            CurrentMethod = astFunction;
            Program.Functions.Add(astFunction);
        }

        #endregion

        #region Error Handling

        public Line CurrentLine { get; set; }
        private List<CompileMessage> _compileMessages { get; } = new List<CompileMessage>();
        public IEnumerable<CompileMessage> CompileMessages => _compileMessages;
        public void AddError(string error)
        {
            _compileMessages.Add(new CompileMessage
            {
                MessageType = CompileMessageType.Error,
                SourceLineNumber = CurrentLine.SourceLineNumber,
                SourceFile = CurrentLine.SourceFile,
                SourceLine = CurrentLine.SourceLine,
                Text = error
            });
        }

        #endregion
    }
}
