﻿using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using REAC_AndroidApi.Utils.Storage;
using REAC_AndroidApi.Utils.Network;
using Nancy.Hosting.Self;
using REAC2_AndroidAPI.Utils.Output;

namespace REAC_AndroidApi
{
    public class Program
    {
        public static DateTime InitStartUTC { get; set; }

        private static SocketListener ServerListener;
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

            using (var nancyHost = new NancyHost(new Uri("http://localhost:8888/nancy/"), new Uri("http://127.0.0.1:8898/nancy/"), new Uri("http://localhost:8889/nancytoo/")))
            {
                try
                {
                    nancyHost.Start();
                } catch (Exception e)
                {
                    Logger.WriteLineWithHeader(e.ToString(), "Couldn't start nancyHost", Logger.LOG_LEVEL.ERROR);
                    Console.ReadKey();
                    return;
                }

                Logger.WriteLine("Nancy now listening - navigating to http://localhost:8888/nancy/. Press enter to stop", Logger.LOG_LEVEL.INFO);
            }

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
