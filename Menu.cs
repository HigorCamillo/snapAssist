using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace snapAssist
{
    public partial class Menu : Form
    {
        Form FormOpen = null;
        public Menu()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cliente cl = new Cliente();

            OpenForm(cl);
        }
        private void OpenForm(Form NewForm)
        {
            try
            {
                if (FormOpen != null)
                {
                    FormOpen.Close();
                    FormOpen.Dispose();
                }
                FormOpen = NewForm;
                FormOpen.Show();
            }
            catch (Exception ex) { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Suporte cl = new Suporte();

            OpenForm(cl);
        }

    }
}
