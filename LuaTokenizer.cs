using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    enum LuaTokenType { 
        KW_and,
        KW_break,
        KW_do,
        KW_else,
        KW_elseif,
        KW_end,
        KW_false,
        KW_for,
        KW_function,
        KW_if,
        KW_in,
        KW_local,
        KW_nil,
        KW_not,
        KW_or,
        KW_repeat,
        KW_return,
        KW_then,
        KW_true,
        KW_until,
        KW_while,

        OP_add,
        OP_sub,
        OP_mul,
        OP_div,
        OP_mod,
        OP_pow,
        OP_hash,
        OP_eq,
        OP_ne,
        OP_le,
        OP_ge,
        OP_lt,
        OP_gt,
        OP_assign,
        OP_lparen,
        OP_rparen,
        OP_lbrace,
        OP_rbrace,
        OP_lbracket,
        OP_rbracket,
        OP_semicolon,
        OP_colon,
        OP_comma,
        OP_ellipsis,
        OP_doubleDot,
        OP_dot,
        StringLiteral,
        Number,
        Identifier,
        EOF,

    }
    internal class LuaToken {
        public LuaTokenType Type;
        public Byte[] data;
        public int pos;
        public int line;
        public LuaToken(LuaTokenType t, Byte[] d, int p,int l) {
            Type = t;
            data = d;
            pos = p;
            line = l;
        }
    }


    internal class LuaTokenizer
    {
        private static Byte[] SubArray(Byte[] data, int index, int length)
        {
            Byte[] result = new Byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        private static Byte[] SubArray(Byte[] data, int index)
        {
            int length = data.Length - index;
            Byte[] result = new Byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        private static bool equal(string s, Byte[] b) {
            if (s.Length != b.Length) return false;
            for (int i = 0; i < s.Length; i++) {
                if (s[i] != b[i]) {
                    return false;
                }
            }
            return true;
        }

        private static string EOF = "\0";

        private static TupleList<string, LuaTokenType> Keywords = new TupleList<string, LuaTokenType>
        {
            {"and",LuaTokenType.KW_and},
            {"break",LuaTokenType.KW_break},
            {"do",LuaTokenType.KW_do},
            {"else",LuaTokenType.KW_else},
            {"elseif",LuaTokenType.KW_elseif},
            {"end",LuaTokenType.KW_end},
            {"false",LuaTokenType.KW_false},
            {"for",LuaTokenType.KW_for},
            {"function",LuaTokenType.KW_function},
            {"if",LuaTokenType.KW_if},
            {"in",LuaTokenType.KW_in},
            {"local",LuaTokenType.KW_local},
            {"nil",LuaTokenType.KW_nil},
            {"not",LuaTokenType.KW_not},
            {"or",LuaTokenType.KW_or},
            {"repeat",LuaTokenType.KW_repeat},
            {"return",LuaTokenType.KW_return},
            {"then",LuaTokenType.KW_then},
            {"true",LuaTokenType.KW_true},
            {"until",LuaTokenType.KW_until},
            {"while",LuaTokenType.KW_while},
        };
        private static TupleList<string, LuaTokenType> Ops = new TupleList<string, LuaTokenType>
        {
            {"+",LuaTokenType.OP_add},
            {"-",LuaTokenType.OP_sub},
            {"*",LuaTokenType.OP_mul},
            {"/",LuaTokenType.OP_div},
            {"%",LuaTokenType.OP_mod},
            {"^",LuaTokenType.OP_pow},
            {"#",LuaTokenType.OP_hash},
            {"==",LuaTokenType.OP_eq},
            {"~=",LuaTokenType.OP_ne},
            {"<=",LuaTokenType.OP_le},
            {">=",LuaTokenType.OP_ge},
            {"<",LuaTokenType.OP_lt},
            {">",LuaTokenType.OP_gt},
            {"=",LuaTokenType.OP_assign},
            {"(",LuaTokenType.OP_lparen},
            {")",LuaTokenType.OP_rparen},
            {"{",LuaTokenType.OP_lbrace},
            {"}",LuaTokenType.OP_rbrace},
            {"[",LuaTokenType.OP_lbracket},
            {"]",LuaTokenType.OP_rbracket},
            {";",LuaTokenType.OP_semicolon},
            {":",LuaTokenType.OP_colon},
            {",",LuaTokenType.OP_comma},
            {"...",LuaTokenType.OP_ellipsis},
            {"..",LuaTokenType.OP_doubleDot},
            {".",LuaTokenType.OP_dot},
        };

        private static string IdentifierChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_1234567890";
        private static string IdentifierStart = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
        private static string PPNumber = "01234567890.+-xXabcdefABCDEF";
        private Byte[] m_data;
        private int m_pos;
        private List<int> m_lines = new List<int>();

        private State m_state;

        public LuaTokenizer(Byte[] str, int pos)
        {

            m_data = new Byte[str.Length + 1];
            for (int i = 0; i < str.Length; i++) {
                m_data[i] = str[i];
            }
            m_data[str.Length] = 0;

            m_pos = pos;

            m_state = State.Start;

            for (int i = 0; i < str.Length; i++) {
                if (str[i] == '\n') m_lines.Add(i);
            }
        }

        int getLine(int p) {
            for (int i = 0; i < m_lines.Count; i++) {
                if (m_lines[i] > p) {
                    return i + 1;
                }
            }
            return m_lines.Count;
        }

        private enum State
        {
            Start,
            Identifier,
            Comment,
            String,
            LongBracketString,
            Number,
        };

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
            char c = Convert.ToChar(m_data[m_pos]);
            return (str.Contains(c));
        }
        private char peek()
        {
            return Convert.ToChar(m_data[m_pos]);
        }

        private int getLongBracket() { 
            int p = m_pos;
            int longBracketLength = 0;
            if (match("[")) {
                m_pos++;
                while (match("=")) {
                    longBracketLength++;
                    m_pos++;
                }
                if(match("[")){
                    m_pos++;
                    if (match("\n")) {
                        m_pos++;
                    }
                    else if (match("\r\n")) {
                        m_pos += 2;
                    }
                    return longBracketLength;
                }
            }
            m_pos = p;
            return -1;
        }

        private int matchLongBracket(int l){
            int p = m_pos;
            if(match("]"))
            {
                m_pos++;
                for (int i = 0; i < l; i++)
                {
                    if (!match("="))
                    {
                        m_pos = p;
                        return -1;
                    }
                    m_pos++;
                }
                if (match("]"))
                {
                    m_pos++;
                    int rst = m_pos - p;
                    m_pos = p;
                    return rst;
                }
            }
            m_pos = p;
            return -1;
        }

        public LuaToken getToken() {
            int sequenceStart = -1;
            int longBracketLength = -1;
            bool isComment = false;
            bool isHex = false;
            bool isExp = false;
            while (true)
            {
                switch (m_state)
                {
                    case State.Start:
                        if (matchChar(EOF)) {
                            return new LuaToken(LuaTokenType.EOF, new Byte[1], m_data.Length,getLine(m_data.Length));
                        }
                        if (match("\"")) {
                            m_pos++;
                            sequenceStart = m_pos;
                            m_state = State.String;
                            continue;
                        }
                        if(match("--")){
                            m_pos+=2;
                            if (match("@")) {
                                m_pos++;
                                continue;
                            }
                            m_state=State.Comment;
                            continue;
                        }
                        if (Char.IsDigit(peek())) {
                            sequenceStart = m_pos;
                            m_state = State.Number;
                            isHex = false;
                            isExp = false;
                            continue;
                        }
                        if(Char.IsWhiteSpace(peek())){
                            m_pos++;
                            continue;
                        }

                        longBracketLength = getLongBracket();
                        if (longBracketLength != -1) {
                            isComment = false;
                            m_state = State.LongBracketString;
                            sequenceStart = m_pos;
                            continue;
                        }

                        foreach (Tuple<string, LuaTokenType> op in Ops)
                        {
                            if (match(op.Item1))
                            {
                                LuaToken rst = new LuaToken(op.Item2, System.Text.Encoding.UTF8.GetBytes(op.Item1), m_pos, getLine(m_pos));
                                m_pos += op.Item1.Length;
                                return rst;
                            }
                        }
                        if (matchChar(IdentifierStart))
                        {
                            m_state = State.Identifier;
                            sequenceStart = m_pos;
                            continue;
                        }
                        m_pos++;
                        continue;

                    case State.Identifier:
                        if (matchChar(IdentifierChars)) {
                            m_pos++;
                            continue;
                        }

                        Byte[] str = SubArray(m_data,sequenceStart, m_pos - sequenceStart);
                        foreach (Tuple<string, LuaTokenType> kw in Keywords)
                        {
                            if (equal(kw.Item1,str))
                            {
                                LuaToken rst = new LuaToken(kw.Item2, System.Text.Encoding.UTF8.GetBytes(kw.Item1), sequenceStart, getLine(sequenceStart));
                                m_state = State.Start;
                                return rst;
                            }
                        }
                        m_state = State.Start;
                        return new LuaToken(LuaTokenType.Identifier, str, sequenceStart,getLine(sequenceStart));

                    case State.Comment:
                        {
                            longBracketLength = getLongBracket();
                            if (longBracketLength != -1)
                            {
                                isComment = true;
                                m_state = State.LongBracketString;
                                sequenceStart = m_pos;
                                continue;
                            }
                            while (!match("\n") && !match(EOF)) {
                                m_pos++;
                            }
                            if (match("\n")) m_pos++;
                            m_state = State.Start;
                            continue;
                        }

                    case State.LongBracketString:
                        {
                            int l = -1;
                            while (!match(EOF) ) {
                                 l = matchLongBracket(longBracketLength);
                                 if (l != -1)
                                 {
                                     break;
                                 }
                                 else
                                 {
                                     m_pos++;
                                 }
                            }
                            Byte[] rst;
                            if (l == -1)
                            {
                                //EOF
                                rst = SubArray(m_data,sequenceStart);
                            }
                            else {
                                rst = SubArray(m_data,sequenceStart, m_pos - sequenceStart);
                                m_pos += l;
                            }
                            m_state = State.Start;
                            if (isComment) {
                                continue;
                            }
                            return new LuaToken(LuaTokenType.StringLiteral, rst, sequenceStart,getLine(sequenceStart));
                        }
                    case State.String:
                        while (!match(EOF) && !match("\"")) {
                            if (match("\\"))
                            {
                                m_pos++;
                                if (match("\""))
                                {
                                    m_pos++;
                                }
                            }
                            else {
                                m_pos++;
                            }
                        }
                        m_state = State.Start;
                        if (match(EOF))
                        {
                            return new LuaToken(LuaTokenType.StringLiteral, SubArray(m_data,sequenceStart), sequenceStart,getLine(sequenceStart));
                        }
                        else {
                            m_pos++;
                            return new LuaToken(LuaTokenType.StringLiteral, SubArray(m_data, sequenceStart, m_pos - sequenceStart - 1), sequenceStart, getLine(sequenceStart));
                        }
                    case State.Number:
                        if (match("0x") || match("0X"))
                        {
                            m_pos += 2;
                            while (matchChar("0123456789abcdefABCDEF"))
                            {
                                m_pos++;
                            }
                        }
                        else {
                            while (Char.IsDigit(peek())) {
                                m_pos++;
                            }
                            if (match(".")) {
                                m_pos++;
                                while (Char.IsDigit(peek()))
                                {
                                    m_pos++;
                                }
                            }

                            if (match("e") || match("E")) {
                                m_pos++;
                                if (match("+") || match("-")) {
                                    m_pos++;
                                }
                                while (Char.IsDigit(peek()))
                                {
                                    m_pos++;
                                }
                            }
                        }
                        m_state = State.Start;
                        return new LuaToken(LuaTokenType.Number, SubArray(m_data, sequenceStart, m_pos - sequenceStart), sequenceStart, getLine(sequenceStart));
                    default:
                        throw new Exception("unimplemented state!");
                }
            }



            
        }
    }
}
