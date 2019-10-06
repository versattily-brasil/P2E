﻿// -----------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class StringExtensions
    {
        public static bool MatchesAny(this string source, params string[] comparisons)
        {
            return comparisons.Any(s => s == source);
        }

        public static bool CaseInsensitiveCompare(this string str1, string str2)
        {
            return string.Compare(str1, str2, true) == 0;
        }

        public static bool ToBool(this string value)
        {
            if (value == null)
            {
                return false;
            }

            return !MatchesAny(value.ToLower(), "", "no", "false", "off", "0", null);
        }

        public static int ToInt(this string s)
        {
            int n;
            if (!int.TryParse(s, out n))
            {
                return 0;
            }

            return n;
        }

        public static long ToLong(this string s)
        {
            long n;
            if (!long.TryParse(s, out n))
            {
                return 0;
            }

            return n;
        }

        public static double ToDouble(this string s)
        {
            double n;
            if (!double.TryParse(s, out n))
            {
                return 0;
            }

            return n;
        }

        public static double ToDecimal(this string s)
        {
            double n;
            if (!double.TryParse(s, out n))
            {
                return 0;
            }

            return n;
        }

        public static string ShortenTo(this string str, int length)
        {
            return ShortenTo(str, length, false);
        }

        public static string ShortenTo(this string str, int length, bool ellipsis)
        {
            if (str.Length > length)
            {
                str = str.Substring(0, length);
                if (ellipsis)
                {
                    str += "&hellip;";
                }
            }
            return str;
        }

        public static List<string> Split(this string delimitedList, char delimiter, bool trimValues, bool stripDuplicates, bool caseSensitiveDuplicateMatch)
        {
            if (delimitedList == null)
            {
                return new List<string>();
            }

            StringUtil.LowerCaseComparer lcc = new StringUtil.LowerCaseComparer();
            List<string> list = new List<string>();
            string[] arr = delimitedList.Split(delimiter);
            for (int i = 0; i < arr.Length; i++)
            {
                string val = trimValues ? arr[i].Trim() : arr[i];
                if (val.Length > 0)
                {
                    if (stripDuplicates)
                    {
                        if (caseSensitiveDuplicateMatch)
                        {
                            if (!list.Contains(val))
                            {
                                list.Add(val);
                            }
                        }
                        else if (!list.Contains(val, lcc))
                        {
                            list.Add(val);
                        }
                    }
                    else
                    {
                        list.Add(val);
                    }
                }
            }
            return list;
        }

        public static List<string> SplitLines(this string listWithOnePerLine, bool trimValues, bool stripDuplicates, bool caseSensitiveDuplicateMatch)
        {
            if (listWithOnePerLine == null)
            {
                return new List<string>();
            }

            StringUtil.LowerCaseComparer lcc = new StringUtil.LowerCaseComparer();
            List<string> list = new List<string>();
            using (System.IO.StringReader reader = new System.IO.StringReader(listWithOnePerLine))
            {
                string val = reader.ReadLine();
                while (val != null)
                {
                    if (trimValues)
                    {
                        val = val.Trim();
                    }

                    if (val.Length > 0 || !trimValues)
                    {
                        if (stripDuplicates)
                        {
                            if (caseSensitiveDuplicateMatch)
                            {
                                if (!list.Contains(val))
                                {
                                    list.Add(val);
                                }
                            }
                            else if (!list.Contains(val, lcc))
                            {
                                list.Add(val);
                            }
                        }
                        else
                        {
                            list.Add(val);
                        }
                    }
                    val = reader.ReadLine();
                }
            }
            return list;
        }
    }
}