using SwaggerJWTAuthorize.Data;
using SwaggerJWTAuthorize.Models;
using SwaggerJWTAuthorize.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace SwaggerJWTAuthorize.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("3.0")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/v{version:apiVersion}/[controller]")]
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
        [Authorize]

        [MapToApiVersion("1.0")]
        [SwaggerOperation(Summary = "��ü Todo ��� ��ȸ", Description = "DB�� ����� ��� Todo �׸��� ��ȸ�մϴ�.")]
        [SwaggerResponse(200, "����", typeof(IEnumerable<GetTodoDto>))]
        public async Task<ActionResult<IEnumerable<GetTodoDto>>> GetAllV1()
        {
            var todos = await _context.Todos.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<GetTodoDto>>(todos));
        }

        [HttpGet]
        [MapToApiVersion("3.0")]
        [ApiExplorerSettings(GroupName = "v3")]
        [SwaggerOperation(Summary = "��ü Todo ��� ��ȸ", Description = "DB�� ����� ��� Todo �׸��� ��ȸ�մϴ�.")]
        [SwaggerResponse(200, "����", typeof(IEnumerable<GetTodoDto>))]
        public async Task<ActionResult<IEnumerable<GetTodoDto>>> GetAllV2()
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
        [SwaggerOperation(Summary = "���ο� Todo �߰�", Description = "Title�� �Է� �޾� ���ο� Todo�� �����մϴ�.")]
        [SwaggerResponse(201, "���� ����", typeof(GetTodoDto))]
        [SwaggerResponse(400, "�Է� �� ���� ����")]
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
        [Authorize(Roles = "Admin")] // Role �����ڸ� ���� ����
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
