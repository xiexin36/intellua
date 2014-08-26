namespace IntelluaTE
{
    partial class DocumentForm
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
            this.scintilla = new Intellua.Intellua();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.scintilla)).BeginInit();
            this.SuspendLayout();
            // 
            // scintilla
            // 
            this.scintilla.AutoComplete.AutoHide = false;
            this.scintilla.AutoComplete.IsCaseSensitive = false;
            this.scintilla.AutoComplete.ListString = "";
            this.scintilla.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scintilla.FilePath = "";
            this.scintilla.Indentation.ShowGuides = true;
            this.scintilla.Indentation.TabWidth = 2;
            this.scintilla.LineWrapping.VisualFlags = ScintillaNET.LineWrappingVisualFlags.End;
            this.scintilla.Location = new System.Drawing.Point(0, 0);
            this.scintilla.Margins.Margin0.Width = 20;
            this.scintilla.Margins.Margin1.AutoToggleMarkerNumber = 0;
            this.scintilla.Margins.Margin1.IsClickable = true;
            this.scintilla.Margins.Margin1.Width = 20;
            this.scintilla.Margins.Margin2.Width = 16;
            this.scintilla.Name = "scintilla";
            this.scintilla.Parse = true;
            this.scintilla.Size = new System.Drawing.Size(292, 246);
            this.scintilla.Styles.BraceBad.FontName = "Verdana\0\0\0\0";
            this.scintilla.Styles.BraceBad.ForeColor = System.Drawing.Color.Red;
            this.scintilla.Styles.BraceLight.FontName = "Verdana\0\0\0\0";
            this.scintilla.Styles.BraceLight.ForeColor = System.Drawing.Color.Magenta;
            this.scintilla.Styles.CallTip.FontName = "Microsoft J";
            this.scintilla.Styles.ControlChar.FontName = "Verdana\0\0\0\0";
            this.scintilla.Styles.Default.FontName = "Verdana\0\0\0\0";
            this.scintilla.Styles.IndentGuide.FontName = "Verdana\0\0\0\0";
            this.scintilla.Styles.LastPredefined.FontName = "Verdana\0\0\0\0";
            this.scintilla.Styles.LineNumber.FontName = "Verdana\0\0\0\0";
            this.scintilla.Styles.Max.FontName = "Verdana\0\0\0\0";
            this.scintilla.TabIndex = 0;
            this.scintilla.StatusChanged += new Intellua.Intellua.StatusChangedHandler(this.scintilla_StatusChanged);
            this.scintilla.ModifiedChanged += new System.EventHandler(this.scintilla_ModifiedChanged);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "Lua (*.lua) |*.lua| All Files (*.*)|*.*";
            // 
            // DocumentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 246);
            this.Controls.Add(this.scintilla);
            this.Font = new System.Drawing.Font("PMingLiU", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Name = "DocumentForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DocumentForm_FormClosing);
            this.Load += new System.EventHandler(this.DocumentForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.scintilla)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Intellua.Intellua scintilla;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}