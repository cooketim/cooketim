using Models.ViewModels;
using Serilog;
using Serilog.Events;
using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace TimsTool.Tree.Authentication
{
    public abstract class AuthBase
    {
        protected LogonModel viewModel;

        /// <summary>
        /// Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="output">string to be appended</param>
        protected void output(string output, LogEventLevel logEventLevel)
        {
            viewModel.Messages = viewModel.Messages + output + Environment.NewLine;
            switch (logEventLevel)
            {
                case LogEventLevel.Error:
                    {
                        Log.Error(output);
                        break;
                    }
                case LogEventLevel.Fatal:
                    {
                        Log.Fatal(output);
                        break;
                    }
                case LogEventLevel.Debug:
                    {
                        Log.Debug(output);
                        break;
                    }
                case LogEventLevel.Verbose:
                    {
                        Log.Verbose(output);
                        break;
                    }
                case LogEventLevel.Warning:
                    {
                        Log.Warning(output);
                        break;
                    }
                case LogEventLevel.Information:
                    {
                        Log.Information(output);
                        break;
                    }
            }
        }

        protected bool CheckInternetConnection()
        {
            var connected = false;
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    connected = true;
                    output("Detected internet connection", LogEventLevel.Information);
                }
            }
            catch
            {
                connected = false;
                output("Internet connection NOT detected", LogEventLevel.Information);
            }

            return connected;
        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        protected static string randomDataBase64url(uint length)
        {
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return base64urlencodeNoPadding(bytes);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        protected static string base64urlencodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }
        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        protected static byte[] sha256(string inputStirng)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            var sha256 = SHA256.Create();
            return sha256.ComputeHash(bytes);
        }

        // ref http://stackoverflow.com/a/3978040
        protected static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
