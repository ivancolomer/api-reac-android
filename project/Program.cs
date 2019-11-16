using System;
using System.Net;

using Nancy.Hosting.Self;

using REAC_AndroidAPI.Utils.Output;
using REAC_AndroidAPI.Utils.Storage;
using REAC_AndroidAPI.Utils.Network;
using REAC2_AndroidAPI.Utils.Network.Udp;
using System.IO;
using System.Threading;
using REAC_AndroidAPI.Utils.Network.Tcp;

namespace REAC_AndroidAPI
{
    //sudo apt-get update
    //sudo apt-get install curl libunwind8 gettext apt-transport-https
    //sudo chmod 755 ./REAC-AndroidAPI

    //dotnet publish --self-contained --runtime linux-arm


    public class Program
    {
        public static DateTime InitStartUTC { get; set; }

        private static SocketListener ServerListener;
        private static BroadcastEmitter BroadcastEmitter;
        private static NancyHost NancyHost;
        private static bool HasExited = false;

        static void Main(string[] args)
        {
            InitStartUTC = DateTime.UtcNow;

            DotNetEnv.Env.Load(AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar + ".." + System.IO.Path.DirectorySeparatorChar + ".env");
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
            Utils.Network.Tcp.Sessions.SessionManager.Initialize();
            Handlers.Requests.UsersManager.Initialize();
            ServerListener = new SocketListener(new IPEndPoint(IPAddress.Any, DotNetEnv.Env.GetInt("TCP_LISTENER_PORT")), 100, new OnNewConnectionCallback(Utils.Network.Tcp.Sessions.SessionManager.HandleIncomingConnection));
            
            //FOR TESTING ONLY
            BroadcastReceiver broadcastReceiver = new BroadcastReceiver();
            
            
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
                ServerListener.Dispose();
            }
            catch (Exception e)
            {
                Logger.WriteLine("Errror closing socket: " + e.ToString(), Logger.LOG_LEVEL.ERROR);
            }

            foreach (var Session in Utils.Network.Tcp.Sessions.SessionManager.CopySessions)
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
