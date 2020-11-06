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
            GetDatabaseConnection<TeamAgent>().ConfigureAwait(false);
            GetDatabaseConnection<TeamAgentsDatabase.TeamAgentDatabaseModel>().ConfigureAwait(false);
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
                LoggingService.Error(e, "Error Querying TeamsDatabase.SaveTeamModel");

                return 1;
            }

            dbLock.Dispose();

            return 0;
        }

        public async Task<int> SaveTeamsModels(IList<TeamModel> teams)
        {
            LoggingService.Trace("Querying TeamsDatabase.SaveTeamsModels");

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var teamsDatabaseModels = new List<TeamDatabaseModel>(
                teams.Select(model => TeamDatabaseModel.ToTeamDatabaseModel(model)));

            var dbLock = databaseConnection.GetConnection().Lock();

            try
            {
                databaseConnection.GetConnection().InsertOrReplaceAllWithChildren(teamsDatabaseModels, true);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying TeamsDatabase.SaveTeamsModels");

                return 1;
            }

            dbLock.Dispose();

            return 0;
        }

        public async Task<TeamModel?> GetTeam(string teamId)
        {
            LoggingService.Trace("Querying TeamsDatabase.GetTeam");

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            var teamDatabaseModel = databaseConnection.GetConnection().GetWithChildren<TeamDatabaseModel>(teamId);
            dbLock.Dispose();

            return teamDatabaseModel != null ?
                TeamDatabaseModel.ToTeamModel(teamDatabaseModel) :
                null;
        }

        public async Task<List<TeamModel>> GetTeams(string userId)
        {
            LoggingService.Trace("Querying TeamsDatabase.GetTeams");

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            var teamDatabaseModels = databaseConnection.GetConnection().GetAllWithChildren<TeamDatabaseModel>();
            dbLock.Dispose();

            return teamDatabaseModels
                .Where(x => x.Agents.Any(a => a.AgentId != null && a.AgentId.Equals(userId)))
                .Select(x => TeamDatabaseModel.ToTeamModel(x))
                .ToList();
        }

#nullable disable
        internal class TeamDatabaseModel
        {
            [PrimaryKey, Unique]
            public string TeamId { get; set; }

            public string Name { get; set; }

            [ManyToMany(typeof(TeamAgent), CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
            public List<TeamAgentsDatabase.TeamAgentDatabaseModel> Agents { get; set; }

            public DateTime DownloadedAt { get; set; }

            public static TeamModel ToTeamModel(TeamDatabaseModel teamDatabaseModel)
            {
                return new TeamModel()
                {
                    Id = teamDatabaseModel.TeamId,
                    Name = teamDatabaseModel.Name,
                    Agents = teamDatabaseModel.Agents?.Select(agentDbModel => TeamAgentsDatabase.TeamAgentDatabaseModel.ToTeamAgentModel(agentDbModel)).ToList(),
                    DownloadedAt = teamDatabaseModel.DownloadedAt
                };
            }

            public static TeamDatabaseModel ToTeamDatabaseModel(TeamModel teamModel)
            {
                return new TeamDatabaseModel
                {
                    TeamId = teamModel.Id,
                    Name = teamModel.Name,
                    Agents = teamModel.Agents?.Select(agentModel => TeamAgentsDatabase.TeamAgentDatabaseModel.ToTeamAgentDatabaseModel(agentModel)).ToList(),
                    DownloadedAt = teamModel.DownloadedAt
                };
            }
        }
#nullable enable
    }
    
#nullable disable
    internal class TeamAgent
    {	   
        [ForeignKey(typeof(TeamsDatabase.TeamDatabaseModel))]
        public string TeamId { get; set; }	

        [ForeignKey(typeof(TeamAgentsDatabase.TeamAgentDatabaseModel))]
        public string AgentId { get; set; }
    }
#nullable enable
}