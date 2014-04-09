using System;
using System.Collections.Generic;
using System.Linq;

namespace Intellua
{
    public enum DeclTokenType
    {
        Identifier,
        Comment,
        Number,
        EOF,
        KW_Static,
        KW_Class,
        OP_LParen,
        OP_RParen,
        OP_LBrace,
        OP_RBrace,
        OP_LSquare,
        OP_RSquare,
        OP_Comma,
        OP_Dot,
        OP_Colon,
        OP_SemiColon,
        OP_Dots,
        OP_Equal,
        OP_Minus,
        OP_Plus,
    };

    internal class DeclToken
    {
        public string Data;

        public DeclTokenType Type;

        public DeclToken(string data, DeclTokenType type)
        {
            Data = data;
            Type = type;
        }
    }

    internal class DeclTokenizer
    {
        private static string EOF = "\0";
        private static string IdentifierChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_\"1234567890";
        private static string IdentifierHeadChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_\"";
        private static TupleList<string, DeclTokenType> Keywords = new TupleList<string, DeclTokenType>
        {
            {"static",DeclTokenType.KW_Static},
            {"class",DeclTokenType.KW_Class},
        };

        private static TupleList<string, DeclTokenType> Operands = new TupleList<string, DeclTokenType>
        {
            {"(",DeclTokenType.OP_LParen},
            {")",DeclTokenType.OP_RParen},
            {"{",DeclTokenType.OP_LBrace},
            {"}",DeclTokenType.OP_RBrace},
            {"[",DeclTokenType.OP_LSquare},
            {"]",DeclTokenType.OP_RSquare},
            {",",DeclTokenType.OP_Comma},
            {":",DeclTokenType.OP_Colon},
            {";",DeclTokenType.OP_SemiColon},
            {"...",DeclTokenType.OP_Dots},
            {".",DeclTokenType.OP_Dot},
            {"=",DeclTokenType.OP_Equal},
            {"-",DeclTokenType.OP_Minus},
            {"+",DeclTokenType.OP_Plus},
        };

        private static string PPNumber = "01234567890.+-xXabcdefABCDEF";
        private string m_data;
        private int m_pos;
        private List<DeclToken> m_result = new List<DeclToken>();

        private State m_state;

        public DeclTokenizer(string str)
        {
            m_data = str + EOF;
            parse();
        }

        private enum State
        {
            Start,
            Identifier,
            LineComment,
            BlockComment,
            Number,
        };

        public List<DeclToken> Result
        {
            get
            {
                return m_result;
            }
        }
        private void addComment(string str)
        {
            if (str.Length == 0 || str[0] != '!') return;
            str = str.Substring(1);
            str = str.Trim();
            str.Replace("\r", "");
            str.Replace("\n", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ");
            m_result.Add(new DeclToken(str, DeclTokenType.Comment));
        }

        private bool match(string str)
        {
            if (m_pos + str.Length > m_data.Length) return false;
            for (int i = 0; i < str.Length; i++)
            {
                if (m_data[m_pos + i] != str[i]) return false;
            }
            return true;
        }

        private bool matchChar(string str)
        {
            return (str.Contains(m_data[m_pos]));
        }
        private void parse()
        {
            m_state = State.Start;
            m_pos = 0;
            int sequenceStart = -1;
            bool running = true;

            int angleBracketLevel = 0;

            while (running)
            {
                switch (m_state)
                {
                    case State.Start:
                        if (matchChar(EOF))
                        {
                            running = false;
                            continue;
                        }

                        if (matchChar(IdentifierHeadChars))
                        {
                            m_state = State.Identifier;
                            sequenceStart = m_pos;
                            continue;
                        }
                        if (match("//"))
                        {
                            m_pos += 2;
                            sequenceStart = m_pos;
                            m_state = State.LineComment;
                            continue;
                        }
                        if (match("/*"))
                        {
                            m_pos += 2;
                            sequenceStart = m_pos;
                            m_state = State.BlockComment;
                            continue;
                        }
                        if (Char.IsDigit(peek()))
                        {
                            sequenceStart = m_pos;
                            m_state = State.Number;
                            continue;
                        }

                        if (Char.IsWhiteSpace(peek()))
                        {
                            m_pos++;
                            continue;
                        }

                        foreach (Tuple<string, DeclTokenType> op in Operands)
                        {
                            if (match(op.Item1))
                            {
                                m_pos += op.Item1.Length - 1;
                                m_result.Add(new DeclToken(op.Item1, op.Item2));
                                break;
                            }
                        }

                        m_pos++;
                        continue;

                    case State.Identifier:
                        if (matchChar(IdentifierChars))
                        {
                            m_pos++;
                            continue;
                        }
                        if (match("<"))
                        {
                            angleBracketLevel++;
                            m_pos++;
                            continue;
                        }
                        if (angleBracketLevel > 0)
                        {
                            if (match(">"))
                            {
                                angleBracketLevel--;
                            }
                            m_pos++;
                            continue;
                        }

                        string str = m_data.Substring(sequenceStart, m_pos - sequenceStart);
                        bool found = false;
                        foreach (Tuple<string, DeclTokenType> kw in Keywords)
                        {
                            if (kw.Item1 == str)
                            {
                                m_result.Add(new DeclToken(kw.Item1, kw.Item2));
                                m_state = State.Start;
                                found = true;
                                break;
                            }
                        }
                        if (found)
                        {
                            break;
                        }

                        m_result.Add(new DeclToken(str, DeclTokenType.Identifier));
                        m_state = State.Start;
                        break;

                    case State.LineComment:
                        if (match("\n") || match(EOF))
                        {
                            addComment(m_data.Substring(sequenceStart, m_pos - sequenceStart));

                            if (match("\n"))
                            {
                                m_pos++;
                            }
                            m_state = State.Start;
                        }
                        else
                        {
                            m_pos++;
                            continue;
                        }
                        break;

                    case State.BlockComment:
                        if (match("*/") || match(EOF))
                        {
                            addComment(m_data.Substring(sequenceStart, m_pos - sequenceStart));
                            if (match("*/"))
                            {
                                m_pos += 2;
                            }
                            m_state = State.Start;
                        }
                        else
                        {
                            m_pos++;
                            continue;
                        }
                        break;

                    case State.Number:
                        if (matchChar(PPNumber))
                        {
                            m_pos++;
                            continue;
                        }
                        if (m_result.Count > 0)
                        {
                            DeclToken last = m_result[m_result.Count - 1];
                            if (last.Type == DeclTokenType.OP_Minus || last.Type == DeclTokenType.OP_Plus)
                            {
                                string sign = last.Data;
                                m_result.RemoveAt(m_result.Count - 1);
                                m_result.Add(new DeclToken(sign + m_data.Substring(sequenceStart, m_pos - sequenceStart), DeclTokenType.Number));
                                m_state = State.Start;
                                continue;
                            }
                        }
                        m_result.Add(new DeclToken(m_data.Substring(sequenceStart, m_pos - sequenceStart), DeclTokenType.Number));
                        m_state = State.Start;
                        break;
                }
            }

            m_result.Add(new DeclToken("", DeclTokenType.EOF));
        }

        private char peek()
        {
            return m_data[m_pos];
        }
    }

    internal class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }
}