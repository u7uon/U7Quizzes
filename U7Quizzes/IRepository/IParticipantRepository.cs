using U7Quizzes.DTOs.Session;
using U7Quizzes.IServices;
using U7Quizzes.Models;

namespace U7Quizzes.IRepository
{
    public interface IParticipantRepository : IGenericRepository<Participant>
    {

        Task<ParticipantDTO> SetName(int id, string name); 
    }
}
