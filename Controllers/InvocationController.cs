using DaprAcaStarter.Models.Requests;
using DaprAcaStarter.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DaprAcaStarter.Controllers;

[ApiController]
[Route("")]
public sealed class InvocationController(IInvocationService invocationService) : ControllerBase
{
    private readonly IInvocationService _invocationService = invocationService;

    [HttpPost("internal/echo")]
    public IActionResult Echo([FromBody] InvocationRequest request)
    {
        return Ok(_invocationService.Echo(request));
    }

    [HttpPost("invoke/self")]
    public async Task<IActionResult> InvokeSelf([FromBody] InvocationRequest request, CancellationToken cancellationToken)
    {
        var response = await _invocationService.InvokeSelfAsync(request, cancellationToken);
        return Ok(response);
    }
}
