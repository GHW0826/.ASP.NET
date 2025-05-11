using _5.FluentValidation.Data;
using _5.FluentValidation.Models;
using _5.FluentValidation.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace _5.FluentValidation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        private readonly ILogger<TodoController> _logger;

        public TodoController(ILogger<TodoController> logger, IMapper mapper, AppDbContext context)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // ��ü ��ȸ (GET /api/todo)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetTodoDto>>> GetAll()
        {
            var todos = await _context.Todos.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<GetTodoDto>>(todos));
        }

        // �ϳ� ��ȸ (GET /api/todo/{id})
        [HttpGet("{id}")]
        public async Task<ActionResult<GetTodoDto>> GetById(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return NotFound();
            return Ok(_mapper.Map<GetTodoDto>(todo));
        }
        
            
        // �߰� (POST /api/todo)
        [HttpPost]
        public async Task<ActionResult<GetTodoDto>> Create(CreateTodoDto createDto)
        {
            var todo = _mapper.Map<TodoItem>(createDto);
            _context.Todos.Add(todo);

            await _context.SaveChangesAsync();
            var getDto = _mapper.Map<GetTodoDto>(todo);
            return CreatedAtAction(nameof(GetById), new { id = todo.Id }, getDto);
        }
        
        // ���� (PUT /api/todo/{id})
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateTodoDto updateDto)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return NotFound();

            _mapper.Map(updateDto, todo);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // ���� (DELETE /api/todo/{id})
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return NotFound();

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
