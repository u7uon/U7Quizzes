using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.Extensions;
using U7Quizzes.IServices;
using U7Quizzes.Models;

namespace U7Quizzes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizzesController : Controller
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var quizzes = await _quizService.GetAllAsync();
            return Ok(quizzes);
        }


        [HttpGet("/{tagname}")]
        public async Task<IActionResult> GetByTagsName([FromQuery] string tagname)
        {
            var quizzes = await _quizService.GetByTagName(tagname);
            return quizzes is null ? NotFound() : Ok(quizzes); 
        } 

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var quiz = await _quizService.GetByIdAsync(id);
            return quiz == null ? NotFound() : Ok(quiz);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Create([FromForm] string quiz , IFormFile image)
        {
            Console.WriteLine(quiz);

            var creatorId = User.GetUserId();

            var dto = JsonConvert.DeserializeObject<QuizCreateDTO>(quiz);
            if (dto == null) return BadRequest("Dữ liệu quiz không hợp lệ");

            dto.CoverImage = image; 

            var result = await _quizService.CreateAsync(dto, creatorId);

            if (result.IsSuccess)
            {
                return Created();
            }
            else
            {
                return BadRequest(new { Message = result.Error }); 
            }   
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Update([FromBody] QuizUpdateDTO dto)
        {
            var updated = await _quizService.UpdateAsync(dto);
            return updated ? Created() : BadRequest();
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _quizService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
