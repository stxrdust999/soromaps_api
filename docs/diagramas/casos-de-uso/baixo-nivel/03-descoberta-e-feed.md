# 03. Descoberta e feed — baixo nível

Detalha a seção Descobrir de [02 — Explorador](../02-explorador.md) e a seção
Feed de [03 — Explorador social](../03-explorador-social.md).

Todo caso desta página roda sobre `src/mocks/markers.ts` e `src/mocks/feed.ts`.

## Descobrir lugares

```mermaid
flowchart LR
    EXP(("👤 Explorador"))

    EXP --- DESC(["Descobrir lugares 🟡"])

    DESC -.->|include| CARREGA[/"Carregar a lista de lugares 🟡"/]
    CARREGA -.->|include| TRILHAS[/"Montar as três trilhas 🟡"/]
    TRILHAS -.->|include| PERTO[/"Ordenar por proximidade 🟡"/]
    TRILHAS -.->|include| NOTA[/"Ordenar por nota 🟡"/]
    TRILHAS -.->|include| RECENTE[/"Ordenar por data de cadastro 🟡"/]

    DESC -.->|extend| BUSCA(["Digitar na busca 🟡"])
    DESC -.->|extend| CHIPCAT(["Marcar chip de categoria 🟡"])
    DESC -.->|extend| CHIPVIBE(["Marcar chip de vibe 🟡"])

    BUSCA -.->|include| COMBINA[/"Combinar busca, categoria e vibe 🟡"/]
    CHIPCAT -.->|include| COMBINA
    CHIPVIBE -.->|include| COMBINA
    COMBINA --> DEC{"Sobrou algum lugar?"}
    DEC -->|sim| LISTA[/"Renderizar os cards 🟡"/]
    DEC -->|não| VAZIO[/"Mostrar o estado vazio com o que remover 🟡"/]

    LISTA -.->|extend| ABRIR(["Abrir a página do ponto ✅"])
```

**Os três critérios combinam, não se substituem** — é a razão de as trilhas
serem seção da mesma tela e não rotas diferentes: critério é seção, não rota.

**`Ordenar por proximidade` é 🟡 duas vezes:** a nota vem do mock *e* a
proximidade também, porque a posição real do explorador não é persistida
(`RF-MAP-04`).

## Ler o feed

```mermaid
flowchart LR
    EXP(("👤 Explorador"))

    EXP --- LER(["Ler o feed 🟡"])

    LER -.->|include| FONTE[/"Reunir itens das cinco fontes 🟡"/]
    FONTE -.->|include| AGREGA[/"Agregar rajada em um item só 🟡"/]
    FONTE -.->|include| SILENCIO[/"Descartar o que casa com regra de silêncio 🟡"/]
    SILENCIO -.->|include| ORDENA[/"Ordenar por relevância ou por data 🟡"/]
    ORDENA -.->|include| DESPACHA[/"Despachar cada item pelo kind 🟡"/]
    DESPACHA -.->|include| MOLDURA[/"Renderizar a moldura com o motivo 🟡"/]

    LER -.->|extend| CHIPS(["Filtrar por fonte 🟡"])
    LER -.->|extend| TROCA(["Trocar a ordenação 🟡"])
    LER -.->|extend| MAIS(["Carregar mais itens 🟡"])

    CHIPS -.->|include| ORDENA
```

**`Agregar rajada` acontece antes de ordenar, não depois.** Sem grafo social, o
lugar movimentado do dia soterraria o feed inteiro: cinco avaliações do mesmo
ponto viram uma linha com o número.

**`Renderizar a moldura com o motivo` é `include` e não `extend`** porque
`FeedCardFrame` exige `reason` no tipo — card sem motivo não compila
(`RF-FED-02`).

## Agir sobre um card

```mermaid
flowchart LR
    EXP(("👤 Explorador"))

    EXP --- UTIL(["Marcar como útil 🟡"])
    UTIL -.->|include| ALTERNA[/"Alternar o estado no item 🟡"/]

    EXP --- SALVAR(["Acompanhar lugar 🟡"])
    SALVAR -.->|include| ALTERNA2[/"Alternar acompanhando 🟡"/]
    ALTERNA2 -.->|include| AVISA[/"Avisar em toast 🟡"/]

    EXP --- MENOS(["Ver menos disso 🟡"])
    MENOS -.->|include| ESCOPO(["Escolher bairro, categoria ou tipo 🟡"])
    ESCOPO -.->|include| REGRA[/"Criar a regra de silêncio 🟡"/]
    REGRA -.->|include| CHIP[/"Mostrar a regra como chip com contador 🟡"/]
    CHIP -.->|extend| REMOVE(["Remover a regra pelo chip 🟡"])

    EXP --- CURADORIA(["Abrir a pauta do card 🟡"])
    CURADORIA -.->|include| SLUG[/"Navegar para /pautas/[slug] 🟡"/]
```

**Nenhum destes escreve em lugar nenhum.** `use-feed.ts` mexe só no array em
memória: recarregar a página desfaz tudo. É o que muda quando `Favorita` e a
tabela de reação nascerem.

**A regra de silêncio vira chip visível de propósito** — filtro que o usuário
esqueceu de ter criado é pior que filtro nenhum (`RNF-USA-07`).

---

## Descrições dos casos

As descrições em template expandido — ator, pré e pós-condição, ações do
ator × ações do sistema numeradas e restrições — vivem em
[`descricoes/03-descoberta-e-feed.md`](../descricoes/03-descoberta-e-feed.md): Descobrir lugares, Ler o feed, Ver menos disso e Acompanhar lugar.

---

➡️ [04 — Comunidade e perfil](./04-comunidade-e-perfil.md)
