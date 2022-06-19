namespace Xandevelop.Wigwam.Compiler.Parsers
{
    public interface ISecondPassParser
    {
        string Name { get; }
        int OrderNumber { get; }  // Low = processed sooner, High = processed later (will definitley happen, unlike first pass parsers)
        void Parse(AstBuilder ast);
    }
}
