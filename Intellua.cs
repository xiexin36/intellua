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
    public class StatusChangedEventArgs {
        public string Text;
    };
    public class Intellua : ScintillaNET.Scintilla, System.ComponentModel.ISupportInitialize
    {
        #region Fields (5)

        private AutoCompleteData m_autoCompleteData;
        private List<IAutoCompleteItem> m_autocompleteList;
        private FunctionCall m_calltipFuncion;
        private string m_filePath = "";
        private bool m_parse = true;
        private bool m_parsePending = false;
        private IntelluaSource m_source;
        private ToolTip m_tooltip;
        private System.ComponentModel.BackgroundWorker m_worker;

        public delegate void StatusChangedHandler(object sender, StatusChangedEventArgs e);
        public event StatusChangedHandler StatusChanged;
        public Intellua()
        {
            

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
            str = asm.GetManifestResourceStream("Intellua.type.png");
            list.Add(new Bitmap(str));
            AutoComplete.RegisterImages(list);

            m_source = new IntelluaSource(this);
        }
        public new void EndInit()
        {
            base.EndInit();
            Styles[ScintillaNET.StylesCommon.BraceBad].ForeColor = System.Drawing.Color.Red;
            Styles[ScintillaNET.StylesCommon.BraceLight].ForeColor = System.Drawing.Color.Magenta;

            Styles[21].ForeColor = System.Drawing.Color.DarkMagenta;
            Styles[22].ForeColor = System.Drawing.Color.DarkMagenta;
            AutoComplete.AutoHide = false;
            Lexing.Colorize();
            
        }
        public AutoCompleteData AutoCompleteData
        {
            get
            {
                return m_autoCompleteData;
            }
        }
        #endregion Fields

        #region Constructors (1)

        public string FilePath
        {
            get { return m_filePath; }
            set { m_filePath = value; }
        }

        public bool Parse
        {
            get { return m_parse; }
            set { m_parse = value; }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd,
           StringBuilder lpClassName,
           int nMaxCount
        );

        public int getDecodedPos()
        {
            return Encoding.GetCharCount(RawText, 0, CurrentPos + 1) - 1;
        }

        public int getDecodedPos(int bytePos)
        {
            return Encoding.GetCharCount(RawText, 0, bytePos + 1) - 1;
        }

        // Public Methods (3) 
        

        public void queueParseFile()
        {
            if (!Parse) return;
            if (m_parsePending) return;

            if (m_worker != null && m_worker.IsBusy)
            {
                m_parsePending = true;
                return;
            }
            parseFile();
        }

        public void setParent(AutoCompleteData parent)
        {
            m_autoCompleteData.setParent(parent);
        }
        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        // Private Methods (11) 
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private void autoCompleteHided() {
            if(m_tooltip!=null)m_tooltip.Hide();
        }
        private void autoCompleteIndexChanged() {
            if (AutoComplete.SelectedIndex == -1) return;
            m_tooltip.setText(m_autocompleteList[AutoComplete.SelectedIndex].getToolTipString());
        }
        private void intellua_CallTipClick(object sender, ScintillaNET.CallTipClickEventArgs e)
        {
            Function func = m_calltipFuncion.Func;
            func.CurrentOverloadIndex++;
            if (func.CurrentOverloadIndex == func.Param.Count)
            {
                func.CurrentOverloadIndex = 0;
            }
            m_calltipFuncion.update();

            CallTip.Show(m_calltipFuncion.CalltipString, m_calltipFuncion.HighLightStart, m_calltipFuncion.HighLightEnd);
        }

        private void intellua_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            //ShowCalltip();
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

            if (!Parse) return;
            if (brackets.Contains(e.Ch)) return;

            MemberChain chain = MemberChain.ParseBackward(m_source);
            if (chain.Elements.Count == 1)
            {
                string word = chain.Elements[0].Name;
                if (char.IsLetterOrDigit(e.Ch) && word.Length >= 3)
                {
                    List<IAutoCompleteItem> list = m_autoCompleteData.Variables.getList(word,CurrentPos);
                    m_autoCompleteData.Types.appendList(list, word);
                    m_autoCompleteData.Keywords.appendList(list, word);

                    list.Sort();
                    if (list.Count > 0)
                    {
                        ShowAutoComplete(word.Length, list);
                    }
                }
            }
            else
            {
                Type t = chain.getType(m_autoCompleteData);
                if (t != null)
                {
                    List<IAutoCompleteItem> list = t.getList(chain.IsNamespace);
                    if (list.Count > 0)
                    {
                        ShowAutoComplete(chain.getLastElement().Length, list);
                    }
                }
            }

            if (!AutoComplete.IsActive)
            {
                m_tooltip.Hide();
            }
        }

        private void Intellua_SelectionChanged(object sender, EventArgs e)
        {
            if (!Parse) return;
            ShowCalltip();
            const string lbracket = "([{";
            const string rbracket = ")]}";
            int pos = CurrentPos;
            int style = Styles.GetStyleAt(pos - 1);
            int start, end;
            start = end = -1;

            Byte[] str = RawText;

            Stack<char> stk = new Stack<char>();

            for (int p = pos - 1; p >= 0; p--)
            {
                // if (Styles.GetStyleAt(p) != style) continue;
                if (p >= str.Length) continue;
                if (str[p] > 127) continue;
                char c = Convert.ToChar(str[p]);
                if (rbracket.Contains(c))
                {
                    stk.Push(c);
                }
                if (lbracket.Contains(c))
                {
                    if (stk.Count == 0)
                    {
                        start = p;
                        break;
                    }
                    char pc = stk.Pop();
                    if ((pc == ')' && c != '(') ||
                        (pc == ']' && c != '[') ||
                        (pc == '}' && c != '{'))
                    {
                        break;
                    }
                }
            }
            stk.Clear();

            for (int p = pos; p < str.Length; p++)
            {
                // if (Styles.GetStyleAt(p) != style) continue;
                if (str[p] > 127) continue;
                char c = Convert.ToChar(str[p]);
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
                char c = Convert.ToChar(str[start]);
                char pc = Convert.ToChar(str[end]);

                if ((pc != ')' && c == '(') ||
                        (pc != ']' && c == '[') ||
                        (pc != '}' && c == '{'))
                {
                    start = -1;
                }
            }

            if (start != -1)
            {
                //start = Encoding.GetByteCount(Text.ToCharArray(), 0, start+1)-1;
            }
            if (end != -1)
            {
                //end = Encoding.GetByteCount(Text.ToCharArray(), 0, end + 1) - 1;
            }

            if (start == -1)
            {
                if (end != -1)
                {
                    NativeInterface.BraceBadLight(end);
                }
                else
                {
                    NativeInterface.BraceHighlight(-1, -1);
                }
            }
            else if (end == -1)
            {
                if (start != -1)
                {
                    NativeInterface.BraceBadLight(start);
                }
                else
                {
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
        private void intellua_TextDeleted(object sender, ScintillaNET.TextModifiedEventArgs e)
        {
            queueParseFile();
        }

        private void intellua_TextInserted(object sender, ScintillaNET.TextModifiedEventArgs e)
        {
            queueParseFile();
        }

        private void parseFile()
        {
            IntelluaSource source = new IntelluaSource(this, true);
            m_worker = new System.ComponentModel.BackgroundWorker();
            FileParser fp = new FileParser(source);
            /*fp.doWork(this,new System.ComponentModel.DoWorkEventArgs(0));
            m_autoCompleteData = fp.result;*/
            m_worker.DoWork += new System.ComponentModel.DoWorkEventHandler(fp.doWork);
            m_worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(parseFileDone);
            m_worker.RunWorkerAsync();
        }

        private void parseFileDone(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            FileParserResult rst = e.Result as FileParserResult;
            m_autoCompleteData = rst.result;
            setStatus(rst.msg);
            if (m_parsePending)
            {
                m_parsePending = false;
                parseFile();
            }
        }
        private void ShowAutoComplete(int lengthEntered, List<IAutoCompleteItem> list)
        {
            m_autocompleteList = list;
            
            List<string> str = new List<string>();
            foreach (IAutoCompleteItem item in list)
            {
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
            FunctionCall fc = FunctionCall.Parse(m_source, m_autoCompleteData, CurrentPos);
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
        protected override void WndProc(ref System.Windows.Forms.Message m) {
            //
            if (AutoComplete != null )
            {
                int acItem = AutoComplete.SelectedIndex;
                base.WndProc(ref m);
                if (!AutoComplete.IsActive)
                {
                    autoCompleteHided();
                }
                else
                {
                    if (acItem != AutoComplete.SelectedIndex)
                    {
                        autoCompleteIndexChanged();
                    }
                }
            }
            else {
                base.WndProc(ref m);
            }
        }

        public void setStatus(string text) {
            if (StatusChanged != null) { 
                StatusChangedEventArgs e= new StatusChangedEventArgs();
                e.Text=text;
                StatusChanged(this, e);
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