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
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.Lists = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 30.25F);
            this.label1.Location = new System.Drawing.Point(463, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(228, 47);
            this.label1.TabIndex = 0;
            this.label1.Text = "Универмаг";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "роль";
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::ynivermag_bad.Properties.Resources.лого;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panel1.Location = new System.Drawing.Point(16, 97);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(877, 493);
            this.panel1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.GreenYellow;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(920, 97);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(192, 66);
            this.button1.TabIndex = 3;
            this.button1.Text = "Запись";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.GreenYellow;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Location = new System.Drawing.Point(920, 169);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(192, 66);
            this.button2.TabIndex = 4;
            this.button2.Text = "Отчёт";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // Lists
            // 
            this.Lists.BackColor = System.Drawing.Color.GreenYellow;
            this.Lists.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Lists.Location = new System.Drawing.Point(920, 241);
            this.Lists.Name = "Lists";
            this.Lists.Size = new System.Drawing.Size(192, 66);
            this.Lists.TabIndex = 5;
            this.Lists.Text = "Справочники";
            this.Lists.UseVisualStyleBackColor = false;
            this.Lists.Click += new System.EventHandler(this.Lists_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.LimeGreen;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4.Location = new System.Drawing.Point(920, 524);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(192, 66);
            this.button4.TabIndex = 6;
            this.button4.Text = "Выйти из аккаунта";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // MenuTovarovedForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1124, 602);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.Lists);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "MenuTovarovedForm";
            this.Text = "MenuTovarovedForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button Lists;
        private System.Windows.Forms.Button button4;
    }
}