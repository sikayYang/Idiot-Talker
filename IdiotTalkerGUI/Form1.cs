using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdiotTalkerGUI
{
    public partial class IdiotTalker : Form
    {
        public IdiotTalker()
        {
            InitializeComponent();
            this.BackColor = Color.FromArgb(255, 218, 227, 212); 
            this.textBox1.BackColor= Color.FromArgb(255, 218, 227, 212); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Label label= new Label();
            label.AutoSize = true;
            label.Location = new System.Drawing.Point(49, 101);
            label.Text = textBox1.Text;
            label.Visible = true;
            label.BackColor = Color.Blue;
            label.Size = new Size(50,20);
            panel.Controls.Add(label);
            this.textBox1.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            string str = e.KeyChar.ToString();
            if (e.KeyChar == 13)
            {
                this.button1_Click(sender, e);
            }
        }
    }
}
