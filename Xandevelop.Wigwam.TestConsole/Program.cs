using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Compiler;
using XanDevelop.Wigwam.Tests;

namespace Xandevelop.Wigwam.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //new CompilerTests().TestIndirectPreConditions1();
            //return;

            // Console app to test things as we go
            var c = Compiler.Compiler.DefaultCompiler();
            var fileReader =
                new MockFileReader("default", @"
test | i can log in
open login page
submit form

test | i can email admin
open contact page
submit form

test | i can email and reuse func
open contact page
submit form

# x1
func | open login page
echo | login page opened
post | page | login

# x1
func | open contact page
echo | contact page opened
post | page | contact

# Should duplicate x2
func | submit form
echo | submitting form
click submit

# x1
func | click submit
pre | page | login
echo | clicked submit on login page

# x1
func | click submit
pre | page | contact
echo | clicked submit on contact page

");

            #region
            //             new MockFileReader("default", @"
            //test | hello
            //echo | first
            //say hello | x:1
            //echo | second

            //func | say hello | x
            //echo | hello

            //func | say hello | y
            //echo | hello v2
            //");
            

            fileReader.AddFile(@"C:\Tests\MyTest.tpp", MyTestContent);
            fileReader.AddFile(@"C:\Tests\Include1.tpp", Include1Content);
            fileReader.AddFile(@"C:\Tests\Include2.tpp", Include2Content);
            fileReader.AddFile(@"C:\Tests\Extras\Include3.tpp", Include3Content);

            c.FileReader = fileReader;
            //var ast = c.Compile(@"C:\Tests\MyTest.tpp");
            #endregion
            var ast = c.Compile("default");


            //var xx = ast.ast.Tests.First().Statements.Last();

        }

        static string MyTestContent = @"
include | C:\Tests\Include1.tpp
control | MyControl | id:abc
using | Extras/Include3.tpp
test | hello
click | hello";

        static string Include1Content = @"
include | Include2.tpp
test | goodbye
click | goodbye";

        static string Include2Content = @"
include | Include1.tpp
test | hello goodbye
click | hello goodbye";


        static string Include3Content = @"
import
test | hello number 3
click | hello goodbye";
    }

    class MockFileReader : FileReader
    {
        Dictionary<string, string> Files { get; } = new Dictionary<string, string>();

        public void AddFile(string fileName, string text)
        {
            Files.Add(fileName.ToLower(), text);
        }


        public MockFileReader() { }
        public MockFileReader(string fileName, string txt)
        {
            Files.Add(fileName.ToLower(), txt);
            
        }
        public override string ReadAllText(string path)
        {
            return Files[path.ToLower()];
        }

        public override bool FileExists(string path)
        {
            return Files.ContainsKey(path.ToLower());
        }
        
    }
}
