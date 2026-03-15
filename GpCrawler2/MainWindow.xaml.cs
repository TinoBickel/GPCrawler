using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using FirstFloor.ModernUI.Windows.Controls;

namespace GpCrawler2 {
  public partial class MainWindow : ModernWindow {
    public MainWindow() {
      InitializeComponent();
      Global.CreateProfileIfNeeded();
    }

    protected override void OnClosing(CancelEventArgs e) {
      NAudioEngine.Instance.Dispose();
      base.OnClosing(e);
    }

    private void ModernWindow_PreviewKeyDown(object sender, KeyEventArgs e) {
      foreach (var workspace in Global.Workspaces) {
        if (!e.Handled) {
          workspace.HandleKeyDown(sender, e);
        }
      }
    }
  }
}
