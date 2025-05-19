using Microsoft.AspNetCore.Mvc.RazorPages;

namespace U7Quizzes.DTOs.Response
{
    public class FilterBase<T> where T:  class
    {
        public string? Keyword { get; set;  }
        public List<T>? Data { get; set; }
        public int CurrentPage { get; set;  } 
        public int MaxPage { get; set;  }

    }
}
 