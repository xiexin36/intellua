using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    abstract class IAutoCompleteItem : IComparable
    {
		#region Methods (5) 

		// Public Methods (5) 

        public Int32 CompareTo(IAutoCompleteItem other)
        {
            return getName().CompareTo(other.getName());
        }

        public int CompareTo(Object obj)
        {
            IAutoCompleteItem item = obj as IAutoCompleteItem;
            if (item != null)
                return CompareTo(item);
            else {
                throw new ArgumentException("Object is not a IAutoCompleteItem");
            }
        }

        public abstract string getACString();

        public abstract string getName();

        public abstract string getToolTipString();

		#endregion Methods 
    }

    class AutoCompleteItemComparer : EqualityComparer<IAutoCompleteItem> {
		#region Methods (2) 

		// Public Methods (2) 

        public override bool Equals(IAutoCompleteItem b1, IAutoCompleteItem b2)
        {
            if (b1.getName() == b2.getName())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode(IAutoCompleteItem item)
        {
            return item.getName().GetHashCode();
        }

		#endregion Methods 
    }
}
