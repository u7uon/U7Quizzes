using AutoMapper;
using AutoMapper.QueryableExtensions;
using CloudinaryDotNet.Core;
using Microsoft.EntityFrameworkCore;
using U7Quizzes.AppData;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.Extensions;
using U7Quizzes.IRepository;
using U7Quizzes.Models;

namespace U7Quizzes.Repository
{
    public class AnswerRepository : IAnswerRepository
    {
        private readonly ApplicationDBContext _context;
         private readonly IMapper _map;


        public AnswerRepository(ApplicationDBContext context, IMapper mapper)
        {
            _context = context;
            _map = mapper; 
        }
        
        public async Task<AnswerResponse?> GetAnswerById(int id)
        {
            return await _context.Answer.ProjectTo<AnswerResponse>(_map.ConfigurationProvider).FirstOrDefaultAsync(x => x.AnswerId == id).ConfigureAwaitFalse();
        }
    }
}