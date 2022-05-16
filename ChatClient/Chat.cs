using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace ChatClient
{
    public partial class Chat : Form
    {

        byte[] buffer1 = new byte[256];
        int size = 0;
        string data;

        /// <summary>
        /// Преобразует общий буфер в буфер с размером сообщения 
        /// </summary>
        /// <param name="oldbuffer">Общий буфер</param>
        /// <param name="size">Размер сообщения</param>
        /// <returns>Буфер с размером сообщения</returns>
        byte[] MakeNewBuffer(byte[] oldbuffer, int size)
        {
            byte[] newbuffer = new byte[size];
            for (int i = 0; i < size; i++)
            {
                newbuffer[i] = oldbuffer[i];
            }
            return newbuffer;
        }

        public Chat()
        {
            InitializeComponent();
            timer1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string message = Menu.nickname + ": " + textBox2.Text;
            byte[] buffer = Encoding.Unicode.GetBytes(message);
            try
            {
                Menu.serversocket.Send(buffer);
            }
            catch
            {
                MessageBox.Show("Потеряно соединение с сервером");
                Menu.serversocket.Shutdown(SocketShutdown.Both);
                return;
            }
            textBox1.Text += message + "\r\n";
            textBox2.Text = "";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Menu.serversocket.Available > 0)
            {
                size = Menu.serversocket.Receive(buffer1);
                if (size > 0)
                {
                    byte[] buffer2;
                    buffer2 = MakeNewBuffer(buffer1, size);
                    data = Encoding.Unicode.GetString(buffer2);
                    textBox1.Text += data + "\r\n";
                    size = 0;
                }
            }
        }
    }
}
