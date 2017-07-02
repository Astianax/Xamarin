﻿using System;
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
                var userSaved = _connection.Table<UserSaved>().CountAsync().Result;
                var vehicleSaved = _connection.Table<Vehicle>().CountAsync().Result;
            }
            catch (Exception e)
            {
                _connection.CreateTableAsync<UserSaved>().Wait();
                _connection.CreateTableAsync<Vehicle>().Wait();
            }
        }

        public UserProfile GetUser()
        {
            try
            {
                var user = _connection.Table<UserSaved>().FirstOrDefaultAsync().Result;
                if (user != null)
                {
                    return new UserProfile()
                    {
                        Email = user.Email,
                        Name = user.FullName,
                        Id = user.Id
                    };
                }
            }
            catch (Exception)
            {
                CreateDatabase();
            }
            return null;
        }
        public void Save<T>(T model)
        {
            _connection.InsertAsync(model);
        }

        public void Remove<T>(T model)
        {
            _connection.DeleteAsync(model).Wait();
            
        }
        public void LogOut()
        {
            //var user = _connection.Table<UserSaved>().FirstOrDefaultAsync().Result;
            //Remove(user);
            _connection.DropTableAsync<UserSaved>().Wait();
        }

    }
}