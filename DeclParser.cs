using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IntVariable = Intellua.Variable;
using IntFunction = Intellua.Function;
/*
 * BNF:
 * declarations:
 *  (declaration OP_Semicolon)*
 *  
 * declaration:
 *  class 
 *  member OP_Semicolon
 *  
 * class:
 *  comment? KW_Class identifier? (OP_Colon identifier)? OP_LBrace declaration* OP_RBrace identifier?
 *  
 * member:
 *  variable
 *  function
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
    namespace Decl {
        class Indent{
            public static void print(string str,int count)
            {
                System.Diagnostics.Debug.Print( "".PadLeft(count) + str);
            }
        }

        class Declarations {
            public Declarations(){}
            public List<Class> classes = new List<Class>();
            public Members members = new Members();

            public void print(int indent)
            {
                foreach (Class c in classes) {
                    c.print(indent);
                }
                members.print(indent);
            }
        }
        class Declaration { 
            
        };

        class Class : Declaration{
            public Class(){}
            public Type outerClass;
            public string name = "";
            public Declarations declarations = new Declarations();
            public string obj = "";
            public string baseClass = "";
            public string desc = "";

            public void print(int indent)
            {
                string n = "class " + name;
                if(baseClass.Length != 0){
                    n+= " : " + baseClass;
                }
                n+= " {";
                Indent.print(n, indent);
                declarations.print(indent + 1);
                n = "}";
                if (obj.Length > 0) {
                    n += " " + obj;
                }
                n += ";";
                Indent.print(n,indent);
            }
            public Type type;
            public void registerClass(AutoCompleteData ac){
                type = new Type(name);
                type.OuterClass = outerClass;
                ac.Types.add(type);

                foreach(Class c in declarations.classes){
                    c.outerClass = type;
                    c.registerClass(ac);
                }
            }

            public void apply(AutoCompleteData ac)
            {
                IntVariable var = new IntVariable(type.DisplayName);
                var.IsNamespace = true;
                var.IsStatic = true;
                var.Type = type;

                var.Desc = desc;

                if (outerClass == null)
                {
                    ac.Variables.add(var);
                }
                else {
                    outerClass.addMember(var);
                }

                foreach (Class c in declarations.classes) {
                    c.apply(ac);
                }

                foreach (Variable v in declarations.members.variables) {
                    v.apply(ac, this);

                }

                foreach (Function v in declarations.members.functions)
                {
                    v.apply(ac, this);
                }
            }

        }

        class Members{
            public Members(){}

            public void Add(Member m) {
                Variable v = m as Variable;
                if (v != null) {
                    variables.Add(v);
                }
                Function f = m as Function;
                if (f != null)
                {
                    functions.Add(f);
                }
            }

            public void print(int indent)
            {
                foreach (Variable v in variables) {
                    v.print(indent);
                }

                foreach (Function v in functions)
                {
                    v.print(indent);
                }
            }

            public List<Variable> variables = new List<Variable>();
            public List<Function> functions = new List<Function>();
        }
        class Member : Declaration{
        
        };
        class Variable : Member{
            public Variable(){}
            public bool isStatic = false;
            public string type = "";
            public string name = "";
            public int arraySize = 0;
            public string defaultValue = "";
            public string desc = "";
            public string ToString() {
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
            public void print(int indent)
            {
                Indent.print(ToString() + ";", indent);
            }

            public void apply(AutoCompleteData ac, Class c) {
                IntVariable var = new IntVariable(name);
                var.Type = ac.Types.get(type);

                var.Desc = desc;
                var.IsStatic = isStatic;
                if (c != null)
                {
                    c.type.addMember(var);
                }
                else {
                    ac.Variables.add(var);
                }

            }
        }

        class Function : Member{
            public Function(){}
            public bool isStatic = false;
            public string type = "";
            public string desc = "";
            public string name = "";
            public List<Variable> parameters = new List<Variable>();

            

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
                str += name +"(";
                for (int i = 0; i < parameters.Count; i++) {
                    str += parameters[i].ToString();
                    if (i < parameters.Count - 1) {
                        str += ", ";
                    }
                }
                str += ");";
                Indent.print(str, indent);
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
                f.Desc.Add(desc);

                if (c != null && f.Name == c.name)
                {
                    f.ReturnType = c.type;
                    f.Static = true;
                    if (c.outerClass != null)
                    {
                        c.outerClass.addMethod(f);
                    }
                    else
                    {
                        ac.Variables.add(f);
                    }
                }
                else {
                    f.Static = isStatic;
                    f.ReturnType = ac.Types.get(type);

                    if (c != null)
                    {
                        c.type.addMethod(f);
                    }
                    else {
                        f.Static = true;
                        ac.Variables.add(f);
                    }
                }

            }
        }
    }
    
    class DeclParser
    {
        class ParserState
        {
            public ParserState(DeclParser p){
                parser = p;

                pos = parser.pos;
            }
            public void restore() {
                parser.pos = pos;
            }
            int pos;
            DeclParser parser;

        };

        public void apply(AutoCompleteData ac) {
            if (declarations == null) return;

            foreach(Decl.Class c in declarations.classes){
                c.registerClass(ac);
            }

            foreach (Decl.Class c in declarations.classes)
            {
                c.apply(ac);
            }

            foreach (Decl.Variable v in declarations.members.variables)
            {
                v.apply(ac, null);

            }

            foreach (Decl.Function v in declarations.members.functions)
            {
                v.apply(ac, null);
            }
        }

        public void parse(string str) {
            DeclString ds = new DeclString(str);
            string declString = ds.Result;
            DeclTokenizer tokenizer = new DeclTokenizer(declString);
            tokens = tokenizer.Result;
            pos = 0;
            try
            {
                declarations = parseDeclarations();
                System.Diagnostics.Debug.Print("==========================");
                declarations.print(0);
            }
            catch (System.Exception e) {
                System.Diagnostics.Debug.Print(e.Message);
            }


        }
        int pos;

        DeclToken peek() {
            return tokens[pos];
        }

        List<DeclToken> tokens;
        Decl.Declarations declarations;

        Decl.Declarations parseDeclarations(){
            Decl.Declarations rst = new Decl.Declarations();
            while (peek().type != DeclTokenType.EOF) {
                var d = parseDeclaration();
                if (d != null)
                {
                    Decl.Class c = d as Decl.Class;
                    if (c != null)
                    {
                        rst.classes.Add(c);
                    }

                    Decl.Member m = d as Decl.Member;
                    if (m != null)
                    {
                        rst.members.Add(m);
                    }

                    if (peek().type != DeclTokenType.OP_SemiColon)
                    {
                        throw new System.Exception("';' expected");
                    }
                    pos++;
                }
                else {
                    break;
                }
            }
            
            return rst;
            
        }

        Decl.Declaration parseDeclaration() {
            Decl.Declaration rst;
            rst = parseClass();
            if (rst != null)
            {   
                return rst;
            }
            rst = parseMember();
            if (rst != null)
            {
                return rst;
            }
            return null;
        }
        string getUniqueString() {
            Guid g = Guid.NewGuid();
            string GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");
            return GuidString;
        }
        Decl.Class parseClass() {
            ParserState ps = new ParserState(this);
            Decl.Class rst = new Decl.Class();

            if (peek().type == DeclTokenType.Comment) {
                rst.desc = peek().data;
                pos++;
            }

            if (peek().type != DeclTokenType.KW_Class)
            {
                ps.restore();
                return null;
            }
            pos++;
            if (peek().type == DeclTokenType.Identifier)
            {
                rst.name = peek().data;
                pos++;
            }
            else {
                rst.name = "__unnamed@"+getUniqueString();
            }
            if (peek().type == DeclTokenType.OP_Colon) {
                pos++;
                if (peek().type == DeclTokenType.Identifier)
                {
                    rst.baseClass = peek().data;
                    pos++;
                }
                else {
                    throw new System.Exception("baseClass expected");
                }
            }

            if (peek().type != DeclTokenType.OP_LBrace) {
                throw new System.Exception("'{' expected");
            }
            pos++;
            rst.declarations = parseDeclarations();

            if (peek().type != DeclTokenType.OP_RBrace)
            {
                throw new System.Exception("'}' expected");
            }
            pos++;
            if (peek().type == DeclTokenType.Identifier) {
                rst.obj = peek().data;
                pos++;
            }

            return rst;
        }

        Decl.Member parseMember() {
            Decl.Member rst;
            rst = parseFunction();
            if (rst != null) return rst;
            rst = parseVariable();
            if (rst != null) return rst;
            return null;
        }
        Decl.Variable parseVariable() {
            ParserState ps = new ParserState(this);
            Decl.Variable rst = new Decl.Variable();
            
            if (peek().type == DeclTokenType.Comment)
            {
                rst.desc = peek().data;
                pos++;
            }

            if (peek().type == DeclTokenType.OP_Dots) {
                rst.name = "...";
                pos++;
                return rst;
            }

            if (peek().type == DeclTokenType.KW_Static) {
                rst.isStatic = true;
                pos++;
            }
            
            if (peek().type != DeclTokenType.Identifier) {
                ps.restore();
                return null;
            }
            string id1 = peek().data;
            pos++;

            if (peek().type == DeclTokenType.Identifier)
            {
                rst.type = id1;
                rst.name = peek().data;
                pos++;
            }
            else {
                rst.name = id1;
            }

            if (peek().type == DeclTokenType.OP_LSquare) {
                pos++;
                if (peek().type == DeclTokenType.Number) {
                    rst.arraySize = System.Convert.ToInt32(peek().data);
                    pos++;
                }
                if (peek().type != DeclTokenType.OP_RSquare) {
                    throw new System.Exception("']' expected");
                }
                pos++;
            }

            if (peek().type == DeclTokenType.OP_Equal) {
                pos++;
                if (peek().type == DeclTokenType.EOF) {
                    throw new System.Exception("token expected");
                }
                rst.defaultValue = peek().data;
                pos++;
            }
            return rst;
        }
        Decl.Function parseFunction() {
            ParserState ps = new ParserState(this);
            Decl.Function rst = new Decl.Function();

            if (peek().type == DeclTokenType.Comment)
            {
                rst.desc = peek().data;
                pos++;
            }

            if (peek().type == DeclTokenType.KW_Static)
            {
                rst.isStatic = true;
                pos++;
            }

            if (peek().type != DeclTokenType.Identifier)
            {
                ps.restore();
                return null;
            }
            string id1 = peek().data;
            pos++;

            if (peek().type == DeclTokenType.Identifier)
            {
                rst.type = id1;
                rst.name = peek().data;
                pos++;
            }
            else
            {
                rst.name = id1;
            }

            if (peek().type != DeclTokenType.OP_LParen) {
                ps.restore();
                return null;
            }
            pos++;

            Decl.Variable var = parseVariable();
            if (var != null) {
                rst.parameters.Add(var);

                while (peek().type == DeclTokenType.OP_Comma) {
                    pos++;
                    Decl.Variable moreVar = parseVariable();
                    if (moreVar == null) {
                        throw new System.Exception("variable expected");
                    }
                    rst.parameters.Add(moreVar);
                }
            }
            if (peek().type != DeclTokenType.OP_RParen)
            {
                throw new System.Exception("')' expected");
            }
            pos++;


            return rst;
        }
    }
}
