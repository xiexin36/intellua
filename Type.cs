using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    class Type
    {
		#region Fields (7) 

        private Type m_base;
        private string m_displayName;
        private bool m_hideDeclare = false;
        private Dictionary<string, Variable> m_members;
        private Dictionary<string, Function> m_methods;
        private string m_name;
        private Type m_outerClass;

		#endregion Fields 

		#region Constructors (1) 

        public Type(string name) {
            DisplayName = InternalName = name;
            m_members = new Dictionary<string, Variable>();
            m_methods = new Dictionary<string, Function>();
        }

		#endregion Constructors 

		#region Properties (7) 

        public Type Base
        {
            get { return m_base; }
            set { m_base = value; }
        }

        public string DisplayName
        {
            get { return m_displayName; }
            set { m_displayName = value; }
        }

        public bool HideDeclare
        {
            get { return m_hideDeclare; }
            set { m_hideDeclare = value; }
        }

        public string InternalName
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public Dictionary<string, Variable> Members
        {
            get { return m_members; }
            set { m_members = value; }
        }

        public Dictionary<string, Function> Methods
        {
            get { return m_methods; }
            set { m_methods = value; }
        }

        public Type OuterClass
        {
            get { return m_outerClass; }
            set { m_outerClass = value; }
        }

		#endregion Properties 

		#region Methods (3) 

		// Public Methods (3) 

        public void addMember(Variable var) {
            var.Class = this;
            m_members[var.Name] = var;
        }

        public void addMethod(Function method) {
            if (!m_methods.ContainsKey(method.Name))
            {
                method.Class = this;
                m_methods[method.Name] = method;
            }
            else {
                m_methods[method.Name].Param.Add(method.Param[0]);
                m_methods[method.Name].Desc.Add(method.Desc[0]);
            }
        }

        public List<IAutoCompleteItem> getList(bool Static) {
            List<IAutoCompleteItem> rst;
            if (Base != null)
            {
                rst = Base.getList(Static);
                AutoCompleteItemComparer comparer = new AutoCompleteItemComparer();
                List<IAutoCompleteItem> rm = new List<IAutoCompleteItem>();
                foreach (IAutoCompleteItem item in rst) {
                    if (m_methods.Values.Contains(item,comparer)) {
                        rm.Add(item);
                    }
                }
                foreach (IAutoCompleteItem item in rm)
                {
                    rst.Remove(item);
                }
            }
            else
            {
                rst = new List<IAutoCompleteItem>();
            }
            foreach (Variable key in m_members.Values) {
                if(key.IsStatic == Static)
                    rst.Add(key);
            }
            foreach (Function key in m_methods.Values) {
                if (key.Static == Static)
                    rst.Add(key);
            }
            rst.Sort();
            return rst;
        }

		#endregion Methods 
    }

    

    class TypeManager {
		#region Fields (2) 

        private Type m_nullType;
        private Dictionary<string, Type> m_types;

		#endregion Fields 

		#region Constructors (1) 

        public TypeManager() { 
            m_types = new Dictionary<string, Type>();
            m_nullType = new Type("(UnknownType)");
            m_nullType.DisplayName = "";
            m_nullType.HideDeclare = true;
        }

		#endregion Constructors 

		#region Properties (1) 

        public Dictionary<string, Type> Types
        {
            get { return m_types; }
            set { m_types = value; }
        }

		#endregion Properties 

		#region Methods (2) 

		// Public Methods (2) 

        public void add(Type t){
            m_types[t.InternalName] = t;
        }

        public Type get(string name) {
            if (name == null) return m_nullType;
            if(m_types.ContainsKey(name)) return m_types[name];
            return m_nullType;
        }

		#endregion Methods 
    }
}
