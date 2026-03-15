using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows;

namespace GpCrawler2 {
  public partial class SongsEntities {

    public static SongsEntities GetContext() {
			// Configure an SQL CE connection string 
			var filePath = @"d:\temp\songs.db3"; //Global.SongDatabasePath;
			//var sqlCeConnectionString = string.Format("Data Source={0}", filePath);
			var sqlCeConnectionString = string.Format("Data Source = {0}", filePath);

			// Create an EDM connection
			var builder = new SQLiteConnectionStringBuilder() {
				DataSource = filePath,				
			};

			//   builder.Metadata = "res://*/SongModel.csdl|res://*/SongModel.ssdl|res://*/SongModel.msl";
			////builder.Provider = "System.Data.SqlServerCe.4.0";
			//builder.Provider = "System.Data.SQLite.EF6";

			//builder.ProviderConnectionString = sqlCeConnectionString;

			var edmConnectionString = builder.ConnectionString;

      return new SongsEntities(edmConnectionString);
    }

    public SongsEntities(string connectionString)
      : base(connectionString) {
    }
  }
}
