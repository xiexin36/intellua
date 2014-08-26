using System.Collections.Generic;

namespace Intellua
{
    public class Keyword : IAutoCompleteItem
    {
        private string m_name;
        public override bool isPrivate()
        {
            return true;
        }
        public Keyword(string name)
        {
            Name = name;
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }
        public override string getACString()
        {
            return Name + "?4";
        }

        public override string getName()
        {
            return Name;
        }

        public override string getToolTipString()
        {
            return Name;
        }
    }

    public class KeywordManager
    {
        private Dictionary<string, Keyword> m_keywords = new Dictionary<string, Keyword>();

        public void add(Keyword k)
        {
            m_keywords[k.Name] = k;
        }

        public void appendList(List<IAutoCompleteItem> lst, string partialName)
        {
            foreach (Keyword k in m_keywords.Values)
            {
                if (k.Name.StartsWith(partialName))
                {
                    lst.Add(k);
                }
            }
            //lst.Sort();
        }
    }
}