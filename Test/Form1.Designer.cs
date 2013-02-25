namespace Test
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.intellua1 = new Intellua.Intellua();
            ((System.ComponentModel.ISupportInitialize)(this.intellua1)).BeginInit();
            this.SuspendLayout();
            // 
            // intellua1
            // 
            this.intellua1.AutoComplete.AutoHide = false;
            this.intellua1.AutoComplete.IsCaseSensitive = false;
            this.intellua1.AutoComplete.ListString = "";
            this.intellua1.ConfigurationManager.Language = "lua";
            this.intellua1.Indentation.TabWidth = 2;
            this.intellua1.Lexing.Lexer = ScintillaNET.Lexer.Lua;
            this.intellua1.Lexing.LexerName = "lua";
            this.intellua1.Lexing.LineCommentPrefix = "";
            this.intellua1.Lexing.StreamCommentPrefix = "{ ";
            this.intellua1.Lexing.StreamCommentSufix = " }";
            this.intellua1.Location = new System.Drawing.Point(12, 12);
            this.intellua1.Margins.Margin0.Width = 20;
            this.intellua1.Margins.Margin1.Width = 20;
            this.intellua1.Margins.Margin2.Width = 20;
            this.intellua1.Name = "intellua1";
            this.intellua1.Size = new System.Drawing.Size(740, 416);
            this.intellua1.Styles.BraceBad.FontName = "Courier New";
            this.intellua1.Styles.BraceBad.Size = 10F;
            this.intellua1.Styles.BraceLight.Bold = true;
            this.intellua1.Styles.BraceLight.FontName = "Courier New";
            this.intellua1.Styles.BraceLight.ForeColor = System.Drawing.Color.Red;
            this.intellua1.Styles.BraceLight.Size = 10F;
            this.intellua1.Styles.ControlChar.FontName = "Courier New";
            this.intellua1.Styles.ControlChar.Size = 10F;
            this.intellua1.Styles.Default.BackColor = System.Drawing.SystemColors.Window;
            this.intellua1.Styles.Default.FontName = "Courier New";
            this.intellua1.Styles.Default.Size = 10F;
            this.intellua1.Styles.IndentGuide.FontName = "Courier New";
            this.intellua1.Styles.IndentGuide.Size = 10F;
            this.intellua1.Styles.LastPredefined.FontName = "Courier New";
            this.intellua1.Styles.LastPredefined.Size = 10F;
            this.intellua1.Styles.LineNumber.FontName = "Courier New";
            this.intellua1.Styles.LineNumber.Size = 10F;
            this.intellua1.Styles.Max.FontName = "Courier New";
            this.intellua1.Styles.Max.Size = 10F;
            this.intellua1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 440);
            this.Controls.Add(this.intellua1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.intellua1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Intellua.Intellua intellua1;
    }
}

