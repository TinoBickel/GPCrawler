using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;

namespace GpCrawler2 {
  public partial class SongsEntities : IDisposable {
    private readonly SQLiteConnection _connection;
    private readonly SongCollection _songs;
    private readonly List<Songs> _pendingInserts = new List<Songs>();

    public SongsEntities() {
      _connection = new SQLiteConnection(string.Format("Data Source={0};Version=3;", Global.SongDatabasePath));
      _connection.Open();

      EnsureSchema();

      _songs = new SongCollection(this);
      Songs = _songs;
      Database = new SongsDatabaseFacade(this);

      LoadSongs();
    }

    public ICollection<Songs> Songs { get; private set; }

    public SongsDatabaseFacade Database { get; private set; }

    public int SaveChanges() {
      var affectedRows = 0;

      using (var transaction = _connection.BeginTransaction()) {
        foreach (var song in _pendingInserts) {
          using (var command = _connection.CreateCommand()) {
            command.Transaction = transaction;
            command.CommandText =
              "INSERT OR REPLACE INTO Songs (ID, SongName, BandName, FileName, Favorite) " +
              "VALUES (@ID, @SongName, @BandName, @FileName, @Favorite);";

            command.Parameters.AddWithValue("@ID", song.ID);
            command.Parameters.AddWithValue("@SongName", (object)song.SongName ?? DBNull.Value);
            command.Parameters.AddWithValue("@BandName", (object)song.BandName ?? DBNull.Value);
            command.Parameters.AddWithValue("@FileName", (object)song.FileName ?? DBNull.Value);
            command.Parameters.AddWithValue("@Favorite", (object)song.Favorite ?? DBNull.Value);

            affectedRows += command.ExecuteNonQuery();
          }
        }

        transaction.Commit();
      }

      _pendingInserts.Clear();
      return affectedRows;
    }

    public void Dispose() {
      _connection.Dispose();
    }

    internal void RegisterPendingInsert(Songs song) {
      if (song == null) {
        return;
      }

      if (_pendingInserts.Exists(existing => existing.ID == song.ID)) {
        return;
      }

      _pendingInserts.Add(song);
    }

    internal int ExecuteSqlCommand(string sql) {
      using (var command = _connection.CreateCommand()) {
        command.CommandText = sql;
        var affectedRows = command.ExecuteNonQuery();

        if (sql.Trim().StartsWith("DELETE", StringComparison.OrdinalIgnoreCase)) {
          _pendingInserts.Clear();
          _songs.ClearAndResetTracking();
        }

        return affectedRows;
      }
    }

    private void EnsureSchema() {
      using (var command = _connection.CreateCommand()) {
        command.CommandText =
          "CREATE TABLE IF NOT EXISTS Songs (" +
          "ID TEXT NOT NULL PRIMARY KEY, " +
          "SongName TEXT NULL, " +
          "BandName TEXT NULL, " +
          "FileName TEXT NULL, " +
          "Favorite TEXT NULL);" +
          "CREATE INDEX IF NOT EXISTS IX_Songs_BandName ON Songs(BandName);" +
          "CREATE INDEX IF NOT EXISTS IX_Songs_SongName ON Songs(SongName);" +
          "CREATE INDEX IF NOT EXISTS IX_Songs_FileName ON Songs(FileName);";

        command.ExecuteNonQuery();
      }

      EnsureColumnExists("Songs", "Favorite", "TEXT NULL");
    }

    private void LoadSongs() {
      using (var command = _connection.CreateCommand()) {
        command.CommandText =
          "SELECT ID, SongName, BandName, FileName, Favorite " +
          "FROM Songs " +
          "ORDER BY BandName, SongName;";

        using (var reader = command.ExecuteReader()) {
          _songs.BeginLoad();

          while (reader.Read()) {
            _songs.Add(new Songs() {
              ID = ReadString(reader, 0),
              SongName = ReadString(reader, 1),
              BandName = ReadString(reader, 2),
              FileName = ReadString(reader, 3),
              Favorite = ReadString(reader, 4)
            });
          }

          _songs.EndLoad();
        }
      }
    }

    private static string ReadString(SQLiteDataReader reader, int index) {
      return reader.IsDBNull(index) ? null : reader.GetString(index);
    }

    private void EnsureColumnExists(string tableName, string columnName, string columnDefinition) {
      using (var command = _connection.CreateCommand()) {
        command.CommandText = string.Format("PRAGMA table_info({0});", tableName);

        using (var reader = command.ExecuteReader()) {
          while (reader.Read()) {
            if (string.Equals(ReadString(reader, 1), columnName, StringComparison.OrdinalIgnoreCase)) {
              return;
            }
          }
        }
      }

      using (var command = _connection.CreateCommand()) {
        command.CommandText = string.Format("ALTER TABLE {0} ADD COLUMN {1} {2};", tableName, columnName, columnDefinition);
        command.ExecuteNonQuery();
      }
    }

    private sealed class SongCollection : Collection<Songs> {
      private readonly SongsEntities _owner;
      private bool _isLoading;

      public SongCollection(SongsEntities owner) {
        _owner = owner;
      }

      public void BeginLoad() {
        _isLoading = true;
      }

      public void EndLoad() {
        _isLoading = false;
      }

      public void ClearAndResetTracking() {
        BeginLoad();
        Clear();
        EndLoad();
      }

      protected override void InsertItem(int index, Songs item) {
        base.InsertItem(index, item);

        if (!_isLoading) {
          _owner.RegisterPendingInsert(item);
        }
      }
    }
  }

  public sealed class SongsDatabaseFacade {
    private readonly SongsEntities _owner;

    internal SongsDatabaseFacade(SongsEntities owner) {
      _owner = owner;
    }

    public int ExecuteSqlCommand(string sql) {
      return _owner.ExecuteSqlCommand(sql);
    }
  }
}
