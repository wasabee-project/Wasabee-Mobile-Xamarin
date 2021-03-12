using Polly;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using SQLite;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.Infra.Databases
{
    /// <summary>
    /// https://raw.githubusercontent.com/brminnick/GitTrends/master/GitTrends/Database/BaseDatabase.cs
    /// </summary>
    public abstract class BaseDatabase
    {
        public static string Name => $"{nameof(Wasabee)}.db3";

        protected readonly ILoggingService LoggingService;

        protected BaseDatabase(IFileSystem fileSystem, ILoggingService loggingService, TimeSpan expiresAt)
        {
            LoggingService = loggingService;
            ExpiresAt = expiresAt;

            var databasePath = Path.Combine(fileSystem.AppDataDirectory, BaseDatabase.Name);
            DatabaseConnection = new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
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
                await DatabaseConnection.EnableWriteAheadLoggingAsync().ConfigureAwait(false);

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