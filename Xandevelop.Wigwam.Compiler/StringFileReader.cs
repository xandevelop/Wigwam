﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Xandevelop.Wigwam.Compiler
{
    public class StringFileReader : FileReader
    {
        Dictionary<string, string> Files { get; } = new Dictionary<string, string>();

        public void AddFile(string fileName, string text)
        {
            Files.Add(fileName.ToLower(), text);
        }

        public StringFileReader() { }
        public StringFileReader(string fileName, string txt)
        {
            Files.Add(fileName.ToLower(), txt);
        }

        public override string ReadAllText(string path)
        {
            return Files[path.ToLower()];
        }

        public override bool FileExists(string path)
        {
            return Files.ContainsKey(path.ToLower());
        }

    }
    
    
}
