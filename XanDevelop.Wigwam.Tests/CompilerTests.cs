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
                .IgnoreProperty(x => x.Name == "Method" /*&& x.DeclaringType == typeof(AstCommand)*/) // Avoid infinite recursion from things tracking their parents
                .IgnoreProperty(x => x.Name == "SourceLineForPatchup" || x.Name == "ConditionsWhenCalled" || x.Name == "ConditionsWhenCompiled"); // Some properties are for internal use only
            if (ignoreSourceTracking)
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

        
        [Test]
        public void TestIndirectPreConditions1()
        {
            var script = @"
test | i can log in
open login page
submit form

test | i can email admin
open contact page
submit form

func | open login page
echo | login page opened
post | page | login

func | open contact page
echo | contact page opened
post | page | contact

func | submit form
echo | submitting form
click submit

func | click submit
pre | page | login
echo | clicked submit on login page

func | click submit
pre | page | contact
echo | clicked submit on contact page
";
            var func_click_submit_login = new AstFunction
            {
                Name = "click submit",
                Statements = new List<IAstStatement>
                {
                    Echo("clicked submit on login page")
                },
                PreConditions = new List<AstPreCondition>
                {
                    new AstPreCondition { Variable = "page", Value="login", Comparison=PreConditionComparisonType.Equals }
                }
            };
            var func_click_submit_contact = new AstFunction
            {
                Name = "click submit",
                Statements = new List<IAstStatement>
                {
                    Echo("clicked submit on contact page")
                },
                PreConditions = new List<AstPreCondition>
                {
                    new AstPreCondition { Variable = "page", Value="contact", Comparison=PreConditionComparisonType.Equals }
                }
            };

            var func_submit_form_1 = new AstFunction
            {
                Name = "submit form",
                Statements = new List<IAstStatement>
                {
                    Echo("submitting form"),
                    Call(func_click_submit_login)
                },
            };
            var func_submit_form_2 = new AstFunction
            {
                Name = "submit form",
                Statements = new List<IAstStatement>
                {
                    Echo("submitting form"),
                    Call(func_click_submit_contact)
                },
                PreConditions = new List<AstPreCondition>
                { new AstPreCondition { Variable = "page", Value = "contact" } }
            };

            var func_open_contact_page = new AstFunction
            {
                Name = "open contact page",
                Statements = new List<IAstStatement>
                {
                    Echo("contact page opened")
                },
                PostConditions = new List<AstPostCondition>
                { new AstPostCondition { Variable = "page", Value = "contact" } }
            };
            var func_open_login_page = new AstFunction
            {
                Name = "open login page",
                Statements = new List<IAstStatement>
                {
                    Echo("login page opened")
                },
                PostConditions = new List<AstPostCondition>
                { new AstPostCondition { Variable = "page", Value = "login" } }
            };



            var test_i_can_login = new AstTest
            {
                Name = "i can log in",
                Statements = new List<IAstStatement>
                {
                    Call(func_open_login_page),
                    Call(func_submit_form_1)
                }
            };
            var test_i_can_email_admin = new AstTest
            {
                Name = "i can email admin",
                Statements = new List<IAstStatement>
                {
                    Call(func_open_contact_page),
                    Call(func_submit_form_2)
                }
            };

            var expectedProgram = new AstProgram
            {
                Functions = new List<AstFunction> { func_click_submit_login, func_click_submit_contact, func_submit_form_1, func_submit_form_2, func_open_contact_page, func_open_login_page },
                Tests = new List<AstTest> { test_i_can_login, test_i_can_email_admin }
            };
            var expectedErrors = new List<CompileMessage>();

            ExecuteTest(script, expectedProgram, expectedErrors);
        }
    }
}
