﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Security.Permissions;
using NLog;
using NLog.Fluent;
using RocksmithToolkitLib.Extensions;
using RocksmithToolkitLib;

namespace RocksmithToolkitGUI
{
    static class Program
    {
        /// <summary>
        /// Usage: RocksmithToolkitGUI.log.Error(«ERROR: {0}», this.Text);
        /// </summary>
        public static Logger Log;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main(string[] args)
        {
            Log = LogManager.GetCurrentClassLogger();
            //I should figure out way for native mac\linux OS
            var logPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "_RSToolkit_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");

            Log.Info(//OSVersion on unix will return it's Kernel version, urgh.
                String.Format("RocksmithToolkitGUI: v{0}\r\n ", ToolkitVersion.RSTKGuiVersion) +
                String.Format("RocksmithToolkitLib: v{0}\r\n ", ToolkitVersion.RSTKLibVersion()) +
                String.Format("RocksmithToolkitUpdater: v{0}\r\n ", ToolkitVersion.RSTKUpdaterVersion()) +
                String.Format("OS: {0} ({1} bit)\r\n ", Environment.OSVersion, Environment.Is64BitOperatingSystem ? "64" : "32") +
                String.Format("Runtime: v{0}\r\n ", Environment.Version) +
                String.Format("JIT: {0}\r\n ", JitVersionInfo.GetJitVersion()) +
                String.Format("Wine: {0}", GeneralExtensions._wine())
            );

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                Log.Error(exception, "\n{0}\n{1}\nException catched:\n{2}\n\n", exception.Source, exception.TargetSite, exception.InnerException);
                //Log.Error("Application Stdout:\n\n{0}", new StreamReader(_stdout.ToString()).ReadToEnd());

                if (MessageBox.Show(String.Format("Application.ThreadException met.\n\n\"{0}\"\n\n{1}\n\nPlease send us \"{2}\", open log file now?",
                    exception.ToString(), exception.Message.ToString(), Path.GetFileName(logPath)), "Unhandled Exception", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    //figure out how to call it single time
                    //Process.Start("explorer.exe", string.Format("/select,\"{0}\"", logPath));
                    Process.Start(logPath);
                }
                //Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true }); //write back to Stdout in console could use custom streamwriter so you could write to console from there
            };

            // UI thread exceptions handling.
            Application.ThreadException += (s, e) =>
            {
                var exception = e.Exception;
                Log.Error(exception, "\n{0}\n{1}\nException catched:\n{2}\n\n", exception.Source, exception.TargetSite, exception.InnerException);
                //Log.Error("Application Stdout:\n\n{0}", new StreamReader(_stdout.ToString()).ReadToEnd());

                if (MessageBox.Show(String.Format("Application.ThreadException met.\n\n\"{0}\"\n\n{1}\n\nPlease send us \"{2}\", open log file now?",
                    exception.ToString(), exception.Message.ToString(), Path.GetFileName(logPath)), "Thread Exception", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    //Process.Start("explorer.exe", string.Format("/select,\"{0}\"", logPath));
                    Process.Start(logPath);
                }
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(args));
        }
    }
}
