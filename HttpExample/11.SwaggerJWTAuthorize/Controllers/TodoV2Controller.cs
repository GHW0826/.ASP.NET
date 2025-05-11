using SwaggerJWTAuthorize.Data;
using SwaggerJWTAuthorize.Models;
using SwaggerJWTAuthorize.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace SwaggerJWTAuthorize.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("api/v{version:apiVersion}/todo")]
    public class TodoV2Controller : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        private readonly ILogger<TodoController> _logger;

        public TodoV2Controller(ILogger<TodoController> logger, IMapper mapper, AppDbContext context)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        // ��ü ��ȸ (GET /api/todo)
        [HttpGet]
        [SwaggerOperation(Summary = "��ü Todo ��� ��ȸ", Description = "DB�� ����� ��� Todo �׸��� ��ȸ�մϴ�.")]
        [SwaggerResponse(200, "����", typeof(IEnumerable<GetTodoDto>))]
        public async Task<ActionResult<IEnumerable<GetTodoDto>>> GetAllTodoV2()
        {
            var todos = await _context.Todos.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<GetTodoDto>>(todos));
        }

        // �ϳ� ��ȸ (GET /api/todo/{id})
        [HttpGet("{id}")]
        public async Task<ActionResult<GetTodoDto>> GetByIdTodoV2(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return NotFound();
            return Ok(_mapper.Map<GetTodoDto>(todo));
        }
        
            
            // �߰� (POST /api/todo)
        [HttpPost]
        [SwaggerOperation(Summary = "���ο� Todo �߰�", Description = "Title�� �Է� �޾� ���ο� Todo�� �����մϴ�.")]
        [SwaggerResponse(201, "���� ����", typeof(GetTodoDto))]
        [SwaggerResponse(400, "�Է� �� ���� ����")]
        public async Task<ActionResult<GetTodoDto>> CreateTodoV2(CreateTodoDto createDto)
        {
            var todo = _mapper.Map<TodoItem>(createDto);
            _context.Todos.Add(todo);

            await _context.SaveChangesAsync();
            var getDto = _mapper.Map<GetTodoDto>(todo);
            return CreatedAtAction(nameof(GetByIdTodoV2), new { id = todo.Id }, getDto);
        }
        
        // ���� (PUT /api/todo/{id})
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodoV2(int id, UpdateTodoDto updateDto)
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
        public async Task<IActionResult> DeleteTodoV2(int id)
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
