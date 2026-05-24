using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartBar.Application.Ingredients.Commands;
using SmartBar.Application.Ingredients.Queries;

namespace SmartBar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientsController : ControllerBase
    {
        private readonly ISender _mediator;

        public IngredientsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateIngredientCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetIngredientsQuery());
            return Ok(result);
        }
    }
}
