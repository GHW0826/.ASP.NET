using _2.CRUDAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace _2.CRUDAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private static List<TodoItem> todos = new List<TodoItem>();
        private static int nextId = 1;

        private readonly ILogger<TodoController> _logger;

        public TodoController(ILogger<TodoController> logger)
        {
            _logger = logger;
        }

        // ��ü ��ȸ (GET /api/todo)
        [HttpGet]
        public ActionResult<IEnumerable<TodoItem>> GetAll()
        {
            return Ok(todos);
        }

        // �ϳ� ��ȸ (GET /api/todo/{id})
        [HttpGet("{id}")]
        public ActionResult<TodoItem> GetById(int id)
        {
            var todo = todos.FirstOrDefault(x => x.Id == id);
            if (todo == null)
                return NotFound();
            return Ok(todo);
        }

        // �߰� (POST /api/todo)
        [HttpPost]
        public ActionResult<TodoItem> Create(TodoItem item)
        {
            item.Id = nextId++;
            todos.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        // ���� (PUT /api/todo/{id})
        [HttpPut("{id}")]
        public IActionResult Update(int id, TodoItem updatedItem)
        {
            var todo = todos.FirstOrDefault(x => x.Id == id);
            if (todo == null)
                return NotFound();

            todo.Title = updatedItem.Title;
            todo.IsComplete = updatedItem.IsComplete;
            return Ok();
        }

        // ���� (DELETE /api/todo/{id})
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var todo = todos.FirstOrDefault(x => x.Id == id);
            if (todo == null)
                return NotFound();
            todos.Remove(todo);
            return Ok();
        }
    }
}
