﻿using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
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
    public class OperationsDatabase : BaseDatabase
    {
        public OperationsDatabase(IFileSystem fileSystem, ILoggingService loggingService) : base(fileSystem, loggingService, TimeSpan.FromDays(7))
        {
            GetDatabaseConnection<LinksDatabase.LinkDatabaseModel>().ConfigureAwait(false);
            GetDatabaseConnection<MarkersDatabase.MarkerDatabaseModel>().ConfigureAwait(false);
        }

        public override async Task<int> DeleteAllData()
        {
            LoggingService.Trace("Querying OperationsDatabase.DeleteAllData");

            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);
            return await databaseConnection.DeleteAllAsync<OperationDatabaseModel>().ConfigureAwait(false);
        }

        public async Task<int> DeleteAllExceptOwnedBy(string ownerId)
        {
            LoggingService.Trace("Querying OperationsDatabase.DeleteAllExceptOwnedBy");

            if (string.IsNullOrEmpty(ownerId))
                return -1;

            var result = 0;
            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var operations = await this.GetOperationModels();

                foreach (var op in operations.Where(x => x.Creator.Equals(ownerId) is false))
                {
                    result += databaseConnection.GetConnection().Delete<OperationDatabaseModel>(op.Id);
                }
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying OperationsDatabase.DeleteAllExceptOwnedBy");
                return -1;
            }
            finally
            {
                dbLock.Dispose();
            }

            return result;
        }

        public async Task<OperationModel?> GetOperationModel(string operationId)
        {
            LoggingService.Trace("Querying OperationsDatabase.GetOperationModel");
            
            if (string.IsNullOrEmpty(operationId))
                return null;

            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var operationDatabaseModel = databaseConnection.GetConnection().GetWithChildren<OperationDatabaseModel>(operationId);

                return operationDatabaseModel != null ?
                    OperationDatabaseModel.ToOperationModel(operationDatabaseModel) :
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

        public async Task<List<OperationModel>> GetOperationModels()
        {
            LoggingService.Trace("Querying OperationsDatabase.GetOperationModels");

            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var operationDatabaseModelList = databaseConnection.GetConnection().GetAllWithChildren<OperationDatabaseModel>();

                return operationDatabaseModelList.Select(x => OperationDatabaseModel.ToOperationModel(x)).ToList();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error OperationsDatabase.GetOperationModels");

                return new List<OperationModel>();
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<int> SaveOperationModel(OperationModel operationModel)
        {
            LoggingService.Trace("Querying OperationsDatabase.SaveOperationModel");

            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var operationDatabaseModel = OperationDatabaseModel.ToOperationDatabaseModel(operationModel);

                var existingData = await GetOperationModel(operationModel.Id);
                if (existingData != null)
                    operationDatabaseModel.IsHiddenLocally = existingData.IsHiddenLocally;

                databaseConnection.GetConnection().InsertOrReplaceWithChildren(operationDatabaseModel);

                return 0;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying OperationsDatabase.SaveOperationModel");

                return 1;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<bool> HideLocalOperation(string opId, bool isHidden)
        {
            LoggingService.Trace("Querying OperationsDatabase.HideLocalOperation");

            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var operationModel = await GetOperationModel(opId);
                if (operationModel == null)
                    return false;

                operationModel.IsHiddenLocally = isHidden;

                var dbModel = OperationDatabaseModel.ToOperationDatabaseModel(operationModel);
                databaseConnection.GetConnection().UpdateWithChildren(dbModel);

                return true;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying OperationsDatabase.HideLocalOperation");

                return false;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<int> CountLocalOperations()
        {
            LoggingService.Trace("Querying OperationsDatabase.CountLocalOperations");

            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();

            try
            {
                var count = databaseConnection.GetConnection().Table<OperationDatabaseModel>().Count();
                return count;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying OperationsDatabase.CountLocalOperations");

                return 0;
            }
            finally
            {
                dbLock.Dispose();
            }
        }
        
        public async Task<int> DeleteLocalOperation(string operationId)
        {
            LoggingService.Trace("Querying OperationsDatabase.DeleteLocalOperation");

            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();

            try
            {
                return databaseConnection.GetConnection().Delete<OperationDatabaseModel>(operationId);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying OperationsDatabase.DeleteLocalOperation");
                return 0;
            }
            finally
            {
                dbLock.Dispose();
            }
        }
#nullable disable
        internal class OperationDatabaseModel
        {
            [PrimaryKey, Unique]
            public string OpId { get; set; }

            public string OpName { get; set; }

            public string Creator { get; set; }

            public string Color { get; set; }

            public string PortalsBlobbed { get; set; }
            [TextBlob("PortalsBlobbed")]
            public List<PortalModel> Portals { get; set; }

            public string AnchorsBlobbed { get; set; }
            [TextBlob("AnchorsBlobbed")]
            public List<string> Anchors { get; set; }

            public string BlockersBlobbed { get; set; }
            [TextBlob("BlockersBlobbed")]
            public List<BlockerModel> Blockers { get; set; }

            public string TeamListBlobbed { get; set; }
            [TextBlob("TeamListBlobbed")]
            public List<TeamModel> TeamList { get; set; }

            public string KeysOnHandBlobbed { get; set; }
            [TextBlob("KeysOnHandBlobbed")]
            public List<KeysOnHandModel> KeysOnHand { get; set; }

            public string ZonesBlobbed { get; set; }
            [TextBlob("ZonesBlobbed")]
            public List<ZoneModel> Zones { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.All)]
            public List<MarkersDatabase.MarkerDatabaseModel> Markers { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.All)]
            public List<LinksDatabase.LinkDatabaseModel> Links { get; set; }

            public string Modified { get; set; }
            public string LastEditId { get; set; }

            public string Comment { get; set; }

            public bool IsHiddenLocally { get; set; }

            public DateTime DownloadedAt { get; set; }

            public static OperationModel ToOperationModel(OperationDatabaseModel operationDatabaseModel)
            {
                try
                {
                    return new OperationModel()
                    {
                        Id = operationDatabaseModel.OpId,
                        Name = operationDatabaseModel.OpName,
                        Creator = operationDatabaseModel.Creator,
                        Color = operationDatabaseModel.Color,
                        Portals = operationDatabaseModel.Portals ?? new List<PortalModel>(),
                        Anchors = operationDatabaseModel.Anchors ?? new List<string>(),
                        Blockers = operationDatabaseModel.Blockers ?? new List<BlockerModel>(),
                        TeamList = operationDatabaseModel.TeamList ?? new List<TeamModel>(),
                        Modified = operationDatabaseModel.Modified,
                        LastEditId = operationDatabaseModel.LastEditId,
                        Comment = operationDatabaseModel.Comment,
                        KeysOnHand = operationDatabaseModel.KeysOnHand ?? new List<KeysOnHandModel>(),
                        Zones = operationDatabaseModel.Zones ?? new List<ZoneModel>(),
                        IsHiddenLocally = operationDatabaseModel.IsHiddenLocally,
                        DownloadedAt = operationDatabaseModel.DownloadedAt,

                        Markers = operationDatabaseModel.Markers?.Select(markerDbModel => MarkersDatabase.MarkerDatabaseModel.ToMarkerModel(markerDbModel)).ToList(),
                        Links = operationDatabaseModel.Links?.Select(linkDbModel => LinksDatabase.LinkDatabaseModel.ToLinkModel(linkDbModel)).ToList()
                    };
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public static OperationDatabaseModel ToOperationDatabaseModel(OperationModel operationModel)
            {
                return new OperationDatabaseModel
                {
                    OpId = operationModel.Id,
                    OpName = operationModel.Name,
                    Creator = operationModel.Creator,
                    Color = operationModel.Color,
                    Portals = operationModel.Portals ?? new List<PortalModel>(),
                    Anchors = operationModel.Anchors ?? new List<string>(),
                    Blockers = operationModel.Blockers ?? new List<BlockerModel>(),
                    TeamList = operationModel.TeamList ?? new List<TeamModel>(),
                    Modified = operationModel.Modified,
                    LastEditId = operationModel.LastEditId,
                    Comment = operationModel.Comment,
                    KeysOnHand = operationModel.KeysOnHand ?? new List<KeysOnHandModel>(),
                    Zones = operationModel.Zones ?? new List<ZoneModel>(),
                    IsHiddenLocally = operationModel.IsHiddenLocally,
                    DownloadedAt = operationModel.DownloadedAt,

                    Markers = operationModel.Markers?.Select(markerModel => MarkersDatabase.MarkerDatabaseModel.ToMarkerDatabaseModel(markerModel)).ToList(),
                    Links = operationModel.Links?.Select(linkModel => LinksDatabase.LinkDatabaseModel.ToLinkDatabaseModel(linkModel)).ToList()
                };
            }
        }
#nullable enable
    }
}