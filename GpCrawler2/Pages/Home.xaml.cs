using FirstFloor.ModernUI.Windows.Navigation;
using GpCrawler2.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.Diagnostics;
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

namespace GpCrawler2 {
  public partial class Home : UserControl, INotifyPropertyChanged {
    
    #region Properties

    #region ExistingSongs

    public List<Songs> ExistingSongs {
      get {
        return _ExistingSongs;
      }
      set {
        if (_ExistingSongs != value) {
          _ExistingSongs = value;
          this.OnPropertyChanged("ExistingSongs");
        }
      }
    }
    private List<Songs> _ExistingSongs;

    #endregion

    #region ApplicationProperties

    public AppProperties ApplicationProperties {
      get {
        return _ApplicationProperties;
      }
      set {
        if (_ApplicationProperties != value) {
          _ApplicationProperties = value;
          this.OnPropertyChanged("ApplicationProperties");
        }
      }
    }
    private AppProperties _ApplicationProperties;

    #endregion

    #region FilterText

    public string FilterText {
      get {
        return _FilterText;
      }
      set {
        if (_FilterText != value) {
          _FilterText = value;
          this.OnPropertyChanged("FilterText");
          ExistingSongsView.Refresh();
        }
      }
    }
    private string _FilterText;

    #endregion
    
    #region ExistingSongsView

    public ICollectionView  ExistingSongsView { get; set; }

    #endregion

    #endregion

    public Home() {
      InitializeComponent();
      BuildExistingFilesList();
      InitializeCollectionView();
      SubscribeToEvents();
      this.HomeGrid.DataContext = this;
    }

    private void SubscribeToEvents() {
      Global.ReloadSongsRequested += Global_ReloadSongsRequested;
    }

    void Global_ReloadSongsRequested(object sender, EventArgs e) {
      BuildExistingFilesList();
    }

    private void InitializeCollectionView() {
      ExistingSongsView = CollectionViewSource.GetDefaultView(ExistingSongs);
      ExistingSongsView.Filter = CollectionViewSource_Filter;

      ExistingSongsView.SortDescriptions.Clear();
      ExistingSongsView.SortDescriptions.Add(new SortDescription("BandName", ListSortDirection.Ascending));
      ExistingSongsView.SortDescriptions.Add(new SortDescription("SongName", ListSortDirection.Ascending));
    }

    private void BuildExistingFilesList() {
      Global.ExistingFiles = new HashSet<string>();

      using (var db = SongsEntities.GetContext()) {
        ExistingSongs = db.Songs.ToList();

        foreach (var song in ExistingSongs) {
          Global.ExistingFiles.Add(song.FileName.ToLower());
        }
      }
    }
   
    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) {
      if (PropertyChanged != null) {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }

    private bool CollectionViewSource_Filter(object item) {
      Songs song = item as Songs;
      if (song != null) {
        if (String.IsNullOrWhiteSpace(FilterText)) {
          return true;
        }
        if (song.FileName.ToUpper().Contains(FilterText.ToUpper())) {
          return true;
        }
      }
      return false;
    }
      
    private void UserControl_Loaded(object sender, RoutedEventArgs e) {
      this.SongFilterTextBox.Focus();
    }

    private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.F2) {
        this.SongFilterTextBox.Focus();
      }
      if (e.Key == Key.Escape) {
        this.FilterText = "";
        this.SongFilterTextBox.Focus();
      }
      if (e.Key == Key.Enter) {
        OpenSelectedItem();
      }
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      OpenSelectedItem();
    }

    private void OpenSelectedItem() {
      if (SongDataGrid.SelectedItem == null) {
        return;
      }

      var selectedSong = SongDataGrid.SelectedItem as Songs;

      if (Path.GetExtension(selectedSong.FileName).ToLower().Contains("gp")) {
        // On GP file we perform a shell execute
        Process.Start(selectedSong.FileName);
      }
      else {
        Global.OpenSong(selectedSong);
      }
    }
  }
}
