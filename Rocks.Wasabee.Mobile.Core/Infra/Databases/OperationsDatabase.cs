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
            var operationDatabaseModels = await databaseConnection.Table<OperationDatabaseModel>().ToListAsync().ConfigureAwait(false);

            if (operationDatabaseModels != null && operationDatabaseModels.Any())
            {
                var operationDatabaseModel = operationDatabaseModels.FirstOrDefault(x => x.OpId.Equals(operationId));
                if (operationDatabaseModel != null)
                    return OperationDatabaseModel.ToOperationModel(operationDatabaseModel);
            }

            return null;
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
            databaseConnection.GetConnection().InsertOrReplaceWithChildren(operationDatabaseModel);
            dbLock.Dispose();

            return 0;
        }

        class OperationDatabaseModel
        {
            [PrimaryKey, Unique]
            public string OpId { get; set; }

            [Indexed]
            public string OpName { get; set; }

            public string Creator { get; set; }

            public string Color { get; set; }

            [TextBlob("PortalsBlobbed")]
            public List<PortalModel> Portals { get; set; }
            public string PortalsBlobbed { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.All)]
            public List<string> Anchors { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.All)]
            public List<LinkModel> Links { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.All)]
            public List<BlockerModel> Blockers { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.All)]
            public List<MarkerModel> Markers { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.All)]
            public List<TeamModel> TeamList { get; set; }

            [OneToMany(CascadeOperations = CascadeOperation.All)]
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