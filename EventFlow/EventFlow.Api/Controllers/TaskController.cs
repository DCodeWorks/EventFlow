using EventFlow.Domain.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskController(IMediator mediator)
        {
            _mediator = mediator;
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
    }
}
