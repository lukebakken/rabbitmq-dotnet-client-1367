using RabbitMQ.Client;

namespace RabbitMqCertCheck
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"[INFO] working directory: {System.IO.Directory.GetCurrentDirectory()}");
            try
            {
                string rabbitMqHostname = "localhost";
                int rabbitMqPort = 5671;
                string rabbitMqUsername = "guest";
                string rabbitMqPassword = "guest";
                string certificatePath = "..\\certs\\client_localhost_certificate.pfx";
                string certificatePassphrase = "test1234";
                string virtualHost = "/";

                var factory = new ConnectionFactory
                {
                    Port = rabbitMqPort,
                    UserName = rabbitMqUsername,
                    Password = rabbitMqPassword,
                    Ssl = new SslOption
                    {
                        Enabled = true,
                        ServerName = rabbitMqHostname,
                        CertPath = certificatePath,
                        CertPassphrase = certificatePassphrase,
                    }
                };


                factory.HostName = rabbitMqHostname;
                factory.VirtualHost = virtualHost;

                using (var connection = factory.CreateConnection())
                {
                    Console.WriteLine("[INFO] connection has been created...");
                    using (var channel = connection.CreateModel())
                    {
                        Console.WriteLine("[INFO] inside CreateModel, any key to exit...");
                        Console.ReadLine();
                        Console.WriteLine("[INFO] EXITING!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] exception: {ex.Message}");
                Console.Error.WriteLine($"stack: {ex.StackTrace}");
                Console.Error.WriteLine($"inner ex: {ex.InnerException}");
            }
        }
    }
}
