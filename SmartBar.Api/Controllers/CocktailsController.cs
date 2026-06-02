using MediatR;
using Microsoft.AspNetCore.Mvc;
using SmartBar.Application.Cocktails.Commands;
using SmartBar.Application.Cocktails.Queries;

namespace SmartBar.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CocktailsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CocktailsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateCocktailCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("order")]
    public async Task<IActionResult> Order([FromBody] OrderCocktailCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetCocktailsQuery());
        return Ok(result);
    }
}