using SolidShineUi;
using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System;

namespace PropertyBuilderWPF
{
    /// <summary>
    /// The main entry point into the app.
    /// </summary>
    public partial class App : Application
    {

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            LoadSettings();

            // CSharp highlighter from https://github.com/icsharpcode/AvalonEdit/blob/30cad99ce905412ed5f5e847e3c00e72e69aee77/ICSharpCode.AvalonEdit/Highlighting/Resources/CSharp-Mode.xshd
            using (Stream? s = GetResourceStream("Highlighting.CSharp.xshd"))
            {
                if (s != null)
                {
                    using XmlReader reader = new XmlTextReader(s);
                    CsharpHighlighter = HighlightingLoader.LoadXshd(reader);
                }
            }

            using (Stream? s = GetResourceStream("Highlighting.None.xshd"))
            {
                if (s != null)
                {
                    using XmlReader reader = new XmlTextReader(s);
                    NoneHighlighter = HighlightingLoader.LoadXshd(reader);
                }
            }

            MainWindow mw = new MainWindow();
            MainWindow = mw;
            mw.Show();
        }

        public static XshdSyntaxDefinition? CsharpHighlighter { get; set; } = null;

        public static XshdSyntaxDefinition? NoneHighlighter { get; set; } = null;

        /// <summary>
        /// Get an embedded resource in this application. Note that subfolders should be changed to a "<c>.</c>" (so <c>"MyFolder.resource.txt"</c>).
        /// </summary>
        /// <param name="resourceName">the filename of the embedded resource</param>
        /// <returns>a stream that contains the embedded resource, if found; otherwise, null</returns>
        public static Stream? GetResourceStream(string resourceName)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("PropertyBuilderWPF." + resourceName);
        }

        public static void OpenBrowserLink(Uri link)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(link.AbsoluteUri) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", link.AbsoluteUri);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", link.AbsoluteUri);
            }
        }

        /// <summary>
        /// A representation of all the settings for this program.
        /// </summary>
        public static Settings AppSettings { get; set; } = new Settings();

        /// <summary>
        /// The location where the settings file is stored.
        /// </summary>
        public static string SettingsFilePath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JaykeBird", "PropertyBuilder", "settings.json");

        // PropertyBuilder currently doesn't use the Qhuill settings system, so instead I'm storing the settings in the program's data folder

        /// <summary>
        /// Load the settings from the specific settings file. If the settings file does not exist or cannot be read, then create a new Settings instance.
        /// </summary>
        public static void LoadSettings()
        {
            FileInfo fi = new(SettingsFilePath);
            AppSettings = fi.Exists ? fi.Length > 0 ? Settings.LoadSettings(SettingsFilePath) ?? new Settings() : new Settings() : new Settings();
        }

        /// <summary>
        /// Save the current settings to the settings file.
        /// </summary>
        public static void SaveSettings()
        {
            AppSettings.Save(SettingsFilePath);
        }
    }

}
