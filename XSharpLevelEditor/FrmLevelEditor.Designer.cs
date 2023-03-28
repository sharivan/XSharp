using System.Drawing;
using System.Windows.Forms;

namespace XSharpLevelEditor;

partial class FrmLevelEditor
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        renderControl1 = new SharpDX.Windows.RenderControl();
        timer1 = new Timer(components);
        SuspendLayout();
        // 
        // renderControl1
        // 
        renderControl1.Location = new Point(1, -1);
        renderControl1.Name = "renderControl1";
        renderControl1.Size = new Size(1019, 649);
        renderControl1.TabIndex = 0;
        // 
        // timer1
        // 
        timer1.Enabled = true;
        timer1.Interval = 15;
        timer1.Tick += timer1_Tick;
        // 
        // FrmLevelEditor
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1264, 681);
        Controls.Add(renderControl1);
        DoubleBuffered = true;
        Name = "FrmLevelEditor";
        Text = "X# Level Editor";
        FormClosed += FrmLevelEditor_FormClosed;
        Load += FrmLevelEditor_Load;
        ResumeLayout(false);
    }

    #endregion

    private SharpDX.Windows.RenderControl renderControl1;
    private Timer timer1;
}