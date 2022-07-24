using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xandevelop.Wigwam.Ast;
using Xandevelop.Wigwam.Compiler;

namespace XanDevelop.Wigwam.Tests
{
    public class IntegrationTests
    {
        [TestCaseSource(nameof(TestCases))]
        public void GeneralTest(TestCase testCase)
        {
            
            var fileReader = new FileReader();
            var compiler = Compiler.DefaultCompiler();
            compiler.FileReader = fileReader;
            var output = compiler.Compile(testCase.FileName);

            StringBuilder actual = new StringBuilder();
            foreach(var err in output.compileErrors)
            {
                string file = new System.IO.FileInfo(err.SourceFile).Name;
                actual.AppendLine($"Error {err.SourceLineNumber} {err.SourceLine} {file} {err.MessageType} {err.Text}");
            }

            var programVisitor = new TestVisitor(actual);

            programVisitor.VisitBreadthFirst(output.ast);
            string actString = actual.ToString().TrimEnd('\r', '\n');
            string expString = testCase.Expect.TrimEnd('\r', '\n');

            Assert.AreEqual(expString, actString);
        }

         

        public static IEnumerable TestCases()
        {
            var result = new List<TestCaseData>();
            var files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + @"/TestCases", "*", System.IO.SearchOption.AllDirectories);

            foreach (var f in files.Where(x => !x.Contains(".expected")))
            {
                var exp = f + ".expected";
                if (!System.IO.File.Exists(exp)) throw new System.Exception("Expected doesn't exist");

                var tcd = new TestCaseData(new TestCase
                {
                    FileName = f,
                    Expect = System.IO.File.ReadAllText(exp)
                }).SetName("Test: " + new System.IO.FileInfo(f).Name);
                result.Add(tcd);
            }

            if (result.Count == 0) throw new Exception();

            //result.Add(new TestCaseData (new TestCase { FileName = "Default", Expect = "" }));
            return result;
        }

        
        public class TestCase
        {
            public string FileName { get; set; }
            public string Expect { get; set; }
        }

        public class TestVisitor : ProgramVisitorBase
        {
            StringBuilder Result;

            public TestVisitor(StringBuilder sb) : base()
            {
                Result = sb;
            }

            protected override void Visitor_Command(object sender, AstCommand e)
            {
                Result.AppendLine($"    Command | command={e.Command} | target={e.Target} | value={e.Value}");
            }

            protected override void Visitor_CommandDefinition(object sender, AstCommandDefinition e)
            {
                if (e.SourceFile == "(no source file)")
                {
                    // Built in command - don't emit
                }
                else
                {
                    Result.AppendLine($"Command Definition {e.Name}");
                }
            }

            protected override void Visitor_Control(object sender, AstControlDeclaration e)
            {
                throw new NotImplementedException();
            }

            protected override void Visitor_EndFunction(object sender, AstFunction e)
            {
                Result.AppendLine($"End Function {e.Name}\r\n");
            }

            protected override void Visitor_EndFunctionCall(object sender, AstFunctionCall e)
            {
                Result.AppendLine($"    End Func Call {e.Function.Name}");
            }

            protected override void Visitor_EndTest(object sender, AstTest e)
            {
                Result.AppendLine($"End Test {e.Name}\r\n");
            }

            protected override void Visitor_StartFunction(object sender, AstFunction e)
            {
                Result.AppendLine($"Start Function {e.Name}");
                foreach(var p in e.PreConditions)
                {
                    Result.AppendLine($"PRE: {p.Variable} {p.Comparison} {p.Value}");
                }
            }

            protected override void Visitor_StartFunctionCall(object sender, AstFunctionCall e)
            {
                Result.AppendLine($"    Start Func Call {e.Function.Name}");
            }

            protected override void Visitor_StartTest(object sender, AstTest e)
            {
                Result.AppendLine($"Start Test {e.Name}");
            }
        }
    }
}
