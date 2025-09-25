using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using U7Quizzes.IServices;

namespace U7Quizzes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadsController(IImageService _file) : Controller
    {
        [HttpGet("images/{fileName}")]
        public async Task<FileStreamResult> GetImgae(string fileName)
        {
            return await _file.GetAsync(fileName);
        }
    }
}
