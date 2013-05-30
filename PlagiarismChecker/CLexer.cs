using System;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using FastColoredTextBoxNS;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PlagiarismChecker
{
    class CLexer
    {
        /// <summary>
        /// 
        /// </summary>
        private bool hasStdio;
        /// <summary>
        /// 
        /// </summary>
        private bool hasString;
        /// <summary>
        /// 
        /// </summary>
        private HashSet<String> reservedwords = new HashSet<string>();
        /// <summary>
        /// 
        /// </summary>
        private void InitReservedWords()
        {
            reservedwords.Add("auto");
            reservedwords.Add("double");
            reservedwords.Add("int");
            reservedwords.Add("struct");
            reservedwords.Add("break");
            reservedwords.Add("else");
            reservedwords.Add("long");
            reservedwords.Add("switch");
            reservedwords.Add("case");
            reservedwords.Add("enum");
            reservedwords.Add("register");
            reservedwords.Add("typedef");
            reservedwords.Add("char");
            reservedwords.Add("extern");
            reservedwords.Add("return");
            reservedwords.Add("union");
            reservedwords.Add("const");
            reservedwords.Add("float");
            reservedwords.Add("short");
            reservedwords.Add("unsigned");
            reservedwords.Add("continue");
            reservedwords.Add("for");
            reservedwords.Add("signed");
            reservedwords.Add("void");
            reservedwords.Add("default");
            reservedwords.Add("goto");
            reservedwords.Add("sizeof");
            reservedwords.Add("volatile");
            reservedwords.Add("do");
            reservedwords.Add("while");
            reservedwords.Add("static");
            reservedwords.Add("if");
        }
        /// <summary>
        /// 
        /// </summary>
        private HashSet<String> stdio = new HashSet<string>();
        /// <summary>
        /// 
        /// </summary>
        private void InitStdio()
        {
            stdio.Add("main");
            stdio.Add("getchar");
            stdio.Add("putchar");
            stdio.Add("scanf");
            stdio.Add("printf");
            stdio.Add("puts");
            stdio.Add("gets");
            stdio.Add("sprintf");
            stdio.Add("sscanf");
            stdio.Add("fscanf");
            stdio.Add("fprintf");
        }
        /// <summary>
        /// 
        /// </summary>
        private HashSet<String> cstring = new HashSet<string>();
        /// <summary>
        /// 
        /// </summary>
        private void InitCString()
        {
            cstring.Add("bcmp");
            cstring.Add("bcopy");
            cstring.Add("bzero");
            cstring.Add("index");
            cstring.Add("memcpy");
            cstring.Add("strcpy");
            cstring.Add("strlen");
            cstring.Add("memcmp");
            cstring.Add("strstr");
            cstring.Add("memset");
        }
        /// <summary>
        /// 
        /// </summary>
        private HashSet<char> operates = new HashSet<char>();
        /// <summary>
        /// 
        /// </summary>
        private void InitOperates()
        {
            operates.Add('&');
            operates.Add('+');
            operates.Add('-');
            operates.Add('*');
            operates.Add('/');
            operates.Add('~');
            operates.Add('!');
            operates.Add('^');
            operates.Add('|');
            operates.Add('(');
            operates.Add(')');
            operates.Add('[');
            operates.Add(']');
            operates.Add('>');
            operates.Add('<');
            operates.Add('=');
            operates.Add('.');
            operates.Add('?');
            operates.Add(',');
        }
        /// <summary>
        /// 
        /// </summary>
        private String buffer;
        /// <summary>
        /// 
        /// </summary>
        private String lexerResult;
        /// <summary>
        /// 
        /// </summary>
        private String nextWord;
        /// <summary>
        /// 
        /// </summary>
        private int nowLoc;
        /// <summary>
        /// 
        /// </summary>
        private int depth;
        private string lastWord;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool hasNext()
        {
            return (nowLoc + 1 < buffer.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool isLineEnd()
        {
            return (nowLoc >= buffer.Length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private char getchar()
        {
            if (hasNext()) return buffer[++nowLoc];
            else return ' ';
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private char nowchar()
        {
            return buffer[nowLoc];
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isdigit(char c)
        {
            return (c >= '0' && c <= '9');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isalpha(char c)
        {
            return (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isVarBegin(char c)
        {
            return (c == '_' || isalpha(c));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isVarInside(char c)
        {
            return (c == '_' || isalpha(c) || isdigit(c));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isEmpty(char c)
        {
            return (c == ' ' || c == '\n' || c == '\t' || c == '\r');
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool getNextResult()
        {
            if (isLineEnd()) return false;
            char c = nowchar();
            while (hasNext() && isEmpty(c)) c = getchar();
            if (isEmpty(c)) return false;
            if (c == '/' && hasNext() && buffer[nowLoc + 1] == '/') return false;
            else if (c == '/' && hasNext() && buffer[nowLoc + 1] == '*')
            {
                c = getchar(); c = getchar();
                nextWord = "BEGIN";
                return true;
            }
            else if (c == '*' && hasNext() && buffer[nowLoc + 1] == '/')
            {
                c = getchar(); c = getchar();
                nextWord = "END";
                return true;
            }
            else if (isdigit(c))
            {
                while (hasNext() && isdigit(c))
                {
                    c = getchar();
                }
                nextWord = "NUMBER";
            }
            else if (isVarBegin(c))
            {
                nextWord = "" + c;
                c = getchar();
                while (hasNext() && isVarInside(c))
                {
                    nextWord = nextWord + c;
                    c = getchar();
                }
                if (reservedwords.Contains(nextWord) || hasStdio && stdio.Contains(nextWord) || hasString && cstring.Contains(nextWord)) { }
                else nextWord = "VAR";
            }
            else if (c == '"')
            {
                do
                {
                    c = getchar();
                    if (c == '\\')
                    {
                        c = getchar();
                    }
                } while (hasNext() && c != '"');
                c = getchar();
                nextWord = "STRING";
            }
            else if (c == '\'')
            {
                c = getchar();
                if (c == '\\') c = getchar();
                c = getchar();
                nextWord = "CHAR";
            }
            else if (c == '(')
            {
                int commatime = 1;
                do
                {
                    c = getchar();
                    if (c == '\\')
                    {
                        c = getchar();
                    }
                    if (c == '(') commatime++;
                    else if (c == ')') commatime--;
                } while (hasNext() && commatime > 0);
                c = getchar();
                if ((lastWord == "while" || lastWord == "if" || lastWord == "for")) nextWord = "CONDITION";
                else nextWord = "PARAMETER";
            }
            else if (c == '[')
            {
                do
                {
                    c = getchar();
                } while (hasNext() && c != ']');
                c = getchar();
                nextWord = "LOC";
            }
            else if (c == '=')
            {
                c = getchar();
                nextWord = "=";
            }
            else if (operates.Contains(c))
            {
                nextWord = "";
                while (hasNext() && operates.Contains(c))
                {
                    nextWord = nextWord + c;
                    c = getchar();
                }
            }
            else
            {
                nextWord = "" + c;
                c = getchar();
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private string tabNTimes(int n)
        {
            string returnValue = "";
            for (int i = 0; i < n; ++i) returnValue += '\t';
            return returnValue;
        }
        /// <summary>
        /// 
        /// </summary>
        private const string EOLN = "\r\n";
        /// <summary>
        /// 
        /// </summary>
        private bool inside;
        /// <summary>
        /// 
        /// </summary>
        private string code = "";
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string getInitCode()
        {
            return code;
        }
        /// <summary>
        /// 
        /// </summary>
        private void lineLexer()
        {
            lastWord = "";
            nextWord = "";
            buffer = buffer + " ";
            if (buffer[0] == '\r' || buffer[0] == '/' && buffer[1] == '/' || buffer[0] == '#') return;
            nowLoc = 0;
            bool isNewLine = true;
            bool nothing = true;
            while (getNextResult())
            {
                if (inside)
                {
                    if (nextWord == "END") inside = false;
                    continue;
                }
                else if (nextWord == "LOC") continue;
                else if (nextWord == "BEGIN")
                {
                    inside = true;
                    continue;
                }
                else if (nextWord == "{")
                {
                    if (nothing)
                    {
                        while (lexerResult.Length > 0 && lexerResult[lexerResult.Length - 1] != '\r') lexerResult = lexerResult.Substring(0, lexerResult.Length - 1);
                        if (lexerResult.Length > 0) lexerResult = lexerResult.Substring(0, lexerResult.Length - 1);
                        lexerResult += " {" + EOLN;
                    }
                    else lexerResult += " {" + EOLN;
                    depth++;
                    isNewLine = true;
                    nothing = false;
                }
                else if (nextWord == "}")
                {
                    depth--;
                    if (nothing) lexerResult += tabNTimes(depth) + "}" + EOLN;
                    else lexerResult += EOLN + tabNTimes(depth) + "}" + EOLN;
                    isNewLine = true;
                    nothing = false;

                }
                else if (nextWord == ";")
                {
                    lexerResult += nextWord + EOLN;
                    isNewLine = true;
                    nothing = false;
                }
                else
                {
                    if (isNewLine)
                    {
                        lexerResult += tabNTimes(depth);
                        isNewLine = false;
                    }
                    else lexerResult += " ";
                    lexerResult += nextWord;
                    nothing = false;
                    isNewLine = false;
                }
                lastWord = nextWord;
            }
            if (!isNewLine) lexerResult += "\r\n";
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="code">原始代码</param>
        public CLexer(String _code)
        {
            code = _code;
        }
        /// <summary>
        /// 
        /// </summary>
        public CLexer()
        {
            lexerResult = "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetLexResult()
        {
            inside = false;
            depth = 0;
            hasStdio = true;
            hasString = true;
            InitCString();
            InitOperates();
            InitReservedWords();
            InitStdio();
            lexerResult = "";
            String[] lines = code.Split('\n');
            foreach (String line in lines)
            {
                if (line.Trim().Length > 0)
                {
                    buffer = line;
                    lineLexer();
                }
            }
            return lexerResult;
        }
    }
}
