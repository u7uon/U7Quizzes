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
                .ForMember(x => x.CoverImage , o => o.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.QuizCategories.Select(qc => qc.Category.Name)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.QuizTags.Select(qt => qt.Tag.Name)));

            CreateMap<Question, QuestionDTO>();
            CreateMap<Answer, AnswerDTO>();


            CreateMap<Answer, AnswerGetDTO>();

            // Map Question -> QuestionGetDTO
            CreateMap<Question, QuestionGetDTO>()
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers));

            CreateMap<ParticipantDTO, Participant>();
        }

    }
}
