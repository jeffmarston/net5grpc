using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grpc.Net.Client;

namespace frameworkForms
{
    static class Program
    { 
        private static string _sslThumbprint = "428299a87bbbe9f6071e53ed6377aa2380415ad0"; // Your own SSL certificate thumbprint 

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Using sample test credentials from https://github.com/grpc/grpc/tree/master/src/core/tsi/test_creds
            string rootDir = $"{Directory.GetCurrentDirectory()}/../..";
            // Note: these ideally shouldn't be text files, but it's fine for testing purposes. I copied the creds from the link above. 
            var keyCertPair = new KeyCertificatePair(File.ReadAllText($"{rootDir}/ca.pem.txt"), File.ReadAllText($"{rootDir}/ca.key.txt"));
            var channelCreds = new SslCredentials(GetRootCertificates(), keyCertPair);

            var channel = new Channel("localhost", 5001, channelCreds);



            // The port number(5001) must match the port of the gRPC server.
            // using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(
                              new HelloRequest { Name = "net 5 forms" });



            var greeter = channel.CreateGrpcService<IGreeterService>();
            try
            {
                Console.WriteLine(greeter.Ping());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            //Console.ReadLine();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static X509Certificate2 LoadSSLCertificate()
        {
            var certStore = new X509Store(StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly);
            var cert = certStore.Certificates.Find(X509FindType.FindByThumbprint, _sslThumbprint, true)[0];
            certStore.Close();
            return cert;
        }

        /**
        * Source: https://stackoverflow.com/questions/58125102/grpc-net-client-fails-to-connect-to-server-with-ssl
        **/
        public static string GetRootCertificates()
        {
            StringBuilder builder = new StringBuilder();
            var cert = LoadSSLCertificate();
            builder.AppendLine(
                    "# Issuer: " + cert.Issuer.ToString() + "\n" +
                    "# Subject: " + cert.Subject.ToString() + "\n" +
                    "# Label: " + cert.FriendlyName.ToString() + "\n" +
                    "# Serial: " + cert.SerialNumber.ToString() + "\n" +
                    "# SHA1 Fingerprint: " + cert.GetCertHashString().ToString() + "\n" +
                    ExportToPEM(cert) + "\n");
            return builder.ToString();
        }

        public static string ExportToPEM(X509Certificate cert)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            builder.AppendLine(Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine("-----END CERTIFICATE-----");

            return builder.ToString();
        }
    }

}