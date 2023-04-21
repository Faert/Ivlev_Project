using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Security.Policy;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;

namespace Ivlev_Project
{
    public partial class Form1 : Form
    {
        Methods methods = new Methods();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MinimumSize = new System.Drawing.Size(this.Width, this.Height);
            this.MaximumSize = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            chart1.Series[0].Points.Clear();
            chart1.Series[1].Points.Clear();
            chart1.Series[2].Points.Clear();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            methods.param[0] = Convert.ToDouble(textBox1.Text);
            methods.param[1] = Convert.ToDouble(textBox2.Text);
            methods.param[2] = Convert.ToDouble(textBox3.Text);
            methods.param[3] = Convert.ToDouble(textBox4.Text);

            methods.x[0] = Convert.ToDouble(textBox5.Text);
            methods.x[1] = Convert.ToDouble(textBox6.Text);

            methods.r = Convert.ToDouble(textBox8.Text);
            methods.er = Convert.ToDouble(textBox9.Text);
            methods.N = Convert.ToDouble(textBox10.Text);

            if (methods.x[1] <= methods.x[0])
            {
                methods.x[1] = methods.x[0] + 1;
            }
            methods.old_x = methods.x[0];
            methods.new_x = methods.x[1];

            methods.z[0] = methods.F(methods.x[0]);
            methods.z[1] = methods.F(methods.x[1]);

            chart1.Series[0].Points.Clear();
            for (double graph_x = methods.x[0];  graph_x < methods.x[1]; graph_x += (methods.x[1] - methods.x[0])/1000)
            {
                double graph_y = methods.F(graph_x);
                chart1.Series[0].Points.AddXY(graph_x, graph_y);
            }

            Methods temp = new Methods();

            if (checkedListBox1.CheckedItems.Contains("Метод перебора"))
            {
                temp = new Enumeration(methods);
            }

            if (checkedListBox1.CheckedItems.Contains("Метод Пиявского"))
            {
                temp = new Piyavsky(methods);
            }

            if (checkedListBox1.CheckedItems.Contains("Метод Стронгина"))
            {
                temp = new Strongin(methods);
            }

            double temp_f = methods.F(temp.new_x);
            textBox11.Text = string.Format("{0:E3}", temp_f);
            textBox12.Text = string.Format("{0:E3}", temp.new_x);
            chart1.Series[2].Points.Clear();
            chart1.Series[2].Points.AddXY(temp.new_x, temp_f);
            textBox13.Text = Convert.ToString(temp.i);

            chart1.Series[1].Points.Clear();
            foreach (double item_x in temp.x)
            {
                chart1.Series[1].Points.AddXY(item_x, chart1.Series[0].Points.FindMinByValue().YValues[0]);
            }
        }
    }

    public class Methods
    {
        public double[] param = { 0, 0, 0, 0 };
        public List<double> x = new List<double>() { 0, 1 };
        public List<double> z = new List<double>() { 0, 0 };
        public double old_x = 0;
        public double new_x = 1;
        public double r = 1;
        public double er = 0.1;
        public double N = 100;
        public int i = 0;

        public Methods() { }

        public void Order_Add(double x_)
        {
            int index = this.x.BinarySearch(x_);
            if (index < 0) index = ~index;
            this.x.Insert(index, x_);
            this.z.Insert(index, F(x_));
        }

        public double F(double x)
        {
            return param[0] * Math.Sin(param[1] * x) +
                    param[2] * Math.Cos(param[3] * x);
        }
    }

    public class Enumeration : Methods
    {
        public Enumeration(Methods methods)
        {
            param = methods.param;
            x = new List<double>(methods.x);
            z = new List<double>(methods.z);
            old_x = methods.old_x;
            new_x = methods.new_x;
            r = methods.r;
            er = methods.er;
            N = methods.N;
            i = methods.i;

            calculate();
        }

        double R(int index)
        {
            return x[index] - x[index - 1];
        }

        double x_(int index)
        {
            return 0.5*(x[index] + x[index - 1]);
        }

        void calculate()
        {
            while (i < N && Math.Abs(new_x - old_x) >= er)
            {
                List<double> R = new List<double>();

                old_x = new_x;

                for(int j = 1; j < x.Count; j++)
                {
                    R.Add(this.R(j));
                }

                new_x = x_(R.IndexOf(R.Max()) + 1);
                Order_Add(new_x);

                i++;
            }

            int temp_i = z.IndexOf(z.Min());
            new_x = x[temp_i];
        }
    }

    public class Piyavsky : Methods
    {
        public Piyavsky(Methods methods)
        {
            param = methods.param;
            x = new List<double>(methods.x);
            z = new List<double>(methods.z);
            old_x = methods.old_x;
            new_x = methods.new_x;
            r = methods.r;
            er = methods.er;
            N = methods.N;
            i = methods.i;

            calculate();
        }

        double R(int index)
        {
            return 0.5 * (r * (x[index] - x[index - 1]) - (z[index] + z[index-1]));
        }

        double x_(int index)
        {
            return 0.5 * ((x[index] + x[index - 1]) - (z[index] - z[index - 1])/r);
        }

        void calculate()
        {
            while (i < N && Math.Abs(new_x - old_x) >= er)
            {
                List<double> R = new List<double>();

                old_x = new_x;

                for (int j = 1; j < x.Count; j++)
                {
                    R.Add(this.R(j));
                }

                new_x = x_(R.IndexOf(R.Max()) + 1);
                Order_Add(new_x);

                i++;
            }
        }
    }

    public class Strongin : Methods
    {
        double m = 1;

        public Strongin(Methods methods)
        {
            param = methods.param;
            x = new List<double>(methods.x);
            z = new List<double>(methods.z);
            old_x = methods.old_x;
            new_x = methods.new_x;
            r = methods.r;
            er = methods.er;
            N = methods.N;
            i = methods.i;

            calculate();
        }

        double M()
        {
            double M = 0;
            for (int j = 1; j < x.Count; j++)
            {
                double temp_M = (Math.Abs(z[j]- z[j-1])) / (x[j] - x[j-1]);
                M = Math.Max(M, temp_M);
            }
            
            if(M > 0)
            {
                return r * M;
            }
            else
            {
                return 1;
            }
        }

        double R(int index)
        {
            return m * (x[index] - x[index - 1]) + Math.Pow(z[index] - z[index-1], 2) / (m* (x[index] - x[index - 1]))
                - 2*(z[index] + z[index-1]);
        }

        double x_(int index)
        {
            return 0.5 * ((x[index] + x[index - 1]) - (z[index] - z[index - 1]) / m);
        }

        void calculate()
        {
            while (i < N && Math.Abs(new_x - old_x) >= er)
            {
                m = M();

                List<double> R = new List<double>();

                old_x = new_x;

                for (int j = 1; j < x.Count; j++)
                {
                    R.Add(this.R(j));
                }

                new_x = x_(R.IndexOf(R.Max()) + 1);
                Order_Add(new_x);

                i++;
            }
        }
    }
}
