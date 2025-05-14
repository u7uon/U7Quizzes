using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc;
using U7Quizzes.IServices;

namespace U7Quizzes.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {

        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            this._imageService = imageService;
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetImage(string name)
        {
            try
            {
                var file = await _imageService.GetAsync(name);
                return file; 

            }
            catch(FileNotFoundException )
            {
                return NotFound(); 
            } 
           
        }
    }
}
