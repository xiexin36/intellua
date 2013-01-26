using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    class Parser {
        public static bool isString(ScintillaNET.Scintilla scintilla, int pos) { 
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

        public static bool isComment(ScintillaNET.Scintilla scintilla, int pos)
        {
            int style = (scintilla.Styles.GetStyleAt(pos) & 0x1f);
            switch (style)
            {
                case 1:
                case 2:
                    return true;
            }
            return false;
        }

        public static bool isCode(ScintillaNET.Scintilla scintilla, int pos) {
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
    }
    
}
