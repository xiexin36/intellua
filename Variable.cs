using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Intellua
{
    public class Variable : IAutoCompleteItem
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
    public class Scope {
        private Scope m_parent = null;
        public Scope Parent
        {
            get { return m_parent; }
            set { m_parent = value; }
        }
        private List<Scope> m_childs = new List<Scope>();
        public List<Scope> Childs
        {
            get { return m_childs; }
            set { m_childs = value; }
        }
        private List<Variable> m_variables = new List<Variable>();
        private int m_startPos = 0;
        public int StartPos
        {
            get { return m_startPos; }
            set { m_startPos = value; }
        }
        private int m_endPos = 0;
        public int EndPos
        {
            get { return m_endPos; }
            set { m_endPos = value; }
        }
        public Scope getScope(int pos) {
            foreach (Scope s in m_childs) {
                if (s.m_startPos <= pos && s.m_endPos > pos) return s;
            }
            return this;
        }

        public Variable getVariable(string name,int pos) {
            Variable rst = null;
            foreach (Variable v in m_variables) {
                if (v.Name != name) continue;
                if (v.StartPos > pos) continue;
                rst = v;
            }
            if (rst != null) return rst;
            if (m_parent!=null) return m_parent.getVariable(name, pos);
            return null;
        }
        public void addVariable(Variable var) {
            Scope s = getScope(var.StartPos);
            s.m_variables.Add(var);
        }
    }

    class VariableManager {
		#region Fields (2) 

        private Dictionary<string, Function> m_globalFunctions;
        private Dictionary<string, Variable> m_variables;
        private VariableManager m_parent;
        private Scope m_scope;
        public Scope scope {
            set { m_scope = value; }
        }
        public VariableManager Parent {
            set {
                m_parent = value;
            }
        }
		#endregion Fields 
        List<AutoCompleteData> m_requires;
        public List<AutoCompleteData> Requires
        {
            get { return m_requires; }
            set { m_requires = value; }
        }
		#region Constructors (1) 

        public VariableManager() { 
            m_variables =new Dictionary<string,Variable>();
            m_globalFunctions = new Dictionary<string, Function>();
            m_parent = null;
        }
        public VariableManager(VariableManager parent)
        {
            m_variables = new Dictionary<string, Variable>();
            m_globalFunctions = new Dictionary<string, Function>();
            m_parent = parent;
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

            if (m_scope != null) {
                m_scope.addVariable(var);
            }
        }

        public void add(Function func) {
            if (m_globalFunctions.ContainsKey(func.Name))
            {
                m_globalFunctions[func.Name].Param.Add(func.Param[0]);
                m_globalFunctions[func.Name].Desc.Add(func.Desc[0]);
            }
            else
            {
                m_globalFunctions[func.Name] = func;
            }
        }

        public Function getFunction(string name)
        {
            

            if (m_globalFunctions.ContainsKey(name))
                return m_globalFunctions[name];

            foreach (AutoCompleteData ac in Requires) {
                var rst = ac.Variables.getFunction(name);
                if (rst != null) return rst;
            }

            if (m_parent!= null)
                return m_parent.getFunction(name);
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
            if (m_parent!=null) {
                List<IAutoCompleteItem> pr = m_parent.getList(partialName);
                foreach (IAutoCompleteItem item in pr) {
                    rst.Add(item);
                }
            }

            foreach (AutoCompleteData ac in Requires)
            {
                var rrst = ac.Variables.getList(partialName);
                rst.AddRange(rrst);
            }

            rst.Sort();
            rst = rst.Distinct().ToList();
            return rst;
        }

        public Variable getVariable(string name) {
            if(m_variables.ContainsKey(name))
                return m_variables[name];
            if (m_parent != null)
                return m_parent.getVariable(name);
            return null;
        }
        public Variable getVariable(string name,int pos)
        {
            if(m_scope != null){
                Scope s = m_scope.getScope(pos);
                Variable rst = s.getVariable(name, pos);
                if (rst != null) return rst;
            }

            foreach (AutoCompleteData ac in Requires)
            {
                var rst = ac.Variables.getVariable(name,-1);
                if (rst != null) return rst;
            }

            return getVariable(name);
        }
        //purge all variables affected by changes made after pos
        public void purge(int pos) {
            m_variables.Clear();
            m_scope = null;
            /*List<string> rm = new List<string>();
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
            }*/
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
