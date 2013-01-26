using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
namespace LuaEditor
{
    class ToolTip : System.Windows.Forms.Form
    {
        private Label label1;
        private IWin32Window m_owner;
        public void ShowToolTip(int x, int y, string message) {
            label1.Text = message;
            Location = new Point(x, y);
            if(!Visible)
            Show(m_owner);
        }

        public void setText(string text) {
            label1.Text = text;
        }
        
        public ToolTip(IWin32Window owner)
    {
        m_owner = owner;
        this.FormBorderStyle = FormBorderStyle.None;
        this.ShowInTaskbar = false;
        this.StartPosition = FormStartPosition.Manual;
        this.BackColor = Color.LightYellow;
        InitializeComponent();
    }
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.MaximumSize = new System.Drawing.Size(400, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(2);
            this.label1.Size = new System.Drawing.Size(39, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            this.label1.Resize += new System.EventHandler(this.label1_Resize);
            // 
            // ToolTip
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ToolTip";
            this.Load += new System.EventHandler(this.ToolTip_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void label1_Resize(object sender, EventArgs e)
        {
            Size = label1.Size;
        }

        private void ToolTip_Load(object sender, EventArgs e)
        {

        }
   
    }
}
