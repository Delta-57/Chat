using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;

namespace ChatClient
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }

        public static Socket serversocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static string nickname;
        const string successcode = "4";


        private void button1_Click(object sender, EventArgs e)
        {
            byte[] authanswer = new byte[2];
            nickname = textBox1.Text;
            string serverip;
            int serverport;
            string password = textBox4.Text;

            try
            {
                serverip = textBox2.Text;
                serverport = int.Parse(textBox3.Text);
            }
            catch
            {
                MessageBox.Show("Неверный формат ip или порта");
                return;
            }

            IPEndPoint endpoint;

            try
            {
                endpoint = new IPEndPoint(IPAddress.Parse(serverip), serverport);
                serversocket.Connect(endpoint);
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к серверу");
                return;
            }

            serversocket.Send(Encoding.Unicode.GetBytes(password));

            bool responsecheck = false;

            while (!responsecheck)
            {
                if (serversocket.Available > 0)
                {
                    serversocket.Receive(authanswer);
                    responsecheck = true;
                }
            }

            if (Encoding.Unicode.GetString(authanswer) == successcode)
            {
                Chat chatform = new Chat();
                Hide();
                chatform.ShowDialog(Owner);
                Close();
            }
            else
            {
                MessageBox.Show("Неверный пароль");
            }
        }
    }
}
