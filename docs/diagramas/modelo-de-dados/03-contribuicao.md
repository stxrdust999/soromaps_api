> 🗄️ **Recorte de [`00-schema-completo.dbml`](./00-schema-completo.dbml)**, que é a fonte de verdade. Snippet aqui é cópia para leitura.

# ⭐ 03. Contribuição

Três tabelas: `analise` (a avaliação), `comentario` (a conversa em torno dela)
e `analise_util` (o voto de "útil"). É o pilar do produto que **nunca saiu do
papel** — as duas primeiras estavam no modelo lógico do TCC desde o começo.

```dbml
Table analise {
  id         integer         [pk, increment]
  ponto_id   integer         [not null, ref: > ponto.id]
  usuario_id integer         [not null, ref: > usuario.id]
  nota       smallint        [not null]  // CHECK 1..5
  corpo      varchar(1000)   [not null]
  foto_url   varchar(500)
  status     status_conteudo [not null, default: 'publicada']
}

Table analise_util {
  analise_id integer
  usuario_id integer
  // PK (analise_id, usuario_id)
}

Table comentario {
  id            integer         [pk, increment]
  analise_id    integer         [not null, ref: > analise.id]
  usuario_id    integer         [not null, ref: > usuario.id]
  resposta_a_id integer         [ref: > comentario.id]
  corpo         varchar(600)    [not null]
  status        status_conteudo [not null, default: 'publicada']
}
```

## ⚠️ Sem `UNIQUE (ponto_id, usuario_id)` — de propósito

A tentação é óbvia: uma avaliação por pessoa por lugar. Mas
`/admin/reviews` lista **"duplicada"** como um dos três sinais formais de
triagem, ao lado de spam e discrepante. O mesmo autor avaliando o mesmo lugar
duas vezes é informação que a moderação precisa **ver e julgar** — pode ser
manipulação de nota, pode ser alguém corrigindo a opinião depois de voltar.

Constraint no banco apagaria a evidência antes de a tela existir. A regra, se
um dia virar regra, é da aplicação e reversível; a constraint não é.

## 💬 `comentario` pendurado em `analise`, com auto-referência

`analise_id` e não `ponto_id`: RF-09 pede comentário **com resposta** —
conversa em torno de uma avaliação específica, não mural do estabelecimento.
Decisão herdada do TCC e mantida.

`resposta_a_id` é o que faltava no modelo original: sem ela, "comentário com
resposta" seria só uma lista plana. Auto-referência nullable dá um nível de
thread sem tabela extra; profundidade maior é decisão de UI, não de schema.

## 👍 "Útil" é tabela, não contador

```
uteis = COUNT(*) FROM analise_util WHERE analise_id = ?
```

Duas razões, e a segunda é a que importa:

1. **Idempotência.** PK composta `(analise_id, usuario_id)` impede voto duplo
   por construção. Coluna acumuladora processada duas vezes fica errada para
   sempre — a mesma razão que tirou XP do produto em 2026-08-12.
2. **É "útil", não "curtir".** Curtida mede simpatia pelo autor; útil mede se a
   dica ajudou a decidir, e por isso pode **ordenar** as avaliações na página do
   ponto. Ordenação precisa saber quem votou para não deixar o autor inflar a
   própria dica.

## 🚫 `status` em vez de `DELETE`

`publicada` / `removida` nas duas tabelas. Remover conteúdo é ato de moderação
com motivo registrado (ver [06 — Moderação](./06-moderacao.md)), e três coisas
dependem do registro sobreviver:

- o selo de verificado cai se a pessoa tem **qualquer** avaliação removida
  (`avaliacoesRemovidas === 0` em `isVerifiedExplorer`);
- `/admin/reports` mostra reincidência do autor (`conteudoRemovido`);
- a média do local precisa **excluir** a removida sem esquecer que ela existiu.

`DELETE` físico apagaria os três.

## 📸 `foto_url` na análise, coluna e não tabela

Diferente de `ponto_foto`: a avaliação anexa **uma** foto, que o card do feed
mostra inteira. Galeria de avaliação não existe em nenhuma tela — quando
existir, vira tabela pelo mesmo caminho.

## 🔗 O que passa a funcionar quando estas três nascerem

| Tela | O que hoje é mock |
|---|---|
| `/places/[id]` | Nota, total de avaliações, comentário do explorador verificado |
| `/admin/reviews` | A tela inteira, incluindo os três sinais e a linha expansível |
| `/feed` | Item `avaliacao`, rajada `movimento` do tipo `avaliacoes`, contador de úteis |
| `/community` | `ultimasAvaliacoes` do perfil público e o critério de ≥3 avaliações do selo |
| `/profile` | Nota registrada junto da visita, e a régua do selo item a item |

## 🪦 Mock que morre aqui

[`src/mocks/admin-reviews.ts`](../../../src/mocks/admin-reviews.ts) inteiro, e
`PlaceCommentMock` de
[`src/mocks/markers.ts`](../../../src/mocks/markers.ts).

## ➡️ Próxima página

[04 — Atividade e feed](./04-atividade-e-feed.md)
