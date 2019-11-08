using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using REAC_AndroidApi.Utils.Storage;
using REAC_AndroidApi.Utils.Network;
using Nancy.Hosting.Self;
using REAC2_AndroidAPI.Utils.Output;
//using Microsoft.AspNetCore.Hosting;
using System.IO;
using REAC2_AndroidAPI;

namespace REAC_AndroidApi
{
    //sudo apt-get update
    //sudo apt-get install curl libunwind8 gettext apt-transport-https
    //sudo chmod 755 ./REAC-AndroidAPI

    //dotnet publish --self-contained --runtime linux-arm


    public class Program
    {
        public static DateTime InitStartUTC { get; set; }

        private static SocketListener ServerListener;
        private static NancyHost NancyHost;
        private static bool HasExited = false;

        static void Main(string[] args)
        {
            InitStartUTC = DateTime.UtcNow;
            DotNetEnv.Env.Load();
            Logger.Initialize();

            Console.CancelKeyPress += delegate
            {
                ExitProgram();
            };
           
            SqlDatabaseManager.Initialize();
            Utils.Network.Sessions.SessionManager.Initialize();

            SocketOption option = new SocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.ReuseAddress, 1);
            ServerListener = new SocketListener(new IPEndPoint(IPAddress.Any, 8081), 100, new OnNewConnectionCallback(Utils.Network.Sessions.SessionManager.HandleIncomingConnection), option);

            NancyHost = new NancyHost(new HostConfiguration { RewriteLocalhost = true }, new Uri("http://localhost:8080/api/"));
            
            try
            {
                NancyHost.Start();
            } catch (Exception e)
            {
                Logger.WriteLineWithHeader(e.ToString(), "Couldn't start nancyHost", Logger.LOG_LEVEL.ERROR);
                Console.ReadKey();
                return;
            }

            Logger.WriteLine("Nancy now listening - navigating to http://localhost:8080/api/. Press enter to stop", Logger.LOG_LEVEL.INFO);
            

            /*var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            host.Run();*/

            string line;
            while (true)
            {
                line = Console.ReadLine();

                if (line == null)
                    return;

                switch (line)
                {
                    case "close":
                        ExitProgram();
                        return;
                    default:
                        break;
                }
            }
        }

        private static void ExitProgram()
        {
            if (!HasExited)
            {
                HasExited = true;
            }
            else { return; }

            NancyHost.Dispose();
            CloseSocket();
            Logger.WriteLine("Stopped. Good bye!", Logger.LOG_LEVEL.DEBUG);
            Environment.Exit(0);
        }

        private static void CloseSocket()
        {
            try
            {
                ServerListener.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteLine("Errror closing socket: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }

            foreach (var Session in Utils.Network.Sessions.SessionManager.CopySessions)
            {
                try
                {
                    Session.Close();
                }
                catch (Exception e)
                {
                    Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.ERROR);
                }
            }
        }
    }
}
