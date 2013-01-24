using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaEditor
{
    class FunctionCall
    {
        private Function m_func;
        private string m_calltipString;
        public string CalltipString
        {
            get { return m_calltipString; }
            private set { m_calltipString = value; }
        }


        private int m_paramIndex;
        public int ParamIndex
        {
            get { return m_paramIndex; }
            set { m_paramIndex = value; }
        }

        private int m_highLightStart;
        public int HighLightStart
        {
            get { return m_highLightStart; }
            private set { m_highLightStart = value; }
        }
        private int m_highLightEnd;
        public int HighLightEnd
        {
            get { return m_highLightEnd; }
            private set { m_highLightEnd = value; }
        }
        private FunctionCall() { 
        
        }

        public static FunctionCall Parse(ScintillaNET.Scintilla scintilla,VariableManager variables, int pos) {
            int paramIndex = 0;
            string str = scintilla.Text;
            bool running = true;
            while (pos > 0) {
                if (char.IsWhiteSpace(str[pos]) || !Parser.isCode(scintilla, pos))
                {
                    pos--;
                    continue;
                }
                if (str[pos] == ',')
                {
                    paramIndex++;
                    pos--;
                    break;
                }
                if (str[pos] == '(')
                {
                    running = false;
                    break;
                }
                break;
            }

            Chain chain = Chain.ParseBackward(scintilla,pos);
            
            while (chain.Elements.Count != 0 && running) {
                pos = chain.StartPos;

                while (pos > 0) {
                    if (char.IsWhiteSpace(str[pos]) || !Parser.isCode(scintilla,pos)) {
                        pos--;
                        continue;
                    }
                    if (str[pos] == ',') {
                        paramIndex++;
                        pos--;
                        break;
                    }
                    if (str[pos] == '(') {
                        running = false;
                        break;
                    }
                    return null;

                }
                if (pos <= 0) return null;
                chain = Chain.ParseBackward(scintilla, pos);
            }

            while (pos > 0) {
                if (char.IsWhiteSpace(str[pos]) || !Parser.isCode(scintilla, pos))
                {
                    pos--;
                    continue;
                }

                if (str[pos] == '(') {
                    chain = Chain.ParseBackward(scintilla, pos - 1);
                    chain.getType(variables);
                    
                    if (chain.LastFunction == null) return null;
                    FunctionCall fc = new FunctionCall();
                    Function func = fc.m_func = chain.LastFunction;
                    fc.ParamIndex = paramIndex;
                    fc.CalltipString = func.ReturnType.Name + " " + (func.Class == null ? "" : func.Class.Name + ":")
                                       + func.Name;
                    int offset = fc.CalltipString.Length;
                    fc.CalltipString += func.Param;

                    str = func.Param;
                    pos = 1;
                    while (paramIndex > 0 && pos < str.Length) {
                        if (str[pos] == ',') paramIndex--;
                        pos++;
                    }

                    if (pos != str.Length) {
                        fc.HighLightStart = pos + offset;

                        while (pos < str.Length -1&& str[pos] != ',') pos++;
                        fc.HighLightEnd = pos + offset;
                    }


                    return fc;

                }
            }

            
            

            return null;
        }
    }
}
