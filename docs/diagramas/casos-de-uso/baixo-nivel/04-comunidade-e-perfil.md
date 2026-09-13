# 04. Comunidade, pauta e perfil — baixo nível

Detalha [03 — Explorador social](../03-explorador-social.md).

## Explorar a comunidade

```mermaid
flowchart LR
    EXP(("👤 Explorador"))

    EXP --- COM(["Explorar a comunidade 🟡"])

    COM -.->|include| LISTA[/"Carregar os exploradores 🟡"/]
    LISTA -.->|include| CONTA[/"Ler os contadores de cada um 🟡"/]
    CONTA -.->|include| REGUA[/"Aplicar isVerifiedExplorer aos contadores 🟡"/]
    REGUA -.->|include| SELO[/"Marcar quem passou na régua 🟡"/]
    CONTA -.->|include| TITULO[/"Derivar o título da contagem de conquistas 🟡"/]

    COM -.->|extend| BUSCAR(["Buscar por nome 🟡"])
    COM -.->|extend| RANK(["Ver o ranking 🟡"])
    RANK -.->|include| FIXA[/"Fixar a linha do próprio usuário 🟡"/]
    RANK -.->|extend| BAIRRO(["Filtrar por bairro 🟡"])
    COM -.->|extend| PERFILP(["Abrir o perfil público 🟡"])
    COM -.->|extend| CRITERIO(["Ler a régua do selo 🟡"])
```

**A régua é aplicada, não consultada.** `isVerifiedExplorer` é função pura sobre
quatro contadores — ≥5 visitas, ≥3 avaliações publicadas, 0 removidas e ≥1 mês
de conta. Não há coluna de selo em lugar nenhum, então mudar o critério é mexer
em `src/constants/verification.ts` e nada migra junto.

**`Fixar a linha do próprio usuário` existe para o ranking não ser só vitrine:**
placar em que a pessoa não se encontra não diz o que fazer a seguir.

## Gerar rascunho de pauta

```mermaid
flowchart LR
    EXP(("👤 Explorador"))
    ADM(("👤 Administrador"))
    GEMINI[["🖥️ Gemini API"]]

    EXP --- GERAR(["Gerar rascunho de pauta ✅"])
    ADM -.->|"passo ausente: verificar papel 🔴"| GERAR

    GERAR -.->|include| TEMA(["Informar o tema ✅"])
    GERAR -.->|include| MARCA(["Marcar os lugares da pauta ✅"])
    MARCA -.->|include| VAL[/"Validar tema e ids com Zod ✅"/]
    VAL -.->|include| FICHA[/"Montar a ficha de fatos de cada lugar 🟡"/]
    FICHA --> DEC{"Todo id virou ficha?"}
    DEC -->|não| ABORTA[/"Recusar antes de chamar o modelo ✅"/]
    DEC -->|sim| CHAVE{"GEMINI_API_KEY existe?"}
    CHAVE -->|não| DESLIGADO[/"Avisar que a geração está desligada ✅"/]
    CHAVE -->|sim| CHAMA[/"Chamar o modelo com system, prompt e schema ✅"/]
    CHAMA -.->|include| TETO[/"Abortar em 30 segundos ✅"/]
    CHAMA -.->|include| REVAL[/"Revalidar a saída com storyDraftSchema ✅"/]
    REVAL -.->|extend| MALF(["Ver aviso de formato inesperado ✅"])
    REVAL -.->|include| DEVOLVE[/"Devolver o rascunho e o nome do modelo ✅"/]
    DEVOLVE -.->|include| REVISAR(["Revisar e publicar à mão 🔴"])

    CHAMA --- GEMINI
```

**Três defesas, nesta ordem: `Montar a ficha`, a instrução de sistema dentro de
`Chamar o modelo`, e `Revalidar a saída`.** A ordem importa — limitar a entrada,
instruir a conduta, validar a saída. A terceira existe porque as duas primeiras
são pedidos, não garantias: `responseSchema` garante forma, não conteúdo.

**`GEMINI_API_KEY existe?` é decisão, não exceção.** Sem chave a tela funciona e
o gerador avisa que está desligado — ambiente sem a variável é estado esperado.

**`Revisar e publicar à mão` é 🔴** porque não há entidade de pauta: o rascunho
volta para a tela e é copiado à mão. Não existe passo de gravar.

## Ver o próprio perfil

```mermaid
flowchart LR
    EXP(("👤 Explorador"))

    EXP --- HUB(["Ver o próprio perfil 🟡"])

    HUB -.->|include| SESSAO[/"Ler nome e e-mail do cookie ✅"/]
    HUB -.->|include| BASE[/"Derivar os números das 24 visitas 🟡"/]
    HUB -.->|include| ABAS[/"Montar as cinco abas de PROFILE_TABS ✅"/]

    ABAS -.->|extend| VISAO(["Abrir a visão geral 🟡"])
    VISAO -.->|include| CHECK[/"Listar a régua item a item 🟡"/]
    CHECK -.->|include| FALTA[/"Calcular o que falta com missingForVerification 🟡"/]

    ABAS -.->|extend| VISITAS(["Abrir visitas 🟡"])
    VISITAS -.->|include| MES[/"Agrupar por mês em UTC 🟡"/]

    ABAS -.->|extend| FAV(["Abrir favoritos 🟡"])
    ABAS -.->|extend| CONQ(["Abrir conquistas 🟡"])
    CONQ -.->|include| PROG[/"Calcular o progresso da conquista travada 🟡"/]
    ABAS -.->|extend| STATS(["Abrir estatísticas 🟡"])
    STATS -.->|include| COBERTURA[/"Calcular a cobertura da cidade 🟡"/]
```

**Cada aba é URL própria, não `useState`.** Conteúdo que se lê, se compartilha e
ao qual se volta precisa de rota — e assim as cinco continuam Server Component,
com `"use client"` só na navegação, que precisa do `usePathname`.

**`Derivar os números das 24 visitas` é o passo que impede divergência.** O
progresso de "visitar 5 cafeterias" conta cafeterias na lista; não é número
digitado. O perfil privado e o público leem a mesma base e não têm como
discordar (`RNF-DAD-13`).

**`Agrupar por mês em UTC` não é preciosismo.** Um dia puro como `2026-08-17` é
meia-noite UTC: formatado no fuso do Brasil vira 16 de agosto, e servidor e
navegador renderizam datas diferentes — hidratação quebrada.

---

## Descrições dos casos

As descrições em template expandido — ator, pré e pós-condição, ações do
ator × ações do sistema numeradas e restrições — vivem em
[`descricoes/04-comunidade-e-perfil.md`](../descricoes/04-comunidade-e-perfil.md): Buscar exploradores, Gerar rascunho de pauta e Ver o próprio perfil.

---

➡️ [05 — Administração](./05-administracao.md)
