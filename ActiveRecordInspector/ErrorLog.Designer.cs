namespace SindaSoft.ActiveRecordInspector
{
    partial class ErrorLog
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
            this.tbErrorTxt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbErrorTxt
            // 
            this.tbErrorTxt.Font = new System.Drawing.Font("Consolas", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.tbErrorTxt.Location = new System.Drawing.Point(25, 33);
            this.tbErrorTxt.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbErrorTxt.Multiline = true;
            this.tbErrorTxt.Name = "tbErrorTxt";
            this.tbErrorTxt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbErrorTxt.Size = new System.Drawing.Size(188, 208);
            this.tbErrorTxt.TabIndex = 0;
            // 
            // ErrorLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1066, 618);
            this.Controls.Add(this.tbErrorTxt);
            this.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ErrorLog";
            this.Text = "ErrorLog";
            this.Load += new System.EventHandler(this.ErrorLog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbErrorTxt;
    }
}