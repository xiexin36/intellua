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
        public int Duration { get; set; }

        public ToolTip(int x, int y, int width, int height, string message, int duration)
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.ShowInTaskbar = false;
        this.Width = width;
        this.Height = height;
        this.Duration = duration;
        this.Location = new Point(x, y);
        this.StartPosition = FormStartPosition.Manual;
        this.BackColor = Color.LightYellow;

        Label label = new Label();
        label.Text = message;
        label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        label.Dock = DockStyle.Fill;

        this.Padding = new Padding(5);
        this.Controls.Add(label);
    }
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }
    protected override void OnShown(System.EventArgs e)
    {
        base.OnShown(e);

        TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();

        Task.Factory.StartNew(() => CloseAfter(this.Duration, ui));
    }

    private void CloseAfter(int duration, TaskScheduler ui)
    {
        Thread.Sleep(duration * 1000);

        Form form = this;

        Task.Factory.StartNew(
            () => form.Close(),
            CancellationToken.None,
            TaskCreationOptions.None,
            ui);
    }
    }
}
