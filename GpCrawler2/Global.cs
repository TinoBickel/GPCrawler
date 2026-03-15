using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace GpCrawler2 {
  public static class Global {

    #region ReloadSongsRequested

    public static event EventHandler<EventArgs> ReloadSongsRequested;

    public static void OnReloadSongsRequested() {
      if (ReloadSongsRequested != null) {
        ReloadSongsRequested(null, EventArgs.Empty);
      }
    }

    #endregion

    public static List<IWorkspace> Workspaces = new List<IWorkspace>();

    public static HashSet<string> ExistingFiles { get; set; }

    public static string AppDataDir {
      get {
        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      }
    }

    public static string StartupDir {
      get {
        //return Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        return System.AppDomain.CurrentDomain.BaseDirectory;
      }
    }

    public static string ProfileDir {
      get {
        return AppDataDir.AppendPath("GpCrawler");
      }
    }

    public static string SongDatabasePath {
      get {
        return ProfileDir.AppendPath("Songs.db3");
      }
    }

    public static void InitializeApplicationData() {
      CreateProfileIfNeeded();
      AppDomain.CurrentDomain.SetData("DataDirectory", ProfileDir);
    }

    public static void CreateProfileIfNeeded() {
      if (!Directory.Exists(ProfileDir)) {
        Directory.CreateDirectory(ProfileDir);
      }

      if (!File.Exists(SongDatabasePath)) {
        var startupDatabasePath = StartupDir.AppendPath("Songs.db3");

        if (File.Exists(startupDatabasePath)) {
          File.Copy(startupDatabasePath, SongDatabasePath);
        }
      }
    }

    public static void OpenSong(Songs song) {
      var newPage = new Uri(String.Format("/Pages/Player.xaml#{0}", song.FileName), UriKind.RelativeOrAbsolute);
      NavigationCommands.GoToPage.Execute(newPage, null);
    }

    public static void ReportException(string message, Exception ex) {
      if (System.Windows.MessageBox.Show(message + ".\nGrund: " + ex.Message + "\n\nMöchten Sie den Fehlerbericht öffnen?", "Fehler", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes) {
        var dumpfile = Path.GetTempPath().AppendPath("GpCrawler_" + DateTime.UtcNow.ToString("yyMMddHHmmsss") + ".txt");
        File.WriteAllText(dumpfile, ex.ToString());

        Process.Start(dumpfile);
      }
    }
  }
}
