> 🗄️ **Recorte de [`00-schema-completo.dbml`](./00-schema-completo.dbml)**, que é a fonte de verdade. Snippet aqui é cópia para leitura.

# 🧭 04. Atividade e feed

Três tabelas: `visita`, `favorita` e `feed_silenciado`. As duas primeiras são o
que a pessoa faz na cidade; a terceira é o que ela pede para ver menos.

```dbml
Table visita {
  usuario_id  integer
  ponto_id    integer
  visitado_em timestamptz   [not null]
  origem      origem_visita [not null, default: 'manual']
  // PK (usuario_id, ponto_id, visitado_em)
}

Table favorita {
  usuario_id integer
  ponto_id   integer
  criado_em  timestamptz [not null, default: `now()`]
  // PK (usuario_id, ponto_id)
}

Table feed_silenciado {
  usuario_id integer
  escopo     escopo_silenciado // bairro | categoria | tipo
  valor      varchar(40)
  // PK (usuario_id, escopo, valor)
}
```

## 🔁 Data na PK de `visita`, ausente na de `favorita`

Decisão herdada do TCC e confirmada pela tela: **visita é evento repetível,
favoritar é estado.** A timeline de `/profile/visits` mostra o Cabocafé três
vezes de propósito — as 24 visitas do mock cobrem 16 lugares. Deduplicar daria
um histórico que mente sobre o que a pessoa fez.

Em `favorita`, desfavoritar é `DELETE`. Não há histórico de "já foi favorito"
porque nenhuma tela pergunta isso.

## 📌 `origem` na visita

`gps` ou `manual`. Existe porque o check-in por GPS é o caminho previsto em
[`docs/todo/user/visits.md`](../../todo/user/visits.md) e porque **conquista
concedida sobre visita não conferida é conquista de graça** — o motor pode
exigir `origem = 'gps'` para critérios de sequência sem migrar nada.

## 🍽️ De onde saem os cinco motivos do feed

O feed **não tem grafo social** (ADR
[0002](../../adr/user/0002-feed-sem-grafo-social.md)). Todo item entra por
vínculo com lugar, e cada motivo é uma consulta sobre tabelas que já estão
aqui:

| Motivo | Consulta |
|---|---|
| `perto` | `ponto.bairro` contra `usuario.bairro` (ou raio sobre `lat`/`lng`) |
| `salvo` | `favorita` do usuário |
| `categoria` | Categorias mais frequentes em `visita` do usuário |
| `cidade` | Volume recente em `analise`/`visita`, sem recorte pessoal |
| `curadoria` | `pauta` publicada — ver [07](./07-editorial.md) |

`relevancia` é coluna calculada na query (decaimento por idade × peso da
fonte), não valor guardado.

## 🚫 Não existe tabela de feed

Item de feed é **consulta**, não linha. Os seis tipos (`avaliacao`,
`movimento`, `novo-ponto`, `conquista`, `marco`, `curadoria`) são projeções
sobre `analise`, `visita`, `ponto`, `ganha_conquista` e `pauta`.

O caso que mais tenta a materialização é `movimento` — a rajada agregada ("4
pessoas avaliaram o Cabocafé nas últimas 6 horas"). Ele continua sendo
`GROUP BY ponto_id` numa janela de tempo: agregação em tabela envelhece (a
janela é relativa a agora) e teria que ser recalculada de qualquer jeito.

Tabela de feed só se paga com fan-out de escrita, que é exatamente a
arquitetura que grafo social exige — e grafo social é o que este produto
recusou.

## 🔇 `feed_silenciado` persiste porque o chip precisa sobreviver ao F5

"Ver menos disso" em três escopos vira chip removível com contador no topo do
feed. Guardar isso só em memória daria um filtro que some ao recarregar; e
filtro que o usuário esqueceu de ter criado é pior que filtro nenhum — por isso
ele é **visível e removível**, o que exige linha persistida com o valor que
originou o silêncio.

`valor` é `varchar` livre porque os três escopos apontam para coisas
diferentes: nome de bairro (que não tem tabela), nome de categoria e chave do
tipo de item (`FeedItemKind`, que é constante de código). FK aqui exigiria três
colunas nullable para ganhar integridade sobre um dado que o usuário pode
descartar a qualquer momento.

## 🪦 Mock que morre aqui

[`src/mocks/feed.ts`](../../../src/mocks/feed.ts) e a parte de visitas/salvos
de [`src/mocks/profile.ts`](../../../src/mocks/profile.ts).

## ➡️ Próxima página

[05 — Gamificação](./05-gamificacao.md)
