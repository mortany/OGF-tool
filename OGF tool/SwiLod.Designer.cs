namespace OGF_tool
{
    partial class SwiLod
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
            this.label1 = new System.Windows.Forms.Label();
            this.LodBar = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.LodBar)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select lod factor: ";
            // 
            // LodBar
            // 
            this.LodBar.Location = new System.Drawing.Point(102, 12);
            this.LodBar.Maximum = 100;
            this.LodBar.Name = "LodBar";
            this.LodBar.Size = new System.Drawing.Size(199, 45);
            this.LodBar.TabIndex = 2;
            this.LodBar.ValueChanged += new System.EventHandler(this.LodNum_ValueChanged);
            // 
            // SwiLod
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(308, 51);
            this.Controls.Add(this.LodBar);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SwiLod";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SwiLod";
            ((System.ComponentModel.ISupportInitialize)(this.LodBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar LodBar;
    }
}