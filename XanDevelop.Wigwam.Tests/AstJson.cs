using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xandevelop.Wigwam.Ast;

namespace XanDevelop.Wigwam.Tests
{
    class AstJson
    {
        public static string ToJson(AstProgram p)
        {
            return JsonConvert.SerializeObject(p, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}
