namespace ynivermag_bad
{
    partial class ShowReports
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowReports));
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.cmbSort = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.dtpFromDate = new System.Windows.Forms.DateTimePicker();
            this.dtpToDate = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dataGridViewOrders = new System.Windows.Forms.DataGridView();
            this.Report = new System.Windows.Forms.Button();
            this.InMenu = new System.Windows.Forms.Button();
            this.cmbStatusFilter = new System.Windows.Forms.ComboBox();
            this.cmbUserFilter = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lblRecordCount = new System.Windows.Forms.Label();
            this.lblTotalSum = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOrders)).BeginInit();
            this.SuspendLayout();
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.txtSearch.Location = new System.Drawing.Point(13, 39);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(325, 31);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // cmbSort
            // 
            this.cmbSort.FormattingEnabled = true;
            this.cmbSort.Location = new System.Drawing.Point(17, 116);
            this.cmbSort.Name = "cmbSort";
            this.cmbSort.Size = new System.Drawing.Size(321, 32);
            this.cmbSort.TabIndex = 1;
            this.cmbSort.TextChanged += new System.EventHandler(this.cmbSort_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "Поиск";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 24);
            this.label2.TabIndex = 3;
            this.label2.Text = "Сортировка";
            // 
            // dtpFromDate
            // 
            this.dtpFromDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.dtpFromDate.Location = new System.Drawing.Point(376, 114);
            this.dtpFromDate.Name = "dtpFromDate";
            this.dtpFromDate.Size = new System.Drawing.Size(200, 31);
            this.dtpFromDate.TabIndex = 4;
            this.dtpFromDate.ValueChanged += new System.EventHandler(this.dtpFromDate_ValueChanged);
            // 
            // dtpToDate
            // 
            this.dtpToDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.dtpToDate.Location = new System.Drawing.Point(609, 114);
            this.dtpToDate.Name = "dtpToDate";
            this.dtpToDate.Size = new System.Drawing.Size(200, 31);
            this.dtpToDate.TabIndex = 5;
            this.dtpToDate.ValueChanged += new System.EventHandler(this.dtpToDate_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(372, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 24);
            this.label3.TabIndex = 6;
            this.label3.Text = "Фильтрация";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.25F);
            this.label4.Location = new System.Drawing.Point(584, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(19, 25);
            this.label4.TabIndex = 7;
            this.label4.Text = "-";
            // 
            // dataGridViewOrders
            // 
            this.dataGridViewOrders.AllowUserToAddRows = false;
            this.dataGridViewOrders.AllowUserToDeleteRows = false;
            this.dataGridViewOrders.AllowUserToResizeColumns = false;
            this.dataGridViewOrders.AllowUserToResizeRows = false;
            this.dataGridViewOrders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOrders.Location = new System.Drawing.Point(13, 162);
            this.dataGridViewOrders.Name = "dataGridViewOrders";
            this.dataGridViewOrders.ReadOnly = true;
            this.dataGridViewOrders.Size = new System.Drawing.Size(1092, 463);
            this.dataGridViewOrders.TabIndex = 8;
            // 
            // Report
            // 
            this.Report.BackColor = System.Drawing.Color.GreenYellow;
            this.Report.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.Report.Location = new System.Drawing.Point(624, 650);
            this.Report.Name = "Report";
            this.Report.Size = new System.Drawing.Size(233, 53);
            this.Report.TabIndex = 9;
            this.Report.Text = "Сформировать отчёт";
            this.Report.UseVisualStyleBackColor = false;
            this.Report.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // InMenu
            // 
            this.InMenu.BackColor = System.Drawing.Color.LimeGreen;
            this.InMenu.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.InMenu.Location = new System.Drawing.Point(872, 650);
            this.InMenu.Name = "InMenu";
            this.InMenu.Size = new System.Drawing.Size(233, 53);
            this.InMenu.TabIndex = 10;
            this.InMenu.Text = "В меню";
            this.InMenu.UseVisualStyleBackColor = false;
            this.InMenu.Click += new System.EventHandler(this.InMenu_Click);
            // 
            // cmbStatusFilter
            // 
            this.cmbStatusFilter.FormattingEnabled = true;
            this.cmbStatusFilter.Location = new System.Drawing.Point(376, 38);
            this.cmbStatusFilter.Name = "cmbStatusFilter";
            this.cmbStatusFilter.Size = new System.Drawing.Size(282, 32);
            this.cmbStatusFilter.TabIndex = 11;
            this.cmbStatusFilter.TextChanged += new System.EventHandler(this.cmbStatusFilter_SelectedIndexChanged);
            // 
            // cmbUserFilter
            // 
            this.cmbUserFilter.FormattingEnabled = true;
            this.cmbUserFilter.Location = new System.Drawing.Point(664, 38);
            this.cmbUserFilter.Name = "cmbUserFilter";
            this.cmbUserFilter.Size = new System.Drawing.Size(282, 32);
            this.cmbUserFilter.TabIndex = 12;
            this.cmbUserFilter.TextChanged += new System.EventHandler(this.cmbUserFilter_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(372, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 24);
            this.label5.TabIndex = 13;
            this.label5.Text = "Фильтр";
            // 
            // lblRecordCount
            // 
            this.lblRecordCount.AutoSize = true;
            this.lblRecordCount.Location = new System.Drawing.Point(9, 641);
            this.lblRecordCount.Name = "lblRecordCount";
            this.lblRecordCount.Size = new System.Drawing.Size(84, 24);
            this.lblRecordCount.TabIndex = 14;
            this.lblRecordCount.Text = "Заказов";
            // 
            // lblTotalSum
            // 
            this.lblTotalSum.AutoSize = true;
            this.lblTotalSum.Location = new System.Drawing.Point(9, 665);
            this.lblTotalSum.Name = "lblTotalSum";
            this.lblTotalSum.Size = new System.Drawing.Size(84, 24);
            this.lblTotalSum.TabIndex = 15;
            this.lblTotalSum.Text = "Заказов";
            // 
            // ShowReports
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1117, 727);
            this.Controls.Add(this.lblTotalSum);
            this.Controls.Add(this.lblRecordCount);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbUserFilter);
            this.Controls.Add(this.cmbStatusFilter);
            this.Controls.Add(this.InMenu);
            this.Controls.Add(this.Report);
            this.Controls.Add(this.dataGridViewOrders);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dtpToDate);
            this.Controls.Add(this.dtpFromDate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbSort);
            this.Controls.Add(this.txtSearch);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShowReports";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ShowReports";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOrders)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ComboBox cmbSort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dataGridViewOrders;
        private System.Windows.Forms.Button Report;
        private System.Windows.Forms.Button InMenu;
        private System.Windows.Forms.ComboBox cmbStatusFilter;
        private System.Windows.Forms.ComboBox cmbUserFilter;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private System.Windows.Forms.Label lblRecordCount;
        private System.Windows.Forms.Label lblTotalSum;
    }
}