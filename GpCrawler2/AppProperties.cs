using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace GpCrawler2 {
  public class AppProperties {

    [Category("Data")]
    [DisplayName("Daten-Verzeichnis")]
    public string DataDirectory { get; set; }

    [DisplayName("Background")]
    public string LastName { get; set; }

  }
}
