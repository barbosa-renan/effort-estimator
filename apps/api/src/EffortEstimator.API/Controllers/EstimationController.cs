using EffortEstimator.API.Mappers;
using EffortEstimator.API.Models.Requests;
using EffortEstimator.API.Models.Responses;
using EffortEstimator.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EffortEstimator.API.Controllers;

/// <summary>
/// Provides PERT-based software effort estimation.
/// </summary>
[ApiController]
[Route("api/estimation")]
[Produces("application/json")]
[Tags("Estimation")]
public class EstimationController : ControllerBase
{
    private readonly IEstimationEngine _estimationEngine;

    public EstimationController(IEstimationEngine estimationEngine) =>
        _estimationEngine = estimationEngine;

    /// <summary>
    /// Estimates effort for a software task using the PERT formula.
    /// </summary>
    /// <remarks>
    /// Computes optimistic, most-likely, and pessimistic hour estimates based on
    /// technical complexity, team knowledge, external integrations, and external dependencies.
    /// Returns PERT hours, Story Points (Fibonacci scale), standard deviation,
    /// a 68% confidence interval, and a risk classification.
    /// </remarks>
    /// <param name="request">Task description and estimation parameters.</param>
    /// <returns>PERT estimation result with hours, Story Points, and risk level.</returns>
    /// <response code="200">Estimation computed successfully.</response>
    /// <response code="400">Request body failed validation (e.g., missing task description).</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(EstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult Estimate([FromBody] EstimateRequest request)
    {
        var input  = EstimationMapper.ToEstimationInput(request);
        var result = _estimationEngine.Estimate(input);
        return Ok(EstimationMapper.ToEstimateResponse(result));
    }
}
