using Microsoft.AspNetCore.Mvc.RazorPages;

namespace U7Quizzes.DTOs.Response
{
    public class PagedResult<T> where T:  class
    {
        public List<T>? Data { get; set; }
        public int CurrentPage { get; set;  } 
        public int MaxPage { get; set;  }

    }
}
 