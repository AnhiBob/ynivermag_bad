namespace ynivermag_bad
{
    partial class ShowAll
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowAll));
            this.label1 = new System.Windows.Forms.Label();
            this.FIOlb = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.AddClient = new System.Windows.Forms.Button();
            this.dataGridViewClient = new System.Windows.Forms.DataGridView();
            this.InMenuClient = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.InMenuProduct = new System.Windows.Forms.Button();
            this.AddProduct = new System.Windows.Forms.Button();
            this.dataGridViewProduct = new System.Windows.Forms.DataGridView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.AddUser = new System.Windows.Forms.Button();
            this.dataGridViewUser = new System.Windows.Forms.DataGridView();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewClient)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewProduct)).BeginInit();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUser)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 30.25F);
            this.label1.Location = new System.Drawing.Point(344, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(228, 47);
            this.label1.TabIndex = 0;
            this.label1.Text = "Универмаг";
            // 
            // FIOlb
            // 
            this.FIOlb.AutoSize = true;
            this.FIOlb.Location = new System.Drawing.Point(17, 21);
            this.FIOlb.Name = "FIOlb";
            this.FIOlb.Size = new System.Drawing.Size(54, 24);
            this.FIOlb.TabIndex = 1;
            this.FIOlb.Text = "Роль";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(17, 56);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1063, 588);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.AddClient);
            this.tabPage1.Controls.Add(this.dataGridViewClient);
            this.tabPage1.Controls.Add(this.InMenuClient);
            this.tabPage1.Location = new System.Drawing.Point(4, 33);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1055, 551);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Клиента";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // AddClient
            // 
            this.AddClient.BackColor = System.Drawing.Color.GreenYellow;
            this.AddClient.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddClient.Location = new System.Drawing.Point(7, 484);
            this.AddClient.Name = "AddClient";
            this.AddClient.Size = new System.Drawing.Size(242, 46);
            this.AddClient.TabIndex = 6;
            this.AddClient.Text = "Добавить";
            this.AddClient.UseVisualStyleBackColor = false;
            this.AddClient.Click += new System.EventHandler(this.AddClient_Click);
            // 
            // dataGridViewClient
            // 
            this.dataGridViewClient.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewClient.Location = new System.Drawing.Point(7, 7);
            this.dataGridViewClient.Name = "dataGridViewClient";
            this.dataGridViewClient.Size = new System.Drawing.Size(1042, 457);
            this.dataGridViewClient.TabIndex = 5;
            this.dataGridViewClient.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dataGridViewClient_MouseClick);
            // 
            // InMenuClient
            // 
            this.InMenuClient.BackColor = System.Drawing.Color.LimeGreen;
            this.InMenuClient.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuClient.Location = new System.Drawing.Point(807, 484);
            this.InMenuClient.Name = "InMenuClient";
            this.InMenuClient.Size = new System.Drawing.Size(242, 46);
            this.InMenuClient.TabIndex = 4;
            this.InMenuClient.Text = "В меню";
            this.InMenuClient.UseVisualStyleBackColor = false;
            this.InMenuClient.Click += new System.EventHandler(this.InMenuClient_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.InMenuProduct);
            this.tabPage2.Controls.Add(this.AddProduct);
            this.tabPage2.Controls.Add(this.dataGridViewProduct);
            this.tabPage2.Location = new System.Drawing.Point(4, 33);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1055, 551);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Товары";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // InMenuProduct
            // 
            this.InMenuProduct.BackColor = System.Drawing.Color.LimeGreen;
            this.InMenuProduct.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenuProduct.Location = new System.Drawing.Point(807, 484);
            this.InMenuProduct.Name = "InMenuProduct";
            this.InMenuProduct.Size = new System.Drawing.Size(242, 46);
            this.InMenuProduct.TabIndex = 2;
            this.InMenuProduct.Text = "В меню";
            this.InMenuProduct.UseVisualStyleBackColor = false;
            this.InMenuProduct.Click += new System.EventHandler(this.InMenuClient_Click);
            // 
            // AddProduct
            // 
            this.AddProduct.BackColor = System.Drawing.Color.GreenYellow;
            this.AddProduct.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddProduct.Location = new System.Drawing.Point(7, 484);
            this.AddProduct.Name = "AddProduct";
            this.AddProduct.Size = new System.Drawing.Size(242, 46);
            this.AddProduct.TabIndex = 1;
            this.AddProduct.Text = "Добавить";
            this.AddProduct.UseVisualStyleBackColor = false;
            this.AddProduct.Click += new System.EventHandler(this.AddProduct_Click);
            // 
            // dataGridViewProduct
            // 
            this.dataGridViewProduct.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewProduct.Location = new System.Drawing.Point(7, 7);
            this.dataGridViewProduct.Name = "dataGridViewProduct";
            this.dataGridViewProduct.Size = new System.Drawing.Size(1042, 457);
            this.dataGridViewProduct.TabIndex = 0;
            this.dataGridViewProduct.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dataGridViewProduct_MouseClick);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.button2);
            this.tabPage3.Controls.Add(this.AddUser);
            this.tabPage3.Controls.Add(this.dataGridViewUser);
            this.tabPage3.Location = new System.Drawing.Point(4, 33);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1055, 551);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Пользователи";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.LimeGreen;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Location = new System.Drawing.Point(807, 484);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(242, 46);
            this.button2.TabIndex = 2;
            this.button2.Text = "В меню";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.InMenuClient_Click);
            // 
            // AddUser
            // 
            this.AddUser.BackColor = System.Drawing.Color.GreenYellow;
            this.AddUser.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.AddUser.Location = new System.Drawing.Point(7, 484);
            this.AddUser.Name = "AddUser";
            this.AddUser.Size = new System.Drawing.Size(242, 46);
            this.AddUser.TabIndex = 1;
            this.AddUser.Text = "Добавить";
            this.AddUser.UseVisualStyleBackColor = false;
            this.AddUser.Click += new System.EventHandler(this.AddUser_Click);
            // 
            // dataGridViewUser
            // 
            this.dataGridViewUser.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewUser.Location = new System.Drawing.Point(7, 7);
            this.dataGridViewUser.Name = "dataGridViewUser";
            this.dataGridViewUser.Size = new System.Drawing.Size(1042, 457);
            this.dataGridViewUser.TabIndex = 0;
            this.dataGridViewUser.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dataGridViewUser_MouseClick);
            // 
            // ShowAll
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1092, 656);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.FIOlb);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShowAll";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Просмотр";
            this.Load += new System.EventHandler(this.ShowAll_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewClient)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewProduct)).EndInit();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewUser)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label FIOlb;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button InMenuClient;
        private System.Windows.Forms.DataGridView dataGridViewClient;
        private System.Windows.Forms.Button InMenuProduct;
        private System.Windows.Forms.Button AddProduct;
        private System.Windows.Forms.DataGridView dataGridViewProduct;
        private System.Windows.Forms.Button AddUser;
        private System.Windows.Forms.DataGridView dataGridViewUser;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button AddClient;
    }
}