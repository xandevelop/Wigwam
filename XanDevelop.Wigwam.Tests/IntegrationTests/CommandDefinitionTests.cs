using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Xandevelop.Wigwam.Compiler;
using static XanDevelop.Wigwam.Tests.IntegrationTestsFromFile;

namespace XanDevelop.Wigwam.Tests.IntegrationTests
{
    [TestFixture]
    public class CommandDefinitionTests
    {
        [TestCase("command | name=MyCustomCommand | arg0 name = a0 | arg1 name = a1 | arg2 name = a3", "Command Definition | name=MyCustomCommand")]
        public void CorrectlyDefinedCommand_ShouldCreateCommand(string code, string expect)
        {

            var fileReader = new MockFileReader("a", code);
            var compiler = Compiler.DefaultCompiler();
            compiler.BreakOnError = true;
            compiler.FileReader = fileReader;
            var output = compiler.Compile("a");

            StringBuilder actual = new StringBuilder();
            foreach (var err in output.compileErrors)
            {
                actual.AppendLine($"Error {err.SourceLineNumber} {err.SourceLine} {err.MessageType} {err.Text}");
            }

            var programVisitor = new TestVisitor(actual);

            programVisitor.VisitBreadthFirst(output.ast);
            string actString = actual.ToString().TrimEnd('\r', '\n');
            string expString = expect.TrimEnd('\r', '\n');

            Assert.AreEqual(expString, actString);
        }

    }
}
