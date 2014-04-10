using System;
using System.Drawing;
using ScintillaNET;

namespace Intellua
{
    // A helper class to use the Scintilla container as an INI lexer.
    // We'll ignore the fact that SciLexer.DLL already has an INI capable lexer. ;)
    internal sealed class LuaLexer
    {
        #region Constants

        private const int EOL = -1;

        // SciLexer's weird choice for a default style _index
        private const int DEFAULT_STYLE = 32;
        
        // Our custom styles (indexes chosen not to conflict with anything else)

        private const int SCE_LUA_DEFAULT = 0;
        private const int SCE_LUA_COMMENT = 1;
        private const int SCE_LUA_COMMENTLINE = 2;
        private const int SCE_LUA_COMMENTDOC = 3;
        private const int SCE_LUA_NUMBER = 4;
        private const int SCE_LUA_WORD = 5;
        private const int SCE_LUA_STRING = 6;
        private const int SCE_LUA_CHARACTER = 7;
        private const int SCE_LUA_LITERALSTRING = 8;
        private const int SCE_LUA_PREPROCESSOR = 9;
        private const int SCE_LUA_OPERATOR = 10;
        private const int SCE_LUA_IDENTIFIER = 11;
        private const int SCE_LUA_STRINGEOL = 12;
        private const int SCE_LUA_WORD2 = 13;
        private const int SCE_LUA_WORD3 = 14;
        private const int SCE_LUA_WORD4 = 15;
        private const int SCE_LUA_WORD5 = 16;
        private const int SCE_LUA_WORD6 = 17;
        private const int SCE_LUA_WORD7 = 18;
        private const int SCE_LUA_WORD8 = 19;
        private const int SCE_LUA_LABEL = 20;
        private const int SCE_LUA_ANNOTATION = 21;
        private const int SCE_LUA_ANNOTATIONLINE = 22;


        #endregion Constants


        #region Fields

        private Scintilla _scintilla;
        private int _startPos;

        private int _index;
        private string _text;

        #endregion Fields


        #region Methods

        public static void Init(Scintilla scintilla)
        {
            // Reset any current language and enable the StyleNeeded
            // event by setting the lexer to container.
            scintilla.Indentation.SmartIndentType = SmartIndent.None;
            scintilla.ConfigurationManager.Language = "Lua";
            scintilla.Lexing.LexerName = "Lua";
            scintilla.Lexing.Lexer = Lexer.Lua;

            // Add our custom styles to the collection
            scintilla.Styles["BRACEBAD"].ForeColor = Color.FromArgb(255, 0, 0);
            scintilla.Styles["BRACELIGHT"].ForeColor = Color.FromArgb(255, 0, 255);

            scintilla.Styles[32].ForeColor = Color.FromArgb(153, 51, 51);
/*
            scintilla.Styles[SCE_LUA_COMMENT].ForeColor = Color.FromArgb(153, 51, 51);
            scintilla.Styles[SCE_LUA_COMMENT].ForeColor = Color.FromArgb(0, 0, 153);
            scintilla.Styles[SCE_LUA_COMMENT].ForeColor = Color.OrangeRed;
            scintilla.Styles[SCE_LUA_COMMENT].ForeColor = Color.FromArgb(102, 0, 102);
            scintilla.Styles[SCE_LUA_COMMENT].ForeColor = Color.FromArgb(102, 102, 102);
            scintilla.Styles[SCE_LUA_COMMENT].ForeColor = Color.FromArgb(0, 0, 102);
            scintilla.Styles[SCE_LUA_COMMENT].Bold = true;*/
        }


        private int Read()
        {
            if (_index < _text.Length)
                return _text[_index];

            return EOL;
        }


        private void SetStyle(int style, int length)
        {
            if (length > 0)
            {
                // TODO Still using old API
                // This will style the _length of chars and advance the style pointer.
                ((INativeScintilla)_scintilla).SetStyling(length, style);
            }
        }


        public void Style()
        {
            // TODO Still using the old API
            // Signals that we're going to begin styling from this point.
            ((INativeScintilla)_scintilla).StartStyling(_startPos, 0x1F);

            // Run our humble lexer...
            StyleWhitespace();
            switch(Read())
            {
                case '[':

                    // Section, default, comment
                    StyleUntilMatch(SCE_LUA_COMMENT, new char[] { ']' });
                    StyleCh(SCE_LUA_COMMENT);
                    StyleUntilMatch(DEFAULT_STYLE, new char[] { ';' });
                    goto case ';';
                
                case ';':

                    // Comment
                    SetStyle(SCE_LUA_COMMENT, _text.Length - _index);
                    break;
                
                default:

                    // Key, assignment, quote, value, comment
                    StyleUntilMatch(SCE_LUA_COMMENT, new char[] { '=', ';' });
                    switch (Read())
                    {
                        case '=':

                            // Assignment, quote, value, comment
                            StyleCh(SCE_LUA_COMMENT);
                            switch (Read())
                            {
                                case '"':

                                    // Quote
                                    StyleCh(SCE_LUA_COMMENT);  // '"'
                                    StyleUntilMatch(SCE_LUA_COMMENT, new char[] { '"' });
                                    
                                    // Make sure it wasn't an escaped quote
                                    if (_index > 0 && _index < _text.Length && _text[_index - 1] == '\\')
                                        goto case '"';

                                    StyleCh(SCE_LUA_COMMENT); // '"'
                                    goto default;

                                default:

                                    // Value, comment
                                    StyleUntilMatch(SCE_LUA_COMMENT, new char[] { ';' });
                                    SetStyle(SCE_LUA_COMMENT, _text.Length - _index);
                                    break;
                            }
                            break;

                        default: // ';', EOL

                            // Comment
                            SetStyle(SCE_LUA_COMMENT, _text.Length - _index);
                            break;
                    }
                    break;
            }
            _scintilla.Lexing.Colorize(_startPos, _startPos + _length);
        }


        private void StyleCh(int style)
        {
            // Style just one char and advance
            SetStyle(style, 1);
            _index++;
        }


        public static void StyleNeeded(Scintilla scintilla, Range range)
        {
            // Create an instance of our lexer and bada-bing the line!
            LuaLexer lexer = new LuaLexer(scintilla, range.Start, range.StartingLine.Length);
            lexer.Style();
        }


        private void StyleUntilMatch(int style, char[] chars)
        {
            // Advance until we match a char in the array
            int startIndex = _index;
            while (_index < _text.Length && Array.IndexOf<char>(chars, _text[_index]) < 0)
                _index++;

            if (startIndex != _index)
                SetStyle(style, _index - startIndex);
        }


        private void StyleWhitespace()
        {
            // Advance the _index until non-whitespace character
            int startIndex = _index;
            while (_index < _text.Length && Char.IsWhiteSpace(_text[_index]))
                _index++;

            SetStyle(DEFAULT_STYLE, _index - startIndex);
        }

        #endregion Methods


        #region Constructors
        private int _length;
        private LuaLexer(Scintilla scintilla, int startPos, int length)
        {
            this._scintilla = scintilla;
            this._startPos = startPos;
            this._length = length;
            // One line of _text
            this._text = scintilla.GetRange(startPos, startPos + length).Text;
        }

        #endregion Constructors
    }
}