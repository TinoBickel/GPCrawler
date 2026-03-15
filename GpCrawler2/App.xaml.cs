using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace GpCrawler2 {
  public partial class App : Application {
    
    private void Application_Exit(object sender, ExitEventArgs e) {
      SaveApplicationSettings();
    }

    private static void SaveApplicationSettings() {
      GpCrawler2.Properties.Settings.Default.Save();
    }

  }
}
