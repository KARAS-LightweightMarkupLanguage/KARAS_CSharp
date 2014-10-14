
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

using System.Text.RegularExpressions;
using KARAS;

public static class TOC
{
    public static string action(string[] options, string markedupText, string text)
    {
        // Remove heading syntax in pre element.
        text = KARAS.KARAS.replaceTextInPreElement(text, "=", "");

        // match group index.
        const int mgiAllText = 0;
        const int mgiMarks = 1;
        const int mgiMarkedupText = 2;
        const int mgiBreaks = 3;

        int topLevel = 1;
        int bottomLevel = 6;

        if (options.Length > 0)
        {
            topLevel = int.Parse(options[0]);
        }

        if (options.Length > 1)
        {
            bottomLevel = int.Parse(options[1]);
        }

        string newText = "";
        int previousLevel = 0;

        Match match;
        int nextMatchIndex = 0;

        while (true)
        {
            match = KARAS.KARAS.RegexHeading.Match(text, nextMatchIndex);

            if (match.Success == false)
            {
                for (int i = 0; i < previousLevel; i += 1)
                {
                    newText += "</li>\n</ul>\n";
                }

                break;
            }

            if (match.Groups[mgiMarks].Length <= bottomLevel)
            {
                int level = match.Groups[mgiMarks].Length;
                level = level - topLevel + 1;

                if (level > 0)
                {
                    int levelDiff = level - previousLevel;
                    previousLevel = level;

                    if (levelDiff > 0)
                    {
                        for (int i = 0; i < levelDiff; i += 1)
                        {
                            newText += "\n<ul>\n<li>";
                        }
                    }
                    else if (levelDiff < 0)
                    {
                        for (int i = 0; i > levelDiff; i -= 1)
                        {
                            newText += "</li>\n</ul>\n";
                        }

                        newText += "<li>";
                    }
                    else
                    {
                        newText += "</li>\n<li>";
                    }

                    string markedupTextInHeading = KARAS.KARAS.convertInlineMarkup
                                                    (match.Groups[mgiMarkedupText].Value);
                    bool hasSpecialOption = false;
                    string[] markedupTexts = KARAS.KARAS.splitOptions(markedupTextInHeading, ref hasSpecialOption);
                    string itemText = markedupTexts[0];

                    if (markedupTexts.Length > 1)
                    {
                        itemText = "<a href=\"#" + markedupTexts[1].Trim() + "\">" + itemText + "</a>";
                    }

                    newText += itemText;
                }
            }

            nextMatchIndex = match.Groups[mgiAllText].Index
                             + match.Groups[mgiAllText].Length
                             - match.Groups[mgiBreaks].Length;
        }

        return newText.Trim();
    }

}