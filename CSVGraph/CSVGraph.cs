using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows;
using System.Net;

namespace CSVGraph
{
    public partial class CSVGraph : Form
    {
        LinkedList<PointF> calculationAreaPoints;
        string workingFileInfo = "";

        public CSVGraph(string filePath = null)
        {
            InitializeComponent();
            this.chart1.MouseClick +=chart1_MouseClick;
            this.chart1.MouseDoubleClick +=chart1_MouseDoubleClick;
            this.comboBox1.DrawItem +=comboBox1_DrawItem;
            this.comboBox1.SelectedIndexChanged +=comboBox1_SelectedIndexChanged;
            this.listView1.ItemChecked +=listView1_ItemChecked;
            this.button1.Click +=button1_Click;
            this.textBox2.KeyDown += textBox2_KeyDown;
            this.listView2.DoubleClick += listView2_DoubleClick;
            this.Text = "Quick CSV Grapher";
            if (filePath != null)
            {
                workingFileInfo = textBox1.Text.Split('\\').Last();
                AddData(new StreamReader(File.OpenRead(@filePath)));
            }
            else
            {
                //AddData(new StreamReader(File.OpenRead(@"C:\\Lift1_EGV001_2015-11-05_133317.CSV")));
            }                  
        }

        void listView2_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine(listView2.SelectedItems[0].Text);
                string adress = "ftp://" + textBox2.Text + "/" + listView2.SelectedItems[0].Text;
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(adress);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential("ftpUser", "egvFTP");

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                workingFileInfo = adress;
                AddData(reader);
                tabControl1.SelectedIndex = 0;
            }
            catch (SystemException e1)
            {
                MessageBox.Show("Connection error!");
            }
        }

        void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                button2_Click(null,null);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "CSV Files|*.csv";
            openFileDialog1.Title = "Select a CSVFile";

            // Show the Dialog.
            // If the user clicked OK in the dialog and
            // a .CUR file was selected, open it.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Assign the cursor in the Stream to the Form's Cursor property.
                textBox1.Text = openFileDialog1.InitialDirectory + openFileDialog1.FileName;
                workingFileInfo = textBox1.Text.Split('\\').Last();
                AddData(new StreamReader(File.OpenRead(@textBox1.Text)));
            }
            
        }

        private void AddData(StreamReader csvStream)
        {
            var reader = csvStream; // new StreamReader(File.OpenRead(@filePath));
            List<string> listA = new List<string>();
            List<string> listB = new List<string>();
            Boolean first = true;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (first)
                {
                    chart1.Series.Clear();
                    listView1.Items.Clear();
                    listView1.Columns[0].Text = "Legend";
                    listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                    foreach (var name in values)
                    {
                        if (name != values[0])
                        {
                            chart1.Series.Add(name);
                            chart1.Series[name].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                            chart1.Series[name].ToolTip = "#VALX, #VALY";
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < chart1.Series.Count; i++)
                    {
                        chart1.Series[i].Points.AddXY(Convert.ToDouble(values[0]), Convert.ToDouble(values[1 + i]));
                    }
                }
                first = false;
            }

            chart1.ApplyPaletteColors();
            comboBox1.Items.Clear();
            foreach (Series item in chart1.Series){
                listView1.Items.Add(@item.Name);
                listView1.Items[listView1.Items.Count-1].BackColor = item.Color;
                comboBox1.Items.Add(@item.Name);
            }
            listView1.Items[0].Checked = true;
            comboBox1.SelectedIndex = 0;
            tabControl1.SelectedIndex = 0;
            this.Text = workingFileInfo;
        }

        private void listView1_ItemChecked(object sender, System.Windows.Forms.ItemCheckedEventArgs e)
        {
            chart1.Series[e.Item.Text].Enabled = e.Item.Checked;
            chart1.ChartAreas[0].RecalculateAxesScale();           
        }

        private void chart1_MouseDoubleClick(object sender, System.EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs me = (System.Windows.Forms.MouseEventArgs)e;
            if (me.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                //chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(0); 
            }
            else
            {
                chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(0); 
            }

        }

        private void chart1_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            System.Windows.Forms.MouseEventArgs me = (System.Windows.Forms.MouseEventArgs)e;
            if (me.Button == System.Windows.Forms.MouseButtons.Right)
            {
                float pX = (float)chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
                float pY = (float)chart1.ChartAreas[0].AxisY.PixelPositionToValue(e.Y); 
                addCross(pX, pY);
            }
        }

        private void addCross(float x, float y){
            if (calculationAreaPoints == null)
            {
                calculationAreaPoints = new LinkedList<PointF>();
                calculationAreaPoints.AddLast(new PointF(x, y));

                chart1.Series.Add("cross1");
                chart1.Series["cross1"].ChartType = SeriesChartType.FastLine;

            }
            else if(calculationAreaPoints.Count == 1)
            {
                calculationAreaPoints.AddLast(new PointF(x, y));

                chart1.Series["cross1"].Points.AddXY(calculationAreaPoints.First().X, calculationAreaPoints.First().Y);
                chart1.Series["cross1"].Points.AddXY(x, calculationAreaPoints.First().Y);
                chart1.Series["cross1"].Points.AddXY(x, y);
                chart1.Series["cross1"].Points.AddXY(calculationAreaPoints.First().X, y);
                chart1.Series["cross1"].Points.AddXY(calculationAreaPoints.First().X, calculationAreaPoints.First().Y);
                calculateData();
            }
            else
            {
                if (chart1.Series.FindByName("cross1") == null)
                    return;
                chart1.Series["cross1"].Points.Clear();
                chart1.Series.Remove(chart1.Series["cross1"]);
                calculationAreaPoints = null;
            }
        }

        private void comboBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                // Draw the background 
                e.DrawBackground();

                // Get the item text    
                string text = ((ComboBox)sender).Items[e.Index].ToString();

                e.Graphics.FillRectangle(new SolidBrush(listView1.Items[e.Index].BackColor), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                e.Graphics.DrawString(text, ((Control)sender).Font, Brushes.Black, e.Bounds.X, e.Bounds.Y);
            }
            catch (SystemException e1)
            {
                MessageBox.Show("Program error, sorry!");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.BackColor = listView1.Items[comboBox1.SelectedIndex].BackColor;
            calculateData();
        }

        private void calculateData()
        {
            if (calculationAreaPoints != null)
            {
                if (calculationAreaPoints.Count == 2)
                {
                    string item;
                    double deltax = calculationAreaPoints.Last().X - calculationAreaPoints.First().X;
                    double deltay = calculationAreaPoints.Last().Y - calculationAreaPoints.First().Y;
                    double yTotal = 0;
                    int pointCount = 0;
                    //set deltas
                    label3.Text = "DELTA Y = " + Math.Round(deltay, 2);
                    label4.Text = "DELTA X = " + Math.Round(deltax, 2);
                    label2.Text = "Slope = " + Math.Round((deltay / deltax), 2);

                    double x1 = Math.Min(calculationAreaPoints.First().X, calculationAreaPoints.Last().X);
                    double x2 = Math.Max(calculationAreaPoints.First().X, calculationAreaPoints.Last().X);
                    double y1 = Math.Min(calculationAreaPoints.First().Y, calculationAreaPoints.Last().Y);
                    double y2 = Math.Max(calculationAreaPoints.First().Y, calculationAreaPoints.Last().Y);
                    if (comboBox1.SelectedIndex != -1)
                    {
                        item = (string)comboBox1.SelectedItem;
                        foreach (var point in chart1.Series[item].Points)
                        {
                            if (point.XValue >= x1 && point.XValue <= x2 &&
                                point.YValues[0] >= y1 && point.YValues[0] <= y2)
                            {
                                yTotal += point.YValues[0];
                                pointCount++;
                            }
                        }
                    }
                    if (pointCount != 0)
                    {
                        label1.Text = "Average Y = " + Math.Round((yTotal / pointCount), 2);
                    }
                    else
                    {
                        label1.Text = "Average Y = NA ";
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string adress = "ftp://" + textBox2.Text + "/";
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(adress);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential("ftpUser","egvFTP");

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string line;
                string[] lineParts;
                listView2.Items.Clear();
                while ((line = reader.ReadLine()) != null)
                {
                    if(line.EndsWith(".CSV")){
                        lineParts = line.Split(' ');
                        listView2.Items.Add(lineParts[lineParts.Length - 1]);
                    }
                }

                Console.WriteLine("Directory List Complete, status {0}", response.StatusDescription);

                reader.Close();
                response.Close();
            }
            catch(SystemException e1)
            {
                MessageBox.Show("Connection error!");
            }
            
        }

    }

}
