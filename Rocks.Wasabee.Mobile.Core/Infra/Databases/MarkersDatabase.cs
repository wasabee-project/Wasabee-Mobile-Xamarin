using Rocks.Wasabee.Mobile.Core.Infra.Logger;
using Rocks.Wasabee.Mobile.Core.Models.Operations;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System;
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

            public string PortalId { get; set; }

            public string Type { get; set; }

            public string Comment { get; set; }

            public string AssignedTo { get; set; }

            public string AssignedTeam { get; set; }

            public string CompletedId { get; set; }

            public string State { get; set; }

            public int Order { get; set; }

            public int Zone { get; set; }

            [ForeignKey(typeof(OperationsDatabase.OperationDatabaseModel))]
            public string OpId { get; set; }

            public static MarkerModel ToMarkerModel(MarkerDatabaseModel markerDatabaseModel)
            {
                return new MarkerModel()
                {
                    Id = markerDatabaseModel.Id,
                    PortalId = markerDatabaseModel.PortalId,
                    Type = markerDatabaseModel.Type,
                    Comment = markerDatabaseModel.Comment,
                    AssignedTo = markerDatabaseModel.AssignedTo,
                    AssignedTeam = markerDatabaseModel.AssignedTeam,
                    CompletedId = markerDatabaseModel.CompletedId,
                    State = markerDatabaseModel.State,
                    Order = markerDatabaseModel.Order,
                    Zone = markerDatabaseModel.Zone
                };
            }

            public static MarkerDatabaseModel ToMarkerDatabaseModel(MarkerModel markerModel)
            {
                return new MarkerDatabaseModel()
                {
                    Id = markerModel.Id,
                    PortalId = markerModel.PortalId,
                    Type = markerModel.Type,
                    Comment = markerModel.Comment,
                    AssignedTo = markerModel.AssignedTo,
                    AssignedTeam = markerModel.AssignedTeam,
                    CompletedId = markerModel.CompletedId,
                    State = markerModel.State,
                    Order = markerModel.Order,
                    Zone = markerModel.Zone
                };
            }
        }
#nullable enable
    }
}