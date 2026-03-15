using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GpCrawler2 {
  public class OpenSongEventArgs : EventArgs {
    public Songs Song { get; private set; }

    public OpenSongEventArgs(Songs song) {
      Song = song;
    }
  }
}
