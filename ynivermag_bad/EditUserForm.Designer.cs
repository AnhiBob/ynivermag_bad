namespace ynivermag_bad
{
    partial class EditUserForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditUserForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.FirstName = new System.Windows.Forms.TextBox();
            this.RoleCb = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.LastName = new System.Windows.Forms.TextBox();
            this.Login = new System.Windows.Forms.TextBox();
            this.Password = new System.Windows.Forms.TextBox();
            this.Email = new System.Windows.Forms.TextBox();
            this.Back = new System.Windows.Forms.Button();
            this.EditUser = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(117, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(161, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Редактирование";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "Имя";
            // 
            // FirstName
            // 
            this.FirstName.Location = new System.Drawing.Point(110, 43);
            this.FirstName.Name = "FirstName";
            this.FirstName.Size = new System.Drawing.Size(292, 29);
            this.FirstName.TabIndex = 2;
            this.FirstName.TextChanged += new System.EventHandler(this.FirstName_TextChanged);
            // 
            // RoleCb
            // 
            this.RoleCb.FormattingEnabled = true;
            this.RoleCb.Location = new System.Drawing.Point(110, 256);
            this.RoleCb.Name = "RoleCb";
            this.RoleCb.Size = new System.Drawing.Size(292, 32);
            this.RoleCb.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(91, 24);
            this.label3.TabIndex = 4;
            this.label3.Text = "Фамилия";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 24);
            this.label4.TabIndex = 5;
            this.label4.Text = "Логин";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 169);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 24);
            this.label5.TabIndex = 6;
            this.label5.Text = "Пароль";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 216);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 24);
            this.label6.TabIndex = 7;
            this.label6.Text = "Почта";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 256);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(54, 24);
            this.label7.TabIndex = 8;
            this.label7.Text = "Роль";
            // 
            // LastName
            // 
            this.LastName.Location = new System.Drawing.Point(110, 87);
            this.LastName.Name = "LastName";
            this.LastName.Size = new System.Drawing.Size(292, 29);
            this.LastName.TabIndex = 9;
            this.LastName.TextChanged += new System.EventHandler(this.LastName_TextChanged);
            // 
            // Login
            // 
            this.Login.Location = new System.Drawing.Point(110, 129);
            this.Login.Name = "Login";
            this.Login.Size = new System.Drawing.Size(292, 29);
            this.Login.TabIndex = 10;
            this.Login.TextChanged += new System.EventHandler(this.Login_TextChanged);
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(110, 169);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(292, 29);
            this.Password.TabIndex = 11;
            this.Password.TextChanged += new System.EventHandler(this.Password_TextChanged);
            // 
            // Email
            // 
            this.Email.Location = new System.Drawing.Point(110, 216);
            this.Email.Name = "Email";
            this.Email.Size = new System.Drawing.Size(292, 29);
            this.Email.TabIndex = 12;
            this.Email.TextChanged += new System.EventHandler(this.Email_TextChanged);
            // 
            // Back
            // 
            this.Back.BackColor = System.Drawing.Color.LimeGreen;
            this.Back.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Back.Location = new System.Drawing.Point(13, 310);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(166, 48);
            this.Back.TabIndex = 13;
            this.Back.Text = "Отмена";
            this.Back.UseVisualStyleBackColor = false;
            this.Back.Click += new System.EventHandler(this.Back_Click);
            // 
            // EditUser
            // 
            this.EditUser.BackColor = System.Drawing.Color.Chartreuse;
            this.EditUser.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.EditUser.Location = new System.Drawing.Point(231, 310);
            this.EditUser.Name = "EditUser";
            this.EditUser.Size = new System.Drawing.Size(171, 48);
            this.EditUser.TabIndex = 14;
            this.EditUser.Text = "Редактировать";
            this.EditUser.UseVisualStyleBackColor = false;
            this.EditUser.Click += new System.EventHandler(this.EditUser_Click);
            // 
            // EditUserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(431, 383);
            this.Controls.Add(this.EditUser);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.Email);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.Login);
            this.Controls.Add(this.LastName);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.RoleCb);
            this.Controls.Add(this.FirstName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditUserForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EditUserForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox FirstName;
        private System.Windows.Forms.ComboBox RoleCb;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox LastName;
        private System.Windows.Forms.TextBox Login;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.TextBox Email;
        private System.Windows.Forms.Button Back;
        private System.Windows.Forms.Button EditUser;
    }
}