> 🗄️ **Recorte de [`00-schema-completo.dbml`](./00-schema-completo.dbml)**, que é a fonte de verdade. Snippet aqui é cópia para leitura.

# 📍 02. Lugar e catálogo

Cinco tabelas: `ponto`, `categoria`, `ponto_foto`, `tag`, `ponto_tag`. É o
núcleo do produto — tudo que vem depois (avaliação, visita, conquista, pauta)
pendura aqui.

```dbml
Table categoria {
  id    integer     [pk, increment]
  nome  varchar(60) [not null, unique]
  slug  varchar(60) [not null, unique]
  icone varchar(40) [not null]
  cor   char(7)     [not null]
  ordem smallint    [not null]
  ativa boolean     [not null, default: true]
}

Table ponto {
  id             integer      [pk, increment]
  nome           varchar(120) [not null]
  lat            double       [not null]
  lng            double       [not null]
  bairro         varchar(60)  [not null]
  categoria_id   integer      [ref: > categoria.id]
  autor_id       integer      [ref: > usuario.id]

  sobre          varchar(160)
  descricao      varchar(600)
  tem_wifi       boolean      [not null, default: false]
  pet_friendly   boolean      [not null, default: false]
  melhor_horario varchar(60)
  segredo_local  varchar(200)

  status         status_ponto [not null, default: 'pendente']
}

Table ponto_foto { id integer [pk]; ponto_id integer; url varchar(500); capa boolean }
Table tag        { id integer [pk]; nome varchar(40) [unique]; slug varchar(40) [unique] }
Table ponto_tag  { ponto_id integer; tag_id integer } // PK composta
```

## 🧾 O formulário já coleta tudo isto

`/places/new` valida oito campos e a API grava três. `sobre`, `descricao`,
`tem_wifi`, `pet_friendly`, `melhor_horario` e `segredo_local` são exatamente
os campos que o navegador valida e descarta hoje — a
[proposta de 2026-08-03](../../propostas/2026-08-03-expansao-modelo-ponto.md)
está inteira aqui dentro.

`bairro` é adição desta rodada. Não estava na proposta e virou obrigatório
porque **quatro telas o usam como eixo**: motivo `perto` do feed, ranking por
bairro em `/community`, cobertura da cidade em `/profile/stats` e alvo de
critério de conquista (`ACHIEVEMENT_TARGET_NEIGHBORHOODS`). Derivar bairro de
`lat`/`lng` exigiria geocodificação reversa a cada consulta.

## 🚦 `status` é o que faz a moderação existir

Quatro valores — `pendente`, `devolvido`, `aprovado`, `rejeitado`. Só
`aprovado` aparece no mapa público. **Sem esta coluna, `/admin/moderation` não
tem o que ler**: é a única dependência da tela inteira, junto com `autor_id`.

`devolvido` existe separado de `rejeitado` porque a tela decide olhando
completude: ficha com metade dos campos vazios volta para o autor, não vira
rejeição. Ver
[ADR 0003 de admin](../../adr/admin/0003-moderacao-mestre-detalhe.md).

## 🔢 O que deliberadamente **não** é coluna

| Aparece na tela | De onde vem |
|---|---|
| `nota` do local | `AVG(analise.nota) WHERE status = 'publicada'` |
| `totalAvaliacoes` | `COUNT` da mesma consulta |
| `distancia` | Calculado por requisição, contra a posição de quem olha |
| `pontos` da categoria | `COUNT(ponto) GROUP BY categoria_id` |
| `novosNaSemana` | O mesmo `COUNT` filtrado por `criado_em` |

Média denormalizada em coluna dessincroniza no dia em que a moderação remove
uma avaliação — e remover avaliação é operação normal aqui, não exceção. Se a
consulta doer, a resposta é *materialized view* com refresh, não coluna
mantida à mão.

## 🖼️ Foto em tabela, não em coluna

`ponto_foto` porque a página do local tem galeria (`fotos[]` no mock) e a
moderação mostra "3 fotos" por sugestão. `capa` marca a que vira banner, card
do feed e popup do mapa — com índice único parcial
(`WHERE capa`), que o DBML não expressa.

`enviada_por` existe para a foto imprópria ter dono: é um dos motivos fechados
de rejeição.

## 🏷️ `tag` separado de `categoria`

Categoria responde **"o que é"** (Cafeteria, Parque) e é exclusiva: um ponto,
uma categoria. Tag responde **"pra quê"** (calmo, trabalho, ar livre) e é
múltipla. `/discover` filtra pelos dois eixos ao mesmo tempo, e a "vibe" do
formulário casa contra tags + amenidades — ver
[`src/constants/places.ts`](../../../src/constants/places.ts).

## 🗑️ Exclusão de categoria com reatribuição

`categoria_id` com `ON DELETE RESTRICT`. A tela de categorias já pede um
destino ao excluir e só então apaga: a reatribuição é `UPDATE ... SET
categoria_id = destino` antes do `DELETE`, dentro da mesma transação. `SET
NULL` deixaria pontos sem categoria no mapa; `CASCADE` apagaria pontos ao
apagar uma categoria — dano permanente por um clique de admin.

## 🗺️ Índice de bounding box, não PostGIS

`(lat, lng)` cobre o filtro do mapa que o backlog prevê para `/api/markers`.
PostGIS entra quando busca por raio (RF-11) sair do papel — hoje seria
complexidade sem uso, como já registrado em
[08 — Banco atual](../../wiki/03-banco.md).

## 🪦 Mock que morre aqui

[`src/mocks/markers.ts`](../../../src/mocks/markers.ts) (`MarkerDetailsMock`) e
[`src/mocks/admin-categories.ts`](../../../src/mocks/admin-categories.ts).

## ➡️ Próxima página

[03 — Contribuição](./03-contribuicao.md)
