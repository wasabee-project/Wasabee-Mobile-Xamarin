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
    public class TeamAgentsDatabase : BaseDatabase
    {
        public TeamAgentsDatabase(IFileSystem fileSystem, ILoggingService loggingService) : base(fileSystem, loggingService, TimeSpan.FromDays(7))
        {

        }

        public override async Task<int> DeleteAllData()
        {
            LoggingService.Trace("Querying TeamAgentsDatabase.DeleteAllData");

            var databaseConnection = await GetDatabaseConnection<TeamAgentDatabaseModel>().ConfigureAwait(false);
            return await databaseConnection.DeleteAllAsync<TeamAgentDatabaseModel>().ConfigureAwait(false);
        }

        public async Task<int> SaveTeamAgentModel(TeamAgentModel teamAgentModel)
        {
            LoggingService.Trace("Querying TeamAgentsDatabase.SaveTeamAgentModel");

            var databaseConnection = await GetDatabaseConnection<TeamAgentDatabaseModel>().ConfigureAwait(false);

            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var teamAgentDatabaseModel = TeamAgentDatabaseModel.ToTeamAgentDatabaseModel(teamAgentModel);
                
                databaseConnection.GetConnection().InsertOrReplace(teamAgentDatabaseModel);

                return 0;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying TeamAgentsDatabase.SaveTeamAgentModel");

                return 1;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<int> SaveTeamAgentsModels(IList<TeamAgentModel> agents)
        {
            LoggingService.Trace("Querying TeamAgentsDatabase.SaveTeamAgentsModels");

            var databaseConnection = await GetDatabaseConnection<TeamAgentDatabaseModel>().ConfigureAwait(false);

            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var teamAgentDatabaseModels = new List<TeamAgentDatabaseModel>(
                    agents.Select(model => TeamAgentDatabaseModel.ToTeamAgentDatabaseModel(model)));
                databaseConnection.GetConnection().InsertOrReplaceAllWithChildren(teamAgentDatabaseModels, true);

                return 0;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying TeamAgentsDatabase.SaveTeamAgentsModels");

                return 1;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<TeamAgentModel?> GetTeamAgent(string agentId)
        {
            LoggingService.Trace("Querying TeamAgentsDatabase.GetTeamAgent");

            var databaseConnection = await GetDatabaseConnection<TeamAgentDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var teamAgentDatabaseModel = databaseConnection.GetConnection().Get<TeamAgentDatabaseModel>(agentId);
                
                return teamAgentDatabaseModel != null ?
                    TeamAgentDatabaseModel.ToTeamAgentModel(teamAgentDatabaseModel) :
                    null;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error uerying TeamAgentsDatabase.GetTeamAgent");

                return null;
            }
            finally
            {
                dbLock.Dispose();
            }

        }

        public async Task<List<TeamAgentModel>> GetAgentsInTeam(string teamId)
        {
            LoggingService.Trace("Querying TeamAgentsDatabase.GetAgentsInTeam");

            var databaseConnection = await GetDatabaseConnection<TeamAgentDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var teamAgentDatabaseModels = databaseConnection.GetConnection().GetAllWithChildren<TeamAgentDatabaseModel>();
                dbLock.Dispose();

                return teamAgentDatabaseModels?
                    .Where(x => x.Teams.Any(t => t.TeamId.Equals(teamId)))
                    .Select(x => TeamAgentDatabaseModel.ToTeamAgentModel(x))
                    .ToList() ?? new List<TeamAgentModel>();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error uerying TeamAgentsDatabase.GetAgentsInTeam");

                return new List<TeamAgentModel>();
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<List<TeamAgentModel>> GetAgentsInTeams(IEnumerable<string> teamIds)
        {
            LoggingService.Trace("Querying TeamAgentsDatabase.GetAgentsInTeams");

            var databaseConnection = await GetDatabaseConnection<TeamAgentDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var teamAgentDatabaseModels = databaseConnection.GetConnection().GetAllWithChildren<TeamAgentDatabaseModel>();

                return teamAgentDatabaseModels?
                    .Where(x => teamIds.Any(id => x.Teams.Any(t => t.TeamId.Equals(id))))
                    .Select(x => TeamAgentDatabaseModel.ToTeamAgentModel(x))
                    .ToList() ?? new List<TeamAgentModel>();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error uerying TeamAgentsDatabase.GetAgentsInTeams");

                return new List<TeamAgentModel>();
            }
            finally
            {
                dbLock.Dispose();
            }
        }

#nullable disable
        internal class TeamAgentDatabaseModel
        {
            [PrimaryKey, Unique]
            public string AgentId { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public string Enlid { get; set; }
            public string Pic { get; set; }
            public bool RocksVerified { get; set; }
            public bool VVerified { get; set; }
            public bool Blacklisted { get; set; }
            public string Squad { get; set; }
            public bool State { get; set; }
            public float Lat { get; set; }
            public float Lng { get; set; }
            public string Date { get; set; }
            public bool ShareWD { get; set; }
            public bool LoadWD { get; set; }
            
            [ManyToMany(typeof(TeamAgent), CascadeOperations = CascadeOperation.CascadeRead)]
            public List<TeamsDatabase.TeamDatabaseModel> Teams { get; set; }

            public DateTime LastUpdatedAt { get; set; }

            public static TeamAgentModel ToTeamAgentModel(TeamAgentDatabaseModel teamAgentDatabaseModel)
            {
                return new TeamAgentModel
                {
                    Id = teamAgentDatabaseModel.AgentId,
                    Name = teamAgentDatabaseModel.Name,
                    Level = teamAgentDatabaseModel.Level,
                    Enlid = teamAgentDatabaseModel.Enlid,
                    Pic = teamAgentDatabaseModel.Pic,
                    RocksVerified = teamAgentDatabaseModel.RocksVerified,
                    VVerified = teamAgentDatabaseModel.VVerified,
                    Blacklisted = teamAgentDatabaseModel.Blacklisted,
                    Squad = teamAgentDatabaseModel.Squad,
                    State = teamAgentDatabaseModel.State,
                    Lat = teamAgentDatabaseModel.Lat,
                    Lng = teamAgentDatabaseModel.Lng,
                    Date = teamAgentDatabaseModel.Date,
                    ShareWD = teamAgentDatabaseModel.ShareWD,
                    LoadWD = teamAgentDatabaseModel.LoadWD,
                    LastUpdatedAt = teamAgentDatabaseModel.LastUpdatedAt
                };
            }
            public static TeamAgentDatabaseModel ToTeamAgentDatabaseModel(TeamAgentModel teamAgentModel)
            {
                return new TeamAgentDatabaseModel()
                {
                    AgentId = teamAgentModel.Id,
                    Name = teamAgentModel.Name,
                    Level = teamAgentModel.Level,
                    Enlid = teamAgentModel.Enlid,
                    Pic = teamAgentModel.Pic,
                    RocksVerified = teamAgentModel.RocksVerified,
                    VVerified = teamAgentModel.VVerified,
                    Blacklisted = teamAgentModel.Blacklisted,
                    Squad = teamAgentModel.Squad,
                    State = teamAgentModel.State,
                    Lat = teamAgentModel.Lat,
                    Lng = teamAgentModel.Lng,
                    Date = teamAgentModel.Date,
                    ShareWD = teamAgentModel.ShareWD,
                    LoadWD = teamAgentModel.LoadWD,
                    LastUpdatedAt = DateTime.Now
                };
            }
        }
#nullable enable
    }
}