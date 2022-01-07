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
            GetDatabaseConnection<AgentsDatabase.AgentDatabaseModel>().ConfigureAwait(false);
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
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var teamDatabaseModel = TeamDatabaseModel.ToTeamDatabaseModel(teamModel);

                databaseConnection.GetConnection().InsertOrReplaceWithChildren(teamDatabaseModel, true);

                return 0;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying TeamsDatabase.SaveTeamModel");

                return 1;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<int> SaveTeamsModels(IList<TeamModel> teams)
        {
            LoggingService.Trace("Querying TeamsDatabase.SaveTeamsModels");

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var teamsDatabaseModels = new List<TeamDatabaseModel>(
                    teams.Select(model => TeamDatabaseModel.ToTeamDatabaseModel(model)));

                databaseConnection.GetConnection().InsertOrReplaceAllWithChildren(teamsDatabaseModels, true);

                return 0;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying TeamsDatabase.SaveTeamsModels");

                return 1;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<TeamModel?> GetTeam(string teamId)
        {
            LoggingService.Trace("Querying TeamsDatabase.GetTeam");
            
            if (string.IsNullOrEmpty(teamId))
                return null;

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var teamDatabaseModel = databaseConnection.GetConnection().GetWithChildren<TeamDatabaseModel>(teamId);

                return teamDatabaseModel != null ?
                    TeamDatabaseModel.ToTeamModel(teamDatabaseModel) :
                    null;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying TeamsDatabase.GetTeam");

                return null;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<List<TeamModel>> GetTeamsForAgent(string agentId)
        {
            LoggingService.Trace("Querying TeamsDatabase.GetTeamsForAgent");

            if (string.IsNullOrEmpty(agentId))
                return new List<TeamModel>();
            
            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var teamDatabaseModels = databaseConnection.GetConnection().GetAllWithChildren<TeamDatabaseModel>();

                return teamDatabaseModels != null && teamDatabaseModels.Any() ?
                    teamDatabaseModels
                        .Where(model => model.Agents.Any(a => a.AgentId != null && a.AgentId.Equals(agentId)))
                        .Select(x => TeamDatabaseModel.ToTeamModel(x))
                        .ToList() :
                    new List<TeamModel>();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying TeamsDatabase.GetTeamsForAgent");

                return new List<TeamModel>();
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<int> DeleteTeam(string teamId)
        {
            LoggingService.Trace("Querying TeamsDatabase.DeleteTeam");

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                return databaseConnection.GetConnection().Delete<TeamDatabaseModel>(teamId);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying TeamsDatabase.DeleteTeam");

                return -1;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<int> CountTeams()
        {
            LoggingService.Trace("Querying TeamsDatabase.CountTeams");

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();

            try
            {
                var result = databaseConnection.GetConnection().Table<TeamDatabaseModel>().Count();
                return result;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying TeamsDatabase.DeleteTeam");

                return 0;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

#nullable disable
        internal class TeamDatabaseModel
        {
            [PrimaryKey, Unique]
            public string TeamId { get; set; }

            public string Name { get; set; }

            [ManyToMany(typeof(TeamAgent), CascadeOperations = CascadeOperation.All)]
            public List<AgentsDatabase.AgentDatabaseModel> Agents { get; set; }

            public DateTime DownloadedAt { get; set; }

            public static TeamModel ToTeamModel(TeamDatabaseModel teamDatabaseModel)
            {
                return new TeamModel()
                {
                    Id = teamDatabaseModel.TeamId,
                    Name = teamDatabaseModel.Name,
                    Agents = teamDatabaseModel.Agents?.Select(agentDbModel => AgentsDatabase.AgentDatabaseModel.ToTeamAgentModel(agentDbModel)).ToList(),
                    DownloadedAt = teamDatabaseModel.DownloadedAt
                };
            }

            public static TeamDatabaseModel ToTeamDatabaseModel(TeamModel teamModel)
            {
                return new TeamDatabaseModel
                {
                    TeamId = teamModel.Id,
                    Name = teamModel.Name,
                    Agents = teamModel.Agents?.Select(agentModel => AgentsDatabase.AgentDatabaseModel.ToAgentDatabaseModel(agentModel)).ToList(),
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

        [ForeignKey(typeof(AgentsDatabase.AgentDatabaseModel))]
        public string AgentId { get; set; }
    }
#nullable enable
}