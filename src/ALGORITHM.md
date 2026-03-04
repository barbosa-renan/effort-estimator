# PERT Effort Estimation Algorithm

## Overview

The algorithm takes a structured task description (JSON) and returns an effort
estimate based on the **PERT technique** (Program Evaluation and Review
Technique). It produces hours, Story Points (Fibonacci), standard deviation,
confidence interval, and a risk level.

Input → 5 sequential adjustment stages → PERT formula → Output metrics

---

## Input Schema

```json
{
  "task_description":    "string (optional, for labeling)",
  "technical_complexity": "trivial | simple | moderate | complex | very_complex",
  "team_knowledge":       "expert | intermediate | beginner | unknown",
  "external_integrations": {
    "count":      "integer >= 0",
    "complexity": "low | medium | high"
  },
  "external_dependencies": {
    "count":            "integer >= 0",
    "team_reliability": "high | medium | low"
  }
}
```

---

## The Three Core Variables

Every stage operates on three values that flow through the entire pipeline:

| Variable | Meaning |
|----------|---------|
| **O** | Optimistic — best-case scenario |
| **M** | Most Likely — realistic day-to-day estimate |
| **P** | Pessimistic — worst-case scenario |

**Key design principle:** O, M and P are affected *asymmetrically* by risk
factors. Risks inflate P more than M, and M more than O, because in practice
problems compound while wins rarely stack.

---

## Stage 1 — Base Hours by Technical Complexity

Sets the initial O, M, P values. These are the anchor for all subsequent stages.

| Level        |   O  |   M  |    P |
|--------------|-----:|-----:|-----:|
| trivial      |  0.5 |    1 |    2 |
| simple       |  1   |    3 |    6 |
| moderate     |  3   |    8 |   16 |
| complex      |  8   |   20 |   40 |
| very_complex | 20   |   48 |  100 |

The O→P spread grows non-linearly (1.5h for trivial, 80h for very_complex)
to model the exponential uncertainty of larger tasks.

---

## Stage 2 — External Integrations Multiplier

Each external service (API, gateway, third-party) adds risk proportional to
its complexity.

```
complexityMultiplier:  low=1.1  |  medium=1.3  |  high=1.6
intMult = 1 + count × (complexityMultiplier - 1)

O *= 1 + (intMult - 1) × 0.5    ← absorbs half the risk
M *= intMult                      ← absorbs full risk
P *= intMult × 1.2               ← amplifies 20% extra
```

**Why asymmetric?** In the optimistic scenario you assume integrations work.
In the pessimistic scenario, APIs go down, docs are wrong, auth fails — risks
multiply. The 1.2x amplifier on P models this compounding effect.

---

## Stage 3 — Team Knowledge Multiplier

The single factor with the highest impact on P. Unknown domain knowledge is
the leading cause of variance in software projects.

| Level        |  ×O  |  ×M  |   ×P |
|--------------|-----:|-----:|-----:|
| expert       | 0.8  | 0.9  |  1.0 |
| intermediate | 1.0  | 1.0  |  1.2 |
| beginner     | 1.3  | 1.6  |  2.5 |
| unknown      | 1.2  | 1.5  |  2.8 |

**Why `unknown` > `beginner` on P?** Beginners are predictably slow. A team
of unknown skill level carries *maximum epistemic uncertainty* — their P must
reflect that we cannot even bound the downside.

---

## Stage 4 — External Dependencies Multiplier

Models the risk of being blocked by other teams or services. Unlike
integrations, this is a *blocking* risk, not a *complexity* risk.

```
reliabilityRisk:  high=0.05  |  medium=0.15  |  low=0.35
depPenalty = 1 + count × reliabilityRisk

M *= 1 + (depPenalty - 1) × 0.6    ← 60% of penalty
P *= depPenalty                      ← full penalty
O is unchanged                       ← optimistic assumes no blocking
```

O is deliberately untouched: in the best case, dependencies are ready on time.

---

## Stage 5 — PERT Formula and Derived Metrics

```
PERT  = (O + 4×M + P) / 6          ← weighted mean from Beta distribution
σ     = (P - O) / 6                 ← standard deviation
σ²    = σ²                          ← variance
CV    = σ / PERT                    ← coefficient of variation (risk signal)

Confidence interval (68%): PERT ± σ
```

The weight of **4 on M** is not arbitrary — it derives from the Beta
distribution, which models asymmetric, bounded processes like project delivery.

### Risk Level (from CV)

| CV range    | Risk Level |
|-------------|------------|
| CV < 0.30   | Baixo      |
| CV < 0.60   | Médio      |
| CV ≥ 0.60   | Alto       |

### Story Points (Fibonacci mapping)

| PERT hours  | Story Points |
|-------------|-------------|
| ≤ 2h        | 1 |
| ≤ 4h        | 2 |
| ≤ 8h        | 3 |
| ≤ 16h       | 5 |
| ≤ 28h       | 8 |
| ≤ 48h       | 13 |
| ≤ 80h       | 21 |
| ≤ 130h      | 34 |
| ≤ 200h      | 55 |
| > 200h      | 89 |

---

## Output Schema

```json
{
  "task_description":    "string",
  "optimistic":          "number (hours)",
  "most_likely":         "number (hours)",
  "pessimistic":         "number (hours)",
  "pert_hours":          "number",
  "standard_deviation":  "number",
  "variance":            "number",
  "story_points":        "integer (Fibonacci)",
  "confidence_range":    { "low": "number", "high": "number" },
  "risk_level":          "Baixo | Médio | Alto"
}
```

---

## Implementation Notes

- All multipliers are applied sequentially on the **same O, M, P** variables —
  effects compound, not average
- Round all outputs to 1 decimal place
- `unknown` team knowledge should always be available as a fallback
- When implementing in a typed language, model each stage as a pure function
  for testability
- The algorithm has no external dependencies — suitable for embedding in any
  service or CLI

---

## Calibration Guidance

The specific multiplier values in this skill are **design heuristics**, not
empirically measured constants. For production use, calibrate against your
team's historical data:

1. Run the estimator on past tasks with known actual hours
2. Compare `pert_hours` vs actual
3. Adjust stage multipliers until the mean absolute error is minimized
4. Re-calibrate every 3–6 months as the team evolves

For a more rigorous foundation, replace the multipliers with values from
**COCOMO II** (Boehm et al., 2000), which were measured across hundreds of
real projects.

---

## Worked Example

Input: `complex + intermediate + 2 high integrations + 1 medium dependency`

```
Stage 1 (base):         O=8     M=20     P=40
Stage 2 (integrations): O=14.4  M=44     P=105.6
Stage 3 (knowledge):    O=14.4  M=44     P=126.7
Stage 4 (dependencies): O=14.4  M=47.0   P=145.7

PERT  = (14.4 + 4×47.0 + 145.7) / 6 = 48.3h
σ     = (145.7 - 14.4) / 6          = 21.9h
CV    = 21.9 / 48.3                  = 0.45 → Médio
SP    = 13 (Fibonacci)
Range = 26.4h — 70.2h
```