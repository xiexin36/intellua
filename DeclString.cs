using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    class DeclString
    {
        enum State
        {
            SearchCommentStart,
            SearchCommentEnd
        };

        State state;
        int pos;
        string data;
        string result = "";
        
        int longCommentLevel;
        int declStart;
        public String Result {
            get {
                return result;
            }
        }

        public DeclString(string str) {
            data = str;
            state = State.SearchCommentStart;
            pos = 0;
            parse();
        }

        bool match(string str) {
            if (pos + str.Length > data.Length) return false;
            for (int i = 0; i < str.Length; i++) {
                if (data[pos + i] != str[i]) return false;
            }
            return true;
        }

        void parse() {
            bool running = true;
            while (running)
            {
                switch (state) {
                    case State.SearchCommentStart: {
                        if (pos >= data.Length) {
                            running = false;
                            continue;
                        }

                        if (!match("--")) {
                            pos++;
                            continue;
                        }
                        pos += 2;
                        int commentStart = pos;
                        longCommentLevel = -1; //single

                        //mach long bracket [=====[
                        if (match("[")) {
                            pos++;
                            longCommentLevel = 0;
                            while (match("=")) {
                                longCommentLevel++;
                                pos++;
                            }
                            if (match("["))
                            {
                                pos++;
                            }
                            else {
                                pos = commentStart;
                            }
                        }

                        if (match("!"))
                        {
                            pos++;
                            declStart = pos;
                        }
                        else {
                            declStart = -1;
                        }

                        state = State.SearchCommentEnd;

                        break;
                    }

                    case State.SearchCommentEnd:{
                        int commentEnd = data.Length;
                        if (pos < data.Length) {
                            if (longCommentLevel == -1)
                            {
                                if (!match("\n"))
                                {
                                    pos++;
                                    continue;
                                }else{
                                    pos++;
                                    commentEnd = pos;
                                }
                            }
                            else { 
                                bool closeLongBracketFound = false;
                                int p = pos;
                                if (match("]")) { 
                                    commentEnd = pos;
                                    pos++;
                                    bool closeLongBracketMatch = true;
                                    for (int i = 0; i < longCommentLevel; i++) {
                                        if (!match("="))
                                        {
                                            closeLongBracketMatch = false;
                                            break;
                                        }
                                        else {
                                            pos++;
                                        }

                                    }
                                    if (closeLongBracketMatch) { 
                                        if(match("]")){
                                            closeLongBracketFound = true;
                                            pos++;
                                        }
                                    }
                                }
                                if (!closeLongBracketFound) {
                                    pos = p;
                                    pos++;
                                    continue;
                                }
                            }
                        }

                        if (declStart != -1) {
                            string str = data.Substring(declStart, commentEnd - declStart);
                            result += str;
                        }
                        state = State.SearchCommentStart;
                        break;
                    }
                
                }


            }
        }
    }
}
