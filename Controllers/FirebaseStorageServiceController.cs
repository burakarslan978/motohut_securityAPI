using Microsoft.AspNetCore.Mvc;

namespace Motohut_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FirebaseStorageServiceController : Controller
    {
        private readonly FirebaseStorageService _firebaseStorageService;

        public FirebaseStorageServiceController(FirebaseStorageService firebaseStorageService)
        {
            _firebaseStorageService = firebaseStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVideoFilesAsync([FromQuery] string email)
        {
            // Use the email parameter to retrieve the list of video files
            var videoFiles = await _firebaseStorageService.GetVideoFilesAsync(email);
            return Ok(videoFiles);
        }
        [HttpGet("{videoName}")]
        public async Task<IActionResult> GetVideoDataAsync([FromQuery] string email, string videoName)
        {
            // Use the email and videoName parameters to retrieve the video data
            var videoData = await _firebaseStorageService.GetVideoDataAsync(email, videoName);

            // Return the video data as a file
            return File(videoData, "video/mp4");
        }
    }

}
