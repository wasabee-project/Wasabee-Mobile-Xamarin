﻿using Newtonsoft.Json;
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

            var userModel = await GetUserModel(googleId);
            return userModel?.Teams ?? new List<UserTeamModel>();
        }

        public async Task<UserModel?> GetUserModel(string googleId)
        {
            LoggingService.Trace("Querying UsersDatabase.GetUserModel");

            var databaseConnection = await GetDatabaseConnection<UserDatabaseModel>().ConfigureAwait(false);

            var dbLock = databaseConnection.GetConnection().Lock();
            var userDatabaseModel = databaseConnection.GetConnection().GetWithChildren<UserDatabaseModel>(googleId);
            dbLock.Dispose();

            return userDatabaseModel != null ?
                UserDatabaseModel.ToUserModel(userDatabaseModel) :
                new UserModel();
        }

        public async Task<int> SaveUserModel(UserModel userModel)
        {
            LoggingService.Trace("Querying UsersDatabase.SaveUserModel");

            var databaseConnection = await GetDatabaseConnection<UserDatabaseModel>().ConfigureAwait(false);
            var userDatabaseModel = UserDatabaseModel.ToUserDatabaseModel(userModel);

            var dbLock = databaseConnection.GetConnection().Lock();

            try
            {
                databaseConnection.GetConnection().InsertOrReplaceWithChildren(userDatabaseModel, true);
            }
            catch (Exception e)
            {
                LoggingService.Error(e, "Error Querying UsersDatabase.SaveUserModel");
                return 1;
            }

            dbLock.Dispose();

            return 0;
        }

#nullable disable
        class UserDatabaseModel
        {
            [PrimaryKey, Unique]
            public string GoogleId { get; set; }

            public string IngressName { get; set; }

            public string ProfileImage { get; set; }

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
            public List<UserTeamModel> OwnedTeams { get; set; }

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
                        IngressName = userDatabaseModel.IngressName,
                        ProfileImage = userDatabaseModel.ProfileImage,
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
                        Telegram = string.IsNullOrWhiteSpace(userDatabaseModel.Telegram) ? new TelegramModel() : JsonConvert.DeserializeObject<TelegramModel>(userDatabaseModel.Telegram),
                        OwnedTeams = userDatabaseModel.OwnedTeams ?? new List<UserTeamModel>(),
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
                    IngressName = userModel.IngressName,
                    ProfileImage = userModel.ProfileImage,
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
                    Telegram = userModel.Telegram != null ? JsonConvert.SerializeObject(userModel.Telegram) : string.Empty,
                    OwnedTeams = userModel.OwnedTeams ?? new List<UserTeamModel>(),
                    Teams = userModel.Teams ?? new List<UserTeamModel>(),
                    Ops = userModel.Ops ?? new List<OpModel>(),
                    Assignments = userModel.Assignments ?? new List<AssignmentModel>()
                };
            }
        }
#nullable enable
    }
}