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
            private string m_param = "()";
            public string Param
            {
                get { return m_param; }
                set { m_param = value; }
            }

            private string m_desc;
            public string Desc
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
                return ReturnType.Name + " " + (Class == null ? "" : Class.Name + ":") + Name + Param + "\n\n" + Desc;
            }
        }
    
}
