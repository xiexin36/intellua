using System.Linq;

namespace Intellua
{
    class FunctionCall
    {
		#region Fields (5) 

        private string m_calltipString;
        private Function m_func;
        private int m_highLightEnd;
        private int m_highLightStart;
        private int m_paramIndex;

		#endregion Fields 

		#region Constructors (1) 

        private FunctionCall() { 
        
        }

		#endregion Constructors 

		#region Properties (5) 

        public string CalltipString
        {
            get { return m_calltipString; }
            private set { m_calltipString = value; }
        }

        public Function Func
        {
            get { return m_func; }
            private set { m_func = value; }
        }

        public int HighLightEnd
        {
            get { return m_highLightEnd; }
            private set { m_highLightEnd = value; }
        }

        public int HighLightStart
        {
            get { return m_highLightStart; }
            private set { m_highLightStart = value; }
        }

        public int ParamIndex
        {
            get { return m_paramIndex; }
            set { m_paramIndex = value; }
        }

		#endregion Properties 

		#region Methods (2) 

		// Public Methods (2) 

        public static FunctionCall Parse(Intellua scintilla,AutoCompleteData data, int pos) {
            VariableManager variables = data.Variables;
            const string luaOperators = "+-*/^%<>=~";
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

            MemberChain chain = MemberChain.ParseBackward(scintilla,pos);
            
            while (chain.Elements.Count != 0 && running) {
                pos = scintilla.getDecodedPos(chain.StartPos);

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
                    if(luaOperators.Contains(str[pos])){
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
                chain = MemberChain.ParseBackward(scintilla, pos);
            }

            while (pos > 0) {
                if (char.IsWhiteSpace(str[pos]) || !Parser.isCode(scintilla, pos))
                {
                    pos--;
                    continue;
                }

                if (str[pos] == '(') {
                    chain = MemberChain.ParseBackward(scintilla, pos - 1);
                    chain.getType(data,true);
                    
                    if (chain.LastFunction == null) return null;
                    FunctionCall fc = new FunctionCall();
                    fc.m_func = chain.LastFunction;
                    fc.ParamIndex = paramIndex;

                   fc.update();
                    return fc;

                }
                break;
            }

            
            

            return null;
        }

        public void update() {
            Function func = Func;
            CalltipString = "";
            if (func.Param.Count > 1) {
                CalltipString += "[" + (func.CurrentOverloadIndex+1) + " of " + func.Param.Count + "]\n";
            }
            CalltipString += func.getTypeName() + func.Name;
            int offset = CalltipString.Length;
            CalltipString += func.Param[func.CurrentOverloadIndex];
            if(func.Desc[func.CurrentOverloadIndex].Length > 0)
                CalltipString += "\n\n" + func.Desc[func.CurrentOverloadIndex];

            string str = func.Param[func.CurrentOverloadIndex];
            int pos = 1;
            int paramIndex = ParamIndex;
            while (paramIndex > 0 && pos < str.Length)
            {
                if (str[pos] == ',') paramIndex--;
                pos++;
            }

            if (pos != str.Length)
            {
                HighLightStart = pos + offset;

                while (pos < str.Length - 1 && str[pos] != ',') pos++;
                HighLightEnd = pos + offset;
            }

        }

		#endregion Methods 
    }
}
