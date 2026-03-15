using FirstFloor.ModernUI.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WPFSoundVisualizationLib;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Serialization;

namespace GpCrawler2.Pages {
  /// <summary>
  /// Interaction logic for Player.xaml
  /// </summary>
  public partial class Player : UserControl , IContent, IWorkspace {

    private string _fileName = String.Empty;

    private string _infoFile {
      get {
        if (!String.IsNullOrEmpty(_fileName)) {
          return Path.GetDirectoryName(_fileName).AppendPath(Path.GetFileNameWithoutExtension(_fileName) + ".gpcinfo");
        }
        return String.Empty;
      }
    }

    public ObservableCollection<TimeSection> TimeSections = new ObservableCollection<TimeSection>();
    
    public Player() {
      InitializeComponent();

      NAudioEngine soundEngine = NAudioEngine.Instance;
      soundEngine.PropertyChanged += NAudioEngine_PropertyChanged;

      UIHelper.Bind(soundEngine, "CanStop", StopButton, Button.IsEnabledProperty);
      UIHelper.Bind(soundEngine, "CanPlay", PlayButton, Button.IsEnabledProperty);
      UIHelper.Bind(soundEngine, "CanPause", PauseButton, Button.IsEnabledProperty);
      UIHelper.Bind(soundEngine, "SelectionBegin", repeatStartTimeEdit, TimeEditor.ValueProperty, BindingMode.TwoWay);
      UIHelper.Bind(soundEngine, "SelectionEnd", repeatStopTimeEdit, TimeEditor.ValueProperty, BindingMode.TwoWay);

      spectrumAnalyzer.RegisterSoundPlayer(soundEngine);
      waveformTimeline.RegisterSoundPlayer(soundEngine);

      LoadExpressionDarkTheme();

      Global.Workspaces.Add(this);
    }

    void Global_OpenSongRequested(object sender, OpenSongEventArgs e) {
      
    }

    #region NAudio Engine Events
    private void NAudioEngine_PropertyChanged(object sender, PropertyChangedEventArgs e) {
      NAudioEngine engine = NAudioEngine.Instance;
      switch (e.PropertyName) {
        case "FileTag":
          if (engine.FileTag != null) {
            TagLib.Tag tag = engine.FileTag.Tag;
            if (tag.Pictures.Length > 0) {
              using (MemoryStream albumArtworkMemStream = new MemoryStream(tag.Pictures[0].Data.Data)) {
                try {
                  BitmapImage albumImage = new BitmapImage();
                  albumImage.BeginInit();
                  albumImage.CacheOption = BitmapCacheOption.OnLoad;
                  albumImage.StreamSource = albumArtworkMemStream;
                  albumImage.EndInit();
                  albumArtPanel.AlbumArtImage = albumImage;
                }
                catch (NotSupportedException) {
                  albumArtPanel.AlbumArtImage = null;
                  // System.NotSupportedException:
                  // No imaging component suitable to complete this operation was found.
                }
                albumArtworkMemStream.Close();
              }
            }
            else {
              albumArtPanel.AlbumArtImage = null;
            }
          }
          else {
            albumArtPanel.AlbumArtImage = null;
          }
          break;
        case "ChannelPosition":
          clockDisplay.Time = TimeSpan.FromSeconds(engine.ChannelPosition);
          break;
        default:
          // Do Nothing
          break;
      }

    }
    #endregion

    private void PlayButton_Click(object sender, RoutedEventArgs e) {
      if (NAudioEngine.Instance.CanPlay)
        NAudioEngine.Instance.Play();
    }

    private void PauseButton_Click(object sender, RoutedEventArgs e) {
      if (NAudioEngine.Instance.CanPause)
        NAudioEngine.Instance.Pause();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e) {
      if (NAudioEngine.Instance.CanStop)
        NAudioEngine.Instance.Stop();
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e) {
      OpenFile();
    }

    private void LoadExpressionDarkTheme() {
      Resources.MergedDictionaries.Clear();
      ResourceDictionary themeResources = Application.LoadComponent(new Uri("ExpressionDark.xaml", UriKind.Relative)) as ResourceDictionary;
      Resources.MergedDictionaries.Add(themeResources);
    }

    private void OpenFile() {
      Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
      openDialog.Filter = "(*.mp3)|*.mp3";
      if (openDialog.ShowDialog() == true) {
        OpenSong(openDialog.FileName);
      }
    }

    private void OpenSong(string fileName) {
      _fileName = fileName;
      NAudioEngine.Instance.OpenFile(fileName);
      FileText.Text = fileName;

      DeserializeTimeSections();
    }

    private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e) {
      OpenFile();
    }

    #region IContent 

    public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e) {
      OpenSong(e.Fragment);
    }

    public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) {    }
    public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e) {    }
    public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e) { }

    #endregion
    
    private void CreateTimesnapshotButton_Click(object sender, RoutedEventArgs e) {
      var timeSection = new TimeSection("Sektion #" + (TimeSections.Count() + 1), this.repeatStartTimeEdit.Value, this.repeatStopTimeEdit.Value);
            
      TimeSections.Add(timeSection);
      UpdateTimeSectionDataContext();
      SerializeTimeSections();
    }

    void timeSection_DeleteRequested(object sender, TimeSection e) {
      TimeSections.Remove(e);
      TimeSectionLister1.DataContext = TimeSections;
      SerializeTimeSections();
    }

    void timeSection_ResetRequested(object sender, TimeSection e) {
      NAudioEngine.Instance.SelectionBegin = e.Start;
      NAudioEngine.Instance.SelectionEnd = e.Stop;

      NAudioEngine.Instance.ActiveStream.CurrentTime = e.Start;
    }
      
    public void HandleKeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.Space) {
        if (NAudioEngine.Instance.CanPlay) {
          NAudioEngine.Instance.Play();
          e.Handled = true;
        }
        else if (NAudioEngine.Instance.CanStop) {
          NAudioEngine.Instance.Stop();
          e.Handled = true;
        }
      }
    }
    
    private void SerializeTimeSections() {
      var songInfo = new SongInfo();
      songInfo.TimeSections = this.TimeSections;

      var settings = new XmlWriterSettings();

      // Set some Properties for a better Human readability
      settings.Indent = true;
      settings.IndentChars = ("\t");
      settings.OmitXmlDeclaration = true;

      try {
        if (File.Exists(_infoFile)) {
          File.Delete(_infoFile);
        }

        using (var stream = new FileStream(_infoFile, FileMode.Create)) {
          using (var writer = XmlWriter.Create(stream, settings)) {
            (new XmlSerializer(typeof(SongInfo))).Serialize(writer, songInfo);
          }
        }
      }
      catch (Exception ex) {
        Global.ReportException("Fehler beim Sichern der TimeSections.", ex);
        return;
      }
    }

    private void DeserializeTimeSections() {
      if (!File.Exists(_infoFile)) {
        return;
      }

      var serializer = new XmlSerializer(typeof(SongInfo));

      using (var reader = new StreamReader(_infoFile)) {
        var songInfo = (SongInfo)serializer.Deserialize(reader);
        TimeSections = songInfo.TimeSections;
        UpdateTimeSectionDataContext();
      }
    }

    private void UpdateTimeSectionDataContext() {
      foreach (var timeSection in TimeSections) {
        timeSection.ResetRequested -= timeSection_ResetRequested;
        timeSection.DeleteRequested -= timeSection_DeleteRequested;
        timeSection.TitleChanged -= timeSection_TitleChanged;

        timeSection.ResetRequested += timeSection_ResetRequested;
        timeSection.DeleteRequested += timeSection_DeleteRequested;
        timeSection.TitleChanged += timeSection_TitleChanged;
      }

      TimeSectionLister1.DataContext = TimeSections;
    }

    void timeSection_TitleChanged(object sender, TimeSection e) {
      SerializeTimeSections();
    }
  }

  
}
