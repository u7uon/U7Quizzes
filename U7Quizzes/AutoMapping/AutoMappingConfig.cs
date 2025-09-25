using AutoMapper;
using U7Quizzes.DTOs.Quiz;
using U7Quizzes.DTOs.Session;
using U7Quizzes.Models;

namespace U7Quizzes.AutoMapping
{
    public class QuizProfile : Profile
    {
        public QuizProfile()
        {
            CreateMap<QuizCreateDTO, Quiz>()
                .ForMember(dest => dest.QuizCategories, opt => opt.Ignore())
                .ForMember(dest => dest.QuizTags, opt => opt.Ignore());

            CreateMap<QuizUpdateDTO, Quiz>().IncludeBase<QuizCreateDTO, Quiz>();

            CreateMap<QuestionCreateDTO, Question>();
            CreateMap<AnswerCreateDTO, Answer>();

            CreateMap<Quiz, QuizDTO>()
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.QuizCategories.Select(qc => qc.Category.Name)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.QuizTags.Select(qt => qt.Tag.Name)));

            CreateMap<Question, QuestionDTO>();
            CreateMap<Answer, AnswerDTO>();
            CreateMap<Question, QuestionResponse>();

            // Map Answer -> AnswerResponse
            CreateMap<Answer, AnswerResponse>();

            CreateMap<Answer, AnswerGetDTO>();


            CreateMap<Question, QuestionGetDTO>()
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers));

            CreateMap<ParticipantDTO, Participant>();


            CreateMap<Session, SessionDTO>().ForMember(dest => dest.SessionStatus, op => op.MapFrom(x => x.Status.ToString()))
                                            .ForMember(dest => dest.HostName, op => op.MapFrom(x => x.Host.DisplayName))
                                            .ForMember(dest => dest.Status, op => op.Ignore());

            CreateMap<Response, ResponseDTO>(); 
            
        }

    }
}
