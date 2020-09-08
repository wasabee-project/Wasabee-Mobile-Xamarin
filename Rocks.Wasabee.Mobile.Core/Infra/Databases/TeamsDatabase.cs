using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Models.Teams;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.Infra.Databases
{
    public class TeamsDatabase : BaseDatabase
    {
        public TeamsDatabase(IFileSystem fileSystem, ILoggingService loggingService) : base(fileSystem, loggingService, TimeSpan.FromDays(7))
        {
        }

        public override async Task<int> DeleteAllData()
        {
            LoggingService.Trace("Querying TeamsDatabase.DeleteAllData");

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            return await databaseConnection.DeleteAllAsync<TeamDatabaseModel>().ConfigureAwait(false);
        }

        public async Task<int> SaveTeamModel(TeamModel teamModel)
        {
            LoggingService.Trace("Querying TeamsDatabase.SaveTeamModel");

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var teamDatabaseModel = TeamDatabaseModel.ToTeamDatabaseModel(teamModel);

            var dbLock = databaseConnection.GetConnection().Lock();

            try
            {
                databaseConnection.GetConnection().InsertOrReplaceWithChildren(teamDatabaseModel, true);
            }
            catch (Exception e)
            {
                LoggingService.Error("Error Querying TeamsDatabase.SaveTeamModel", e);

                return 1;
            }

            dbLock.Dispose();

            return 0;
        }

        public async Task<List<TeamModel>> GetTeams(string userId)
        {
            LoggingService.Trace("Querying TeamsDatabase.GetTeams");

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            var teamDatabaseModels = databaseConnection.GetConnection().GetAllWithChildren<TeamDatabaseModel>();
            dbLock.Dispose();

            return teamDatabaseModels
                .Where(x => x.Agents.Any(a => a.Id.Equals(userId)))
                .Select(x => TeamDatabaseModel.ToTeamModel(x))
                .ToList();
        }

        class TeamDatabaseModel
        {
            [PrimaryKey, Unique]
            public string Id { get; set; }

            [Indexed]
            public string Name { get; set; }

            public string AgentsBlobbed { get; set; }
            [TextBlob("AgentsBlobbed")]
            public List<TeamAgentModel> Agents { get; set; }

            public DateTime DownloadedAt { get; set; }

            public static TeamModel ToTeamModel(TeamDatabaseModel teamDatabaseModel)
            {
                return new TeamModel()
                {
                    Id = teamDatabaseModel.Id,
                    Name = teamDatabaseModel.Name,
                    Agents = teamDatabaseModel.Agents,
                    DownloadedAt = teamDatabaseModel.DownloadedAt
                };
            }

            public static TeamDatabaseModel ToTeamDatabaseModel(TeamModel teamModel)
            {
                return new TeamDatabaseModel
                {
                    Id = teamModel.Id,
                    Name = teamModel.Name,
                    Agents = teamModel.Agents,
                    DownloadedAt = teamModel.DownloadedAt
                };
            }
        }
    }
}