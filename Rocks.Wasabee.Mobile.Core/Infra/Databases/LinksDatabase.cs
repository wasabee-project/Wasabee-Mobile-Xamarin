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
            public string FromPortalId { get; set; }
            public string ToPortalId { get; set; }
            public string Description { get; set; }
            public string AssignedTo { get; set; }
            public string AssignedToNickname { get; set; }
            public int ThrowOrderPos { get; set; }
            public bool Completed { get; set; }
            public string Color { get; set; }
            public int Zone { get; set; }

            [ForeignKey(typeof(OperationsDatabase.OperationDatabaseModel))]
            public string OpId { get; set; }


            public static LinkModel ToLinkModel(LinkDatabaseModel linkDatabaseModel)
            {
                return new LinkModel
                {
                    Id = linkDatabaseModel.Id,
                    FromPortalId = linkDatabaseModel.FromPortalId,
                    ToPortalId = linkDatabaseModel.ToPortalId,
                    Description = linkDatabaseModel.Description,
                    AssignedTo = linkDatabaseModel.AssignedTo,
                    AssignedToNickname = linkDatabaseModel.AssignedToNickname,
                    ThrowOrderPos = linkDatabaseModel.ThrowOrderPos,
                    Completed = linkDatabaseModel.Completed,
                    Color = linkDatabaseModel.Color,
                    Zone = linkDatabaseModel.Zone
                };
            }

            public static LinkDatabaseModel ToLinkDatabaseModel(LinkModel linkModel)
            {
                return new LinkDatabaseModel
                {
                    Id = linkModel.Id,
                    FromPortalId = linkModel.FromPortalId,
                    ToPortalId = linkModel.ToPortalId,
                    Description = linkModel.Description,
                    AssignedTo = linkModel.AssignedTo,
                    AssignedToNickname = linkModel.AssignedToNickname,
                    ThrowOrderPos = linkModel.ThrowOrderPos,
                    Completed = linkModel.Completed,
                    Color = linkModel.Color,
                    Zone = linkModel.Zone
                };
            }
        }
#nullable enable
    }
}