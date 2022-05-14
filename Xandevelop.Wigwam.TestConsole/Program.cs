using System;
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
            c.FileReader = new MockFileReader(@"some error
#a comment
test | hello
click | hello");

            var ast = c.Compile("");

        }
    }

    class MockFileReader : IFileReader
    {
        private string Txt;
        public MockFileReader(string txt)
        {
            Txt = txt;
        }
        public string ReadAllText(string path)
        {
            return Txt;
        }
    }
}
