using System.ComponentModel.DataAnnotations;

namespace Dockerfiles.Models.Dtos
{
    public class CreateTodoDto
    {
        [Required(ErrorMessage = "제목은 필수입니다.")]
        public string Title { get; set; }
    }
}
