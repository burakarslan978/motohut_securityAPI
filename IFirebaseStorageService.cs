using Motohut_API;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IFirebaseStorageService
{
    Task<List<VideoFileInfo>> GetVideoFilesAsync(string email);
    Task<byte[]> GetVideoDataAsync(string email, string videoName);
    Task<string> GetDownloadUrlAsync(string folderName, string objectName);
    Task<string> AddUserAsync(string email, int huisnummer, string postcode, string stad, string straat);
    Task<long> CheckEmailAsync(string email);
}
