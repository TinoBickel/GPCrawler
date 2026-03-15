using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GpCrawler2 {
  [Serializable]
  [XmlRoot("SongInfo")]
  public class SongInfo {
    public ObservableCollection<TimeSection> TimeSections { get; set; }

    public SongInfo() {

    }
  }
}
