using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NewBackEnd.Data;
using NewBackEnd.Hubs;
using NewBackEnd.Models;
using NewBackEnd.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewBackEnd.Controllers
{
    [Route("todo")]
    [ApiController]
    public class TodoController : Controller
    {
        private readonly IHubContext<UpdateClientsHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public TodoController(ApplicationDbContext context, IHubContext<UpdateClientsHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        [Route("getlist")]
        public IActionResult Index()
        {
            var todolist = _context.Todos.ToList();
            return Ok(todolist);
        }

        //
        [HttpPost]
        public async Task<IActionResult> Post(TodoDTO t)
        {
            if (ModelState.IsValid)
            {
                var todo = new Todo()
                {
                    Title = t.Title,
                    Completed = false
                };
                _context.Todos.Add(todo);
                await _context.SaveChangesAsync();

                var newtodo = _context.Todos.ToList();
                await SendMessage();
                return Ok(newtodo);
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var todo = await _context.Todos.FindAsync(id);
            if (todo == null)
                return NotFound();
            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync();
            await SendMessage();
            return Ok(id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id)
        {
            if (ModelState.IsValid)
            {
                    if (id == 0)
                        return BadRequest(new { message = "Invalid Todo" });
                    var todo = _context.Todos.Find(id);

                        if(todo.Completed == true)
                        {
                            todo.Completed = false;
                        }
                        else
                        {
                            todo.Completed = true;
                        }
                    _context.Update(todo);
                    await _context.SaveChangesAsync();
                await SendMessage();
                return Ok(todo);
            }
            return BadRequest();
        }
        private async Task SendMessage()
        {
            var list = _context.Todos.ToList();
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", list);
        }
    }
}
