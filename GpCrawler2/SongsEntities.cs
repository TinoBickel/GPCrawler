using System;

namespace GpCrawler2 {
  public partial class SongsEntities {

    public static SongsEntities GetContext() {
      Global.InitializeApplicationData();
      return new SongsEntities();
    }
  }
}
