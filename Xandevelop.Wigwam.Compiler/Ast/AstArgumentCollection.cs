using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

namespace Xandevelop.Wigwam.Ast
{
    // Use to replace anywhere we use List<AstArgument>
    public class AstArgumentCollection
    {
        public List<AstArgument> Collection { get; set; } = new List<AstArgument>();

        public string ToDebugString()
        {
            return "todo";
        }

        public AstArgument this[string name]
        {
            get
            {
                return Collection.FirstOrDefault(x => x.Name.ToLower().Trim() == name.ToLower().Trim());
            }
        }

        public int Count => Collection.Count;

        public void Add(AstArgument a) => Collection.Add(a);

        public static implicit operator AstArgumentCollection(List<AstArgument> list)
        {
            return new AstArgumentCollection { Collection = list };
        }
    }
}
