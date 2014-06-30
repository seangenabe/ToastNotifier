using ShellLinkPlus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace ToastNotifier
{
    class Program
    {

        const string APPUSERMODELID = "ToastNotifier.ToastNotifier.ToastNotifier.1";

        static void Main(string[] args)
        {
            string installLocation;

#if DEBUG
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
#endif

            // Do a self-install.
            try
            {
                installLocation = SelfInstall();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Environment.ExitCode = 1;
                return;
            }

            // Pipe into the installed executable.
            if (installLocation != null)
            {
                var startInfo = new ProcessStartInfo(installLocation);
                startInfo.Arguments = JoinCommandLineArguments(args);
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                var process = Process.Start(startInfo);
                process.WaitForExit();
                Environment.ExitCode = process.ExitCode;
                return;
            }

            string title = "";
            var messages = new List<string>();
            string image = "";
            string audio = "";
            string xml = "";
            bool multiline = true;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-h" || args[i] == "--help")
                {
                    OutputHelp();
                    return;
                }
                if (args[i] == "-i" || args[i] == "--install")
                {
                    return;
                }
                if (args[i] == "-t" || args[i] == "--title" || args[i] == "-title")
                {
                    i++;
                    if (i < args.Length)
                    {
                        title = args[i];
                    }
                }
                else if (args[i] == "-p" || args[i] == "--image")
                {
                    i++;
                    if (i < args.Length)
                    {
                        image = args[i];
                    }
                }
                else if (args[i] == "-a" || args[i] == "--audio")
                {
                    i++;
                    if (i < args.Length)
                    {
                        audio = args[i];
                    }
                }
                else if (args[i] == "-x" || args[i] == "--xml")
                {
                    i++;
                    if (i < args.Length)
                    {
                        xml = args[i];
                    }
                }
                else if (args[i] == "-v" || args[i] == "--verbose")
                {
                    Trace.Listeners.Add(new ConsoleTraceListener());
                }
                else if (args[i] == "-m" || args[i] == "--multiline")
                {
                    i++;
                    if (i < args.Length)
                    {
                        try
                        {
                            multiline = int.Parse(args[i]) != 0;
                        }
                        catch (FormatException) {
                        }
                    }
                }

                else if (args[i] == "-desc")
                {
                    i++;
                    if (i < args.Length)
                    {
                        messages.Add(args[i]);
                    }
                }
                else if (args[i].StartsWith("-") || args[i].StartsWith("--"))
                {
                    // Forward-compatibility / compatibility with Scripty:
                    // Reject other arguments that start with "-" or "--".
                    i++;
                }
                else
                {
                    messages.Add(args[i]);
                }
            }

            string message = string.Join(" ", messages);

            if (message == "")
            {
                OutputHelp();
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(xml))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);
                    SendToastNotification(xmlDoc);
                }
                else
                {
                    SendToastNotification(message, title, image, audio, multiline);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured. Have you installed the program properly?");
                Console.WriteLine(ex.Message);
                Environment.ExitCode = 1;
                return;
            }
        }

        /// <summary>
        /// Installs this executable to the user's local application data.
        /// </summary>
        /// <returns>
        /// The path of the installed executable, if the application was installed.
        /// Null, if the path of the installed executable is equal to the path the current program is running from.
        /// </returns>
        public static string SelfInstall()
        {
            string runLocation = Assembly.GetEntryAssembly().Location;
            string installedDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ToastNotifier");
            string installedLocation = Path.Combine(installedDirectory, "ToastNotifier.exe");
            string shortcutDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "ToastNotifier");
            string shortcutLocation = Path.Combine(shortcutDirectory, "ToastNotifier.lnk");

            if (installedLocation != runLocation)
            {
                // Copy myself to AppData/Local
                Directory.CreateDirectory(installedDirectory);
                System.IO.File.Copy(runLocation, installedLocation, true);
            }
            else
            {
                return null;
            }
            // We strongly recommend that you do this in the Windows Installer blah blah blah...
            // Create a shortcut to the installed program.
            Directory.CreateDirectory(shortcutDirectory);
            using (ShellLink shortcut = new ShellLink())
            {
                shortcut.TargetPath = installedLocation;
                shortcut.AppUserModelID = APPUSERMODELID;
                shortcut.Save(shortcutLocation);
            }
            return installedLocation;
        }

        public static void OutputHelp()
        {
            string help = string.Join("\r\n",
                "",
                "Usage:",
                AppDomain.CurrentDomain.FriendlyName + " [options] message",
                AppDomain.CurrentDomain.FriendlyName + " (without arguments) : enter one-line interactive mode",
                "",
                "  -desc [4]                 The message.",
                "  -t, --title, -title [4]   The title.",
                "  -p, --image               Include an image. Should be a URI. [1]",
                "  -a, --audio               Specify audio to play. A URI of the form ms-winsoundevent:*. [2][3] Set to \"silent\" to mute.",
                "  -x, --xml                 Custom XML to send to the notifier. (Overrides everything above.)",
                "  -m, --multiline [0|1]     Multiline support. Enabled by default.",
                "  -i, --install             Installs the program. Use if you want to install the program manually without actually doing anything else.",
                "  -h, --help                Display help.",
                "",
                "Exit codes:",
                "",
                "  1    Generic, consult the error message.",
                "",
                "Note: I will try to automatically install myself. Also, I will pipe any input to the installed version.",
                "",
                "  [1] Notification schema: image element  http://msdn.microsoft.com/en-us/library/windows/apps/br230844.aspx",
                "  [2] The toast audio options catalog     http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh761492.aspx",
                "  [3] Notification schema: audio element  http://msdn.microsoft.com/en-us/library/windows/apps/br230842.aspx",
                "  [4] Additional parameters exist so you can use this program directly with the Scripty display for Growl for Windows.",
                ""
            );
            Console.WriteLine(help);
        }                                 

        public static void SendToastNotification(string message, string title, string image, string audio, bool multiline)
        {
            Trace.WriteLine("Called SendToastNotification with arguments:");
            Trace.WriteLine("message: " + message);
            Trace.WriteLine("title: " + title);
            Trace.WriteLine("image: " + image);
            Trace.WriteLine("audio: " + audio);
            Trace.WriteLine("multiline: " + multiline.ToString());

            ToastTemplateType template;
            var lines = new List<string>(3);
            bool hasTitle = !string.IsNullOrEmpty(title);
            bool hasMessage = !string.IsNullOrEmpty(message);
            bool hasImage = !string.IsNullOrEmpty(image);

            if (hasTitle)
            {
                lines.Add(title);
            }
            if (hasMessage)
            {
                if (multiline)
                {
                    message = message.Replace("\\n", "\n");
                    char[] splitChars = {'\n', '\r'};
                    string[] split = message.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length == 1)
                    {
                        if (hasImage)
                        {
                            template = hasTitle ? ToastTemplateType.ToastImageAndText02 : ToastTemplateType.ToastImageAndText01;
                        }
                        else
                        {
                            template = hasTitle ? ToastTemplateType.ToastText02 : ToastTemplateType.ToastText01;
                        }
                        lines.AddRange(split);
                    }
                    else if (split.Length == 2)
                    {
                        if (hasImage)
                        {
                            template = ToastTemplateType.ToastImageAndText04;
                            lines.AddRange(split);
                        }
                        else
                        {
                            template = ToastTemplateType.ToastText04;
                            lines.AddRange(split);
                        }
                        if (!hasTitle) lines.Insert(0, string.Empty);
                    }
                    else
                    {
                        if (hasTitle) lines.RemoveAt(0);
                        template = hasImage ? ToastTemplateType.ToastImageAndText01 : ToastTemplateType.ToastText01;
                        lines.Add(message);
                    }
                }
                else
                {
                    if (hasImage)
                    {
                        template = hasTitle ? ToastTemplateType.ToastImageAndText02 : ToastTemplateType.ToastImageAndText01;
                        lines.Add(message);
                    }
                    else
                    {
                        template = hasTitle ? ToastTemplateType.ToastText02 : ToastTemplateType.ToastText01;
                    }
                }
            }
            else
            {
                if (hasImage)
                {
                    template = ToastTemplateType.ToastImageAndText03;
                }
                else
                {
                    template = ToastTemplateType.ToastText03;
                }
            }
            Trace.WriteLine("Using template: " + Enum.GetName(typeof(ToastTemplateType), template));
            XmlDocument xml = ToastNotificationManager.GetTemplateContent(template);
            XmlNodeList textElements = xml.GetElementsByTagName("text");
            for (int i = 0; i < textElements.Count; i++)
            {
                textElements.Item((uint)i).AppendChild(xml.CreateTextNode(lines[i]));
            }

            if (!string.IsNullOrEmpty(image))
            {
                XmlElement imageElement = (XmlElement)xml.GetElementsByTagName("image").Item(0);
                imageElement.SetAttribute("src", image);
            }

            if (!string.IsNullOrEmpty(audio)) {
                var toastElement = xml.SelectSingleNode("/toast");
                XmlElement audioElement = xml.CreateElement("audio");
                if (audio == "silent") {
                    audioElement.SetAttribute("silent", "true");
                }
                else {
                    audioElement.SetAttribute("src", audio);
                }
                toastElement.AppendChild(audioElement);
            }

            SendToastNotification(xml);
        }

        public static void SendToastNotification(XmlDocument xml)
        {
            Trace.WriteLine("XML: " + xml.GetXml());
            ToastNotification notification = new ToastNotification(xml);
            var notifier = ToastNotificationManager.CreateToastNotifier(APPUSERMODELID);
            notifier.Show(notification);
        }

        public static string JoinCommandLineArguments(string[] arguments)
        {
            string[] escaped = new string[arguments.Length];
            arguments.CopyTo(escaped, 0);
            for (int i = 0; i < escaped.Length; i++)
            {
                // Credit: http://stackoverflow.com/a/12364234/1149962
                if (string.IsNullOrEmpty(escaped[i])) continue;
                string value = Regex.Replace(escaped[i], @"(\\*)" + "\"", @"$1\$0");
                value = Regex.Replace(value, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"");
                escaped[i] = value;
            }
            return string.Join(" ", escaped);
        }

    }
}
