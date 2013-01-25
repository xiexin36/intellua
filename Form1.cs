using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using System.IO;
using System.Reflection;
				
using System.Runtime.InteropServices;


namespace LuaEditor
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd,
           StringBuilder lpClassName,
           int nMaxCount
        );
        private TypeManager m_types;
        private VariableManager m_variables;
        private ToolTip m_tooltip;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_tooltip = new ToolTip(this);
            m_types = new TypeManager();
            m_variables = new VariableManager();
            scintilla1.ConfigurationManager.CustomLocation = "ScintillaNET.xml";
            scintilla1.ConfigurationManager.Language = "lua";
            scintilla1.ConfigurationManager.Configure();
            scintilla1.Folding.IsEnabled = true;
            scintilla1.Margins[0].Width = 20;
            scintilla1.Margins[1].Width = 20;
            scintilla1.Margins[2].Width = 20;
            scintilla1.AutoComplete.IsCaseSensitive = false;
            scintilla1.AutoComplete.AutoHide = false;

            

            m_types.add(new Type("int"));
            m_types.add(new Type("void"));
            m_types.add(new Type("char"));
            m_types.add(new Type("float"));
            m_types.add(new Type("double"));
            m_types.add(new Type("string"));

            List<Bitmap> list = new List<Bitmap>();
            Assembly asm = Assembly.GetExecutingAssembly();
            Stream str;
            str = asm.GetManifestResourceStream("LuaEditor.member.png");
            list.Add(new Bitmap(str));
            str = asm.GetManifestResourceStream("LuaEditor.method.png");
            list.Add(new Bitmap(str));
            str = asm.GetManifestResourceStream("LuaEditor.function.png");
            list.Add(new Bitmap(str));

            scintilla1.AutoComplete.RegisterImages(list);

            
            DoxygenXMLParser.Parse("all.xml",m_variables,m_types);

            
        }
       

        private void scintilla1_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            ShowCalltip();

            


            const string newline = "\r\n";
            if (newline.Contains(e.Ch)) return;
            Chain chain = Chain.ParseBackward(scintilla1);
            if (chain.Elements.Count == 1)
            {
                string word = chain.Elements[0];

                bool isFunction = false;
                string str=  scintilla1.Text;
                for (int i = chain.EndPos+1; i < scintilla1.CurrentPos; i++) {
                    char c = str[i];
                    if (!Parser.isCode(scintilla1, i)) continue;
                    if (char.IsWhiteSpace(c)) continue;
                    if (c == '(')
                    {
                        isFunction = true;
                    }
                    break;
                }
                if (isFunction) {
                    if (e.Ch != '.' && e.Ch != ':') return;
                    Function func = m_variables.getFunction(word);
                    if (func != null) {
                        Type t = func.ReturnType;
                        if (t != null)
                        {
                            List<IAutoCompleteItem> list = t.getList();
                            if (list.Count > 0)
                            {
                                ShowAutoComplete(0, list);
                            }
                        }
                    }
                    return;
                }
                else if (e.Ch == '.' || e.Ch == ':') {
                    Variable var = m_variables.getVariable(word);
                    if (var != null) {
                        List<IAutoCompleteItem> list = var.Type.getList();
                        if (list.Count > 0)
                        {
                            ShowAutoComplete(0, list);
                            
                        }
                    }
                }
                else if (char.IsLetterOrDigit(e.Ch) &&word.Length >= 3)
                {
                    List<IAutoCompleteItem> list = m_variables.getList(word);
                    if (list.Count > 0)
                    {
                        ShowAutoComplete(word.Length, list);
                    }
                }
            }
            else {
                Type t = chain.getType(m_variables);
                if (t!=null) {
                    List<IAutoCompleteItem> list = t.getList();
                    if (list.Count > 0)
                    {
                        ShowAutoComplete(chain.getLastElement().Length, list);
                    }
                }
            }

            if (!scintilla1.AutoComplete.IsActive) {
                m_tooltip.Hide();
            }
            
            
        }
        private FunctionCall m_calltipFuncion;
        private void ShowCalltip()
        {
            FunctionCall fc = FunctionCall.Parse(scintilla1, m_variables, scintilla1.CurrentPos - 1);
            if (fc != null)
            {
                m_calltipFuncion = fc;
                scintilla1.CallTip.Show(fc.CalltipString, fc.HighLightStart, fc.HighLightEnd);
            }
            else
            {
                scintilla1.CallTip.Hide();
            }
        }
        private List<IAutoCompleteItem> m_autocompleteList;
        private void ShowAutoComplete(int lengthEntered, List<IAutoCompleteItem> list)
        {
            m_autocompleteList = list;
            List<string> str = new List<string>();
            foreach (IAutoCompleteItem item in list) {
                str.Add(item.getACString());
            }
            scintilla1.AutoComplete.Show(lengthEntered, str);

            if (scintilla1.AutoComplete.SelectedIndex < 0) return;
            IntPtr hwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "ListBoxX", null);

            if (hwnd != IntPtr.Zero)
            {
                RECT rect;
                GetWindowRect(hwnd, out rect);

                m_tooltip.ShowToolTip(rect.Right, rect.Top, m_autocompleteList[scintilla1.AutoComplete.SelectedIndex].getToolTipString());
                
            }
        }

        private void parseFile(int pos) {
            string str = scintilla1.Text;
            m_variables.purge(pos);
            for (; pos< str.Length; pos++) {
                char c = str[pos];

                //search for assignment operator

                if(!Parser.isCode(scintilla1,pos)){
                    continue;
                }

                if(c != '=') continue;
                if(pos>0){
                    if(str[pos-1] == '=') continue;
                }
                if(pos < str.Length -1){
                    if(str[pos+1] == '=') continue;
                }
                Chain v = Chain.ParseBackward(scintilla1,pos-1);
                if(v.Elements.Count > 1) continue;
                string varName = v.getLastElement();
                Variable var = m_variables.getVariable(varName);
                if (var != null) continue;

                Chain e = Chain.ParseFoward(scintilla1,pos+1);
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

        private void scintilla1_TextDeleted(object sender, ScintillaNET.TextModifiedEventArgs e)
        {
            parseFile(scintilla1.Lines.Current.StartPosition);
        }

        private void scintilla1_TextInserted(object sender, ScintillaNET.TextModifiedEventArgs e)
        {
            parseFile(scintilla1.Lines.Current.StartPosition);
        }

        private void scintilla1_AutoCompleteAccepted(object sender, ScintillaNET.AutoCompleteAcceptedEventArgs e)
        {
            m_tooltip.Hide();
            
        }

        private void scintilla1_AutoCompleteCancelled(object sender, EventArgs e)
        {
            m_tooltip.Hide();
        }

        private void scintilla1_KeyDown(object sender, KeyEventArgs e)
        {
           
        }

        private void scintilla1_AutoCompleteMoved(object sender, ScintillaNET.NativeScintillaEventArgs e)
        {
            m_tooltip.setText(m_autocompleteList[scintilla1.AutoComplete.SelectedIndex].getToolTipString());
        }

        private void scintilla1_CallTipClick(object sender, ScintillaNET.CallTipClickEventArgs e)
        {
            Function func = m_calltipFuncion.Func;
            func.CurrentOverloadIndex++;
            if (func.CurrentOverloadIndex == func.Param.Count) {
                func.CurrentOverloadIndex = 0;
            }
            m_calltipFuncion.update();

            scintilla1.CallTip.Show(m_calltipFuncion.CalltipString, m_calltipFuncion.HighLightStart, m_calltipFuncion.HighLightEnd);

        }


    }
}
