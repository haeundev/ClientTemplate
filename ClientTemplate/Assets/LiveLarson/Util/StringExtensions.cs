using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = System.Random;

namespace LiveLarson.Util
{
    public static class StringExtensions
    {
        private static readonly string[] PluralExceptions = { "s", "sh", "ch", "x", "z" };

        private static readonly string[] Letters =
            { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
        
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static string ToPlural(this string text)
        {
            if (PluralExceptions.Any(p => text.EndsWith(p)))
                return text + "es";
            return text + "s";
        }

        public static string ToUppers(this string text)
        {
            var builder = new StringBuilder(text);
            for (var i = 0; i < builder.Length; ++i)
                builder[i] = char.ToUpper(builder[i]);
            return builder.ToString();
        }

        public static string ToLowers(this string text)
        {
            var builder = new StringBuilder(text);
            for (var i = 0; i < builder.Length; ++i)
                builder[i] = char.ToLower(builder[i]);
            return builder.ToString();
        }

        public static IEnumerable<int> AllIndexesOf(this string source, char character, int startIndex = 0)
        {
            for (var index = startIndex;; ++index)
            {
                index = source.IndexOf(character, index);
                if (index == -1)
                    yield break;
                yield return index;
            }
        }

        public static string[] SplitIntoTwo(this string sentence)
        {
            // Find the middle point index of the sentence
            var middleIndex = sentence.Length / 2;
            // Find the nearest space to the middle index from the left
            var leftSplitIndex = sentence.LastIndexOf(' ', middleIndex);
            // Find the nearest space to the middle index from the right
            var rightSplitIndex = sentence.IndexOf(' ', middleIndex);
            // Check which space is closer to the middle point and choose that as the split index
            var splitIndex = middleIndex - leftSplitIndex <= rightSplitIndex - middleIndex
                ? leftSplitIndex
                : rightSplitIndex;
            // Adjust the split point to avoid breaking a word
            if (splitIndex == -1 || splitIndex == 0 || splitIndex == sentence.Length - 1)
                // If there are no spaces or the split point is at the beginning/end of the sentence
                // then simply split at the middle point
                splitIndex = middleIndex;
            // Split the sentence into two parts
            var sentence1 = sentence.Substring(0, splitIndex).Trim();
            var sentence2 = sentence.Substring(splitIndex).Trim();

            // Return the result as an array
            return new[] { sentence1, sentence2 };
        }

        public static string RemoveAllSubstringAfterLastMark(string inString, char mark)
        {
            if (inString == "") return "";

            inString = inString.Trim();

            var lastIndex = inString.LastIndexOf(mark);

            return inString.Substring(0, lastIndex);
        }

        public static string GetSubstringUntilLastMark_Inclusive(string inString, char mark)
        {
            if (inString == "") return "";

            inString = inString.Trim();

            var lastIndex = inString.LastIndexOf(mark);

            return inString.Substring(0, lastIndex + 1);
        }

        public static string RemoveAllSubstringAfterFirstMark(string inString, char mark)
        {
            if (inString == "") return "";

            inString = inString.Trim();

            var lastIndex = inString.IndexOf(mark);

            return inString.Substring(0, lastIndex);
        }

        public static string RemoveAllSubstringBeforeFirstMark(string inString, char mark)
        {
            if (inString == "") return "";

            inString = inString.Trim();

            var firstIndex = inString.IndexOf(mark);

            return inString.Substring(firstIndex);
        }

        public static string RemoveAllSubstringBeforeFirstMark_Inclusive(string inString, char mark)
        {
            if (inString == "") return "";

            inString = inString.Trim();

            var firstIndex = inString.IndexOf(mark);

            return inString.Substring(firstIndex + 1);
        }

        public static bool IsArticle(this string str)
        {
            return str == "a" || str == "an" || str == "the";
        }

        public static string[] SeparateWords(string wordList)
        {
            return wordList.Split(' ');
        }

        public static string[] SeparateWords(string wordList, char[] seperator)
        {
            return wordList.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SeperateByString(string wordList, string seperator)
        {
            return wordList.Split(new[] { seperator }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string RemoveBetween(string inputString, string start, string end)
        {
            var result = GetSubstringBetween(inputString, start, end);

            return inputString.Replace(result, "");
        }

        public static string RemoveBetween_Inclusive(string inputString, string start, string end)
        {
            var startIndex = inputString.IndexOf(start);
            var endIndex = inputString.IndexOf(end);
            var result = inputString.Substring(startIndex, endIndex + 1 - startIndex);

            return inputString.Replace(result, "");
        }

        // ex: HAVE.NOT(s:i)(o:money)(x:now)  --->  HAVE.NOT
        public static string RemoveAllBetween_Inclusive(string inputString, string start, string end)
        {
            if (!inputString.Contains(start) || !inputString.Contains(end))
                return inputString;

            return RemoveAllBetween_Inclusive(RemoveBetween_Inclusive(inputString, start, end), start, end);
        }


        // ex: ab(cd (ef))(gh)ij  --->  abij
        public static string RemoveAllSubstringBetweenParentheses(string input)
        {
            var indexOpen = input.IndexOf('(');
            if (indexOpen < 0) return input;

            var indexClose = input.IndexOf(')');

            var start = indexOpen + 1;
            var count = indexClose - start;
            var indexOpen2 = input.IndexOf('(', start, count);

            if (indexOpen2 >= 0)
                return RemoveAllSubstringBetweenParentheses(RemoveInnerMostParentheses(input, start, indexClose));

            return RemoveAllSubstringBetweenParentheses(RemoveBetween(input, indexOpen, indexClose));
        }


        public static string RemoveInnerMostParentheses(string input, int start, int end)
        {
            var count = end - start;
            var indexOpen = input.IndexOf('(', start, count);

            if (indexOpen >= 0)
                RemoveInnerMostParentheses(input, indexOpen + 1, end);

            return RemoveBetween(input, start, end);
        }


        public static string RemoveBetween(string inputString, int start, int end)
        {
            var result = inputString.Substring(start, end + 1 - start);

            return inputString.Replace(result, "");
        }


        public static string ReplaceBetween_inclusive(string input, string newWord, string start, string end)
        {
            var old = GetSubstringBetween_Inclusive(input, start, end);
            var output = input.Replace(old, newWord);
            return output;
        }


        public static string RemoveEndCharacter(string input, char characterToRemove)
        {
            if (input.Length < 1)
                return input;

            var last = input[input.Length - 1];
            if (characterToRemove != last)
                return input;

            return input.Substring(0, input.Length - 1);
        }


        public static string GetSubstringBetween(string inputString, string start, string end)
        {
            if (!inputString.Contains(start) || !inputString.Contains(end))
                return string.Empty;

            var startIndex = inputString.IndexOf(start);
            var endIndex = inputString.IndexOf(end);
            var result = inputString.Substring(startIndex + 1, endIndex - startIndex - 1);
            return result.RemoveWhitespace();
        }

        public static string GetSubstringBetween_Inclusive(string inputString, string start, string end)
        {
            if (!inputString.Contains(start) || !inputString.Contains(end))
                return string.Empty;

            var startIndex = inputString.IndexOf(start);
            var endIndex = inputString.IndexOf(end);
            var result = inputString.Substring(startIndex, endIndex + 1 - startIndex);
            return result.RemoveWhitespace();
        }


        public static List<string> GetAllSubstringBetweenParenthesis(string input)
        {
            var output = new List<string>();

            while (input.Contains("("))
            {
                var content = input.Split('(', ')')[1];
                output.Add(content);
                input = input.Replace("(" + content + ")", "");
            }

            return output;
        }

        public static string[] SeperateStringBy(string wordList, params char[] delimiter)
        {
            var splited = wordList.Split(delimiter);

            for (var i = 0; i < splited.Length; i++) splited[i] = splited[i].Trim();

            return splited;
        }


        public static string GetSubstringBeforeIfHaveMark(string inputString, string mark)
        {
            if (inputString == "") return "";

            if (!inputString.Contains(mark)) return inputString;

            var markIndex = inputString.IndexOf(mark);

            if (markIndex == 0)
                return string.Empty;

            return inputString.Substring(0, markIndex).RemoveWhitespace();
        }


        public static string GetSubstringBefore(string inputString, string mark)
        {
            if (inputString == "") return "";

            if (!inputString.Contains(mark)) return string.Empty;

            inputString = inputString.Trim();

            var markIndex = inputString.IndexOf(mark);

            if (markIndex == 0)
                return string.Empty;

            return inputString.Substring(0, markIndex).RemoveWhitespace();
        }


        public static string GetSubstringAfter(string inputString, string mark)
        {
            if (inputString == "") return "";

            inputString = inputString.Trim();

            var lastIndex = inputString.LastIndexOf(mark) + 1;

            if (inputString.Length <= lastIndex)
                return string.Empty;

            return inputString.Substring(lastIndex);
        }


        public static string RemoveSubstring(string orignal, string sub)
        {
            return orignal.Replace(sub, "");
        }

        public static string RemoveSubstring2(this string orignal, string sub)
        {
            return orignal.Replace(sub, "");
        }


        public static string RemovePunctuations(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return
                input.Replace("?", "").Replace(",", "")
                    .Replace("!", "").Replace(".", "").Replace("'", "").Replace(";", "")
                    .Replace(":", "").Replace("\"", "").Replace("\n", "");
        }

        public static string RemovePunctuations_PreserveApostrophe(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return input
                .Replace("?", "").Replace(",", "")
                .Replace("!", "").Replace(".", "")
                //.Replace("'","").Replace(";","")
                .Replace(":", "").Replace("\"", "")
                .Replace("\n", "");
        }

        public static string RemoveNonAlphanumericCharacters(this string input)
        {
            var nonAlphanumerics = new Regex("[^a-zA-Z0-9 -]");
            return nonAlphanumerics.Replace(input, string.Empty);
        }

        public static List<string> GetLowerAlphanumericWords(this string input)
        {
            return input.RemoveNonAlphanumericCharacters().ToLower().Split(' ').ToList();
        }

        public static bool IsDemonstrative(this string str)
        {
            return str == "this" || str == "that" || str == "these" || str == "those";
        }

        public static string GetUntilNonAlphabet(this string input)
        {
            var charArr = new List<char>();
            foreach (var ch in input)
                if (char.IsLetter(ch))
                    charArr.Add(ch);
                else return new string(charArr.ToArray());
            return new string(charArr.ToArray());
        }

        public static string RemovePunctuationsForTestBot(this string input) //ex) "There is an A."  --> "There is an A"
        {
            if (string.IsNullOrEmpty(input)) return input;

            return
                input.Replace("?", "").Replace(",", "")
                    .Replace("!", "").Replace(".", "").Replace(";", "")
                    .Replace(":", "").Replace("\"", "").Replace("\n", "");
        }

        public static List<string> GetStringsBetweenAngleBrackets(this string input)
        {
            var regex = new Regex(@"(?<=<).*?(?=>)");
            return (from Match match in regex.Matches(input) select match.ToString()).ToList();
        }

        public static string GetStringsBetweenSquareBrackets(this string input)
        {
            var regex = new Regex(@"(?<=<).*?(?=>)");
            return (from Match match in regex.Matches(input) select match.ToString()).FirstOrDefault();
        }

        public static string MakeFirstLetterUppercase(this string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return input.First().ToString().ToUpper() + input.Substring(1);
        }


        public static string RemoveWhitespace(this string input)
        {
            return new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        public static List<(string, string)>
            GetIntentRequirementCallsInParenthesis(this string input) // "PIK(x:how)(o:kind)(o:of)"
        {
            var result = new List<(string, string)>();
            var calls = input.Split(new[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
            // string[] {("x", "how"), ("o", "kind"), ("o", "of")}
            foreach (var call in calls)
            {
                var split = call.Split(':');
                if (split.Length < 2)
                {
                    Debug.LogError($"IntentGroup Error : {input}");
                    continue;
                }

                result.Add((split[0], split[1]));
            }

            return result;
        }

        public static string RemoveSquareBrackets(this string input)
        {
            return new string(input.Where(c => c != '[' && c != ']').ToArray());
        }

        public static string RemoveCurlyBrackets(this string input)
        {
            return new string(input.Where(c => c != '{' && c != '}').ToArray());
        }

        public static string RemoveWhitespace2(this string str)
        {
            return string.Join("", str.Split(default(string[])
                , StringSplitOptions.RemoveEmptyEntries));
        }

        public static void PrintStringArray(string[] strings)
        {
            Debug.Log("PrintStringArray: ===============");
            foreach (var str in strings) Debug.Log(str + "\n");

            Debug.Log("=================================");
        }


        /// <summary>
        ///     Contains approximate string matching
        /// </summary>
        /// <summary>
        ///     Compute the distance between two strings.
        /// </summary>
        public static int LevenshteinDistance(string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0) return m;

            if (m == 0) return n;

            // Step 2
            for (var i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (var j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (var i = 1; i <= n; i++)
                //Step 4
            for (var j = 1; j <= m; j++)
            {
                // Step 5
                var cost = t[j - 1] == s[i - 1] ? 0 : 1;

                // Step 6
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }

            // Step 7
            return d[n, m];
        }


        public static string NumberToLetter(string val)
        {
            switch (val)
            {
                case "1": return "one";
                case "2": return "two";
                case "3": return "three";
                case "4": return "four";
                case "5": return "five";
                case "6": return "six";
                case "7": return "seven";
                case "8": return "eight";
                case "9": return "nine";
                case "10": return "ten";
                case "11": return "eleven";
                case "12": return "twelve";
                case "13": return "thirteen";
                case "14": return "fourteen";
                case "15": return "fifteen";
            }

            return val;
        }


        public static string LetterToNumber(string val)
        {
            switch (val)
            {
                case "one": return "1";
                case "two": return "2";
                case "three": return "3";
                case "four": return "4";
                case "five": return "5";
                case "six": return "6";
                case "seven": return "7";
                case "eight": return "8";
                case "nine": return "9";
                case "ten": return "10";
                case "eleven": return "11";
                case "twelve": return "12";
                case "thirteen": return "13";
                case "fourteen": return "14";
                case "fifteen": return "15";
            }

            return val;
        }

        public static int StringToInt(string val)
        {
            switch (val)
            {
                case "one": return 1;
                case "two": return 2;
                case "three": return 3;
                case "four": return 4;
                case "five": return 5;
                case "six": return 6;
                case "seven": return 7;
                case "eight": return 8;
                case "nine": return 9;
                case "ten": return 10;
                case "eleven": return 11;
                case "twelve": return 12;
                case "thirteen": return 13;
                case "fourteen": return 14;
                case "fifteen": return 15;
            }

            return 0;
        }

        public static string ReplaceAllLetterToNumberInJObject(string sentence)
        {
            foreach (var letter in Letters)
                sentence = sentence.Replace("\"" + letter + "\"",
                    "\"" + LetterToNumber(letter) + "\""); //ex: "four" --> "4" 
            //sentence = Regex.Replace(sentence, letter, LetterToNumber(letter));  //ex: "four" --> "4" // fixes NLG issue


            return sentence;
        }


        public static string RemoveDeterminers(string word)
        {
            if (!(word.Contains("a ") || word.Contains("an ") || word.Contains("the ")))
                return word;

            var s = "";
            if (word.Contains("an "))
                s = word.Substring(3, word.Length - 3);
            else if (word.Contains("a "))
                s = word.Substring(2, word.Length - 2);
            else if (word.Contains("the "))
                s = word.Substring(4, word.Length - 4);
            return s;
        }


        public static Dictionary<TKey, TValue> RandomSortDictionary<TKey, TValue>(
            this Dictionary<TKey, TValue> dictionary)
        {
            var r = new Random();
            return dictionary.OrderBy(x => r.Next())
                .ToDictionary(item => item.Key, item => item.Value);
        }


        public static string[] SeparateAllParenthesis(this string input, bool withParenthesis = true)
        {
            string[] ops = { };

            if (input == string.Empty)
                return ops;

            input = RemoveWhitespace(input);

            if (input.Contains(")("))
                ops = SeperateByString(input, ")(");
            else
                ops = new[] { input };

            for (var i = 0; i < ops.Length; i++)
            {
                ops[i] = RemoveSubstring(ops[i], "(");
                ops[i] = RemoveSubstring(ops[i], ")");
            }

            if (withParenthesis)
                for (var i = 0; i < ops.Length; i++)
                    ops[i] = "(" + ops[i] + ")";


            return ops;
        }


        public static bool IsNumber(this string input)
        {
            return int.TryParse(input, out var output);
        }

        public static string ConvertNonBreakingHyphen(this string input)
        {
            return input.Replace("-", "\u2011");
        }

        public static string ConvertNonBreakingWhitespace(this string input)
        {
            return input.Replace(" ", "\u00A0");
        }

        public static bool IsNullOrEmpty(this List<string> list)
        {
            if (list == default)
                return true;
            return list.Count == 0;
        }
    }
}