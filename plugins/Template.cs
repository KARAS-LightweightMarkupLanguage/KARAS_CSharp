﻿
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

//You can use namespace (and the class) in System.dll and KARAS.dll.
//If you want to use some namespace in the other dll,
//you have to add the reference to "CompilerParameters" before compile.

public static class Template
{
    public static string convert(string[] options, string markedupText)
    {
        //Markup syntax is this. "[[plugin::option1::option2…:::markedupText]]"
        //Markedup text(and the syntax) in original text will be removed,
        //and insert return text there.
        //If you wont insert anything text, you have to return ""(empty string).
        return markedupText;
    }

    public static string action(string[] options, string markedupText, string text)
    {
        //Markup syntax is this. "[[[plugin::option1::option2…:::markedupText]]]"
        //"text" is all of the original text.
        //Markedup text(and the syntax) in original text will be removed,
        //and insert return text there.
        //If you wont insert anything text, you have to return ""(empty string).
        return text;
    }
}