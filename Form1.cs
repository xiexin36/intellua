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
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            scintilla1.ConfigurationManager.CustomLocation = "ScintillaNET.xml";
            scintilla1.ConfigurationManager.Language = "lua";
            scintilla1.ConfigurationManager.Configure();
            scintilla1.Folding.IsEnabled = true;
            scintilla1.Margins[0].Width = 20;
            scintilla1.Margins[1].Width = 20;
            scintilla1.Margins[2].Width = 20;
        }

        private void scintilla1_CharAdded(object sender, ScintillaNET.CharAddedEventArgs e)
        {
            if (e.Ch == '.')
            {
                int start = scintilla1.Lines.Current.StartPosition;
                int end = scintilla1.CurrentPos;
                string str = scintilla1.Text.Substring(start, end - start - 1);
                Regex rex = new Regex(@"(?:.*?)(?<word>[\w.:\(\)]+$)");
                Match m = rex.Match(str);
                if (m.Success)
                {
                    string lw = m.Groups["word"].Value;
                    scintilla1.CallTip.Show(lw);
                }
            }
        }

        private void parseLine(string line) {
            /*const string expr = @"(?<var>\w+)\s*=\s*(?<exp>.+)((?=\W$)|\z)";
            Regex rex = new Regex(expr);
            Match m = rex.Match(line);
            if(m.Success){
                string var = m.Groups["var"].Value;
                string exp = m.Groups["exp"].Value;
                 
                scintilla1.CallTip.Show(var + " = " + exp);
                System.Diagnostics.Debug.Print(var + " = " + exp);
            }*/
        }

        private void scintilla1_TextDeleted(object sender, ScintillaNET.TextModifiedEventArgs e)
        {
            parseLine(scintilla1.Lines.Current.Text);
        }

        private void scintilla1_TextInserted(object sender, ScintillaNET.TextModifiedEventArgs e)
        {
            parseLine(scintilla1.Lines.Current.Text);
        }
    }
}
