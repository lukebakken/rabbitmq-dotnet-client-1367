using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace tlsclient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"[INFO] working directory: {System.IO.Directory.GetCurrentDirectory()}");
            try
            {
                string serverAddress = "localhost";
                int serverPort = 5671;
                string certificatePath = "..\\certs\\client_localhost_certificate.pfx";
                string certificatePassword = "test1234"; // Change to your certificate password
                Console.WriteLine($"{serverAddress} \n {serverPort} \n {certificatePath} \n");
                var client = new TcpClient();
                client.Connect(serverAddress, serverPort);
                X509Certificate2 clientCertificate = new X509Certificate2(certificatePath, certificatePassword);


                X509Chain chain = new X509Chain();
                chain.Build(clientCertificate);

                foreach (X509ChainElement element in chain.ChainElements)
                {
                    Console.WriteLine($"[INFO] element Info Thumbprint: {element.Certificate.Thumbprint} \n Friendly Name: {element.Certificate.FriendlyName} \n Issuer: {element.Certificate.Issuer} \n Subject {element.Certificate.SubjectName.Name} \n Expiration {element.Certificate.GetExpirationDateString()} ");
                }

                Console.WriteLine($"[INFO] ChainStatus Count: {chain.ChainStatus.Length}");
                foreach (X509ChainStatus status in chain.ChainStatus)
                {
                    Console.WriteLine("[INFO] chain status: " + status.Status);
                    Console.WriteLine("[INFO] chain status information: " + status.StatusInformation);
                }

                var callback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
                using SslStream sslStream = new SslStream(client.GetStream(), false, callback, null);
                {
                    try
                    {
                        sslStream.AuthenticateAsClient(serverAddress, new X509CertificateCollection() { clientCertificate }, SslProtocols.Tls12, false);
                        Console.WriteLine("[INFO] TLS client has connected successfully!");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[ERROR] Exception Message {ex.Message}{Environment.NewLine}Inner Exception {ex.InnerException}{Environment.NewLine}Stack Trace {ex.StackTrace}");
                    }
                }

                client.Close();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] Exception Message {ex.Message} \n Inner Exception {ex.InnerException}  ");
            }
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                // No errors, certificate is valid
                Console.WriteLine("[INFO] ValidateServerCertificate -> certificate is Valid " + sslPolicyErrors);
                return true;
            }

            Console.Error.WriteLine("[ERROR] ValidateServerCertificate -> Certificate validation error: " + sslPolicyErrors);

            foreach (X509ChainStatus status in chain.ChainStatus)
            {
                Console.WriteLine("[INFO] ValidateServerCertificate -> Chain status: " + status.Status);
                Console.WriteLine("[INFO] ValidateServerCertificate -> Chain status information: " + status.StatusInformation);
            }

            return true;
        }
    }
}
