namespace Xandevelop.Wigwam.Ast
{
    public enum SelectorStrategy
    {
        Id,
        XPath,
        Css
    }
    
    public class AstControlDeclaration : AstBase
    {
        public override string ToDebugString()
        {
            throw new System.NotImplementedException();
        }
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string Selector { get; set; }
        public SelectorStrategy Strategy { get; set; }
        public string Description { get; set; }
    }

    
}
