using System.Drawing;
using System.Windows.Forms;

namespace XSharp.Editor;

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
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmLevelEditor));
        tmrRender = new Timer(components);
        ssStatusBar = new StatusStrip();
        mnuMainMenu = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        newToolStripMenuItem = new ToolStripMenuItem();
        openToolStripMenuItem = new ToolStripMenuItem();
        saveToolStripMenuItem = new ToolStripMenuItem();
        saveAsToolStripMenuItem = new ToolStripMenuItem();
        closeToolStripMenuItem = new ToolStripMenuItem();
        editToolStripMenuItem = new ToolStripMenuItem();
        viewToolStripMenuItem = new ToolStripMenuItem();
        toolsToolStripMenuItem = new ToolStripMenuItem();
        windowToolStripMenuItem = new ToolStripMenuItem();
        helpToolStripMenuItem = new ToolStripMenuItem();
        pnlMain = new Panel();
        sdxRender = new SharpDX.Windows.RenderControl();
        pnlRight = new Panel();
        tcRight = new TabControl();
        tpTiles = new TabPage();
        sdxTiles = new SharpDX.Windows.RenderControl();
        tpEntities = new TabPage();
        lvEntities = new ListView();
        gbProperties = new GroupBox();
        pgProperties = new PropertyGrid();
        tbMain = new ToolStrip();
        btnPointer = new ToolStripButton();
        btnHand = new ToolStripButton();
        mnuMainMenu.SuspendLayout();
        pnlMain.SuspendLayout();
        pnlRight.SuspendLayout();
        tcRight.SuspendLayout();
        tpTiles.SuspendLayout();
        tpEntities.SuspendLayout();
        gbProperties.SuspendLayout();
        tbMain.SuspendLayout();
        SuspendLayout();
        // 
        // tmrRender
        // 
        tmrRender.Enabled = true;
        tmrRender.Interval = 15;
        tmrRender.Tick += timer1_Tick;
        // 
        // ssStatusBar
        // 
        ssStatusBar.Location = new Point(3, 953);
        ssStatusBar.Name = "ssStatusBar";
        ssStatusBar.Size = new Size(1257, 22);
        ssStatusBar.TabIndex = 2;
        ssStatusBar.Text = "statusStrip1";
        // 
        // mnuMainMenu
        // 
        mnuMainMenu.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, viewToolStripMenuItem, toolsToolStripMenuItem, windowToolStripMenuItem, helpToolStripMenuItem });
        mnuMainMenu.Location = new Point(3, 64);
        mnuMainMenu.Name = "mnuMainMenu";
        mnuMainMenu.Size = new Size(1257, 24);
        mnuMainMenu.TabIndex = 3;
        mnuMainMenu.Text = "menuStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, saveToolStripMenuItem, saveAsToolStripMenuItem, closeToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(37, 20);
        fileToolStripMenuItem.Text = "File";
        // 
        // newToolStripMenuItem
        // 
        newToolStripMenuItem.Name = "newToolStripMenuItem";
        newToolStripMenuItem.Size = new Size(121, 22);
        newToolStripMenuItem.Text = "New";
        // 
        // openToolStripMenuItem
        // 
        openToolStripMenuItem.Name = "openToolStripMenuItem";
        openToolStripMenuItem.Size = new Size(121, 22);
        openToolStripMenuItem.Text = "Open";
        // 
        // saveToolStripMenuItem
        // 
        saveToolStripMenuItem.Name = "saveToolStripMenuItem";
        saveToolStripMenuItem.Size = new Size(121, 22);
        saveToolStripMenuItem.Text = "Save";
        // 
        // saveAsToolStripMenuItem
        // 
        saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
        saveAsToolStripMenuItem.Size = new Size(121, 22);
        saveAsToolStripMenuItem.Text = "Save as...";
        // 
        // closeToolStripMenuItem
        // 
        closeToolStripMenuItem.Name = "closeToolStripMenuItem";
        closeToolStripMenuItem.Size = new Size(121, 22);
        closeToolStripMenuItem.Text = "Close";
        // 
        // editToolStripMenuItem
        // 
        editToolStripMenuItem.Name = "editToolStripMenuItem";
        editToolStripMenuItem.Size = new Size(39, 20);
        editToolStripMenuItem.Text = "Edit";
        // 
        // viewToolStripMenuItem
        // 
        viewToolStripMenuItem.Name = "viewToolStripMenuItem";
        viewToolStripMenuItem.Size = new Size(44, 20);
        viewToolStripMenuItem.Text = "View";
        // 
        // toolsToolStripMenuItem
        // 
        toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
        toolsToolStripMenuItem.Size = new Size(46, 20);
        toolsToolStripMenuItem.Text = "Tools";
        // 
        // windowToolStripMenuItem
        // 
        windowToolStripMenuItem.Name = "windowToolStripMenuItem";
        windowToolStripMenuItem.Size = new Size(63, 20);
        windowToolStripMenuItem.Text = "Window";
        // 
        // helpToolStripMenuItem
        // 
        helpToolStripMenuItem.Name = "helpToolStripMenuItem";
        helpToolStripMenuItem.Size = new Size(44, 20);
        helpToolStripMenuItem.Text = "Help";
        // 
        // pnlMain
        // 
        pnlMain.Controls.Add(sdxRender);
        pnlMain.Controls.Add(pnlRight);
        pnlMain.Dock = DockStyle.Fill;
        pnlMain.Location = new Point(3, 113);
        pnlMain.Name = "pnlMain";
        pnlMain.Size = new Size(1257, 840);
        pnlMain.TabIndex = 4;
        // 
        // sdxRender
        // 
        sdxRender.Dock = DockStyle.Fill;
        sdxRender.Location = new Point(0, 0);
        sdxRender.Name = "sdxRender";
        sdxRender.Size = new Size(960, 840);
        sdxRender.TabIndex = 2;
        sdxRender.MouseDown += sdxRender_MouseDown;
        sdxRender.MouseMove += sdxRender_MouseMove;
        sdxRender.MouseUp += sdxRender_MouseUp;
        // 
        // pnlRight
        // 
        pnlRight.Controls.Add(tcRight);
        pnlRight.Controls.Add(gbProperties);
        pnlRight.Dock = DockStyle.Right;
        pnlRight.Location = new Point(960, 0);
        pnlRight.Name = "pnlRight";
        pnlRight.Size = new Size(297, 840);
        pnlRight.TabIndex = 3;
        // 
        // tcRight
        // 
        tcRight.Controls.Add(tpTiles);
        tcRight.Controls.Add(tpEntities);
        tcRight.Dock = DockStyle.Fill;
        tcRight.Location = new Point(0, 0);
        tcRight.Name = "tcRight";
        tcRight.SelectedIndex = 0;
        tcRight.Size = new Size(297, 568);
        tcRight.TabIndex = 2;
        // 
        // tpTiles
        // 
        tpTiles.Controls.Add(sdxTiles);
        tpTiles.Location = new Point(4, 24);
        tpTiles.Name = "tpTiles";
        tpTiles.Padding = new Padding(3);
        tpTiles.Size = new Size(289, 540);
        tpTiles.TabIndex = 0;
        tpTiles.Text = "Tiles";
        tpTiles.UseVisualStyleBackColor = true;
        // 
        // sdxTiles
        // 
        sdxTiles.Dock = DockStyle.Fill;
        sdxTiles.Location = new Point(3, 3);
        sdxTiles.Name = "sdxTiles";
        sdxTiles.Size = new Size(283, 534);
        sdxTiles.TabIndex = 0;
        // 
        // tpEntities
        // 
        tpEntities.Controls.Add(lvEntities);
        tpEntities.Location = new Point(4, 24);
        tpEntities.Name = "tpEntities";
        tpEntities.Padding = new Padding(3);
        tpEntities.Size = new Size(289, 420);
        tpEntities.TabIndex = 1;
        tpEntities.Text = "Entities";
        tpEntities.UseVisualStyleBackColor = true;
        // 
        // lvEntities
        // 
        lvEntities.BackColor = SystemColors.ControlDarkDark;
        lvEntities.Dock = DockStyle.Fill;
        lvEntities.Location = new Point(3, 3);
        lvEntities.Name = "lvEntities";
        lvEntities.Size = new Size(283, 414);
        lvEntities.TabIndex = 0;
        lvEntities.UseCompatibleStateImageBehavior = false;
        // 
        // gbProperties
        // 
        gbProperties.Controls.Add(pgProperties);
        gbProperties.Dock = DockStyle.Bottom;
        gbProperties.Location = new Point(0, 568);
        gbProperties.Name = "gbProperties";
        gbProperties.Size = new Size(297, 272);
        gbProperties.TabIndex = 1;
        gbProperties.TabStop = false;
        gbProperties.Text = "Properties";
        // 
        // pgProperties
        // 
        pgProperties.Dock = DockStyle.Fill;
        pgProperties.Location = new Point(3, 19);
        pgProperties.Name = "pgProperties";
        pgProperties.Size = new Size(291, 250);
        pgProperties.TabIndex = 0;
        pgProperties.ViewBackColor = SystemColors.ControlDarkDark;
        // 
        // tbMain
        // 
        tbMain.Items.AddRange(new ToolStripItem[] { btnPointer, btnHand });
        tbMain.Location = new Point(3, 88);
        tbMain.Name = "tbMain";
        tbMain.Size = new Size(1257, 25);
        tbMain.TabIndex = 4;
        tbMain.Text = "toolStrip1";
        // 
        // btnPointer
        // 
        btnPointer.DisplayStyle = ToolStripItemDisplayStyle.Image;
        btnPointer.Image = (Image) resources.GetObject("btnPointer.Image");
        btnPointer.ImageTransparentColor = Color.Magenta;
        btnPointer.Name = "btnPointer";
        btnPointer.Size = new Size(23, 22);
        btnPointer.Text = "Pointer";
        btnPointer.Click += btnPointer_Click;
        // 
        // btnHand
        // 
        btnHand.DisplayStyle = ToolStripItemDisplayStyle.Image;
        btnHand.Image = (Image) resources.GetObject("btnHand.Image");
        btnHand.ImageTransparentColor = Color.Magenta;
        btnHand.Name = "btnHand";
        btnHand.Size = new Size(23, 22);
        btnHand.Text = "Hand";
        btnHand.ToolTipText = "Hand";
        btnHand.Click += btnHand_Click;
        // 
        // FrmLevelEditor
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.ControlDarkDark;
        ClientSize = new Size(1263, 978);
        Controls.Add(pnlMain);
        Controls.Add(ssStatusBar);
        Controls.Add(tbMain);
        Controls.Add(mnuMainMenu);
        MainMenuStrip = mnuMainMenu;
        Name = "FrmLevelEditor";
        Text = "X# Level Editor";
        FormClosed += FrmLevelEditor_FormClosed;
        Load += FrmLevelEditor_Load;
        mnuMainMenu.ResumeLayout(false);
        mnuMainMenu.PerformLayout();
        pnlMain.ResumeLayout(false);
        pnlRight.ResumeLayout(false);
        tcRight.ResumeLayout(false);
        tpTiles.ResumeLayout(false);
        tpEntities.ResumeLayout(false);
        gbProperties.ResumeLayout(false);
        tbMain.ResumeLayout(false);
        tbMain.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private Timer tmrRender;
    private StatusStrip ssStatusBar;
    private MenuStrip mnuMainMenu;
    private Panel pnlMain;
    private SharpDX.Windows.RenderControl sdxRender;
    private Panel pnlRight;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem newToolStripMenuItem;
    private ToolStripMenuItem openToolStripMenuItem;
    private ToolStripMenuItem saveToolStripMenuItem;
    private ToolStripMenuItem saveAsToolStripMenuItem;
    private ToolStripMenuItem closeToolStripMenuItem;
    private ToolStripMenuItem editToolStripMenuItem;
    private ToolStripMenuItem viewToolStripMenuItem;
    private ToolStripMenuItem toolsToolStripMenuItem;
    private ToolStripMenuItem windowToolStripMenuItem;
    private ToolStripMenuItem helpToolStripMenuItem;
    private ToolStrip tbMain;
    private GroupBox gbProperties;
    private TabControl tcRight;
    private TabPage tpTiles;
    private TabPage tpEntities;
    private ListView lvEntities;
    private SharpDX.Windows.RenderControl sdxTiles;
    private ToolStripButton btnPointer;
    private ToolStripButton btnHand;
    private PropertyGrid pgProperties;
}