namespace ynivermag_bad
{
    partial class MenuTovarovedForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MenuTovarovedForm));
            this.label1 = new System.Windows.Forms.Label();
            this.FIOlb = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.Report = new System.Windows.Forms.Button();
            this.Lists = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 30.25F);
            this.label1.Location = new System.Drawing.Point(472, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(228, 47);
            this.label1.TabIndex = 0;
            this.label1.Text = "Универмаг";
            // 
            // FIOlb
            // 
            this.FIOlb.AutoSize = true;
            this.FIOlb.Location = new System.Drawing.Point(10, 33);
            this.FIOlb.Name = "FIOlb";
            this.FIOlb.Size = new System.Drawing.Size(49, 20);
            this.FIOlb.TabIndex = 1;
            this.FIOlb.Text = "роль";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.GreenYellow;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(883, 88);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(229, 72);
            this.button1.TabIndex = 3;
            this.button1.Text = "Запись";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // Report
            // 
            this.Report.BackColor = System.Drawing.Color.GreenYellow;
            this.Report.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Report.Location = new System.Drawing.Point(883, 166);
            this.Report.Name = "Report";
            this.Report.Size = new System.Drawing.Size(229, 72);
            this.Report.TabIndex = 4;
            this.Report.Text = "Отчёт";
            this.Report.UseVisualStyleBackColor = false;
            this.Report.Click += new System.EventHandler(this.Report_Click);
            // 
            // Lists
            // 
            this.Lists.BackColor = System.Drawing.Color.GreenYellow;
            this.Lists.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Lists.Location = new System.Drawing.Point(883, 244);
            this.Lists.Name = "Lists";
            this.Lists.Size = new System.Drawing.Size(229, 72);
            this.Lists.TabIndex = 5;
            this.Lists.Text = "Справочники";
            this.Lists.UseVisualStyleBackColor = false;
            this.Lists.Click += new System.EventHandler(this.Lists_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.LimeGreen;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4.Location = new System.Drawing.Point(883, 483);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(229, 72);
            this.button4.TabIndex = 6;
            this.button4.Text = "Выйти из аккаунта";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.FIOlb);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.groupBox1.Location = new System.Drawing.Point(12, 9);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(161, 73);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Товаровед";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::ynivermag_bad.Properties.Resources.лого;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Location = new System.Drawing.Point(12, 88);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(851, 467);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // MenuTovarovedForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1124, 565);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.Lists);
            this.Controls.Add(this.Report);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "MenuTovarovedForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Меню товароведа";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label FIOlb;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button Report;
        private System.Windows.Forms.Button Lists;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}