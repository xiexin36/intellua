using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
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

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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

            loadXML();


            timer1.Start();
            
        }
        private void loadXML() {
            XDocument doc = XDocument.Load("all.xml");

            //scan all classes first.
            foreach(XElement node in doc.Descendants("compounddef")){
                if (node.Attribute("kind").Value == "class") {
                    string name = node.Element("compoundname").Value;
                    Type t = new Type(name);
                    System.Diagnostics.Debug.Print("Type added: " + name);
                    m_types.add(t);
                }
            }
            foreach (XElement node in doc.Descendants("compounddef"))
            {
                if (node.Attribute("kind").Value == "class")
                {
                    string name = node.Element("compoundname").Value;
                    Type t = m_types.get(name);

                    foreach (XElement member in node.Descendants("memberdef"))
                    {
                        if (member.Attribute("kind").Value == "variable") {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;

                            Type mt = m_types.get(memberType);
                            t.addMember(memberName, mt);
                            System.Diagnostics.Debug.Print("Member added: " + memberType + " " + name + ":" + memberName);
                        }
                        else if (member.Attribute("kind").Value == "function") {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;

                            if (memberName == name)
                            {
                                Function f = new Function(memberName);
                                f.ReturnType = t;
                                m_variables.add(f);
                                System.Diagnostics.Debug.Print("Constructor added: " + name + ":" + f.ToString());
                            }
                            else
                            {

                                Type mt = m_types.get(memberType);
                                Function f = new Function(memberName);
                                f.ReturnType = mt;

                                t.addMethod(f);
                                System.Diagnostics.Debug.Print("Method added: " + memberType + " " + name + ":" + f.ToString());
                            }
                        }
                    }


                }
                else if (node.Attribute("kind").Value == "file") {
                    foreach (XElement member in node.Descendants("memberdef"))
                    {
                        if (member.Attribute("kind").Value == "variable")
                        {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;

                            Type mt = m_types.get(memberType);
                            Variable var = new Variable(memberName);
                            var.Type = mt;
                            var.IsStatic = true;
                            m_variables.add(var);
                            System.Diagnostics.Debug.Print("Static variable added: " + memberType + " " + memberName);
                        }
                        else if (member.Attribute("kind").Value == "function")
                        {
                            string memberName = member.Element("name").Value;
                            string memberType = member.Element("type").Value;

                            Type mt = m_types.get(memberType);
                            Function f = new Function(memberName);
                            f.ReturnType = mt;
                            m_variables.add(f);
                            System.Diagnostics.Debug.Print("Global function added: " + memberType + " " + f.ToString());
                        }
                    }
                }
            }
            /*foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Attributes["kind"].InnerText == "class")
                {
                    string name = node["compoundname"].InnerText;
                    Type t = m_types.get(name);

                    foreach (XmlElement in node.)
                    {
                        if (member.Name != "member") continue;
                        if (member.Attributes["kind"].InnerText == "variable") { 
                            
                        }
                    }

                    System.Diagnostics.Debug.Print("Type added: " + name);
                    m_types.add(t);
                }
            }*/

        }

        private void scintilla1_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            
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
                    if (Parser.isString(scintilla1, i) || Parser.isComment(scintilla1, i)) continue;
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
                            List<string> list = t.getList();
                            if (list.Count > 0)
                            {
                                scintilla1.AutoComplete.Show(0, list);
                            }
                        }
                    }
                    return;
                }
                else if (e.Ch == '.' || e.Ch == ':') {
                    Variable var = m_variables.getVariable(word);
                    if (var != null) {
                        List<string> list = var.Type.getList();
                        if (list.Count > 0)
                        {
                            scintilla1.AutoComplete.Show(0, list);
                            
                        }
                    }
                }
                else if (char.IsLetterOrDigit(e.Ch) &&word.Length >= 3)
                {
                    List<string> list = m_variables.getList(word);
                    if (list.Count > 0)
                    {
                        scintilla1.AutoComplete.Show(word.Length, list);
                    }
                }
            }
            else {
                Type t = chain.getType(m_variables);
                if (t!=null) {
                    List<string> list = t.getList();
                    if (list.Count > 0)
                    {
                        scintilla1.AutoComplete.Show(chain.getLastElement().Length, list);
                    }
                }
            }

            IntPtr hwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "ListBoxX", null);


            if (hwnd != IntPtr.Zero)
            {
                RECT rect;
                GetWindowRect(hwnd, out rect);
                System.Diagnostics.Debug.Print(rect.Left + "," + rect.Top);

                ToolTip tooltip = new ToolTip(rect.Right, rect.Top, 100, 32, "helloworld", 10);
                tooltip.Show();
            }
            
        }

        private void parseFile(int pos) {
            string str = scintilla1.Text;
            m_variables.purge(pos);
            for (; pos< str.Length; pos++) {
                char c = str[pos];

                //search for assignment operator

                if(Parser.isComment(scintilla1,pos) || Parser.isString(scintilla1,pos)){
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
            
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

           
           
            timer1.Start();
        }
    }
}
