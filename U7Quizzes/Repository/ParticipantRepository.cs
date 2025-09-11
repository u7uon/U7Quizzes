using Microsoft.EntityFrameworkCore;
using U7Quizzes.AppData;
using U7Quizzes.DTOs.Session;
using U7Quizzes.IRepository;
using U7Quizzes.Models;

namespace U7Quizzes.Repository
{
    public class ParticipantRepository : GenericRepository<Participant> ,  IParticipantRepository
    {
        public ParticipantRepository(ApplicationDBContext context) : base(context)
        {
        }

        public async Task<ParticipantDTO> SetName(int id, string name)
        {
            var participant = await _dbSet.FirstOrDefaultAsync(p => p.ParticipantId == id);

            participant.Nickname = name ;
            _dbSet.Update(participant);
            await _context.SaveChangesAsync();

            return new ParticipantDTO
            {
                ParticipantId = participant.ParticipantId,
                UserID = participant.UserId,
                DisplayName = participant.Nickname,
            };
        }
    }
}
