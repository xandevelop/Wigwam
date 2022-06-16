using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepEqual.Syntax;
using NUnit.Framework;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler;


namespace XanDevelop.Wigwam.Tests
{
    [TestFixture]
    public class CompilerTests
    {
        #region Test Helpers

        private (AstProgram ast, IEnumerable<CompileMessage> compileErrors) RunSingleFileScript(string script)
        {
            var fileReader = new MockFileReader("default", script);
            var compiler = Compiler.DefaultCompiler();
            compiler.FileReader = fileReader;
            return compiler.Compile("default");
        }

        private IAstStatement BuildCommand(string command, string target = null, string value = null, string description = null)
        {
            return new AstCommand
            {
                Command = command,
                Description = description,
                Target = target,
                Value = value
            };
        }
        private IAstStatement Echo(string target, string description = null)
        {
            return new AstCommand
            {
                Command = "echo",
                Description = description,
                Target = target,
            };
        }
        private IAstStatement Call(AstFunction func_say_hello, List<AstArgument> arguments = null, string description = null)
        {
            if (arguments == null) arguments = new List<AstArgument>();

            return new AstFunctionCall
            {
                Function = func_say_hello,
                Arguments = arguments,
                Description = description,
            };
        }

        private void ExecuteTest(string script, AstProgram expectedProgram, IEnumerable<CompileMessage> expectedCompileErrors, bool ignoreSourceTracking = true)
        {
            var actual = RunSingleFileScript(script);
            var expected = (expectedProgram, expectedCompileErrors);

            var assertProg = expectedProgram.WithDeepEqual(actual.ast)
                .IgnoreProperty(x => x.Name == "Method" /*&& x.DeclaringType == typeof(AstCommand)*/); // Avoid infinite recursion from things tracking their parents

            if(ignoreSourceTracking)
            {
                assertProg = assertProg.IgnoreProperty(x => x.Name == "SourceLine")
                                       .IgnoreProperty(x => x.Name == "SourceFile")
                                       .IgnoreProperty(x => x.Name == "SourceLineNumber");
            }

#warning assert error list todo

            assertProg.Assert();
            
        }

        #endregion

        [Test]
        public void TestCanDefineSimpleTest_WithSourceTracking()
        {
            var script = @"
test | hello world
echo | hello
";
            var expectedProgram = new AstProgram
            {
                SourceFile = "default",
                Tests = new List<AstTest>
                {
                    new AstTest
                    {
                        Name = "hello world",
                        Description = null,
                        Statements = new List<IAstStatement>
                                {
                                    new AstCommand
                                    {
                                        Command = "echo",
                                        Target = "hello",
                                        SourceFile = "default",
                                        SourceLine = "echo | hello",
                                        SourceLineNumber = 3
                                    }
                                },
                        SourceFile = "default",
                        SourceLine = "test | hello world",
                        SourceLineNumber = 2
                    }
                }
            };

            var expectedErrors = new List<CompileMessage>(); // No errors

            ExecuteTest(script, expectedProgram, expectedErrors, false);
            
        }

        

        [Test]
        public void TestCanDefineSimpleTest()
        {
            var script = @"
test | hello world
echo | hello
";
            var expectedProgram = new AstProgram
            {
                Tests = new List<AstTest>
                {
                    new AstTest
                    {
                        Name = "hello world",
                        Statements = new List<IAstStatement>
                        {
                            Echo("hello")
                        }
                    }
                }
            };

            var expectedErrors = new List<CompileMessage>(); // No errors

            ExecuteTest(script, expectedProgram, expectedErrors);
            
        }

        [Test]
        public void CanDefineFunction()
        {
            var script = @"
func | say hello
echo | hello
";
            var expectedProgram = new AstProgram
            {
                Functions = new List<AstFunction>
                {
                    new AstFunction
                    {
                        Name = "say hello",
                        Statements = new List<IAstStatement>
                        {
                            Echo("hello")
                        }
                    }
                }
            };

            var expectedErrors = new List<CompileMessage>(); // No errors

            ExecuteTest(script, expectedProgram, expectedErrors);

        }

        [Test]
        public void TestCanCallFunction_NoParameters_NoConditions()
        {
            var script = @"
test | hello world
say hello

func | say hello
echo | hello
";

            var func_say_hello = new AstFunction
            {
                Name = "say hello",
                Statements = new List<IAstStatement>
                {
                    Echo("hello")
                }
            };
            var test_hello_world = new AstTest
            {
                Name = "hello world",
                Statements = new List<IAstStatement>
                {
                    Call(func_say_hello)
                }
            };

            var expectedProgram = new AstProgram
            {
                Functions = new List<AstFunction> { func_say_hello },
                Tests = new List<AstTest> { test_hello_world }
            };

            var expectedErrors = new List<CompileMessage>(); // No errors

            ExecuteTest(script, expectedProgram, expectedErrors);

        }

        

        public void TestIndirectPreConditions()
        {
            var script = @"
test | hello
set pre a
call f indirect

test | hello
set pre b
call f indirect

func | set pre a
post | a | a

# virtually duplicated
func | call f indirect
call f

func | call f
pre | a | a

func | call f
pre | a | b

";
            var compilerResult = RunSingleFileScript(script);
            ;

#warning todo assertions
        }
    }
}
