using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    class DeclToken {
        public string data;


        public DeclTokenType type;

        public DeclToken(string _data, DeclTokenType _type)
        {
            data = _data;
            type = _type;
        }
    }

    class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }

    class DeclTokenizer
    {
        List<DeclToken> result = new List<DeclToken>();
        public List<DeclToken> Result{
            get{
                return result;
            }
        }
        string data;
        public DeclTokenizer(string str) {
            data = str + EOF;
            parse();    
        }
        static string IdentifierHeadChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_\"";
        static string IdentifierChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_\"1234567890";
        static string PPNumber = "01234567890.+-xXabcdefABCDEF";
        static TupleList<string, DeclTokenType> Kws = new TupleList<string, DeclTokenType>
        {
            {"static",DeclTokenType.KW_Static},
            {"class",DeclTokenType.KW_Class},
        };
        static TupleList<string, DeclTokenType> Ops = new TupleList<string, DeclTokenType>
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
        
        enum State
        {
            Start,
            Identifier,
            LineComment,
            BlockComment,
            Number,
        };
        State state;
        int pos;
        bool matchChar(string str){
            return (str.Contains(data[pos]));
        }
        bool match(string str) {
            if (pos + str.Length > data.Length) return false;
            for (int i = 0; i < str.Length; i++) {
                if (data[pos + i] != str[i]) return false;
            }
            return true;
        }
        static string EOF = "\0";

        char peek() {
            return data[pos];
        }

        void addComment(string str) { 
            if(str.Length == 0 || str[0] != '!') return;
            str = str.Substring(1);
            str = str.Trim();
            str.Replace("\r", "");
            str.Replace("\n", "");
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ");
            result.Add(new DeclToken(str, DeclTokenType.Comment));
        }

        void parse()
        {
            state = State.Start;
            pos = 0;
            int sequenceStart = -1;
            bool running = true;
            
            int angleBracketLevel = 0;

            while (running) {
                switch (state) { 
                    case State.Start:
                        if(matchChar(EOF)){
                            running = false;
                            continue;
                        }

                        if(matchChar(IdentifierHeadChars)){
                            state = State.Identifier;
                            sequenceStart = pos;
                            continue;
                        }
                        if (match("//")) {
                            pos += 2;
                            sequenceStart = pos;
                            state = State.LineComment;
                            continue;
                        }
                        if (match("/*"))
                        {
                            pos += 2;
                            sequenceStart = pos;
                            state = State.BlockComment;
                            continue;
                        }
                        if (Char.IsDigit(peek()))
                        {
                            sequenceStart = pos;
                            state = State.Number;
                            continue;
                        }
                        

                        if (Char.IsWhiteSpace(peek())) {
                            pos++;
                            continue;
                        }

                        foreach (Tuple<string, DeclTokenType> op in Ops) {
                            if (match(op.Item1)) {
                                pos += op.Item1.Length -1;
                                result.Add(new DeclToken(op.Item1, op.Item2));
                                break;
                            }
                        }

                        pos++;
                        continue;


                    case State.Identifier:
                        if (matchChar(IdentifierChars)) {
                            pos++;
                            continue;
                        }
                        if (match("<")) {
                            angleBracketLevel++;
                            pos++;
                            continue;
                        }
                        if (angleBracketLevel > 0) {
                            if (match(">")) {
                                angleBracketLevel--;
                            }
                            pos++;
                            continue;
                        }

                        string str = data.Substring(sequenceStart, pos - sequenceStart);
                        bool found = false;
                        foreach (Tuple<string, DeclTokenType> kw in Kws)
                        {
                            if (kw.Item1 == str) {
                                result.Add(new DeclToken(kw.Item1, kw.Item2));
                                state = State.Start;
                                found = true;
                                break;
                            }
                        }
                        if (found) {
                            break;
                        }

                        result.Add(new DeclToken(str,DeclTokenType.Identifier));
                        state = State.Start;
                        break;

                    case State.LineComment:
                        if (match("\n") || match(EOF))
                        {
                            addComment(data.Substring(sequenceStart, pos - sequenceStart));
                            
                            if (match("\n"))
                            {
                                pos++;
                            }
                            state = State.Start;
                        }
                        else {
                            pos++;
                            continue;
                        }
                        break;

                    case State.BlockComment:
                        if (match("*/") || match(EOF))
                        {
                            addComment(data.Substring(sequenceStart, pos - sequenceStart));
                            if (match("*/"))
                            {
                                pos+=2;
                            }
                            state = State.Start;
                        }
                        else
                        {
                            pos++;
                            continue;
                        }
                        break;

                    case State.Number:
                        if (matchChar(PPNumber)) {
                            pos++;
                            continue;
                        }
                        if(result.Count >0){
                            DeclToken last = result[result.Count-1];
                            if (last.type == DeclTokenType.OP_Minus || last.type == DeclTokenType.OP_Plus)
                            {
                                string sign = last.data;
                                result.RemoveAt(result.Count - 1);
                                result.Add(new DeclToken(sign + data.Substring(sequenceStart, pos - sequenceStart), DeclTokenType.Number));
                                state = State.Start;
                                continue;
                            }
                        }
                        result.Add(new DeclToken(data.Substring(sequenceStart, pos - sequenceStart), DeclTokenType.Number));
                        state = State.Start;
                        break;


                }
            }

            result.Add(new DeclToken("", DeclTokenType.EOF));
        }
    }
}
