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
using Android.Graphics;
using System.Net;
using System.Threading.Tasks;
using SQLite.Net;

namespace RescueMe.Droid.Data
{
    public class DbContext
    {
        private static DbContext _instance;
        private SQLiteConnection _connection;
        public bool IsNetworkConnected { get; set; }
        private DbContext()
        {
            //_
            //string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            //_connection = new SQLiteAsyncConnection(System.IO.Path.Combine(path, "db.db3"));
            var platform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroidN();
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            _connection = new SQLiteConnection(platform, System.IO.Path.Combine(path, "db.db3"));

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

        public SQLiteConnection Connection
        {
            get
            {
                return _connection;
            }
        }
        private async Task CreateDatabase()
        {
            try
            {
                _connection.CreateTable<UserSaved>();
                _connection.CreateTable<StatusSaved>();
                _connection.CreateTable<VehicleSaved>();
                _connection.CreateTable<Settings>();
                _connection.CreateTable<ReasonRequestSaved>();
                _connection.CreateTable<RequestSaved>();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Remove all user information 
        /// </summary>
        public void LogOut()
        {
            //var user = _connection.Table<UserSaved>().FirstOrDefaultAsync().Result;
            //Remove(user);
            _connection.DeleteAll<UserSaved>();
            _connection.DeleteAll<Settings>();
            _connection.CreateTable<StatusSaved>();
            _connection.DeleteAll<VehicleSaved>();
            _connection.DeleteAll<ReasonRequestSaved>();
        }

        /// <summary>
        /// Save all user information
        /// </summary>
        /// <param name="user"></param>
        /// <param name="vehicles"></param>
        public void LogIn(UserProfile user, List<Vehicle> vehicles,
                                List<ReasonRequest> reasons,
                                List<Request> requests = null,
                                List<Status> status = null)
        {
            try
            {

                SaveUser(user);

                if (vehicles != null)
                {
                    UpdateVehicles(vehicles);
                }
                if (reasons != null)
                {
                    UpdateReasons(reasons);
                }
                if (status != null)
                {
                    UpdateStatus(status);
                }
                if (requests != null)
                {
                    UpdateRequests(requests);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool SaveUser(UserProfile user)
        {
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
            var isSaved = _connection.Update(userSaved) > 0;
            if (isSaved == false)
            {
                isSaved = _connection.Insert(userSaved) > 0;
            }
            return isSaved;
        }

        /// <summary>
        /// Update Vehicle List
        /// </summary>
        /// <param name="vehicles"></param>
        public void InsertVehicle(Vehicle vehicle)
        {
            var vehicleSaved = new VehicleSaved()
            {
                Id = vehicle.Id,
                Marque = vehicle.Marque,
                Type = vehicle.Type
            };

            _connection.Insert(vehicleSaved);
        }
        /// <summary>
        /// Update Vehicle List
        /// </summary>
        /// <param name="vehicles"></param>
        public void UpdateVehicles(List<Vehicle> vehicles)
        {
            var vehicleSaved = vehicles.Select(v => new VehicleSaved()
            {
                Id = v.Id,
                Marque = v.Marque,
                Type = v.Type
            }).ToList();

            var isSaved = _connection.UpdateAll(vehicleSaved) > 0;
            if (isSaved == false)
            {
                _connection.InsertAll(vehicleSaved);
            }

        }

        public void InsertRequest(Request request)
        {
            var requestSaved = new RequestSaved()
            {
                Id = request.Id,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                StatusID = request.StatusID,
                Comments = request.Comments,
                VehicleID = request.VehicleID.HasValue ? request.VehicleID.Value : 0,
                ReasonID = request.ReasonID,
                AgentName = request.AgentProfile == null ? "" : request.AgentProfile.Name,
                Time = request.CreatedAt.ToString()

            };
            if (IsNetworkConnected)
            {
                _connection.Insert(requestSaved);
            }
            else
            {
                var lastRequested =
                      _connection.Table<RequestSaved>().OrderByDescending(o => o.Id).FirstOrDefault();
                if (lastRequested != null)
                {
                    requestSaved.Id = lastRequested.Id + 1;
                    requestSaved.Type = "offline";
                }
                _connection.Insert(requestSaved);

            }

        }
        public async void UpdateRequests(List<Request> requests)
        {

            var requestsSaved = requests.Select(r => new RequestSaved()
            {
                Id = r.Id,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                StatusID = r.StatusID,
                Comments = r.Comments,
                VehicleID = r.VehicleID.HasValue ? r.VehicleID.Value : 0,
                ReasonID = r.ReasonID,
                Status = r.Status.Name,
                AgentName = r.AgentProfile.Name,
                Time = r.CreatedAt.ToString()
            }).ToList();

            var isSaved = _connection.UpdateAll(requestsSaved) > 0;
            if (isSaved == false)
            {
                _connection.InsertAll(requestsSaved);
            }
            //Download all image's
            foreach (var request in requests)
            {
                await GetImageBitmapFromRequest(request);
            }
        }
        public void UpdateStatus(List<Status> status)
        {
            var statusSaved = status.Select(s => new StatusSaved()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList();
            var isSaved = _connection.UpdateAll(statusSaved) > 0;
            if (isSaved == false)
            {
                _connection.InsertAll(statusSaved);
            }

        }

        public List<Status> getStatusList()
        {
            var listStatus = _connection.Table<StatusSaved>().ToList().Select(s => new Status()
            {
                Id = s.Id,
                Name = s.Name
            }).ToList();

            return listStatus;
        }

        /// <summary>
        /// Update Status ---Cancel-Close
        /// </summary>
        /// <param name="requestID"></param>
        public void CancelRequestStatus(int requestID)
        {
            var request = _connection.Table<RequestSaved>().FirstOrDefault(r => r.Id == requestID);
            var status = getStatusList().FirstOrDefault(s => s.Name == "cancelado");
            request.StatusID = status.Id;
            request.Status = status.Name;
            _connection.Update(request);
        }
        /// <summary>
        /// Update Id of Request from API
        /// </summary>
        /// <param name="requestToUpdate"></param>
        /// <param name="requestId"></param>
        /// <param name="statusId"></param>
        public void UpdateRequest(Request requestToUpdate, int requestId, int statusId)
        {
            var request = _connection.Table<RequestSaved>().FirstOrDefault(r => r.Id == requestToUpdate.Id);
            var status = getStatusList().FirstOrDefault(s => s.Id == statusId);
            request.StatusID = status.Id;
            request.Id = requestId;
            request.Status = status.Name;
            request.AgentName = requestToUpdate.AgentProfile.Name;
            //_connection.Update(request);
            //_connection.trans
            _connection.Execute($@"UPDATE RequestSaved 
                                    SET StatusID = {status.Id}, 
                                        Status = '{status.Name}', 
                                        AgentName = '{requestToUpdate.AgentProfile.Name}',  
                                        Id = {requestId} 
                                    WHERE Id = {requestToUpdate.Id}");
          
        }

        public void CloseRequestStatus(int requestID)
        {
            var request = _connection.Table<RequestSaved>().FirstOrDefault(r => r.Id == requestID);
            var status = getStatusList().FirstOrDefault(s => s.Name == "completado");
            request.StatusID = status.Id;
            request.Status = status.Name;
            _connection.Update(request);

        }








        /// <summary>
        /// Save image request
        /// </summary>
        /// <param name="request"></param>
        private async Task GetImageBitmapFromUrl(Request request)
        {
            using (var webClient = new WebClient())
            {
                //Save Local
                webClient.DownloadDataCompleted += (s, e) =>
                {
                    var bytes = e.Result; // get the downloaded data
                    string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                    string localFilename = $"{request.Id}_{GetUser().UserID}.png";
                    string localPath = System.IO.Path.Combine(documentsPath, localFilename);
                    File.WriteAllBytes(localPath, bytes); // writes to local storage
                };
                webClient.DownloadDataAsync(new Uri($"http://rescueme-api.azurewebsites.net/api/Map?" +
                                       $"requestID={request.Id}&UserID={GetUser().UserID}"));


            }

        }
        public async Task<Bitmap> GetImageBitmapFromRequest(Request request)
        {
            Stream fileImage;
            Bitmap imageBitmap = null;
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string localFilename = $"{request.Id}_{GetUser().UserID}.png";
            string localPath = System.IO.Path.Combine(documentsPath, localFilename);
            //Return Local image or 
            if (System.IO.File.Exists(localPath))
            {
                fileImage = System.IO.File.OpenRead(localPath);
                imageBitmap = BitmapFactory.DecodeStream(fileImage);
            }
            else if (IsNetworkConnected)
            {
                await GetImageBitmapFromUrl(request);
            }
            return imageBitmap;
        }
       
        public List<Request> GetRequest()
        {
            var requests = _connection.Table<RequestSaved>().ToList()
                                      .Select(r => new Request()
                                      {
                                          Id = r.Id,
                                          Latitude = r.Latitude,
                                          Longitude = r.Longitude,
                                          StatusID = r.StatusID,
                                          Comments = r.Comments,
                                          VehicleID = r.VehicleID,
                                          ReasonID = r.ReasonID,
                                          ReasonRequest = GetReasons().FirstOrDefault(l => l.Id == r.ReasonID),
                                          Vehicle = GetVehicleByRequest(r.VehicleID),
                                          User = GetUser().User,
                                          Status = new Status()
                                          {
                                              Name = getStatusList().FirstOrDefault(s => s.Id == r.StatusID).Name
                                          },
                                          AgentProfile = new UserProfile()
                                          {
                                              Name = r.AgentName
                                          },
                                          CreatedAt = DateTime.Parse(r.Time)

                                      }).OrderByDescending(o => o.Id).ToList();

            return requests;
        }
        private Vehicle GetVehicleByRequest(int VehicleID)
        {
            Vehicle vehicle;
            if (GetVehicles().Count > 0 &&
                    GetVehicles().FirstOrDefault(v => v.Id == VehicleID) != null)
            {
                vehicle = GetVehicles().FirstOrDefault(v => v.Id == VehicleID);
            }
            else
            {
                vehicle = new Vehicle() { Marque = "Vehículo de Tercero", Id = 0 };
            }

            return vehicle;
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
                vehicles = _connection.Table<VehicleSaved>().ToList()
                                        .Select(v => new Vehicle()
                                        {
                                            Id = v.Id,
                                            Marque = v.Marque,
                                            Type = v.Type
                                        }).ToList();

            }
            catch (Exception e)
            {
                throw e;
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
                var userProfile = _connection.Table<UserSaved>().FirstOrDefault();
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
            catch (Exception e)
            {
                return null;
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
                setting = _connection.Table<Settings>().FirstOrDefault();

            }
            catch (SQLite.Net.SQLiteException e)
            {
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }

            return setting;
        }
        public void SaveSetting(Settings setting)
        {
            _connection.Insert(setting);
        }

        //Get Reasons
        public List<ReasonRequest> GetReasons()
        {
            var reasons = _connection.Table<ReasonRequestSaved>().Select(r => new ReasonRequest()
            {
                Id = r.Id,
                Name = r.Name
            }).ToList(); ;
            return reasons;
        }
        /// <summary>
        /// Update all Reasons
        /// </summary>
        /// <param name="reasons"></param>
        public void UpdateReasons(List<ReasonRequest> reasons)
        {
            var reasonSaved = reasons.Select(r => new ReasonRequestSaved()
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();
            var isSaved = _connection.UpdateAll(reasonSaved) > 0;
            if (isSaved == false)
            {
                _connection.InsertAll(reasonSaved);
            }

        }




    }
}