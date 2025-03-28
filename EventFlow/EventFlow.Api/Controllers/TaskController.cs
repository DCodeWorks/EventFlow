using EventFlow.Domain.Commands;
using EventFlow.Domain.Queries;
using EventFlow.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly EventFlowDbContext _readDbContext;

        public TaskController(IMediator mediator, EventFlowDbContext readDbContext)
        {
            _mediator = mediator;
            _readDbContext = readDbContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command)
        {
            var result = await _mediator.Send(command);
            if (result)
                return Ok("Task created successfully.");
            return BadRequest("Error creating task.");
        }

        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateTaskCommand command)
        {
            if (taskId != command.TaskId)
                return BadRequest("Task ID mismatch.");

            var result = await _mediator.Send(command);
            if (result)
                return Ok("Task updated successfully.");
            return BadRequest("Error updating task.");
        }

        [HttpPost("{taskId}/complete")]
        public async Task<IActionResult> CompleteTask(Guid taskId)
        {
            var command = new CompleteTaskCommand(taskId);
            var result = await _mediator.Send(command);
            if (result)
                return Ok("Task completed successfully.");
            return BadRequest("Error completing task.");
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTask(Guid taskId)
        {
            var command = new GetTaskQuery(taskId);

            var result = await _mediator.Send(command);

            if (result != null)
                return Ok(result);
            return NotFound("Task Not Found!");
        }

        [HttpGet("norehydration/{taskId}")]
        public async Task<IActionResult> GetTaskNoRehydration(Guid taskId)
        {
            var task = await _readDbContext.TaskReadModels.FirstOrDefaultAsync(t => t.TaskId == taskId);
            if (task == null)
                return NotFound("Task not found.");
            return Ok(task);
        }
    }
}
