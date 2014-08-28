/*
using adjusted BNF to remove left recurrsion:
value ::= nil | false | true | Number | String | '...' | function |
tableconstructor | functioncall | var | '(' exp ')'
exp ::= unop exp | value [binop exp]
prefix ::= '(' exp ')' | Name
index ::= '[' exp ']' | '.' Name
call ::= args | ':' Name args
suffix ::= call | index
var ::= prefix {suffix} index | Name
functioncall ::= prefix {suffix} call

 http://lua-users.org/lists/lua-l/2010-12/msg00699.html
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intellua
{
    class LuaAST {
        public string Name = "";
        public Dictionary<string,LuaAST> Components = new Dictionary<string,LuaAST>();
        public List<LuaAST> ComponentGroup = new List<LuaAST>();
        public LuaToken Token;
        public int start;
        public int end;


        string indent(int id) {
            return new string(' ', id);
        }
        public void print(int id)
        {
            System.Diagnostics.Debug.Print(indent(id) + start + "~" + end + " " + Name + (Token == null ? "" : " " + Token.Type.ToString() + ":" + System.Text.Encoding.UTF8.GetString(Token.data)));
            id++;
            foreach(KeyValuePair<string,LuaAST> kv in Components){
                System.Diagnostics.Debug.Print(indent(id) + kv.Key);
                kv.Value.print(id+1);
            }

            for(int i=0;i<ComponentGroup.Count;i++){
                System.Diagnostics.Debug.Print(indent(id) + i.ToString());
                ComponentGroup[i].print(id+1);
            }
        }

        public void transformPosition(List<LuaToken> tokens) {
            start = tokens[start].pos;
            end = tokens[end].pos;
            foreach (KeyValuePair<string, LuaAST> kv in Components)
            {

                kv.Value.transformPosition(tokens);
            }

            for (int i = 0; i < ComponentGroup.Count; i++)
            {
                ComponentGroup[i].transformPosition(tokens);
            }
        }
    }

    class LuaParser
    {
        List<LuaToken> m_tokens;
        int m_pos = 0;

        public string errMsg;

        private LuaToken peek()
        {
            return m_tokens[m_pos];
        }

        private class ParserState
        {
            private LuaParser parser;

            public int pos;

            public ParserState(LuaParser p)
            {
                parser = p;

                pos = parser.m_pos;
            }

            public void restore()
            {
                parser.m_pos = pos;
            }
        };


        public LuaParser(List<LuaToken> tokens) {
            m_tokens = tokens;
        }

        void error(string msg) {
            string rst = "Line " + peek().line + ": " + msg;
            throw new Exception(rst);
        }

        public LuaAST parse() {
            LuaAST rst = parseChunk();
            while (peek().Type != LuaTokenType.EOF) {
                m_pos++;
                LuaAST partial = parseChunk();
                if (partial != null) {
                    foreach (LuaAST t in partial.ComponentGroup) {
                        rst.ComponentGroup.Add(t);
                    }
                    
                }
            }
            rst.end = m_tokens.Count-1;
            rst.transformPosition(m_tokens);
            return rst;
        }

        LuaAST parseChunk() {
            ParserState ps = new ParserState(this);
            int i = 0;
            LuaAST rst = new LuaAST();
            rst.start = m_pos;
            rst.Name = "chunk";
            
            while(true)
            {
                ParserState statState = new ParserState(this);
                try
                {
                    LuaAST stat = parseStat();
                    if (stat == null) {
                        LuaAST laststat = parseLaststat();
                        if (laststat != null)
                        {
                            rst.Components.Add("laststat", laststat);
                            if (peek().Type == LuaTokenType.OP_semicolon)
                            {
                                m_pos++;
                            }
                        }
                        break;
                    }
                    rst.ComponentGroup.Add(stat);
                    if (peek().Type == LuaTokenType.OP_semicolon)
                    {
                        m_pos++;
                    }

                }
                catch (Exception e) {
                    if (errMsg == null) errMsg = e.Message;
                    statState.restore();
                    if (peek().Type != LuaTokenType.EOF)
                    {
                        m_pos++;
                    }
                    else {
                        break;
                    }
                }
                
            }



            rst.end = m_pos;

            return rst;

            ps.restore();
            return null;
        }
        LuaAST parseBlock() {
            ParserState ps = new ParserState(this);
            return parseChunk();

            ps.restore();
            return null;
        }

        LuaAST parseStat()
        {
            ParserState ps = new ParserState(this);
            
            do
            {
                LuaAST varlist = parseVarlist();
                if (varlist == null) break;
                if (peek().Type != LuaTokenType.OP_assign) break;
                m_pos++;
                LuaAST explist = parseExplist();
                if (explist == null) break;
                LuaAST rst = new LuaAST();
                rst.Name = "assignExp";
                rst.start = ps.pos;
                rst.end = m_pos;
                rst.Components.Add("varlist", varlist);
                rst.Components.Add("explist", explist);
                return rst;
            } while (false);
            ps.restore();

            do
            {
                LuaAST functioncall = parseFunctioncall();
                if (functioncall == null) break;
                LuaAST rst = new LuaAST();
                rst.Name = "functioncallExp";
                rst.Components.Add("functioncall", functioncall);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do
            {
                if (peek().Type != LuaTokenType.KW_do) break;
                m_pos++;
                LuaAST block = parseBlock();
                if (block == null) error("block expected");
                if (peek().Type != LuaTokenType.KW_end) error("'end' expected");
                m_pos++;
                LuaAST rst = new LuaAST();
                
                rst.Name = "doExp";
                rst.Components.Add("block", block);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do
            {
                if (peek().Type != LuaTokenType.KW_while) break;
                m_pos++;

                LuaAST exp = parseExp();
                if (exp == null) error("expression expected");
                if (peek().Type != LuaTokenType.KW_do) error("'do' expected");
                m_pos++;
                LuaAST block = parseBlock();
                if (block == null) break;
                if (peek().Type != LuaTokenType.KW_end) error("'end' expected");
                m_pos++;

                LuaAST rst = new LuaAST();
                rst.Name = "whileExp";
                rst.Components.Add("exp", exp);
                rst.Components.Add("block", block);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();
            do
            {
                if (peek().Type != LuaTokenType.KW_repeat) break;
                m_pos++;
                LuaAST block = parseBlock();
                if (block == null) error("block expected"); ;
                if (peek().Type != LuaTokenType.KW_until) error("'until' expected");
                m_pos++;

                LuaAST exp = parseExp();
                if (exp == null) error("expression expected"); ;

                LuaAST rst = new LuaAST();
                rst.Name = "repeatExp";
                rst.Components.Add("exp", exp);
                rst.Components.Add("block", block);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();
            do
            {
                if (peek().Type != LuaTokenType.KW_if) break;
                m_pos++;

                LuaAST exp = parseExp();
                if (exp == null) error("expression expected");
                if (peek().Type != LuaTokenType.KW_then) error("'then' expected");
                m_pos++;
                LuaAST block = parseBlock();
                if (block == null) error("block expected");

                LuaAST rst = new LuaAST();
                rst.Name = "ifExp";
                rst.Components.Add("exp", exp);
                rst.Components.Add("block", block);

                while (peek().Type == LuaTokenType.KW_elseif) {
                    m_pos++;
                    LuaAST expblock = new LuaAST();
                    expblock.Name = "expblock";
                    LuaAST elexp = parseExp();
                    if(elexp == null) error("expression expected");
                    expblock.Components.Add("exp", elexp);
                    LuaAST elblock = parseBlock();
                    if (elblock == null) error("block expected");
                    expblock.Components.Add("block", elblock);
                    rst.ComponentGroup.Add(expblock);
                }

                if (peek().Type == LuaTokenType.KW_else) {
                    m_pos++;
                    LuaAST elblock = parseBlock();
                    if (elblock == null) error("block expected");
                    rst.Components.Add("elseBlock", elblock);
                }

                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();
            do
            {
                if (peek().Type != LuaTokenType.KW_for) break;
                m_pos++;
                LuaAST name = parseName();
                if (name == null) break;
                if (peek().Type != LuaTokenType.OP_assign) break;
                m_pos++;
                LuaAST initExp = parseExp();
                if (initExp == null) error("expression expected");
                if (peek().Type != LuaTokenType.OP_comma) error("',' expected");
                m_pos++;

                LuaAST condExp = parseExp();
                if (condExp == null) error("expression expected");

                LuaAST rst = new LuaAST();
                rst.Name = "forExp";
                rst.Components.Add("name", name);
                rst.Components.Add("initExp", initExp);
                rst.Components.Add("condExp", condExp);
                

                if (peek().Type == LuaTokenType.OP_comma)
                {
                    m_pos++;
                    LuaAST stepExp = parseExp();
                    if (stepExp == null) error("expression expected");
                    rst.Components.Add("stepExp", stepExp);
                }

                if (peek().Type != LuaTokenType.KW_do) error("'do' expected");
                m_pos++;
                LuaAST block = parseBlock();
                if (block == null) error("block expected");
                rst.Components.Add("block", block);
                if (peek().Type != LuaTokenType.KW_end) error("'end' expected");
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();
            do
            {
                if (peek().Type != LuaTokenType.KW_for) break;
                m_pos++;
                LuaAST namelist = parseNamelist();
                if (namelist == null) error("name or namelist expected");

                if (peek().Type != LuaTokenType.KW_in) error("'in' expected");
                m_pos++;

                LuaAST explist = parseExplist();
                if (explist == null) error("expression list expected");
                
                LuaAST rst = new LuaAST();
                rst.Name = "forInExp";
                rst.Components.Add("namelist", namelist);
                rst.Components.Add("explist", explist);
              
                if (peek().Type != LuaTokenType.KW_do) error("'do' expected");
                m_pos++;
                LuaAST block = parseBlock();
                if (block == null) error("block expected");
                rst.Components.Add("block", block);
                if (peek().Type != LuaTokenType.KW_end) error("'end' expected");
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();
            do
            {
                if (peek().Type != LuaTokenType.KW_function) break;
                m_pos++;
                LuaAST funcname = parseFuncname();
                if (funcname == null) error("function name expected");

                LuaAST funcbody = parseFuncbody();
                if (funcbody == null) error("function body expected");
                

                LuaAST rst = new LuaAST();
                rst.Name = "functionExp";
                rst.Components.Add("funcname", funcname);
                rst.Components.Add("funcbody", funcbody);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();
            do
            {
                if (peek().Type != LuaTokenType.KW_local) break;
                m_pos++;
                if (peek().Type != LuaTokenType.KW_function) break;
                m_pos++;
                LuaAST funcname = parseFuncname();
                if (funcname == null) error("function name expected");

                LuaAST funcbody = parseFuncbody();
                if (funcbody == null) error("function body expected");


                LuaAST rst = new LuaAST();
                rst.Name = "localFunctionExp";
                rst.Components.Add("funcname", funcname);
                rst.Components.Add("funcbody", funcbody);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do
            {
                if (peek().Type != LuaTokenType.KW_local) break;
                m_pos++;
                LuaAST varlist = parseVarlist();
                if (varlist == null) error("name expected"); ;
                if (peek().Type != LuaTokenType.OP_assign) break;
                m_pos++;
                LuaAST explist = parseExplist();
                if (explist == null) break;
                LuaAST rst = new LuaAST();
                rst.Name = "localAssignExp";
                rst.Components.Add("varlist", varlist);
                rst.Components.Add("explist", explist);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;

            } while (false);
            ps.restore();
            //allow id-expression
            do{
                return parseName();
            }while(false);

            ps.restore();
            return null;
        }

        LuaAST parseLaststat() {
            ParserState ps = new ParserState(this);
            do
            {
                if (peek().Type != LuaTokenType.KW_return) break;
                m_pos++;
                LuaAST rst = new LuaAST();
                rst.Name = "return";
                LuaAST explist = parseExplist();
                if (explist != null) {
                    rst.Components.Add("explist", explist);
                }
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();
            if (peek().Type == LuaTokenType.KW_break) {
                m_pos++;
                LuaAST rst = new LuaAST();
                rst.Name = "break";
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }
            ps.restore();
            return null;
        }
        LuaAST parseFuncname() {
            ParserState ps = new ParserState(this);
            do
            {
                LuaAST name = parseName();
                if (name == null) break;
                LuaAST rst = new LuaAST();
                rst.Name = "funcname";
                rst.ComponentGroup.Add(name);
                while (peek().Type == LuaTokenType.OP_dot) {
                    m_pos++;
                    name = parseName();
                    if (name == null) error("name expected");
                    rst.ComponentGroup.Add(name);
                }
                if (peek().Type == LuaTokenType.OP_colon) {
                    m_pos++;
                    name = parseName();
                    if (name == null) error("name expected");
                    rst.Components.Add("colonName",name);
                }
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;

            } while (false);

            ps.restore();
            return null;
        }
        LuaAST parseVarlist() {
            ParserState ps = new ParserState(this);
            do
            {
                LuaAST var = parseVar();
                if (var == null) break;
                LuaAST rst = new LuaAST();
                rst.Name = "varlist";
                rst.ComponentGroup.Add(var);
                while (peek().Type == LuaTokenType.OP_comma)
                {
                    m_pos++;
                    var = parseVar();
                    if (var == null) error("var expected");
                    rst.ComponentGroup.Add(var);
                }
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;

            } while (false);

            ps.restore();
            return null;
        }
        LuaAST parseValue() {
            ParserState ps = new ParserState(this);
            LuaAST rst = new LuaAST();
            rst.Name = "value";
            if (peek().Type == LuaTokenType.KW_nil) {
                rst.Token = peek();
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }
            if (peek().Type == LuaTokenType.KW_false)
            {
                rst.Token = peek();
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }
            if (peek().Type == LuaTokenType.KW_true)
            {
                rst.Token = peek();
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }
            if (peek().Type == LuaTokenType.Number)
            {
                rst.Token = peek();
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }
            if (peek().Type == LuaTokenType.StringLiteral)
            {
                rst.Token = peek();
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }
            if (peek().Type == LuaTokenType.OP_ellipsis)
            {
                rst.Token = peek();
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }
            do
            {
                LuaAST func = parseFunction();
                if (func == null) break;
                rst.Components.Add("function", func);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do
            {
                LuaAST tc = parseTableconstructor();
                if (tc == null) break;
                rst.Components.Add("tableconstructor", tc);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();
            do
            {
                LuaAST fc = parseFunctioncall();
                if (fc == null) break;
                rst.Components.Add("functioncall", fc);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do
            {
                LuaAST var = parseVar();
                if (var == null) break;
                rst.Components.Add("var", var);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);

            do
            {
                if (peek().Type != LuaTokenType.OP_lparen) break;
                m_pos++;
                LuaAST exp = parseExp();
                if (exp == null) error ("expression expected");
                rst.Components.Add("exp", exp);
                if (peek().Type != LuaTokenType.OP_rparen) error("')' expected");
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            return null;
        }
        LuaAST parseVar() {
            ParserState ps = new ParserState(this);
            do
            {
                LuaAST name = parseName();
                if (name != null) return name;
            } while (false);
            ps.restore();
            do
            {
                LuaAST prefix = parsePrefix();
                if (prefix == null) break;
                LuaAST suffix = parseSuffix();
                LuaAST rst = new LuaAST();
                rst.Name = "var";
                rst.Components.Add("prefix", prefix);
                while (suffix != null) {
                    rst.ComponentGroup.Add(suffix);
                    suffix = parseSuffix();
                }
                if (rst.ComponentGroup.Count == 0 || rst.ComponentGroup[rst.ComponentGroup.Count - 1].Name != "index") error("index expected");
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            
            ps.restore();
            return null;
        }
        LuaAST parseNamelist() {
            ParserState ps = new ParserState(this);

            do
            {
                LuaAST name = parseName();
                if (name == null) break;
                LuaAST rst = new LuaAST();
                rst.Name = "namelist";
                rst.ComponentGroup.Add(name);
                while (peek().Type == LuaTokenType.OP_comma)
                {
                    m_pos++;
                    name = parseName();
                    if (name == null) error("name expected");
                    rst.ComponentGroup.Add(name);
                }
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;

            } while (false);

            ps.restore();
            return null;

        }
        LuaAST parseExplist() {
            ParserState ps = new ParserState(this);
            do
            {
                LuaAST exp = parseExp();
                if (exp == null) break;
                LuaAST rst = new LuaAST();
                rst.Name = "explist";
                rst.ComponentGroup.Add(exp);
                while (peek().Type == LuaTokenType.OP_comma)
                {
                    m_pos++;
                    exp = parseExp();
                    if (exp == null) error("expression expected");
                    rst.ComponentGroup.Add(exp);
                }
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;

            } while (false);

            ps.restore();
            return null;
        }
        LuaAST parseExp() {
            ParserState ps = new ParserState(this);
            do
            {
                LuaAST unop = parseUnop();
                if (unop == null) break;
                LuaAST exp = parseExp();
                if (exp == null) error("expression expected");

                LuaAST rst = new LuaAST();
                rst.Name = "unopExp";
                rst.Components.Add("unop", unop);
                rst.Components.Add("exp", exp);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do{
                LuaAST value = parseValue();
                if(value == null) break;
                LuaAST rst =new LuaAST();
                rst.Name = "biopExp";
                rst.Components.Add("value",value);
                
                LuaAST biop = parseBiop();
                if(biop != null){
                    LuaAST exp = parseExp();
                    if(exp == null) error("expression expected");
                    rst.Components.Add("biop",biop);
                    rst.Components.Add("exp",exp);
                }
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }while(false);
            ps.restore();
            return null;
        }
        LuaAST parsePrefix()
        {
            ParserState ps = new ParserState(this);
            LuaAST rst = new LuaAST();
            rst.Name = "prefix";
            do
            {
                if (peek().Type != LuaTokenType.OP_lparen) break;
                m_pos++;
                LuaAST exp = parseExp();
                if (exp == null) error("expression expected");
                if (peek().Type != LuaTokenType.OP_rparen) error("')' expected");
                m_pos++;
                rst.Components.Add("exp", exp);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);

            LuaAST name = parseName();
            if (name != null)
            {
                rst.Components.Add("name", name);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }

            ps.restore();
            return null;
        }
        LuaAST parseIndex()
        {
            ParserState ps = new ParserState(this);
            LuaAST rst = new LuaAST();
            rst.Name = "index";
            do
            {
                if (peek().Type != LuaTokenType.OP_lbracket) break;
                m_pos++;
                LuaAST exp = parseExp();
                if (exp == null) error("expression expected");
                if (peek().Type != LuaTokenType.OP_rbracket) error("']' expected");
                m_pos++;
                rst.Components.Add("exp", exp);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do
            {
                if (peek().Type != LuaTokenType.OP_dot) break;
                m_pos++;
                LuaAST name = parseName();
                if (name == null) error("name expected");
                
                rst.Components.Add("name", name);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            return null;
        }
        LuaAST parseCall()
        {
            ParserState ps = new ParserState(this);
            LuaAST rst = new LuaAST();
            rst.Name = "call";
            do
            {
                LuaAST args = parseArgs();
                if (args == null) break;
                rst.Components.Add("args", args);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do
            {
                if (peek().Type != LuaTokenType.OP_colon) break;
                m_pos++;
                LuaAST name = parseName();
                if (name == null) error("name expected");
                rst.Components.Add("name", name);
                LuaAST args = parseArgs();
                if (args == null) error("args expected");
                rst.Components.Add("args", args);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            return null;
        }
        LuaAST parseSuffix()
        {
            ParserState ps = new ParserState(this);
            LuaAST rst = new LuaAST();
            rst.Name = "suffix";
            LuaAST call = parseCall();
            if (call != null) {
                rst.Components.Add("call", call);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }
            LuaAST index = parseIndex();
            if (index != null)
            {
                rst.Components.Add("index", index);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }
            ps.restore();
            return null;
        }

        LuaAST parseFunctioncall() {
            ParserState ps = new ParserState(this);
            do
            {
                LuaAST prefix = parsePrefix();
                if (prefix == null) break;
                LuaAST rst = new LuaAST();
                rst.Components.Add("prefix", prefix);
                LuaAST suffix = parseSuffix();
                while (suffix!=null) {
                    rst.ComponentGroup.Add(suffix);
                    suffix = parseSuffix();
                }

                if (rst.ComponentGroup.Count == 0) break;
                LuaAST last = rst.ComponentGroup[rst.ComponentGroup.Count-1];
                if (!last.Components.ContainsKey("call")) break;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);


            ps.restore();
            return null;
        }
        LuaAST parseArgs() {
            ParserState ps = new ParserState(this);
            LuaAST rst = new LuaAST();
            rst.Name = "args";
            do
            {
                if (peek().Type != LuaTokenType.OP_lparen) break;
                m_pos++;
                
                LuaAST explist = parseExplist();
                if (explist != null) rst.Components.Add("explist",explist);
                if (peek().Type != LuaTokenType.OP_rparen) error("')' expected");
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do
            {
                LuaAST tc = parseTableconstructor();
                if (tc == null) break;
                rst.Components.Add("tableconstructor", tc);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            if (peek().Type == LuaTokenType.StringLiteral) {
                rst.Token = peek();
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }

            return null;
        }
        LuaAST parseFunction() {
            ParserState ps = new ParserState(this);
            do
            {
                if (peek().Type != LuaTokenType.KW_function) break;
                m_pos++;

                LuaAST body = parseFuncbody();
                if (body == null) error("function body expected");
                LuaAST rst = new LuaAST();
                rst.Name = "function";
                rst.Components.Add("funcbody", body);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);

            ps.restore();
            return null;
        }
        LuaAST parseFuncbody() {
            ParserState ps = new ParserState(this);
            do{
                if (peek().Type != LuaTokenType.OP_lparen) break;
                m_pos++;
                LuaAST rst = new LuaAST();
                rst.Name = "funcbody";
                LuaAST parlist = parseParlist();
                
                if (parlist != null) {
                    rst.Components.Add("parlist", parlist);
                }
                if (peek().Type != LuaTokenType.OP_rparen) error("')' expected");
                m_pos++;

                LuaAST block = parseBlock();
                if (block == null) error("block expected");
                rst.Components.Add("block", block);
                if (peek().Type != LuaTokenType.KW_end) error("'end' expected");
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }while (false);


            ps.restore();
            return null;
        }
        LuaAST parseParlist() {
            ParserState ps = new ParserState(this);
            LuaAST rst = new LuaAST();
            rst.Name = "parlist";
            do
            {
                LuaAST namelist = parseNamelist();
                if (namelist == null) break;
                rst.Components.Add("namelist", namelist);
                if (peek().Type == LuaTokenType.OP_comma) {
                    m_pos++;
                    if (peek().Type != LuaTokenType.OP_ellipsis) error("'...' expected");
                    rst.Token = peek();
                    m_pos++;
                   
                }
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();
            if (peek().Type == LuaTokenType.OP_ellipsis)
            {
                rst.Token = peek();
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }

            ps.restore();
            return null;
        }
        LuaAST parseTableconstructor() {
            ParserState ps = new ParserState(this);

            do
            {
                if (peek().Type != LuaTokenType.OP_lbrace) break;
                m_pos++;

                LuaAST rst = new LuaAST();
                rst.Name = "tableconstructor";
                LuaAST fieldlist = parseFieldlist();
                if (fieldlist != null) {
                    rst.Components.Add("fieldlist", fieldlist);
                }
                if (peek().Type != LuaTokenType.OP_rbrace) error("'}' expected");
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);


            ps.restore();
            return null;
        }
        LuaAST parseFieldlist() {
            ParserState ps = new ParserState(this);
            do
            {
                LuaAST field = parseField();
                if (field == null) break;
                LuaAST rst = new LuaAST();
                rst.Name = "fieldlist";
                rst.ComponentGroup.Add(field);

                LuaAST fieldsep = parseFieldsep();
                while (fieldsep != null) {
                    field = parseField();
                    if (field == null) { break; }
                    rst.ComponentGroup.Add(field);
                    fieldsep = parseFieldsep();
                }
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);

            ps.restore();
            return null;
        }
        LuaAST parseField() {
            ParserState ps = new ParserState(this);
            LuaAST rst = new LuaAST();
            rst.Name = "field";
            do
            {
                if (peek().Type != LuaTokenType.OP_lbracket) break;
                m_pos++;
                LuaAST nameExp = parseExp();
                if(nameExp == null) error("expression expected");
                if (peek().Type != LuaTokenType.OP_lbracket) error("']' expected");
                m_pos++;
                if (peek().Type != LuaTokenType.OP_assign) error("'=' expected");
                m_pos++;
                LuaAST valExp = parseExp();
                if (valExp == null) error("expression expected");
                
                rst.Components.Add("nameExp", nameExp);
                rst.Components.Add("valExp", valExp);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do
            {
                LuaAST name = parseName();
                if (name == null) break;
                if (peek().Type != LuaTokenType.OP_assign) error("'=' expected");
                m_pos++;
                LuaAST exp = parseExp();
                if ( exp == null) error("expression expected");

                rst.Components.Add("name", name);
                rst.Components.Add("exp", exp);
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            } while (false);
            ps.restore();

            do
            {
                LuaAST exp = parseExp();
                if (exp == null) break;
                rst.Components.Add("exp", exp);
                rst.start = ps.pos;
                rst.end = m_pos;
                return exp;
            } while (false);
            ps.restore();
            return null;
        }
        LuaAST parseFieldsep() {
            ParserState ps = new ParserState(this);
            switch (peek().Type) { 
                case LuaTokenType.OP_comma:
                case LuaTokenType.OP_semicolon:
                    LuaAST rst = new LuaAST();
                    rst.Token = peek();
                    m_pos++;
                    rst.start = ps.pos;
                    rst.end = m_pos;
                    return rst;
            }

            ps.restore();
            return null;
        }
        LuaAST parseBiop() {
            ParserState ps = new ParserState(this);
            switch (peek().Type)
            {
                case LuaTokenType.OP_add:
                case LuaTokenType.OP_sub:
                case LuaTokenType.OP_mul:
                case LuaTokenType.OP_div:
                case LuaTokenType.OP_pow:
                case LuaTokenType.OP_mod:
                case LuaTokenType.OP_doubleDot:
                case LuaTokenType.OP_lt:
                case LuaTokenType.OP_le:
                case LuaTokenType.OP_gt:
                case LuaTokenType.OP_ge:
                case LuaTokenType.OP_eq:
                case LuaTokenType.OP_ne:
                case LuaTokenType.KW_and:
                case LuaTokenType.KW_or:
                    LuaAST rst = new LuaAST();
                    rst.Token = peek();
                    m_pos++;
                    rst.start = ps.pos;
                rst.end = m_pos;
                    return rst;
            }

            ps.restore();
            return null;
        }
        LuaAST parseUnop() {
            ParserState ps = new ParserState(this);

            switch (peek().Type)
            {
                case LuaTokenType.OP_sub:
                case LuaTokenType.OP_hash:
                case LuaTokenType.KW_not:
                    LuaAST rst = new LuaAST();
                    rst.Token = peek();
                    m_pos++;
                    rst.start = ps.pos;
                rst.end = m_pos;
                    return rst;
            }
            ps.restore();
            return null;
        }

        LuaAST parseName() {
            ParserState ps = new ParserState(this);

            if (peek().Type == LuaTokenType.Identifier) {
                LuaAST rst = new LuaAST();
                rst.Name = "Name";
                rst.Token = peek();
                m_pos++;
                rst.start = ps.pos;
                rst.end = m_pos;
                return rst;
            }
            ps.restore();
            return null;
        }
    }
}
