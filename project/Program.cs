using System;
using System.Net;

using Nancy.Hosting.Self;

using REAC_AndroidAPI.Utils.Output;
using REAC_AndroidAPI.Utils.Storage;
using REAC_AndroidAPI.Utils.Network;
using REAC2_AndroidAPI.Utils.Network.Udp;
using System.IO;
using System.Threading;
using REAC_AndroidAPI.Utils.Network.Tcp.Common;
using REAC_AndroidAPI.Utils.Network.Tcp.LockerDevices;
using REAC_AndroidAPI.Utils.Network.Tcp.StreamVideoClients;

namespace REAC_AndroidAPI
{
    //sudo apt-get update
    //sudo apt-get install curl libunwind8 gettext apt-transport-https
    //sudo chmod 755 ./REAC-AndroidAPI

    //dotnet publish --self-contained --runtime linux-arm


    public class Program
    {
        public static DateTime InitStartUTC { get; set; }

        private static BroadcastEmitter BroadcastEmitter;
        private static NancyHost NancyHost;

        public static LockerDevicesManager LockerDevicesManager;
        public static VideoClientsManager VideoClientsManager;

        public static VideoStreamServer VideoStreamServer;

        private static bool HasExited = false;

        static void Main(string[] args)
        {
            InitStartUTC = DateTime.UtcNow;

            DotNetEnv.Env.Load(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".env");
            Logger.Initialize();

            Console.CancelKeyPress += delegate
            {
                ExitProgram();
            };

            AppDomain.CurrentDomain.ProcessExit += delegate
            {
                ExitProgram();
            };

            SqlDatabaseManager.Initialize();

            VideoStreamServer = new VideoStreamServer();
            LockerDevicesManager = new LockerDevicesManager();
            VideoClientsManager = new VideoClientsManager();
            Handlers.Requests.UsersManager.Initialize();

            BroadcastEmitter = new BroadcastEmitter();
            NancyHost = new NancyHost(new HostConfiguration { RewriteLocalhost = true }, new Uri("http://localhost:" + DotNetEnv.Env.GetInt("WEB_SERVER_PORT") + "/")); 
            try
            {
                NancyHost.Start();
            } catch (Exception e)
            {
                Logger.WriteLineWithHeader(e.ToString(), "Couldn't start nancyHost", Logger.LOG_LEVEL.ERROR);
                Console.ReadKey();
                return;
            }

            Logger.WriteLine("Nancy now listening - navigating to http://localhost:" + DotNetEnv.Env.GetInt("WEB_SERVER_PORT") + "/api/. Type 'close' to stop", Logger.LOG_LEVEL.INFO);

            string line;
            while (true)
            {
                try
                {
                    line = Console.ReadLine();

                    if (line != null)
                    {
                        switch (line)
                        {
                            case "close":
                                ExitProgram();
                                return;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                    
                }
                catch(Exception e)
                {
                    Logger.WriteLine(e.ToString(), Logger.LOG_LEVEL.ERROR);
                    Thread.Sleep(Timeout.Infinite);
                }              
            }
        }

        private static void ExitProgram()
        {
            if (HasExited)
                return;

            HasExited = true;

            try
            {
                NancyHost.Dispose();
            } catch(Exception)
            {

            }
            CloseSocket();
            Logger.WriteLine("Stopped. Good bye!", Logger.LOG_LEVEL.DEBUG);
            Environment.Exit(0);
        }

        private static void CloseSocket()
        {
            try
            {
                VideoClientsManager.ServerListener.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error closing socket: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }

            try
            {
                LockerDevicesManager.ServerListener.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error closing socket: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }

            foreach (var Session in VideoClientsManager.CopySessions)
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

            foreach (var Session in LockerDevicesManager.CopySessions)
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
