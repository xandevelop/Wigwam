using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Compiler
{
    public interface IFileReader
    {
        string ReadAllText(string path);
    }
    public class FileReader : IFileReader
    {
        public string ReadAllText(string path) => System.IO.File.ReadAllText(path);
    }
}
