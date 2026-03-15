using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GpCrawler2 {
  [Serializable]
  public class TimeSection : EventArgs {

    #region ResetRequested

    public event EventHandler<TimeSection> ResetRequested;

    public void Reset() {
      if (ResetRequested != null) {
        ResetRequested(this, this);
      }
    }

    #endregion

    #region DeleteRequested

    public event EventHandler<TimeSection> DeleteRequested;

    public void OnDeleted() {
      if (DeleteRequested != null) {
        DeleteRequested(this, this);
      }
    }

    #endregion

    #region TitleChanged

    public event EventHandler<TimeSection> TitleChanged;

    public void OnTitleChanged() {
      if (TitleChanged != null) {
        TitleChanged(this, this);
      }
    }

    #endregion
    
    [XmlAttribute]
    public string Title { get; set; }

    [XmlIgnore]
    public TimeSpan Start { get; set; }

    [XmlIgnore]
    public TimeSpan Stop { get; set; }

    [XmlAttribute("Start")]
    public long StartTicks {
      get { return Start.Ticks; }
      set { this.Start = new TimeSpan(value); }
    }

    [XmlAttribute("Stop")]
    public long StopTicks {
      get { return Stop.Ticks; }
      set { this.Stop = new TimeSpan(value); }
    }
    
    public string SubTitle {
      get {
        var startTime = String.Format("{0:D2}:{1:D2}", Start.Minutes, Start.Seconds);
        var stopTime = String.Format("{0:D2}:{1:D2}", Stop.Minutes, Stop.Seconds);

        return String.Format("{0} - {1}", startTime, stopTime);
      }
    }

    public TimeSection() {
      // For serialization only
    }

    public TimeSection(string title, TimeSpan start, TimeSpan stop) {
      this.Start = start;
      this.Stop = stop;
      this.Title = title;
    }

    public override string ToString() {
      return Title;
    }
      
  }
}
