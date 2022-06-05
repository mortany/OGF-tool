
namespace OGF_tool
{
    partial class OGF_Editor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OGF_Editor));
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.TabControl = new System.Windows.Forms.TabControl();
            this.TexturesPage = new System.Windows.Forms.TabPage();
            this.MotionRefsPage = new System.Windows.Forms.TabPage();
            this.CreateMotionRefsButton = new System.Windows.Forms.Button();
            this.MotionRefsBox = new System.Windows.Forms.RichTextBox();
            this.MotionPage = new System.Windows.Forms.TabPage();
            this.MotionBox = new System.Windows.Forms.RichTextBox();
            this.UserDataPage = new System.Windows.Forms.TabPage();
            this.CreateUserdataButton = new System.Windows.Forms.Button();
            this.UserDataBox = new System.Windows.Forms.RichTextBox();
            this.BoneNamesPage = new System.Windows.Forms.TabPage();
            this.BoneNamesBox = new System.Windows.Forms.RichTextBox();
            this.BoneParamsPage = new System.Windows.Forms.TabPage();
            this.MenuPanel = new System.Windows.Forms.MenuStrip();
            this.FileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LoadMenuParam = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveMenuParam = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.reloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.oGFInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusFile = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.BkpCheckBox = new System.Windows.Forms.CheckBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.TabControl.SuspendLayout();
            this.MotionRefsPage.SuspendLayout();
            this.MotionPage.SuspendLayout();
            this.UserDataPage.SuspendLayout();
            this.BoneNamesPage.SuspendLayout();
            this.MenuPanel.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "OGF |*.ogf";
            // 
            // TabControl
            // 
            this.TabControl.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.TabControl.Controls.Add(this.TexturesPage);
            this.TabControl.Controls.Add(this.MotionRefsPage);
            this.TabControl.Controls.Add(this.MotionPage);
            this.TabControl.Controls.Add(this.UserDataPage);
            this.TabControl.Controls.Add(this.BoneNamesPage);
            this.TabControl.Controls.Add(this.BoneParamsPage);
            this.TabControl.Location = new System.Drawing.Point(12, 27);
            this.TabControl.Multiline = true;
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(403, 285);
            this.TabControl.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.TabControl.TabIndex = 7;
            this.TabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            this.TabControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            // 
            // TexturesPage
            // 
            this.TexturesPage.AutoScroll = true;
            this.TexturesPage.Location = new System.Drawing.Point(4, 25);
            this.TexturesPage.Name = "TexturesPage";
            this.TexturesPage.Padding = new System.Windows.Forms.Padding(3);
            this.TexturesPage.Size = new System.Drawing.Size(395, 256);
            this.TexturesPage.TabIndex = 0;
            this.TexturesPage.Text = "Textures";
            this.TexturesPage.UseVisualStyleBackColor = true;
            // 
            // MotionRefsPage
            // 
            this.MotionRefsPage.Controls.Add(this.CreateMotionRefsButton);
            this.MotionRefsPage.Controls.Add(this.MotionRefsBox);
            this.MotionRefsPage.Location = new System.Drawing.Point(4, 25);
            this.MotionRefsPage.Name = "MotionRefsPage";
            this.MotionRefsPage.Padding = new System.Windows.Forms.Padding(3);
            this.MotionRefsPage.Size = new System.Drawing.Size(395, 256);
            this.MotionRefsPage.TabIndex = 1;
            this.MotionRefsPage.Text = "Motion Refs";
            this.MotionRefsPage.UseVisualStyleBackColor = true;
            // 
            // CreateMotionRefsButton
            // 
            this.CreateMotionRefsButton.Location = new System.Drawing.Point(6, 6);
            this.CreateMotionRefsButton.Name = "CreateMotionRefsButton";
            this.CreateMotionRefsButton.Size = new System.Drawing.Size(383, 244);
            this.CreateMotionRefsButton.TabIndex = 1;
            this.CreateMotionRefsButton.Text = "Create Motion Refs";
            this.CreateMotionRefsButton.UseVisualStyleBackColor = true;
            this.CreateMotionRefsButton.Click += new System.EventHandler(this.CreateMotionRefsButton_Click);
            // 
            // MotionRefsBox
            // 
            this.MotionRefsBox.DetectUrls = false;
            this.MotionRefsBox.Location = new System.Drawing.Point(0, 0);
            this.MotionRefsBox.Name = "MotionRefsBox";
            this.MotionRefsBox.Size = new System.Drawing.Size(395, 256);
            this.MotionRefsBox.TabIndex = 0;
            this.MotionRefsBox.Text = "";
            this.MotionRefsBox.WordWrap = false;
            // 
            // MotionPage
            // 
            this.MotionPage.Controls.Add(this.MotionBox);
            this.MotionPage.Location = new System.Drawing.Point(4, 25);
            this.MotionPage.Name = "MotionPage";
            this.MotionPage.Padding = new System.Windows.Forms.Padding(3);
            this.MotionPage.Size = new System.Drawing.Size(395, 256);
            this.MotionPage.TabIndex = 4;
            this.MotionPage.Text = "Motions";
            this.MotionPage.UseVisualStyleBackColor = true;
            // 
            // MotionBox
            // 
            this.MotionBox.Location = new System.Drawing.Point(0, 0);
            this.MotionBox.Name = "MotionBox";
            this.MotionBox.ReadOnly = true;
            this.MotionBox.Size = new System.Drawing.Size(395, 256);
            this.MotionBox.TabIndex = 2;
            this.MotionBox.Text = "";
            // 
            // UserDataPage
            // 
            this.UserDataPage.Controls.Add(this.CreateUserdataButton);
            this.UserDataPage.Controls.Add(this.UserDataBox);
            this.UserDataPage.Location = new System.Drawing.Point(4, 25);
            this.UserDataPage.Name = "UserDataPage";
            this.UserDataPage.Padding = new System.Windows.Forms.Padding(3);
            this.UserDataPage.Size = new System.Drawing.Size(395, 256);
            this.UserDataPage.TabIndex = 2;
            this.UserDataPage.Text = "UserData";
            this.UserDataPage.UseVisualStyleBackColor = true;
            // 
            // CreateUserdataButton
            // 
            this.CreateUserdataButton.Location = new System.Drawing.Point(6, 6);
            this.CreateUserdataButton.Name = "CreateUserdataButton";
            this.CreateUserdataButton.Size = new System.Drawing.Size(383, 244);
            this.CreateUserdataButton.TabIndex = 1;
            this.CreateUserdataButton.Text = "Create UserData";
            this.CreateUserdataButton.UseVisualStyleBackColor = true;
            this.CreateUserdataButton.Click += new System.EventHandler(this.CreateUserdataButton_Click);
            // 
            // UserDataBox
            // 
            this.UserDataBox.Location = new System.Drawing.Point(0, 0);
            this.UserDataBox.Name = "UserDataBox";
            this.UserDataBox.Size = new System.Drawing.Size(395, 256);
            this.UserDataBox.TabIndex = 0;
            this.UserDataBox.Text = "";
            // 
            // BoneNamesPage
            // 
            this.BoneNamesPage.Controls.Add(this.BoneNamesBox);
            this.BoneNamesPage.Location = new System.Drawing.Point(4, 25);
            this.BoneNamesPage.Name = "BoneNamesPage";
            this.BoneNamesPage.Padding = new System.Windows.Forms.Padding(3);
            this.BoneNamesPage.Size = new System.Drawing.Size(395, 256);
            this.BoneNamesPage.TabIndex = 5;
            this.BoneNamesPage.Text = "Bone Names";
            this.BoneNamesPage.UseVisualStyleBackColor = true;
            // 
            // BoneNamesBox
            // 
            this.BoneNamesBox.DetectUrls = false;
            this.BoneNamesBox.Location = new System.Drawing.Point(0, 0);
            this.BoneNamesBox.Name = "BoneNamesBox";
            this.BoneNamesBox.ReadOnly = true;
            this.BoneNamesBox.Size = new System.Drawing.Size(395, 256);
            this.BoneNamesBox.TabIndex = 1;
            this.BoneNamesBox.Text = "";
            this.BoneNamesBox.WordWrap = false;
            // 
            // BoneParamsPage
            // 
            this.BoneParamsPage.AutoScroll = true;
            this.BoneParamsPage.Location = new System.Drawing.Point(4, 25);
            this.BoneParamsPage.Name = "BoneParamsPage";
            this.BoneParamsPage.Padding = new System.Windows.Forms.Padding(3);
            this.BoneParamsPage.Size = new System.Drawing.Size(395, 256);
            this.BoneParamsPage.TabIndex = 3;
            this.BoneParamsPage.Text = "Bone Params";
            this.BoneParamsPage.UseVisualStyleBackColor = true;
            // 
            // MenuPanel
            // 
            this.MenuPanel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenuItem,
            this.oGFInfoToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.MenuPanel.Location = new System.Drawing.Point(0, 0);
            this.MenuPanel.Name = "MenuPanel";
            this.MenuPanel.Size = new System.Drawing.Size(427, 24);
            this.MenuPanel.TabIndex = 8;
            this.MenuPanel.Text = "menuStrip1";
            this.MenuPanel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LoadMenuParam,
            this.SaveMenuParam,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.reloadToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.FileMenuItem.Name = "FileMenuItem";
            this.FileMenuItem.Size = new System.Drawing.Size(37, 20);
            this.FileMenuItem.Text = "File";
            // 
            // LoadMenuParam
            // 
            this.LoadMenuParam.Name = "LoadMenuParam";
            this.LoadMenuParam.Size = new System.Drawing.Size(121, 22);
            this.LoadMenuParam.Text = "Load";
            this.LoadMenuParam.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // SaveMenuParam
            // 
            this.SaveMenuParam.Name = "SaveMenuParam";
            this.SaveMenuParam.Size = new System.Drawing.Size(121, 22);
            this.SaveMenuParam.Text = "Save";
            this.SaveMenuParam.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.saveAsToolStripMenuItem.Text = "Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(118, 6);
            // 
            // reloadToolStripMenuItem
            // 
            this.reloadToolStripMenuItem.Name = "reloadToolStripMenuItem";
            this.reloadToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.reloadToolStripMenuItem.Text = "Reload";
            this.reloadToolStripMenuItem.Click += new System.EventHandler(this.reloadToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // oGFInfoToolStripMenuItem
            // 
            this.oGFInfoToolStripMenuItem.Name = "oGFInfoToolStripMenuItem";
            this.oGFInfoToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.oGFInfoToolStripMenuItem.Text = "OGF Info";
            this.oGFInfoToolStripMenuItem.Click += new System.EventHandler(this.oGFInfoToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            this.viewToolStripMenuItem.Click += new System.EventHandler(this.viewToolStripMenuItem_Click);
            // 
            // FileLabel
            // 
            this.FileLabel.Name = "FileLabel";
            this.FileLabel.Size = new System.Drawing.Size(28, 17);
            this.FileLabel.Text = "File:";
            // 
            // StatusFile
            // 
            this.StatusFile.Name = "StatusFile";
            this.StatusFile.Size = new System.Drawing.Size(12, 17);
            this.StatusFile.Text = "-";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileLabel,
            this.StatusFile});
            this.statusStrip1.Location = new System.Drawing.Point(0, 315);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(427, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 30;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // BkpCheckBox
            // 
            this.BkpCheckBox.AutoSize = true;
            this.BkpCheckBox.Location = new System.Drawing.Point(319, 4);
            this.BkpCheckBox.Name = "BkpCheckBox";
            this.BkpCheckBox.Size = new System.Drawing.Size(96, 17);
            this.BkpCheckBox.TabIndex = 31;
            this.BkpCheckBox.Text = "Create backup";
            this.BkpCheckBox.UseVisualStyleBackColor = true;
            this.BkpCheckBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog1_FileOk);
            // 
            // OGF_Editor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(427, 337);
            this.Controls.Add(this.BkpCheckBox);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.MenuPanel);
            this.Controls.Add(this.TabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MenuPanel;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(443, 800);
            this.MinimumSize = new System.Drawing.Size(443, 251);
            this.Name = "OGF_Editor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OGF Params Editor v1.4";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.TabControl.ResumeLayout(false);
            this.MotionRefsPage.ResumeLayout(false);
            this.MotionPage.ResumeLayout(false);
            this.UserDataPage.ResumeLayout(false);
            this.BoneNamesPage.ResumeLayout(false);
            this.MenuPanel.ResumeLayout(false);
            this.MenuPanel.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.TabPage TexturesPage;
        private System.Windows.Forms.TabPage MotionRefsPage;
        private System.Windows.Forms.RichTextBox MotionRefsBox;
        private System.Windows.Forms.TabPage UserDataPage;
        private System.Windows.Forms.RichTextBox UserDataBox;
        private System.Windows.Forms.MenuStrip MenuPanel;
        private System.Windows.Forms.ToolStripMenuItem FileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveMenuParam;
        private System.Windows.Forms.ToolStripMenuItem LoadMenuParam;
        private System.Windows.Forms.ToolStripStatusLabel FileLabel;
        private System.Windows.Forms.ToolStripStatusLabel StatusFile;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripMenuItem oGFInfoToolStripMenuItem;
        private System.Windows.Forms.CheckBox BkpCheckBox;
        private System.Windows.Forms.TabPage BoneParamsPage;
        private System.Windows.Forms.TabPage MotionPage;
        private System.Windows.Forms.RichTextBox MotionBox;
        private System.Windows.Forms.TabPage BoneNamesPage;
        private System.Windows.Forms.RichTextBox BoneNamesBox;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.Button CreateUserdataButton;
        private System.Windows.Forms.Button CreateMotionRefsButton;
        private System.Windows.Forms.ToolStripMenuItem reloadToolStripMenuItem;
    }
}

