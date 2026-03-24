using EffortEstimator.Core.Application.Dtos;

namespace EffortEstimator.Core.Application.Interfaces.Services;

public interface IEstimationEngine
{
    EstimationResult Estimate(EstimationInput input);
}
