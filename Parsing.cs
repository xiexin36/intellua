using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    class Parser {
		#region Methods (3) 

		// Public Methods (3) 

        public static bool isCode(Intellua scintilla, int pos) {
            pos = scintilla.Encoding.GetByteCount(scintilla.Text.ToCharArray(), 0, pos + 1) - 1;
            int style = (scintilla.Styles.GetStyleAt(pos) & 0x1f);
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

        public static bool isComment(Intellua scintilla, int pos)
        {
            pos = scintilla.Encoding.GetByteCount(scintilla.Text.ToCharArray(), 0, pos + 1) - 1;
            int style = (scintilla.Styles.GetStyleAt(pos) & 0x1f);
            switch (style)
            {
                case 1:
                case 2:
                    return true;
            }
            return false;
        }

        public static bool isString(Intellua scintilla, int pos)
        {
            pos = scintilla.Encoding.GetByteCount(scintilla.Text.ToCharArray(), 0, pos + 1) - 1;
            int style = (scintilla.Styles.GetStyleAt(pos) & 0x1f);
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
