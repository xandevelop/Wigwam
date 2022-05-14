using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xandevelop.Wigwam.Compiler.Scanners
{
    public class FileScanner
    {
        // Read file lines, remove whitespace, remove comments, work out what bits are commands and what bits aren't.
        public List<Line> ReadLines(string sourceFileName, string fileContent)
        {
            var lineStrings = fileContent.Split('\n').ToList();

            List<Line> result = new List<Line>();
            int lineNumber = 1;
            foreach (var line in lineStrings)
            {
                Line curLine = new Line
                {
                    SourceFile = sourceFileName,
                    SourceLine = line,
                    SourceLineNumber = lineNumber
                };

                string lineTrimmed = line.Trim();

                if (!String.IsNullOrEmpty(lineTrimmed) && !lineTrimmed.StartsWith("#"))
                {
                    string lineWithoutComment;

                    var partsLineAndComment = lineTrimmed.Split(new[] { '#' }, 2).ToList();
                    switch (partsLineAndComment.Count)
                    {
                        case 1: lineWithoutComment = lineTrimmed; break;
                        case 2: lineWithoutComment = partsLineAndComment[0]; curLine.CommentBlock = partsLineAndComment[1].Trim(); break;
                        default: throw new Exception("Line part length invalid");
                    }

                    var partsWithoutComment = lineWithoutComment.Split('|').Select(x => x.Trim()).ToList();
                    curLine.Command = partsWithoutComment.First();
                    curLine.Blocks = partsWithoutComment.Skip(1).ToList();

                    result.Add(curLine);
                }

                lineNumber++;
            }

            return result;
        }

    }
}
