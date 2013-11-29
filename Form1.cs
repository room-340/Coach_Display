using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.IO.Ports;
using System.Collections;

namespace Coach_Display
{
    public partial class Form1 : Form
    {
        System.Drawing.Graphics graph; // используется для рисования
        SerialPort active_com = new SerialPort();
        Thread reading_active;
        Thread display_active;
        Queue byte2display = new Queue();  // очередь на отображение

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            com_search();
            bool connected = Connect();
           
            
            
            
        }

        private void reading(object com)
        {
            int read_buffer = 64;
            int len = 0;
            byte[] buffer = new byte[read_buffer];
            //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            //timer.Start();
            byte2display = new Queue();
            try
            {
                while (true)
                {
                    if (active_com.BytesToRead >= read_buffer)
                    {
                        len = active_com.Read(buffer, 0, read_buffer);
                        byte2display.Enqueue(buffer.Clone());
                    }

                }
            }
            catch (COMException sd)
            {
                MessageBox.Show("Произошла ошибка - " + sd.Message + "\nПопробуйте выбрать другой СОМ порт.");
                reading_active.Abort();
                Thread.Sleep(50);
                display_active.Abort();
                Thread.Sleep(50);
                
                return;
            }
            
            //timer.Stop();
        }

        private void display(object com)
        {
            byte one = 0;
            byte two = 0;
            byte[] buffer = new byte[17];
            byte[] full_buffer = new byte[50];
            //System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            //timer.Start();
            packet legacy = new packet();
            byte[] temp = new byte[2];
            while (true)
            {
                if (byte2display.Count > 1)
                {
                    full_buffer = (byte[])byte2display.Dequeue();
                    for (int i = 0; i < full_buffer.Length; i++)
                    {
                        if (i<63)
                        {
                            one = full_buffer[i];
                            i = i + 1;
                        }

                        if ((one == 10))
                        {
                            if (i < 63)
                            {
                                two = full_buffer[i];
                                i = i + 1;
                            }
                            if (two == 13)
                            {
                                /*for (int i = 0; i < 17; i++)
                                    if (byte2display.Peek() != null)
                                        buffer[i] = (byte)byte2display.Dequeue();
                                    else
                                        byte2display.Dequeue();
                                */
                                if (i + 17 <= full_buffer.Length)
                                    for (int j = 0; j < buffer.Length; j++)
                                        buffer[j] = full_buffer[i + j];     
                           
                                textBox13.Invoke(new Action(() => textBox13.Text = "" + buffer[0]));
                                textBox12.Invoke(new Action(() => textBox12.Text = "" + buffer[1]));
                                textBox11.Invoke(new Action(() => textBox11.Text = "" + buffer[2]));

                                temp[0] = buffer[5]; temp[1] = buffer[4];
                                textBox6.Invoke(new Action(() => textBox6.Text = "" + BitConverter.ToInt16(temp, 0)));
                                temp[0] = buffer[7]; temp[1] = buffer[6];
                                textBox5.Invoke(new Action(() => textBox5.Text = "" + BitConverter.ToInt16(temp, 0)));
                                temp[0] = buffer[9]; temp[1] = buffer[8];
                                textBox3.Invoke(new Action(() => textBox3.Text = "" + BitConverter.ToInt16(temp, 0)));
                                temp[0] = buffer[11]; temp[1] = buffer[10];
                                textBox7.Invoke(new Action(() => textBox7.Text = "" + BitConverter.ToInt16(temp, 0)));
                                temp[0] = buffer[14]; temp[1] = buffer[13];
                                textBox4.Invoke(new Action(() => textBox4.Text = "" + buffer[12] + " " + BitConverter.ToUInt16(temp, 0)));

                                if ((buffer[3] != 0) && (buffer[3] != 0x10))
                                {
                                    legacy.time_h = buffer[0];
                                    legacy.time_min = buffer[1];
                                    legacy.time_sec = buffer[2];
                                    legacy.status = buffer[3];
                                    legacy.temp = BitConverter.ToInt16(buffer, 4);
                                    legacy.prok = BitConverter.ToInt16(buffer, 6);
                                    legacy.n = BitConverter.ToInt16(buffer, 8);
                                    legacy.vel = BitConverter.ToInt16(buffer, 10);
                                    legacy.Km = buffer[12];
                                    legacy.meter = BitConverter.ToUInt16(buffer, 13);
                                    legacy.char_pulse = buffer[15];
                                    legacy.check_sum = buffer[16];
                                }
                                textBox10.Invoke(new Action(() => textBox10.Text = "" + (buffer[0] - legacy.time_h)));
                                textBox9.Invoke(new Action(() => textBox9.Text = "" + (buffer[1] - legacy.time_min)));
                                textBox8.Invoke(new Action(() => textBox8.Text = "" + (buffer[2] - legacy.time_sec)));

                                textBox1.Invoke(new Action(() => textBox1.Text = "" + (BitConverter.ToInt16(buffer, 8) - legacy.n)));
                                textBox2.Invoke(new Action(() => textBox2.Text = "" + (buffer[12] - legacy.Km) + " " + (BitConverter.ToUInt16(buffer, 13) - legacy.meter)));
                            }
                        }
                    }
                }   
            }
            //timer.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (reading_active.IsAlive)
                reading_active.Abort();
            if (display_active.IsAlive)
                display_active.Abort();
            if (active_com.IsOpen)
                active_com.Close();
            Application.Exit();
        }

        private void draw_bar(int index, double value)
        {
            Rectangle clean_area = new Rectangle();
            Rectangle draw_area = new Rectangle();
            Rectangle fill_area = new Rectangle();
            SolidBrush cleaner = new SolidBrush(this.BackColor);
            
            int step = 0;
            
            switch (index)
            {
                case 1:
                    value += 5;
                    step = (int)(value * 2.5);
                    clean_area = new Rectangle(270, 66, 22, 127);
                    draw_area = new Rectangle(271, 67, 20, 125);
                    fill_area = new Rectangle(271, (125 - step) + 67, 20, step);
                    break;
                case 2:
                    value += 5;
                    step = (int)value * 5;
                    clean_area = new Rectangle(424, 79, 22, 352);
                    draw_area = new Rectangle(425, 80, 20, 350);
                    fill_area = new Rectangle(425, (350 - step) + 80, 20, step);
                    break;
                case 3:
                    value += 1;
                    step = (int)(value * 12.9);
                    clean_area = new Rectangle(270, 275, 22, 156);
                    draw_area = new Rectangle(271, 276, 20, 154);
                    fill_area = new Rectangle(271, (154 - step) + 276, 20, step);
                    break;
            }
            graph.FillRectangle(cleaner, clean_area);
            ProgressBarRenderer.DrawVerticalBar(graph, draw_area);
            ProgressBarRenderer.DrawVerticalChunks(graph, fill_area);
        }

        private void draw_dot(bool is_active)
        {
            SolidBrush brush = new SolidBrush(Color.Black);
            if (is_active)
                brush = new SolidBrush(Color.LightGreen);
            // 344 и 30 - координаты левого верхнего угла окружности
            graph.FillEllipse(brush, 344, 30, 16, 16);
            
        }

        private void com_search()
        {
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
                listBox1.Items.Add(port);
            
            if (ports.Length != 0)
                listBox1.SelectedIndex = 0;
        }

        private bool Connect()
        {
            if (listBox1.Items.Count > 0)
            {
                active_com = new SerialPort(listBox1.SelectedItem.ToString(), 115200, 0, 8, StopBits.One);
                active_com.WriteBufferSize = 512;
                active_com.ReadBufferSize = 8192;
            }
            try
            {
                active_com.Open();
            }
            catch (Exception)
            {
            }
            bool result = active_com.IsOpen;
            reading_active = new Thread(reading);
            display_active = new Thread(display);
            if (result)
            {
                richTextBox1.ForeColor = Color.Green;
                reading_active.Start();
                display_active.Start();
            }
            else
                richTextBox1.ForeColor = Color.Black;
            return result;
        }

        private void Disconnect()
        {
            if (reading_active.IsAlive)
                reading_active.Abort();
            if (display_active.IsAlive)
                display_active.Abort();
            if (active_com.IsOpen)
                active_com.Close();
            Thread.Sleep(50);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (reading_active.IsAlive)
                reading_active.Abort();
            if (display_active.IsAlive)
                display_active.Abort();
            if (active_com.IsOpen)
                active_com.Close();
            Application.Exit();
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            Disconnect();
            Connect();
        }



    }

    public struct packet
    {
        public byte time_h;
        public byte time_min;
        public byte time_sec;
        public byte status;
        public short temp;
        public short prok;
        public short n;
        public short vel;
        public byte Km;
        public ushort meter;
        public byte char_pulse;
        public byte check_sum;
    }

    public class VerticalProgressBar : ProgressBar
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x04;
                return cp;
            }
        }
    }
}
