using System.Collections.Generic;

namespace Intellua
{
    public class AutoCompleteData
    {
        private KeywordManager m_keywords;
        private AutoCompleteData m_parent;
        private List<AutoCompleteData> m_requires = new List<AutoCompleteData>();
        private TypeManager m_typeManager;
        private VariableManager m_variableManager;
        public AutoCompleteData()
        {
            m_typeManager = new TypeManager();
            m_variableManager = new VariableManager();
            m_keywords = new KeywordManager();
            m_typeManager.add(new Type("object"));
            m_typeManager.add(new Type("int"));
            m_typeManager.add(new Type("void"));
            m_typeManager.add(new Type("char"));
            m_typeManager.add(new Type("float"));
            m_typeManager.add(new Type("double"));
            m_typeManager.add(new Type("string"));
            m_typeManager.add(new Type("table"));
            m_typeManager.add(new Type("number"));
            m_typeManager.add(new Type("boolean"));
            m_typeManager.add(new Type("function"));
            m_typeManager.add(new Type("thread"));
            m_typeManager.add(new Type("userdata"));

            m_keywords.add(new Keyword("break"));
            m_keywords.add(new Keyword("else"));
            m_keywords.add(new Keyword("elseif"));
            m_keywords.add(new Keyword("false"));
            m_keywords.add(new Keyword("function"));
            m_keywords.add(new Keyword("local"));
            m_keywords.add(new Keyword("repeat"));
            m_keywords.add(new Keyword("return"));
            m_keywords.add(new Keyword("then"));
            m_keywords.add(new Keyword("true"));
            m_keywords.add(new Keyword("until"));
            m_keywords.add(new Keyword("while"));
            init();
        }

        public AutoCompleteData(AutoCompleteData parent)
        {
            m_typeManager = new TypeManager(parent.Types);
            m_variableManager = new VariableManager(parent.Variables);
            m_keywords = new KeywordManager();

            m_keywords.add(new Keyword("break"));
            m_keywords.add(new Keyword("else"));
            m_keywords.add(new Keyword("elseif"));
            m_keywords.add(new Keyword("false"));
            m_keywords.add(new Keyword("function"));
            m_keywords.add(new Keyword("local"));
            m_keywords.add(new Keyword("repeat"));
            m_keywords.add(new Keyword("return"));
            m_keywords.add(new Keyword("then"));
            m_keywords.add(new Keyword("true"));
            m_keywords.add(new Keyword("until"));
            m_keywords.add(new Keyword("while"));

            init();
        }

        public AutoCompleteData(string filename)
        {
            AutoCompleteData rst = DoxygenXMLParser.Parse(filename);
            m_typeManager = rst.Types;
            m_variableManager = rst.Variables;
            init();
        }

        public List<AutoCompleteData> Requires
        {
            get { return m_requires; }
            set { m_requires = value; }
        }

        internal KeywordManager Keywords
        {
            get
            {
                return m_keywords;
            }
        }

        internal TypeManager Types
        {
            get
            {
                return m_typeManager;
            }
        }

        internal VariableManager Variables
        {
            get
            {
                return m_variableManager;
            }
        }
        public AutoCompleteData getParent()
        {
            return m_parent;
        }

        public void setParent(AutoCompleteData parent)
        {
            m_parent = parent;
            if (m_parent != null)
            {
                m_typeManager.Parent = parent.Types;
                m_variableManager.Parent = parent.Variables;
            }
            else
            {
                m_typeManager.Parent = null;
                m_variableManager.Parent = null;
            }
        }

        private void init()
        {
            m_typeManager.Requires = Requires;
            m_variableManager.Requires = Requires;
        }
    }
}