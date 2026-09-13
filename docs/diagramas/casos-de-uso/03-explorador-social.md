# 03. Explorador — feed, comunidade, pauta e perfil

## Feed · `/feed`

```mermaid
flowchart LR
    EXP(("👤 Explorador"))

    EXP --- LER(["Ler o feed 🟡"])

    LER -.->|include| MOTIVO(["Ver por que o item apareceu 🟡"])
    LER -.->|extend| FONTE(["Filtrar por fonte 🟡"])
    LER -.->|extend| ORDEM(["Ordenar por relevância ou data 🟡"])
    LER -.->|extend| UTIL(["Marcar avaliação como útil 🟡"])
    LER -.->|extend| SALVAR(["Acompanhar lugar 🟡"])
    LER -.->|extend| MENOS(["Ver menos disso 🟡"])
    LER -.->|extend| PAUTA(["Abrir a pauta do card de curadoria 🟡"])

    MENOS -.->|extend| SILENCIA(["Silenciar bairro, categoria ou tipo 🟡"])
    SILENCIA -.->|extend| DESFAZ(["Remover a regra pelo chip 🟡"])
```

`Ver por que o item apareceu` é `include`, não `extend`: `FeedCardFrame` exige
`reason`, então card sem motivo não compila. Os cinco motivos são `perto`,
`salvo`, `categoria`, `cidade` e `curadoria`.

Não há `Seguir usuário` — decisão de 2026-08-17, detalhada em
[`adr/user/0002-feed-sem-grafo-social.md`](../../adr/user/0002-feed-sem-grafo-social.md).
"Útil" mede se a dica ajudou a decidir; "curtir" mediria simpatia pelo autor,
que é o eixo que este feed não tem.

---

## Comunidade e pauta · `/community`, `/pautas/[slug]`

```mermaid
flowchart LR
    EXP(("👤 Explorador"))
    ADM(("👤 Administrador"))
    GEMINI[["🖥️ Gemini API"]]

    EXP --- COM(["Explorar a comunidade 🟡"])
    EXP --- LERP(["Ler uma pauta 🟡"])

    COM -.->|extend| BUSCAR(["Buscar exploradores 🟡"])
    COM -.->|extend| PERFILP(["Ver perfil público 🟡"])
    COM -.->|extend| RANK(["Ver ranking de contribuição 🟡"])
    COM -.->|extend| SELO(["Consultar a régua do selo 🟡"])
    RANK -.->|extend| BAIRRO(["Filtrar ranking por bairro 🟡"])

    EXP --- GERAR(["Gerar rascunho de pauta ✅"])
    ADM -.->|"deveria ser exclusivo — sem gate"| GERAR
    GERAR -.->|include| FICHA(["Enviar lista fechada de lugares 🟡"])
    GERAR -.->|include| REVAL(["Revalidar a saída com Zod ✅"])
    GERAR -.->|extend| REVISAR(["Revisar e publicar à mão 🟡"])

    GERAR --- GEMINI
    LERP -.->|include| ROTULO(["Ver o rótulo de origem IA 🟡"])
```

O gerador fica no cabeçalho da vitrine de pautas, aberto a **qualquer
sessão** — o mesmo buraco de `/places/[id]`, e ele queima cota de um modelo
pago. Entra no gate junto com o papel de administrador.

A chamada ao Gemini é **o único caminho de verdade** desta área — o resto vem
de `src/mocks/{community,stories}.ts`. Três regras que o diagrama carrega:

- **Geração é etapa de autoria, não de render.** Nada roda durante a navegação
  do leitor: alguém pede o rascunho, o modelo escreve, um humano publica.
- **`Revalidar a saída com Zod` é `include`.** `responseSchema` garante forma,
  não conteúdo — saída de LLM é entrada não confiável.
- **Rascunho responde 404** na rota pública, e pauta de origem `ia` aparece
  rotulada ao leitor.

O selo de verificado é régua pública, não chancela: ≥5 visitas, ≥3 avaliações
publicadas, nenhuma removida e ≥1 mês de conta
(`src/constants/verification.ts`).

---

## Perfil · `/profile`

```mermaid
flowchart LR
    EXP(("👤 Explorador"))

    EXP --- HUB(["Ver o próprio perfil 🟡"])

    HUB -.->|include| IDENT(["Ler identidade da sessão ✅"])
    HUB -.->|extend| VISAO(["Visão geral com a régua do selo 🟡"])
    HUB -.->|extend| VISITAS(["Ver histórico de visitas 🟡"])
    HUB -.->|extend| FAV(["Ver lugares salvos 🟡"])
    HUB -.->|extend| CONQ(["Ver galeria de conquistas 🟡"])
    HUB -.->|extend| STATS(["Ver placar e cobertura da cidade 🟡"])

    VISAO -.->|include| FALTA(["Ver o que falta para o selo 🟡"])
    CONQ -.->|include| PROG(["Ver progresso da conquista travada 🟡"])
```

As cinco abas são **segmento de rota**, não `useState` — decisão de
2026-08-19: conteúdo que se lê, se compartilha e ao qual se volta precisa de
URL. `/visits`, `/favorites`, `/stats` e `/achievements` viraram `redirect()`.

`Ler identidade da sessão` é o único ✅ da tela; o resto deriva de
`src/mocks/profile.ts` sobre as mesmas 24 visitas de `currentExplorerMock`.

**Fora do diagrama porque não existe:** editar dados do perfil (é `/settings`,
travado pelo `PUT` que re-hasheia a senha) e remover favorito (nasce com a
tabela `Favorita`).

---

➡️ [04 — Administrador](./04-administrador.md)
