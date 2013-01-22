using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
namespace LuaEditor
{
    public partial class Form1 : Form
    {

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
            Type Number = new Type("Number");
            m_types.add(Number);

            Type Vector = new Type("Vector");
            Vector.addMember("x",Number);
            Vector.addMember("y",Number);
            Function length = new Function("Length");
            length.ReturnType = Number;
            Vector.addMethod(length);
            Function clone = new Function("clone");
            clone.ReturnType = Vector;
            Vector.addMethod(clone);
            m_types.add(Vector);

            Type Rect = new Type("Rect");
            Rect.addMember("TopLeft", Vector);
            Rect.addMember("BottomRight", Vector);
            m_types.add(Rect);

            Variable var = new Variable("helloworld");
            var.IsStatic = true;
            var.Type = m_types.get("Rect");
            m_variables.add(var);
            var = new Variable("hello");
            var.IsStatic = true;
            var.Type = m_types.get("Rect");
            m_variables.add(var);

            Function f = new Function("Vector");
            f.ReturnType = Vector;
            m_variables.add(f);
            

            
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
                for (int i = chain.EndPos+1; i < str.Length; i++) {
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
                    return;
                }
                else if (word.Length >= 3)
                {
                    List<string> list = m_variables.getList(word);
                    if (list.Count > 0)
                    {
                        scintilla1.AutoComplete.Show(word.Length, list);
                    }
                    return;
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
    }
}
