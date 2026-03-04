# EffortEstimator

Algoritmo de estimativa de esforço para tarefas de desenvolvimento de software baseado na técnica **PERT** (Program Evaluation and Review Technique). Recebe uma descrição de tarefa em JSON e retorna horas estimadas, Story Points em escala Fibonacci, desvio padrão, intervalo de confiança e nível de risco.

---

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

---

## Instalação

```bash
git clone https://github.com/seu-usuario/EffortEstimator.git
cd EffortEstimator
dotnet build
```

---

## Uso

### Executar com o exemplo embutido

```bash
dotnet run
```

### Passar um JSON via stdin

```bash
dotnet run -- --stdin < task.json
```

### Exemplo de `task.json`

```json
{
  "task_description": "Integração com gateway de pagamento",
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

### Saída esperada

```
==============================================
       PERT ESTIMATOR - RESULTADO
==============================================

  TASK:              Integração com gateway de pagamento
  Otimista (O):        14.4 h
  Mais Provável (M):   47.0 h
  Pessimista (P):     145.7 h

  PERT = (O + 4M + P) / 6 =   48.3 h

  Story Points:        13 (Fibonacci)
  Desvio Padrão:       21.9 h
  Intervalo 68%:       26.4 h - 70.2 h
  Risco:               Médio
```

---

## Schema de Input

| Campo | Tipo | Obrigatório | Valores aceitos |
|---|---|---|---|
| `task_description` | string | Não | Qualquer texto |
| `technical_complexity` | string | Sim | `trivial` `simple` `moderate` `complex` `very_complex` |
| `team_knowledge` | string | Sim | `expert` `intermediate` `beginner` `unknown` |
| `external_integrations.count` | int | Não | `0` ou mais |
| `external_integrations.complexity` | string | Não | `low` `medium` `high` |
| `external_dependencies.count` | int | Não | `0` ou mais |
| `external_dependencies.team_reliability` | string | Não | `high` `medium` `low` |

---

## Como o Algoritmo Funciona

A estimativa é calculada em **5 etapas sequenciais**. Cada etapa ajusta três variáveis que percorrem o pipeline inteiro:

- **O** — Otimista: melhor cenário possível
- **M** — Mais Provável: estimativa realista
- **P** — Pessimista: pior cenário

Os fatores de risco afetam O, M e P de forma **assimétrica**: riscos inflam P mais do que M, e M mais do que O — refletindo como problemas se acumulam em projetos reais.

### Etapa 1 — Base pela complexidade técnica

Define os valores iniciais de O, M e P:

| Nível | O | M | P |
|---|---|---|---|
| `trivial` | 0.5h | 1h | 2h |
| `simple` | 1h | 3h | 6h |
| `moderate` | 3h | 8h | 16h |
| `complex` | 8h | 20h | 40h |
| `very_complex` | 20h | 48h | 100h |

### Etapa 2 — Integrações externas

Cada serviço externo (API, gateway, terceiros) adiciona risco multiplicativo:

```
intMult = 1 + count × (complexityMult - 1)
  onde: low=1.1  |  medium=1.3  |  high=1.6

O *= 1 + (intMult - 1) × 0.5   ← absorve metade do risco
M *= intMult
P *= intMult × 1.2              ← amplifica 20% a mais
```

### Etapa 3 — Conhecimento do time

| Nível | ×O | ×M | ×P |
|---|---|---|---|
| `expert` | 0.8 | 0.9 | 1.0 |
| `intermediate` | 1.0 | 1.0 | 1.2 |
| `beginner` | 1.3 | 1.6 | 2.5 |
| `unknown` | 1.2 | 1.5 | 2.8 |

> `unknown` tem P maior que `beginner` porque a incerteza epistêmica — não saber nem o nível do time — é o pior cenário possível.

### Etapa 4 — Dependências externas

Modela risco de bloqueio por outros times ou serviços. O não é afetado (no cenário otimista, dependências chegam a tempo):

```
depPenalty = 1 + count × reliabilityRisk
  onde: high=0.05  |  medium=0.15  |  low=0.35

M *= 1 + (depPenalty - 1) × 0.6
P *= depPenalty
```

### Etapa 5 — Fórmula PERT e métricas

```
PERT  = (O + 4×M + P) / 6
σ     = (P - O) / 6
σ²    = σ²
CV    = σ / PERT

Intervalo 68% = PERT ± σ
```

O peso 4 no valor mais provável deriva da **distribuição Beta**, adequada para modelar processos assimétricos como duração de tarefas de software.

**Nível de risco pelo coeficiente de variação (CV):**

| CV | Risco |
|---|---|
| < 0.30 | Baixo |
| < 0.60 | Médio |
| ≥ 0.60 | Alto |

**Mapeamento para Story Points (Fibonacci):**

| Horas PERT | Story Points |
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

## Estrutura do Projeto

```
EffortEstimator/
├── PertEstimator.cs     # Implementação completa (modelos + engine + CLI)
├── README.md            # Este arquivo
└── DECISIONS.md         # Justificativas das decisões do algorítmo e por que as coisas são como são
└── ALGORITHM.md         # Explicação de como o algorítmo funciona
```

---

## Referências

- Malcolm et al. (1959) — *Application of a Technique for Research and Development Program Evaluation* — Operations Research, Vol. 7
- PMI — *PMBOK Guide*
- Cohn, Mike (2005) — *Agile Estimating and Planning* — Prentice Hall
- Boehm, B. et al. (2000) — *Software Cost Estimation with COCOMO II* — Prentice Hall

---

## Calibração

Os multiplicadores do algoritmo são **heurísticas de design**, não valores medidos empiricamente. Para uso em produção, calibre com dados históricos do seu time:

1. Execute o estimador retroativamente em tarefas passadas com tempo real conhecido
2. Compare `pert_hours` com o tempo real registrado
3. Ajuste os multiplicadores no fator com maior erro
4. Recalibre a cada 3–6 meses conforme o time evolui

Para uma base mais rigorosa, os multiplicadores podem ser substituídos pelos valores do **COCOMO II**, que foram medidos em centenas de projetos reais.

---

## Licença

MIT