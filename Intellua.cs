using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;


namespace Intellua
{
    public class Intellua: ScintillaNET.Scintilla
    {
		#region Fields (5) 

        private List<IAutoCompleteItem> m_autocompleteList;
        private FunctionCall m_calltipFuncion;
        private ToolTip m_tooltip;
        private AutoCompleteData m_autoCompleteData;

		#endregion Fields 

		#region Constructors (1) 
        public void setParent(AutoCompleteData parent) {
            m_autoCompleteData.setParent(parent);
        }
        public Intellua()
        {
            this.AutoCompleteAccepted += new System.EventHandler<ScintillaNET.AutoCompleteAcceptedEventArgs>(this.intellua_AutoCompleteAccepted);
            this.AutoCompleteCancelled += new System.EventHandler<ScintillaNET.NativeScintillaEventArgs>(this.intellua_AutoCompleteCancelled);
            this.AutoCompleteMoved += new System.EventHandler<ScintillaNET.NativeScintillaEventArgs>(this.intellua_AutoCompleteMoved);
            this.CallTipClick += new System.EventHandler<ScintillaNET.CallTipClickEventArgs>(this.intellua_CallTipClick);
            this.CharAdded += new System.EventHandler<ScintillaNET.CharAddedEventArgs>(this.intellua_CharAdded);
            this.TextDeleted += new System.EventHandler<ScintillaNET.TextModifiedEventArgs>(this.intellua_TextDeleted);
            this.TextInserted += new System.EventHandler<ScintillaNET.TextModifiedEventArgs>(this.intellua_TextInserted);
            this.SelectionChanged += new EventHandler(this.Intellua_SelectionChanged);
            
            m_tooltip = new ToolTip(this);
            m_autoCompleteData = new AutoCompleteData();
            ScintillaNET.Configuration.Configuration config =
                new ScintillaNET.Configuration.Configuration(Assembly.GetExecutingAssembly().GetManifestResourceStream("Intellua.ScintillaNET.xml"),
                    "lua", true);
            ConfigurationManager.Language = "lua";
            ConfigurationManager.Configure(config);
            Folding.IsEnabled = true;
            /*Margins[0].Width = 20;
            Margins[1].Width = 20;
            Margins[2].Width = 20;*/
            AutoComplete.IsCaseSensitive = false;
            AutoComplete.AutoHide = false;
            //Indentation.ShowGuides = true;

           
            List<Bitmap> list = new List<Bitmap>();
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream str;
            str = asm.GetManifestResourceStream("Intellua.member.png");
            list.Add(new Bitmap(str));
            str = asm.GetManifestResourceStream("Intellua.method.png");
            list.Add(new Bitmap(str));
            str = asm.GetManifestResourceStream("Intellua.function.png");
            list.Add(new Bitmap(str));

            AutoComplete.RegisterImages(list);

            
            

            
        }
        public int getDecodedPos()
        {
            return Encoding.GetCharCount(RawText, 0, CurrentPos + 1) - 1;
        }

        public int getDecodedPos(int bytePos)
        {
            return Encoding.GetCharCount(RawText, 0, bytePos + 1) - 1;
        }
        void Intellua_SelectionChanged(object sender, EventArgs e)
        {
            const string lbracket = "([{";
            const string rbracket = ")]}";
            int pos = getDecodedPos();
            int style = Styles.GetStyleAt(pos-1);
            int start, end;
            start = end = -1;

            Stack<char> stk = new Stack<char>();

            for (int p = pos-1; p >= 0; p--) {
               // if (Styles.GetStyleAt(p) != style) continue;
                if (p >= Text.Length) continue;
                char c = Text[p];
                if (rbracket.Contains(c))
                {
                    stk.Push(c);
                }
                if (lbracket.Contains(c))
                {
                    if(stk.Count == 0){
                        start = p;
                        break;
                    }
                    char pc = stk.Pop();
                    if ((pc == ')' && c != '(') ||
                        (pc == ']' && c != '[') ||
                        (pc == '}' && c != '{')) { 
                        break;
                    }
                }
            }
            stk.Clear();

            for (int p = pos; p < Text.Length; p++)
            {
               // if (Styles.GetStyleAt(p) != style) continue;
                char c = Text[p];
                if (lbracket.Contains(c)) 
                {
                    stk.Push(c);
                }
                if (rbracket.Contains(c))
                {
                    if (stk.Count == 0)
                    {
                        end = p;
                        break;
                    }
                    char pc = stk.Pop();
                    if ((pc != ')' && c == '(') ||
                        (pc != ']' && c == '[') ||
                        (pc != '}' && c == '{'))
                    {
                        break;
                    }
                }
            }

            if (start >= 0 && end >= 0)
            {
                char c = Text[start];
                char pc = Text[end];

                if ((pc != ')' && c == '(') ||
                        (pc != ']' && c == '[') ||
                        (pc != '}' && c == '{'))
                {
                    start = -1;
                }
            }

            if (start != -1) {
                start = Encoding.GetByteCount(Text.ToCharArray(), 0, start+1)-1;
            }
            if (end != -1) {
                end = Encoding.GetByteCount(Text.ToCharArray(), 0, end + 1) - 1;
            }

            if (start == -1)
            {
                if (end != -1)
                {
                    NativeInterface.BraceBadLight(end);
                }
                else {
                    NativeInterface.BraceHighlight(-1, -1);
                }
            }
            else if (end == -1)
            {
                if (start != -1)
                {
                    NativeInterface.BraceBadLight(start);
                }
                else{
                    NativeInterface.BraceHighlight(-1, -1);
                }
            }
            else
            {

                NativeInterface.BraceHighlight(start, end);
            }



        }

        #endregion Constructors

        #region Methods (14)

        // Public Methods (3) 

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd,
           StringBuilder lpClassName,
           int nMaxCount
        );
        Scope parseScope(int start,int end) {
            int level = Lines[start].FoldLevel;
            Scope rst = new Scope();
            rst.StartPos = Lines[start].StartPosition;
            rst.EndPos = Lines[end].EndPosition;

            for (int i = start; i <= end; i++) {
                if (Lines[i].FoldLevel != level) {
                    int s = i;
                    while (i <= end && Lines[i].FoldLevel != level) {
                        i++;
                    }
                    i--;
                    int e = i;
                    Scope c = parseScope(s, e);
                    c.Parent = rst;
                    rst.Childs.Add(c);
                }
            }

            return rst;
        }
        public void parseFile(int pos) {
            pos = 0;
            string str = Text;
            m_autoCompleteData.Variables.purge(pos);
            m_autoCompleteData.Variables.scope = parseScope(0,Lines.Count-1);
            
            for (; pos< str.Length; pos++) {
                char c = str[pos];

                //search for assignment operator

                if(!Parser.isCode(this,pos)){
                    continue;
                }

                if(c != '=') continue;
                if(pos>0){
                    if(str[pos-1] == '=') continue;
                }
                if(pos < str.Length -1){
                    if(str[pos+1] == '=') continue;
                }
                MemberChain v = MemberChain.ParseBackward(this,pos-1);
                if(v.Elements.Count > 1 || v.Elements.Count ==0) continue;
                
                string varName = v.getLastElement();
                Variable var = m_autoCompleteData.Variables.getVariable(varName);
//                if (var != null) continue;

                MemberChain e = MemberChain.ParseFoward(this,pos+1);
                if(e == null) continue;
                Type t = e.getType(m_autoCompleteData.Variables);
                if (t == null) continue;

                //System.Diagnostics.Debug.Print(varName + " added");

                var = new Variable(varName);
                var.IsStatic = false;
                var.Type = t;
                var.StartPos = v.StartPos;
                var.EndPos = e.EndPos;
                m_autoCompleteData.Variables.add(var);
                
            }

        }
		// Private Methods (11) 

         [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private void intellua_AutoCompleteAccepted(object sender, ScintillaNET.AutoCompleteAcceptedEventArgs e)
        {
            m_tooltip.Hide();
            
        }

        private void intellua_AutoCompleteCancelled(object sender, EventArgs e)
        {
            m_tooltip.Hide();
        }

        private void intellua_AutoCompleteMoved(object sender, ScintillaNET.NativeScintillaEventArgs e)
        {
            m_tooltip.setText(m_autocompleteList[AutoComplete.SelectedIndex].getToolTipString());
        }

        private void intellua_CallTipClick(object sender, ScintillaNET.CallTipClickEventArgs e)
        {
            Function func = m_calltipFuncion.Func;
            func.CurrentOverloadIndex++;
            if (func.CurrentOverloadIndex == func.Param.Count) {
                func.CurrentOverloadIndex = 0;
            }
            m_calltipFuncion.update();

            CallTip.Show(m_calltipFuncion.CalltipString, m_calltipFuncion.HighLightStart, m_calltipFuncion.HighLightEnd);

        }

        private void intellua_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            ShowCalltip();
            const string brackets = "()[]{}";
            const string newline = "\r\n";
            if (newline.Contains(e.Ch))
            {
                if (e.Ch == '\n')
                {
                    InsertText(string.Concat(Enumerable.Repeat("\t", Lines.Current.Previous.Indentation / Indentation.TabWidth)));
                    
                    if (Lines.Current.FoldParent != null && Lines.Current.FoldParent.StartPosition == Lines.Current.Previous.StartPosition)
                    {
                        InsertText("\t");
                    }
                }
                
                return;
            }
            if (brackets.Contains(e.Ch)) return;

            MemberChain chain = MemberChain.ParseBackward(this);
            if (chain.Elements.Count == 1) {
                string word = chain.Elements[0].Name;
                if (char.IsLetterOrDigit(e.Ch) && word.Length >= 3)
                {
                    List<IAutoCompleteItem> list = m_autoCompleteData.Variables.getList(word);
                    if (list.Count > 0)
                    {
                        ShowAutoComplete(word.Length, list);
                    }
                }
            }
            else
            {
                Type t = chain.getType(m_autoCompleteData.Variables);
                if (t!=null) {
                    List<IAutoCompleteItem> list = t.getList(chain.IsNamespace);
                    if (list.Count > 0)
                    {
                        ShowAutoComplete(chain.getLastElement().Length, list);
                    }
                }
            }

            if (!AutoComplete.IsActive) {
                m_tooltip.Hide();
            }
            
            
        }

        private void intellua_TextDeleted(object sender, ScintillaNET.TextModifiedEventArgs e)
        {
            parseFile(getDecodedPos(Lines.Current.StartPosition));
        }

        private void intellua_TextInserted(object sender, ScintillaNET.TextModifiedEventArgs e)
        {
            parseFile(getDecodedPos(Lines.Current.StartPosition));
        }

        private void ShowAutoComplete(int lengthEntered, List<IAutoCompleteItem> list)
        {
            m_autocompleteList = list;
            List<string> str = new List<string>();
            foreach (IAutoCompleteItem item in list) {
                str.Add(item.getACString());
            }
            AutoComplete.Show(lengthEntered, str);

            if (AutoComplete.SelectedIndex < 0) return;
            IntPtr hwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "ListBoxX", null);

            if (hwnd != IntPtr.Zero)
            {
                RECT rect;
                GetWindowRect(hwnd, out rect);

                m_tooltip.ShowToolTip(rect.Right, rect.Top, m_autocompleteList[AutoComplete.SelectedIndex].getToolTipString());
                
            }
        }

        private void ShowCalltip()
        {
            FunctionCall fc = FunctionCall.Parse(this, m_autoCompleteData.Variables, getDecodedPos() - 1);
            if (fc != null)
            {
                m_calltipFuncion = fc;
                CallTip.Show(fc.CalltipString, fc.HighLightStart, fc.HighLightEnd);
            }
            else
            {
                CallTip.Hide();
            }
        }

        
		#endregion Methods 


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
		#region Data Members (4) 

       // x position of lower-right corner
            public int Bottom;
            public int Left;
         // y position of upper-left corner
            public int Right;
        // x position of upper-left corner
            public int Top;

		#endregion Data Members 

      // y position of lower-right corner
        }
    }
}
