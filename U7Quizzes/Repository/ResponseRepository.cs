using U7Quizzes.Models; 
using Microsoft.EntityFrameworkCore;
using U7Quizzes.IRepository;
using U7Quizzes.AppData;

namespace U7Quizzes.Repository
{
    public class ResponseRepository : GenericRepository<Response>, IResponseRepository
    {
        public ResponseRepository(ApplicationDBContext context) : base(context)
        {
            
        }
        public async Task<bool> IsResponseSubmitted(int participantId, int questionId)
        {
            return await _dbSet.AnyAsync(x => x.ParticipantId == participantId && x.QuestionId == questionId); 
        }
    }
}