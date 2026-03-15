using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace GpCrawler2.Controls {
  /// <summary>
  /// Interaction logic for TimeSectionLister.xaml
  /// </summary>
  public partial class TimeSectionLister : UserControl {
    public TimeSectionLister() {
      InitializeComponent();
      DataContextChanged += TimeSectionLister_DataContextChanged;
    }

    void TimeSectionLister_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
      TimeSectionsListerCtrl.ItemsSource = e.NewValue as ObservableCollection<TimeSection>;
    }

    private void TextBox_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e) {
      //var textBox = sender as TextBox;
      //textBox.Cursor = textBox.Focusable ? Cursors.IBeam : Cursors.Arrow;
    }

    private string _lastText = null;

    private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      var textBox = sender as TextBox;
      _lastText = textBox.Text;
      textBox.Focusable = true;
      textBox.Focus();
      textBox.Background = new SolidColorBrush(Colors.White);
      textBox.Foreground = new SolidColorBrush(Colors.Black);
      //textBox.CaretIndex = textBox.Text.Length;
      textBox.SelectAll();
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs e) {
      if (e.Key == Key.Enter || e.Key == Key.Return || e.Key == Key.Escape) {
        var textBox = sender as TextBox;
        var section = textBox.DataContext as TimeSection;

        if (e.Key == Key.Escape || String.IsNullOrEmpty(textBox.Text)) {
          textBox.Text = _lastText;
        }
        else {
          section.OnTitleChanged();
        }
        
        ResetTextbox(textBox);
      }
    }

    private void ResetTextbox(TextBox textBox) {
      textBox.Focusable = false;
      textBox.Background = new SolidColorBrush(Colors.Transparent);
      textBox.Foreground = new SolidColorBrush(Colors.White);
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e) {
      ((sender as Button).DataContext as TimeSection).Reset();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e) {
      var button = sender as Button;
      var section = button.DataContext as TimeSection;
      var question = String.Format("[{0}] Wirklich löschen?", section.Title);

      if (MessageBox.Show(question, "Löschen?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
        section.OnDeleted();
      }
    }
  }
}
