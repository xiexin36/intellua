using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LuaEditor
{
    class Type
    {
        public Type(string name) {
            Name = name;
            m_members = new Dictionary<string, Variable>();
            m_methods = new Dictionary<string, Function>();
        }
        private string m_name;
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private Type m_base;
        public LuaEditor.Type Base
        {
            get { return m_base; }
            set { m_base = value; }
        }
        private Dictionary<string, Variable> m_members;
        public Dictionary<string, Variable> Members
        {
            get { return m_members; }
            set { m_members = value; }
        }
        public void addMember(Variable var) {
            var.Class = this;
            m_members[var.Name] = var;
        }
        private Dictionary<string, Function> m_methods;
        public Dictionary<string, Function> Methods
        {
            get { return m_methods; }
            set { m_methods = value; }
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
        public List<IAutoCompleteItem> getList() {
            List<IAutoCompleteItem> rst;
            if (Base != null)
            {
                rst = Base.getList();
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
                rst.Add(key);
            }
            foreach (Function key in m_methods.Values) {
                rst.Add(key);
            }
            rst.Sort();
            return rst;
        }
    }

    

    class TypeManager {
        public TypeManager() { 
            m_types = new Dictionary<string, Type>();
        }
        private Dictionary<string, Type> m_types;
        public Dictionary<string, Type> Types
        {
            get { return m_types; }
            set { m_types = value; }
        }

        public void add(Type t){
            m_types[t.Name] = t;
        }

        public Type get(string name) {
            if(m_types.ContainsKey(name)) return m_types[name];
            return null;
        }
    }
}
