# EffortEstimator

PERT-based software effort estimation **Web API** built with .NET 10.
Accepts a task description and returns estimated hours, Story Points (Fibonacci scale),
standard deviation, a 68% confidence interval, and a risk level.

---

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

---

## Running the API

```bash
git clone https://github.com/your-username/effort-estimator.git
cd effort-estimator
dotnet run --project src/EffortEstimator.API
```

Swagger UI (development only):

```
https://localhost:<port>/swagger
```

---

## API Endpoint

### `POST /api/estimation`

Estimates effort for a software task using the PERT formula.

#### Request body

```json
{
  "task_description": "Implement OAuth2 login flow with Google provider",
  "technical_complexity": "complex",
  "team_knowledge": "intermediate",
  "external_integrations": {
    "count": 2,
    "complexity": "high"
  },
  "external_dependencies": {
    "count": 1,
    "team_reliability": "medium"
  }
}
```

#### Response

```json
{
  "task_description": "Implement OAuth2 login flow with Google provider",
  "optimistic": 12.8,
  "most_likely": 48.0,
  "pessimistic": 145.7,
  "pert_hours": 58.4,
  "standard_deviation": 22.2,
  "variance": 492.8,
  "story_points": 21,
  "confidence_range": {
    "low": 36.2,
    "high": 80.6
  },
  "risk_level": "Medium"
}
```

---

## Input Schema

| Field | Type | Required | Accepted values |
|---|---|---|---|
| `task_description` | string | Yes (3–500 chars) | Any text |
| `technical_complexity` | string | No | `trivial` `simple` `moderate` `complex` `very_complex` |
| `team_knowledge` | string | No | `expert` `intermediate` `beginner` `unknown` |
| `external_integrations.count` | int | No | `0` or more (max 50) |
| `external_integrations.complexity` | string | No | `low` `medium` `high` |
| `external_dependencies.count` | int | No | `0` or more (max 50) |
| `external_dependencies.team_reliability` | string | No | `high` `medium` `low` |

---

## Project Structure

```
EffortEstimator/
├── src/
│   ├── EffortEstimator.Core/          # Domain + Application (zero external deps)
│   │   ├── Application/
│   │   │   ├── Dtos/                  # Input/output shapes for Core services
│   │   │   ├── Interfaces/Services/   # Port — IEstimationEngine
│   │   │   └── Services/              # PertEngine (core business logic)
│   │   └── Domain/
│   │       ├── Enums/                 # TechnicalComplexityLevel, RiskLevel, …
│   │       └── ValueObjects/          # ConfidenceRange
│   ├── EffortEstimator.Infrastructure/ # DI wiring for infrastructure adapters
│   └── EffortEstimator.API/           # Web API composition root
│       ├── Controllers/               # EstimationController
│       ├── Mappers/                   # EstimationMapper (API ↔ Core)
│       ├── Middleware/                # GlobalExceptionHandler
│       ├── Models/Requests/           # EstimateRequest
│       └── Models/Responses/          # EstimateResponse
└── tests/
    └── EffortEstimator.Core.Tests/    # PertEngine unit tests
```

---

## How the Algorithm Works

The estimate is computed in **5 sequential stages** operating on three variables:

- **O** — Optimistic: best-case scenario
- **M** — Most Likely: realistic estimate
- **P** — Pessimistic: worst-case scenario

Risk factors affect O, M, and P **asymmetrically** — risks inflate P more than M, and M more than O, reflecting how problems compound in real projects.

### Stage 1 — Base hours by technical complexity

| Level | O | M | P |
|---|---|---|---|
| `trivial` | 0.5h | 1h | 2h |
| `simple` | 1h | 3h | 6h |
| `moderate` | 3h | 8h | 16h |
| `complex` | 8h | 20h | 40h |
| `very_complex` | 20h | 48h | 100h |

### Stage 2 — External integrations

```
intMult = 1 + count × (complexityMult - 1)
  where: low=1.1  |  medium=1.3  |  high=1.6

O *= 1 + (intMult - 1) × 0.5   ← absorbs half the risk
M *= intMult
P *= intMult × 1.2              ← amplifies 20% extra
```

### Stage 3 — Team knowledge

| Level | ×O | ×M | ×P |
|---|---|---|---|
| `expert` | 0.8 | 0.9 | 1.0 |
| `intermediate` | 1.0 | 1.0 | 1.2 |
| `beginner` | 1.3 | 1.6 | 2.5 |
| `unknown` | 1.2 | 1.5 | 2.8 |

### Stage 4 — External dependencies

```
depPenalty = 1 + count × reliabilityRisk
  where: high=0.05  |  medium=0.15  |  low=0.35

M *= 1 + (depPenalty - 1) × 0.6    ← 60% of penalty
P *= depPenalty                      ← full penalty
O is unchanged                       ← optimistic assumes no blocking
```

### Stage 5 — PERT formula and metrics

```
PERT  = (O + 4×M + P) / 6
σ     = (P - O) / 6
σ²    = σ²
CV    = σ / PERT

Confidence interval (68%) = PERT ± σ
```

**Risk level by coefficient of variation (CV):**

| CV | Risk |
|---|---|
| < 0.30 | Low |
| < 0.60 | Medium |
| ≥ 0.60 | High |

**Story Points (Fibonacci mapping):**

| PERT hours | Story Points |
|---|---|
| ≤ 2h | 1 |
| ≤ 4h | 2 |
| ≤ 8h | 3 |
| ≤ 16h | 5 |
| ≤ 28h | 8 |
| ≤ 48h | 13 |
| ≤ 80h | 21 |
| ≤ 130h | 34 |
| ≤ 200h | 55 |
| > 200h | 89 |

---

## References

- Malcolm et al. (1959) — *Application of a Technique for Research and Development Program Evaluation* — Operations Research, Vol. 7
- PMI — *PMBOK Guide*
- Cohn, Mike (2005) — *Agile Estimating and Planning* — Prentice Hall
- Boehm, B. et al. (2000) — *Software Cost Estimation with COCOMO II* — Prentice Hall

---

## Calibration

The algorithm multipliers are **design heuristics**, not empirically measured values. For production use, calibrate against your team's historical data:

1. Run the estimator retrospectively on past tasks with known actual hours
2. Compare `pert_hours` with actual time recorded
3. Adjust the multiplier with the highest error
4. Re-calibrate every 3–6 months as the team evolves

---

## License

MIT