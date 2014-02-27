using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    class FileParser
    {
        public AutoCompleteData result;
        IntelluaSource m_source;
        public FileParser(IntelluaSource source) {
            m_source = source;
            result = new AutoCompleteData();
            result.setParent(source.m_intellua.AutoCompleteData.getParent());
            result.Variables.scope = source.m_intellua.parseScope(0, source.m_intellua.Lines.Count - 1);
        }
        public void doWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {

            int pos = 0;
            Byte[] str = m_source.RawText;

           
            for (; pos < str.Length; pos++)
            {
                char c = Convert.ToChar(str[pos]);

                //search for assignment operator

                if (!Parser.isCode(m_source, pos))
                {
                    continue;
                }

                if (c != '=') continue;
                if (pos > 0)
                {
                    if (Convert.ToChar(str[pos - 1]) == '=') continue;
                }
                if (pos < str.Length - 1)
                {
                    if (Convert.ToChar(str[pos + 1]) == '=') continue;
                }
                MemberChain v = MemberChain.ParseBackward(m_source, pos - 1);
                if (v.Elements.Count > 1 || v.Elements.Count == 0) continue;

                string varName = v.getLastElement();
                Variable var = result.Variables.getVariable(varName);
                //                if (var != null) continue;

                MemberChain elem = MemberChain.ParseFoward(m_source, pos + 1);
                if (elem == null) continue;
                Type t = elem.getType(result);
                if (t == null) continue;

                //System.Diagnostics.Debug.Print(varName + " added");

                var = new Variable(varName);
                var.IsStatic = false;
                var.Type = t;
                var.StartPos = v.StartPos;
                var.EndPos = elem.EndPos;
                result.Variables.add(var);
            }
            e.Result = result;
        }
        
    }
}
