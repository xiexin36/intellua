using System;

namespace Intellua
{
    internal class DeclString
    {
        private string m_data;

        private int m_declStart;

        private int m_longCommentLevel;

        private int m_pos;

        private string m_result = "";

        private State m_state;

        public DeclString(string str)
        {
            m_data = str;
            m_state = State.SearchCommentStart;
            m_pos = 0;
            parse();
        }

        private enum State
        {
            SearchCommentStart,
            SearchCommentEnd
        };
        public String Result
        {
            get
            {
                return m_result;
            }
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

        private void parse()
        {
            bool running = true;
            while (running)
            {
                switch (m_state)
                {
                    case State.SearchCommentStart:
                        {
                            if (m_pos >= m_data.Length)
                            {
                                running = false;
                                continue;
                            }

                            if (!match("--"))
                            {
                                m_pos++;
                                continue;
                            }
                            m_pos += 2;
                            int commentStart = m_pos;
                            m_longCommentLevel = -1; //single

                            //mach long bracket [=====[
                            if (match("["))
                            {
                                m_pos++;
                                m_longCommentLevel = 0;
                                while (match("="))
                                {
                                    m_longCommentLevel++;
                                    m_pos++;
                                }
                                if (match("["))
                                {
                                    m_pos++;
                                }
                                else
                                {
                                    m_pos = commentStart;
                                }
                            }

                            if (match("!"))
                            {
                                m_pos++;
                                m_declStart = m_pos;
                            }
                            else
                            {
                                m_declStart = -1;
                            }

                            m_state = State.SearchCommentEnd;

                            break;
                        }

                    case State.SearchCommentEnd:
                        {
                            int commentEnd = m_data.Length;
                            if (m_pos < m_data.Length)
                            {
                                if (m_longCommentLevel == -1)
                                {
                                    if (!match("\n"))
                                    {
                                        m_pos++;
                                        continue;
                                    }
                                    else
                                    {
                                        m_pos++;
                                        commentEnd = m_pos;
                                    }
                                }
                                else
                                {
                                    bool closeLongBracketFound = false;
                                    int p = m_pos;
                                    if (match("]"))
                                    {
                                        commentEnd = m_pos;
                                        m_pos++;
                                        bool closeLongBracketMatch = true;
                                        for (int i = 0; i < m_longCommentLevel; i++)
                                        {
                                            if (!match("="))
                                            {
                                                closeLongBracketMatch = false;
                                                break;
                                            }
                                            else
                                            {
                                                m_pos++;
                                            }
                                        }
                                        if (closeLongBracketMatch)
                                        {
                                            if (match("]"))
                                            {
                                                closeLongBracketFound = true;
                                                m_pos++;
                                            }
                                        }
                                    }
                                    if (!closeLongBracketFound)
                                    {
                                        m_pos = p;
                                        m_pos++;
                                        continue;
                                    }
                                }
                            }

                            if (m_declStart != -1)
                            {
                                string str = m_data.Substring(m_declStart, commentEnd - m_declStart);
                                m_result += str;
                            }
                            m_state = State.SearchCommentStart;
                            break;
                        }
                }
            }
        }
    }
}