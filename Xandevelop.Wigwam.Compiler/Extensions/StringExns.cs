﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Xandevelop.Wigwam.Compiler.Extensions
{
    // For taking an arg or parameter and splitting into substrings
    // : and = both split the string, \: and \= are literals and don't split the string
    
    public static class StringExns
    {
        
        private static bool IsEscapeChar(char c)
        {
            return c == '\\';
        }

        private static bool IsDelimiterChar(char c, char[] separator)
        {
            return separator.Contains(c);
        }

        /// <summary>
        /// Split a string similar to String.Split but don't split escaped separators.
        /// </summary> 
        public static string[] SplitWithEscape(this string source, char[] separator, int? count = null)
        {
            // Modified from https://coding.abel.nu/2016/06/string-split-and-join-with-escaping/
            var result = new List<string>();

            int segmentStart = 0;
            for (int i = 0; i < source.Length; i++)
            {
                bool readEscapeChar = false;
                if (IsEscapeChar(source[i]))
                {
                    readEscapeChar = true;
                    i++;
                }

                if (!readEscapeChar && IsDelimiterChar(source[i], separator))
                {
                    result.Add(UnEscapeString(
                      source.Substring(segmentStart, i - segmentStart), separator));
                    segmentStart = i + 1;

                    if(result.Count >= count.GetValueOrDefault(Int32.MaxValue)-1)
                    {
                        result.Add(source.Substring(segmentStart, source.Length - segmentStart));
                        break;
                    }
                }

                if (i == source.Length - 1)
                {
                    result.Add(UnEscapeString(source.Substring(segmentStart), separator));

                    
                }
            }

            return result.ToArray();
        }

        private static string UnEscapeString(string src, char[] separator)
        {
            string result = src.Replace("\\\\", "\\");

            foreach(var d in separator)
            {
                result = result.Replace("\\" + d, d.ToString());
            }
            return result;
        }


        
        public static (string Part0, string Part1) Split2(this string s, params char[] splitChars)
        {
            var parts = s.SplitWithEscape(splitChars, 2).ToList();
            switch (parts.Count)
            {
                case 0: return (null, null);
                case 1: return (parts[0], null);
                case 2: return (parts[0], parts[1]);
                default: throw new Exception("Can't happen");
            }
        }

        
    }
}
