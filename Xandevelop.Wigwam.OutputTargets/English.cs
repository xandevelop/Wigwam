using Xandevelop.Wigwam.Compiler;

namespace Xandevelop.Wigwam.OutputTargets
{
    public class English
    {
        public string ToEnglish(Ast.AstProgram program)
        {
            EnglishResult = "";

            ProgramVisitor programVisitor = new ProgramVisitor();

            programVisitor.Command += OnCommand;
            programVisitor.EndTest += (object sender, Ast.AstTest t) => { EnglishResult += "*** End of test ***" + Environment.NewLine; };
            programVisitor.StartTest += (object sender, Ast.AstTest t) => { EnglishResult += "*** Start test \"" + t.Name + "\" ***" + Environment.NewLine; };

            programVisitor.VisitDepthFirst(program);

            return EnglishResult;
        }

        private string EnglishResult { get; set; }

        private void OnCommand(object sender, Ast.AstCommand e)
        {
            switch (e.Command)
            {
                case "echo":
                    EnglishResult += $"  Print \"{e.Target}\" to the console" + Environment.NewLine;
                    break;
                default:
                    EnglishResult += e.Command + " " + e.Target ?? "" + " " + e.Value ?? "";
                    break;

            }
        }
    }
}