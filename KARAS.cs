
// Copyright (c) 2014, Daiki Umeda (XJINE) - lightweightmarkuplanguage.com
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// * Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// 
// * Neither the name of the copyright holder nor the names of its
//   contributors may be used to endorse or promote products derived from
//   this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KARAS
{
    public static class KARAS
    {
        //Block group
        public static readonly Regex RegexBlockGroup
            = new Regex("^(?:(\\{\\{)(.*?)|\\}\\}.*?)$", RegexOptions.Multiline);
        public static readonly Regex RegexFigcaptionSummary
            = new Regex("(?:^|\n)(\\=+)(.*?)(\n(?:(?:(?:\\||\\!)"
                        + "[\\|\\=\\>\\<]|\\=|\\-|\\+|\\;|\\>|\\<)|\n)|$)",
                        RegexOptions.Singleline);

        //Block markup
        public static readonly Regex RegexBlockquote
            = new Regex("(?:^|\n)(\\>+)(.*?)(?:(\n\\>)"
                        + "|(\n(?:\n|\\=|\\-|\\+|\\;|(?:(?:\\||\\!)[\\|\\=\\>\\<])))|$)",
                        RegexOptions.Singleline);
        public static readonly Regex RegexTableBlock
            = new Regex("(?:^|\n)((?:\\||\\!)(?:\\||\\>|\\<|\\=).*?)"
                        + "(\n(?!(?:\\||\\!)[\\|\\=\\>\\<])|$)",
                        RegexOptions.Singleline);
        public static readonly Regex RegexTableCell
            = new Regex("(\\\\*)(\\||\\!)(\\||\\>|\\<|\\=)", RegexOptions.Singleline);
        public static readonly Regex RegexList
            = new Regex("(?:^|\n)((?:\\-|\\+)+)(.*?)(?:(\n(?:\\-|\\+))|(\n(?:\n|\\;|\\=))|$)",
                        RegexOptions.Singleline);
        public static readonly Regex RegexDefList
            = new Regex("(?:^|\n)(\\;+)(.*?)(?:(\n\\;)|(\n(?:\n|\\=))|$)",
                        RegexOptions.Singleline);
        public static readonly Regex RegexHeading
            = new Regex("(?:^|\n)(\\=+)(.*?)(\n\\=|\n{2,}|$)", RegexOptions.Singleline);
        //It is important to check '\n{2,}' first, to exclude \n chars.
        public static readonly Regex RegexBlockLink
            = new Regex("(?:\n{2,}|^\n*)\\s*\\({2,}.+?(?:\n{2,}|$)", RegexOptions.Singleline);
        public static readonly Regex RegexParagraph
            = new Regex("(?:\n{2,}|^\n*)(\\s*(<*).+?)(?:\n{2,}|$)", RegexOptions.Singleline);

        //Inline markup
        public static readonly Regex RegexInlineMarkup
            = new Regex("(\\\\*)(\\*{2,}|/{2,}|_{2,}|%{2,}|\\@{2,}"
                        + "|\\?{2,}|\\${2,}|`{2,}|'{2,}|,{2,}|\"{2,}"
                        + "|\\({2,}[\t\v\f\u0020\u00A0]*\\(*|\\){2,}[\t\v\f\u0020\u00A0]*\\)*"
                        + "|<{2,}[\u0020\u00A0]*<*|>{2,}[\u0020\u00A0]*>*)",
                        RegexOptions.Singleline);
        public static readonly Regex RegexLineBreak
            = new Regex("(\\\\*)(\\~(?:\n|$))");

        //Other syntax
        public static readonly Regex RegexPlugin
             = new Regex("(\\\\*)((\\[{2,}[\u0020\u00A0]*\\[*)|(\\]{2,}[\u0020\u00A0]*\\]*))");
        public static readonly Regex RegexCommentOut
            = new Regex("(\\\\*)(\\#{2,})", RegexOptions.Singleline);
        public static readonly Regex RegexSplitOption
             = new Regex("(\\\\*)(:{2,3})", RegexOptions.Singleline);

        //Other
        public static readonly Regex RegexEscape
            = new Regex("\\\\+", RegexOptions.Singleline);
        public static readonly Regex RegexProtocol
            = new Regex(":{1,1}(/{2,})", RegexOptions.Singleline);
        public static readonly Regex RegexWhiteSpace
            = new Regex("\\s");
        public static readonly Regex RegexWhiteSpaceLine
            = new Regex("^[\t\v\f\u0020\u00A0]+$", RegexOptions.Multiline);
        public static readonly Regex RegexLineBreakCode
            = new Regex("\r\n|\r|\n");
        public static readonly Regex RegexBlankLine
            = new Regex("^\n", RegexOptions.Multiline);
        public static readonly Regex RegexPreElement
            = new Regex("(<pre\\s*.*?>)|</pre>", 
                        RegexOptions.Multiline | RegexOptions.IgnoreCase);
        public static readonly Regex RegexLinkElement
            = new Regex("(?:<a.*?>.*?</a>)|(?:<img.*?>)|(?:<video.*?>.*?</video>)"
                        + "|(?:<audio.*?>.*?</audio>)|<object.*?>.*?</object>",
                        RegexOptions.IgnoreCase);
        public static readonly Regex RegexStringTypeAttribute
            = new Regex("([^\\s]+?)\\s*=\\s*\"(.+?)\"");
        public static readonly Regex RegexFileExtension
            = new Regex(".+\\.(.+?)$");

        public const int BlockGroupTypeUndefined = -1;
        public const int BlockGroupTypeDiv = 0;
        public const int BlockGroupTypeDetails = 8;
        public const int BlockGroupTypeFigure = 9;
        public const int BlockGroupTypePre = 10;
        public const int BlockGroupTypeCode = 11;
        public const int BlockGroupTypeKbd = 12;
        public const int BlockGroupTypeSamp = 13;

        public static string[] ReservedBlockGroupTypes = new string[]
        {
            "div", "header", "footer", "nav",
            "article", "section", "aside", "address",
            "details", "figure",
            "pre", "code", "kbd", "samp"
        };

        public const int InlineMarkupTypeDefAbbr = 5;
        public const int InlineMarkupVarCode = 6;
        public const int InlineMarkupKbdSamp = 7;
        public const int InlineMarkupTypeSupRuby = 8;
        public const int InlineMarkupTypeLinkOpen = 11;
        public const int InlineMarkupTypeLinkClose = 12;
        public const int InlineMarkupTypeInlineGroupOpen = 13;
        public const int InlineMarkupTypeInlineGroupClose = 14;

        public static readonly string[][] InlineMarkupSets = new string[][]
        {
            new string[] {"*", "b", "strong"},
            new string[] {"/", "i", "em"},
            new string[] {"_", "u", "ins"},
            new string[] {"%", "s", "del"},
            new string[] {"@", "cite", "small"},
            new string[] {"?", "dfn", "abbr"},
            new string[] {"$", "kbd", "samp"},
            new string[] {"`", "var", "code"},
            new string[] {"'", "sup", "ruby"},
            new string[] {",", "sub"},
            new string[] {"\"", "q"},
            new string[] {"(",  "a"},
            new string[] {")",  "a"},
            new string[] {"<",  "span"},
            new string[] {">",  "span"},
        };

        public const int MediaTypeImage = 0;
        public const int MediaTypeAudio = 1;
        public const int MediaTypeVideo = 2;
        public const int MediaTypeUnknown = 3;

        public static readonly string[] MediaExtensions = new string[]
        {
            "bmp|bitmap|gif|jpg|jpeg|png",
            "aac|aiff|flac|mp3|ogg|wav|wave",
            "asf|avi|flv|mov|movie|mpg|mpeg|mp4|ogv|webm"
        };

        public static readonly string[] ReservedObjectAttributes = new string[] 
        {
            "width", "height", "type", "typemustmatch", "name", "usemap", "form" 
        };

        public const bool ListTypeUl = true;
        public const bool ListTypeOl = false;

        public const string PluginDirectory = "./plugins";
        public const string DefaultEscapeCode = "escpcode";





        public static string convert(string text, string pluginDirectory, int startLevelOfHeading)
        {
            string escapeCode = KARAS.generateSafeEscapeCode(text, KARAS.DefaultEscapeCode);
            string lineBreakCode = KARAS.getDefaultLineBreakCode(text);
            text = text.Replace("\r\n", "\n");
            text = text.Replace("\r", "\n");

            text = KARAS.replaceTextInPluginSyntax(text, ">", escapeCode + ">");
            text = KARAS.replaceTextInPluginSyntax(text, "{", escapeCode + "{");
            text = KARAS.replaceTextInPreElement(text, "\n", escapeCode + "\n" + escapeCode);
            bool hasUnConvertedBlockquote = true;
            while (hasUnConvertedBlockquote)
            {
                text = KARAS.convertBlockGroup(text, escapeCode);
                KARAS.convertBlockquote(ref text, ref hasUnConvertedBlockquote, escapeCode);
            }

            PluginManager pluginManager = new KARAS.PluginManager(pluginDirectory);
            text = text.Replace(escapeCode, "");
            text = KARAS.replaceTextInPreElement(text, "[", escapeCode + "[");
            text = KARAS.convertPlugin(text, pluginManager);

            text = KARAS.replaceTextInPreElement(text, "\n", escapeCode + "\n" + escapeCode);
            hasUnConvertedBlockquote = true;
            while (hasUnConvertedBlockquote)
            {
                text = KARAS.convertBlockGroup(text, escapeCode);
                KARAS.convertBlockquote(ref text, ref hasUnConvertedBlockquote, escapeCode);
            }

            text = KARAS.replaceTextInPreElement(text, "#", escapeCode + "#");
            text = KARAS.convertCommentOut(text);
            text = KARAS.convertWhiteSpaceLine(text);
            text = KARAS.convertProtocol(text);
            text = KARAS.convertTable(text);
            text = KARAS.convertList(text);
            text = KARAS.convertDefList(text);
            text = KARAS.convertHeading(text, startLevelOfHeading);
            text = KARAS.convertBlockLink(text);
            text = KARAS.convertParagraph(text);
            text = KARAS.reduceBlankLine(text);

            text = text.Replace(escapeCode, "");
            text = KARAS.replaceTextInPreElement(text, "\\", escapeCode);
            text = KARAS.reduceEscape(text);
            text = KARAS.replaceTextInPreElement(text, escapeCode, "\\");
            text = text.Replace("\n", lineBreakCode);

            return text;
        }

        public static string getDefaultLineBreakCode(string text)
        {
            Match match = KARAS.RegexLineBreakCode.Match(text);

            if (match.Success)
            {
                return match.Groups[0].Value;
            }
            else
            {
                return "\n";
            }
        }

        public static string generateSafeEscapeCode(string text, string escapeCode)
        {
            while (true)
            {
                if (text.Contains(escapeCode) == false)
                {
                    break;
                }

                Guid guid = Guid.NewGuid();
                escapeCode = guid.ToString("N").Substring(0, 8);
            }

            return escapeCode;
        }

        private class PluginMatch
        {
            public int index;
            public string marks;

            public PluginMatch()
            {
                this.index = -1;
                this.marks = "";
            }
        }

        public static string replaceTextInPluginSyntax(string text, string oldText, string newText)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiEscapes = 1;
            const int mgiMarks = 2;
            const int mgiOpenMarks = 3;
            //const int mgiCloseMarks = 4;

            Stack<PluginMatch> matchStack = new Stack<PluginMatch>();
            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexPlugin.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                if (match.Groups[mgiEscapes].Length % 2 == 1)
                {
                    nextMatchIndex = match.Groups[mgiMarks].Index + 1;
                    continue;
                }

                if (match.Groups[mgiOpenMarks].Length != 0)
                {
                    PluginMatch pluginMatch = new PluginMatch();
                    pluginMatch.index = match.Groups[mgiMarks].Index;
                    matchStack.Push(pluginMatch);
                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiAllText].Length;
                    continue;
                }

                if (matchStack.Count == 0)
                {
                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiAllText].Length;
                    continue;
                }

                PluginMatch preMatch = matchStack.Pop();
                string markedupText = text.Substring
                    (preMatch.index, match.Groups[mgiMarks].Index - preMatch.index);
                markedupText = markedupText.Replace(oldText, newText);

                text = KARAS.removeAndInsertText(text,
                                                 preMatch.index,
                                                 match.Groups[mgiMarks].Index - preMatch.index,
                                                 markedupText);
                nextMatchIndex = preMatch.index + markedupText.Length;
            }

            return text;
        }

        public static string replaceTextInPreElement(string text, string oldText, string newText)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiOpenPreElement = 1;

            Stack<int> matchStack = new Stack<int>();
            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexPreElement.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                if (match.Groups[mgiOpenPreElement].Length != 0)
                {
                    int index = match.Groups[mgiOpenPreElement].Index
                                + match.Groups[mgiOpenPreElement].Length;
                    matchStack.Push(index);
                    nextMatchIndex = index;
                    continue;
                }

                if (matchStack.Count == 0)
                {
                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiAllText].Length;
                    continue;
                }

                int preTextStart = matchStack.Pop();
                int preTextEnd = match.Groups[mgiAllText].Index;
                string preText = text.Substring(preTextStart, preTextEnd - preTextStart);
                preText = preText.Replace(oldText, newText);
                text = KARAS.removeAndInsertText
                    (text, preTextStart, preTextEnd - preTextStart, preText);
                nextMatchIndex = preTextStart + newText.Length + match.Groups[mgiAllText].Length;
            }

            return text;
        }





        public static string encloseWithLinebreak(string text)
        {
            return "\n" + text + "\n";
        }

        public static string escapeHTMLSpecialCharacters(string text)
        {
            text = text.Replace("&", "&amp;");
            text = text.Replace("\"", "&#34;");
            text = text.Replace("'", "&#39;");
            text = text.Replace("<", "&lt;");
            text = text.Replace(">", "&gt;");
            return text;
        }

        public static string removeAndInsertText
            (string text, int index, int removeLength, string newText)
        {
            text = text.Remove(index, removeLength);
            return text.Insert(index, newText);
        }

        public static string removeWhiteSpace(string text)
        {
            return KARAS.RegexWhiteSpace.Replace(text, "");
        }

        public static string[] splitOption(string text, ref bool isSpecialOption)
        {
            //match group index.
            //const int mgiAllText = 0;
            const int mgiEscapes = 1;
            const int mgiMarks = 2;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexSplitOption.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    return new string[] { text.Trim() };
                }

                if (match.Groups[mgiEscapes].Length % 2 == 1)
                {
                    nextMatchIndex = match.Groups[mgiMarks].Index + 1;
                    continue;
                }

                if(match.Groups[mgiMarks].Length == 3)
                {
                    isSpecialOption = true;
                }
                else
                {
                    isSpecialOption = false;
                }

                return new string[]
                {
                    text.Substring(0, match.Groups[mgiMarks].Index).Trim(),
                    text.Substring
                        (match.Groups[mgiMarks].Index + match.Groups[mgiMarks].Length).Trim()
                };
            }
        }

        public static string[] splitOptions(string text, ref bool hasSpecialOption)
        {
            List<string> options = new List<string>();
            string restText = text.Trim();

            while (true)
            {
                bool isSpecialOption = false;
                string[] splitResult = splitOption(restText, ref isSpecialOption);
                
                if (splitResult.Length == 1)
                {
                    options.Add(restText);
                    break;
                }

                if(isSpecialOption == true)
                {
                    options.Add(splitResult[0]);
                    options.Add(splitResult[1]);
                    hasSpecialOption = true;
                    break;
                }

                options.Add(splitResult[0]);
                restText = splitResult[1];
            }

            return options.ToArray();
        }





        public class PluginManager
        {
            // Key = Lowercase plugin name, Value = Plugin
            private Dictionary<string, Type> loadedPluginHash;
            // Key = Original plugin name, Value = File path
            private Dictionary<string, string> pluginFilePathHash;

            public PluginManager(string pluginDirectory)
            {
                this.loadedPluginHash = new Dictionary<string, Type>();
                this.pluginFilePathHash = new Dictionary<string, string>();
                loadPluginFilePaths(PluginManager.getSafeDirectoryPath(pluginDirectory));
            }

            public static string getSafeDirectoryPath(string directory)
            {
                char lastchar = directory[directory.Length - 1];

                if(lastchar == '\\')
                {
                   return directory;            
                }
                else
                {
                    return directory + "\\";
                }
            }

            private void loadPluginFilePaths(string pluginDirectory)
            {
                foreach (string filePath in getPluginFilePaths(pluginDirectory))
                {
                    string pluginName = Path.GetFileNameWithoutExtension(filePath);
                    this.pluginFilePathHash[pluginName] = filePath;
                }
            }

            public static string[] getPluginFilePaths(string pluginDirectory)
            {
                if (Directory.Exists(pluginDirectory) == true)
                    return Directory.GetFiles(pluginDirectory, "*.cs");

                return new string[0];
            }

            public Type getPlugin(string pluginName)
            {
                pluginName = pluginName.ToLower();

                if (loadedPluginHash.ContainsKey(pluginName))
                    return loadedPluginHash[pluginName];
                else
                    return loadAndChacePlugin(pluginName);
            }

            public Type loadAndChacePlugin(string pluginName)
            {
                try
                {
                    foreach (string key in this.pluginFilePathHash.Keys)
                    {
                        if (String.Compare(key, pluginName, true) == 0)
                        {
                            Assembly assembly = compilePlugin(this.pluginFilePathHash[key]);
                            Type plugin = assembly.GetType(pluginName, false, true);

                            if (plugin == null)
                                break;

                            this.loadedPluginHash[pluginName] = plugin;
                            return plugin;
                        }
                    }

                    loadedPluginHash[pluginName] = null;
                    return null;
                }
                catch
                {
                    loadedPluginHash[pluginName] = null;
                    return null;
                }
            }

            public static Assembly compilePlugin(string pluginFilePath)
            {
                CodeDomProvider codeDomProvider = new Microsoft.CSharp.CSharpCodeProvider();
                CompilerParameters compilerParameters = new CompilerParameters();
                compilerParameters.GenerateInMemory = true;
                compilerParameters.ReferencedAssemblies.Add("System.dll");
                compilerParameters.ReferencedAssemblies.Add("KARAS.dll");
                CompilerResults compilerResults;

                compilerResults = codeDomProvider.CompileAssemblyFromFile
                                    (compilerParameters, pluginFilePath);

                if (compilerResults.Errors.Count > 0)
                {
                    string compileErrorText = "Failed to compile script :\""
                                              + pluginFilePath + "\"\n";
                    foreach (CompilerError compilerError in compilerResults.Errors)
                        compileErrorText += compilerError + "\n";
                    throw new ApplicationException(compileErrorText);
                }

                return compilerResults.CompiledAssembly;
            }
        }

        public static string convertPlugin(string text, PluginManager pluginManager)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiEscapes = 1;
            const int mgiMarks = 2;
            const int mgiOpenMarks = 3;
            const int mgiCloseMarks = 4;

            Stack<PluginMatch> matchStack = new Stack<PluginMatch>();
            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexPlugin.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                if (match.Groups[mgiEscapes].Length % 2 == 1)
                {
                    nextMatchIndex = match.Groups[mgiMarks].Index + 1;
                    continue;
                }

                if (match.Groups[mgiOpenMarks].Length != 0)
                {
                    PluginMatch pluginMatch = new PluginMatch();
                    pluginMatch.index = match.Groups[mgiMarks].Index;
                    pluginMatch.marks = match.Groups[mgiMarks].Value;
                    matchStack.Push(pluginMatch);
                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiAllText].Length;
                    continue;
                }

                if (matchStack.Count == 0)
                {
                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiAllText].Length;
                    continue;
                }

                PluginMatch preMatch = matchStack.Pop();
                int markedupTextIndex = preMatch.index + preMatch.marks.Length;
                string markedupText = text.Substring
                    (markedupTextIndex, match.Groups[mgiAllText].Index - markedupTextIndex);
                string openMarks = removeWhiteSpace(preMatch.marks);
                string closeMarks = removeWhiteSpace(match.Groups[mgiCloseMarks].Value);
                string newText = constructPluginText
                                    (text, markedupText, openMarks, closeMarks, pluginManager);
                int markDiff = openMarks.Length - closeMarks.Length;

                if (markDiff > 0)
                {
                    openMarks = openMarks.Remove(markDiff);
                    closeMarks = "";

                    if (markDiff > 1)
                    {
                        preMatch.marks = openMarks.Remove(markDiff);
                        matchStack.Push(preMatch);
                    }
                }
                else
                {
                    openMarks = "";
                    closeMarks = closeMarks.Remove(-markDiff);
                }

                newText = openMarks + newText + closeMarks;

                //It is important to trim close marks to exclude whitespace out of syntax.
                text = removeAndInsertText(text,
                                           preMatch.index,
                                           match.Groups[mgiAllText].Index
                                           + match.Groups[mgiAllText].Value.Trim().Length
                                           - preMatch.index,
                                           newText);
                nextMatchIndex = preMatch.index + newText.Length - closeMarks.Length;
            }

            return text;
        }

        private static string constructPluginText(string text, string markedupText,
            string openMarks, string closeMarks, PluginManager pluginManager)
        {
            bool hasSpecialOption = false;
            string[] markedupTexts = splitOptions(markedupText, ref hasSpecialOption);
            string pluginName = markedupTexts[0];
            string[] options = new string[0];

            if (markedupTexts.Length > 1)
            {
                options = markedupTexts.Skip(1).Take(markedupTexts.Length - 1).ToArray();
            }

            if(hasSpecialOption == true)
            {
                markedupText = options[options.Length - 1];
                options = options.Take(markedupTexts.Length - 2).ToArray();
            }

            if (openMarks.Length > 2 && closeMarks.Length > 2)
            {
                return constructActionTypePluginText
                    (pluginManager, pluginName, options, markedupText, text);
            }
            else
            {
                return constructConvertTypePluginText
                    (pluginManager, pluginName, options, markedupText);
            }
        }

        private static string constructActionTypePluginText
            (PluginManager pluginManager, string pluginName, string[] options, string markedupText, string text)
        {
            Type plugin = pluginManager.getPlugin(pluginName);

            if (plugin == null)
            {
                return " Plugin \"" + pluginName + "\" has wrong. ";
            }

            try
            {
                MethodInfo methodInfo = plugin.GetMethod("action");
                return (string)methodInfo.Invoke(null, new object[] { options, markedupText, text });
            }
            catch
            {
                return " Plugin \"" + pluginName + "\" has wrong. ";
            }
        }

        private static string constructConvertTypePluginText
            (PluginManager pluginManager, string pluginName, string[] options, string markedupText)
        {
            Type plugin = pluginManager.getPlugin(pluginName);

            if (plugin == null)
            {
                return " Plugin \"" + pluginName + "\" has wrong. ";
            }

            try
            {
                MethodInfo methodInfo = plugin.GetMethod("convert");
                return (string)methodInfo.Invoke(null, new object[] { options, markedupText });
            }
            catch
            {
                return " Plugin \"" + pluginName + "\" has wrong. ";
            }
        }







        private class BlockGroupMatch
        {
            public int type;
            public int index;
            public int length;
            public string option;

            public BlockGroupMatch()
            {
                this.type = -1;
                this.index = -1;
                this.length = -1;
                this.option = "";
            }
        }

        public static string convertBlockGroup(string text, string escapeCode)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiOpenMarks = 1;
            const int mgiOption = 2;

            Match match = null;
            int nextMatchIndex = 0;

            Stack<BlockGroupMatch> matchStack = new Stack<BlockGroupMatch>();
            Match unhandledGroupClose = null;
            int groupsInPreCodeKbdSamp = 0;

            while (true)
            {
                match = KARAS.RegexBlockGroup.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    if (groupsInPreCodeKbdSamp > 0 && unhandledGroupClose != null)
                    {
                        match = unhandledGroupClose;
                        groupsInPreCodeKbdSamp = 0;
                    }
                    else
                    {
                        break;
                    }
                }

                if (match.Groups[mgiOpenMarks].Length > 0)
                {
                    BlockGroupMatch blockGroupMatch =
                        KARAS.constructBlockGroupMatch(match.Groups[mgiAllText].Index,
                                                 match.Groups[mgiAllText].Length,
                                                 match.Groups[mgiOption].Value);

                    if (blockGroupMatch.type >= KARAS.BlockGroupTypePre)
                    {
                        groupsInPreCodeKbdSamp += 1;

                        if (groupsInPreCodeKbdSamp == 1)
                        {
                            matchStack.Push(blockGroupMatch);
                        }
                    }
                    else
                    {
                        //if pre or code group is open.
                        if (groupsInPreCodeKbdSamp > 0)
                        {
                            groupsInPreCodeKbdSamp += 1;
                        }
                        else
                        {
                            matchStack.Push(blockGroupMatch);
                        }
                    }

                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiAllText].Length;
                    continue;
                }

                if (matchStack.Count == 0)
                {
                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiAllText].Length;
                    continue;
                }

                if (groupsInPreCodeKbdSamp > 1)
                {
                    groupsInPreCodeKbdSamp -= 1;
                    unhandledGroupClose = match;
                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiAllText].Length;
                    continue;
                }

                BlockGroupMatch preMatch = matchStack.Pop();
                string newOpenText = "";
                string newCloseText = "";
                KARAS.constructBlockGroupText(preMatch, ref newOpenText, ref newCloseText);

                //Note, it is important to exclude linebreak code.
                int markedutTextIndex = preMatch.index + preMatch.length + 1;
                string markedupText = text.Substring
                    (markedutTextIndex, match.Groups[mgiAllText].Index - markedutTextIndex - 1);

                switch (preMatch.type)
                {
                    case KARAS.BlockGroupTypeDetails:
                        {
                            markedupText = 
                                KARAS.convertFigcaptionSummary(markedupText, "summary");
                            break;
                        }
                    case KARAS.BlockGroupTypeFigure:
                        {
                            markedupText = 
                                KARAS.convertFigcaptionSummary(markedupText, "figcaption");
                            break;
                        }
                    case KARAS.BlockGroupTypePre:
                    case KARAS.BlockGroupTypeCode:
                    case KARAS.BlockGroupTypeKbd:
                    case KARAS.BlockGroupTypeSamp:
                        {
                            markedupText = KARAS.escapeHTMLSpecialCharacters(markedupText);
                            markedupText = markedupText.Replace("\n", escapeCode + "\n" + escapeCode);
                            groupsInPreCodeKbdSamp = 0;
                            break;
                        }
                }

                string newText = newOpenText + markedupText + newCloseText;
                text = removeAndInsertText(text,
                                           preMatch.index,
                                           match.Groups[mgiAllText].Index
                                           + match.Groups[mgiAllText].Length
                                           - preMatch.index,
                                           newText);
                nextMatchIndex = preMatch.index + newText.Length;
            }

            return text;
        }

        private static BlockGroupMatch constructBlockGroupMatch
            (int index, int textLength, string optionText)
        {
            BlockGroupMatch blockGroupMatch = new BlockGroupMatch();
            blockGroupMatch.index = index;
            blockGroupMatch.length = textLength;

            bool isSpecialOption = false;
            string[] options = KARAS.splitOptions(optionText, ref isSpecialOption);

            if (options.Length > 0)
            {
                string groupType = options[0];
                blockGroupMatch.type = getGroupType(groupType);

                if (blockGroupMatch.type == KARAS.BlockGroupTypeUndefined)
                {
                    blockGroupMatch.type = KARAS.BlockGroupTypeDiv;
                    blockGroupMatch.option = groupType;
                }
            }

            if (options.Length > 1)
            {
                blockGroupMatch.option = options[1];
            }

            return blockGroupMatch;
        }

        private static int getGroupType(string groupTypeText)
        {
            for (int i = 0; i < KARAS.ReservedBlockGroupTypes.Length; i += 1)
            {
                if (String.Compare(groupTypeText, KARAS.ReservedBlockGroupTypes[i], true) == 0)
                {
                    return i;
                }
            }

            return KARAS.BlockGroupTypeUndefined;
        }

        private static void constructBlockGroupText
            (BlockGroupMatch groupOpen, ref string newOpenText, ref string newCloseText)
        {
            newCloseText = "</" + KARAS.ReservedBlockGroupTypes[groupOpen.type] + ">";
            string optionText = "";

            if (groupOpen.option.Length != 0)
            {
                optionText = " class=\"" + groupOpen.option + "\"";
            }

            newOpenText = "<" + KARAS.ReservedBlockGroupTypes[groupOpen.type] + optionText + ">";

            if (groupOpen.type >= KARAS.BlockGroupTypePre)
            {
                if (groupOpen.type >= KARAS.BlockGroupTypeCode)
                {
                    newOpenText = "<pre" + optionText + ">" + newOpenText;
                    newCloseText += "</pre>";
                }

                newOpenText = "\n" + newOpenText;
                newCloseText = newCloseText + "\n";
            }
            else
            {
                newOpenText = KARAS.encloseWithLinebreak(newOpenText) + "\n";
                newCloseText = "\n" + KARAS.encloseWithLinebreak(newCloseText);
            }
        }

        private static string convertFigcaptionSummary(string text, string element)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiMarks = 1;
            const int mgiMarkedupText = 2;
            const int mgiBreaks = 3;

            const int maxLevelOfHeading = 6;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexFigcaptionSummary.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                string newText = "";
                int level = match.Groups[mgiMarks].Length;

                if (level >= maxLevelOfHeading + 1)
                {
                    newText = KARAS.encloseWithLinebreak("<hr>");
                }
                else
                {
                    //Note, it is important to convert inline markups first,
                    //to convert inline markup's options first.
                    string markedupText = 
                        KARAS.convertInlineMarkup(match.Groups[mgiMarkedupText].Value);
                    bool hasSpecialOption = false;
                    string[] markedupTexts = KARAS.splitOptions(markedupText, ref hasSpecialOption);
                    string id = "";

                    if (markedupTexts.Length > 1)
                    {
                        id = " id=\"" + markedupTexts[1] + "\"";
                    }

                    newText = KARAS.encloseWithLinebreak("<" + element + id + ">"
                                                         + markedupTexts[0]
                                                         + "</" + element + ">");
                }

                nextMatchIndex = match.Groups[mgiAllText].Index + newText.Length;
                text = KARAS.removeAndInsertText(text,
                                                 match.Groups[mgiAllText].Index,
                                                 match.Groups[mgiAllText].Length
                                                 - match.Groups[mgiBreaks].Length,
                                                 KARAS.encloseWithLinebreak(newText));
            }

            return text;
        }

        private class SequentialBlockquote
        {
            public int level;
            public string text;

            public SequentialBlockquote()
            {
                this.level = -1;
                this.text = "";
            }
        }

        public static void convertBlockquote(ref string text, ref bool hasUnConvertedBlockquote, string escapeCode)
        {
            //match group index.
            const int mgiAllText = 0;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexBlockquote.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    if (nextMatchIndex == 0)
                    {
                        hasUnConvertedBlockquote = false;
                    }
                    else
                    {
                        hasUnConvertedBlockquote = true;
                    }

                    break;
                }

                List<SequentialBlockquote> sequentialBlockquotes
                    = new List<SequentialBlockquote>();
                int indexOfBlockquoteStart = match.Groups[mgiAllText].Index;
                int indexOfBlockquoteEnd = KARAS.constructSequentialBlockquotes
                    (text, indexOfBlockquoteStart, ref sequentialBlockquotes);

                string newText = KARAS.constructBlockquoteText(sequentialBlockquotes);
                newText = KARAS.replaceTextInPreElement(newText, "\n", escapeCode + "\n" + escapeCode);
                newText = KARAS.encloseWithLinebreak(newText);
                nextMatchIndex = indexOfBlockquoteStart + newText.Length;
                text = KARAS.removeAndInsertText(text,
                                                 indexOfBlockquoteStart,
                                                 indexOfBlockquoteEnd - indexOfBlockquoteStart,
                                                 KARAS.encloseWithLinebreak(newText));
            }
        }

        private static int constructSequentialBlockquotes
            (string text, int indexOfBlockquoteStart, ref List<SequentialBlockquote> blockquotes)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiMarks = 1;
            const int mgiMarkedupText = 2;
            const int mgiNextMarks = 3;
            const int mgiBreaks = 4;

            Match match = null;
            int nextMatchIndex = indexOfBlockquoteStart;

            while (true)
            {
                match = KARAS.RegexBlockquote.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                int level = match.Groups[mgiMarks].Length;
                string markedupText = match.Groups[mgiMarkedupText].Value;

                if (blockquotes.Count == 0)
                {
                    SequentialBlockquote sequentialBlockquote = new SequentialBlockquote();
                    sequentialBlockquote.level = level;
                    sequentialBlockquote.text = markedupText.Trim();
                    blockquotes.Add(sequentialBlockquote);
                }
                else
                {
                    SequentialBlockquote previousBlockquote =
                        blockquotes[blockquotes.Count - 1];
                    int previousLevel = previousBlockquote.level;

                    if (level != previousLevel)
                    {
                        SequentialBlockquote sequentialBlockquote = new SequentialBlockquote();
                        sequentialBlockquote.level = level;
                        sequentialBlockquote.text = markedupText.Trim();
                        blockquotes.Add(sequentialBlockquote);
                    }
                    else
                    {
                        if (previousBlockquote.text.Length != 0)
                        {
                            previousBlockquote.text += "\n";
                        }

                        previousBlockquote.text += markedupText.Trim();
                    }
                }

                if (match.Groups[mgiNextMarks].Length == 0)
                {
                    return match.Groups[mgiAllText].Index
                           + match.Groups[mgiAllText].Length
                           - match.Groups[mgiBreaks].Length;
                }

                nextMatchIndex = match.Groups[mgiNextMarks].Index;
            }

            return -1;
        }

        private static string constructBlockquoteText
            (List<SequentialBlockquote> sequentialBlockquotes)
        {
            string blockquoteText = "";

            for (int i = 0; i < sequentialBlockquotes[0].level; i += 1)
            {
                blockquoteText += "<blockquote>\n\n";
            }

            blockquoteText += sequentialBlockquotes[0].text;

            for (int i = 1; i < sequentialBlockquotes.Count; i += 1)
            {
                int levelDiff = sequentialBlockquotes[i].level
                                - sequentialBlockquotes[i - 1].level;

                if (levelDiff > 0)
                {
                    for (int j = 0; j < levelDiff; j += 1)
                    {
                        blockquoteText += "\n\n<blockquote>";
                    }
                }
                else
                {
                    for (int j = levelDiff; j < 0; j += 1)
                    {
                        blockquoteText += "\n\n</blockquote>";
                    }
                }

                blockquoteText += "\n\n" + sequentialBlockquotes[i].text;
            }

            for (int i = 0; i < sequentialBlockquotes[sequentialBlockquotes.Count - 1].level; i += 1)
            {
                blockquoteText += "\n\n</blockquote>";
            }

            return blockquoteText;
        }





        public static string convertCommentOut(string text)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiEscapes = 1;
            const int mgiMarks = 2;

            Match match = null;
            int nextMatchIndex = 0;
            int indexOfOpenMarks = 0;
            bool markIsOpen = false;

            while (true)
            {
                match = KARAS.RegexCommentOut.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                if (match.Groups[mgiEscapes].Length % 2 == 1)
                {
                    nextMatchIndex = match.Groups[mgiMarks].Index + 1;
                    continue;
                }

                if (markIsOpen == false)
                {
                    markIsOpen = true;
                    indexOfOpenMarks = match.Groups[mgiMarks].Index;
                    nextMatchIndex = indexOfOpenMarks + match.Groups[mgiMarks].Length;
                    continue;
                }

                text = text.Remove(indexOfOpenMarks,
                                   match.Groups[mgiAllText].Index
                                   + match.Groups[mgiAllText].Length
                                   - indexOfOpenMarks);
                markIsOpen = false;
                nextMatchIndex = indexOfOpenMarks;
            }

            return text;
        }

        public static string convertWhiteSpaceLine(string text)
        {
            //match group index.
            const int mgiAllText = 0;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexWhiteSpaceLine.Match(text);

                if (match.Success == false)
                {
                    break;
                }

                string newText = "\n";

                nextMatchIndex = match.Groups[mgiAllText].Index + newText.Length;
                text = KARAS.removeAndInsertText(text,
                                                 match.Groups[mgiAllText].Index,
                                                 match.Groups[mgiAllText].Length,
                                                 newText);
            }

            return text;
        }

        public static string convertProtocol(string text)
        {
            //match group index.
            //const int mgiAllText = 0
            const int mgiMarks = 1;

            Match match = null;
            int nextMatchIndex = 0;

            while(true)
            {
                match = KARAS.RegexProtocol.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                string newText = "";

                for(int i = 0; i < match.Groups[mgiMarks].Length; i +=1)
                {
                    newText += "\\/";
                }

                nextMatchIndex = match.Groups[mgiMarks].Index + newText.Length;
                text = KARAS.removeAndInsertText(text,
                                                 match.Groups[mgiMarks].Index,
                                                 match.Groups[mgiMarks].Length,
                                                 newText);
            }

            return text;
        }

        private class TableCell
        {
            public bool isCollSpanBlank;
            public bool isRowSpanBlank;
            public string type;
            public string textAlign;
            public string text;

            public TableCell()
            {
                this.isCollSpanBlank = false;
                this.isRowSpanBlank = false;
                this.type = "";
                this.textAlign = "";
                this.text = "";
            }
        }

        public static string convertTable(string text)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiTableText = 1;
            const int mgiBreaks = 2;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexTableBlock.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                List<TableCell>[] cells = 
                    KARAS.constructTableCells(match.Groups[mgiTableText].Value);
                string newText = KARAS.constructTableText(cells);
                newText = KARAS.encloseWithLinebreak(newText);
                nextMatchIndex = match.Groups[mgiAllText].Index + newText.Length;
                text = KARAS.removeAndInsertText(text, 
                                                 match.Groups[mgiAllText].Index,
                                                 match.Groups[mgiAllText].Length
                                                 - match.Groups[mgiBreaks].Length,
                                                 KARAS.encloseWithLinebreak(newText));
            }

            return text;
        }

        private static List<TableCell>[] constructTableCells(string tableBlock)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiEscapes = 1;
            const int mgiCellType = 2;
            const int mgiTextAlign = 3;

            //like '||' or any other...
            const int tableCellMarkLength = 2;

            string[] tableLines = tableBlock.Split('\n');
            List<TableCell>[] cells = new List<TableCell>[tableLines.Length];

            for (int i = 0; i < tableLines.Length; i += 1)
            {
                string tableLine = tableLines[i];
                cells[i] = new List<TableCell>();
                Match match = null;
                string markedupText = "";
                int nextMatchIndex = 0;

                while (true)
                {
                    match = KARAS.RegexTableCell.Match(tableLine, nextMatchIndex);

                    if (match.Success == false)
                    {
                        markedupText = tableLine.Substring(nextMatchIndex);

                        if (cells[i].Count > 0)
                        {
                            KARAS.setTableCellTextAndBlank(cells[i][cells[i].Count - 1], markedupText);
                        }

                        break;
                    }

                    if (match.Groups[mgiEscapes].Length % 2 == 1)
                    {
                        nextMatchIndex = match.Groups[mgiCellType].Index + 1;
                        continue;
                    }

                    TableCell cell = new TableCell();
                    KARAS.setTableCellTypeAndAlign
                        (cell, match.Groups[mgiCellType].Value, match.Groups[mgiTextAlign].Value);
                    markedupText = tableLine.Substring
                        (nextMatchIndex, match.Groups[mgiAllText].Index - nextMatchIndex);

                    if (cells[i].Count > 0)
                    {
                        KARAS.setTableCellTextAndBlank(cells[i][cells[i].Count - 1], markedupText);
                    }

                    cells[i].Add(cell);
                    nextMatchIndex = match.Groups[mgiAllText].Index + tableCellMarkLength;
                }
            }

            return cells;
        }

        private static void setTableCellTypeAndAlign
            (TableCell cell, string cellTypeMark, string textAlignMark)
        {
            if (cellTypeMark == "|")
            {
                cell.type = "td";
            }
            else
            {
                cell.type = "th";
            }

            switch (textAlignMark)
            {
                case ">":
                    {
                        cell.textAlign = " style=\"text-align:right\"";
                        break;
                    }
                case "<":
                    {
                        cell.textAlign = " style=\"text-align:left\"";
                        break;
                    }
                case "=":
                    {
                        cell.textAlign = " style=\"text-align:center\"";
                        break;
                    }
                default:
                    {
                        cell.textAlign = "";
                        break;
                    }
            }
        }

        private static void setTableCellTextAndBlank(TableCell cell, string markedupText)
        {
            markedupText = markedupText.Trim();

            switch (markedupText)
            {
                case "::":
                    {
                        cell.isCollSpanBlank = true;
                        break;
                    }
                case ":::":
                    {
                        cell.isRowSpanBlank = true;
                        break;
                    }
                default:
                    {
                        cell.text = KARAS.convertInlineMarkup(markedupText);
                        break;
                    }
            }
        }

        private static string constructTableText(List<TableCell>[] cells)
        {
            string tableText = "<table>\n";

            for (int row = 0; row < cells.Length; row += 1)
            {
                tableText += "<tr>";

                for (int column = 0; column < cells[row].Count; column += 1)
                {
                    TableCell cell = cells[row][column];

                    if (cell.isCollSpanBlank || cell.isRowSpanBlank)
                    {
                        continue;
                    }

                    int columnBlank = KARAS.countBlankColumn(cells, column, row);
                    int rowBlank = KARAS.countBlankRow(cells, column, row);
                    string colspanText = "";
                    string rowspanText = "";

                    if (columnBlank > 1)
                    {
                        colspanText = " colspan = \"" + columnBlank + "\"";
                    }

                    if (rowBlank > 1)
                    {
                        rowspanText = " rowspan = \"" + rowBlank + "\"";
                    }
                    
                    tableText += "<" + cell.type + colspanText + rowspanText + cell.textAlign + ">"
                                 + cell.text
                                 + "</" + cell.type + ">";
                }

                tableText += "</tr>\n";
            }

            tableText += "</table>";
            return tableText;
        }

        private static int countBlankColumn(List<TableCell>[] cells, int column, int row)
        {
            int blank = 1;
            int rightColumn = column + 1;

            while (rightColumn < cells[row].Count)
            {
                TableCell rightCell = cells[row][rightColumn];

                if (rightCell.isCollSpanBlank)
                {
                    blank += 1;
                    rightColumn += 1;
                }
                else
                {
                    break;
                }
            }

            return blank;
        }

        private static int countBlankRow(List<TableCell>[] cells, int column, int row)
        {
            int blank = 1;
            int underRow = row + 1;

            while (underRow < cells.Length)
            {
                //Note, sometimes there is no column in next row.
                if (column >= cells[underRow].Count)
                {
                    break;
                }

                TableCell underCell = cells[underRow][column];

                if (underCell.isRowSpanBlank)
                {
                    blank += 1;
                    underRow += 1;
                }
                else
                {
                    break;
                }
            }

            return blank;
        }

        private class SequentialList
        {
            public bool type;
            public int level;
            public List<string> items;

            public SequentialList()
            {
                this.type = false;
                this.level = -1;
                this.items = new List<string>();            
            }
        }

        public static string convertList(string text)
        {
            //match group index.
            const int mgiAllText = 0;
            
            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexList.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                List<SequentialList> sequentialLists = new List<SequentialList>();
                int listStartIndex = match.Groups[mgiAllText].Index;
                int listEndIndex = 
                    KARAS.constructSequentialLists(text, listStartIndex, sequentialLists);

                string newText = KARAS.constructListText(sequentialLists);
                newText = KARAS.encloseWithLinebreak(newText);
                nextMatchIndex = listStartIndex + newText.Length;
                text = KARAS.removeAndInsertText(text,
                                                 listStartIndex,
                                                 listEndIndex - listStartIndex,
                                                 KARAS.encloseWithLinebreak(newText));
            }

            return text;
        }

        private static int constructSequentialLists
            (string text, int indexOfListStart, List<SequentialList> sequentialLists)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiMarks = 1;
            const int mgiMarkedupText = 2;
            const int mgiNextMarks = 3;
            const int mgiBreaks = 4;

            Match match = null;
            int nextMatchIndex = indexOfListStart;
            int levelDiff = 0;
            int previousLevel = 0;

            while (true)
            {
                match = KARAS.RegexList.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                //Update
                levelDiff = match.Groups[mgiMarks].Length - previousLevel;
                previousLevel = match.Groups[mgiMarks].Length;

                //If start of the items. || If Level up or down.
                if (levelDiff != 0)
                {
                    SequentialList sequentialList = new SequentialList();

                    if (match.Groups[mgiMarks].Value[match.Groups[mgiMarks].Length - 1] == '-')
                    {
                        sequentialList.type = KARAS.ListTypeUl;
                    }
                    //If  == '+'
                    else
                    {
                        sequentialList.type = KARAS.ListTypeOl;
                    }

                    sequentialList.level = match.Groups[mgiMarks].Length;
                    sequentialList.items.Add(match.Groups[mgiMarkedupText].Value);
                    sequentialLists.Add(sequentialList);
                }
                //If same Level.
                else
                {
                    SequentialList previousSequentialList
                        = sequentialLists[sequentialLists.Count - 1];
                    bool listType = KARAS.ListTypeUl;

                    if (match.Groups[mgiMarks].Value[match.Groups[mgiMarks].Length - 1] == '-')
                    {
                        listType = KARAS.ListTypeUl;
                    }
                    //If == '+'
                    else
                    {
                        listType = KARAS.ListTypeOl;
                    }

                    if (listType != previousSequentialList.type)
                    {
                        SequentialList sequentialList = new SequentialList();
                        sequentialList.type = listType;
                        sequentialList.level = match.Groups[mgiMarks].Length;
                        sequentialList.items.Add(match.Groups[mgiMarkedupText].Value);
                        sequentialLists.Add(sequentialList);
                    }
                    //If same items type.
                    else
                    {
                        previousSequentialList.items.Add(match.Groups[mgiMarkedupText].Value);
                    }
                }

                if (match.Groups[mgiNextMarks].Length == 0)
                {
                    return match.Groups[mgiAllText].Index
                           + match.Groups[mgiAllText].Length
                           - match.Groups[mgiBreaks].Length;
                }

                nextMatchIndex = match.Groups[mgiNextMarks].Index;
            }

            return -1;
        }

        private static string constructListText(List<SequentialList> sequentialLists)
        {
            //Key = Level, Value = isUL(true:ul, false:ol)
            Dictionary<int, bool> listTypeHash = KARAS.constructListTypeHash(sequentialLists);
            string listText = "";

            listText += KARAS.constructFirstSequentialListText(sequentialLists[0], listTypeHash);

            //Write later lists.
            int previousLevel = sequentialLists[0].level;

            for (int i = 1; i < sequentialLists.Count; i += 1)
            {
                SequentialList sequentialList = sequentialLists[i];

                //If level up.
                if (previousLevel < sequentialList.level)
                {
                    listText += KARAS.constructUpLevelSequentialListText
                        (previousLevel, sequentialList, listTypeHash);
                }
                //If level down.
                else if (previousLevel > sequentialList.level)
                {
                    listText += KARAS.constructDownLevelSequentialListText
                        (previousLevel, sequentialList, listTypeHash);
                }
                //If same level.(It means the list type is changed.)
                else
                {
                    listText += KARAS.constructSameLevelSequentialListText
                        (previousLevel, sequentialList, listTypeHash);
                }

                previousLevel = sequentialList.level;
            }

            listText += KARAS.constructListCloseText(previousLevel, listTypeHash);

            return KARAS.encloseWithLinebreak(listText);
        }

        private static Dictionary<int, bool> constructListTypeHash
            (List<SequentialList> sequentialLists)
        {
            Dictionary<int, bool> listTypeHash = new Dictionary<int, bool>();
            int maxLevel = 1;

            foreach (SequentialList list in sequentialLists)
            {
                if (maxLevel < list.level)
                {
                    maxLevel = list.level;
                }

                if (listTypeHash.ContainsKey(list.level) == false)
                {
                    listTypeHash[list.level] = list.type;
                }
            }

            //If there is undefined level,
            //set the list type of the next higher defined level to it.
            //Note, the maximum level always has level type. 
            for (int level = 1; level < maxLevel; level += 1)
            {
                if (listTypeHash.ContainsKey(level) == false)
                {
                    List<int> typeUndefinedLevels = new List<int>();
                    typeUndefinedLevels.Add(level);

                    for (int nextLevel = level + 1; nextLevel <= maxLevel; nextLevel += 1)
                    {
                        if (listTypeHash.ContainsKey(nextLevel))
                        {
                            foreach (int typeUndefinedLevel in typeUndefinedLevels)
                            {
                                listTypeHash[typeUndefinedLevel] = listTypeHash[nextLevel];
                            }

                            //Skip initialized level.
                            level = nextLevel + 1;
                            break;
                        }

                        typeUndefinedLevels.Add(nextLevel);
                    }
                }
            }

            return listTypeHash;
        }

        private static string constructFirstSequentialListText
            (SequentialList sequentialList, Dictionary<int, bool> listTypeHash)
        {
            string listText = "";

            for (int level = 1; level < sequentialList.level; level += 1)
            {
                if (listTypeHash[level] == KARAS.ListTypeUl)
                {
                    listText += "<ul>\n<li>\n";
                }
                else
                {
                    listText += "<ol>\n<li>\n";
                }
            }

            if (sequentialList.type == KARAS.ListTypeUl)
            {
                listText += "<ul>\n<li";
            }
            else
            {
                listText += "<ol>\n<li";
            }

            for (int i = 0; i < sequentialList.items.Count - 1; i += 1)
            {
                listText += KARAS.constructListItemText(sequentialList.items[i]) + "</li>\n<li";
            }

            listText +=
                KARAS.constructListItemText(sequentialList.items[sequentialList.items.Count - 1]);

            return listText;
        }

        private static string constructUpLevelSequentialListText
            (int previousLevel, SequentialList sequentialList, Dictionary<int, bool> listTypeHash)
        {
            string listText = "";

            for (int level = previousLevel; level < sequentialList.level - 1; level += 1)
            {
                if (listTypeHash[level] == KARAS.ListTypeUl)
                {
                    listText += "\n<ul>\n<li>";
                }
                else
                {
                    listText += "\n<ol>\n<li>";
                }
            }

            if (sequentialList.level != 1)
            {
                listText += "\n";
            }

            if (sequentialList.type == KARAS.ListTypeUl)
            {
                listText += "<ul>\n<li";
            }
            else
            {
                listText += "<ol>\n<li";
            }

            for (int i = 0; i < sequentialList.items.Count - 1; i += 1)
            {
                listText += KARAS.constructListItemText(sequentialList.items[i]) + "</li>\n<li";
            }

            listText +=
                KARAS.constructListItemText(sequentialList.items[sequentialList.items.Count - 1]);

            return listText;
        }

        private static string constructDownLevelSequentialListText
            (int previousLevel, SequentialList sequentialList, Dictionary<int, bool> listTypeHash)
        {
            //Close previous list item.
            string listText = "</li>\n";

            //Close previous level lists.
            for (int level = previousLevel; level > sequentialList.level; level -= 1)
            {
                if (listTypeHash[level] == KARAS.ListTypeUl)
                {
                    listText += "</ul>\n";
                }
                else
                {
                    listText += "</ol>\n";
                }

                listText += "</li>\n";
            }

            //if current level's list type is different from previous same level's list type.
            if (listTypeHash[sequentialList.level] != sequentialList.type)
            {
                //Note, it is important to update hash.
                if (listTypeHash[sequentialList.level] == KARAS.ListTypeUl)
                {
                    listText += "</ul>\n<ol>\n";
                    listTypeHash[sequentialList.level] = KARAS.ListTypeOl;
                }
                else
                {
                    listText += "</ol>\n<ul>\n";
                    listTypeHash[sequentialList.level] = KARAS.ListTypeUl;
                }
            }

            for (int i = 0; i < sequentialList.items.Count - 1; i += 1)
            {
                listText +=
                    "<li" + KARAS.constructListItemText(sequentialList.items[i]) + "</li>\n";
            }

            listText += "<li" + KARAS.constructListItemText
                (sequentialList.items[sequentialList.items.Count - 1]);

            return listText;
        }

        private static string constructSameLevelSequentialListText
            (int previousLevel, SequentialList sequentialList, Dictionary<int, bool> listTypeHash)
        {
            //Close previous list item.
            string listText = "";

            if (listTypeHash[previousLevel] == KARAS.ListTypeUl)
            {
                listText += "</li>\n</ul>\n";
            }
            else
            {
                listText += "</li>\n</ol>\n";
            }

            if (sequentialList.type == KARAS.ListTypeUl)
            {
                listText += "<ul>\n";
            }
            else
            {
                listText += "<ol>\n";
            }

            for (int i = 0; i < sequentialList.items.Count - 1; i += 1)
            {
                listText +=
                    "<li" + KARAS.constructListItemText(sequentialList.items[i]) + "</li>\n";
            }

            listText += "<li" + 
                KARAS.constructListItemText(sequentialList.items[sequentialList.items.Count - 1]);

            //Note, it is important to update hash.
            listTypeHash[sequentialList.level] = sequentialList.type;

            return listText;
        }

        private static string constructListItemText(string listItemText)
        {
            listItemText = KARAS.convertInlineMarkup(listItemText);
            bool isSpecialOption = false;
            string[] listItemTexts = splitOption(listItemText, ref isSpecialOption);

            if (listItemTexts.Length > 1)
            {
                listItemText = " value=\"" + listItemTexts[1] + "\">";
            }
            else
            {
                listItemText = ">";
            }

            listItemText += listItemTexts[0];

            return listItemText;
        }
        
        private static string constructListCloseText
            (int previousLevel, Dictionary<int, bool> listTypeHash)
        {
            //Close previous list item.
            string listText = "</li>\n";

            //Close all.
            for (int level = previousLevel; level > 1; level -= 1)
            {
                if (listTypeHash[level] == KARAS.ListTypeUl)
                {
                    listText += "</ul>\n";
                }
                else
                {
                    listText += "</ol>\n";
                }

                listText += "</li>\n";
            }

            if (listTypeHash[1] == KARAS.ListTypeUl)
            {
                listText += "</ul>";
            }
            else
            {
                listText += "</ol>";
            }

            return listText;
        }

        public static string convertDefList(string text)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiMarks = 1;
            const int mgiMarkedupText = 2;
            const int mgiNextMarks = 3;
            const int mgiBreaks = 4;

            Match match = null;
            int nextMatchIndex = 0;
            int indexOfDefListText = 0;
            bool defListIsOpen = false;
            string newText = "";

            while (true)
            {
                match = KARAS.RegexDefList.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                if (defListIsOpen == false)
                {
                    defListIsOpen = true;
                    indexOfDefListText = match.Groups[mgiAllText].Index;
                    newText = "<dl>\n";
                }

                if (match.Groups[mgiMarks].Length == 1)
                {
                    newText += "<dt>"
                               + KARAS.convertInlineMarkup
                                   (match.Groups[mgiMarkedupText].Value.Trim())
                               + "</dt>\n";
                }
                else
                {
                    newText += "<dd>"
                               + KARAS.convertInlineMarkup
                                   (match.Groups[mgiMarkedupText].Value.Trim())
                               + "</dd>\n";
                }

                if (match.Groups[mgiNextMarks].Length == 0)
                {
                    newText = KARAS.encloseWithLinebreak(newText + "</dl>");
                    nextMatchIndex = indexOfDefListText + newText.Length;
                    text = KARAS.removeAndInsertText(text,
                                                     indexOfDefListText,
                                                     match.Groups[mgiAllText].Index
                                                     + match.Groups[mgiAllText].Length
                                                     - match.Groups[mgiBreaks].Length
                                                     - indexOfDefListText,
                                                     KARAS.encloseWithLinebreak(newText));
                    indexOfDefListText = 0;
                    defListIsOpen = false;
                    newText = "";
                    continue;
                }
                
                nextMatchIndex = match.Groups[mgiNextMarks].Index;                
            }

            return text;
        }

        public static string convertHeading(string text, int startLevelOfHeading)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiMarks = 1;
            const int mgiMarkedupText = 2;
            const int mgiBreaks = 3;

            const int maxLevelOfHeading = 6;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexHeading.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                string newText = "";
                int level = match.Groups[mgiMarks].Length;
                level = level + startLevelOfHeading - 1;

                if (level >= maxLevelOfHeading + 1)
                {
                    newText = KARAS.encloseWithLinebreak("<hr>");
                }
                else
                {
                    //Note, it is important to convert inline markups first,
                    //to convert inline markup's options first.
                    string markedupText
                        = KARAS.convertInlineMarkup(match.Groups[mgiMarkedupText].Value);
                    bool isSpecialOption = false;
                    string[] markedupTexts = KARAS.splitOption(markedupText, ref isSpecialOption);
                    string id = "";

                    if (markedupTexts.Length > 1)
                    {
                        id = " id=\"" + markedupTexts[1] + "\"";
                    }

                    newText = "<h" + level + id + ">"
                              + markedupTexts[0]
                              + "</h" + level + ">";
                    newText = KARAS.encloseWithLinebreak(newText);
                }

                nextMatchIndex = match.Groups[mgiAllText].Index + newText.Length;
                text = KARAS.removeAndInsertText(text,
                                                 match.Groups[mgiAllText].Index,
                                                 match.Groups[mgiAllText].Length
                                                 - match.Groups[mgiBreaks].Length,
                                                 KARAS.encloseWithLinebreak(newText));
            }

            return text;
        }

        public static string convertBlockLink(string text)
        {
            //match group index.
            const int mgiAllText = 0;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexBlockLink.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                string newText = KARAS.convertInlineMarkup(match.Groups[mgiAllText].Value).Trim();

                if (KARAS.textIsParagraph(newText))
                {
                    newText = "<p>" + newText + "</p>";
                }

                newText = KARAS.encloseWithLinebreak(newText);
                nextMatchIndex = match.Groups[mgiAllText].Index + newText.Length;
                text = KARAS.removeAndInsertText(text,
                                                 match.Groups[mgiAllText].Index,
                                                 match.Groups[mgiAllText].Length,
                                                 KARAS.encloseWithLinebreak(newText));
            }

            return text;
        }

        private static bool textIsParagraph(string text)
        {
            string restText = KARAS.RegexLinkElement.Replace(text, "");
            restText = restText.Trim();

            if (restText.Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string convertParagraph(string text)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiMarkedupText = 1;
            const int mgiLTMarks = 2;

            //means \n\n length.
            const int lineBreaks = 2;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexParagraph.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }
                
                if (match.Groups[mgiLTMarks].Length == 1)
                {
                    //Note, it is important to exclude line breaks (like \n).
                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiMarkedupText].Length;
                    continue;
                }

                string markedupText = match.Groups[mgiMarkedupText].Value.Trim();

                if (markedupText.Length == 0)
                {
                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiAllText].Length;
                    continue;
                }

                string newText = "<p>" + KARAS.convertInlineMarkup(markedupText) + "</p>\n";
                newText = KARAS.encloseWithLinebreak(newText);
                nextMatchIndex = match.Groups[mgiAllText].Index + newText.Length - lineBreaks;
                text = KARAS.removeAndInsertText(text,
                                                 match.Groups[mgiAllText].Index,
                                                 match.Groups[mgiAllText].Length,
                                                 newText);
            }

            return text;
        }

        public static string reduceBlankLine(string text)
        {
            //match group index.
            const int mgiLineBreakCode = 0;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexBlankLine.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                text = text.Remove(match.Groups[mgiLineBreakCode].Index,
                                   match.Groups[mgiLineBreakCode].Length);
                nextMatchIndex = match.Groups[mgiLineBreakCode].Index;
            }

            return text.Trim();
        }

        public static string reduceEscape(string text)
        {
            //match group index.
            const int mgiEscapes = 0;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexEscape.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                int reduceLength = (int)Math.Round
                    ((double)(match.Groups[mgiEscapes].Length) / 2, MidpointRounding.AwayFromZero);
                
                text = text.Remove(match.Groups[mgiEscapes].Index, reduceLength);

                nextMatchIndex = match.Groups[mgiEscapes].Index
                                 + match.Groups[mgiEscapes].Length
                                 - reduceLength;
            }

            return text;
        }





        private class InlineMarkupMatch
        {
            public int type;
            public int index;
            public string marks;

            public InlineMarkupMatch()
            {
                this.type = -1;
                this.index = -1;
                this.marks = "";
            }
        }

        public static string convertInlineMarkup(string text)
        {
            //match group index.
            //const int mgiAllText = 0;
            const int mgiEscapes = 1;
            const int mgiMarks = 2;

            Stack<InlineMarkupMatch> matchStack = new Stack<InlineMarkupMatch>();
            Match match = null;
            int nextMatchIndex = 0;

            text = convertLineBreak(text);

            while (true)
            {
                match = KARAS.RegexInlineMarkup.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                if (match.Groups[mgiEscapes].Length % 2 == 1)
                {
                    nextMatchIndex = match.Groups[mgiMarks].Index + 1;
                    continue;
                }

                InlineMarkupMatch inlineMarkupMatch = KARAS.constructInlineMarkupMatch
                    (match.Groups[mgiMarks].Index, match.Groups[mgiMarks].Value);
                InlineMarkupMatch preMatch;
                string newText = "";
                string closeMarks = "";

                if (inlineMarkupMatch.type >= KARAS.InlineMarkupTypeLinkOpen)
                {
                    //InlieneMarkupType*Close - 1 = InlineMarkupType*Open
                    preMatch
                        = KARAS.getPreMatchedInlineMarkup(matchStack, inlineMarkupMatch.type - 1);
                    KARAS.handleLinkAndInlineGroupMatch(text,
                                                        preMatch,
                                                        inlineMarkupMatch,
                                                        matchStack,
                                                        ref nextMatchIndex,
                                                        ref newText,
                                                        ref closeMarks);
                    if (nextMatchIndex != -1)
                    {
                        continue;
                    }
                }
                else
                {
                    preMatch = KARAS.getPreMatchedInlineMarkup(matchStack, inlineMarkupMatch.type);
                    KARAS.handleBasicInlineMarkupMatch(text,
                                                       preMatch,
                                                       inlineMarkupMatch,
                                                       matchStack,
                                                       ref nextMatchIndex,
                                                       ref newText,
                                                       ref closeMarks);
                    if (nextMatchIndex != -1)
                    {
                        continue;
                    }
                }

                //It is important to trim close marks to exclude whitespace out of syntax.
                text = KARAS.removeAndInsertText(text, 
                                                 preMatch.index, 
                                                 inlineMarkupMatch.index
                                                 + inlineMarkupMatch.marks.Trim().Length
                                                 - preMatch.index,
                                                 newText);
                nextMatchIndex = preMatch.index + newText.Length - closeMarks.Length;
            }

            return text;
        }

        public static string convertLineBreak(string text)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiEscapes = 1;
            const int mgiLineBreak = 2;

            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexLineBreak.Match(text, nextMatchIndex);

                if (match.Success == false)
                {
                    break;
                }

                if (match.Groups[mgiEscapes].Length % 2 == 1)
                {
                    nextMatchIndex = match.Groups[mgiAllText].Index
                                     + match.Groups[mgiAllText].Length;
                    continue;
                }

                string newText = "<br>\n";
                text = KARAS.removeAndInsertText(text,
                                                 match.Groups[mgiLineBreak].Index,
                                                 match.Groups[mgiLineBreak].Length,
                                                 newText);
                nextMatchIndex = match.Groups[mgiLineBreak].Index + newText.Length;
            }

            return text;
        }

        private static InlineMarkupMatch constructInlineMarkupMatch(int index, string marks)
        {
            InlineMarkupMatch inlineMarkupMatch = new InlineMarkupMatch();

            for (int i = 0; i < KARAS.InlineMarkupSets.Length; i += 1)
            {
                if (marks[0] == KARAS.InlineMarkupSets[i][0][0])
                {
                    inlineMarkupMatch.type = i;
                    inlineMarkupMatch.index = index;
                    inlineMarkupMatch.marks = marks;
                    break;
                }
            }

            return inlineMarkupMatch;
        }

        private static void handleLinkAndInlineGroupMatch
            (string text, InlineMarkupMatch openMatch, InlineMarkupMatch closeMatch,
            Stack<InlineMarkupMatch> matchStack, ref int nextMatchIndex,
            ref string newText, ref string closeMarks)
        {
            if (closeMatch.type == KARAS.InlineMarkupTypeLinkOpen
                || closeMatch.type == KARAS.InlineMarkupTypeInlineGroupOpen)
            {
                matchStack.Push(closeMatch);
                nextMatchIndex = closeMatch.index + closeMatch.marks.Length;
                return;
            }

            if (openMatch == null)
            {
                nextMatchIndex = closeMatch.index + closeMatch.marks.Length;
                return;
            }

            int markedupTextIndex = openMatch.index + openMatch.marks.Length;
            string markedupText = text.Substring
                (markedupTextIndex, closeMatch.index - markedupTextIndex);
            string openMarks = KARAS.removeWhiteSpace(openMatch.marks);
            closeMarks = KARAS.removeWhiteSpace(closeMatch.marks);

            if (closeMatch.type == KARAS.InlineMarkupTypeLinkClose)
            {
                KARAS.constructLinkText
                    (markedupText, ref newText, ref openMarks, ref closeMarks);
            }
            else
            {
                KARAS.constructInlineGroupText
                    (markedupText, ref newText, ref openMarks, ref closeMarks);
            }

            if (openMarks.Length > 1)
            {
                openMatch.marks = openMarks;
            }
            else
            {
                while (true)
                {
                    if (matchStack.Pop().type == openMatch.type)
                    {
                        break;
                    }
                }
            }

            nextMatchIndex = -1;
            return;
        }

        private static void handleBasicInlineMarkupMatch
            (string text, InlineMarkupMatch openMatch, InlineMarkupMatch closeMatch,
            Stack<InlineMarkupMatch> matchStack, ref int nextMatchIndex,
            ref string newText, ref string closeMarks)
        {
            if (openMatch == null)
            {
                matchStack.Push(closeMatch);
                nextMatchIndex = closeMatch.index + closeMatch.marks.Length;
                return;
            }

            int markedupTextIndex = openMatch.index + openMatch.marks.Length;
            string markedupText = text.Substring
                (markedupTextIndex, closeMatch.index - markedupTextIndex).Trim();

            if (openMatch.type <= KARAS.InlineMarkupTypeSupRuby
                && openMatch.marks.Length >= 3 && closeMatch.marks.Length >= 3)
            {
                KARAS.constructSecondInlineMarkupText
                    (markedupText, openMatch, closeMatch, ref newText, ref closeMarks);
            }
            else
            {
                KARAS.constructFirstInlineMarkupText
                    (markedupText, openMatch, closeMatch, ref newText, ref closeMarks);
            }

            while (true)
            {
                if (matchStack.Pop().type == closeMatch.type)
                {
                    break;
                }
            }

            nextMatchIndex = -1;
            return;
        }

        private static InlineMarkupMatch getPreMatchedInlineMarkup
            (Stack<InlineMarkupMatch> matchStack, int inlineMarkupType)
        {
            //Note, check from latest match.
            for (int i = 0; i < matchStack.Count; i += 1)
            {
                if (matchStack.ElementAt<InlineMarkupMatch>(i).type == inlineMarkupType)
                {
                    return matchStack.ElementAt<InlineMarkupMatch>(i);
                }
            }

            return null;
        }

        private static void constructLinkText(string markedupText, 
            ref string newText, ref string openMarks, ref string closeMarks)
        {
            bool isSpecialOption = false;
            string[] markedupTexts = KARAS.splitOption(markedupText, ref isSpecialOption);
            string url = markedupTexts[0];

            if (openMarks.Length >= 5 && closeMarks.Length >= 5)
            {
                newText = "<a href=\"" + url + "\">"
                           + KARAS.constructMediaText(url, markedupTexts) + "</a>";
            }
            else if (openMarks.Length >= 3 && closeMarks.Length >= 3)
            {
                newText = KARAS.constructMediaText(url, markedupTexts);
            }
            else
            {
                string aliasText = "";

                if (markedupTexts.Length > 1)
                {
                    aliasText = markedupTexts[1];
                }
                else
                {
                    aliasText = url;
                }

                newText = "<a href=\"" + url + "\">" + aliasText + "</a>";
            }

            int markDiff = openMarks.Length - closeMarks.Length;

            if (markDiff > 0)
            {
                openMarks = openMarks.Remove(markDiff);
                closeMarks = "";
            }
            else
            {
                openMarks = "";
                closeMarks = closeMarks.Remove(-markDiff);
            }

            newText = openMarks + newText + closeMarks;
        }

        private static string constructMediaText(string url, string[] markedupTexts)
        {
            string mediaText = "";
            string option = "";
            string reservedAttribute = "";
            string otherAttribute = "";
            string embedAttribute = "";
            int mediaType = KARAS.getMediaType(KARAS.getFileExtension(url));

            if (markedupTexts.Length > 1)
            {
                option = markedupTexts[1];
                KARAS.constructObjectAndEmbedAttributes
                    (option, ref reservedAttribute, ref otherAttribute, ref embedAttribute);
                option = " " + markedupTexts[1];
            }

            switch (mediaType)
            {
                case KARAS.MediaTypeImage:
                    {
                        mediaText = "<img src=\"" + url + "\"" + option + ">";
                        break;
                    }
                case KARAS.MediaTypeAudio:
                    {
                        mediaText = "<audio src=\"" + url + "\"" + option + ">"
                                    + "<object data=\"" + url + "\"" + reservedAttribute + ">"
                                    + otherAttribute
                                    + "<embed src=\"" + url + "\"" + embedAttribute
                                    + "></object></audio>";
                        break;
                    }
                case KARAS.MediaTypeVideo:
                    {
                        mediaText = "<video src=\"" + url + "\"" + option + ">"
                                    + "<object data=\"" + url + "\"" + reservedAttribute + ">"
                                    + otherAttribute
                                    + "<embed src=\"" + url + "\"" + embedAttribute
                                    + "></object></video>";
                        break;
                    }
                default:
                    {
                        mediaText = "<object data=\"" + url + "\"" + reservedAttribute + ">"
                                    + otherAttribute
                                    + "<embed src=\"" + url + "\"" + embedAttribute
                                    + "></object>";
                        break;
                    }
            }

            return mediaText;
        }

        public static int getMediaType(string extension)
        {
            Match match = null;

            for (int i = 0; i < KARAS.MediaExtensions.Length; i += 1)
            {
                match = Regex.Match(extension, KARAS.MediaExtensions[i], RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    return i;
                }
            }

            return KARAS.MediaTypeUnknown;
        }

        public static string getFileExtension(string text)
        {
            //match group index.
            //const int mgiAllText = 0;
            const int mgiFileExtension = 1;

            Match match = KARAS.RegexFileExtension.Match(text);

            if (match.Success)
            {
                return match.Groups[mgiFileExtension].Value;
            }
            else
            {
                return "";
            }
        }

        private static void constructObjectAndEmbedAttributes
            (string option, ref string reservedAttribute,
            ref string otherAttribute, ref string embedAttribute)
        {
            Dictionary<string, string> parameterHash = KARAS.constructParameterHash(option);

            foreach (string name in parameterHash.Keys)
            {
                if (attributeIsReserved(name))
                {
                    reservedAttribute += name + "=\"" + parameterHash[name] + "\" ";
                }
                else
                {
                    otherAttribute += "<param name=\"" + name
                                      + "\" value=\"" + parameterHash[name] + "\">";
                }

                embedAttribute += name + "=\"" + parameterHash[name] + "\" ";
            }

            if (reservedAttribute.Length > 0)
            {
                reservedAttribute = " " + reservedAttribute.Trim();
            }

            if (embedAttribute.Length > 0)
            {
                embedAttribute = " " + embedAttribute.Trim();
            }
        }

        private static Dictionary<string, string> constructParameterHash(string option)
        {
            //match group index.
            const int mgiAllText = 0;
            const int mgiName = 1;
            const int mgiValue = 2;

            Dictionary<string, string> parameterHash = new Dictionary<string, string>();
            Match match = null;
            int nextMatchIndex = 0;

            while (true)
            {
                match = KARAS.RegexStringTypeAttribute.Match(option);

                if (match.Success == false)
                {
                    break;
                }

                parameterHash[match.Groups[mgiName].Value] = match.Groups[mgiValue].Value;
                option = option.Remove
                            (match.Groups[mgiAllText].Index, match.Groups[mgiAllText].Length);
                nextMatchIndex = match.Groups[mgiAllText].Index;
            }

            string[] logicalValues = option.Split();

            foreach (string value in logicalValues)
            {
                if (value.Length > 0)
                {
                    parameterHash[value] = "true";
                }
            }

            return parameterHash;
        }

        private static bool attributeIsReserved(string attribute)
        {
            foreach (string reservedAttribute in KARAS.ReservedObjectAttributes)
            {
                if (String.Compare(attribute, reservedAttribute, true) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void constructInlineGroupText(string markedupText,
            ref string newText, ref string openMarks, ref string closeMarks)
        {
            bool isSpecialOption = false;
            string[] markedupTexts = KARAS.splitOption(markedupText, ref isSpecialOption);
            string idClass = "";

            if (openMarks.Length >= 3 && closeMarks.Length >= 3)
            {
                idClass = " id=\"";
            }
            else
            {
                idClass = " class=\"";
            }

            if (markedupTexts[0].Length == 0)
            {
                idClass = "";
            }
            else
            {
                idClass += markedupTexts[0] + "\"";
            }

            if (markedupTexts.Length > 1)
            {
                newText = markedupTexts[1];
            }
            else
            {
                newText = "";
            }

            int markDiff = openMarks.Length - closeMarks.Length;

            if (markDiff > 0)
            {
                openMarks = openMarks.Remove(markDiff);
                closeMarks = "";
            }
            else
            {
                openMarks = "";
                closeMarks = closeMarks.Remove(-markDiff);
            }

            newText = openMarks + "<span" + idClass + ">" + newText + "</span>" + closeMarks;
        }

        private static void constructSecondInlineMarkupText
            (string markedupText, InlineMarkupMatch openMatch, InlineMarkupMatch closeMatch,
             ref string newText, ref string closeMarks)
        {
            string[] inlineMarkupSet = KARAS.InlineMarkupSets[openMatch.type];
            string openMarks = openMatch.marks.Remove(0, 3);
            closeMarks = closeMatch.marks.Remove(0, 3);
            string openTag = "";
            string closeTag = "";

            if (openMatch.type == KARAS.InlineMarkupTypeSupRuby)
            {
                openTag = "<ruby>";
                closeTag = "</ruby>";

                bool hasSpecialOption = false;
                string[] markedupTexts = KARAS.splitOptions(markedupText, ref hasSpecialOption);
                markedupText = markedupTexts[0];

                for (int i = 1; i < markedupTexts.Length; i += 2)
                {
                    markedupText += "<rp> (</rp><rt>" + markedupTexts[i] + "</rt><rp>) </rp>";

                    if (i + 1 < markedupTexts.Length)
                    {
                        markedupText += markedupTexts[i + 1];
                    }
                }
            }
            else
            {
                openTag = "<" + inlineMarkupSet[2] + ">";
                closeTag = "</" + inlineMarkupSet[2] + ">";

                if (openMatch.type == KARAS.InlineMarkupTypeDefAbbr)
                {
                    openTag = "<" + inlineMarkupSet[1] + ">" + openTag;
                    closeTag = closeTag + "</" + inlineMarkupSet[1] + ">";
                }

                if (openMatch.type == KARAS.InlineMarkupKbdSamp
                    || openMatch.type == KARAS.InlineMarkupVarCode)
                {
                    markedupText = KARAS.escapeHTMLSpecialCharacters(markedupText);
                }
            }

            newText = openMarks + openTag + markedupText + closeTag + closeMarks;
        }

        private static void constructFirstInlineMarkupText
            (string markedupText, InlineMarkupMatch openMatch, InlineMarkupMatch closeMatch,
             ref string newText, ref string closeMarks)
        {
            string[] inlineMarkupSet = KARAS.InlineMarkupSets[openMatch.type];
            string openMarks = openMatch.marks.Remove(0, 2);
            closeMarks = closeMatch.marks.Remove(0, 2);
            string openTag = "<" + inlineMarkupSet[1] + ">";
            string closeTag = "</" + inlineMarkupSet[1] + ">";

            if (openMatch.type == KARAS.InlineMarkupVarCode
                || openMatch.type == KARAS.InlineMarkupKbdSamp)
            {
                markedupText = KARAS.escapeHTMLSpecialCharacters(markedupText);
            }

            newText = openMarks + openTag + markedupText + closeTag + closeMarks;
        }
    }
}