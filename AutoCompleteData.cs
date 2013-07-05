using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    public class AutoCompleteData
    {
        private TypeManager m_typeManager;
        private VariableManager m_variableManager;

        internal TypeManager Types {
            get {
                return m_typeManager;
            }
        }
        internal VariableManager Variables
        {
            get {
                return m_variableManager;
            }
        }

        public AutoCompleteData() {
            m_typeManager = new TypeManager();
            m_variableManager = new VariableManager();

            m_typeManager.add(new Type("int"));
            m_typeManager.add(new Type("void"));
            m_typeManager.add(new Type("char"));
            m_typeManager.add(new Type("float"));
            m_typeManager.add(new Type("double"));
            //m_types.add(new Type("string"));
            //m_types.add(new Type("table"));
            m_typeManager.add(new Type("number"));
            m_typeManager.add(new Type("boolean"));
            m_typeManager.add(new Type("function"));
            m_typeManager.add(new Type("thread"));
            m_typeManager.add(new Type("userdata"));
        }

        public AutoCompleteData(AutoCompleteData parent) {
            m_typeManager = new TypeManager(parent.Types);
            m_variableManager = new VariableManager(parent.Variables);
        }

        public AutoCompleteData(string filename) {
            AutoCompleteData rst = DoxygenXMLParser.Parse(filename);
            m_typeManager = rst.Types;
            m_variableManager = rst.Variables;
        }

        public void setParent(AutoCompleteData parent) {
            m_typeManager.Parent = parent.Types;
            m_variableManager.Parent = parent.Variables;
        }
    }
}
