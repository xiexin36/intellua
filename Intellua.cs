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
        private TypeManager m_types;
        private VariableManager m_variables;

		#endregion Fields 

		#region Constructors (1) 

        public Intellua()
        {
            this.AutoCompleteAccepted += new System.EventHandler<ScintillaNET.AutoCompleteAcceptedEventArgs>(this.intellua_AutoCompleteAccepted);
            this.AutoCompleteCancelled += new System.EventHandler<ScintillaNET.NativeScintillaEventArgs>(this.intellua_AutoCompleteCancelled);
            this.AutoCompleteMoved += new System.EventHandler<ScintillaNET.NativeScintillaEventArgs>(this.intellua_AutoCompleteMoved);
            this.CallTipClick += new System.EventHandler<ScintillaNET.CallTipClickEventArgs>(this.intellua_CallTipClick);
            this.CharAdded += new System.EventHandler<ScintillaNET.CharAddedEventArgs>(this.intellua_CharAdded);
            this.TextDeleted += new System.EventHandler<ScintillaNET.TextModifiedEventArgs>(this.intellua_TextDeleted);
            this.TextInserted += new System.EventHandler<ScintillaNET.TextModifiedEventArgs>(this.intellua_TextInserted);

            

            m_tooltip = new ToolTip(this);
            m_types = new TypeManager();
            m_variables = new VariableManager();
            ScintillaNET.Configuration.Configuration config =
                new ScintillaNET.Configuration.Configuration(Assembly.GetExecutingAssembly().GetManifestResourceStream("Intellua.ScintillaNET.xml"),
                    "lua", true);
            ConfigurationManager.Language = "lua";
            ConfigurationManager.Configure(config);
            Folding.IsEnabled = true;
            Margins[0].Width = 20;
            Margins[1].Width = 20;
            Margins[2].Width = 20;
            AutoComplete.IsCaseSensitive = false;
            AutoComplete.AutoHide = false;
            Indentation.ShowGuides = true;


            m_types.add(new Type("int"));
            m_types.add(new Type("void"));
            m_types.add(new Type("char"));
            m_types.add(new Type("float"));
            m_types.add(new Type("double"));
            //m_types.add(new Type("string"));
            //m_types.add(new Type("table"));
            m_types.add(new Type("number"));
            m_types.add(new Type("boolean"));
            m_types.add(new Type("function"));
            m_types.add(new Type("thread"));
            m_types.add(new Type("userdata"));
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

		#endregion Constructors 

		#region Methods (14) 

		// Public Methods (3) 

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd,
           StringBuilder lpClassName,
           int nMaxCount
        );

        public void LoadDoxygenXML(string filename) {
            DoxygenXMLParser.Parse(filename, m_variables, m_types);
        }

        public void parseFile(int pos) {
            string str = Text;
            m_variables.purge(pos);
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
                if(v.Elements.Count > 1) continue;
                string varName = v.getLastElement();
                Variable var = m_variables.getVariable(varName);
                if (var != null) continue;

                MemberChain e = MemberChain.ParseFoward(this,pos+1);
                if(e == null) continue;
                Type t = e.getType(m_variables);
                if (t == null) continue;

                //System.Diagnostics.Debug.Print(varName + " added");

                var = new Variable(varName);
                var.IsStatic = false;
                var.Type = t;
                var.StartPos = v.StartPos;
                var.EndPos = e.EndPos;
                m_variables.add(var);
                
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
                    List<IAutoCompleteItem> list = m_variables.getList(word);
                    if (list.Count > 0)
                    {
                        ShowAutoComplete(word.Length, list);
                    }
                }
            }
            else
            {
                Type t = chain.getType(m_variables);
                if (t!=null) {
                    List<IAutoCompleteItem> list = t.getList();
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
            parseFile(Lines.Current.StartPosition);
        }

        private void intellua_TextInserted(object sender, ScintillaNET.TextModifiedEventArgs e)
        {
            parseFile(Lines.Current.StartPosition);
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
            FunctionCall fc = FunctionCall.Parse(this, m_variables, CurrentPos - 1);
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
