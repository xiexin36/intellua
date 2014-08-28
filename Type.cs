using System.Collections.Generic;
using System.Linq;

namespace Intellua
{
    public class Type : IAutoCompleteItem
    {
        #region Fields (7)

        private Type m_base;
        private string m_displayName;
        private bool m_hideDeclare = false;
        private Dictionary<string, Variable> m_members;
        private Dictionary<string, Function> m_methods;
        private Dictionary<string, Type> m_innerClasses;
        private string m_name;
        private Type m_outerClass;
        public bool NoAC = false;
        #endregion Fields

        #region Constructors (1)

        public Type(string name)
        {
            DisplayName = InternalName = name;
            m_members = new Dictionary<string, Variable>();
            m_methods = new Dictionary<string, Function>();
            m_innerClasses = new Dictionary<string, Type>();
        }
        public Type(string name,bool noac)
        {
            DisplayName = InternalName = name;
            m_members = new Dictionary<string, Variable>();
            m_methods = new Dictionary<string, Function>();
            m_innerClasses = new Dictionary<string, Type>();
            NoAC = noac;
        }

        #endregion Constructors

        #region Properties (7)
        public bool Private = false;
        public override bool isPrivate()
        {
            return Private;
        }
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

        public Dictionary<string, Type> InnerClasses
        {
            get { return m_innerClasses; }
            set { m_innerClasses = value; }
        }

        #endregion Properties

        #region Methods (3)

        public override string getACString()
        {
            return DisplayName + "?3";
        }

        public override string getName()
        {
            return DisplayName;
        }

        public override string getToolTipString()
        {
            return DisplayName;
        }


        public void addMember(Variable var)
        {
            if (var.Class == null) var.Class = this;
            m_members[var.Name] = var;
        }

        public void addMethod(Function method)
        {
            if (!m_methods.ContainsKey(method.Name))
            {
                method.Class = this;
                m_methods[method.Name] = method;
            }
            else
            {
                /*m_methods[method.Name].Param.Add(method.Param[0]);
                m_methods[method.Name].Desc.Add(method.Desc[0]);*/

                for (int i = 0; i < method.Param.Count; i++)
                {
                    if (m_methods[method.Name].Param.Contains(method.Param[i]))
                    {
                        continue;
                    }
                    m_methods[method.Name].Param.Add(method.Param[i]);
                    m_methods[method.Name].Desc.Add(method.Desc[i]);
                }
            }
        }
        public void addClass(Type cls) {
            InnerClasses.Add(cls.DisplayName, cls);
        }

        public List<IAutoCompleteItem> getList(bool Static)
        {
            List<IAutoCompleteItem> rst;
            if (Base != null)
            {
                rst = Base.getList(Static);
                AutoCompleteItemComparer comparer = new AutoCompleteItemComparer();
                List<IAutoCompleteItem> rm = new List<IAutoCompleteItem>();
                foreach (IAutoCompleteItem item in rst)
                {
                    if (m_methods.Values.Contains(item, comparer))
                    {
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
            foreach (Variable key in m_members.Values)
            {
                if (key.IsStatic == Static)
                    rst.Add(key);
            }
            foreach (Function key in m_methods.Values)
            {
                if (key.Static == Static)
                    rst.Add(key);
            }
            foreach (Type key in m_innerClasses.Values)
            {
                    rst.Add(key);
            }
            rst.Sort();
            return rst;
        }

        // Public Methods (3) 
        public Variable getMember(string name)
        {
            if (Members.ContainsKey(name)) return Members[name];
            if (Base != null) return Base.getMember(name);
            return null;
        }

        public Type getClass(string name) { 
            if (InnerClasses.ContainsKey(name)) return InnerClasses[name];
            if (Base != null) return Base.getClass(name);
            return null;

        }

        public Function getMethod(string name)
        {
            if (Methods.ContainsKey(name)) return Methods[name];
            if (Base != null) return Base.getMethod(name);
            return null;
        }

        
        #endregion Methods
    }

    internal class TypeManager
    {
        #region Fields (2)

        private static Type m_nullType;
        private Dictionary<string, Type> m_dtypes = new Dictionary<string, Type>();
        private TypeManager m_parent;
        private List<AutoCompleteData> m_requires;
        private Dictionary<string, Type> m_types;
        static TypeManager()
        {
            m_nullType = new Type("(UnknownType)");
            m_nullType.DisplayName = "";
            m_nullType.HideDeclare = true;
        }

        public TypeManager()
        {
            m_types = new Dictionary<string, Type>();

            m_parent = null;
        }

        public TypeManager(TypeManager parent)
        {
            m_types = new Dictionary<string, Type>();
            m_nullType = m_parent.NullType;
            m_parent = parent;
        }

        public Type NullType
        {
            get { return m_nullType; }
        }

        public TypeManager Parent
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

        #endregion Fields
        #region Constructors (1)
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

        public void add(Type t)
        {
            m_types[t.InternalName] = t;
            m_dtypes[t.DisplayName] = t;
        }

        public Type get(string name)
        {
            if (name == null) return m_nullType;
            if (m_types.ContainsKey(name)) return m_types[name];
            if (m_dtypes.ContainsKey(name)) return m_dtypes[name];

            foreach (AutoCompleteData ac in Requires)
            {
                var rst = ac.Types.get(name);
                if (rst != null && rst != m_nullType && !rst.isPrivate()) return rst;
            }

            if (m_parent != null) return m_parent.get(name);
            return m_nullType;
        }

        public void appendList(List<IAutoCompleteItem> lst, string partialName)
        {
            foreach (Type t in m_types.Values)
            {
                if (t.OuterClass != null) continue;
                if (t.NoAC) continue;
                if (t.DisplayName.StartsWith(partialName,true,null))
                {
                    if (!lst.Contains(t))
                    {
                        lst.Add(t);
                    }
                }
            }
            if (m_parent != null) m_parent.appendList(lst, partialName);
            //lst.Sort();
        }

        public void removeEmptyNamespace()
        {
            foreach (Type t in Types.Values)
            {
                List<string> rm = new List<string>();
                foreach (Variable var in t.Members.Values)
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
                    t.Members.Remove(str);
                }
            }
        }

        #endregion Methods
    }
}