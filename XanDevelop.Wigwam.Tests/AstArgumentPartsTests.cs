using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xandevelop.Wigwam.Ast;

namespace XanDevelop.Wigwam.Tests
{
    class AstArgumentPartsTests
    {
        public void TestPartsSplit()
        {
            AstArgument argCut = new AstArgument();

            var x = argCut.Parts;

            argCut.Value = "hello there";
            argCut.Value = "hello ${user name}";
            argCut.Value = "hello ${user name} world";
            argCut.Value = "hello ${user name";
            argCut.Value = "hello \\${user name}";
            argCut.Value = "${user name} hello";
            argCut.Value = "hello ${} world";
            return;
        }
    }
}
