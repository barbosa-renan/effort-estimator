# PERT Estimator — Design Decisions & References

Este documento explica **por que** o algoritmo foi construído da forma que foi,
diferenciando o que tem base em literatura estabelecida do que são decisões de
design próprias. Transparência sobre essas fronteiras é essencial para quem
quiser usar, ajustar ou evoluir o estimador em produção.

---

## O que tem base sólida na literatura

### A fórmula PERT

A fórmula central `(O + 4M + P) / 6` e o desvio padrão `(P - O) / 6` são
matemática estabelecida, não opinião.

Foram desenvolvidos pela Marinha americana em 1958 no programa Polaris e
publicados formalmente em:

> Malcolm, D.G., Roseboom, J.H., Clark, C.E., Fazar, W. (1959).
> *Application of a Technique for Research and Development Program Evaluation.*
> Operations Research, Vol. 7, No. 5, pp. 646–669.

O peso **4 no valor mais provável** deriva da **distribuição Beta**, que modela
processos assimétricos e limitados — exatamente o perfil de duração de tarefas
de software. O PERT é hoje referenciado como técnica padrão no:

> Project Management Institute — *PMBOK Guide* (todas as edições)

### Story Points em escala Fibonacci

O uso da sequência de Fibonacci para estimativas relativas é documentado e
justificado em:

> Cohn, Mike (2005). *Agile Estimating and Planning.* Prentice Hall.

A justificativa é que o espaçamento crescente da sequência (1, 2, 3, 5, 8, 13…)
reflete a incerteza proporcional em tarefas maiores. Não faz sentido distinguir
entre 11 e 12 story points — mas faz sentido distinguir entre 8 e 13.

---

## O que são decisões de design (heurísticas)

Esta seção descreve os valores e escolhas que **não** foram extraídos de
literatura, mas foram definidos com lógica interna coerente. São o ponto de
partida mais honesto possível — mas devem ser calibrados com dados reais do
time.

### Valores base por complexidade técnica

```
trivial      → O:0.5h   M:1h    P:2h
simple       → O:1h     M:3h    P:6h
moderate     → O:3h     M:8h    P:16h
complex      → O:8h     M:20h   P:40h
very_complex → O:20h    M:48h   P:100h
```

Esses valores foram definidos com base em intuição sobre o que "parece razoável"
para um time de desenvolvimento médio. A progressão não-linear do spread O→P
(1.5h no trivial, 80h no very_complex) é intencional: quanto mais complexa a
tarefa, mais aberta é a janela de incerteza.

**Como calibrar:** registre as estimativas O/M/P de tarefas passadas e compare
com o tempo real. Ajuste os valores base até minimizar o erro médio absoluto.

### Multiplicadores de integrações externas

```
low    → 1.1x
medium → 1.3x
high   → 1.6x
```

E a **assimetria entre O, M e P**:

```
O *= 1 + (intMult - 1) × 0.5   ← metade do risco
M *= intMult
P *= intMult × 1.2              ← 20% a mais
```

A assimetria foi uma escolha deliberada baseada na observação de que no cenário
otimista você assume que as integrações funcionam. No pessimista, APIs caem,
documentações estão erradas, autenticações expiram — os problemas se multiplicam.
O fator 1.2x extra no P modela esse comportamento composto.

Os valores 1.1 / 1.3 / 1.6 são razoáveis mas arbitrários. Uma integração de
alta complexidade com um parceiro pouco confiável pode facilmente ser 2.5x ou
mais — depende do contexto.

### Multiplicadores de conhecimento do time

```
expert       → ×O:0.8  ×M:0.9  ×P:1.0
intermediate → ×O:1.0  ×M:1.0  ×P:1.2
beginner     → ×O:1.3  ×M:1.6  ×P:2.5
unknown      → ×O:1.2  ×M:1.5  ×P:2.8
```

O ponto mais importante aqui é que `unknown` tem o P mais alto — maior até que
`beginner`. Isso foi intencional: um time de iniciantes é *previsivelmente* lento.
Um time de nível desconhecido carrega **incerteza epistêmica máxima** — não
sabemos nem como limitar o pior caso.

O multiplicador de P para `beginner` (2.5x) e `unknown` (2.8x) são os que mais
carecem de calibração. Em times muito jovens, 3x ou 4x pode ser mais realista.

### Fatores de risco por dependência externa

```
high   → 0.05 por dependência
medium → 0.15 por dependência
low    → 0.35 por dependência
```

E a escolha de **não afetar O**: no melhor cenário, as dependências chegam a
tempo. O risco de dependências é modelado como risco de *bloqueio*, não de
*complexidade* — por isso afeta principalmente M e P.

Os valores 0.05 / 0.15 / 0.35 foram estimados com base no raciocínio de que
uma dependência com time não-confiável (`low`) deve adicionar 35% de risco por
item — o que rapidamente torna o pessimista proibitivo com 3+ dependências.
Isso foi intencional: 3 dependências não-confiáveis devem disparar um alerta
claro no estimador.

### Limiares de risco pelo coeficiente de variação

```
CV < 0.30  → Baixo
CV < 0.60  → Médio
CV ≥ 0.60  → Alto
```

O coeficiente de variação (σ / PERT) normaliza o desvio em relação à estimativa
central, permitindo comparar risco entre tasks de tamanhos diferentes. Os limiares
0.3 e 0.6 são convencionais na análise de risco estatística mas não têm uma
referência canônica para software especificamente.

---

## O que existe na literatura para embasar melhor

Se você quiser substituir as heurísticas por valores com base empírica, as
referências mais relevantes são:

### COCOMO II

> Boehm, B., et al. (2000). *Software Cost Estimation with COCOMO II.*
> Prentice Hall.

O COCOMO II define **multiplicadores de esforço** medidos em centenas de projetos
reais. Os fatores mais diretamente equivalentes aos usados neste estimador são:

| Fator COCOMO II | Equivalente aqui |
|-----------------|-----------------|
| `APEX` — experiência da equipe na aplicação | `team_knowledge` |
| `PLEX` — experiência com a plataforma | `team_knowledge` |
| `LTEX` — experiência com linguagem e ferramentas | `team_knowledge` |
| `RELY` — confiabilidade requerida | `technical_complexity` |
| Interfaces externas | `external_integrations` |

Esses multiplicadores variam tipicamente entre 0.73 e 1.74 por fator, calibrados
com dados reais. Substituir os multiplicadores deste estimador pelos valores do
COCOMO II tornaria o modelo consideravelmente mais defensável em contextos formais.

### Function Point Analysis (IFPUG)

> IFPUG — *Function Point Counting Practices Manual.*

Define formalmente o impacto de interfaces externas (External Interface Files,
External Inputs/Outputs) no esforço de desenvolvimento. Mais rigoroso para
projetos onde o escopo é bem definido antes de iniciar.

---

## Recomendações para uso em produção

O algoritmo, como está, é um **bom ponto de partida** — mas os multiplicadores
precisam de calibração para refletir a realidade do seu time específico.

**Passo 1:** rode o estimador retrospectivamente em 10–20 tarefas passadas com
tempo real conhecido.

**Passo 2:** calcule o erro médio absoluto entre `pert_hours` e o tempo real.

**Passo 3:** identifique qual fator está mais distorcido (geralmente
`team_knowledge` ou `external_integrations`) e ajuste os valores base ou
multiplicadores naquele fator.

**Passo 4:** re-calibre a cada 3–6 meses — o perfil de risco de um time muda
com o tempo.

Nenhum modelo externo substitui dados históricos do próprio time. A estrutura
do algoritmo (a sequência de fatores, a assimetria O/M/P, o mapeamento Fibonacci)
está conceitualmente correta. Os números específicos são onde mora a
subjetividade — e é exatamente aí que os dados do seu time fazem a diferença.