using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
 
        class Function : IAutoCompleteItem
        {
		#region Fields (7) 

            private Type m_class;
            private int m_currentOverloadIndex = 0;
            private List<string> m_desc = new List<string>();
            private string m_name;
            private List<string> m_param = new List<string>();
            private Type m_returnType;
            private bool m_static = false;

		#endregion Fields 

		#region Constructors (1) 

            public Function(string name)
            {
                m_name = name;
            }

		#endregion Constructors 

		#region Properties (7) 

            public Type Class
            {
                get { return m_class; }
                set { m_class = value; }
            }

            public int CurrentOverloadIndex
            {
                get { return m_currentOverloadIndex; }
                set { m_currentOverloadIndex = value; }
            }

            public List<string> Desc
            {
                get { return m_desc; }
                set { m_desc = value; }
            }

            public string Name
            {
                get { return m_name; }
                set { m_name = value; }
            }

            public List<string> Param
            {
                get { return m_param; }
                set { m_param = value; }
            }

            public Type ReturnType
            {
                get { return m_returnType; }
                set { m_returnType = value; }
            }

            public bool Static 
            {
                get { return m_static; }
                set { m_static = value; }
            }

		#endregion Properties 

		#region Methods (4) 

		// Public Methods (4) 

            public override string getACString()
            {
                return Name + "?" + (Static ? "2" : "1");
            }

            public override string getName()
            {
                return Name;
            }

            public override string getToolTipString()
            {
                string rst = getTypeName()+ Name + Param[0];
                if (Param.Count > 1) {
                    rst += " (" + Param.Count + " overloads)";
                }
                if(Desc[CurrentOverloadIndex].Length >0)
                    rst += "\n\n" + Desc[CurrentOverloadIndex];
                return rst;
            }

            public string getTypeName() {
                string rst = (ReturnType != null ? ReturnType.DisplayName + " " : "") + (Class == null ? "" : Class.DisplayName + (Static ? "." : ":"));
                return rst;
            }

		#endregion Methods 
        }
    
}
