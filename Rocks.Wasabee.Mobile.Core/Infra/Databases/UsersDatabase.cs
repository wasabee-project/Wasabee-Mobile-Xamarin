using Newtonsoft.Json;
using Rocks.Wasabee.Mobile.Core.Infra.Logger;
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
        public UsersDatabase(IFileSystem fileSystem, ILoggingService loggingService) : base(fileSystem, loggingService, TimeSpan.FromDays(7))
        {

        }

        public override async Task<int> DeleteAllData()
        {
            LoggingService.Trace("Querying UsersDatabase.DeleteAllData");

            var databaseConnection = await GetDatabaseConnection<UserDatabaseModel>().ConfigureAwait(false);
            return await databaseConnection.DeleteAllAsync<UserDatabaseModel>().ConfigureAwait(false);
        }

        public async Task<List<UserTeamModel>> GetUserTeams(string googleId)
        {
            LoggingService.Trace("Querying UsersDatabase.GetUserTeams");
            
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
                LoggingService.Error(e, "Error Querying UsersDatabase.GetUserTeams");
                return new List<UserTeamModel>();
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<UserModel?> GetUserModel(string googleId)
        {
            LoggingService.Trace("Querying UsersDatabase.SaveUserModel");
            
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
                LoggingService.Error(e, "Error Querying UsersDatabase.GetUserModel");
                return null;
            }
            finally
            {
                dbLock.Dispose();
            }
        }

        public async Task<int> SaveUserModel(UserModel userModel)
        {
            LoggingService.Trace("Querying UsersDatabase.SaveUserModel");

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
                LoggingService.Error(e, "Error Querying UsersDatabase.SaveUserModel");
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

            public string EnlId { get; set; }

            public string IngressName { get; set; }

            public int Level { get; set; }

            public string LocationKey { get; set; }

            public bool RocksVerified { get; set; }

            public bool VVerified { get; set; }

            public bool Blacklisted { get; set; }

            public string ProfileImage { get; set; }

            public bool Raid { get; set; }

            public bool Risc { get; set; }

            public string Telegram { get; set; }

            public string TeamsBlobbed { get; set; }
            [TextBlob("TeamsBlobbed")]
            public List<UserTeamModel> Teams { get; set; }

            public string OpsBlobbed { get; set; }
            [TextBlob("OpsBlobbed")]
            public List<OpModel> Ops { get; set; }

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
                        EnlId = userDatabaseModel.EnlId,
                        IngressName = userDatabaseModel.IngressName,
                        Level = userDatabaseModel.Level,
                        LocationKey = userDatabaseModel.LocationKey,
                        RocksVerified = userDatabaseModel.RocksVerified,
                        VVerified = userDatabaseModel.VVerified,
                        Blacklisted = userDatabaseModel.Blacklisted,
                        ProfileImage = userDatabaseModel.ProfileImage,
                        Raid = userDatabaseModel.Raid,
                        Risc = userDatabaseModel.Risc,
                        Telegram = string.IsNullOrWhiteSpace(userDatabaseModel.Telegram) ? new TelegramModel() : JsonConvert.DeserializeObject<TelegramModel>(userDatabaseModel.Telegram),
                        Teams = userDatabaseModel.Teams ?? new List<UserTeamModel>(),
                        Ops = userDatabaseModel.Ops ?? new List<OpModel>(),
                        Assignments = userDatabaseModel.Assignments ?? new List<AssignmentModel>()

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
                    EnlId = userModel.EnlId,
                    IngressName = userModel.IngressName,
                    Level = userModel.Level,
                    LocationKey = userModel.LocationKey,
                    RocksVerified = userModel.RocksVerified,
                    VVerified = userModel.VVerified,
                    Blacklisted = userModel.Blacklisted,
                    ProfileImage = userModel.ProfileImage,
                    Raid = userModel.Raid,
                    Risc = userModel.Risc,
                    Telegram = userModel.Telegram != null ? JsonConvert.SerializeObject(userModel.Telegram) : string.Empty,
                    Teams = userModel.Teams ?? new List<UserTeamModel>(),
                    Ops = userModel.Ops ?? new List<OpModel>(),
                    Assignments = userModel.Assignments ?? new List<AssignmentModel>()
                };
            }
        }
#nullable enable
    }
}