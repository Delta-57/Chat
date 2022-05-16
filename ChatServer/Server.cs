using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ChatServer
{
    public class Server
    {

        const int maxusers = 4;
        const int keylength = 5;
        const string successcode = "4";
        const string failcode = "5";

        static Socket[] clientsocket = new Socket[maxusers];
        static bool[] userverifed = new bool[maxusers];

        static MD5 mD5 = MD5.Create();


        static byte[] serverkey = new byte[keylength];
        static byte[] clientkey = new byte[keylength];

        /// <summary>
        /// Преобразует общий буфер в буфер с размером сообщения 
        /// </summary>
        /// <param name="oldbuffer">Общий буфер</param>
        /// <param name="size">Размер сообщения</param>
        /// <returns>Буфер с размером сообщения</returns>
        static byte[] MakeNewBuffer(byte[] oldbuffer, int size)
        {
            byte[] newbuffer = new byte[size];
            for (int i = 0; i < size; i++)
            {
                newbuffer[i] = oldbuffer[i];
            }

            return newbuffer;
        }

        /// <summary>
        /// Получает ключ на основе буфера
        /// </summary>
        /// <param name="buffer">буфер</param>
        /// <returns>Ключ шифрования</returns>
        static byte[] GetKey(byte[] buffer)
        {
            byte[] key = new byte[keylength];
            byte[] bufferhash = mD5.ComputeHash(buffer);

            for (int i = 0; i < keylength; i++)
            {
                key[i] = bufferhash[i];
            }

            return key;
        }

        /// <summary>
        /// Сравнивает ключи шифрования
        /// </summary>
        /// <param name="key1">Первый ключ</param>
        /// <param name="key2">Второй ключ</param>
        /// <returns>Результат сравнения</returns>
        static bool KeyCheck(byte[] key1, byte[] key2)
        {
            bool check = true;

            for (int i = 0; i < keylength; i++)
            {
                if (key1[i] != key2[i])
                {
                    check = false;
                    break;
                }
            }

            return check;
        }

        /// <summary>
        /// Подключает нового пользователя к серверу, если поступил запрос на подключение и ключи шифрования совпали
        /// </summary>
        /// <param name="obj">Сокет сервера</param>
        static void GetNewConnection(Object obj)
        {
            Socket socket = (Socket)obj;
            int i = 0;
            byte[] buffer3 = new byte[256];
            bool passwordcheck;
            while (i < maxusers)
            {
                clientsocket[i] = socket.Accept();
                if (clientsocket[i].Connected)
                {
                    Thread.Sleep(200);
                    if (clientsocket[i].Available > 0)
                    {
                        int size = clientsocket[i].Receive(buffer3);
                        var buffer4 = new byte[size];

                        buffer4 = MakeNewBuffer(buffer3, size);
                        clientkey = GetKey(buffer4);
                        passwordcheck = KeyCheck(serverkey, clientkey);

                        if (passwordcheck)
                        {
                            userverifed[i] = true;
                            clientsocket[i].Send(Encoding.Unicode.GetBytes(successcode));
                            Console.WriteLine("Подключение от " + clientsocket[i].RemoteEndPoint.ToString());
                            i++;
                        }
                        else
                        {
                            clientsocket[i].Send(Encoding.Unicode.GetBytes(failcode));
                        }
                    }

                }
            }
        }


        static void Main(string[] args)
        {
            string hostip;
            int hostport;
            string serverpassword;
            byte[] serverpasswordhash;

            try
            {
                Console.WriteLine("Введите ip сервера:");
                hostip = Console.ReadLine();
                Console.WriteLine("Введите порт сервера:");
                hostport = int.Parse(Console.ReadLine());
            }
            catch
            {
                Console.WriteLine("Некорректный формат ip или порта");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Введите пароль сервера");
            serverpassword = Console.ReadLine();
            serverpasswordhash = mD5.ComputeHash(Encoding.Unicode.GetBytes(serverpassword));
            for (int i = 0; i < 5; i++)
            {
                serverkey[i] = serverpasswordhash[i];
            }

            Socket hostsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint hostendpoint;

            try
            {
                hostendpoint = new IPEndPoint(IPAddress.Parse(hostip), hostport);
                hostsocket.Bind(hostendpoint);
                hostsocket.Listen(2);
            }
            catch
            {
                Console.WriteLine("Не удалось создать сервер");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Сервер успешно создан");

            Thread ConnectionReciever = new Thread(GetNewConnection);
            ConnectionReciever.Start(hostsocket);

            byte[] buffer1 = new byte[256];
            int size;
            string message;
            byte[][] bytemessages = new byte[maxusers][];

            while (true)
            {
                //Получаем сообщения от всех пользователей
                for (int i = 0; i < maxusers; i++)
                {
                    if (clientsocket[i] != null && clientsocket[i].Available > 0 && userverifed[i])
                    {
                        size = clientsocket[i].Receive(buffer1);
                        if (size > 0)
                        {
                            var buffer2 = new byte[size];
                            buffer2 = MakeNewBuffer(buffer1, size);
                            bytemessages[i] = buffer2;
                            message = Encoding.Unicode.GetString(buffer2);
                            Console.WriteLine(message);
                        }
                    }
                }

                //Отправляем сообщения всем пользователям, кроме автора сообщения
                for (int i = 0; i < maxusers; i++)
                {
                    for (int j = 0; j < maxusers; j++)
                    {
                        if (i != j && clientsocket[j] != null && bytemessages[i] != null && userverifed[j])
                        {
                            try
                            {
                                clientsocket[j].Send(bytemessages[i]);
                            }
                            catch
                            {
                                clientsocket[j].Shutdown(SocketShutdown.Both);
                                continue;
                            }
                        }
                    }
                    bytemessages[i] = null;
                }
            }
        }
    }
}

