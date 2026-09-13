> 🗄️ **Recorte de [`00-schema-completo.dbml`](./00-schema-completo.dbml)**, que é a fonte de verdade. Snippet aqui é cópia para leitura.

# 🛡️ 06. Moderação

Quatro tabelas — `decisao_moderacao`, `denuncia`, `remocao_conteudo` e
`feedback` — e **nenhuma delas estava no modelo do TCC**. São adição desta
rodada, porque as telas de admin já existem e não tinham onde escrever.

```dbml
Table decisao_moderacao {
  id           integer       [pk, increment]
  ponto_id     integer       [not null, ref: > ponto.id]
  moderador_id integer       [not null, ref: > usuario.id]
  decisao      decisao_ponto [not null] // aprovado | devolvido | rejeitado
  motivo       varchar(40)
  observacao   varchar(300)
  criado_em    timestamptz   [not null, default: `now()`]
}

Table denuncia {
  id             integer             [pk, increment]
  alvo_tipo      tipo_alvo_moderavel [not null] // avaliacao | comentario | ponto | perfil
  alvo_id        integer             [not null]
  denunciante_id integer             [not null, ref: > usuario.id]
  motivo         motivo_denuncia     [not null]
  status         status_denuncia     [not null, default: 'aberta']
  // UNIQUE (alvo_tipo, alvo_id, denunciante_id)
}

Table remocao_conteudo {
  id           integer             [pk, increment]
  alvo_tipo    tipo_alvo_moderavel [not null]
  alvo_id      integer             [not null]
  motivo       varchar(40)         [not null]
  moderador_id integer             [not null, ref: > usuario.id]
  denuncia_id  integer             [ref: > denuncia.id]
}

Table feedback {
  id          integer         [pk, increment]
  usuario_id  integer         [ref: > usuario.id] // null = anônimo
  tipo        tipo_feedback   [not null] // bug | sugestao | elogio
  mensagem    varchar(2000)   [not null]
  status      status_feedback [not null, default: 'novo']
  rota        varchar(120)
  dispositivo varchar(80)
}
```

## 📜 Estado no ponto, história na tabela de decisão

`ponto.status` responde "onde este ponto está agora". `decisao_moderacao`
responde "como ele chegou lá", e é **append-only**:

- **"Desfazer" dentro de 24 h é linha nova**, nunca `DELETE`. A aba de
  histórico precisa mostrar que houve reversão — apagar a decisão original
  esconderia justamente o que vale auditar.
- "Decididos hoje" e "tempo médio de decisão" — os dois números do topo da fila
  — saem desta tabela, não de `ponto`.
- `motivo` é lista fechada (`REJECTION_REASONS`) porque vira métrica e vira
  aviso ao autor. Campo livre não serve para nenhum dos dois.

## 🎯 Alvo polimórfico em `denuncia`: o custo, explícito

`(alvo_tipo, alvo_id)` sem FK. A fila de `/admin/reports` é **uma só** e agrupa
por alvo — avaliação, comentário, ponto e perfil chegam na mesma tela, com o
mesmo fluxo de decisão.

**O que se perde:** o banco não impõe integridade referencial em `alvo_id`. A
aplicação passa a ser responsável por não deixar denúncia órfã, e um `DELETE`
manual de avaliação deixa lixo.

**A alternativa considerada:** quatro colunas FK nullable
(`analise_id`, `comentario_id`, `ponto_id`, `perfil_id`) com `CHECK` de
exclusividade. Dá integridade de verdade e transforma **toda** consulta da fila
num `COALESCE` de quatro caminhos — inclusive o agrupamento por alvo, que é a
operação central da tela.

Escolha registrada: polimórfico, com a aplicação pagando a conta.

## 🚩 O UNIQUE que sustenta o selo de denúncia coordenada

`UNIQUE (alvo_tipo, alvo_id, denunciante_id)`. A tela destaca alvo com muitas
denúncias de contas recentes e mesmo motivo. Esse sinal só significa alguma
coisa se **cinco denúncias forem cinco pessoas** — sem o UNIQUE, seria uma
pessoa clicando cinco vezes, e o destaque viraria ruído.

## 🗑️ Uma tabela de remoção para dois caminhos

`/admin/reports` remove reagindo a denúncia; `/admin/reviews` remove varrendo
por conta própria. `denuncia_id` nullable é a única diferença entre os dois.

A promessa está cumprida no front desde 2026-08-09 — `REMOVAL_REASONS` vive em
[`src/constants/content-removal.ts`](../../../src/constants/content-removal.ts)
e `RemovalDialog` é compartilhado. O schema termina o trabalho: **duas tabelas
divergiriam no primeiro ajuste de regra** e quebrariam a métrica de moderação.

A remoção em si continua sendo `status = 'removida'` no conteúdo; esta tabela
guarda o porquê, quem e a partir de quê.

## 🐛 `feedback` não tem alvo

É sobre o produto, não sobre conteúdo de outra pessoa: não há o que remover nem
quem punir. Por isso não entra no polimorfismo acima.

- `usuario_id` nullable = envio anônimo, que **não tem canal de resposta** — e
  é o que faz `status = 'respondido'` ser impossível para essas linhas.
- `rota` e `dispositivo` só em `bug`. São o que separa relato acionável de "não
  funciona".

## 🔗 Quem depende disto

| Tela | Tabela |
|---|---|
| `/admin/moderation` | `decisao_moderacao` + `ponto.status` + `ponto.autor_id` |
| `/admin/reports` (fila) | `denuncia` + `remocao_conteudo` |
| `/admin/reports` (triagem) | `feedback` |
| `/admin/reviews` | `remocao_conteudo` + `analise.status` |
| `/community`, `/profile` | `remocao_conteudo` derruba o selo de verificado |

## 🪦 Mock que morre aqui

[`src/mocks/admin-moderation.ts`](../../../src/mocks/admin-moderation.ts) e
[`src/mocks/admin-reports.ts`](../../../src/mocks/admin-reports.ts).

## ➡️ Próxima página

[07 — Editorial](./07-editorial.md)
