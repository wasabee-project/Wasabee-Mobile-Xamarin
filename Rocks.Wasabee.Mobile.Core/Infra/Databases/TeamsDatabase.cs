﻿using Rocks.Wasabee.Mobile.Core.Infra.Logger;
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

        public async Task<List<TeamModel>> GetTeams(string userId)
        {
            LoggingService.Trace("Querying TeamsDatabase.GetTeams");

            var databaseConnection = await GetDatabaseConnection<TeamDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var teamDatabaseModels = databaseConnection.GetConnection().GetAllWithChildren<TeamDatabaseModel>();

                return teamDatabaseModels
                    .Where(x => x.Agents.Any(a => a.AgentId != null && a.AgentId.Equals(userId)))
                    .Select(x => TeamDatabaseModel.ToTeamModel(x))
                    .ToList();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying TeamsDatabase.GetTeam");

                return new List<TeamModel>();
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