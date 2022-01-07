using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Helpers;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.Infra.Databases
{
    public class MarkersDatabase : BaseDatabase
    {
        public MarkersDatabase(IFileSystem fileSystem, ILoggingService loggingService) : base(fileSystem, loggingService, TimeSpan.FromDays(7))
        {
            GetDatabaseConnection<MarkerDatabaseModel>().ConfigureAwait(false);
        }

        public override async Task<int> DeleteAllData()
        {
            LoggingService.Trace("Querying MarkersDatabase.DeleteAllData");

            var databaseConnection = await GetDatabaseConnection<MarkerDatabaseModel>().ConfigureAwait(false);
            return await databaseConnection.DeleteAllAsync<MarkerDatabaseModel>().ConfigureAwait(false);
        }

        public async Task<MarkerModel?> GetMarkerModel(string markerId)
        {
            LoggingService.Trace("Querying MarkersDatabase.GetMarkerModel");

            if (string.IsNullOrEmpty(markerId))
                return null;

            var databaseConnection = await GetDatabaseConnection<MarkerDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var markerDatabaseModel = databaseConnection.GetConnection().Get<MarkerDatabaseModel>(markerId);

                return markerDatabaseModel != null ?
                    MarkerDatabaseModel.ToMarkerModel(markerDatabaseModel) :
                    new MarkerModel();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying MarkersDatabase.GetMarkerModel");

                return null;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<int> SaveMarkerModel(MarkerModel markerModel, string operationId)
        {
            LoggingService.Trace("Querying MarkersDatabase.SaveMarkerModel");

            var databaseConnection = await GetDatabaseConnection<MarkerDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var markerDatabaseModel = MarkerDatabaseModel.ToMarkerDatabaseModel(markerModel);
                markerDatabaseModel.OpId = operationId;

                databaseConnection.GetConnection().InsertOrReplaceWithChildren(markerDatabaseModel);

                return 0;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying MarkersDatabase.SaveMarkerModel");

                return 1;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

#nullable disable
        internal class MarkerDatabaseModel
        {
            [PrimaryKey, Unique]
            public string Id { get; set; }
            
            public string Assignments { get; set; }
            public string DependsOn { get; set; }
            public int Zone { get; set; }
            public int DeltaMinutes { get; set; }
            public TaskState State { get; set; }
            public string Comment { get; set; }
            public int Order { get; set; }
            public string PortalId { get; set; }
            public MarkerType Type { get; set; }

            [ForeignKey(typeof(OperationsDatabase.OperationDatabaseModel))]
            public string OpId { get; set; }

            public static MarkerModel ToMarkerModel(MarkerDatabaseModel markerDatabaseModel)
            {
                return new MarkerModel()
                {
                    Id = markerDatabaseModel.Id,
                    Assignments = string.IsNullOrEmpty(markerDatabaseModel.Assignments) ? new List<string>() : 
                        JsonConvert.DeserializeObject<List<string>>(markerDatabaseModel.Assignments),
                    DependsOn = string.IsNullOrEmpty(markerDatabaseModel.DependsOn) ? new List<string>() : 
                        JsonConvert.DeserializeObject<List<string>>(markerDatabaseModel.DependsOn),
                    Zone = markerDatabaseModel.Zone,
                    DeltaMinutes = markerDatabaseModel.DeltaMinutes,
                    State = markerDatabaseModel.State,
                    Comment = markerDatabaseModel.Comment,
                    Order = markerDatabaseModel.Order,
                    PortalId = markerDatabaseModel.PortalId,
                    Type = markerDatabaseModel.Type
                };
            }

            public static MarkerDatabaseModel ToMarkerDatabaseModel(MarkerModel markerModel)
            {
                return new MarkerDatabaseModel()
                {
                    Id = markerModel.Id,
                    Assignments = markerModel.Assignments.IsNullOrEmpty() ? string.Empty : 
                        JsonConvert.SerializeObject(markerModel.Assignments),
                    DependsOn = markerModel.DependsOn.IsNullOrEmpty() ? string.Empty : 
                        JsonConvert.SerializeObject(markerModel.DependsOn),
                    Zone = markerModel.Zone,
                    DeltaMinutes = markerModel.DeltaMinutes,
                    State = markerModel.State,
                    Comment = markerModel.Comment,
                    Order = markerModel.Order,
                    PortalId = markerModel.PortalId,
                    Type = markerModel.Type
                };
            }
        }
#nullable enable
    }
}