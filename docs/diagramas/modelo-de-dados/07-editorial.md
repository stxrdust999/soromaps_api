> 🗄️ **Recorte de [`00-schema-completo.dbml`](./00-schema-completo.dbml)**, que é a fonte de verdade. Snippet aqui é cópia para leitura.

# 📰 07. Editorial

Duas tabelas: `pauta` e `pauta_ponto`. É onde mora o único caminho de verdade
que já roda hoje — a chamada ao Gemini em
[`src/lib/gemini.ts`](../../../src/lib/gemini.ts) — e cujo resultado, por não
ter tabela, é **copiado à mão** da tela.

```dbml
Table pauta {
  id            integer      [pk, increment]
  slug          varchar(80)  [not null, unique]
  chapeu        varchar(24)  [not null]
  titulo        varchar(90)  [not null]
  chamada       varchar(220) [not null]
  corpo         jsonb        [not null] // array de 3 a 6 parágrafos
  foto_url      varchar(500)

  origem        origem_pauta [not null] // ia | equipe
  modelo        varchar(40)
  status        status_pauta [not null, default: 'rascunho']
  revisado_por  integer      [ref: > usuario.id]
  publicado_em  timestamptz
  tempo_leitura smallint     [not null]
}

Table pauta_ponto {
  pauta_id integer
  ponto_id integer
  ordem    smallint [not null]
  // PK (pauta_id, ponto_id)
}
```

## ✅ Dois CHECK que são regra de produto

```sql
CHECK (origem <> 'ia' OR modelo IS NOT NULL)
CHECK (status <> 'publicada' OR (revisado_por IS NOT NULL AND publicado_em IS NOT NULL))
```

1. **Pauta de IA aparece rotulada ao leitor.** Sem `modelo`, o rótulo não teria
   o que dizer, e texto sobre comércio real escrito por modelo passaria como
   texto da equipe.
2. **Publicar é decisão de gente.** Geração é etapa de autoria, não de render:
   `rascunho` é o estado inicial de tudo que a IA escreve, e a rota pública
   responde **404** para rascunho. O `CHECK` impede que um `UPDATE` apressado
   publique sem revisor.

São as duas defesas do ADR
[0003](../../adr/user/0003-comunidade-selo-e-pauta-ia.md) descendo para o
banco. As outras duas — lista fechada de lugares e revalidação Zod da saída —
ficam na aplicação, onde já estão.

## 📄 `corpo` em `jsonb`, não em tabela de parágrafo

Array de 3 a 6 strings, mesma régua do `storyDraftSchema`. Parágrafo de pauta
**não é entidade**: ninguém comenta, denuncia, linka ou ordena um parágrafo
isolado — ele só é lido em sequência. Tabela filha custaria um `JOIN` com
`ORDER BY` em toda leitura para não ganhar nada.

Se um dia parágrafo virar âncora de link ou de comentário, aí ele vira tabela —
é o mesmo critério que fez `ponto_foto` ser tabela e `analise.foto_url` ser
coluna.

## 🔒 `pauta_ponto` é o que fecha a lista entregue ao modelo

De 2 a 4 lugares por pauta (`MIN_PLACES` / `MAX_PLACES`, também no
`generateStorySchema`). Não é decoração de rodapé: é a **primeira defesa**
contra alucinação. O modelo recebe ficha de fatos de pontos que existem no mapa
e não escolhe assunto sobre a cidade inteira — deixá-lo escolher é o caminho
mais curto para citar um estabelecimento que não existe, ou inventar preço e
horário de um que existe.

`ordem` porque a pauta cita os lugares numa sequência que o texto segue.

## 🌐 Rota própria, e o schema não muda isso

`/pautas/[slug]`, fora de `/community`: a pauta é destino de três lugares —
vitrine da comunidade, card `curadoria` do feed e, no futuro, a página do
ponto. `slug` é `UNIQUE` porque é URL pública já compartilhável.

## ⏭️ O que nasce junto

A fila de revisão em `/admin` — hoje inexistente. Com `status` e
`revisado_por`, ela é uma listagem sobre `WHERE status = 'rascunho'`, no mesmo
padrão de tabela das outras telas de admin.

## 🪦 Mock que morre aqui

[`src/mocks/stories.ts`](../../../src/mocks/stories.ts) e o item `curadoria` de
[`src/mocks/feed.ts`](../../../src/mocks/feed.ts).

## ➡️ Volta ao índice

[README](./README.md)
