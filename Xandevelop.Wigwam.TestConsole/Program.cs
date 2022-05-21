﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Compiler;

namespace Xandevelop.Wigwam.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // Console app to test things as we go
            var c = Compiler.Compiler.DefaultCompiler();
            var fileReader = 
             new MockFileReader("default", @"some error
#a comment
test | hello
click | hello");

            fileReader.AddFile(@"C:\Tests\MyTest.tpp", MyTestContent);
            fileReader.AddFile(@"C:\Tests\Include1.tpp", Include1Content);
            fileReader.AddFile(@"C:\Tests\Include2.tpp", Include2Content);
            fileReader.AddFile(@"C:\Tests\Extras\Include3.tpp", Include3Content);

            c.FileReader = fileReader;
            var ast = c.Compile(@"C:\Tests\MyTest.tpp");

        }

        static string MyTestContent = @"
include | C:\Tests\Include1.tpp
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