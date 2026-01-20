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
    public partial class MenuTovarovedForm : Form
    {
        private string _fio;
        public MenuTovarovedForm(string FIO)
        {
            InitializeComponent();
            _fio = FIO;
            FIOlb.Text = _fio;
        }

        private void Lists_Click(object sender, EventArgs e)
        {
            ShowAll showall = new ShowAll(_fio, 3);
            showall.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
