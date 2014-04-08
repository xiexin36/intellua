using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
namespace Intellua
{
    class FileParser
    {

        static FileParser() {
            s_extensions.Add("");
            //s_extensions.Add(".lua");
            try {
                XmlTextReader reader = new XmlTextReader(AppDomain.CurrentDomain.BaseDirectory + "intelluaconfig.xml");
                while (reader.Read()) {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "path") {
                            reader.Read();
                            s_includePaths.Add(reader.Value);
                        }
                        else if (reader.Name == "ext") {
                            reader.Read();
                            s_extensions.Add(reader.Value);
                        }
                    }
                }
            }catch{
            
            }

        }
        static Dictionary<string, FileParser> s_files = new Dictionary<string, FileParser>();
        static FileParser getFile(string filename, Intellua parent, Dictionary<string, int> required)
        {
          /*  if (s_files.ContainsKey(filename)) { 
                //todo: check file changes

                return s_files[filename];
            }*/
            IntelluaSource source = new IntelluaSource(filename,parent);
            FileParser fp = new FileParser(source);
            foreach (var kv in required) {
                fp.m_required[kv.Key] = kv.Value;
            }
            
            fp.parse(true);
            s_files[filename] = fp;
            return fp;
        }

        public AutoCompleteData result;
        IntelluaSource m_source;
        public FileParser(IntelluaSource source) {
            m_source = source;
            result = new AutoCompleteData();
            result.setParent(source.m_intellua.AutoCompleteData.getParent());
            result.Variables.scope = source.m_intellua.parseScope(0, source.m_intellua.Lines.Count - 1);
        }

        static Regex s_requreReg = new Regex(@"(?<![\.:]\s*)(?:\s+|^)require\s*\(\s*\""(.*)\""\s*\)", RegexOptions.Compiled);

        static List<string> s_extensions = new List<string>();

        void addExtention(List<string> paths, string fn) {
            List<string> extensions = new List<string>();
            
            foreach (string str in s_extensions)
            {
                paths.Add(fn + str);
            }
        }
        static List<string> s_includePaths = new List<string>();

        Dictionary<string, int> m_required = new Dictionary<string,int>();
        string getFilename(string filename, string filebasepath) {
            string exebasepath = AppDomain.CurrentDomain.BaseDirectory;
            
            List<string> paths = new List<string>();

            if (filebasepath != null) {
                addExtention(paths, filebasepath + filename);
            }

            addExtention(paths, exebasepath + filename);

            foreach (string str in s_includePaths) {
                addExtention(paths,exebasepath + str + filename);
            }

            addExtention(paths, filename);

            foreach(string str in paths){
                if (System.IO.File.Exists(str)) { 
                    System.IO.FileInfo fi = new System.IO.FileInfo(str);
                    return fi.FullName;
                }
            }
            return null;

        }
        private void parseRequire()
        {
            MatchCollection rst = s_requreReg.Matches(m_source.text);
            string filebasepath = null;
            if (m_source.FilePath.Length!=0)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(m_source.FilePath);
                filebasepath = fi.DirectoryName;
            }
           

            foreach (Match m in rst)
            {
                string fn = getFilename(m.Groups[1].Value,filebasepath);
                if (fn != null) {
                    if (m_required.ContainsKey(fn)) {
                        continue;
                    }
                    System.Diagnostics.Debug.Print("require " + fn);
                    m_required[fn] = 0;

                    FileParser fp = getFile(fn,m_source.m_intellua,m_required);
                    result.Requires.Add(fp.result);
                }
            }
        }
        void parse(bool importMode) {
            parseRequire();
            parseDeclaration();
            //if (importMode) return;
            parseVariables();
               
            
        }
        void parseDeclaration() { 
            
        }
        void parseVariables (){
            int pos = 0;
            Byte[] str = m_source.RawText;


            for (; pos < str.Length; pos++)
            {
                char c = Convert.ToChar(str[pos]);

                //search for assignment operator

                if (!Parser.isCode(m_source, pos))
                {
                    continue;
                }

                if (c != '=') continue;
                if (pos > 0)
                {
                    if (Convert.ToChar(str[pos - 1]) == '=') continue;
                }
                if (pos < str.Length - 1)
                {
                    if (Convert.ToChar(str[pos + 1]) == '=') continue;
                }
                MemberChain v = MemberChain.ParseBackward(m_source, pos - 1);
                if (v.Elements.Count > 1 || v.Elements.Count == 0) continue;

                string varName = v.getLastElement();
                Variable var = result.Variables.getVariable(varName);
                //                if (var != null) continue;

                MemberChain elem = MemberChain.ParseFoward(m_source, pos + 1);
                if (elem == null) continue;
                Type t = elem.getType(result);
                if (t == null) continue;

                //System.Diagnostics.Debug.Print(varName + " added");

                var = new Variable(varName);
                var.IsStatic = false;
                var.Type = t;
                var.StartPos = v.StartPos;
                var.EndPos = elem.EndPos;
                result.Variables.add(var);
            }
        }
        public void doWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            parse(false);
 
            e.Result = result;

        }
        
    }
}
