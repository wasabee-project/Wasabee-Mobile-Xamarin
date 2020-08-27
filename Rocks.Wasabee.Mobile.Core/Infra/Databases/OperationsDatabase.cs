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
        public OperationsDatabase(IFileSystem fileSystem) : base(fileSystem, TimeSpan.FromDays(7))
        {

        }

        public override async Task<int> DeleteAllData()
        {
            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);
            return await databaseConnection.DeleteAllAsync<OperationDatabaseModel>().ConfigureAwait(false);
        }

        public async Task DeleteExpiredData()
        {
            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);

            var referringSites = await databaseConnection.Table<OperationDatabaseModel>().ToListAsync();
            var expiredReferringSites = referringSites.Where(x => IsExpired(x.DownloadedAt)).ToList();

            foreach (var expiredReferringSite in expiredReferringSites)
                await databaseConnection.DeleteAsync(expiredReferringSite).ConfigureAwait(false);
        }

        public async Task<OperationModel> GetOperationModel(string operationId)
        {
            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);

            var dbLock = databaseConnection.GetConnection().Lock();
            var operationDatabaseModel = databaseConnection.GetConnection().GetWithChildren<OperationDatabaseModel>(operationId);
            dbLock.Dispose();

            return operationDatabaseModel != null ?
                OperationDatabaseModel.ToOperationModel(operationDatabaseModel) :
                null;
        }

        public async Task<List<OperationModel>> GetOperationModels()
        {
            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);

            var dbLock = databaseConnection.GetConnection().Lock();
            var operationDatabaseModelList = databaseConnection.GetConnection().GetAllWithChildren<OperationDatabaseModel>();
            dbLock.Dispose();

            return operationDatabaseModelList.Select(x => OperationDatabaseModel.ToOperationModel(x)).ToList();
        }

        public async Task<int> SaveOperationModel(OperationModel operationModel)
        {
            var databaseConnection = await GetDatabaseConnection<OperationDatabaseModel>().ConfigureAwait(false);
            var operationDatabaseModel = OperationDatabaseModel.ToOperationDatabaseModel(operationModel);

            var dbLock = databaseConnection.GetConnection().Lock();

            try
            {
                databaseConnection.GetConnection().InsertOrReplaceWithChildren(operationDatabaseModel, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }

            dbLock.Dispose();

            return 0;
        }

        class OperationDatabaseModel
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

            public string LinksBlobbed { get; set; }
            [TextBlob("LinksBlobbed")]
            public List<LinkModel> Links { get; set; }

            public string BlockersBlobbed { get; set; }
            [TextBlob("BlockersBlobbed")]
            public List<BlockerModel> Blockers { get; set; }

            public string MarkersBlobbed { get; set; }
            [TextBlob("MarkersBlobbed")]
            public List<MarkerModel> Markers { get; set; }

            public string TeamListBlobbed { get; set; }
            [TextBlob("TeamListBlobbed")]
            public List<TeamModel> TeamList { get; set; }

            public string KeysOnHandBlobbed { get; set; }
            [TextBlob("KeysOnHandBlobbed")]
            public List<KeysOnHandModel> KeysOnHand { get; set; }

            public string Modified { get; set; }

            public string Comment { get; set; }

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
                        Portals = operationDatabaseModel.Portals,
                        Anchors = operationDatabaseModel.Anchors,
                        Links = operationDatabaseModel.Links,
                        Blockers = operationDatabaseModel.Blockers,
                        Markers = operationDatabaseModel.Markers,
                        TeamList = operationDatabaseModel.TeamList,
                        Modified = operationDatabaseModel.Modified,
                        Comment = operationDatabaseModel.Comment,
                        KeysOnHand = operationDatabaseModel.KeysOnHand,
                        DownloadedAt = operationDatabaseModel.DownloadedAt
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
                    Portals = operationModel.Portals,
                    Anchors = operationModel.Anchors,
                    Links = operationModel.Links,
                    Blockers = operationModel.Blockers,
                    Markers = operationModel.Markers,
                    TeamList = operationModel.TeamList,
                    Modified = operationModel.Modified,
                    Comment = operationModel.Comment,
                    KeysOnHand = operationModel.KeysOnHand,
                    DownloadedAt = operationModel.DownloadedAt,
                };
            }
        }
    }
}