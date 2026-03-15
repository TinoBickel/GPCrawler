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
using System.Windows.Shapes;
using WPFSoundVisualizationLib;

namespace GpCrawler2 {
  /// <summary>
  /// Interaction logic for Player.xaml
  /// </summary>
  public partial class Player : UserControl {
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

    private void LoadDefaultTheme() {
      DefaultThemeMenuItem.IsChecked = true;
      DefaultThemeMenuItem.IsEnabled = false;
      ExpressionDarkMenuItem.IsChecked = false;
      ExpressionDarkMenuItem.IsEnabled = true;
      ExpressionLightMenuItem.IsChecked = false;
      ExpressionLightMenuItem.IsEnabled = true;

      Resources.MergedDictionaries.Clear();
    }

    private void LoadDarkBlueTheme() {
      DefaultThemeMenuItem.IsChecked = false;
      DefaultThemeMenuItem.IsEnabled = true;
      ExpressionDarkMenuItem.IsChecked = false;
      ExpressionDarkMenuItem.IsEnabled = true;
      ExpressionLightMenuItem.IsChecked = false;
      ExpressionLightMenuItem.IsEnabled = true;

      Resources.MergedDictionaries.Clear();
      ResourceDictionary themeResources = Application.LoadComponent(new Uri("DarkBlue.xaml", UriKind.Relative)) as ResourceDictionary;
      Resources.MergedDictionaries.Add(themeResources);
    }

    private void LoadExpressionDarkTheme() {
      DefaultThemeMenuItem.IsChecked = false;
      DefaultThemeMenuItem.IsEnabled = true;
      ExpressionDarkMenuItem.IsChecked = true;
      ExpressionDarkMenuItem.IsEnabled = false;
      ExpressionLightMenuItem.IsChecked = false;
      ExpressionLightMenuItem.IsEnabled = true;

      Resources.MergedDictionaries.Clear();
      ResourceDictionary themeResources = Application.LoadComponent(new Uri("ExpressionDark.xaml", UriKind.Relative)) as ResourceDictionary;
      Resources.MergedDictionaries.Add(themeResources);
    }

    private void LoadExpressionLightTheme() {
      DefaultThemeMenuItem.IsChecked = false;
      DefaultThemeMenuItem.IsEnabled = true;
      ExpressionDarkMenuItem.IsChecked = false;
      ExpressionDarkMenuItem.IsEnabled = true;
      ExpressionLightMenuItem.IsChecked = true;
      ExpressionLightMenuItem.IsEnabled = false;

      Resources.MergedDictionaries.Clear();
      ResourceDictionary themeResources = Application.LoadComponent(new Uri("ExpressionLight.xaml", UriKind.Relative)) as ResourceDictionary;
      Resources.MergedDictionaries.Add(themeResources);
    }

    private void DefaultThemeMenuItem_Checked(object sender, RoutedEventArgs e) {
      LoadDefaultTheme();
    }

    private void ExpressionDarkMenuItem_Checked(object sender, RoutedEventArgs e) {
      LoadExpressionDarkTheme();
    }

    private void ExpressionLightMenuItem_Checked(object sender, RoutedEventArgs e) {
      LoadExpressionLightTheme();
    }

    private void OpenFile() {
      Microsoft.Win32.OpenFileDialog openDialog = new Microsoft.Win32.OpenFileDialog();
      openDialog.Filter = "(*.mp3)|*.mp3";
      if (openDialog.ShowDialog() == true) {
        NAudioEngine.Instance.OpenFile(openDialog.FileName);
        FileText.Text = openDialog.FileName;
      }
    }

    private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e) {
      OpenFile();
    }

    private void CloseMenuItem_Click(object sender, RoutedEventArgs e) {
      Close();
    }

    public void Close() {
      NAudioEngine.Instance.Dispose();
    }
  }
}
