using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;
using System.IO;
using RescueMe.Domain;

namespace RescueMe.Droid.Data
{
    public class DbContext
    {
        private static DbContext _instance;
        private SQLiteAsyncConnection _connection;

        private DbContext()
        {
            //_
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            _connection = new SQLiteAsyncConnection(Path.Combine(path, "db.db3"));
            CreateDatabase();
        }
        public static DbContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DbContext();
                }
                return _instance;
            }
        }

        public SQLiteAsyncConnection Connection
        {
            get
            {
                return _connection;
            }
        }
        private void CreateDatabase()
        {
            try
            {
                var userProfileSaved = _connection.Table<UserSaved>().CountAsync().Result;
                var vehicleSaved = _connection.Table<VehicleSaved>().CountAsync().Result;
                var SettingSaved = _connection.Table<Settings>().CountAsync().Result;
            }
            catch (Exception e)
            {
                try
                {
                    _connection.CreateTableAsync<UserSaved>().Wait();
                    _connection.CreateTableAsync<VehicleSaved>().Wait();
                    _connection.CreateTableAsync<Settings>().Wait();
                }catch(Exception m)
                {
                    var a = 1;
                }
            }
        }

        /// <summary>
        /// Remove all user information 
        /// </summary>
        public void LogOut()
        {
            //var user = _connection.Table<UserSaved>().FirstOrDefaultAsync().Result;
            //Remove(user);
            _connection.DropTableAsync<UserSaved>().Wait();
            _connection.DropTableAsync<Settings>().Wait();
            _connection.DropTableAsync<VehicleSaved>().Wait();
        }

        /// <summary>
        /// Save all user information
        /// </summary>
        /// <param name="user"></param>
        /// <param name="vehicles"></param>
        public void LogIn(UserProfile user, 
                                List<Vehicle> vehicles)
        {
            UpdateUser(user);
            UpdateVehicles(vehicles);
        }
        public void UpdateUser(UserProfile user)
        {
            _connection.DropTableAsync<UserSaved>().Wait();
            _connection.CreateTableAsync<UserSaved>().Wait();
            var userSaved = new UserSaved()
            {
                City = user.City,
                Email = user.Email,
                Name = user.Name,
                Id = user.Id,
                IdentificationCard = user.IdentificationCard,
                PassworDigest = user.User.PassworDigest,
                TelephoneNumber = user.TelephoneNumber,
                Type = user.User.Type,
                UserID = user.UserID,
                LastLogged = DateTime.Now
            };
            _connection.InsertAsync(userSaved);
        }
        public void UpdateVehicles(List<Vehicle> vehicles)
        {
            _connection.DropTableAsync<VehicleSaved>().Wait();
            _connection.CreateTableAsync<VehicleSaved>().Wait();
            var vehicleSaved = vehicles.Select(v => new VehicleSaved() {
                Id = v.Id,
                Marque = v.Marque,
                Type = v.Type
            }).ToList();

            _connection.InsertAllAsync(vehicleSaved);
        }
        /// <summary>
        /// Return All Vehicles
        /// </summary>
        /// <returns></returns>
        public List<Vehicle> GetVehicles()
        {
            List<Vehicle> vehicles = null;
            try
            {
                vehicles = _connection.Table<VehicleSaved>().ToListAsync()
                                        .Result.Select(v => new Vehicle()
                                        {
                                            Id = v.Id,
                                            Marque = v.Marque,
                                            Type = v.Type
                                        }).ToList();

            }
            catch (Exception e)
            {
                CreateDatabase();
            }
            return vehicles;
        }
        /// <summary>
        /// Return Current User Information
        /// </summary>
        /// <returns></returns>
        public UserProfile GetUser()
        {
            try
            {
                var userProfile = _connection.Table<UserSaved>().FirstOrDefaultAsync().Result;
                if (userProfile != null)
                {
             
                    return new UserProfile()
                    {
                        Email = userProfile.Email,
                        Name = userProfile.Name,
                        IdentificationCard = userProfile.IdentificationCard,
                        TelephoneNumber = userProfile.TelephoneNumber,
                        UserID = userProfile.UserID,
                        User = new User()
                        {
                            PassworDigest = userProfile.PassworDigest,
                            Type = userProfile.Type,
                            Id = userProfile.Id
                        },
                        Id = userProfile.Id
                    };

                }
            }
            catch (Exception)
            {
                CreateDatabase();
            }
            return null;
        }

        /// <summary>
        /// Return Settings about application
        /// </summary>
        /// <returns></returns>
        public Settings GetSettings()
        {
            Settings setting = null;
            try
            {
                setting = _connection.Table<Settings>().FirstOrDefaultAsync().Result;

            }
            catch (Exception e)
            {
                CreateDatabase();
            }
            return setting;
        }
        public void SaveSetting(Settings setting)
        {
            _connection.DropTableAsync<Settings>().Wait();
            _connection.CreateTableAsync<Settings>().Wait();
            _connection.InsertAsync(setting);
        }

    }
}