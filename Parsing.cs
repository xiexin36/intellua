using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    class Parser {
		#region Methods (3) 

		// Public Methods (3) 

        public static bool isCode(IntelluaSource source, int pos) {

            int style = source.getStyleAt(pos);
            switch (style)
            {
                case 1:
                case 2:
                case 6:
                case 7:
                case 12:
                    return false;
            }
            return true;
        }

        public static bool isComment(IntelluaSource source, int pos)
        {
            int style = source.getStyleAt(pos);
            switch (style)
            {
                case 1:
                case 2:
                    return true;
            }
            return false;
        }

        public static bool isString(IntelluaSource source, int pos)
        {
            int style = source.getStyleAt(pos);
            switch (style)
            {
                case 6:
                case 7:
                case 12:
                    return true;
            }
            return false;
        }

		#endregion Methods 
    }
    
}
