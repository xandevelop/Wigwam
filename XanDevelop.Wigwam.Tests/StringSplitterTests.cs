using NUnit.Framework;
using Xandevelop.Wigwam.Compiler.Scanners;
using Xandevelop.Wigwam.Compiler.Extensions;

namespace XanDevelop.Wigwam.Tests
{
    public class StringSplitterTests
    {
#warning todo split into proper tests
        [Test]
        public void Test()
        {
            char[] separators = new char[] { '=', ':' };

            var a = "abc".SplitWithEscape(separators);  //exp abc
            var b = "abc=def".SplitWithEscape(separators); // abc    def 
            var c = "abc\\=def=ghi".SplitWithEscape(separators); //  abc=def     ghi
            var d = "abc\\\\=def".SplitWithEscape(separators); // abc\\     def

            
            var e = "abc:def".SplitWithEscape(separators);  // abc    def
            var f = "abc\\:def:ghi".SplitWithEscape(separators); // abc:def    ghi
            var g = "abc\\\\:def".SplitWithEscape(separators);  // abc\\    def


            var h = "abc=def=xyz".SplitWithEscape(separators, 2);      // abc    def=xyz
            var i = "abc=def:xyz".SplitWithEscape(separators, 2);      // abc    def:xyz
            var j = "abc\\=def=ghi:q".SplitWithEscape(separators, 2);  // abc=def   ghi:q
            var k = "abc\\\\=def:q:q".SplitWithEscape(separators, 2);  // abc\\    def:q:q
            var l = "abc:def:q:q".SplitWithEscape(separators, 2);      // abc      def:q:q
            var m = "abc\\:def:ghi:q=q".SplitWithEscape(separators, 2);// abc:def     ghi:q=q
            var n = "abc\\\\:def:q:q".SplitWithEscape(separators, 2);  // abc\\      def:q:q

            var x = "abc".SplitWithEscape(separators, 2);  //exp abc

            ;
        }

    }
}
