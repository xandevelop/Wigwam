using System;
using System.Linq;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Scanners;

namespace Xandevelop.Wigwam.Compiler.Parsers
{
    class ControlDeclarationParser : ILineParser
    {
        public string Name => "ControlDeclaration";

        public int OrderNumber => 1;

        public bool IsMatch(Line line)
        {
            return line.Command == "control";
        }

        ArgumentScanner ArgumentScanner { get; } = new ArgumentScanner();

        // Control | name:MyControl | strategy:xpath | selector:something${x:1}
        public void Parse(AstBuilder ast, Line line)
        {
            // expected args are odd for this one... selector and strategy may be switched implicitly
            // e.g.
            // Control | name:abc | xpath | someselector # Strategy then selector (when block count is 3)
            // Control | name:abc | xpath:someselector   # only selector, strategy is xpath (when block count is 2)
            // Control | name:abc | someselector         # only selector, assumed to be xpath (when block count is 2)

            var args = ArgumentScanner.ScanLineArguments(line,
                    new AstFormalParameter { Name = "name" },
                    new AstFormalParameter { Name = "strategy", DefaultValue = "xpath" }, //note: may have to swap location with selector
                    new AstFormalParameter { Name = "selector", AllowNoValue = true }, // No default, but also not mandatory
                    new AstFormalParameter { Name = "xpath", AllowNoValue = true },
                    new AstFormalParameter { Name = "id", AllowNoValue = true },
                    new AstFormalParameter { Name = "css", AllowNoValue = true },
                    new AstFormalParameter { Name = "friendly name", AllowNoValue = true }
                );
            if (args.ArgumentErrors?.Any()??false)
            {
                foreach (var x in args.ArgumentErrors)
                {
                    ast.AddError(x.Name);
                }
                return;
            }
            var nameArgument = args["name"];
            var strategyArgument = args["strategy"];
            var selectorArgument = args["selector"];
            var xpathArgument = args["xpath"];
            var idArgument = args["id"];
            var cssArgument = args["css"];
            var friendlyNameArgument = args["friendly name"];

            string selectorValue = null;
            Xandevelop.Wigwam.Ast.SelectorStrategy? strategy = null;
            if (selectorArgument.ValueString != null)
            {
                strategy = Map(strategyArgument.ValueString);
                selectorValue = selectorArgument.ValueString;
            }
            if (xpathArgument.ValueString != null)
            {
                if (strategy != null || selectorValue != null)
                {
#warning review error code
                    ast.AddError("Selector/Strategy already set");
                    return;
                }
                strategy = Ast.SelectorStrategy.XPath;
                selectorValue = xpathArgument.ValueString;
            }
            if (idArgument.ValueString != null)
            {
                if (strategy != null || selectorValue != null)
                {
#warning review error code
                    ast.AddError("Selector/Strategy already set");
                    return;
                }
                strategy = Ast.SelectorStrategy.Id;
                selectorValue = idArgument.ValueString;
            }
            if (cssArgument.ValueString != null)
            {
                if (strategy != null || selectorValue != null)
                {
#warning review error code
                    ast.AddError("Selector/Strategy already set");
                    return;
                }
                strategy = Ast.SelectorStrategy.Css;
                selectorValue = cssArgument.ValueString;
            }

#warning todo sub-read selector vars

            Ast.AstControlDeclaration control = new Ast.AstControlDeclaration
            {
                SourceCode = line,
                
                Name = nameArgument.ValueString,
                FriendlyName = friendlyNameArgument.ValueString,
                Strategy = strategy.Value,
                Selector = selectorValue,
                Description = line.CommentBlock
            };

            ast.AddControlDeclaration(control);
        }


        private Xandevelop.Wigwam.Ast.SelectorStrategy Map(string s)
        {
            switch (s)
            {
                case "xpath": return Ast.SelectorStrategy.XPath;
                case "id": return Ast.SelectorStrategy.Id;
                case "css": return Ast.SelectorStrategy.Css;
                default: throw new Exception();
            }
        }
    }

}
