using Rocks.Wasabee.Core.Adapters;
using Rocks.Wasabee.Core.Extensions;
using Rocks.Wasabee.Core.Models.Agent;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;

namespace Rocks.Wasabee.Core.Infra.Databases;

public class AgentsDatabase : BaseDatabase
{
    public AgentsDatabase(IFileSystemAdapter fileSystem) : base(fileSystem, TimeSpan.FromDays(7))
    {

    }

    public override async Task<int> DeleteAllData()
    {
        var databaseConnection = await GetDatabaseConnection<AgentDatabaseModel>().ConfigureAwait(false);
        return await databaseConnection.DeleteAllAsync<AgentDatabaseModel>().ConfigureAwait(false);
    }

    public async Task<int> SaveTeamAgentModel(AgentModel agentModel)
    {
        var databaseConnection = await GetDatabaseConnection<AgentDatabaseModel>().ConfigureAwait(false);

        var dbLock = databaseConnection.GetConnection().Lock();
        try
        {
            var agentDatabaseModel = AgentDatabaseModel.ToAgentDatabaseModel(agentModel);

            databaseConnection.GetConnection().InsertOrReplace(agentDatabaseModel);

            return 0;
        }
        catch (Exception e)
        {
            return 1;
        }
        finally
        {
            dbLock.Dispose();
        }
    }

    public async Task<int> SaveAgentsModels(IList<AgentModel> agents)
    {
        var databaseConnection = await GetDatabaseConnection<AgentDatabaseModel>().ConfigureAwait(false);

        var dbLock = databaseConnection.GetConnection().Lock();
        try
        {
            var agentDatabaseModels = new List<AgentDatabaseModel>(
                agents.Select(model => AgentDatabaseModel.ToAgentDatabaseModel(model)));
            databaseConnection.GetConnection().InsertOrReplaceAllWithChildren(agentDatabaseModels, true);

            return 0;
        }
        catch (Exception e)
        {
            return 1;
        }
        finally
        {
            dbLock.Dispose();
        }
    }

    public async Task<AgentModel?> GetAgent(string agentId)
    {
        if (string.IsNullOrEmpty(agentId))
            return null;

        var databaseConnection = await GetDatabaseConnection<AgentDatabaseModel>().ConfigureAwait(false);
        var dbLock = databaseConnection.GetConnection().Lock();
        try
        {
            var agentDatabaseModel = databaseConnection.GetConnection().Get<AgentDatabaseModel>(agentId);

            return agentDatabaseModel != null ?
                AgentDatabaseModel.ToTeamAgentModel(agentDatabaseModel) :
                null;
        }
        catch (Exception e)
        {
            return null;
        }
        finally
        {
            dbLock.Dispose();
        }

    }

    public async Task<List<AgentModel>> GetAgents(IList<string> agentIds)
    {
        if (agentIds.Count == 0)
            return new List<AgentModel>();

        var databaseConnection = await GetDatabaseConnection<AgentDatabaseModel>().ConfigureAwait(false);
        var dbLock = databaseConnection.GetConnection().Lock();
        try
        {
            var agentDatabaseModels = databaseConnection.GetConnection().GetAllWithChildren<AgentDatabaseModel>();

            return agentDatabaseModels?
                .Where(x => agentIds.Contains(x.AgentId))
                .Select(x => AgentDatabaseModel.ToTeamAgentModel(x))
                .ToList() ?? new List<AgentModel>();
        }
        catch (Exception e)
        {
            return new List<AgentModel>();
        }
        finally
        {
            dbLock.Dispose();
        }
    }

    public async Task<List<AgentModel>> GetAgentsInTeam(string teamId)
    {
        if (string.IsNullOrEmpty(teamId))
            return new List<AgentModel>();

        var databaseConnection = await GetDatabaseConnection<AgentDatabaseModel>().ConfigureAwait(false);
        var dbLock = databaseConnection.GetConnection().Lock();
        try
        {
            var agentDatabaseModels = databaseConnection.GetConnection().GetAllWithChildren<AgentDatabaseModel>();

            return agentDatabaseModels?
                .Where(x => x.Teams.Any(t => t.TeamId.Equals(teamId)))
                .Select(x => AgentDatabaseModel.ToTeamAgentModel(x))
                .ToList() ?? new List<AgentModel>();
        }
        catch (Exception e)
        {
            return new List<AgentModel>();
        }
        finally
        {
            dbLock.Dispose();
        }
    }

    public async Task<List<AgentModel>> GetAgentsInTeams(IList<string> teamIds)
    {
        if (teamIds.IsNullOrEmpty())
            return new List<AgentModel>();

        var databaseConnection = await GetDatabaseConnection<AgentDatabaseModel>().ConfigureAwait(false);
        var dbLock = databaseConnection.GetConnection().Lock();
        try
        {
            var agentDatabaseModels = databaseConnection.GetConnection().GetAllWithChildren<AgentDatabaseModel>();

            return agentDatabaseModels?
                .Where(x => teamIds.Any(id => x.Teams.Any(t => t.TeamId.Equals(id))))
                .Select(x => AgentDatabaseModel.ToTeamAgentModel(x))
                .ToList() ?? new List<AgentModel>();
        }
        catch (Exception e)
        {
            return new List<AgentModel>();
        }
        finally
        {
            dbLock.Dispose();
        }
    }

#nullable disable
    internal class AgentDatabaseModel
    {
        [PrimaryKey, Unique]
        public string AgentId { get; set; }
        public string Name { get; set; }
        public string Pic { get; set; }
        public bool RocksVerified { get; set; }
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

        public static AgentModel ToTeamAgentModel(AgentDatabaseModel agentDatabaseModel)
        {
            return new AgentModel
            {
                Id = agentDatabaseModel.AgentId,
                Name = agentDatabaseModel.Name,
                Pic = agentDatabaseModel.Pic,
                RocksVerified = agentDatabaseModel.RocksVerified,
                Blacklisted = agentDatabaseModel.Blacklisted,
                Squad = agentDatabaseModel.Squad,
                State = agentDatabaseModel.State,
                Lat = agentDatabaseModel.Lat,
                Lng = agentDatabaseModel.Lng,
                Date = agentDatabaseModel.Date,
                ShareWD = agentDatabaseModel.ShareWD,
                LoadWD = agentDatabaseModel.LoadWD,
                LastUpdatedAt = agentDatabaseModel.LastUpdatedAt
            };
        }
        public static AgentDatabaseModel ToAgentDatabaseModel(AgentModel teamAgentModel)
        {
            return new AgentDatabaseModel()
            {
                AgentId = teamAgentModel.Id,
                Name = teamAgentModel.Name,
                Pic = teamAgentModel.Pic,
                RocksVerified = teamAgentModel.RocksVerified,
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