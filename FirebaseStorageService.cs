using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1;
using Google.Cloud.Datastore.V1;
using Google.Cloud.Storage.V1;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Grpc.Core.Metadata;

namespace Motohut_API
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly UrlSigner _urlSigner;
        private readonly DatastoreDb _db;

        public FirebaseStorageService()
        {
            var credentials = GoogleCredential.FromFile(@"./motohut-security-667ba3d83463.json");
            _storageClient = StorageClient.Create(credentials);
            _bucketName = "motohut-security.appspot.com";
            _urlSigner = UrlSigner.FromCredential(credentials);
            DatastoreClient client = new DatastoreClientBuilder { CredentialsPath = @"./motohut-security-667ba3d83463.json" }.Build();
            _db = DatastoreDb.Create("motohut-security", "", client);
        }

        public async Task<List<VideoFileInfo>> GetVideoFilesAsync(string email)
        {
            var videoFiles = new List<VideoFileInfo>();

            try
            {
                // Use a `await foreach` loop to iterate over the objects in the bucket
                var folderPath = $"{email}";
                await foreach (var storageObject in _storageClient.ListObjectsAsync(_bucketName, folderPath))
                {
                    if (storageObject.Name.EndsWith(".MP4") || storageObject.Name.EndsWith(".mp4"))
                    {
                        var fileSize = storageObject.Size != null ? (long)storageObject.Size : 0;
                        var fileName = storageObject.Name.Substring(folderPath.Length);

                        var videoFileInfo = new VideoFileInfo
                        {
                            Name = fileName,
                            Size = fileSize,
                            DownloadUrl = await GetDownloadUrlAsync(email, fileName)
                        };
                        videoFiles.Add(videoFileInfo);
                    }
                }
            }
            catch (Google.GoogleApiException ex)
            {
                throw ex;
            }

            return videoFiles;
        }

        public async Task<byte[]> GetVideoDataAsync(string email, string videoName)
        {
            try
            {
                // Get the full path of the video file in the bucket
                var filePath = $"{email}/{videoName}";

                // Download the video data
                var videoData = await _storageClient.GetObjectAsync(_bucketName, filePath);

                // Use the MediaLink property to download the video data
                using (var memoryStream = new MemoryStream())
                using (var httpClient = new HttpClient())
                {
                    var mediaLink = videoData.MediaLink;
                    var response = await httpClient.GetAsync(mediaLink);
                    await response.Content.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (GoogleApiException ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetDownloadUrlAsync(string folderName, string objectName)
        {
            var fullObjectName = $"{folderName}{objectName}";
            var url = await _urlSigner.SignAsync(_bucketName, fullObjectName, TimeSpan.FromHours(1));
            return url;
        }

        public async Task<string> AddUserAsync(string email, int huisnummer, string postcode, string stad, string straat)
        {
            Entity user = new Entity
            {
                Key = _db.CreateKeyFactory("Adres").CreateIncompleteKey(),
                ["Email"] = email,
                ["Huisnummer"] = huisnummer,
                ["Postcode"] = postcode,
                ["Stad"] = stad,
                ["Straat"] = straat
            };
            using (DatastoreTransaction transaction = await _db.BeginTransactionAsync())
            {
                transaction.Upsert(user);
                await transaction.CommitAsync();
            }
            return user.Key.Path.First().Id.ToString();
        }

        public async Task<long> CheckEmailAsync(string email)
        {
            Query query = new Query("Adres")
            {
                Filter = Filter.Equal("Email", email)
            };
            var results = await _db.RunQueryAsync(query);
            if (results.Entities.Any())
            {
                return results.Entities.First().Key.Path.First().Id;
            }
            else
            {
                return 0;
            }
        }


    }
}