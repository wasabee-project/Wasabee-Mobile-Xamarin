using Rocks.Wasabee.Core.Adapters;
using Rocks.Wasabee.Core.Models.Users;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System.Text.Json;

namespace Rocks.Wasabee.Core.Infra.Databases
{
    public class UsersDatabase : BaseDatabase
    {
        public UsersDatabase(IFileSystemAdapter fileSystem) : base(fileSystem, TimeSpan.FromDays(7))
        {

        }

        public override async Task<int> DeleteAllData()
        {
            var databaseConnection = await GetDatabaseConnection<UserDatabaseModel>().ConfigureAwait(false);
            return await databaseConnection.DeleteAllAsync<UserDatabaseModel>().ConfigureAwait(false);
        }

        public async Task<List<UserTeamModel>> GetUserTeams(string googleId)
        {
            if (string.IsNullOrEmpty(googleId))
                return new List<UserTeamModel>();

            var databaseConnection = await GetDatabaseConnection<UserDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var userDatabaseModel = databaseConnection.GetConnection().GetWithChildren<UserDatabaseModel>(googleId);
                var userModel = UserDatabaseModel.ToUserModel(userDatabaseModel);

                return userModel?.Teams ?? new List<UserTeamModel>();
            }
            catch (Exception e)
            {
                return new List<UserTeamModel>();
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<UserModel?> GetUserModel(string googleId)
        {
            if (string.IsNullOrEmpty(googleId))
                return null;

            var databaseConnection = await GetDatabaseConnection<UserDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var userDatabaseModel = databaseConnection.GetConnection().GetWithChildren<UserDatabaseModel>(googleId);

                return userDatabaseModel != null ?
                    UserDatabaseModel.ToUserModel(userDatabaseModel) :
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

        public async Task<int> SaveUserModel(UserModel userModel)
        {
            var databaseConnection = await GetDatabaseConnection<UserDatabaseModel>().ConfigureAwait(false);
            var dbLock = databaseConnection.GetConnection().Lock();
            try
            {
                var userDatabaseModel = UserDatabaseModel.ToUserDatabaseModel(userModel);

                databaseConnection.GetConnection().InsertOrReplaceWithChildren(userDatabaseModel, true);

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

#nullable disable
        class UserDatabaseModel
        {
            [PrimaryKey, Unique]
            public string GoogleId { get; set; }

            public string Name { get; set; }

            public int Level { get; set; }

            public string OneTimeToken { get; set; }

            public bool RocksVerified { get; set; }

            public bool Blacklisted { get; set; }

            public string ProfileImage { get; set; }

            public string Telegram { get; set; }

            public string TeamsBlobbed { get; set; }
            [TextBlob("TeamsBlobbed")]
            public List<UserTeamModel> Teams { get; set; }

            public string OpsBlobbed { get; set; }
            [TextBlob("OpsBlobbed")]
            public List<OpModel> Ops { get; set; }

            public static UserModel ToUserModel(UserDatabaseModel userDatabaseModel)
            {
                try
                {
                    return new UserModel()
                    {
                        GoogleId = userDatabaseModel.GoogleId,
                        Name = userDatabaseModel.Name,
                        Level = userDatabaseModel.Level,
                        OneTimeToken = userDatabaseModel.OneTimeToken,
                        RocksVerified = userDatabaseModel.RocksVerified,
                        Blacklisted = userDatabaseModel.Blacklisted,
                        Pic = userDatabaseModel.ProfileImage,
                        Telegram = string.IsNullOrWhiteSpace(userDatabaseModel.Telegram) ? new TelegramModel() :
                            JsonSerializer.Deserialize<TelegramModel>(userDatabaseModel.Telegram),
                        Teams = userDatabaseModel.Teams ?? new List<UserTeamModel>(),
                        Ops = userDatabaseModel.Ops ?? new List<OpModel>()

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
                    Name = userModel.Name,
                    Level = userModel.Level,
                    OneTimeToken = userModel.OneTimeToken,
                    RocksVerified = userModel.RocksVerified,
                    Blacklisted = userModel.Blacklisted,
                    ProfileImage = userModel.Pic,
                    Telegram = userModel.Telegram != null ?
                        JsonSerializer.Serialize(userModel.Telegram) : string.Empty,
                    Teams = userModel.Teams ?? new List<UserTeamModel>(),
                    Ops = userModel.Ops ?? new List<OpModel>()
                };
            }
        }
#nullable enable
    }
}