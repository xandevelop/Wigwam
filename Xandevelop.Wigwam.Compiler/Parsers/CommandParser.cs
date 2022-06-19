using System;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    public class CommandParser : ILineParser
    {
        public string Name => "Command Parser";

        public int OrderNumber => 1;

        public bool IsMatch(Line line)
        {
            switch (line.Command)
            {
                case "click": return true; break;
                case "echo": return true; break;
            }
            return false;
        }

        public void Parse(AstBuilder ast, Line line)
        {
            switch (line.Command)
            {
                case "click": ParseClick(ast, line); break;
                case "echo": ParseEcho(ast, line); break;
                default: throw new Exception();
            }
        }

        ArgumentScanner ArgumentScanner { get; } = new ArgumentScanner();

        private void ParseClick(AstBuilder ast, Line line)
        {
            var args = ArgumentScanner.ScanLineArguments(line, new ExpectedArgument { Name = "target" });
            

            if (args.IsError) { ast.AddArgumentErrors(args.ArgumentErrors); return; }
            ArgumentData targetArgument = args["target"];
            

            if (ast.HasCurrentMethod)
            {
                ast.AddStatementToCurrentMethod(new AstCommand {
                    SourceFile = line.SourceFile,
                    SourceLine = line.SourceLine,
                    SourceLineNumber = line.SourceLineNumber,
                    Command = line.Command,
                    Target = targetArgument.ValueString,
                    Value = null,
                    Description = line.CommentBlock });
            }
            else
            {
                ast.AddError(StandardMessages.CommandOutsideOfMethod(line));
            }
        }

        private void ParseEcho(AstBuilder ast, Line line)
        {
            var args = ArgumentScanner.ScanLineArguments(line, new ExpectedArgument { Name = "target" });
            
            if (args.IsError) { ast.AddArgumentErrors(args.ArgumentErrors); return; }
            ArgumentData targetArgument = args["target"];
            
            if (ast.HasCurrentMethod)
            {
                ast.AddStatementToCurrentMethod(new AstCommand
                {
                    SourceFile = line.SourceFile,
                    SourceLine = line.SourceLine,
                    SourceLineNumber = line.SourceLineNumber,
                    Command = line.Command,
                    Target = targetArgument.ValueString,
                    Value = null,
                    Description = line.CommentBlock
                });
            }
            else
            {
                ast.AddError(StandardMessages.CommandOutsideOfMethod(line));
            }
        }
    }
}
