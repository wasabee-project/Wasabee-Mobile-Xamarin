using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Models.Users;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace Rocks.Wasabee.Mobile.Core.Infra.Databases
{
    public class UsersDatabase : BaseDatabase
    {
        public UsersDatabase(IFileSystem fileSystem) : base(fileSystem, TimeSpan.FromDays(7))
        {

        }

        public override async Task<int> DeleteAllData()
        {
            var databaseConnection = await GetDatabaseConnection<UserDatabaseModel>().ConfigureAwait(false);
            return await databaseConnection.DeleteAllAsync<UserDatabaseModel>().ConfigureAwait(false);
        }

        public async Task<UserModel> GetUserModel(string googleId)
        {
            var databaseConnection = await GetDatabaseConnection<UserDatabaseModel>().ConfigureAwait(false);

            var dbLock = databaseConnection.GetConnection().Lock();
            var userDatabaseModel = databaseConnection.GetConnection().GetWithChildren<UserDatabaseModel>(googleId);
            dbLock.Dispose();

            return userDatabaseModel != null ?
                UserDatabaseModel.ToUserModel(userDatabaseModel) :
                null;
        }

        public async Task<int> SaveUserModel(UserModel userModel)
        {
            var databaseConnection = await GetDatabaseConnection<UserDatabaseModel>().ConfigureAwait(false);
            var userDatabaseModel = UserDatabaseModel.ToUserDatabaseModel(userModel);

            var dbLock = databaseConnection.GetConnection().Lock();

            try
            {
                databaseConnection.GetConnection().InsertOrReplaceWithChildren(userDatabaseModel, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }

            dbLock.Dispose();

            return 0;
        }

        class UserDatabaseModel
        {
            [PrimaryKey, Unique]
            public string GoogleId { get; set; }

            [Indexed]
            public string IngressName { get; set; }

            public int Level { get; set; }

            public string LocationKey { get; set; }

            public string OwnTracksPw { get; set; }

            public bool VVerified { get; set; }

            public bool VBlacklisted { get; set; }

            public string Vid { get; set; }

            public string OwnTracksJson { get; set; }

            public bool RocksVerified { get; set; }

            public bool Raid { get; set; }

            public bool Risc { get; set; }

            public string Telegram { get; set; }

            public string OwnedTeamsBlobbed { get; set; }
            [TextBlob("OwnedTeamsBlobbed")]
            public List<OwnedTeamModel> OwnedTeams { get; set; }

            public string TeamsBlobbed { get; set; }
            [TextBlob("TeamsBlobbed")]
            public List<TeamModel> Teams { get; set; }

            public string OpsBlobbed { get; set; }
            [TextBlob("OpsBlobbed")]
            public List<OpModel> Ops { get; set; }

            public string OwnedOpsBlobbed { get; set; }
            [TextBlob("OwnedOpsBlobbed")]
            public List<OwnedOpModel> OwnedOps { get; set; }

            public string AssignmentsBlobbed { get; set; }
            [TextBlob("AssignmentsBlobbed")]
            public List<AssignmentModel> Assignments { get; set; }

            public static UserModel ToUserModel(UserDatabaseModel userDatabaseModel)
            {
                try
                {
                    return new UserModel()
                    {
                        GoogleId = userDatabaseModel.GoogleId,
                        IngressName = userDatabaseModel.IngressName,
                        Level = userDatabaseModel.Level,
                        LocationKey = userDatabaseModel.LocationKey,
                        OwnTracksPw = userDatabaseModel.OwnTracksPw,
                        VVerified = userDatabaseModel.VVerified,
                        VBlacklisted = userDatabaseModel.VBlacklisted,
                        Vid = userDatabaseModel.Vid,
                        OwnTracksJson = userDatabaseModel.OwnTracksJson,
                        RocksVerified = userDatabaseModel.RocksVerified,
                        Raid = userDatabaseModel.Raid,
                        Risc = userDatabaseModel.Risc,
                        Telegram = JsonConvert.DeserializeObject<TelegramModel>(userDatabaseModel.Telegram),
                        OwnedTeams = userDatabaseModel.OwnedTeams,
                        Teams = userDatabaseModel.Teams,
                        Ops = userDatabaseModel.Ops,
                        OwnedOps = userDatabaseModel.OwnedOps,
                        Assignments = userDatabaseModel.Assignments

                    };
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public static UserDatabaseModel ToUserDatabaseModel(UserModel userModel)
            {
                return new UserDatabaseModel()
                {
                    GoogleId = userModel.GoogleId,
                    IngressName = userModel.IngressName,
                    Level = userModel.Level,
                    LocationKey = userModel.LocationKey,
                    OwnTracksPw = userModel.OwnTracksPw,
                    VVerified = userModel.VVerified,
                    VBlacklisted = userModel.VBlacklisted,
                    Vid = userModel.Vid,
                    OwnTracksJson = userModel.OwnTracksJson,
                    RocksVerified = userModel.RocksVerified,
                    Raid = userModel.Raid,
                    Risc = userModel.Risc,
                    Telegram = JsonConvert.SerializeObject(userModel.Telegram),
                    OwnedTeams = userModel.OwnedTeams,
                    Teams = userModel.Teams,
                    Ops = userModel.Ops,
                    OwnedOps = userModel.OwnedOps,
                    Assignments = userModel.Assignments

                };
            }
        }
    }
}