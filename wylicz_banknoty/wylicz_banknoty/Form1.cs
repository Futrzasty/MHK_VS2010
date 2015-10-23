using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace wylicz_banknoty
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double kwota1, l200, l100, l50, l20, l10, l5, l2, l1, l50g, l20g, l10g, l5g, l2g, l1g;

            kwota1 = Double.Parse(viKwota.Text);
            l200 = Math.Floor(kwota1 / 200);
            kwota1 -= l200 * 200;
            kwota1 = Math.Round(kwota1, 2);

            l100 = Math.Floor(kwota1 / 100);
            kwota1 -= l100 * 100;
            kwota1 = Math.Round(kwota1, 2);

            l50 = Math.Floor(kwota1 / 50);
            kwota1 -= l50 * 50;
            kwota1 = Math.Round(kwota1, 2);

            l20 = Math.Floor(kwota1 / 20);
            kwota1 -= l20 * 20;
            kwota1 = Math.Round(kwota1, 2);

            l10 = Math.Floor(kwota1 / 10);
            kwota1 -= l10 * 10;
            kwota1 = Math.Round(kwota1, 2);

            l5 = Math.Floor(kwota1 / 5);
            kwota1 -= l5 * 5;
            kwota1 = Math.Round(kwota1, 2);

            l2 = Math.Floor(kwota1 / 2);
            kwota1 -= l2 * 2;
            kwota1 = Math.Round(kwota1, 2);

            l1 = Math.Floor(kwota1 / 1);
            kwota1 -= l1 * 1;
            kwota1 = Math.Round(kwota1, 2);

            l50g = Math.Floor(kwota1 / 0.50);
            kwota1 -= l50g * 0.50;
            kwota1 = Math.Round(kwota1, 2);

            l20g = Math.Floor(kwota1 / 0.20);
            kwota1 -= l20g * 0.20;
            kwota1 = Math.Round(kwota1, 2);

            l10g = Math.Floor(kwota1 / 0.10);
            kwota1 -= l10g * 0.10;
            kwota1 = Math.Round(kwota1, 2);

            l5g = Math.Floor(kwota1 / 0.05);
            kwota1 -= l5g * 0.05;
            kwota1 = Math.Round(kwota1, 2);

            l2g = Math.Floor(kwota1 / 0.02);
            kwota1 -= l2g * 0.02;
            kwota1 = Math.Round(kwota1, 2);

            l1g = Math.Floor(kwota1 / 0.01);
            kwota1 -= l1g * 0.01;
            kwota1 = Math.Round(kwota1, 2);

            //viCount200.Text = kwota1.ToString("C");
            viCount200.Text = l200.ToString() + " 200";
            viCount100.Text = l100.ToString() + " 100";
            viCount50.Text = l50.ToString() + " 50";
            viCount20.Text = l20.ToString() + " 20";
            viCount10.Text = l10.ToString() + " 10";
            viCount5.Text = l5.ToString() + " 5";
            viCount2.Text = l2.ToString() + " 2";
            viCount1.Text = l1.ToString() + " 1";

            viCount50g.Text = l50g.ToString() + " 50g";
            viCount20g.Text = l20g.ToString() + " 20g";
            viCount10g.Text = l10g.ToString() + " 10g";
            viCount5g.Text = l5g.ToString() + " 5g";
            viCount2g.Text = l2g.ToString() + " 2g";
            viCount1g.Text = l1g.ToString() + " 1g";
        }
    }
}
