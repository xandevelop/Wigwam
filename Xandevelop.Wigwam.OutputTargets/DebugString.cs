using Xandevelop.Wigwam.Compiler;

namespace Xandevelop.Wigwam.OutputTargets
{
    public class DebugString
    {
        public string ToDebugString(Ast.AstProgram program)
        {
            Result = "";

            ProgramVisitor programVisitor = new ProgramVisitor();

            programVisitor.Command += (object sender, Ast.AstCommand command) => { Result += command.ToDebugString() + Environment.NewLine; };
            programVisitor.EndTest += (object sender, Ast.AstTest t) => { Result += "End of test" + Environment.NewLine; };
            programVisitor.StartTest += (object sender, Ast.AstTest t) => { Result += "Test " + t.Name + "" + Environment.NewLine; };

            programVisitor.VisitBreadthFirst(program);

            return Result;
        }


        //public event EventHandler<Ast.AstTest> StartTest;
        //public event EventHandler<Ast.AstTest> EndTest; 
        //public event EventHandler<Ast.AstFunctionCall> StartFunctionCall;
        //public event EventHandler<Ast.AstFunction> StartFunction;
        //public event EventHandler<Ast.AstFunction> EndFunction;
        //public event EventHandler<Ast.AstFunctionCall> EndFunctionCall; 
        //public event EventHandler<Ast.AstCommand> Command; 
        //public event EventHandler<Ast.AstControlDeclaration> Control;
        //public event EventHandler<Ast.AstCommandDefinition> CommandDefinition;


        private string Result { get; set; }

    }
}
