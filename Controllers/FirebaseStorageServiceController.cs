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
        public async Task<IActionResult> GetVideoFilesAsync()
        {
            var videoFiles = await _firebaseStorageService.GetVideoFilesAsync();
            return Ok(videoFiles);
        }
    }
}
