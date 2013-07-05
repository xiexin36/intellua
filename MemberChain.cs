using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    class Word {
		#region Fields (3) 

        bool m_isFunction;
       
        string m_name;

		#endregion Fields 

		#region Constructors (1) 

        public Word(string name,bool isFunction) {
            Name = name;
            IsFunction = isFunction;
        }

		#endregion Constructors 

		#region Properties (3) 

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

		#endregion Properties 
    }
    class MemberChain
    {
        private MemberChain()
        {
            m_elements = new List<Word>();
            m_startPos = m_endPos = -1;
            m_isNamespace = false;
        }
        public bool IsNamespace
        {
            get { return m_isNamespace; }
            set { m_isNamespace = value; }
        }
        bool m_isNamespace;

        private List<Word> m_elements;
        public List<Word> Elements
        {
            get { return m_elements; }
            set { m_elements = value; }
        }



        private int m_startPos;
        public int StartPos
        {
            get { return m_startPos; }
            private set { m_startPos = value; }
        }
        private int m_endPos;
        public int EndPos
        {
            get { return m_endPos; }
            private set { m_endPos = value; }
        }

        private Function m_lastFunction;
        public Function LastFunction
        {
            get { return m_lastFunction; }
            private set { m_lastFunction = value; }
        }
        public Type getType(VariableManager variables,bool lastAsFuncion =false)
        {
            if (Elements.Count == 0) return null;
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
                Variable var = variables.getVariable(word);
                if (var != null)
                {
                    IsNamespace = var.IsNamespace;
                    t = var.Type;
                }
            }
            
            if (t == null) return null;

            if (Elements.Count == 1) return t;

            for (int i = 1; i < Elements.Count - 1; i++)
            {
                string name = Elements[i].Name;

                if (Elements[i].IsFunction)
                {
                    if (t.Methods.ContainsKey(name))
                    {
                        IsNamespace = false;
                        t = t.Methods[name].ReturnType;
                    }
                    else
                    {
                        return null;
                    }
                }
                else {
                    if (t.Members.ContainsKey(name))
                    {
                        IsNamespace = t.Members[name].IsNamespace;
                        t = t.Members[name].Type;
                    }
                    else return null;
                }
            }
            //last
            string last = getLastElement();

            if (lastAsFuncion || Elements[Elements.Count - 1].IsFunction)
            {
                IsNamespace = false;
                if (t.Methods.ContainsKey(last))
                {
                    LastFunction = t.Methods[last];
                    return t.Methods[last].ReturnType;
                }
                else
                {
                    return t;
                }
            }
            else {
                if (t.Members.ContainsKey(last))
                {
                    IsNamespace = t.Members[last].IsNamespace;
                    return t.Members[last].Type;
                }
                else {
                    return t;
                }
            }
        }

        public string getLastElement()
        {
            return Elements[Elements.Count - 1].Name;
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


        enum PaserState
        {
            searchWordEnd,
            searchWordStart,
            searchSeperator,
            searchBracket
        };
        public static MemberChain ParseBackward(Intellua scintilla, int pos = -1)
        {
            const string seperator = ".:";
            const string lbracket = "([{";
            const string rbracket = ")]}";

            string str = scintilla.Text;
            if (pos < 0)
            {
                pos = scintilla.getDecodedPos() - 1;
            }
            PaserState state = PaserState.searchWordEnd;

            MemberChain rst = new MemberChain();
            int wordStart = pos;
            int wordEnd = pos;

            int bracketLevel = 0;

            bool isFuncion = false;


            while (pos >= 0)
            {
                char c = str[pos];
                bool isComment = Parser.isComment(scintilla, pos);
                bool isString = Parser.isString(scintilla, pos);



                switch (state)
                {
                    case PaserState.searchWordStart:
                        if (isString) return rst;
                        if (!char.IsLetterOrDigit(c) || isComment || pos == 0)
                        {
                            wordStart = pos;
                            string word;
                            if (pos != 0) word = str.Substring(wordStart + 1, wordEnd - wordStart);
                            else word = str.Substring(wordStart, wordEnd - wordStart + 1);
                            word.Trim();
                            {
                                rst.Elements.Insert(0, new Word(word, isFuncion));
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
                        if (isString) return rst;
                        if (isComment)
                        {
                            pos--;
                            break;
                        }
                        if (seperator.Contains(c)) {
                            if (rst.Elements.Count == 0)
                            {
                                rst.Elements.Add(new Word("", false));
                            }
                        }

                        if (rbracket.Contains(c))
                        {
                            if (rst.Elements.Count == 0) {
                                rst.Elements.Add(new Word("",false));
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
                        if (seperator.Contains(c))
                        {
                            state = PaserState.searchWordEnd;
                            pos--;
                        }
                        else if (char.IsWhiteSpace(c) || isComment)
                        {
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

        public static MemberChain ParseFoward(Intellua scintilla, int pos)
        {
            const string seperator = ".:";
            const string lbracket = "([{";
            const string rbracket = ")]}";
            const string operators = "=+-*/";
            string str = scintilla.Text;

            PaserState state = PaserState.searchWordStart;

            MemberChain rst = new MemberChain();
            int wordStart = pos;
            int wordEnd = pos;

            int bracketLevel = 0;


            while (pos < str.Length)
            {
                char c = str[pos];

                bool isComment = Parser.isComment(scintilla, pos);
                bool isString = Parser.isString(scintilla, pos);

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
                                word = str.Substring(wordStart, wordEnd - wordStart + 1);
                            }
                            else
                            {
                                word = str.Substring(wordStart, wordEnd - wordStart);
                            }
                            word.Trim();
                            {
                                rst.Elements.Add(new Word(word,false));

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
                            if (c == '(') {
                                if (rst.Elements.Count > 0) {
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
    }
}
