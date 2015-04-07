using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace Intellua
{
    internal class FileParserResult {
        public AutoCompleteData result;
        public string msg;
    }
    internal class FileParser
    {
        public AutoCompleteData result;
        public string msg = "";
        private static List<string> s_extensions = new List<string>();

        private static Dictionary<string, FileParser> s_files = new Dictionary<string, FileParser>();

        private static List<string> s_includePaths = new List<string>();

        private static Regex s_requreReg = new Regex(@"(?<![\.:]\s*)(?:\s+|^)require\s*\(\s*\""(.*)\""\s*\)", RegexOptions.Compiled);

        private System.DateTime m_lastCheckTime;

        private System.DateTime m_lastWriteTime;

        private Dictionary<string, int> m_required = new Dictionary<string, int>();

        private IntelluaSource m_source;

        static FileParser()
        {
            s_extensions.Add("");
            //s_extensions.Add(".lua");
            try
            {
                XmlTextReader reader = new XmlTextReader(AppDomain.CurrentDomain.BaseDirectory + "intelluaconfig.xml");
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "path")
                        {
                            reader.Read();
                            s_includePaths.Add(reader.Value);
                        }
                        else if (reader.Name == "ext")
                        {
                            reader.Read();
                            s_extensions.Add(reader.Value);
                        }
                    }
                }
            }
            catch
            {
            }
        }
        public FileParser(IntelluaSource source)
        {
            m_source = source;
            result = new AutoCompleteData();
            result.setParent(source.m_intellua.AutoCompleteData.getParent());
            //result.Variables.scope = source.m_intellua.parseScope(0, source.m_intellua.Lines.Count - 1);

            m_lastCheckTime = System.DateTime.Now;
            if (source.FilePath.Length != 0)
            {
                m_lastWriteTime = System.IO.File.GetLastWriteTime(m_source.FilePath);
            }
            m_required[source.FilePath] = 0;
        }

        public void doWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            parse(false);
            FileParserResult rst = new FileParserResult();
            rst.result = result;
            rst.msg = msg;

            e.Result = rst;
        }

        private static FileParser getFile(string filename, Intellua parent, Dictionary<string, int> required)
        {
            if (s_files.ContainsKey(filename))
            {
                FileParser afp = s_files[filename];
                if (!afp.modified())
                {
                    return afp;
                }
                else
                {
                    System.Diagnostics.Debug.Print("modified");
                }
            }
            System.Diagnostics.Debug.Print("parse file " + filename);
            IntelluaSource source = new IntelluaSource(filename, parent);
            FileParser fp = new FileParser(source);
            foreach (var kv in required)
            {
                fp.m_required[kv.Key] = kv.Value;
            }

            fp.parse(true);
            s_files[filename] = fp;
            return fp;
        }
        private void addExtention(List<string> paths, string fn)
        {
            List<string> extensions = new List<string>();

            foreach (string str in s_extensions)
            {
                paths.Add(fn + str);
            }
        }
        private string getFilename(string filename, string filebasepath)
        {
            string exebasepath = AppDomain.CurrentDomain.BaseDirectory;

            List<string> paths = new List<string>();

            if (filebasepath != null)
            {
                addExtention(paths, filebasepath + filename);
            }

            addExtention(paths, exebasepath + filename);

            foreach (string str in s_includePaths)
            {
                addExtention(paths, exebasepath + str + filename);
            }

            addExtention(paths, filename);

            foreach (string str in paths)
            {
                if (System.IO.File.Exists(str))
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(str);
                    return fi.FullName;
                }
            }
            return null;
        }

        private bool modified()
        {
            if (m_source.FilePath.Length == 0) return false;

            if (m_lastWriteTime == null) return true;

            if ((System.DateTime.Now - m_lastCheckTime) < System.TimeSpan.FromSeconds(5)) return false;
            if (!System.IO.File.Exists(m_source.FilePath)) return true;

            m_lastCheckTime = System.DateTime.Now;

            if (m_lastWriteTime != System.IO.File.GetLastWriteTime(m_source.FilePath))
            {
                return true;
            }
            return false;
        }

        private void parse(bool importMode)
        {
            parseRequire();
            parseDeclaration();
            if (importMode) return;
            parseVariables();
        }

        private void parseDeclaration()
        {
            DeclParser dp = new DeclParser();
            dp.parse(m_source.text);
            msg = dp.msg;
            dp.apply(result);
            
            result.Types.removeEmptyNamespace();
            result.Variables.removeEmptyNamespace();
        }

        private void parseRequire()
        {
            MatchCollection rst = s_requreReg.Matches(m_source.text);
            string filebasepath = null;
            if (m_source.FilePath.Length != 0)
            {
                System.IO.FileInfo fi = new System.IO.FileInfo(m_source.FilePath);
                filebasepath = fi.DirectoryName + "\\";
            }

            foreach (Match m in rst)
            {
                string fn = getFilename(m.Groups[1].Value, filebasepath);
                if (fn != null)
                {
                    if (m_required.ContainsKey(fn))
                    {
                        continue;
                    }

                    m_required[fn] = 0;

                    FileParser fp = getFile(fn, m_source.m_intellua, m_required);
                    result.Requires.Add(fp.result);
                }
            }
        }
        private Scope parseScope(int start, int end)
        {
            /*int level = m_source.Lines[start].FoldLevel;
            Scope rst = new Scope();
            rst.StartPos = Lines[start].StartPosition;
            rst.EndPos = Lines[end].EndPosition;

            for (int i = start; i <= end; i++)
            {
                if (Lines[i].FoldLevel != level)
                {
                    int s = i;
                    while (i <= end && Lines[i].FoldLevel != level)
                    {
                        i++;
                    }
                    i--;
                    int e = i;
                    Scope c = parseScope(s, e);
                    c.Parent = rst;
                    rst.Childs.Add(c);
                }
            }

            return rst;*/
            return null;
        }

        private void parseVariables()
        {
            {
                LuaTokenizer lt = new LuaTokenizer(m_source.RawText, 0);
                List<LuaToken> tokens = new List<LuaToken>();
                while (true)
                {
                    LuaToken t = lt.getToken();
                    tokens.Add(t);
                    if (t.Type == LuaTokenType.EOF) break;
                }
                //System.Diagnostics.Debug.Print("=========");
                LuaParser lp = new LuaParser(tokens);

                LuaAST chunk = lp.parse();
                //chunk.print(0);
                LuaASTWalker walker = new LuaASTWalker();
                if (lp.errMsg != null)
                {
                    msg = lp.errMsg;
                }

                walker.walk(chunk, result);
                
                
                
            }

/*

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
                //if (t.displa == "") continue;
                //System.Diagnostics.Debug.Print(varName + " added");

                var = new Variable(varName);
                var.IsStatic = false;
                var.Type = t;
                var.StartPos = v.StartPos;
                var.EndPos = elem.EndPos;
                result.Variables.add(var);
            }*/
        }
    }
}