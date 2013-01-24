namespace LuaEditor
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.scintilla1 = new ScintillaNET.Scintilla();
            ((System.ComponentModel.ISupportInitialize)(this.scintilla1)).BeginInit();
            this.SuspendLayout();
            // 
            // scintilla1
            // 
            this.scintilla1.Location = new System.Drawing.Point(54, 13);
            this.scintilla1.Name = "scintilla1";
            this.scintilla1.Size = new System.Drawing.Size(587, 406);
            this.scintilla1.Styles.BraceBad.Size = 9F;
            this.scintilla1.Styles.BraceLight.Size = 9F;
            this.scintilla1.Styles.ControlChar.Size = 9F;
            this.scintilla1.Styles.Default.BackColor = System.Drawing.SystemColors.Window;
            this.scintilla1.Styles.Default.Size = 9F;
            this.scintilla1.Styles.IndentGuide.Size = 9F;
            this.scintilla1.Styles.LastPredefined.Size = 9F;
            this.scintilla1.Styles.LineNumber.Size = 9F;
            this.scintilla1.Styles.Max.Size = 9F;
            this.scintilla1.TabIndex = 0;
            this.scintilla1.AutoCompleteAccepted += new System.EventHandler<ScintillaNET.AutoCompleteAcceptedEventArgs>(this.scintilla1_AutoCompleteAccepted);
            this.scintilla1.AutoCompleteCancelled += new System.EventHandler<ScintillaNET.NativeScintillaEventArgs>(this.scintilla1_AutoCompleteCancelled);
            this.scintilla1.CharAdded += new System.EventHandler<ScintillaNET.CharAddedEventArgs>(this.scintilla1_CharAdded);
            this.scintilla1.TextDeleted += new System.EventHandler<ScintillaNET.TextModifiedEventArgs>(this.scintilla1_TextDeleted);
            this.scintilla1.TextInserted += new System.EventHandler<ScintillaNET.TextModifiedEventArgs>(this.scintilla1_TextInserted);
            this.scintilla1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.scintilla1_KeyDown);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 468);
            this.Controls.Add(this.scintilla1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.scintilla1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ScintillaNET.Scintilla scintilla1;
    }
}

