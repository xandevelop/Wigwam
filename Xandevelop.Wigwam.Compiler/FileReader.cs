using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Compiler
{
    public interface IFileReader
    {
        string ReadAllText(string path);
        bool FileExists(string path);
        string BuildAbsPath(string originAbsPath, string relPath);
    }
    public class FileReader : IFileReader
    {
        public virtual string ReadAllText(string path) => System.IO.File.ReadAllText(path);

        public virtual bool FileExists(string path) => System.IO.File.Exists(path);
        
        public virtual string BuildAbsPath(string originAbsPath, string relPath)
        {
            // If relpath is already absolute, just return it
            if (FileExists(relPath))
            {
                return relPath;
            }
            else
            {
                string originDir = new FileInfo(originAbsPath).DirectoryName;
                var result = Path.Combine(originDir, relPath);

                // Use backslash instead of forward slash:
                // 1. when we compare paths C:\A\B\C.test is the same as C:/A/B/C.test as far as reprocessing or not is concerned
                // 2. backslash is normally used for absolute directories on Windows (we're not supporting unix etc right now)
                // 3. forward slash indicates this might be a relative path or a URL, but it's not.
                result = result.Replace("/", "\\");

                if (FileExists(result)) return result;
                else return null;
            }
        }
    }
}
