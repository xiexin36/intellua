using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Intellua
{
    class Variable : IAutoCompleteItem
    {
        public Variable(string name) {
            Name = name;
            m_isStatic = false;
        }
        private string m_name;
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }
        private Type m_Type;
        public Type Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }
        private Type m_Class;
        public Type Class
        {
            get { return m_Class; }
            set { m_Class = value; }
        }
        private bool m_isStatic;
        public bool IsStatic
        {
            get { return m_isStatic; }
            set { m_isStatic = value; }
        }
        private int m_startPos;
        public int StartPos
        {
            get { return m_startPos; }
            set { m_startPos = value; }
        }
        private int m_endPos;
        public int EndPos
        {
            get { return m_endPos; }
            set { m_endPos = value; }
        }

        private string m_desc;
        public string Desc
        {
            get { return m_desc; }
            set { m_desc = value; }
        }

        public override string getName()
        {
            return Name;
        }
        public override string getACString()
        {
            return Name + "?0";
        }
        public override string getToolTipString()
        {
            return (Type.HideDeclare ? "" :Type.DisplayName + " ") +
                   (Class == null  ?  "" : Class.DisplayName + "::")
                   + Name + "\n\n" + Desc;
        }
    }

    class VariableManager {
        public VariableManager() { 
            m_variables =new Dictionary<string,Variable>();
            m_globalFunctions = new Dictionary<string, Function>();
        }
        private Dictionary<string, Variable> m_variables;
        public Dictionary<string, Variable> Variables
        {
            get { return m_variables; }
            set { m_variables = value; }
        }

        private Dictionary<string, Function> m_globalFunctions;
        public Dictionary<string, Function> GlobalFunctions
        {
            get { return m_globalFunctions; }
            set { m_globalFunctions = value; }
        }
        public void add(Variable var) {
            m_variables[var.Name] = var;
        }
        public void add(Function func) {
            m_globalFunctions[func.Name] = func;
        }
        public Variable getVariable(string name) {
            if(m_variables.ContainsKey(name))
                return m_variables[name];
            return null;
        }
        public Function getFunction(string name)
        {
            if (m_globalFunctions.ContainsKey(name))
                return m_globalFunctions[name];
            return null;
        }

        public List<IAutoCompleteItem> getList(string partialName) {
            List<IAutoCompleteItem> rst = new List<IAutoCompleteItem>();
            foreach (Variable var in Variables.Values) {
                if (var.Name.StartsWith(partialName,true,null)) {
                    rst.Add(var);
                }
            }
            foreach(Function func in GlobalFunctions.Values){
                if(func.Name.StartsWith(partialName,true,null)){
                    rst.Add(func);
                }
            }
            rst.Sort();
            return rst;
        }

        //purge all variables affected by changes made after pos
        public void purge(int pos) {
            List<string> rm = new List<string>();
            foreach (Variable var in m_variables.Values) {
                if (var.IsStatic) continue;


                if (var.StartPos >= pos)
                {
                    rm.Add(var.Name);
                    continue;
                }
                if (var.EndPos >= pos)
                {
                    rm.Add(var.Name);
                    continue;
                }
            }
            foreach (string name in rm) {
                //System.Diagnostics.Debug.Print(name + " removed");
                m_variables.Remove(name);
            }
        }
    }
}
