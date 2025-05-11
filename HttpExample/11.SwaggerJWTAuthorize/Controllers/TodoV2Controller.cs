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

        // 전체 조회 (GET /api/todo)
        [HttpGet]
        [SwaggerOperation(Summary = "전체 Todo 목록 조회", Description = "DB에 저장된 모든 Todo 항목을 조회합니다.")]
        [SwaggerResponse(200, "성공", typeof(IEnumerable<GetTodoDto>))]
        public async Task<ActionResult<IEnumerable<GetTodoDto>>> GetAllTodoV2()
        {
            var todos = await _context.Todos.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<GetTodoDto>>(todos));
        }

        // 하나 조회 (GET /api/todo/{id})
        [HttpGet("{id}")]
        public async Task<ActionResult<GetTodoDto>> GetByIdTodoV2(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return NotFound();
            return Ok(_mapper.Map<GetTodoDto>(todo));
        }
        
            
            // 추가 (POST /api/todo)
        [HttpPost]
        [SwaggerOperation(Summary = "새로운 Todo 추가", Description = "Title을 입력 받아 새로운 Todo를 생성합니다.")]
        [SwaggerResponse(201, "생성 성공", typeof(GetTodoDto))]
        [SwaggerResponse(400, "입력 값 검증 실패")]
        public async Task<ActionResult<GetTodoDto>> CreateTodoV2(CreateTodoDto createDto)
        {
            var todo = _mapper.Map<TodoItem>(createDto);
            _context.Todos.Add(todo);

            await _context.SaveChangesAsync();
            var getDto = _mapper.Map<GetTodoDto>(todo);
            return CreatedAtAction(nameof(GetByIdTodoV2), new { id = todo.Id }, getDto);
        }
        
        // 수정 (PUT /api/todo/{id})
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

        // 삭제 (DELETE /api/todo/{id})
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
