using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Intellua
{
    class Variable : IAutoCompleteItem
    {
		#region Fields (8) 

        private Type m_Class;
        private string m_desc;
        private int m_endPos;
        private bool m_isNamespace;
        private bool m_isStatic;
        private string m_name;
        private int m_startPos;
        private Type m_Type;

		#endregion Fields 

		#region Constructors (1) 

        public Variable(string name) {
            Name = name;
            m_isStatic = false;
        }

		#endregion Constructors 

		#region Properties (8) 

        public Type Class
        {
            get { return m_Class; }
            set { m_Class = value; }
        }

        public string Desc
        {
            get { return m_desc; }
            set { m_desc = value; }
        }

        public int EndPos
        {
            get { return m_endPos; }
            set { m_endPos = value; }
        }

        public bool IsNamespace
        {
            get { return m_isNamespace; }
            set { m_isNamespace = value; }
        }

        public bool IsStatic
        {
            get { return m_isStatic; }
            set { m_isStatic = value; }
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public int StartPos
        {
            get { return m_startPos; }
            set { m_startPos = value; }
        }

        public Type Type
        {
            get { return m_Type; }
            set { m_Type = value; }
        }

		#endregion Properties 

		#region Methods (3) 

		// Public Methods (3) 

        public override string getACString()
        {
            return Name + "?0";
        }

        public override string getName()
        {
            return Name;
        }

        public override string getToolTipString()
        {
            return (Type.HideDeclare || IsNamespace ? "" :Type.DisplayName + " ") +
                (Class == null  ?  "" : Class.DisplayName + (IsStatic?".":"::"))
                   + Name + "\n\n" + Desc;
        }

		#endregion Methods 
    }

    class VariableManager {
		#region Fields (2) 

        private Dictionary<string, Function> m_globalFunctions;
        private Dictionary<string, Variable> m_variables;

		#endregion Fields 

		#region Constructors (1) 

        public VariableManager() { 
            m_variables =new Dictionary<string,Variable>();
            m_globalFunctions = new Dictionary<string, Function>();
        }

		#endregion Constructors 

		#region Properties (2) 

        public Dictionary<string, Function> GlobalFunctions
        {
            get { return m_globalFunctions; }
            set { m_globalFunctions = value; }
        }

        public Dictionary<string, Variable> Variables
        {
            get { return m_variables; }
            set { m_variables = value; }
        }

		#endregion Properties 

		#region Methods (6) 

		// Public Methods (6) 

        public void add(Variable var) {
            m_variables[var.Name] = var;
        }

        public void add(Function func) {
            m_globalFunctions[func.Name] = func;
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

        public Variable getVariable(string name) {
            if(m_variables.ContainsKey(name))
                return m_variables[name];
            return null;
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

        public void removeEmptyNamespace() {
            List<string> rm = new List<string>();
            foreach (Variable var in m_variables.Values)
            {
                if (var.IsNamespace) {
                    if (var.Type.getList(true).Count == 0) {
                        rm.Add(var.Name);
                    }
                }
            }
            foreach (string str in rm) {
                m_variables.Remove(str);
            }
        }

		#endregion Methods 
    }
}
