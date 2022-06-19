using System.Collections.Generic;
using System.Linq;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Parsers;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler
{
    
    // Convert file(s) to AST
    public class Compiler
    {
        private Compiler() { }

        // should the C# code break when a compile error is found? Useful for debugging when you don't expect an error.
        public bool BreakOnError { get; set; }

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
            c.LineParsers.Add(new CommandParser()); // click, echo, screenshot, speak, assert, verify, store
            c.LineParsers.Add(new FunctionCallParser());
            c.LineParsers.Add(new TestDeclarationParser());
            c.LineParsers.Add(new FunctionDeclarationParser());
            c.LineParsers.Add(new PreConditionParser());
            c.LineParsers.Add(new PostConditionParser());
            c.LineParsers.Add(new ControlDeclarationParser());
            c.LineParsers.Add(new CommandDefinitionParser());
            
            //c.LineParsers.Add(new VariableDeclarationParser()); // Maybe?

            // I've not decided if these are really a good idea or not yet... They may be superficially done with commands?
            //c.LineParsers.Add(new IfBlockParser());
            //c.LineParsers.Add(new WhileBlockParser());
            //c.LineParsers.Add(new EndBlockParser());

            return c;
        }

        public List<BuiltInCommandSignature> BuiltInCommandList { get; set; } = new List<BuiltInCommandSignature>();

        public IFileReader FileReader { get; set; } = new FileReader();

        public List<ILineParser> LineParsers { get; set; } = new List<ILineParser>();
        private List<ILineParser> OrderedLineParsers => LineParsers.OrderBy(x => x.OrderNumber).ThenBy(x => x.Name).ToList();

        public (AstProgram ast, IEnumerable<CompileMessage> compileErrors) Compile(string filePath)
        {
            AstBuilder astBuilder = new AstBuilder(filePath);
            astBuilder.BreakOnError = this.BreakOnError;

            FirstPass(filePath, astBuilder);
            SecondPass(astBuilder);
            return (astBuilder.Program, astBuilder.CompileMessages);
        }

        // To deal with possibly recursive include statements - we don't reprocess any files in this list
        private List<string> ProcessedPaths { get; } = new List<string>();

        // First Pass
        // WARNING: Recursive
        private void FirstPass(string filePath, AstBuilder astBuilder)
        {
            
            FileScanner fileScanner = new FileScanner();
            List<Line> lines = fileScanner.ReadLines(filePath, FileReader.ReadAllText(filePath));

            foreach (Line line in lines)
            {
                astBuilder.CurrentLine = line; // For error handling - when an error is found, we can use this to not have to pass loads of trace info.

                if (line.Command == "include" || line.Command == "import" || line.Command == "using")
                {
                    // Special logic applies for includes so that they can be recursive.  Other commands can be plugin-like things.

                    string relPath = line.Blocks.FirstOrDefault();
                    if (relPath == null)
                    {
                        astBuilder.AddError(StandardMessages.IncludeMustSpecifyPath(line));
                    }
                    else
                    {
                        string absPath = FileReader.BuildAbsPath(filePath, relPath);
                        if (absPath == null)
                        {
                            astBuilder.AddError(StandardMessages.IncludeFileNotFound(line));
                        }
                        else
                        {
                            if (ProcessedPaths.Contains(absPath))
                            {
                                // Already included - possible circular reference
                            }
                            else
                            {
                                ProcessedPaths.Add(absPath);
                                FirstPass(absPath, astBuilder);
                            }
                        }
                    }
                }
                else
                {
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
            }
            
        }

        private void SecondPass(AstBuilder astBuilder)
        {
            // First pass identifies function calls,
            // but since functions may be declared AFTER they're used, we can't determine if they're valid until all files are fully read.
            // So second pass just goes over what is in the builder and verifies anything that needed the full context to determine validity.

            //FunctionPatchupParser fpp = new FunctionPatchupParser();
            //fpp.Parse(astBuilder);

            FunctionPatchup fp = new FunctionPatchup();
            fp.Parse(astBuilder);

        }

        
    }


}
