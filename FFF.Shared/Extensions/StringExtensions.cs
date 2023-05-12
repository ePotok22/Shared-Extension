using FFF.Shared.Wrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FFF.Shared
{
    public static class StringExtensions
    {
        private static string _specialCharacter = @"\|!#$%&/()=?»«@£§€{}.-;'<>,^+*:~`";
        private static readonly char DefaultMaskCharacter = '*';
        private static readonly Regex _pattern = new Regex("[A-Z]{2}([A-Z0-9]){10}", RegexOptions.Compiled);
        private static readonly Regex _regExEmail = new Regex("[^\\s@]+@([^\\s@.,]+\\.)+[^\\s@.,]{2,}$");
        private static readonly Regex _removeSpacesRegex = new Regex("\\s+");
        private static readonly Regex _removeDoubledNewLineRegex = new Regex("(?:\\r\\n|\\r(?!\\n)|(?<!\\r)\\n){2,}");

        public static int[] AllIndexOf(this string str, string searchString, bool IsMatchCase)
        {
            List<int> tempList = new List<int>();
            int minIndex = str.IndexOf(searchString, IsMatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
            while (minIndex != -1)
            {
                tempList.Add(minIndex);
                minIndex = str.IndexOf(searchString, minIndex + searchString.Length, IsMatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
            }
            return tempList.ToArray();
        }

        public static DateTime ParseDateTime(this string value) =>
            DateTime.Parse(value, System.Globalization.CultureInfo.CurrentCulture);

        public static string MakePrintable(this string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var ch in value)
            {
                if (ch == '\n') sb.Append("\\n");
                else if (ch == '\r') sb.Append("\\r");
                else if (ch == '\t') sb.Append("\\t");
                else if (char.IsControl(ch)) sb.Append("#");
                else sb.Append(ch);
            }
            return sb.ToString();
        }

        public static string AppendIfNotEndWith(this string message, string wordForAppend)
        {
            if (string.IsNullOrEmpty(wordForAppend))
                throw new ArgumentException($"{nameof(wordForAppend)} is null or empty.", nameof(wordForAppend));

            if (message.EndsWith(wordForAppend) == false)
                return message + wordForAppend;
            else
                return message;
        }

        public static bool IsSpecialChar(this string input)
        {
            foreach (var item in _specialCharacter)
                if (input.Contains(item)) return true;
            return false;
        }

        public static string RemoveSpecialCharacter(this string input)
        {
            StringBuilder @string = new StringBuilder();
            char[] temp = _specialCharacter.ToCharArray();
            foreach (var item in input)
                if (!temp.Contains(item)) @string.Append(item);
            return @string.ToString();
        }

        public static string ReplaceSpecialCharacter(this string input, char character) =>
              ReplaceSpecialCharacter(input, character.ToString());

        public static string ReplaceSpecialCharacter(this string input, string text)
        {
            StringBuilder @string = new StringBuilder();
            char[] temp = _specialCharacter.ToCharArray();
            foreach (var item in input)
            {
                if (!temp.Contains(item)) @string.Append(item);
                else @string.Append(text);
            }

            return @string.ToString();
        }

        public static string SanitizeFilename(this string iInput)
        {
            string retVal = "";
            string replaceStr = "";

            // Invalid characters for filenames: \ / : * ? " < > |

            retVal = iInput.Replace("\\", replaceStr);
            retVal = retVal.Replace("/", replaceStr);
            retVal = retVal.Replace(":", replaceStr);
            retVal = retVal.Replace("*", replaceStr);
            retVal = retVal.Replace("?", replaceStr);
            retVal = retVal.Replace("\"", replaceStr);
            retVal = retVal.Replace("<", replaceStr);
            retVal = retVal.Replace(">", replaceStr);
            retVal = retVal.Replace("|", replaceStr);

            return retVal;
        }

        public static string ConvertEncoding(this string s, string fromEncoding, string toEncoding = "UTF-16")
        {
            Encoding sourceEncode = Encoding.GetEncoding(fromEncoding);
            Encoding targetEncode = Encoding.GetEncoding(toEncoding);
            byte[] inputBytes = targetEncode.GetBytes(s);
            byte[] outputBytes = Encoding.Convert(sourceEncode, targetEncode, inputBytes);
            return targetEncode.GetString(outputBytes).Replace("\0", "");
        }

        public static bool IsNullOrEmpty(this string value) =>
            string.IsNullOrEmpty(value);

        public static bool IsNullOrWhiteSpace(this string value) =>
            string.IsNullOrWhiteSpace(value);

        public static bool IsNullOrEmptyOrWhiteSpace(this string input) =>
            string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input);

        public static string IsInterned(this string value) =>
            string.IsInterned(value);

        public static bool IsQuote(this string s)
        {
            if (!string.IsNullOrEmpty(s))
                return s.Count(x => x == '\"') == 2 || (s.StartsWith("\"") && s.EndsWith("\""));
            else
                return false;
        }

        public static bool IsWildCardMatch(this string source, string pattern, char singleWildCard = '?', char multiWildCard = '*')
        {
            if (pattern == null)
                return false;

            if (pattern == "")
                return source == "";

            if (pattern == multiWildCard.ToString())
                return true;

            Stack<(int sourcePt, int patternPt)> stack = new Stack<(int, int)>(source.Length);

            stack.Push((0, 0));
            while (stack.Any())
            {
                var cc = stack.Pop();
                if (cc.sourcePt >= source.Length && cc.patternPt >= pattern.Length)
                    return true;

                if (cc.sourcePt >= source.Length)
                {
                    int ppt = cc.patternPt;
                    while (ppt < pattern.Length)
                    {
                        if (pattern[ppt] != multiWildCard)
                            break;
                        ppt += 1;
                    }
                    if (ppt >= pattern.Length)
                        return true;

                    continue;
                }

                if (cc.patternPt >= pattern.Length)
                    continue;

                char sc = source[cc.sourcePt];
                char pc = pattern[cc.patternPt];

                if (pc == singleWildCard || pc == sc)
                    stack.Push((cc.sourcePt + 1, cc.patternPt + 1));
                else if (pc == multiWildCard)
                {
                    int lastMulti = cc.patternPt;
                    while (lastMulti < pattern.Length && pattern[lastMulti] == multiWildCard)
                        lastMulti += 1;

                    for (int i = cc.sourcePt; i <= source.Length; i += 1)
                        stack.Push((i, lastMulti));

                    stack.Push((cc.sourcePt + 1, cc.patternPt + 1));
                }
            }

            return false;
        }

        public static bool IsEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                return _regExEmail.Match(email).Success && new MailAddress(email).Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string ToValidClassName(this string text, char badIdentifierReplacement = '_')
        {
            text = text?.Trim();
            if (string.IsNullOrEmpty(text))
                return badIdentifierReplacement.ToString();
            try
            {
                string source = text.Normalize(NormalizationForm.FormKC);
                if (!char.IsLetter(source[0]) && source[0] != '_' && badIdentifierReplacement != char.MinValue)
                    source = badIdentifierReplacement.ToString() + source;
                return new string(source.Select<char, char>((Func<char, char>)(c => !char.IsLetterOrDigit(c) && c != '_' ? badIdentifierReplacement : c)).Where<char>((Func<char, bool>)(c => c > char.MinValue)).ToArray<char>());
            }
            catch
            {
                return badIdentifierReplacement.ToString();
            }
        }

        public static string ToValidXMLName(this string text, char badIdentifierReplacement = '_')
        {
            text = text?.Trim();
            if (string.IsNullOrEmpty(text))
                return badIdentifierReplacement.ToString();
            try
            {
                return XmlConvert.VerifyName(text);
            }
            catch
            {
                return XmlConvert.EncodeName(text);
            }
        }

        public static string StripWhitespaces(this string text) =>
            text != null ? _removeSpacesRegex.Replace(text, "") : (string)null;

        public static string ReplaceWhitespacesWithDot(this string text) =>
            text != null ? _removeSpacesRegex.Replace(text, ".") : (string)null;

        public static string Unbox(this string text, char start, char end) =>
            string.IsNullOrWhiteSpace(text) || text.Length < 2 || (int)text[0] != (int)start || (int)text[text.Length - 1] != (int)end ? text : text.Substring(1, text.Length - 2);

        public static string WrapInDoubleQuotes(this string value) =>
            value == null ? (string)null : "\"" + value + "\"";

        public static string SubstringBefore(this string value, char delimiter)
        {
            if (value == null)
                return (string)null;
            int length = value.IndexOf(delimiter);
            return length >= 0 ? value.Substring(0, length) : value;
        }

        public static string TrimStartToDelimiter(this string input, char delimiter, out string trimmedValue)
        {
            if (input == null)
            {
                trimmedValue = (string)null;
                return (string)null;
            }
            int length = input.IndexOf(delimiter);
            if (length >= 0)
            {
                trimmedValue = input.Substring(0, length);
                return length != input.Length - 1 ? input.Substring(length + 1) : "";
            }
            trimmedValue = input;
            return string.Empty;
        }

        public static List<string> SplitByAndTrim(this string value, char delimiter) => value == null ? (List<string>)null : ((IEnumerable<string>)value.Split(delimiter)).Select<string, string>((Func<string, string>)(s => s.Trim())).ToList<string>();

        public static List<string> SplitByAndRemoveSpaces(this string value, char delimiter)
        {
            if (value == null)
                return (List<string>)null;
            return ((IEnumerable<string>)value.Split(new char[1]
            {
        delimiter
            }, StringSplitOptions.RemoveEmptyEntries)).Select<string, string>((Func<string, string>)(x => x.Replace(" ", string.Empty))).ToList<string>();
        }

        public static string RemoveDoubledNewLine(this string value) =>
            value != null ? _removeDoubledNewLineRegex.Replace(value, Environment.NewLine) : (string)null;

        public static string ToSanitizedValue(this string input, char[] invalidChars, char replacement = '_')
        {
            if (input == null || (invalidChars != null ? (!((IEnumerable<char>)invalidChars).Any<char>() ? 1 : 0) : 1) != 0)
                return input;
            StringBuilder stringBuilder = new StringBuilder(input);
            for (int index = 0; index < input.Length; ++index)
                stringBuilder[index] = ((IEnumerable<char>)invalidChars).Contains<char>(input[index]) ? replacement : input[index];
            return stringBuilder.ToString();
        }

        public static string ToCamelCase(this string str) =>
            !string.IsNullOrEmpty(str) && str.Length > 1 ? char.ToLowerInvariant(str[0]).ToString() + str.Substring(1) : str;

        /// <summary>
        /// Checks string object's value to array of string values
        /// </summary>
        /// <param name="stringValues">Array of string values to compare</param>
        /// <returns>Return true if any string value matches</returns>
        public static bool In(this string value, params string[] stringValues)
        {
            foreach (string otherValue in stringValues)
                if (string.Compare(value, otherValue) == 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Converts string to enum object
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="value">String value to convert</param>
        /// <returns>Returns enum object</returns>
        public static T ToEnum<T>(this string value)
            where T : struct
        {
            return (T)System.Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Returns characters from right of specified length
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="length">Max number of charaters to return</param>
        /// <returns>Returns string from right</returns>
        public static string Right(this string value, int length)
        {
            return value != null && value.Length > length ? value.Substring(value.Length - length) : value;
        }

        /// <summary>
        /// Returns characters from left of specified length
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="length">Max number of charaters to return</param>
        /// <returns>Returns string from left</returns>
        public static string Left(this string value, int length)
        {
            return value != null && value.Length > length ? value.Substring(0, length) : value;
        }

        /// <summary>
        ///  Replaces the format item in a specified System.String with the text equivalent
        ///  of the value of a specified System.Object instance.
        /// </summary>
        /// <param name="value">A composite format string</param>
        /// <param name="arg0">An System.Object to format</param>
        /// <returns>A copy of format in which the first format item has been replaced by the
        /// System.String equivalent of arg0</returns>
        public static string Format(this string value, object arg0)
        {
            return string.Format(value, arg0);
        }

        /// <summary>
        ///  Replaces the format item in a specified System.String with the text equivalent
        ///  of the value of a specified System.Object instance.
        /// </summary>
        /// <param name="value">A composite format string</param>
        /// <param name="args">An System.Object array containing zero or more objects to format.</param>
        /// <returns>A copy of format in which the format items have been replaced by the System.String
        /// equivalent of the corresponding instances of System.Object in args.</returns>
        public static string Format(this string value, params object[] args)
        {
            return string.Format(value, args);
        }

        /// <summary>
        /// Formats the string according to the specified mask
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="mask">The mask for formatting. Like "A##-##-T-###Z"</param>
        /// <returns>The formatted string</returns>
        public static string FormatWithMask(this string input, string mask)
        {
            if (input.IsNullOrEmpty()) return input;
            var output = string.Empty;
            var index = 0;
            foreach (var m in mask)
            {
                if (m == '#')
                {
                    if (index < input.Length)
                    {
                        output += input[index];
                        index++;
                    }
                }
                else
                    output += m;
            }
            return output;
        }

        public static bool IsNumeric(this string theValue)
        {
            long retNum;
            return long.TryParse(theValue, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out retNum);
        }

        public static string UcFirst(this string theString)
        {
            if (string.IsNullOrEmpty(theString))
            {
                return string.Empty;
            }

            char[] theChars = theString.ToCharArray();
            theChars[0] = char.ToUpper(theChars[0]);

            return new string(theChars);

        }

        public static bool IsDate(this string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                DateTime dt;
                return (DateTime.TryParse(input, out dt));
            }
            else
            {
                return false;
            }
        }

        public static T Parse<T>(this string value)
        {
            // Get default value for type so if string
            // is empty then we can return default value.
            T result = default(T);
            if (!string.IsNullOrEmpty(value))
            {
                // we are not going to handle exception here
                // if you need SafeParse then you should create
                // another method specially for that.
                TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
                result = (T)tc.ConvertFrom(value);
            }
            return result;
        }

        public static bool ContainsAny(this string theString, char[] characters)
        {
            foreach (char character in characters)
            {
                if (theString.Contains(character.ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        public static string ToProperCase(this string text)
        {
            System.Globalization.CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(text);
        }

        /// <summary>
        /// True se a string passada for um ISIN válido
        /// </summary>
        public static bool IsIsin(this string isin)
        {
            if (string.IsNullOrEmpty(isin))
            {
                return false;
            }
            if (!_pattern.IsMatch(isin))
            {
                return false;
            }

            var digits = new int[22];
            int index = 0;
            for (int i = 0; i < 11; i++)
            {
                char c = isin[i];
                if (c >= '0' && c <= '9')
                {
                    digits[index++] = c - '0';
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    int n = c - 'A' + 10;
                    int tens = n / 10;
                    if (tens != 0)
                    {
                        digits[index++] = tens;
                    }
                    digits[index++] = n % 10;
                }
                else
                {
                    // Not a digit or upper-case letter.
                    return false;
                }
            }
            int sum = 0;
            for (int i = 0; i < index; i++)
            {
                int digit = digits[index - 1 - i];
                if (i % 2 == 0)
                {
                    digit *= 2;
                }
                sum += digit / 10;
                sum += digit % 10;
            }

            int checkDigit = isin[11] - '0';
            if (checkDigit < 0 || checkDigit > 9)
            {
                // Not a digit.
                return false;
            }
            int tensComplement = (sum % 10 == 0) ? 0 : ((sum / 10) + 1) * 10 - sum;
            return checkDigit == tensComplement;
        }

        /// <summary>
        /// Converts the string representation of a Guid to its Guid
        /// equivalent. A return value indicates whether the operation
        /// succeeded.
        /// </summary>
        /// <param name="s">A string containing a Guid to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the Guid value equivalent to
        /// the Guid contained in <paramref name="s"/>, if the conversion
        /// succeeded, or <see cref="Guid.Empty"/> if the conversion failed.
        /// The conversion fails if the <paramref name="s"/> parameter is a
        /// <see langword="null" /> reference (<see langword="Nothing" /> in
        /// Visual Basic), or is not of the correct format.
        /// </param>
        /// <value>
        /// <see langword="true" /> if <paramref name="s"/> was converted
        /// successfully; otherwise, <see langword="false" />.
        /// </value>
        /// <exception cref="ArgumentNullException">
        ///        Thrown if <pararef name="s"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// Original code at https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=94072&wa=wsignin1.0#tabs
        ///
        /// </remarks>
        public static bool IsGuid(this string s)
        {
            if (s == null)
                throw new ArgumentNullException("Input");

            Regex format = new Regex(
                "^[A-Fa-f0-9]{32}$|" +
                "^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" +
                "^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2}, {0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$");
            Match match = format.Match(s);

            return match.Success;
        }

        /// <summary>
        /// Returns true if the string is non-null and at least the specified number of characters.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="length">The minimum length.</param>
        /// <returns>True if string is non-null and at least the length specified.</returns>
        /// <exception>throws ArgumentOutOfRangeException if length is not a non-negative number.</exception>
        public static bool IsLengthAtLeast(this string value, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length", length,
                                                        "The length must be a non-negative number.");
            }

            return value != null
                        ? value.Length >= length
                        : false;
        }

        /// <summary>
        /// Mask the source string with the mask char except for the last exposed digits.
        /// </summary>
        /// <param name="sourceValue">Original string to mask.</param>
        /// <param name="maskChar">The character to use to mask the source.</param>
        /// <param name="numExposed">Number of characters exposed in masked value.</param>
        /// <param name="style">The masking style to use (all characters or just alpha-nums).</param>
        /// <returns>The masked account number.</returns>
        public static string Mask(this string sourceValue, char maskChar, int numExposed, MaskStyle style)
        {
            var maskedString = sourceValue;

            if (sourceValue.IsLengthAtLeast(numExposed))
            {
                var builder = new StringBuilder(sourceValue.Length);
                int index = maskedString.Length - numExposed;

                if (style == MaskStyle.AlphaNumericOnly)
                {
                    CreateAlphaNumMask(builder, sourceValue, maskChar, index);
                }
                else
                {
                    builder.Append(maskChar, index);
                }

                builder.Append(sourceValue.Substring(index));
                maskedString = builder.ToString();
            }

            return maskedString;
        }

        /// <summary>
        /// Mask the source string with the mask char except for the last exposed digits.
        /// </summary>
        /// <param name="sourceValue">Original string to mask.</param>
        /// <param name="maskChar">The character to use to mask the source.</param>
        /// <param name="numExposed">Number of characters exposed in masked value.</param>
        /// <returns>The masked account number.</returns>
        public static string Mask(this string sourceValue, char maskChar, int numExposed)
        {
            return Mask(sourceValue, maskChar, numExposed, MaskStyle.All);
        }

        /// <summary>
        /// Mask the source string with the mask char.
        /// </summary>
        /// <param name="sourceValue">Original string to mask.</param>
        /// <param name="maskChar">The character to use to mask the source.</param>
        /// <returns>The masked account number.</returns>
        public static string Mask(this string sourceValue, char maskChar)
        {
            return Mask(sourceValue, maskChar, 0, MaskStyle.All);
        }

        /// <summary>
        /// Mask the source string with the default mask char except for the last exposed digits.
        /// </summary>
        /// <param name="sourceValue">Original string to mask.</param>
        /// <param name="numExposed">Number of characters exposed in masked value.</param>
        /// <returns>The masked account number.</returns>
        public static string Mask(this string sourceValue, int numExposed)
        {
            return Mask(sourceValue, DefaultMaskCharacter, numExposed, MaskStyle.All);
        }

        /// <summary>
        /// Mask the source string with the default mask char.
        /// </summary>
        /// <param name="sourceValue">Original string to mask.</param>
        /// <returns>The masked account number.</returns>
        public static string Mask(this string sourceValue)
        {
            return Mask(sourceValue, DefaultMaskCharacter, 0, MaskStyle.All);
        }

        /// <summary>
        /// Mask the source string with the mask char.
        /// </summary>
        /// <param name="sourceValue">Original string to mask.</param>
        /// <param name="maskChar">The character to use to mask the source.</param>
        /// <param name="style">The masking style to use (all characters or just alpha-nums).</param>
        /// <returns>The masked account number.</returns>
        public static string Mask(this string sourceValue, char maskChar, MaskStyle style)
        {
            return Mask(sourceValue, maskChar, 0, style);
        }

        /// <summary>
        /// Mask the source string with the default mask char except for the last exposed digits.
        /// </summary>
        /// <param name="sourceValue">Original string to mask.</param>
        /// <param name="numExposed">Number of characters exposed in masked value.</param>
        /// <param name="style">The masking style to use (all characters or just alpha-nums).</param>
        /// <returns>The masked account number.</returns>
        public static string Mask(this string sourceValue, int numExposed, MaskStyle style)
        {
            return Mask(sourceValue, DefaultMaskCharacter, numExposed, style);
        }

        /// <summary>
        /// Mask the source string with the default mask char.
        /// </summary>
        /// <param name="sourceValue">Original string to mask.</param>
        /// <param name="style">The masking style to use (all characters or just alpha-nums).</param>
        /// <returns>The masked account number.</returns>
        public static string Mask(this string sourceValue, MaskStyle style)
        {
            return Mask(sourceValue, DefaultMaskCharacter, 0, style);
        }

        /// <summary>
        /// Masks all characters for the specified length.
        /// </summary>
        /// <param name="buffer">String builder to store result in.</param>
        /// <param name="source">The source string to pull non-alpha numeric characters.</param>
        /// <param name="mask">Masking character to use.</param>
        /// <param name="length">Length of the mask.</param>
        private static void CreateAlphaNumMask(StringBuilder buffer, string source, char mask, int length)
        {
            for (int i = 0; i < length; i++)
            {
                buffer.Append(char.IsLetterOrDigit(source[i])
                                ? mask
                                : source[i]);
            }
        }

        public static string DefaultIfEmpty(this string str, string defaultValue, bool considerWhiteSpaceIsEmpty = false)
        {
            return (considerWhiteSpaceIsEmpty ? string.IsNullOrWhiteSpace(str) : string.IsNullOrEmpty(str)) ? defaultValue : str;
        }

        public static bool IsValidNIP(this string input)
        {
            int[] weights = { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
            bool result = false;
            if (input.Length == 10)
            {
                int controlSum = CalculateControlSum(input, weights);
                int controlNum = controlSum % 11;
                if (controlNum == 10)
                {
                    controlNum = 0;
                }
                int lastDigit = int.Parse(input[input.Length - 1].ToString());
                result = controlNum == lastDigit;
            }
            return result;
        }

        public static bool IsValidREGON(this string input)
        {
            int controlSum;
            if (input.Length == 7 || input.Length == 9)
            {
                int[] weights = { 8, 9, 2, 3, 4, 5, 6, 7 };
                int offset = 9 - input.Length;
                controlSum = CalculateControlSum(input, weights, offset);
            }
            else if (input.Length == 14)
            {
                int[] weights = { 2, 4, 8, 5, 0, 9, 7, 3, 6, 1, 2, 4, 8 };
                controlSum = CalculateControlSum(input, weights);
            }
            else
            {
                return false;
            }

            int controlNum = controlSum % 11;
            if (controlNum == 10)
            {
                controlNum = 0;
            }
            int lastDigit = int.Parse(input[input.Length - 1].ToString());

            return controlNum == lastDigit;
        }

        public static bool IsValidPESEL(this string input)
        {
            int[] weights = { 1, 3, 7, 9, 1, 3, 7, 9, 1, 3 };
            bool result = false;
            if (input.Length == 11)
            {
                int controlSum = CalculateControlSum(input, weights);
                int controlNum = controlSum % 10;
                controlNum = 10 - controlNum;
                if (controlNum == 10)
                {
                    controlNum = 0;
                }
                int lastDigit = int.Parse(input[input.Length - 1].ToString());
                result = controlNum == lastDigit;
            }
            return result;
        }

        private static int CalculateControlSum(string input, int[] weights, int offset = 0)
        {
            int controlSum = 0;
            for (int i = 0; i < input.Length - 1; i++)
            {
                controlSum += weights[i + offset] * int.Parse(input[i].ToString());
            }
            return controlSum;
        }

        public static string ToSlug(this string text)
        {
            string value = text.Normalize(NormalizationForm.FormD).Trim();
            StringBuilder builder = new StringBuilder();

            foreach (char c in text)
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);

            value = builder.ToString();

            byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(text);

            value = Regex.Replace(Regex.Replace(Encoding.ASCII.GetString(bytes), @"\s{2,}|[^\w]", " ", RegexOptions.ECMAScript).Trim(), @"\s+", "_");

            return value.ToLowerInvariant();
        }

        /// <summary>
        /// Returns a value indicating whether the specified <see cref="string"/> object occurs within the <paramref name="this"/> string.
        /// A parameter specifies the type of search to use for the specified string.
        /// </summary>
        /// <param name="this">The string to search in</param>
        /// <param name="value">The string to seek</param>
        /// <param name="comparisonType">One of the enumeration values that specifies the rules for the search</param>
        /// <exception cref="ArgumentNullException"><paramref name="this"/> or <paramref name="value"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException"><paramref name="comparisonType"/> is not a valid <see cref="StringComparison"/> value</exception>
        /// <returns>
        /// <c>true</c> if the <paramref name="value"/> parameter occurs within the <paramref name="this"/> parameter,
        /// or if <paramref name="value"/> is the empty string (<c>""</c>);
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The <paramref name="comparisonType"/> parameter specifies to search for the value parameter using the current or invariant culture,
        /// using a case-sensitive or case-insensitive search, and using word or ordinal comparison rules.
        /// </remarks>
        public static bool Contains(this string @this, string value, StringComparison comparisonType)
        {
            if (@this == null)
            {
                throw new ArgumentNullException("this");
            }

            return @this.IndexOf(value, comparisonType) >= 0;
        }

        public static bool IsValidIPAddress(this string s) =>
            Regex.IsMatch(s, @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b");

        public static string DefaultIfNull(this string str, string defaultValue) =>
             str ?? defaultValue;

        public static bool IsUnicode(this string value)
        {
            int asciiBytesCount = Encoding.ASCII.GetByteCount(value);
            int unicodBytesCount = Encoding.UTF8.GetByteCount(value);

            if (asciiBytesCount != unicodBytesCount)
                return true;

            return false;
        }

        public static bool EqualsAny(this string str, params string[] args) =>
            args.Any(x => StringComparer.InvariantCultureIgnoreCase.Equals(x, str));

        public static string Inject(this string source, IFormatProvider formatProvider, params object[] args)
        {
            var objectWrappers = new object[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                objectWrappers[i] = new ObjectWrapper(args[i]);
            }

            return string.Format(formatProvider, source, objectWrappers);
        }

        public static string Inject(this string source, params object[] args) =>
            Inject(source, CultureInfo.CurrentUICulture, args);

        /// <summary>
        /// Send an email using the supplied string.
        /// </summary>
        /// <param name="body">String that will be used i the body of the email.</param>
        /// <param name="subject">Subject of the email.</param>
        /// <param name="sender">The email address from which the message was sent.</param>
        /// <param name="recipient">The receiver of the email.</param>
        /// <param name="server">The server from which the email will be sent.</param>
        /// <returns>A boolean value indicating the success of the email send.</returns>
        public static bool SendEmail(this string body, string subject, string sender, string recipient, string server)
        {
            try
            {
                // To
                MailMessage mailMsg = new MailMessage();
                mailMsg.To.Add(recipient);

                // From
                MailAddress mailAddress = new MailAddress(sender);
                mailMsg.From = mailAddress;

                // Subject and Body
                mailMsg.Subject = subject;
                mailMsg.Body = body;

                // Init SmtpClient and send
                SmtpClient smtpClient = new SmtpClient(server);
                smtpClient.Credentials = new System.Net.NetworkCredential();

                smtpClient.Send(mailMsg);
            }
            catch (Exception ex)
            {
                ex.Trace();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Strip a string of the specified character.
        /// </summary>
        /// <param name="s">the string to process</param>
        /// <param name="char">character to remove from the string</param>
        /// <example>
        /// string s = "abcde";
        ///
        /// s = s.Strip('b');  //s becomes 'acde;
        /// </example>
        /// <returns></returns>
        public static string Strip(this string s, char character)
        {
            s = s.Replace(character.ToString(), "");

            return s;
        }

        /// <summary>
        /// Strip a string of the specified characters.
        /// </summary>
        /// <param name="s">the string to process</param>
        /// <param name="chars">list of characters to remove from the string</param>
        /// <example>
        /// string s = "abcde";
        ///
        /// s = s.Strip('a', 'd');  //s becomes 'bce;
        /// </example>
        /// <returns></returns>
        public static string Strip(this string s, params char[] chars)
        {
            foreach (char c in chars)
            {
                s = s.Replace(c.ToString(), "");
            }

            return s;
        }
        /// <summary>
        /// Strip a string of the specified substring.
        /// </summary>
        /// <param name="s">the string to process</param>
        /// <param name="subString">substring to remove</param>
        /// <example>
        /// string s = "abcde";
        ///
        /// s = s.Strip("bcd");  //s becomes 'ae;
        /// </example>
        /// <returns></returns>
        public static string Strip(this string s, string subString)
        {
            s = s.Replace(subString, "");

            return s;
        }

        /// <summary>
        /// JavaScript style Eval for simple calculations
        /// http://www.osix.net/modules/article/?id=761
        /// This is a safe evaluation.  IE will not allow for injection.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string Evaluate(this string e)
        {
            Func<string, bool> VerifyAllowed = e1 =>
            {
                string allowed = "0123456789+-*/()%.,";
                for (int i = 0; i < e1.Length; i++)
                {
                    if (allowed.IndexOf("" + e1[i]) == -1)
                    {
                        return false;
                    }
                }
                return true;
            };

            if (e.Length == 0) { return string.Empty; }
            if (!VerifyAllowed(e)) { return "String contains illegal characters"; }
            if (e[0] == '-') { e = "0" + e; }
            string res = "";
            try
            {
                res = Calculate(e).ToString();
            }
            catch
            {
                return "The call caused an exception";
            }
            return res;
        }

        /// <summary>
        /// JavaScript Eval Calculations for simple calculations
        /// http://www.osix.net/modules/article/?id=761
        /// This is an unsafe calculation. IE may allow for injection.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static double Calculate(this string e)
        {
            e = e.Replace(".", ",");
            if (e.IndexOf("(") != -1)
            {
                int a = e.LastIndexOf("(");
                int b = e.IndexOf(")", a);
                double middle = Calculate(e.Substring(a + 1, b - a - 1));
                return Calculate(e.Substring(0, a) + middle.ToString() + e.Substring(b + 1));
            }
            double result = 0;
            string[] plus = e.Split('+');
            if (plus.Length > 1)
            {
                // there were some +
                result = Calculate(plus[0]);
                for (int i = 1; i < plus.Length; i++)
                {
                    result += Calculate(plus[i]);
                }
                return result;
            }
            else
            {
                // no +
                string[] minus = plus[0].Split('-');
                if (minus.Length > 1)
                {
                    // there were some -
                    result = Calculate(minus[0]);
                    for (int i = 1; i < minus.Length; i++)
                    {
                        result -= Calculate(minus[i]);
                    }
                    return result;
                }
                else
                {
                    // no -
                    string[] mult = minus[0].Split('*');
                    if (mult.Length > 1)
                    {
                        // there were some *
                        result = Calculate(mult[0]);
                        for (int i = 1; i < mult.Length; i++)
                        {
                            result *= Calculate(mult[i]);
                        }
                        return result;
                    }
                    else
                    {
                        // no *
                        string[] div = mult[0].Split('/');
                        if (div.Length > 1)
                        {
                            // there were some /
                            result = Calculate(div[0]);
                            for (int i = 1; i < div.Length; i++)
                            {
                                result /= Calculate(div[i]);
                            }
                            return result;
                        }
                        else
                        {
                            // no /
                            string[] mod = mult[0].Split('%');
                            if (mod.Length > 1)
                            {
                                // there were some %
                                result = Calculate(mod[0]);
                                for (int i = 1; i < mod.Length; i++)
                                {
                                    result %= Calculate(mod[i]);
                                }
                                return result;
                            }
                            else
                            {
                                // no %
                                return double.Parse(e);
                            }
                        }
                    }
                }
            }
        }

        public static bool IsBoolean(this string value)
        {
            var val = value.ToLower().Trim();
            if (val == "false")
                return true;
            if (val == "f")
                return true;
            if (val == "true")
                return true;
            if (val == "t")
                return true;
            if (val == "yes")
                return true;
            if (val == "no")
                return true;
            if (val == "y")
                return true;
            if (val == "n")
                return true;

            return false;
        }

        public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
        {
            int startIndex = 0;

            while (true)
            {
                startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);

                if (startIndex < 0)
                {
                    break;
                }

                originalString = String.Concat(originalString.Substring(0, startIndex), newValue, originalString.Substring(startIndex + oldValue.Length));

                startIndex += newValue.Length;
            }

            return (originalString);
        }

        /// <summary>
        /// Truncates the string to a specified length and replace the truncated to a ...
        /// </summary>
        /// <param name="text">string that will be truncated</param>
        /// <param name="maxLength">total length of characters to maintain before the truncate happens</param>
        /// <returns>truncated string</returns>
        public static string Truncate(this string text, int maxLength)
        {
            // replaces the truncated string to a ...
            const string suffix = "...";
            string truncatedString = text;

            if (maxLength <= 0) return truncatedString;
            int strLength = maxLength - suffix.Length;

            if (strLength <= 0) return truncatedString;

            if (text == null || text.Length <= maxLength) return truncatedString;

            truncatedString = text.Substring(0, strLength);
            truncatedString = truncatedString.TrimEnd();
            truncatedString += suffix;
            return truncatedString;
        }

        /// <summary>
        /// <para>Creates a log-string from the Exception.</para>
        /// <para>The result includes the stacktrace, innerexception et cetera, separated by <seealso cref="Environment.NewLine"/>.</para>
        /// </summary>
        /// <param name="ex">The exception to create the string from.</param>
        /// <param name="additionalMessage">Additional message to place at the top of the string, maybe be empty or null.</param>
        /// <returns></returns>
        public static string ToLogString(this Exception ex, string additionalMessage)
        {
            StringBuilder msg = new StringBuilder();

            if (!string.IsNullOrEmpty(additionalMessage))
            {
                msg.Append(additionalMessage);
                msg.Append(Environment.NewLine);
            }

            if (ex != null)
            {
                try
                {
                    Exception orgEx = ex;

                    msg.Append("Exception:");
                    msg.Append(Environment.NewLine);
                    while (orgEx != null)
                    {
                        msg.Append(orgEx.Message);
                        msg.Append(Environment.NewLine);
                        orgEx = orgEx.InnerException;
                    }

                    if (ex.Data != null)
                    {
                        foreach (object i in ex.Data)
                        {
                            msg.Append("Data :");
                            msg.Append(i.ToString());
                            msg.Append(Environment.NewLine);
                        }
                    }

                    if (ex.StackTrace != null)
                    {
                        msg.Append("StackTrace:");
                        msg.Append(Environment.NewLine);
                        msg.Append(ex.StackTrace.ToString());
                        msg.Append(Environment.NewLine);
                    }

                    if (ex.Source != null)
                    {
                        msg.Append("Source:");
                        msg.Append(Environment.NewLine);
                        msg.Append(ex.Source);
                        msg.Append(Environment.NewLine);
                    }

                    if (ex.TargetSite != null)
                    {
                        msg.Append("TargetSite:");
                        msg.Append(Environment.NewLine);
                        msg.Append(ex.TargetSite.ToString());
                        msg.Append(Environment.NewLine);
                    }

                    Exception baseException = ex.GetBaseException();
                    if (baseException != null)
                    {
                        msg.Append("BaseException:");
                        msg.Append(Environment.NewLine);
                        msg.Append(ex.GetBaseException());
                    }
                }
                finally
                {
                }
            }
            return msg.ToString();
        }

        public static string Repeat(this string self, int count)
        {
            return string.Concat(Enumerable.Repeat(self, count));
        }

        public static string Repeat(this char character, int count)
        {
            return new string(character, count);
        }

        /// <summary>
        /// Converts line endings in the string to <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string NormalizeLineEndings(this string str)
        {
            return str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
        }

        // or

        /// <summary>
        /// Normalize line endings.
        /// </summary>
        /// <param name="lines">Lines to normalize.</param>
        /// <param name="targetLineEnding">If targetLineEnding is null, Environment.NewLine is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">Unknown target line ending character(s).</exception>
        /// <returns>Lines normalized.</returns>
        public static string NormalizeLineEndings(string lines, string targetLineEnding = null)
        {
            if (string.IsNullOrEmpty(lines))
            {
                return lines;
            }

            targetLineEnding = targetLineEnding ?? Environment.NewLine;

            const string unixLineEnding = "\n";
            const string windowsLineEnding = "\r\n";
            const string macLineEnding = "\r";

            if (targetLineEnding != unixLineEnding && targetLineEnding != windowsLineEnding &&
                targetLineEnding != macLineEnding)
            {
                throw new ArgumentOutOfRangeException(nameof(targetLineEnding), "Unknown target line ending character(s).");
            }

            lines = lines
                .Replace(windowsLineEnding, unixLineEnding)
                .Replace(macLineEnding, unixLineEnding);

            if (targetLineEnding != unixLineEnding)
            {
                lines = lines.Replace(unixLineEnding, targetLineEnding);
            }

            return lines;
        }
    }
}
