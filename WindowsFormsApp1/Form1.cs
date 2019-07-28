using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream stream;

        public Form1()
        {
            InitializeComponent();

            client = new TcpClient();
        }



        private async void Button1_Click(object sender, EventArgs e)
        {
            if (!client.Connected)
            {
                await client.ConnectAsync("192.168.1.172", 9999);
            }

            stream = client.GetStream();
            byte[] str = Encoding.UTF8.GetBytes(textBox1.Text);
            byte[] output = new byte[str.Length + 50];
            await stream.WriteAsync(str, 0, str.Length);
            await stream.ReadAsync(output, 0, output.Length);
            listBox1.Items.Add(Encoding.UTF8.GetString(output));
        }
    }
}
