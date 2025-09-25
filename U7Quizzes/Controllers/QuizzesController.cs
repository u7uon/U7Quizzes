using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private IHubContext hubContext; 
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


        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] QuizFilter filter)
        {
            try
            {
                var quizzes = await _quizService.GetByTagName(filter);
                return quizzes is null || quizzes.Data is null ? NotFound() : Ok(quizzes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { messsage = ex.Message });
            }
           
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
            var creatorId = User.GetUserId();

            var dto = JsonConvert.DeserializeObject<QuizCreateDTO>(quiz);
            if (dto == null) return BadRequest("Dữ liệu quiz không hợp lệ");

            //dto.CoverImage = image; 

            var result = await _quizService.CreateAsync(dto, creatorId , image);

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

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var quiz = await _quizService.GetByIdAsync(id);
            return quiz == null ? NotFound() : Ok(quiz);
        }
    }
}
