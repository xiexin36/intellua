using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    internal class MemberChain
    {
        private List<Word> m_elements;

        private int m_endPos;

        private bool m_isNamespace;

        private Function m_lastFunction;

        private int m_startPos;

        private MemberChain()
        {
            m_elements = new List<Word>();
            m_startPos = m_endPos = -1;
            m_isNamespace = false;
        }

        private enum PaserState
        {
            searchWordEnd,
            searchWordStart,
            searchSeperator,
            searchBracket
        };

        public List<Word> Elements
        {
            get { return m_elements; }
            set { m_elements = value; }
        }

        public int EndPos
        {
            get { return m_endPos; }
            private set { m_endPos = value; }
        }

        public bool IsNamespace
        {
            get { return m_isNamespace; }
            set { m_isNamespace = value; }
        }
        public Function LastFunction
        {
            get { return m_lastFunction; }
            private set { m_lastFunction = value; }
        }

        public int StartPos
        {
            get { return m_startPos; }
            private set { m_startPos = value; }
        }
        public static MemberChain ParseBackward(IntelluaSource source, int pos = -1)
        {
            const string seperator = ".:";
            const string lbracket = "([{";
            const string rbracket = ")]}";
            const string operators = "=+-*/;";
            Byte[] str = source.RawText;
            if (pos < 0)
            {
                pos = source.getRawPos() - 1;
            }
            PaserState state = PaserState.searchWordEnd;

            MemberChain rst = new MemberChain();
            int wordStart = pos;
            int wordEnd = pos;

            int bracketLevel = 0;

            bool isFuncion = false;

            while (pos >= 0 && pos < str.Length)
            {
                if (str[pos] > 127)
                {
                    pos--;
                    continue;
                }
                char c = Convert.ToChar(str[pos]);
                bool isComment = Parser.isComment(source, pos);
                bool isString = Parser.isString(source, pos);

                switch (state)
                {
                    case PaserState.searchWordStart:
                        if (isString) return rst;
                        if (!char.IsLetterOrDigit(c) || isComment || pos == 0)
                        {
                            wordStart = pos + 1;
                            if (pos == 0 && char.IsLetterOrDigit(c)) wordStart = 0;
                            Byte[] bword = SubArray(str, wordStart, wordEnd - wordStart + 1);
                            string word = Encoding.UTF8.GetString(bword);//str.Substring(wordStart, wordEnd - wordStart + 1);
                            //word.Trim();
                            {
                                //int p = source.getRawPos(wordStart);
                                rst.Elements.Insert(0, new Word(word, isFuncion, wordStart));
                                isFuncion = false;
                                rst.StartPos = pos;
                            }
                            state = PaserState.searchSeperator;
                        }
                        else
                        {
                            pos--;
                        }

                        break;

                    case PaserState.searchWordEnd:
                        if (isComment)
                        {
                            pos--;
                            break;
                        }
                        if (isString) return rst;
                        if (operators.Contains(c)) return rst;

                        if (seperator.Contains(c))
                        {
                            if (rst.Elements.Count == 0)
                            {
                                //int p = source.getRawPos(pos);
                                rst.Elements.Add(new Word("", false, pos));
                            }
                        }

                        if (rbracket.Contains(c))
                        {
                            if (rst.Elements.Count == 0)
                            {
                                //int p = source.getRawPos(pos);
                                rst.Elements.Add(new Word("", false, pos));
                            }
                            state = PaserState.searchBracket;
                            break;
                        }
                        if (char.IsLetterOrDigit(c))
                        {
                            wordEnd = pos;
                            if (rst.EndPos < 0) rst.EndPos = pos;
                            state = PaserState.searchWordStart;
                        }
                        else
                        {
                            pos--;
                        }
                        break;

                    case PaserState.searchSeperator:
                        if (isString) return rst;

                        if (char.IsWhiteSpace(c) || isComment)
                        {
                            pos--;
                        }
                        else if (seperator.Contains(c))
                        {
                            state = PaserState.searchWordEnd;
                            pos--;
                        }
                        else
                        {
                            //end
                            return rst;
                        }
                        break;

                    case PaserState.searchBracket:
                        if (!isComment && !isString)
                        {
                            if (rbracket.Contains(c)) bracketLevel++;
                            else if (lbracket.Contains(c))
                            {
                                bracketLevel--;
                                if (bracketLevel == 0)
                                {
                                    if (c == '(') isFuncion = true;
                                    state = PaserState.searchWordEnd;
                                }
                            }
                        }
                        pos--;
                        break;
                }
            }

            return rst;
        }

        public static MemberChain ParseFoward(IntelluaSource source, int pos)
        {
            const string seperator = ".:";
            const string lbracket = "([{";
            const string rbracket = ")]}";
            const string operators = "=+-*/;";
            Byte[] str = source.RawText;

            PaserState state = PaserState.searchWordStart;

            MemberChain rst = new MemberChain();
            int wordStart = pos;
            int wordEnd = pos;

            int bracketLevel = 0;

            while (pos < str.Length)
            {
                if (str[pos] > 127)
                {
                    pos++;
                    continue;
                }
                char c = Convert.ToChar(str[pos]);

                bool isComment = Parser.isComment(source, pos);
                bool isString = Parser.isString(source, pos);

                switch (state)
                {
                    case PaserState.searchWordEnd:
                        if (isString) return rst;
                        if (!char.IsLetterOrDigit(c) || isComment || pos == str.Length - 1)
                        {
                            wordEnd = pos;
                            string word;
                            if (pos == str.Length - 1)
                            {
                                Byte[] bword = SubArray(str, wordStart, wordEnd - wordStart + 1);
                                word = Encoding.UTF8.GetString(bword);
                            }
                            else
                            {
                                Byte[] bword = SubArray(str, wordStart, wordEnd - wordStart);
                                word = Encoding.UTF8.GetString(bword);
                                //word = str.Substring(wordStart, wordEnd - wordStart);
                            }
                            word.Trim();
                            {
                                //int p = wordStart;//source.getRawPos(wordStart);
                                rst.Elements.Add(new Word(word, false, wordStart));

                                rst.EndPos = pos;
                            }
                            state = PaserState.searchSeperator;
                        }
                        else
                        {
                            pos++;
                        }

                        break;

                    case PaserState.searchWordStart:
                        if (isString) return rst;
                        if (operators.Contains(c)) return rst;
                        if (isComment)
                        {
                            pos++;
                            break;
                        }

                        if (char.IsLetterOrDigit(c))
                        {
                            wordStart = pos;
                            if (rst.StartPos < 0) rst.StartPos = pos;
                            state = PaserState.searchWordEnd;
                        }
                        else
                        {
                            pos++;
                        }
                        break;

                    case PaserState.searchSeperator:
                        if (isString) return rst;
                        if (lbracket.Contains(c))
                        {
                            if (c == '(')
                            {
                                if (rst.Elements.Count > 0)
                                {
                                    rst.Elements[rst.Elements.Count - 1].IsFunction = true;
                                }
                            }
                            state = PaserState.searchBracket;
                            break;
                        }
                        if (seperator.Contains(c))
                        {
                            state = PaserState.searchWordStart;
                            pos++;
                        }
                        else if (char.IsWhiteSpace(c) || isComment)
                        {
                            pos++;
                        }
                        else
                        {
                            //end
                            return rst;
                        }
                        break;

                    case PaserState.searchBracket:
                        if (!isComment && !isString)
                        {
                            if (lbracket.Contains(c)) bracketLevel++;
                            else if (rbracket.Contains(c))
                            {
                                bracketLevel--;
                                if (bracketLevel == 0)
                                {
                                    state = PaserState.searchSeperator;
                                }
                            }
                        }
                        pos++;
                        break;
                }
            }

            return rst;
        }

        public string getLastElement()
        {
            return Elements[Elements.Count - 1].Name;
        }

        public Type getType(AutoCompleteData data, bool lastAsFuncion = false)
        {
            if (Elements.Count == 0) return null;
            VariableManager variables = data.Variables;
            string word = Elements[0].Name;
            Type t = null;
            if (Elements[0].IsFunction || (Elements.Count == 1 && lastAsFuncion))
            {
                Function func = variables.getFunction(word);
                if (func != null)
                {
                    LastFunction = func;
                    t = func.ReturnType;
                }
            }
            else
            {
                Variable var = variables.getVariable(word, Elements[0].StartPos);
                if (var != null)
                {
                    IsNamespace = var.IsNamespace;
                    t = var.Type;
                }
                else
                {
                    t = data.Types.get(word);
                }
            }

            if (t == null) return null;

            if (Elements.Count == 1) return t;

            for (int i = 1; i < Elements.Count - 1; i++)
            {
                string name = Elements[i].Name;

                if (Elements[i].IsFunction)
                {
                    Function f = t.getMethod(name);
                    if (f != null)
                    {
                        IsNamespace = false;
                        t = f.ReturnType;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    Variable v = t.getMember(name);
                    if (v != null)
                    {
                        IsNamespace = v.IsNamespace;
                        t = v.Type;
                    }
                    else return null;
                }
            }
            //last
            string last = getLastElement();

            if (lastAsFuncion || Elements[Elements.Count - 1].IsFunction)
            {
                IsNamespace = false;
                Function f = t.getMethod(last);
                if (f != null)
                {
                    LastFunction = f;
                    return f.ReturnType;
                }
                else
                {
                    return t;
                }
            }
            else
            {
                Variable v = t.getMember(last);
                if (v != null)
                {
                    IsNamespace = v.IsNamespace;
                    return v.Type;
                }
                else
                {
                    return t;
                }
            }
        }

        public override string ToString()
        {
            string rst = "";
            for (int i = 0; i < Elements.Count; i++)
            {
                rst += Elements[i];
                if (i != Elements.Count - 1)
                {
                    rst += "\n";
                }
            }

            return rst;
        }

        private static Byte[] SubArray(Byte[] data, int index, int length)
        {
            Byte[] result = new Byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }

    internal class Word
    {
        #region Fields (3)

        private bool m_isFunction;

        private string m_name;

        private int m_startPos;

        public Word(string name, bool isFunction, int pos)
        {
            Name = name;
            IsFunction = isFunction;
            StartPos = pos;
        }

        public bool IsFunction
        {
            get { return m_isFunction; }
            set { m_isFunction = value; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public int StartPos
        {
            get { return m_startPos; }
            set { m_startPos = value; }
        }

        #endregion Fields

        #region Constructors (1)
        #endregion Constructors

        #region Properties (3)
        #endregion Properties
    }
}