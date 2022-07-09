namespace Xandevelop.Wigwam.Ast
{ 
    public class AstPostCondition : AstBase {
        public override string ToDebugString()
        {
            throw new System.NotImplementedException();
        }
        public string Variable { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
    }
}
