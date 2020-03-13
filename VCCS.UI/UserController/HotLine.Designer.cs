namespace VCCS.UI.UserController
{
    partial class HotLine
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblHotLine = new System.Windows.Forms.Label();
            this.lblSdt = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblHotLine
            // 
            this.lblHotLine.AutoSize = true;
            this.lblHotLine.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHotLine.Location = new System.Drawing.Point(76, 36);
            this.lblHotLine.Name = "lblHotLine";
            this.lblHotLine.Size = new System.Drawing.Size(57, 16);
            this.lblHotLine.TabIndex = 0;
            this.lblHotLine.Text = "Hot Line";
            // 
            // lblSdt
            // 
            this.lblSdt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSdt.Location = new System.Drawing.Point(50, 66);
            this.lblSdt.Name = "lblSdt";
            this.lblSdt.Size = new System.Drawing.Size(110, 23);
            this.lblSdt.TabIndex = 1;
            this.lblSdt.Text = "Hot Line";
            this.lblSdt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // HotLine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.Controls.Add(this.lblSdt);
            this.Controls.Add(this.lblHotLine);
            this.Name = "HotLine";
            this.Size = new System.Drawing.Size(219, 143);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblHotLine;
        private System.Windows.Forms.Label lblSdt;
    }
}
