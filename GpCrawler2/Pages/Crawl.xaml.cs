using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GpCrawler2.Pages {
  /// <summary>
  /// Interaction logic for Crawl.xaml
  /// </summary>
  public partial class Crawl : UserControl, INotifyPropertyChanged {

    #region DataDir

    public string DataDir {
      get {
        return _DataDir;
      }
      set {
        if (_DataDir != value) {
          _DataDir = value;
          Properties.Settings.Default["DataDir"] = _DataDir;
          Properties.Settings.Default.Save();
          this.OnPropertyChanged("DataDir");
        }
      }
    }
    private string _DataDir;

    #endregion
    
    public Crawl() {
      InitializeComponent();
      this.CrawlGrid.DataContext = this;
      this.DataDir = Properties.Settings.Default.DataDir;
    }

    private void StartCrawlButton_Click(object sender, RoutedEventArgs e) {
      ThreadPool.QueueUserWorkItem(ExamineDirectory);
    }

    private void ExamineDirectory(object state) {
      DirectoryInfo di = new DirectoryInfo(DataDir);

      var gpFiles = di.GetFiles("*.*", SearchOption.AllDirectories)
                      .Where(file =>  file.Extension.ToLower().Contains("gp") ||
                                      file.Extension.ToLower().Contains("mp3") ||
                                      file.Extension.ToLower().Contains("wav"));

      double counter = 0;
      double total = gpFiles.Count();

      using (var db = SongsEntities.GetContext()) {
        foreach (FileInfo gpFile in gpFiles) {
          if (!Global.ExistingFiles.Contains(gpFile.FullName.ToLower())) {
            var song = GetSongFromFile(gpFile);
            db.Songs.Add(song);

            // Intermediate Save to prevent memory problems
            if (counter % 1000 == 0) {
              try {
                db.SaveChanges();
              }
              catch (DbEntityValidationException ex) {
                HandleDbEntityValidationException(ex);
                return;
              }
              this.Dispatcher.BeginInvoke(new Action<double, double>(UpdateStatus), counter, total);
            }
          }

          counter++;
        }

        db.SaveChanges();
        this.Dispatcher.BeginInvoke(new Action<double, double>(UpdateStatus), total, total);
      }
    }

    private void UpdateStatus(double counter, double total) {
      this.InfoTextBlock.Text = counter + "/" + total;
      this.Progress.Maximum = total;
      this.Progress.Value = counter;
    }

    private static void HandleDbEntityValidationException(DbEntityValidationException ex) {
      var errorMessage = new StringBuilder();

      foreach (var eve in ex.EntityValidationErrors) {
        errorMessage.AppendLine(String.Format("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State));

        foreach (var ve in eve.ValidationErrors) {
          errorMessage.AppendLine(String.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage));
        }
      }

      MessageBox.Show(errorMessage.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private Songs GetSongFromFile(FileInfo fileInfo) {
      var tokens = fileInfo.FullName.Split(new char[] { '\\' });

      Songs song = new Songs();
      song.ID = Guid.NewGuid().ToString();
      song.BandName = tokens[tokens.Length - 2];
      song.SongName = ReplaceString(tokens[tokens.Length - 1], song.BandName + " - ", "", StringComparison.OrdinalIgnoreCase);
      //song.SongName = tokens[tokens.Length - 1].Replace(song.BandName + " - ", "");
      song.FileName = fileInfo.FullName;

      return song;
    }

    private void ClearDatabaseButton_Click(object sender, RoutedEventArgs e) {
      using (var db = SongsEntities.GetContext()) {
        db.Database.ExecuteSqlCommand("Delete From [Songs]");
        db.SaveChanges();
        Global.OnReloadSongsRequested();
      }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) {
      if (PropertyChanged != null) {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    public static string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison) {
      StringBuilder sb = new StringBuilder();

      int previousIndex = 0;
      int index = str.IndexOf(oldValue, comparison);
      while (index != -1) {
        sb.Append(str.Substring(previousIndex, index - previousIndex));
        sb.Append(newValue);
        index += oldValue.Length;

        previousIndex = index;
        index = str.IndexOf(oldValue, index, comparison);
      }
      sb.Append(str.Substring(previousIndex));

      return sb.ToString();
    }

  }
}
