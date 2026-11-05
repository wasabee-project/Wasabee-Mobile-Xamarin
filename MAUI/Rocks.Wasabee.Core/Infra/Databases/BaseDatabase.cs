using Polly;
using Rocks.Wasabee.Core.Adapters;
using SQLite;

namespace Rocks.Wasabee.Core.Infra.Databases
{
    public abstract class BaseDatabase
    {
        public static string Name => $"{nameof(Wasabee)}V2.db3";

        protected BaseDatabase(IFileSystemAdapter fileSystem, TimeSpan expiresAt)
        {
            ExpiresAt = expiresAt;

            var databasePath = Path.Combine(fileSystem.AppDataDirectory, BaseDatabase.Name);
            DatabaseConnection = new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            DatabaseConnection.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
        }

        public TimeSpan ExpiresAt { get; }

        SQLiteAsyncConnection DatabaseConnection { get; }

        public abstract Task<int> DeleteAllData();

        protected static Task<T> AttemptAndRetry<T>(Func<Task<T>> action, int numRetries = 12)
        {
            return Policy.Handle<SQLiteException>().WaitAndRetryAsync(numRetries, pollyRetryAttempt).ExecuteAsync(action);

            static TimeSpan pollyRetryAttempt(int attemptNumber) => TimeSpan.FromMilliseconds(Math.Pow(2, attemptNumber));
        }

        protected bool IsExpired(in DateTimeOffset downloadedAt) => downloadedAt.CompareTo(DateTimeOffset.UtcNow.Subtract(ExpiresAt)) <= 0;

        protected async ValueTask<SQLiteAsyncConnection> GetDatabaseConnection<T>()
        {
            if (DatabaseConnection.TableMappings.All(x => x.MappedType != typeof(T)))
            {
                try
                {
                    await DatabaseConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
                }
                catch (SQLiteException e) when (e.Message.Contains("PRIMARY KEY"))
                {
                    await DatabaseConnection.DropTableAsync(DatabaseConnection.TableMappings.First(x => x.MappedType == typeof(T)));
                    await DatabaseConnection.CreateTablesAsync(CreateFlags.None, typeof(T)).ConfigureAwait(false);
                }
            }

            return DatabaseConnection;
        }
    }
}