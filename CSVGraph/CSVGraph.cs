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
        string dataCharArea = "";
        string dataCharAreaPre = "";
        ContextMenu legendContectMenu;
        ContextMenu chartAreaContectMenu;
        int addDataCounter = 0;

        public CSVGraph(string[] arguments = null)
        {
            InitializeComponent();
            legendContectMenu = createLegendContextMenu();
            this.mainChart.MouseClick +=mainChart_MouseClick;
            this.mainChart.MouseDoubleClick +=mainChart_MouseDoubleClick;
            this.calculationItemComboBox.DrawItem +=calculationItemComboBox_DrawItem;
            this.calculationItemComboBox.SelectedIndexChanged +=calculationItemComboBox_SelectedIndexChanged;
            this.legendListView.MouseClick += legendListView_MouseClick;
            this.legendListView.ItemChecked +=legendListView_ItemChecked;
            this.dataBrowseButton.Click +=dataBrowseButton_Click;
            this.dataIPTextBox.KeyDown += dataIPTextBox_KeyDown;
            this.dataRemoteList.DoubleClick += dataRemoteList_DoubleClick;
            this.Text = "Quick CSV Grapher";
            if (arguments != null)
            {
                foreach(string filepath in arguments)
                {
                    AddData(new StreamReader(File.OpenRead(@filepath)), filepath.Split('\\').Last());
                }
            }
            else
            {
                //AddData(new StreamReader(File.OpenRead(@"C:\\Lift1_EGV001_2015-11-05_133317.CSV")));
            }                  
        }

        ContextMenu createLegendContextMenu()
        {
            ContextMenu tempContextMenu = new ContextMenu();

            MenuItem mnuItemNewChart = new MenuItem();
            mnuItemNewChart.Text = "New Chart";
            mnuItemNewChart.Click +=mnuItemNewChart_Click;
            MenuItem mnuItemLineWidth = new MenuItem();
            mnuItemLineWidth.Text = "Line width";
            MenuItem mnuItemLineWidth_1 = new MenuItem();
            mnuItemLineWidth_1.Text = "1";
            mnuItemLineWidth_1.Click += mnuItemLineWidth_1_Click;
            MenuItem mnuItemLineWidth_2 = new MenuItem();
            mnuItemLineWidth_2.Text = "2";
            mnuItemLineWidth_2.Click += mnuItemLineWidth_2_Click;
            MenuItem mnuItemLineWidth_3 = new MenuItem();
            mnuItemLineWidth_3.Text = "3";
            mnuItemLineWidth_3.Click += mnuItemLineWidth_3_Click;
            mnuItemLineWidth.MenuItems.Add(mnuItemLineWidth_1);
            mnuItemLineWidth.MenuItems.Add(mnuItemLineWidth_2);
            mnuItemLineWidth.MenuItems.Add(mnuItemLineWidth_3);

            MenuItem mnuItemLineColor = new MenuItem();
            mnuItemLineColor.Text = "Line color";
            mnuItemLineColor.Click += mnuItemLineColor_Click;

            tempContextMenu.MenuItems.Add(mnuItemNewChart);
            tempContextMenu.MenuItems.Add(mnuItemLineColor);
            tempContextMenu.MenuItems.Add(mnuItemLineWidth);

            return tempContextMenu;
        }

        void mnuItemNewChart_Click(object sender, EventArgs e)
        {
            legendListView.SelectedItems[0].Checked = true;
            mainChart.ChartAreas.Add(new ChartArea(legendListView.SelectedItems[0].Text));
            mainChart.ChartAreas.Last().AlignWithChartArea = mainChart.ChartAreas.First().Name;
            mainChart.ChartAreas.Last().AlignmentOrientation = AreaAlignmentOrientations.Vertical;
            mainChart.ChartAreas.Last().AlignmentStyle = AreaAlignmentStyles.All;
            mainChart.Series[legendListView.SelectedItems[0].Text].ChartArea = mainChart.ChartAreas.Last().Name;
        }

        void mnuItemLineColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog1 = new ColorDialog();
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                mainChart.Series[legendListView.SelectedItems[0].Text].Color = colorDialog1.Color;
                legendListView.SelectedItems[0].BackColor = colorDialog1.Color;
                calculationItemComboBox.Refresh();
                // set correct colors accoring to main legend
                foreach (ListViewItem item in legendListView.Items)
                {
                    mainChart.Series[item.Text].Color = item.BackColor;
                }

            }
        }

        void mnuItemLineWidth_1_Click(object sender, EventArgs e)
        {
            setLineWidth(1);
        }

        void mnuItemLineWidth_2_Click(object sender, EventArgs e)
        {
            setLineWidth(2);
        }

        void mnuItemLineWidth_3_Click(object sender, EventArgs e)
        {
            setLineWidth(3);
        }

        void legendListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (legendListView.FocusedItem.Bounds.Contains(e.Location) == true)
                {
                    legendContectMenu.Show(legendListView, e.Location);
                }
            } 
        }

        void dataRemoteList_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine(dataRemoteList.SelectedItems[0].Text);
                string adress = "ftp://" + dataIPTextBox.Text + "/" + dataRemoteList.SelectedItems[0].Text;
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(adress);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential("ftpUser", "egvFTP");

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                AddData(reader, adress);
                tabControl1.SelectedIndex = 0;
            }
            catch (SystemException e1)
            {
                MessageBox.Show("Connection error!");
            }
        }

        void dataIPTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                button2_Click(null,null);
            }
        }

        private void dataBrowseButton_Click(object sender, EventArgs e)
        {
            // Displays an OpenFileDialog so the user can select a Cursor.
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "CSV Files|*.csv";
            openFileDialog1.Title = "Select a CSVFile";
            openFileDialog1.Multiselect = true;

            // Show the Dialog.
            // If the user clicked OK in the dialog and
            // a .CUR file was selected, open it.
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Read the files
                foreach (String file in openFileDialog1.FileNames)
                {
                    // Assign the cursor in the Stream to the Form's Cursor property.
                    textBox1.Text = openFileDialog1.InitialDirectory + file;
                    AddData(new StreamReader(File.OpenRead(@textBox1.Text)), textBox1.Text.Split('\\').Last());
                }
            }
            
        }

        private void AddData(StreamReader csvStream, string workingFileInfo)
        {
            addDataCounter++;
            var reader = csvStream; // new StreamReader(File.OpenRead(@filePath));
            List<string> names = new List<string>();

            Boolean first = true;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (first)
                {
                    names.Clear();
                    if (mainChart.Series.FindByName("Series1") != null)
                        mainChart.Series.Remove(mainChart.Series.FindByName("Series1"));
                    //legendListView.Items.Clear();
                    legendListView.Columns[0].Text = "Legend";
                    legendListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                    foreach (var name in values)
                    {
                        if (name != values[0])
                        {
                            names.Add(addDataCounter.ToString() +"_"+ name.Replace("\"",""));
                            
                            mainChart.Series.Add(names.Last());
                            mainChart.Series[names.Last()].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                            mainChart.Series[names.Last()].ToolTip = "#VALX, #VALY";
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < values.Length - 1; i++)
                    {
                        double x = 0;
                        double y = 0;
                        if (!double.TryParse(values[0], out x) || !double.TryParse(values[1 + i], out y))
                        {
                            // Failed parsing
                            MessageBox.Show("Failed parsing: Change Region settings in Control Panel!");
                            return;
                        }
                        else
                        {
                            mainChart.Series[names[i]].Points.AddXY(x, y);
                        }    
                    }
                }
                first = false;
            }

            mainChart.ApplyPaletteColors();
            calculationItemComboBox.Items.Clear();
            legendListView.Items.Clear();
            foreach (Series item in mainChart.Series){
                legendListView.Items.Add(@item.Name);
                legendListView.Items[legendListView.Items.Count-1].BackColor = item.Color;
                calculationItemComboBox.Items.Add(@item.Name);
            }
            legendListView.Items[0].Checked = true;
            calculationItemComboBox.SelectedIndex = 0;
            tabControl1.SelectedIndex = 0;
            this.Text = workingFileInfo;
        }

        private void legendListView_ItemChecked(object sender, System.Windows.Forms.ItemCheckedEventArgs e)
        {
            mainChart.Series[e.Item.Text].Enabled = e.Item.Checked;
            mainChart.ChartAreas[0].RecalculateAxesScale();  
            if(mainChart.ChartAreas.FindByName(e.Item.Text) != null && !e.Item.Checked)
            {
                mainChart.Series[e.Item.Text].ChartArea = mainChart.ChartAreas.First().Name;
                mainChart.ChartAreas.Remove(mainChart.ChartAreas[e.Item.Text]);
            }         
        }

        private void mainChart_MouseDoubleClick(object sender, System.EventArgs e)
        {
            System.Windows.Forms.MouseEventArgs me = (System.Windows.Forms.MouseEventArgs)e;
            if (me.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                //chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(0); 
            }
            else
            {
                mainChart.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                mainChart.ChartAreas[0].AxisY.ScaleView.ZoomReset(0); 
            }

        }

        private void mainChart_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            foreach (ChartArea ca in mainChart.ChartAreas)
            {
                if (ChartAreaClientRectangle(mainChart, ca).Contains(e.Location))
                {
                    dataCharAreaPre = dataCharArea;
                    dataCharArea = ca.Name;
                    break;
                }
            }

            System.Windows.Forms.MouseEventArgs me = (System.Windows.Forms.MouseEventArgs)e;
            if (me.Button == System.Windows.Forms.MouseButtons.Right)
            {
                float pX = (float)mainChart.ChartAreas[dataCharArea].AxisX.PixelPositionToValue(e.X);
                float pY = (float)mainChart.ChartAreas[dataCharArea].AxisY.PixelPositionToValue(e.Y);
                addCross(pX, pY);
            }
        }

        RectangleF ChartAreaClientRectangle(Chart chart, ChartArea CA)
        {
            RectangleF CAR = CA.Position.ToRectangleF();
            float pw = chart.ClientSize.Width / 100f;
            float ph = chart.ClientSize.Height / 100f;
            return new RectangleF(pw * CAR.X, ph * CAR.Y, pw * CAR.Width, ph * CAR.Height);
        }

        private void addCross(float x, float y){
            if (calculationAreaPoints == null || dataCharArea!=dataCharAreaPre)
            {
                calculationAreaPoints = new LinkedList<PointF>();
                calculationAreaPoints.AddLast(new PointF(x, y));
                if(mainChart.Series.FindByName("cross1") != null)
                    mainChart.Series.Remove(mainChart.Series["cross1"]);

                mainChart.Series.Add("cross1");
                mainChart.Series["cross1"].ChartType = SeriesChartType.FastLine;

            }
            else if(calculationAreaPoints.Count == 1)
            {
                calculationAreaPoints.AddLast(new PointF(x, y));

                mainChart.Series["cross1"].Points.AddXY(calculationAreaPoints.First().X, calculationAreaPoints.First().Y);
                mainChart.Series["cross1"].Points.AddXY(x, calculationAreaPoints.First().Y);
                mainChart.Series["cross1"].Points.AddXY(x, y);
                mainChart.Series["cross1"].Points.AddXY(calculationAreaPoints.First().X, y);
                mainChart.Series["cross1"].Points.AddXY(calculationAreaPoints.First().X, calculationAreaPoints.First().Y);
                mainChart.Series["cross1"].ChartArea = dataCharArea;
                calculateData();
            }
            else
            {
                if (mainChart.Series.FindByName("cross1") == null)
                    return;
                mainChart.Series["cross1"].Points.Clear();
                mainChart.Series.Remove(mainChart.Series["cross1"]);
                calculationAreaPoints = null;
            }
        }

        private void calculationItemComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                // Draw the background 
                e.DrawBackground();

                // Get the item text    
                string text = ((ComboBox)sender).Items[e.Index].ToString();

                e.Graphics.FillRectangle(new SolidBrush(legendListView.Items[e.Index].BackColor), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                e.Graphics.DrawString(text, ((Control)sender).Font, Brushes.Black, e.Bounds.X, e.Bounds.Y);
            }
            catch (SystemException e1)
            {
                MessageBox.Show("Program error, sorry!");
            }
        }

        private void calculationItemComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            calculationItemComboBox.BackColor = legendListView.Items[calculationItemComboBox.SelectedIndex].BackColor;
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
                    if (calculationItemComboBox.SelectedIndex != -1)
                    {
                        item = (string)calculationItemComboBox.SelectedItem;
                        foreach (var point in mainChart.Series[item].Points)
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
                string adress = "ftp://" + dataIPTextBox.Text + "/";
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
                dataRemoteList.Items.Clear();
                while ((line = reader.ReadLine()) != null)
                {
                    if(line.EndsWith(".CSV")){
                        lineParts = line.Split(' ');
                        dataRemoteList.Items.Add(lineParts[lineParts.Length - 1]);
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

        private void setLineWidth(int lineWidth)
        {
            mainChart.Series[legendListView.SelectedItems[0].Text].BorderWidth = lineWidth;
        }

    }

}
