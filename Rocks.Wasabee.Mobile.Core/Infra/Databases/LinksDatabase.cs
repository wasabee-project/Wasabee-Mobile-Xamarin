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
    public class LinksDatabase : BaseDatabase
    {
        public LinksDatabase(IFileSystem fileSystem, ILoggingService loggingService) : base(fileSystem, loggingService, TimeSpan.FromDays(7))
        {
            GetDatabaseConnection<LinkDatabaseModel>().ConfigureAwait(false);
        }

        public override async Task<int> DeleteAllData()
        {
            LoggingService.Trace("Querying OperationsDatabase.DeleteAllData");

            var databaseConnection = await GetDatabaseConnection<LinkDatabaseModel>().ConfigureAwait(false);
            return await databaseConnection.DeleteAllAsync<LinkDatabaseModel>().ConfigureAwait(false);
        }

        public async Task<LinkModel?> GetLinkModel(string linkId)
        {
            LoggingService.Trace("Querying LinksDatabase.GetLinkModel");

            if (string.IsNullOrEmpty(linkId))
                return null;

            var databaseConnection = await GetDatabaseConnection<LinkDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var linkDatabaseModel = databaseConnection.GetConnection().Get<LinkDatabaseModel>(linkId);

                return linkDatabaseModel != null ?
                    LinkDatabaseModel.ToLinkModel(linkDatabaseModel) :
                    new LinkModel();
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying LinksDatabase.GetLinkModel");

                return null;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<int> SaveLinkModel(LinkModel linkModel, string operationId)
        {
            LoggingService.Trace("Querying LinksDatabase.SaveLinkModel");

            var databaseConnection = await GetDatabaseConnection<LinkDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var linkDatabaseModel = LinkDatabaseModel.ToLinkDatabaseModel(linkModel);
                linkDatabaseModel.OpId = operationId;

                databaseConnection.GetConnection().InsertOrReplaceWithChildren(linkDatabaseModel);

                return 0;
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying LinksDatabase.SaveLinkModel");

                return 1;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

#nullable disable
        internal class LinkDatabaseModel
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

            public string FromPortalId { get; set; }
            public string ToPortalId { get; set; }
            public string Color { get; set; }
            public int MusCaptured { get; set; }

            [ForeignKey(typeof(OperationsDatabase.OperationDatabaseModel))]
            public string OpId { get; set; }

            public static LinkModel ToLinkModel(LinkDatabaseModel linkDatabaseModel)
            {
                return new LinkModel()
                {
                    Id = linkDatabaseModel.Id,
                    Assignments = string.IsNullOrEmpty(linkDatabaseModel.Assignments) ? new List<string>() : 
                        JsonConvert.DeserializeObject<List<string>>(linkDatabaseModel.Assignments),
                    DependsOn = string.IsNullOrEmpty(linkDatabaseModel.DependsOn) ? new List<string>() : 
                        JsonConvert.DeserializeObject<List<string>>(linkDatabaseModel.DependsOn),
                    Zone = linkDatabaseModel.Zone,
                    DeltaMinutes = linkDatabaseModel.DeltaMinutes,
                    State = linkDatabaseModel.State,
                    Comment = linkDatabaseModel.Comment,
                    Order = linkDatabaseModel.Order,
                    FromPortalId = linkDatabaseModel.FromPortalId,
                    ToPortalId = linkDatabaseModel.ToPortalId,
                    Color = linkDatabaseModel.Color,
                    MusCaptured = linkDatabaseModel.MusCaptured
                };
            }

            public static LinkDatabaseModel ToLinkDatabaseModel(LinkModel linkModel)
            {
                return new LinkDatabaseModel()
                {
                    Id = linkModel.Id,
                    Assignments = linkModel.Assignments.IsNullOrEmpty() ? string.Empty : 
                        JsonConvert.SerializeObject(linkModel.Assignments),
                    DependsOn = linkModel.DependsOn.IsNullOrEmpty() ? string.Empty : 
                        JsonConvert.SerializeObject(linkModel.DependsOn),
                    Zone = linkModel.Zone,
                    DeltaMinutes = linkModel.DeltaMinutes,
                    State = linkModel.State,
                    Comment = linkModel.Comment,
                    Order = linkModel.Order,
                    FromPortalId = linkModel.FromPortalId,
                    ToPortalId = linkModel.ToPortalId,
                    Color = linkModel.Color,
                    MusCaptured = linkModel.MusCaptured
                };
            }
        }
#nullable enable
    }
}