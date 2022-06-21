using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler.Parsers;

namespace Xandevelop.Wigwam.Compiler
{
    /*
     USAGE:
            programVisitor.CommandDefinition += (object sender, AstCommandDefinition d) => { Console.WriteLine(d.Name); };
            programVisitor.Control += (object sender, AstControlDeclaration c) => { Console.WriteLine(c.Name); };
            programVisitor.EndFunction += (object sender, AstFunction f) => { Console.WriteLine("End of func" + f.Name); };
            programVisitor.FunctionCall += (object sender, AstFunctionCall f) => { Console.WriteLine("Call " + f.Function.Name); };
            programVisitor.StartFunction += (object sender, AstFunction f) => { Console.WriteLine("Start func " + f.Name); };
            programVisitor.EndTest += (object sender, AstTest t) => { Console.WriteLine("*** End of test ***\n"); };
            programVisitor.StartTest += (object sender, AstTest t) => { Console.WriteLine("*** Start test \"" + t.Name + "\" ***"); };
     */


    public class ProgramVisitor
    {
        /// <summary>
        /// Visit the nodes in a program.  When a function call or similar is found, visit the function immediately.
        /// </summary>
        /// <param name="program"></param>
        /// <remarks>
        /// Use this for inlining languages like SIDE.
        /// </remarks>
        public void VisitDepthFirst(Ast.AstProgram program)
        {
            foreach (var x in program.Controls)
            {
                VisitControl(x);
            }
            foreach (var test in program.Tests)
            {
                VisitTest(test, true);
            }
        }

        /// <summary>
        /// Visit the nodes in a program.  When a function call or similar is found, do not visit the function. Functions are visited later.
        /// </summary>
        /// <param name="program"></param>
        /// <remarks>
        /// Use this for output to languages like C# where we don't want to have to jump backwards and forwards as we navigate a stack.
        /// </remarks>
        public void VisitBreadthFirst(Ast.AstProgram program)
        {
            foreach (var test in program.Tests)
            {
                VisitTest(test, false);
            }
            foreach (var function in program.Functions)
            {
                VisitFunction(function, false);
            }
            foreach (var x in program.CommandDefinitions)
            {
                VisitCommandDefinition(x);
            }
            foreach (var x in program.Controls)
            {
                VisitControl(x);
            }
        }



        public event EventHandler<Ast.AstTest> StartTest;
        public event EventHandler<Ast.AstTest> EndTest;

        public event EventHandler<Ast.AstFunction> StartFunction;
        public event EventHandler<Ast.AstFunction> EndFunction;

        public event EventHandler<Ast.AstCommand> Command;
        public event EventHandler<Ast.AstFunctionCall> FunctionCall;

        public event EventHandler<Ast.AstControlDeclaration> Control;
        public event EventHandler<Ast.AstCommandDefinition> CommandDefinition;

        private void VisitTest(Ast.AstTest test, bool deep)
        {
            StartTest?.Invoke(this, test);
            foreach (var s in test.Statements)
            {
                VisitStatement(s, deep);
            }
            EndTest?.Invoke(this, test);
        }
        private void VisitFunction(Ast.AstFunction function, bool deep)
        {
            StartFunction?.Invoke(this, function);

            foreach (var s in function.Statements)
            {
                VisitStatement(s, deep);
            }

            EndFunction?.Invoke(this, function);
        }

        private void VisitStatement(Ast.IAstStatement statement, bool deep)
        {
            VisitCommand(statement as Ast.AstCommand);
            VisitFunctionCall(statement as Ast.AstFunctionCall, deep);
            VisitFunctionCall(statement as AstFunctionCallNoContext, deep);
        }

        private void VisitCommand(Ast.AstCommand command)
        {
            if (command == null) return; // Cast check
            Command?.Invoke(this, command);

        }

        private void VisitFunctionCall(Ast.AstFunctionCall call, bool deep)
        {
            if (call == null) return; // Cast check
            FunctionCall?.Invoke(this, call);

            if (deep)
            {
                VisitFunction(call.Function, true);
            }
        }

        private void VisitFunctionCall(AstFunctionCallNoContext call, bool deep)
        {
            if (call == null) return; // Cast check

            if (call.Command != null)
            {
                Command?.Invoke(this, call.Command);
            }
            else
            {
                FunctionCall?.Invoke(this, new Ast.AstFunctionCall
                {
                    Arguments = call.ContextFreeArguments,
                    Function = call.Function,
                    Description = call.Description,
                    SourceFile = call.SourceFile,
                    SourceLine = call.SourceLine,
                    SourceLineNumber = call.SourceLineNumber
                });

                if (deep)
                {
                    VisitFunction(call.Function, true);
                }
            }
        }



        private void VisitControl(AstControlDeclaration x)
        {
            Control?.Invoke(this, x);
        }

        private void VisitCommandDefinition(AstCommandDefinition x)
        {
            CommandDefinition?.Invoke(this, x);
        }
    }
}
