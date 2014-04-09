using System;
using System.Collections.Generic;
using IntFunction = Intellua.Function;
using IntVariable = Intellua.Variable;

/*
 * BNF:
 * declarations:
 *  (declaration OP_Semicolon)*
 *
 * declaration:
 *  class
 *  variable
 *  function
 *
 * class:
 *  comment? KW_Class identifier? (OP_Colon identifier)? OP_LBrace declaration* OP_RBrace identifier?
 *
 * variable:
 *  comment? KW_Static? identifier? identifier (OP_LSquare  number? OP_RSquare)? (OP_Equal token)?
 *  comment? OP_Dots
 *
 * function:
 *  comment? KW_Static? identifier? identifier OP_LParen (variable? (OP_Comma variable)*)? OP_RParen)
 */

namespace Intellua
{
    namespace Decl
    {
        internal interface Declaration
        {
            void addToDeclarations(Declarations d);
        };

        internal class Class : Declaration
        {
            public string BaseClass = "";

            public Declarations Declarations = new Declarations();

            public string Desc = "";

            public string Name = "";

            public string Object = "";

            public Type OuterClass;

            public Type Type;

            public Class()
            {
            }

            public void addToDeclarations(Declarations d)
            {
                d.Classes.Add(this);
            }

            public void apply(AutoCompleteData ac)
            {
                Type.Base = ac.Types.get(BaseClass);
                IntVariable var = new IntVariable(Type.DisplayName);
                var.IsNamespace = true;
                var.IsStatic = true;
                var.Type = Type;

                var.Desc = Desc;

                if (OuterClass == null)
                {
                    ac.Variables.add(var);
                }
                else
                {
                    OuterClass.addMember(var);
                }

                foreach (Class c in Declarations.Classes)
                {
                    c.apply(ac);
                }

                foreach (Variable v in Declarations.Variables)
                {
                    v.apply(ac, this);
                }

                foreach (Function v in Declarations.Functions)
                {
                    v.apply(ac, this);
                }
                if (Object.Length > 0)
                {
                    IntVariable dec = new IntVariable(Object);
                    dec.Type = Type;
                    dec.Desc = Desc;
                    if (OuterClass == null)
                    {
                        ac.Variables.add(dec);
                    }
                    else
                    {
                        OuterClass.addMember(dec);
                    }
                }
            }

            public void print(int indent)
            {
                string n = "class " + Name;
                if (BaseClass.Length != 0)
                {
                    n += " : " + BaseClass;
                }
                n += " {";
                Indent.print(n, indent);
                Declarations.print(indent + 1);
                n = "}";
                if (Object.Length > 0)
                {
                    n += " " + Object;
                }
                n += ";";
                Indent.print(n, indent);
            }

            public void registerClass(AutoCompleteData ac)
            {
                Type = new Type(Name);
                Type.OuterClass = OuterClass;
                ac.Types.add(Type);

                foreach (Class c in Declarations.Classes)
                {
                    c.OuterClass = Type;
                    c.registerClass(ac);
                }
            }
        }
        internal class Declarations
        {
            public List<Class> Classes = new List<Class>();

            public List<Function> Functions = new List<Function>();

            public List<Variable> Variables = new List<Variable>();

            public Declarations()
            {
            }

            public void add(Declaration m)
            {
                m.addToDeclarations(this);
            }
            public void print(int indent)
            {
                foreach (Class c in Classes)
                {
                    c.print(indent);
                }
                foreach (Variable v in Variables)
                {
                    v.print(indent);
                }

                foreach (Function v in Functions)
                {
                    v.print(indent);
                }
            }
        }

        internal class Function : Declaration
        {
            public string Desc = "";

            public bool isStatic = false;

            public string name = "";

            public List<Variable> parameters = new List<Variable>();

            public string type = "";

            public Function()
            {
            }

            public void addToDeclarations(Declarations d)
            {
                d.Functions.Add(this);
            }

            public void apply(AutoCompleteData ac, Class c)
            {
                IntFunction f = new IntFunction(name);
                string paramStr = "(";
                for (int i = 0; i < parameters.Count; i++)
                {
                    paramStr += parameters[i].ToString();
                    if (i < parameters.Count - 1)
                    {
                        paramStr += ", ";
                    }
                }
                paramStr += ")";
                f.Param.Add(paramStr);
                f.Desc.Add(Desc);

                if (c != null && f.Name == c.Name)
                {
                    f.ReturnType = c.Type;
                    f.Static = true;
                    if (c.OuterClass != null)
                    {
                        c.OuterClass.addMethod(f);
                    }
                    else
                    {
                        ac.Variables.add(f);
                    }
                }
                else
                {
                    f.Static = isStatic;
                    f.ReturnType = ac.Types.get(type);

                    if (c != null)
                    {
                        c.Type.addMethod(f);
                    }
                    else
                    {
                        f.Static = true;
                        ac.Variables.add(f);
                    }
                }
            }

            public void print(int indent)
            {
                string str = "";
                if (isStatic)
                {
                    str += "static ";
                }
                if (type.Length > 0)
                {
                    str += type + " ";
                }
                str += name + "(";
                for (int i = 0; i < parameters.Count; i++)
                {
                    str += parameters[i].ToString();
                    if (i < parameters.Count - 1)
                    {
                        str += ", ";
                    }
                }
                str += ");";
                Indent.print(str, indent);
            }
        }

        internal class Indent
        {
            public static void print(string str, int count)
            {
                System.Diagnostics.Debug.Print("".PadLeft(count) + str);
            }
        }

        internal class Variable : Declaration
        {
            public int arraySize = 0;

            public string defaultValue = "";

            public string desc = "";

            public bool isStatic = false;

            public string name = "";

            public string type = "";

            public Variable()
            {
            }

            public void addToDeclarations(Declarations d)
            {
                d.Variables.Add(this);
            }

            public void apply(AutoCompleteData ac, Class c)
            {
                IntVariable var = new IntVariable(name);
                var.Type = ac.Types.get(type);

                var.Desc = desc;
                var.IsStatic = isStatic;
                if (c != null)
                {
                    c.Type.addMember(var);
                }
                else
                {
                    ac.Variables.add(var);
                }
            }

            public void print(int indent)
            {
                Indent.print(toString() + ";", indent);
            }

            public string toString()
            {
                string str = "";
                if (isStatic)
                {
                    str += "static ";
                }
                if (type.Length > 0)
                {
                    str += type + " ";
                }
                str += name;
                if (arraySize > 0)
                {
                    str += "[" + arraySize.ToString() + "]";
                }
                if (defaultValue.Length > 0)
                {
                    str += "=" + defaultValue;
                }
                return str;
            }
        }
    }

    internal class DeclParser
    {
        private Decl.Declarations m_declarations;

        private int m_pos;

        private List<DeclToken> m_tokens;

        public void apply(AutoCompleteData ac)
        {
            if (m_declarations == null) return;

            foreach (Decl.Class c in m_declarations.Classes)
            {
                c.registerClass(ac);
            }

            foreach (Decl.Class c in m_declarations.Classes)
            {
                c.apply(ac);
            }

            foreach (Decl.Variable v in m_declarations.Variables)
            {
                v.apply(ac, null);
            }

            foreach (Decl.Function v in m_declarations.Functions)
            {
                v.apply(ac, null);
            }
        }

        public void parse(string str)
        {
            DeclString ds = new DeclString(str);
            string declString = ds.Result;
            DeclTokenizer tokenizer = new DeclTokenizer(declString);
            m_tokens = tokenizer.Result;
            m_pos = 0;
            try
            {
                m_declarations = parseDeclarations();
                System.Diagnostics.Debug.Print("==========================");
                m_declarations.print(0);
            }
            catch (System.Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }
        }

        private string getUniqueString()
        {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            return GuidString;
        }

        private Decl.Class parseClass()
        {
            ParserState ps = new ParserState(this);
            Decl.Class rst = new Decl.Class();

            if (peek().Type == DeclTokenType.Comment)
            {
                rst.Desc = peek().Data;
                m_pos++;
            }

            if (peek().Type != DeclTokenType.KW_Class)
            {
                ps.restore();
                return null;
            }
            m_pos++;
            if (peek().Type == DeclTokenType.Identifier)
            {
                rst.Name = peek().Data;
                m_pos++;
            }
            else
            {
                rst.Name = "__unnamed@" + getUniqueString();
            }
            if (peek().Type == DeclTokenType.OP_Colon)
            {
                m_pos++;
                if (peek().Type == DeclTokenType.Identifier)
                {
                    rst.BaseClass = peek().Data;
                    m_pos++;
                }
                else
                {
                    throw new System.Exception("baseClass expected");
                }
            }

            if (peek().Type != DeclTokenType.OP_LBrace)
            {
                throw new System.Exception("'{' expected");
            }
            m_pos++;
            rst.Declarations = parseDeclarations();

            if (peek().Type != DeclTokenType.OP_RBrace)
            {
                throw new System.Exception("'}' expected");
            }
            m_pos++;
            if (peek().Type == DeclTokenType.Identifier)
            {
                rst.Object = peek().Data;
                m_pos++;
            }

            return rst;
        }

        private Decl.Declaration parseDeclaration()
        {
            Decl.Declaration rst;
            rst = parseClass();
            if (rst != null) return rst;
            rst = parseFunction();
            if (rst != null) return rst;
            rst = parseVariable();
            if (rst != null) return rst;
            return null;
        }

        private Decl.Declarations parseDeclarations()
        {
            Decl.Declarations rst = new Decl.Declarations();
            while (peek().Type != DeclTokenType.EOF)
            {
                var d = parseDeclaration();
                if (d != null)
                {
                    rst.add(d);

                    if (peek().Type != DeclTokenType.OP_SemiColon)
                    {
                        throw new System.Exception("';' expected");
                    }
                    m_pos++;
                }
                else
                {
                    break;
                }
            }

            return rst;
        }

        private Decl.Function parseFunction()
        {
            ParserState ps = new ParserState(this);
            Decl.Function rst = new Decl.Function();

            if (peek().Type == DeclTokenType.Comment)
            {
                rst.Desc = peek().Data;
                m_pos++;
            }

            if (peek().Type == DeclTokenType.KW_Static)
            {
                rst.isStatic = true;
                m_pos++;
            }

            if (peek().Type != DeclTokenType.Identifier)
            {
                ps.restore();
                return null;
            }
            string id1 = peek().Data;
            m_pos++;

            if (peek().Type == DeclTokenType.Identifier)
            {
                rst.type = id1;
                rst.name = peek().Data;
                m_pos++;
            }
            else
            {
                rst.name = id1;
            }

            if (peek().Type != DeclTokenType.OP_LParen)
            {
                ps.restore();
                return null;
            }
            m_pos++;

            Decl.Variable var = parseVariable();
            if (var != null)
            {
                rst.parameters.Add(var);

                while (peek().Type == DeclTokenType.OP_Comma)
                {
                    m_pos++;
                    Decl.Variable moreVar = parseVariable();
                    if (moreVar == null)
                    {
                        throw new System.Exception("variable expected");
                    }
                    rst.parameters.Add(moreVar);
                }
            }
            if (peek().Type != DeclTokenType.OP_RParen)
            {
                throw new System.Exception("')' expected");
            }
            m_pos++;

            return rst;
        }

        private Decl.Variable parseVariable()
        {
            ParserState ps = new ParserState(this);
            Decl.Variable rst = new Decl.Variable();

            if (peek().Type == DeclTokenType.Comment)
            {
                rst.desc = peek().Data;
                m_pos++;
            }

            if (peek().Type == DeclTokenType.OP_Dots)
            {
                rst.name = "...";
                m_pos++;
                return rst;
            }

            if (peek().Type == DeclTokenType.KW_Static)
            {
                rst.isStatic = true;
                m_pos++;
            }

            if (peek().Type != DeclTokenType.Identifier)
            {
                ps.restore();
                return null;
            }
            string id1 = peek().Data;
            m_pos++;

            if (peek().Type == DeclTokenType.Identifier)
            {
                rst.type = id1;
                rst.name = peek().Data;
                m_pos++;
            }
            else
            {
                rst.name = id1;
            }

            if (peek().Type == DeclTokenType.OP_LSquare)
            {
                m_pos++;
                if (peek().Type == DeclTokenType.Number)
                {
                    rst.arraySize = System.Convert.ToInt32(peek().Data);
                    m_pos++;
                }
                if (peek().Type != DeclTokenType.OP_RSquare)
                {
                    throw new System.Exception("']' expected");
                }
                m_pos++;
            }

            if (peek().Type == DeclTokenType.OP_Equal)
            {
                m_pos++;
                if (peek().Type == DeclTokenType.EOF)
                {
                    throw new System.Exception("token expected");
                }
                rst.defaultValue = peek().Data;
                m_pos++;
            }
            return rst;
        }

        private DeclToken peek()
        {
            return m_tokens[m_pos];
        }

        private class ParserState
        {
            private DeclParser parser;

            private int pos;

            public ParserState(DeclParser p)
            {
                parser = p;

                pos = parser.m_pos;
            }

            public void restore()
            {
                parser.m_pos = pos;
            }
        };
    }
}