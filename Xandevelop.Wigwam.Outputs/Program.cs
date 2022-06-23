using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Parsers;
using Xandevelop.Wigwam.Outputs.TargetLanguages;

namespace Xandevelop.Wigwam.Outputs
{
    class Program
    {
        static void Main(string[] args)
        {
            string target = args[0];
            string fileName = args[1];
            

            var c = Compiler.Compiler.DefaultCompiler();
            var ast = c.Compile(fileName);

            if (ast.compileErrors.Any())
            {
                foreach(var x in ast.compileErrors)
                {
                    Console.WriteLine(x.Text);
                }
            }
            else
            {
                // Translate to plain English and SIDE first
                if(target == "english")
                {
                    ast.ast.ToEnglish();
                }
                if(target == "side")
                {
                    ToSide(ast.ast);
                }
            }

            Console.ReadLine();
        }

        private static void ToSide(Ast.AstProgram program)
        {
            var side = new SIDE();
            var result = side.ToSide(program);
            var json = JsonConvert.SerializeObject(result);
            File.WriteAllText(@"C:\Wigwam\V2\Xan.Wigwam\out.side", json);
            ;
        }

        
    }
    

    static class Exn
    {
        public static void ToEnglish(this Ast.AstProgram program)
        {
            Compiler.ProgramVisitor programVisitor = new Compiler.ProgramVisitor();

            programVisitor.Command += OnCommand;
            programVisitor.EndTest += (object sender, AstTest t) => { Console.WriteLine("*** End of test ***\n"); };
            programVisitor.StartTest += (object sender, AstTest t) => { Console.WriteLine("*** Start test \"" + t.Name + "\" ***"); };

            programVisitor.VisitDepthFirst(program);
        }


        private static void OnCommand(object sender, AstCommand e)
        {
            switch(e.Command)
            {
                case "echo":
                    Console.WriteLine($"  Print \"{e.Target}\" to the console");
                    break;
                default:
                    Console.WriteLine(e.Command + " " + e.Target ?? "" + " " + e.Value ?? "");
                    break;

            }
        }


    }
}
