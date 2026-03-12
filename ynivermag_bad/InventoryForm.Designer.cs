namespace ynivermag_bad
{
    partial class InventoryForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InventoryForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageReceive = new System.Windows.Forms.TabPage();
            this.InMenu = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.lblReceiveTotal = new System.Windows.Forms.Label();
            this.dataGridViewReceiveCart = new System.Windows.Forms.DataGridView();
            this.addProduct = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.numericReceive = new System.Windows.Forms.NumericUpDown();
            this.btnReceive = new System.Windows.Forms.Button();
            this.btnClearReceive = new System.Windows.Forms.Button();
            this.dataGridViewReceiveSearch = new System.Windows.Forms.DataGridView();
            this.txtSearchReceive = new System.Windows.Forms.TextBox();
            this.tabPageWriteOff = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblWriteOffTotal = new System.Windows.Forms.Label();
            this.dataGridViewWriteOffCart = new System.Windows.Forms.DataGridView();
            this.dataGridViewWriteOffSearch = new System.Windows.Forms.DataGridView();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.comboReason = new System.Windows.Forms.ComboBox();
            this.txtSearchWriteOff = new System.Windows.Forms.TextBox();
            this.numericWriteOff = new System.Windows.Forms.NumericUpDown();
            this.tabPageHistory = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.lblTotalRecords = new System.Windows.Forms.Label();
            this.dataGridViewHistory = new System.Windows.Forms.DataGridView();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPageReceive.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewReceiveCart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericReceive)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewReceiveSearch)).BeginInit();
            this.tabPageWriteOff.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewWriteOffCart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewWriteOffSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWriteOff)).BeginInit();
            this.tabPageHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewHistory)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageReceive);
            this.tabControl1.Controls.Add(this.tabPageWriteOff);
            this.tabControl1.Controls.Add(this.tabPageHistory);
            this.tabControl1.Location = new System.Drawing.Point(14, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1616, 723);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageReceive
            // 
            this.tabPageReceive.Controls.Add(this.InMenu);
            this.tabPageReceive.Controls.Add(this.label5);
            this.tabPageReceive.Controls.Add(this.lblReceiveTotal);
            this.tabPageReceive.Controls.Add(this.dataGridViewReceiveCart);
            this.tabPageReceive.Controls.Add(this.addProduct);
            this.tabPageReceive.Controls.Add(this.label1);
            this.tabPageReceive.Controls.Add(this.numericReceive);
            this.tabPageReceive.Controls.Add(this.btnReceive);
            this.tabPageReceive.Controls.Add(this.btnClearReceive);
            this.tabPageReceive.Controls.Add(this.dataGridViewReceiveSearch);
            this.tabPageReceive.Controls.Add(this.txtSearchReceive);
            this.tabPageReceive.Location = new System.Drawing.Point(4, 33);
            this.tabPageReceive.Name = "tabPageReceive";
            this.tabPageReceive.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageReceive.Size = new System.Drawing.Size(1608, 686);
            this.tabPageReceive.TabIndex = 0;
            this.tabPageReceive.Text = "Приёмка";
            this.tabPageReceive.UseVisualStyleBackColor = true;
            // 
            // InMenu
            // 
            this.InMenu.BackColor = System.Drawing.Color.LimeGreen;
            this.InMenu.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenu.Location = new System.Drawing.Point(6, 577);
            this.InMenu.Name = "InMenu";
            this.InMenu.Size = new System.Drawing.Size(224, 52);
            this.InMenu.TabIndex = 16;
            this.InMenu.Text = "В меню";
            this.InMenu.UseVisualStyleBackColor = false;
            this.InMenu.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(275, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(117, 24);
            this.label5.TabIndex = 15;
            this.label5.Text = "Количество";
            // 
            // lblReceiveTotal
            // 
            this.lblReceiveTotal.AutoSize = true;
            this.lblReceiveTotal.Location = new System.Drawing.Point(759, 584);
            this.lblReceiveTotal.Name = "lblReceiveTotal";
            this.lblReceiveTotal.Size = new System.Drawing.Size(60, 24);
            this.lblReceiveTotal.TabIndex = 14;
            this.lblReceiveTotal.Text = "label2";
            // 
            // dataGridViewReceiveCart
            // 
            this.dataGridViewReceiveCart.AllowUserToAddRows = false;
            this.dataGridViewReceiveCart.AllowUserToDeleteRows = false;
            this.dataGridViewReceiveCart.AllowUserToResizeColumns = false;
            this.dataGridViewReceiveCart.AllowUserToResizeRows = false;
            this.dataGridViewReceiveCart.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewReceiveCart.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewReceiveCart.Location = new System.Drawing.Point(869, 73);
            this.dataGridViewReceiveCart.Name = "dataGridViewReceiveCart";
            this.dataGridViewReceiveCart.Size = new System.Drawing.Size(723, 498);
            this.dataGridViewReceiveCart.TabIndex = 13;
            this.dataGridViewReceiveCart.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewReceiveCart_CellClick);
            this.dataGridViewReceiveCart.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.DataGridViewReceiveCart_CellValidating);
            // 
            // addProduct
            // 
            this.addProduct.Location = new System.Drawing.Point(410, 37);
            this.addProduct.Name = "addProduct";
            this.addProduct.Size = new System.Drawing.Size(224, 30);
            this.addProduct.TabIndex = 11;
            this.addProduct.Text = "Добавить товар";
            this.addProduct.UseVisualStyleBackColor = true;
            this.addProduct.Click += new System.EventHandler(this.addProduct_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 24);
            this.label1.TabIndex = 10;
            this.label1.Text = "Найти товар";
            // 
            // numericReceive
            // 
            this.numericReceive.Location = new System.Drawing.Point(279, 37);
            this.numericReceive.Name = "numericReceive";
            this.numericReceive.Size = new System.Drawing.Size(113, 29);
            this.numericReceive.TabIndex = 6;
            this.numericReceive.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // btnReceive
            // 
            this.btnReceive.BackColor = System.Drawing.Color.GreenYellow;
            this.btnReceive.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnReceive.Location = new System.Drawing.Point(1368, 584);
            this.btnReceive.Name = "btnReceive";
            this.btnReceive.Size = new System.Drawing.Size(224, 52);
            this.btnReceive.TabIndex = 3;
            this.btnReceive.Text = "Оформить приёмку";
            this.btnReceive.UseVisualStyleBackColor = false;
            this.btnReceive.Click += new System.EventHandler(this.btnReceiveProcess_Click);
            // 
            // btnClearReceive
            // 
            this.btnClearReceive.BackColor = System.Drawing.Color.GreenYellow;
            this.btnClearReceive.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnClearReceive.Location = new System.Drawing.Point(1138, 584);
            this.btnClearReceive.Name = "btnClearReceive";
            this.btnClearReceive.Size = new System.Drawing.Size(224, 52);
            this.btnClearReceive.TabIndex = 2;
            this.btnClearReceive.Text = "Очистить";
            this.btnClearReceive.UseVisualStyleBackColor = false;
            this.btnClearReceive.Click += new System.EventHandler(this.btnReceiveClear_Click);
            // 
            // dataGridViewReceiveSearch
            // 
            this.dataGridViewReceiveSearch.AllowUserToAddRows = false;
            this.dataGridViewReceiveSearch.AllowUserToDeleteRows = false;
            this.dataGridViewReceiveSearch.AllowUserToResizeColumns = false;
            this.dataGridViewReceiveSearch.AllowUserToResizeRows = false;
            this.dataGridViewReceiveSearch.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewReceiveSearch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewReceiveSearch.Location = new System.Drawing.Point(6, 73);
            this.dataGridViewReceiveSearch.Name = "dataGridViewReceiveSearch";
            this.dataGridViewReceiveSearch.ReadOnly = true;
            this.dataGridViewReceiveSearch.Size = new System.Drawing.Size(825, 498);
            this.dataGridViewReceiveSearch.TabIndex = 1;
            this.dataGridViewReceiveSearch.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewReceiveSearch_CellClick);
            // 
            // txtSearchReceive
            // 
            this.txtSearchReceive.Location = new System.Drawing.Point(6, 37);
            this.txtSearchReceive.Name = "txtSearchReceive";
            this.txtSearchReceive.Size = new System.Drawing.Size(267, 29);
            this.txtSearchReceive.TabIndex = 0;
            this.txtSearchReceive.TextChanged += new System.EventHandler(this.TxtSearchReceive_TextChanged);
            // 
            // tabPageWriteOff
            // 
            this.tabPageWriteOff.Controls.Add(this.button2);
            this.tabPageWriteOff.Controls.Add(this.label4);
            this.tabPageWriteOff.Controls.Add(this.label3);
            this.tabPageWriteOff.Controls.Add(this.label2);
            this.tabPageWriteOff.Controls.Add(this.lblWriteOffTotal);
            this.tabPageWriteOff.Controls.Add(this.dataGridViewWriteOffCart);
            this.tabPageWriteOff.Controls.Add(this.dataGridViewWriteOffSearch);
            this.tabPageWriteOff.Controls.Add(this.button4);
            this.tabPageWriteOff.Controls.Add(this.button3);
            this.tabPageWriteOff.Controls.Add(this.comboReason);
            this.tabPageWriteOff.Controls.Add(this.txtSearchWriteOff);
            this.tabPageWriteOff.Controls.Add(this.numericWriteOff);
            this.tabPageWriteOff.Location = new System.Drawing.Point(4, 33);
            this.tabPageWriteOff.Name = "tabPageWriteOff";
            this.tabPageWriteOff.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageWriteOff.Size = new System.Drawing.Size(1608, 686);
            this.tabPageWriteOff.TabIndex = 1;
            this.tabPageWriteOff.Text = "Списание";
            this.tabPageWriteOff.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.LimeGreen;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Location = new System.Drawing.Point(8, 587);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(245, 49);
            this.button2.TabIndex = 15;
            this.button2.Text = "В меню";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(464, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 24);
            this.label4.TabIndex = 14;
            this.label4.Text = "Причина";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(330, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 24);
            this.label3.TabIndex = 13;
            this.label3.Text = "Количество";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 24);
            this.label2.TabIndex = 12;
            this.label2.Text = "Найти товар";
            // 
            // lblWriteOffTotal
            // 
            this.lblWriteOffTotal.AutoSize = true;
            this.lblWriteOffTotal.Location = new System.Drawing.Point(853, 45);
            this.lblWriteOffTotal.Name = "lblWriteOffTotal";
            this.lblWriteOffTotal.Size = new System.Drawing.Size(60, 24);
            this.lblWriteOffTotal.TabIndex = 11;
            this.lblWriteOffTotal.Text = "label3";
            // 
            // dataGridViewWriteOffCart
            // 
            this.dataGridViewWriteOffCart.AllowUserToAddRows = false;
            this.dataGridViewWriteOffCart.AllowUserToDeleteRows = false;
            this.dataGridViewWriteOffCart.AllowUserToResizeColumns = false;
            this.dataGridViewWriteOffCart.AllowUserToResizeRows = false;
            this.dataGridViewWriteOffCart.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewWriteOffCart.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dataGridViewWriteOffCart.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewWriteOffCart.Location = new System.Drawing.Point(857, 72);
            this.dataGridViewWriteOffCart.Name = "dataGridViewWriteOffCart";
            this.dataGridViewWriteOffCart.Size = new System.Drawing.Size(723, 498);
            this.dataGridViewWriteOffCart.TabIndex = 10;
            this.dataGridViewWriteOffCart.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewWriteOffCart_CellClick);
            this.dataGridViewWriteOffCart.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.DataGridViewWriteOffCart_CellValidating);
            // 
            // dataGridViewWriteOffSearch
            // 
            this.dataGridViewWriteOffSearch.AllowUserToAddRows = false;
            this.dataGridViewWriteOffSearch.AllowUserToDeleteRows = false;
            this.dataGridViewWriteOffSearch.AllowUserToResizeColumns = false;
            this.dataGridViewWriteOffSearch.AllowUserToResizeRows = false;
            this.dataGridViewWriteOffSearch.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewWriteOffSearch.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewWriteOffSearch.Location = new System.Drawing.Point(8, 72);
            this.dataGridViewWriteOffSearch.Name = "dataGridViewWriteOffSearch";
            this.dataGridViewWriteOffSearch.ReadOnly = true;
            this.dataGridViewWriteOffSearch.Size = new System.Drawing.Size(825, 498);
            this.dataGridViewWriteOffSearch.TabIndex = 5;
            this.dataGridViewWriteOffSearch.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewWriteOffSearch_CellClick);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.GreenYellow;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4.Location = new System.Drawing.Point(1353, 576);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(227, 49);
            this.button4.TabIndex = 4;
            this.button4.Text = "Списание";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.btnWriteOffProcess_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.GreenYellow;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.Location = new System.Drawing.Point(1119, 578);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(228, 47);
            this.button3.TabIndex = 3;
            this.button3.Text = "Очистить";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.btnWriteOffClear_Click);
            // 
            // comboReason
            // 
            this.comboReason.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboReason.FormattingEnabled = true;
            this.comboReason.Location = new System.Drawing.Point(468, 35);
            this.comboReason.Name = "comboReason";
            this.comboReason.Size = new System.Drawing.Size(209, 32);
            this.comboReason.TabIndex = 2;
            // 
            // txtSearchWriteOff
            // 
            this.txtSearchWriteOff.Location = new System.Drawing.Point(6, 36);
            this.txtSearchWriteOff.Name = "txtSearchWriteOff";
            this.txtSearchWriteOff.Size = new System.Drawing.Size(322, 29);
            this.txtSearchWriteOff.TabIndex = 1;
            this.txtSearchWriteOff.TextChanged += new System.EventHandler(this.TxtSearchWriteOff_TextChanged);
            // 
            // numericWriteOff
            // 
            this.numericWriteOff.Location = new System.Drawing.Point(334, 36);
            this.numericWriteOff.Name = "numericWriteOff";
            this.numericWriteOff.Size = new System.Drawing.Size(120, 29);
            this.numericWriteOff.TabIndex = 0;
            this.numericWriteOff.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // tabPageHistory
            // 
            this.tabPageHistory.Controls.Add(this.button1);
            this.tabPageHistory.Controls.Add(this.label6);
            this.tabPageHistory.Controls.Add(this.lblTotalRecords);
            this.tabPageHistory.Controls.Add(this.dataGridViewHistory);
            this.tabPageHistory.Location = new System.Drawing.Point(4, 33);
            this.tabPageHistory.Name = "tabPageHistory";
            this.tabPageHistory.Size = new System.Drawing.Size(1608, 686);
            this.tabPageHistory.TabIndex = 2;
            this.tabPageHistory.Text = "История";
            this.tabPageHistory.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.LimeGreen;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(10, 629);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(234, 46);
            this.button1.TabIndex = 7;
            this.button1.Text = "В меню";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 26.25F);
            this.label6.Location = new System.Drawing.Point(561, 14);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(327, 39);
            this.label6.TabIndex = 6;
            this.label6.Text = "Просмотр истории ";
            // 
            // lblTotalRecords
            // 
            this.lblTotalRecords.AutoSize = true;
            this.lblTotalRecords.Location = new System.Drawing.Point(10, 586);
            this.lblTotalRecords.Name = "lblTotalRecords";
            this.lblTotalRecords.Size = new System.Drawing.Size(60, 24);
            this.lblTotalRecords.TabIndex = 5;
            this.lblTotalRecords.Text = "label6";
            // 
            // dataGridViewHistory
            // 
            this.dataGridViewHistory.AllowUserToAddRows = false;
            this.dataGridViewHistory.AllowUserToDeleteRows = false;
            this.dataGridViewHistory.AllowUserToResizeColumns = false;
            this.dataGridViewHistory.AllowUserToResizeRows = false;
            this.dataGridViewHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewHistory.Location = new System.Drawing.Point(14, 81);
            this.dataGridViewHistory.Name = "dataGridViewHistory";
            this.dataGridViewHistory.ReadOnly = true;
            this.dataGridViewHistory.Size = new System.Drawing.Size(1559, 502);
            this.dataGridViewHistory.TabIndex = 4;
            // 
            // InventoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1637, 748);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InventoryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Форма приёма и списания";
            this.tabControl1.ResumeLayout(false);
            this.tabPageReceive.ResumeLayout(false);
            this.tabPageReceive.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewReceiveCart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericReceive)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewReceiveSearch)).EndInit();
            this.tabPageWriteOff.ResumeLayout(false);
            this.tabPageWriteOff.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewWriteOffCart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewWriteOffSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericWriteOff)).EndInit();
            this.tabPageHistory.ResumeLayout(false);
            this.tabPageHistory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewHistory)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageReceive;
        private System.Windows.Forms.TabPage tabPageWriteOff;
        private System.Windows.Forms.DataGridView dataGridViewReceiveSearch;
        private System.Windows.Forms.TextBox txtSearchReceive;
        private System.Windows.Forms.Button btnReceive;
        private System.Windows.Forms.Button btnClearReceive;
        private System.Windows.Forms.NumericUpDown numericReceive;
        private System.Windows.Forms.NumericUpDown numericWriteOff;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ComboBox comboReason;
        private System.Windows.Forms.TextBox txtSearchWriteOff;
        private System.Windows.Forms.TabPage tabPageHistory;
        private System.Windows.Forms.DataGridView dataGridViewHistory;
        private System.Windows.Forms.DataGridView dataGridViewWriteOffSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button addProduct;
        private System.Windows.Forms.DataGridView dataGridViewReceiveCart;
        private System.Windows.Forms.DataGridView dataGridViewWriteOffCart;
        private System.Windows.Forms.Label lblReceiveTotal;
        private System.Windows.Forms.Label lblWriteOffTotal;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblTotalRecords;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button InMenu;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}