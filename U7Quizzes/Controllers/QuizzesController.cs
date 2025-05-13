using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.Extensions;
using U7Quizzes.IServices;

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
        public async Task<IActionResult> Create([FromBody] QuizCreateDTO dto)
        {
            var creatorId = User.GetUserId(); 
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
