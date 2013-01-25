using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaEditor
{
 
        class Function : IAutoCompleteItem
        {


            public Function(string name)
            {
                m_name = name;
            }
            private string m_name;
            public string Name
            {
                get { return m_name; }
                set { m_name = value; }
            }
            private Type m_returnType;
            public LuaEditor.Type ReturnType
            {
                get { return m_returnType; }
                set { m_returnType = value; }
            }

            private Type m_class;
            public LuaEditor.Type Class
            {
                get { return m_class; }
                set { m_class = value; }
            }
            private List<string> m_param = new List<string>();
            public List<string> Param
            {
                get { return m_param; }
                set { m_param = value; }
            }
            private int m_currentOverloadIndex = 0;
            public int CurrentOverloadIndex
            {
                get { return m_currentOverloadIndex; }
                set { m_currentOverloadIndex = value; }
            }
            private List<string> m_desc = new List<string>();
            public List<string> Desc
            {
                get { return m_desc; }
                set { m_desc = value; }
            }

            private bool m_static = false;
            public bool Static 
            {
                get { return m_static; }
                set { m_static = value; }
            }
            public override string getName()
            {
                return Name;
            }
            public override string getACString()
            {
                return Name + "?" + (Static ? "2" : "1");
            }
            public override string getToolTipString()
            {
                string rst = ReturnType.Name + " " + (Class == null ? "" : Class.Name + ":") + Name + Param[0];
                if (Param.Count > 1) {
                    rst += " (" + Param.Count + " overloads)";
                }
                if(Desc[CurrentOverloadIndex].Length >0)
                    rst += "\n\n" + Desc[CurrentOverloadIndex];
                return rst;
            }
        }
    
}
