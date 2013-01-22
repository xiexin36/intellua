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
            m_members = new Dictionary<string, Type>();
            m_methods = new Dictionary<string, Function>();
        }
        private string m_name;
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }
        private Dictionary<string, Type> m_members;
        public Dictionary<string, Type> Members
        {
            get { return m_members; }
            set { m_members = value; }
        }
        public void addMember(string name, Type type) {
            m_members[name] = type;
        }
        private Dictionary<string, Function> m_methods;
        public Dictionary<string, Function> Methods
        {
            get { return m_methods; }
            set { m_methods = value; }
        }
        public void addMethod(Function method) {
            m_methods[method.Name] = method;
        }
        public List<string> getList() { 
            List<string> rst = new List<string>();

            foreach (string key in m_members.Keys) {
                rst.Add(key);
            }
            foreach (Function key in m_methods.Values) {
                rst.Add(key.ToString());
            }
            rst.Sort();
            return rst;
        }
    }

    class Function {
        public Function(string name) {
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
        private string m_param;
        public string Param
        {
            get { return m_param; }
            set { m_param = value; }
        }

        public override string ToString() {
            return Name + "()";
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
            return m_types[name];
        }
    }
}
