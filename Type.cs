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
            method.Class = this;
            m_methods[method.Name] = method;
        }
        public List<IAutoCompleteItem> getList() {
            List<IAutoCompleteItem> rst = new List<IAutoCompleteItem>();

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
