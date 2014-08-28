using System.Collections.Generic;
using System.Linq;

namespace Intellua
{
    public class Scope
    {
        private List<Scope> m_childs = new List<Scope>();
        private int m_endPos = 0;
        private Scope m_parent = null;

        private int m_startPos = 0;
        
        private List<Variable> m_variables = new List<Variable>();

        public List<Variable> Variables {
            get { return m_variables; }
        }
        
        public List<Scope> Childs
        {
            get { return m_childs; }
            set { m_childs = value; }
        }

        public int EndPos
        {
            get { return m_endPos; }
            set { m_endPos = value; }
        }

        public Scope Parent
        {
            get { return m_parent; }
            set { m_parent = value; }
        }
        public int StartPos
        {
            get { return m_startPos; }
            set { m_startPos = value; }
        }
        public void addVariable(Variable var)
        {
            Scope s = getScope(var.StartPos);
            s.m_variables.Add(var);
        }
        public void addChild(Scope child) {
            Childs.Add(child);
            child.Parent = this;
        }
        public Scope getScope(int pos)
        {
            foreach (Scope s in m_childs)
            {
                if (s.m_startPos <= pos && s.m_endPos > pos) return s.getScope(pos);
            }
            return this;
        }

        public Variable getVariable(string name, int pos)
        {
            Variable rst = null;
            foreach (Variable v in m_variables)
            {
                if (v.Name != name) continue;
                if (v.StartPos > pos) continue;
                rst = v;
            }
            if (rst != null) return rst;
            if (m_parent != null) return m_parent.getVariable(name, pos);
            return null;
        }
    }

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

        public Variable(string name)
        {
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
        public bool Private = false;
        public override bool isPrivate()
        {
            return Private;
        }
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
            return (Type.HideDeclare || IsNamespace ? "" : Type.DisplayName + " ") +
                (Class == null ? "" : Class.DisplayName + (IsStatic ? "." : "::"))
                   + Name + "\n\n" + Desc;
        }

        #endregion Methods
    }
    internal class VariableManager
    {
        #region Fields (2)

        private Dictionary<string, Function> m_globalFunctions;
        private VariableManager m_parent;
        private List<AutoCompleteData> m_requires;
        private Scope m_scope;
        private Dictionary<string, Variable> m_variables;
        public VariableManager()
        {
            m_variables = new Dictionary<string, Variable>();
            m_globalFunctions = new Dictionary<string, Function>();
            m_parent = null;
        }

        public VariableManager(VariableManager parent)
        {
            m_variables = new Dictionary<string, Variable>();
            m_globalFunctions = new Dictionary<string, Function>();
            m_parent = parent;
        }

        public Dictionary<string, Function> GlobalFunctions
        {
            get { return m_globalFunctions; }
            set { m_globalFunctions = value; }
        }

        public VariableManager Parent
        {
            set
            {
                m_parent = value;
            }
        }

        public List<AutoCompleteData> Requires
        {
            get { return m_requires; }
            set { m_requires = value; }
        }

        public Scope scope
        {
            get { return m_scope; }
            set { m_scope = value; }
        }
        #endregion Fields
        #region Constructors (1)
        #endregion Constructors

        #region Properties (2)
        public Dictionary<string, Variable> Variables
        {
            get { return m_variables; }
            set { m_variables = value; }
        }

        #endregion Properties

        #region Methods (6)

        // Public Methods (6) 

        public void add(Variable var)
        {
            m_variables[var.Name] = var;

            if (m_scope != null)
            {
                m_scope.addVariable(var);
            }
        }

        public void add(Function func)
        {
            if (m_globalFunctions.ContainsKey(func.Name))
            {
                for(int i =0;i<func.Param.Count;i++){
                    if(m_globalFunctions[func.Name].Param.Contains(func.Param[i])) {
                        continue;
                    }
                    m_globalFunctions[func.Name].Param.Add(func.Param[i]);
                    m_globalFunctions[func.Name].Desc.Add(func.Desc[i]);
                }
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

            foreach (AutoCompleteData ac in Requires)
            {
                var rst = ac.Variables.getFunction(name);
                if (rst != null && rst.Private) rst = null;
                if (rst != null) return rst;
            }

            if (m_parent != null)
                return m_parent.getFunction(name);
            return null;
        }

        public List<IAutoCompleteItem> getList(string partialName,int pos)
        {
            List<IAutoCompleteItem> rst = new List<IAutoCompleteItem>();
            if (pos != -1 && m_scope != null) {
                Scope s = m_scope.getScope(pos);
                while (s != null) {
                    for (int i = s.Variables.Count - 1; i >= 0; i--) {
                        Variable var = s.Variables[i];
                        if (var.StartPos < pos && var.Name.StartsWith(partialName, true, null))
                        {
                            if (!rst.Contains(var))
                            {
                                rst.Add(var);
                            }
                        }
                    }
                    s = s.Parent;
                }
            }

            foreach (Variable var in Variables.Values)
            {
                if (var.Name.StartsWith(partialName, true, null))
                {
                    rst.Add(var);
                }
            }

            foreach (Function func in GlobalFunctions.Values)
            {
                if (func.Name.StartsWith(partialName, true, null))
                {
                    rst.Add(func);
                }
            }
            if (m_parent != null)
            {
                List<IAutoCompleteItem> pr = m_parent.getList(partialName,pos);
                foreach (IAutoCompleteItem item in pr)
                {
                    rst.Add(item);
                }
            }

            foreach (AutoCompleteData ac in Requires)
            {
                var rrst = ac.Variables.getList(partialName,pos);
                foreach (IAutoCompleteItem t in rrst) {
                    if (t.isPrivate()) continue;
                    rst.Add(t);
                }
                
            }

            rst.Sort();
            rst = rst.Distinct().ToList();
            return rst;
        }

        public Variable getVariable(string name)
        {
            if (m_variables.ContainsKey(name))
                return m_variables[name];
            if (m_parent != null)
                return m_parent.getVariable(name);
            return null;
        }

        public Variable getVariable(string name, int pos)
        {
            if (m_scope != null)
            {
                Scope s = m_scope.getScope(pos);
                Variable rst = s.getVariable(name, pos);
                if (rst != null) return rst;
            }

            foreach (AutoCompleteData ac in Requires)
            {
                var rst = ac.Variables.getVariable(name, -1);

                if (rst != null && !rst.isPrivate()) return rst;
            }

            return getVariable(name);
        }

        //purge all variables affected by changes made after pos
        public void purge(int pos)
        {
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

        public void removeEmptyNamespace()
        {
            List<string> rm = new List<string>();
            foreach (Variable var in m_variables.Values)
            {
                if (var.IsNamespace)
                {
                    if (var.Type.getList(true).Count == 0)
                    {
                        rm.Add(var.Name);
                    }
                }
            }
            foreach (string str in rm)
            {
                m_variables.Remove(str);
            }
        }

        #endregion Methods
    }
}