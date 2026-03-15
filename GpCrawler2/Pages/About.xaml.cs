using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
  public partial class About : UserControl {

    public int NumberOfSongs { get; set; }

    public About() {
      InitializeComponent();
      NumberOfSongs = Global.ExistingFiles.Count;
      AboutGrid.DataContext = this;
    }
  }
}
