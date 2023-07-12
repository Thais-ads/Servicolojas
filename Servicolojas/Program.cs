
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Textoteste
{
    class Program
    {
        static void Main(string[] args)
        {
            string strComputerName = Environment.MachineName.ToString();

            try
            {

                string caminho = @"C:\sintese\Sinfte.txt";
                using (StreamReader sr = new StreamReader(caminho))

                {
                    string dados;
                    var parametros = File.ReadAllLines(caminho);

                    if ((dados = sr.ReadLine()) != null)
                    {
                        if (parametros[6] == "Servidor")
                        {

                            ExecutarServidor(strComputerName);

                        }
                        else
                        {
                            string log = ($"Marca: {parametros[0]}, Loja: {parametros[1]}, NomeDoServidor: {strComputerName},Backup: {parametros[3]}, QntTerminais: {parametros[4]}, TipoCaixa: {parametros[5]}");
                            ExecutarCliente(log, parametros[6]);
                        }

                    }

                }

            }

            catch (Exception)
            {

                Console.WriteLine("Esse caixa é um servidor? (s/n)");
                var escolha = Console.ReadLine();
                if (escolha.Equals("s", StringComparison.OrdinalIgnoreCase))
                {
                    string caminhoBackup;
                    int contador = 0;
                    List<string> nomesTerminais = new List<string>();

                    Console.WriteLine("Marca:  ");
                    string marca = Console.ReadLine()!;

                    Console.WriteLine("\nFilial: ");
                    string filial = Console.ReadLine()!;


                    Console.WriteLine("\nNome da Maquina que se localiza o Backup: ");
                    string nomeMaquinaBackup = Console.ReadLine();

                    Console.WriteLine("\nCaminho do Backup: ");
                    caminhoBackup = Console.ReadLine()!;

                    Console.WriteLine("\nQuantos terminais existem? (servidor NÂO conta)");
                    int qntTerminais = int.Parse(Console.ReadLine());

                    while (contador < qntTerminais)
                    {
                        contador++;
                        Console.WriteLine($"Nome do {contador}° Terminal: ");
                        string nomeMaquina = Console.ReadLine();
                        nomesTerminais.Add(nomeMaquina);

                    }
                    contador = 0;

                    Console.WriteLine($"\nNome do Servidor: {strComputerName}\nMarca: {marca}\nFilial: {filial}\nQntTerminais: {qntTerminais}\nMaquina que se localiza Buckup: {nomeMaquinaBackup}\nLocalizacao do Backup: {caminhoBackup}");
                    while (contador < qntTerminais)
                    {
                        contador++;
                        Console.WriteLine($"{contador}° Terminal: {nomesTerminais[(contador - 1)]}");

                    }

                    Console.WriteLine("Confirmar Dados(s / n):");
                    string confirmDados = Console.ReadLine();



                    if (confirmDados.Equals("s", StringComparison.OrdinalIgnoreCase))
                    {
                        string caminho = @"C:\sintese\Sinfte.txt";
                        using (StreamWriter sw = new StreamWriter(caminho))
                        {
                            sw.WriteLine(marca); //Marca
                            sw.WriteLine(filial); //Filal
                            sw.WriteLine(strComputerName); //Nome Servidor
                            sw.WriteLine(nomeMaquinaBackup); //Nome da Maquina Backup
                            sw.WriteLine(caminhoBackup); //Caminho do Backup
                            sw.WriteLine(qntTerminais); //Qnt Terminais
                            sw.WriteLine("Servidor"); //Servidor ou Caixa

                        }

                        string log = ($"{marca},{filial},{strComputerName},{nomeMaquinaBackup},{caminhoBackup},{qntTerminais}");

                        foreach (var nomeConexao in nomesTerminais)
                        {
                            ExecutarCliente(log, nomeConexao);
                        }

                    }

                }
                else
                {
                    string config = ExecutarServidor(strComputerName);
                    string caminho = @"C:\sintese\Sinfte.txt";
                    string[] parametros = config.Split(",");
                    using (StreamWriter sw = new StreamWriter(caminho))
                    {
                        sw.WriteLine(parametros[0]); //Marca
                        sw.WriteLine(parametros[1]); //Filal
                        sw.WriteLine(parametros[2]); //NomeServidor
                        if (strComputerName == parametros[3])
                        {
                            sw.WriteLine(parametros[4]); //Backup
                        }
                        else
                        {
                            sw.WriteLine("NaoPossueBackup"); //Backup
                        }

                        sw.WriteLine(parametros[5]); //Qnt Terminais
                        sw.WriteLine("Caixa"); //Servidor ou Caixa

                    }
                }

            }
        }



        public static string ExecutarServidor(string strComputerName)
        {

            string hostname = strComputerName;
            IPAddress[] addresses = Dns.GetHostAddresses(hostname);
            IPAddress ipAddr = addresses[0];

            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);
            Socket listener = new Socket(ipAddr.AddressFamily,
                         SocketType.Stream, ProtocolType.Tcp);

            try
            {

                listener.Bind(localEndPoint);

                listener.Listen(10);

                while (true)
                {

                    Console.WriteLine("Waiting connection ... ");

                    Socket clientSocket = listener.Accept();

                    byte[] bytes = new Byte[1024];
                    string data = null;

                    while (true)
                    {

                        int numByte = clientSocket.Receive(bytes);

                        data += Encoding.ASCII.GetString(bytes,
                                                   0, numByte);

                        if (data.IndexOf("<EOF>") > -1)
                            break;
                    }

                    data = data.Replace("<EOF>", "");

                    Console.WriteLine(data);
                    //byte[] message = Encoding.ASCII.GetBytes("Test Server");

                    //clientSocket.Send(message);

                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    return data;
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return "Erro ao receber config";
            }
        }

        public static void ExecutarCliente(string log, string nomeConexao)
        {
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(nomeConexao);
                IPAddress ipAddr = addresses[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);
                Socket sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

                try
                {

                    sender.Connect(localEndPoint);

                    byte[] messageSent = Encoding.ASCII.GetBytes(log + "<EOF>");
                    int byteSent = sender.Send(messageSent);

                    byte[] messageReceived = new byte[1024];

                    int byteRecv = sender.Receive(messageReceived);

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }

                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException se)
                {

                    Console.WriteLine("SocketException : {0}", se.ToString());
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
        }

    }

}









