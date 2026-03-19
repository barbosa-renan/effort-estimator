using EffortEstimator.API.Mappers;
using EffortEstimator.API.Models.Requests;
using EffortEstimator.API.Models.Responses;
using EffortEstimator.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EffortEstimator.API.Controllers;

[ApiController]
[Route("api/estimation")]
public class EstimationController : ControllerBase
{
    private readonly IEstimationEngine _estimationEngine;

    public EstimationController(IEstimationEngine estimationEngine) =>
        _estimationEngine = estimationEngine;

    [HttpPost]
    [ProducesResponseType(typeof(EstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Estimate([FromBody] EstimateRequest request)
    {
        var input  = EstimationMapper.ToEstimationInput(request);
        var result = _estimationEngine.Estimate(input);
        return Ok(EstimationMapper.ToEstimateResponse(result));
    }
}
