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

        public FirebaseStorageService()
        {
            _storageClient = StorageClient.Create();
            _bucketName = "motohut-security.appspot.com";
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
                    if (storageObject.Name.EndsWith(".MP4"))
                    {
                        var fileSize = storageObject.Size != null ? (long)storageObject.Size : 0;
                        var fileName = storageObject.Name.Substring(folderPath.Length);

                        var videoFileInfo = new VideoFileInfo
                        {
                            Name = fileName,
                            Size = fileSize
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
    }
}