using _3.DTO_AutoMapper.Models;
using _3.DTO_AutoMapper.Models.Dtos;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace _3.DTO_AutoMapper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private static List<TodoItem> _todos = new List<TodoItem>();
        private static int _nextId = 1;
        private readonly IMapper _mapper;

        private readonly ILogger<TodoController> _logger;

        public TodoController(ILogger<TodoController> logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        // 전체 조회 (GET /api/todo)
        [HttpGet]
        public ActionResult<IEnumerable<TodoItem>> GetAll()
        {
            var todoDtos = _mapper.Map<IEnumerable<GetTodoDto>>(_todos);
            return Ok(todoDtos);
        }

        // 하나 조회 (GET /api/todo/{id})
        [HttpGet("{id}")]
        public ActionResult<TodoItem> GetById(int id)
        {
            var todo = _todos.FirstOrDefault(x => x.Id == id);
            if (todo == null)
                return NotFound();
            return Ok(_mapper.Map<GetTodoDto>(todo));
        }

        // 추가 (POST /api/todo)
        [HttpPost]
        public ActionResult<TodoItem> Create(CreateTodoDto createDto)
        {
            var todo = _mapper.Map<TodoItem>(createDto);
            todo.Id = _nextId++;
            _todos.Add(todo);
            var getDto = _mapper.Map<GetTodoDto>(todo);
            return CreatedAtAction(nameof(GetById), new { id = todo.Id }, getDto);
        }

        // 수정 (PUT /api/todo/{id})
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateTodoDto updateDto)
        {
            var todo = _todos.FirstOrDefault(x => x.Id == id);
            if (todo == null)
                return NotFound();
            _mapper.Map(updateDto, todo);
            return Ok(updateDto);
        }

        // 삭제 (DELETE /api/todo/{id})
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var todo = _todos.FirstOrDefault(x => x.Id == id);
            if (todo == null)
                return NotFound();
            _todos.Remove(todo);
            return Ok();
        }
    }
}
