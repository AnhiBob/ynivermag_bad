using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ynivermag_bad
{
    public partial class MenuAdminForm : Form
    {
        private string _fio;
        public MenuAdminForm(string FIO)
        {
            InitializeComponent();
            _fio = FIO;
        }

        private void Lists_Click(object sender, EventArgs e)
        {
            ShowAll showall = new ShowAll(_fio, 1);
            showall.Show();
            this.Hide();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
