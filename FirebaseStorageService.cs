using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Storage.v1;
using Google.Cloud.Storage.V1;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Motohut_API
{
    public class FirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly UrlSigner _urlSigner;

        public FirebaseStorageService()
        {
            var credentials = GoogleCredential.FromFile(@"./motohut-security-667ba3d83463.json");
            _storageClient = StorageClient.Create(credentials);
            _bucketName = "motohut-security.appspot.com";
            _urlSigner = UrlSigner.FromCredential(credentials);
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
        //gs://motohut-security.appspot.com/b.arslan32@gmail.com/GH012776.MP4
        //private async Task<string> GetVideoDownloadUrlAsync(string email, string fileName)
        //{
        //    // Get a reference to the video file in Firebase Storage
        //    var fileRef = _storageClient.GetBucket(_bucketName).GetObject(email + fileName);

        //    // Get the download URL for the video file
        //    var downloadUrl = await fileRef.GetSignedUrlAsync(new SignedUrlOptions
        //    {
        //        Method = HttpMethod.Get,
        //        Expiration = DateTimeOffset.UtcNow.AddHours(1)
        //    });

        //    return downloadUrl;
        //}

        public async Task<string> GetDownloadUrlAsync(string folderName, string objectName)
        {
            var fullObjectName = $"{folderName}{objectName}";
            var url = await _urlSigner.SignAsync(_bucketName, fullObjectName, TimeSpan.FromHours(1));
            return url;
        }

    }
}