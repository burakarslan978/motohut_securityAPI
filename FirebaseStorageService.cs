using Google;
using Google.Cloud.Storage.V1;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Motohut_API
{
    public class FirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly string _folderPath;

        public FirebaseStorageService(IHttpContextAccessor httpContextAccessor)
        {
            //var email = httpContextAccessor.HttpContext.User.FindFirst("email")?.Value;
            var email = "b.arslan32@gmail.com";
            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("User email not found.");
            }

            _storageClient = StorageClient.Create();
            _bucketName = "motohut-security.appspot.com";
            _folderPath = $"{email}";
        }

        //public async Task<List<VideoFileInfo>> GetVideoFilesAsync()
        //{
        //    var videoFiles = new List<VideoFileInfo>();

        //    var objects = _storageClient.ListObjects(_bucketName, _folderPath);
        //    foreach (var storageObject in objects)
        //    {
        //        if (storageObject.Name.EndsWith(".mp4"))
        //        {
        //            var fileSize = storageObject.Size != null ? (long)storageObject.Size : 0;
        //            var fileName = storageObject.Name.Substring(_folderPath.Length);

        //            var videoFileInfo = new VideoFileInfo
        //            {
        //                Name = fileName,
        //                Size = fileSize
        //            };
        //            videoFiles.Add(videoFileInfo);
        //        }
        //    }

        //    return videoFiles;
        //}

        public async Task<List<VideoFileInfo>> GetVideoFilesAsync()
        {
            var videoFiles = new List<VideoFileInfo>();

            try
            {
                // Use a `await foreach` loop to iterate over the objects in the bucket
                await foreach (var storageObject in _storageClient.ListObjectsAsync(_bucketName, _folderPath))
                {
                    if (storageObject.Name.EndsWith(".MP4"))
                    {
                        var fileSize = storageObject.Size != null ? (long)storageObject.Size : 0;
                        var fileName = storageObject.Name.Substring(_folderPath.Length);

                        var videoFileInfo = new VideoFileInfo
                        {
                            Name = fileName,
                            Size = fileSize
                        };
                        videoFiles.Add(videoFileInfo);
                    }
                }
            }
            catch (Google.GoogleApiException e)
            {
                throw e;
            }

            return videoFiles;
        }
    }
}