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
    
    // Convert file(s) to AST
    public class Compiler
    {
        private Compiler() { }

        public static Compiler DefaultCompiler()
        {
            Compiler c = new Compiler();
            c.BuiltInCommandList = new List<BuiltInCommandSignature> {
                ("click", ParamType.Selector),
                ("type", ParamType.Selector, ParamType.String),
                ("echo", ParamType.String),
                ("open", ParamType.Url),
                ("store", ParamType.String, ParamType.Variable)
            };

            c.LineParsers = new List<ILineParser>();
            c.LineParsers.Add(new CommandParser());
            c.LineParsers.Add(new FunctionCallParser());
            c.LineParsers.Add(new TestDeclarationParser());
            c.LineParsers.Add(new FunctionDeclarationParser());

            return c;
        }

        public List<BuiltInCommandSignature> BuiltInCommandList { get; set; } = new List<BuiltInCommandSignature>();

        public IFileReader FileReader { get; set; } = new FileReader();

        public List<ILineParser> LineParsers { get; set; } = new List<ILineParser>();
        private List<ILineParser> OrderedLineParsers => LineParsers.OrderBy(x => x.OrderNumber).ThenBy(x => x.Name).ToList();

        public (AstProgram ast, IEnumerable<CompileMessage> compileErrors) Compile(string filePath)
        {
            AstBuilder astBuilder = new AstBuilder();
            FirstPass(filePath, astBuilder);
            SecondPass(astBuilder);
            return (astBuilder.Program, astBuilder.CompileMessages);
        }

        // First Pass
        // WARNING: Recursive
        private void FirstPass(string filePath, AstBuilder astBuilder)
        {
            string prev = filePath;

            FileScanner fileScanner = new FileScanner();
            List<Line> lines = fileScanner.ReadLines(filePath, FileReader.ReadAllText(filePath));

            foreach (Line line in lines)
            {
                astBuilder.CurrentLine = line; // For error handling - when an error is found, we can use this to not have to pass loads of trace info.

                var lineParser = OrderedLineParsers.FirstOrDefault(x => x.IsMatch(line));
                if (lineParser == null)
                {
                    astBuilder.AddError(StandardMessages.NoParserFound(line));
                }
                else
                {
                    lineParser.Parse(astBuilder, line);
                }
            }

            filePath = prev;
        }

        private void SecondPass(AstBuilder astBuilder)
        {
            // First pass identifies function calls,
            // but since functions may be declared AFTER they're used, we can't determine if they're valid until all files are fully read.
            // So second pass just goes over what is in the builder and verifies anything that needed the full context to determine validity.

#warning todo
        }
    }


}
