using System.CommandLine;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace tlsclient
{
    public class Program
    {
        private const string defaultRabbitMQHost = "localhost";
        private const ushort defaultRabbitMQPort = 5671;
        private static readonly FileInfo defaultCertificateFile = new FileInfo("..\\certs\\client_localhost_certificate.pfx");
        private const string defaultCertificatePassword = "test1234";

        private static readonly Option<string> rabbitmqHostOption = new(name: "--host",
                description: "RabbitMQ host to which to connect",
                getDefaultValue: () => defaultRabbitMQHost);
        private static readonly Option<ushort> rabbitmqPortOption = new(name: "--port",
                description: "RabbitMQ TCP port to which to connect",
                getDefaultValue: () => defaultRabbitMQPort);
        private static readonly Option<FileInfo> certificateFileOption = new(name: "--certfile",
                description: "p12 / pfx file to present as X509 client certificate",
                getDefaultValue: () => defaultCertificateFile);
        private static readonly Option<string> certificatePasswordOption = new(name: "--certpass",
                description: "p12 / pfx file password",
                getDefaultValue: () => defaultCertificatePassword);

        public static void Main(string[] args)
        {
            Console.WriteLine($"[INFO] working directory: {System.IO.Directory.GetCurrentDirectory()}");

            RootCommand rootCommand = new(description: "a TLS client application")
            {
                rabbitmqHostOption,
                rabbitmqPortOption,
                certificateFileOption,
                certificatePasswordOption
            };

            rootCommand.SetHandler(RootCommandHandler, rabbitmqHostOption, rabbitmqPortOption, certificateFileOption, certificatePasswordOption);
            rootCommand.Invoke(args);
        }

        private static void RootCommandHandler(string rabbitmqHost, ushort rabbitmqPort, FileInfo certificateFile, string certificatePassword)
        {
            try
            {
                Console.WriteLine($"[INFO] RabbitMQ host: {rabbitmqHost}");
                Console.WriteLine($"[INFO] RabbitMQ port: {rabbitmqPort}");
                Console.WriteLine($"[INFO] Client cert file: {certificateFile.FullName}");

                var clientCertificate = new X509Certificate2(certificateFile.FullName, certificatePassword);
                var chain = new X509Chain();
                chain.Build(clientCertificate);

                foreach (X509ChainElement element in chain.ChainElements)
                {
                    Console.WriteLine($"[INFO] element Info Thumbprint: {element.Certificate.Thumbprint}{Environment.NewLine}Friendly Name: {element.Certificate.FriendlyName}{Environment.NewLine}Issuer: {element.Certificate.Issuer}{Environment.NewLine}Subject: {element.Certificate.SubjectName.Name}{Environment.NewLine}Expiration:{element.Certificate.GetExpirationDateString()}");
                }

                Console.WriteLine($"[INFO] ChainStatus Count: {chain.ChainStatus.Length}");
                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    Console.WriteLine("[INFO] chain status: " + status.Status);
                    Console.WriteLine("[INFO] chain status information: " + status.StatusInformation);
                }

                var remoteCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

                using (var client = new TcpClient())
                {
                    client.Connect(rabbitmqHost, rabbitmqPort);
                    using (var sslStream = new SslStream(client.GetStream(), false))
                    {
                        try
                        {
                            var clientCertificates = new X509CertificateCollection();
                            clientCertificates.Add(clientCertificate);
                            var sslClientAuthenticationOptions = new SslClientAuthenticationOptions
                            {
                                AllowRenegotiation = true,
                                ClientCertificates = clientCertificates,
                                RemoteCertificateValidationCallback = remoteCertificateValidationCallback,
                                TargetHost = rabbitmqHost

                            };
                            sslStream.AuthenticateAsClient(sslClientAuthenticationOptions);
                            Console.WriteLine("[INFO] TLS client has connected successfully!");
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"[ERROR] Exception Message {ex.Message}{Environment.NewLine}Inner Exception {ex.InnerException}{Environment.NewLine}Stack Trace {ex.StackTrace}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] Exception Message {ex.Message}{Environment.NewLine}Inner Exception {ex.InnerException}");
            }
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                // No errors, certificate is valid
                Console.WriteLine($"[INFO] ValidateServerCertificate -> certificate is Valid! (sslPolicyErrors: {sslPolicyErrors})");
                return true;
            }

            Console.Error.WriteLine($"[ERROR] ValidateServerCertificate -> Certificate validation error: {sslPolicyErrors}");

            if (chain != null)
            {
                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    Console.WriteLine($"[INFO] ValidateServerCertificate -> Chain status: {status.Status}");
                    Console.WriteLine($"[INFO] ValidateServerCertificate -> Chain status information: {status.StatusInformation}");
                }
            }

            return true;
        }
    }
}
