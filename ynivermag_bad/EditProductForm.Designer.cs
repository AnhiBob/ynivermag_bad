namespace ynivermag_bad
{
    partial class EditProductForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditProductForm));
            this.label1 = new System.Windows.Forms.Label();
            this.Name = new System.Windows.Forms.TextBox();
            this.CategoryCb = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.Price = new System.Windows.Forms.TextBox();
            this.Count = new System.Windows.Forms.TextBox();
            this.Back = new System.Windows.Forms.Button();
            this.EditProduct = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(128, 1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Редактировать";
            // 
            // Name
            // 
            this.Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.Name.Location = new System.Drawing.Point(147, 49);
            this.Name.Name = "Name";
            this.Name.Size = new System.Drawing.Size(292, 31);
            this.Name.TabIndex = 1;
            // 
            // CategoryCb
            // 
            this.CategoryCb.FormattingEnabled = true;
            this.CategoryCb.Location = new System.Drawing.Point(147, 177);
            this.CategoryCb.Name = "CategoryCb";
            this.CategoryCb.Size = new System.Drawing.Size(292, 32);
            this.CategoryCb.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 24);
            this.label2.TabIndex = 3;
            this.label2.Text = "Название";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 24);
            this.label3.TabIndex = 4;
            this.label3.Text = "Цена";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 135);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(117, 24);
            this.label4.TabIndex = 5;
            this.label4.Text = "Количество";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 179);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 24);
            this.label5.TabIndex = 6;
            this.label5.Text = "Категория";
            // 
            // Price
            // 
            this.Price.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.Price.Location = new System.Drawing.Point(147, 93);
            this.Price.Name = "Price";
            this.Price.Size = new System.Drawing.Size(292, 31);
            this.Price.TabIndex = 7;
            // 
            // Count
            // 
            this.Count.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.Count.Location = new System.Drawing.Point(147, 136);
            this.Count.Name = "Count";
            this.Count.Size = new System.Drawing.Size(292, 31);
            this.Count.TabIndex = 8;
            // 
            // Back
            // 
            this.Back.BackColor = System.Drawing.Color.LimeGreen;
            this.Back.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Back.Location = new System.Drawing.Point(16, 240);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(179, 46);
            this.Back.TabIndex = 9;
            this.Back.Text = "Отмена";
            this.Back.UseVisualStyleBackColor = false;
            this.Back.Click += new System.EventHandler(this.Back_Click);
            // 
            // EditProduct
            // 
            this.EditProduct.BackColor = System.Drawing.Color.GreenYellow;
            this.EditProduct.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.EditProduct.Location = new System.Drawing.Point(252, 240);
            this.EditProduct.Name = "EditProduct";
            this.EditProduct.Size = new System.Drawing.Size(188, 46);
            this.EditProduct.TabIndex = 10;
            this.EditProduct.Text = "Редактировать";
            this.EditProduct.UseVisualStyleBackColor = false;
            this.EditProduct.Click += new System.EventHandler(this.EditProduct_Click);
            // 
            // EditProductForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(471, 325);
            this.Controls.Add(this.EditProduct);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.Count);
            this.Controls.Add(this.Price);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CategoryCb);
            this.Controls.Add(this.Name);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EditProductForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox Name;
        private System.Windows.Forms.ComboBox CategoryCb;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox Price;
        private System.Windows.Forms.TextBox Count;
        private System.Windows.Forms.Button Back;
        private System.Windows.Forms.Button EditProduct;
    }
}