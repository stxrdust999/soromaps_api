> 🗄️ **Recorte de [`00-schema-completo.dbml`](./00-schema-completo.dbml)**, que é a fonte de verdade. Snippet aqui é cópia para leitura.

# 👤 01. Identidade e acesso

Uma tabela: `usuario`. É a única do modelo que **já existe** hoje, como
`tbUsuario` — este capítulo é tanto sobre o que entra quanto sobre o que a
migração renomeia.

```dbml
Table usuario {
  id            integer       [pk, increment]
  nome          varchar(80)   [not null, unique]
  email         varchar(160)  [not null, unique]
  senha_hash    varchar(72)   [not null]
  papel         papel_usuario [not null, default: 'explorador']

  avatar_url    varchar(500)
  bio           varchar(280)
  bairro        varchar(60)

  criado_em     timestamptz   [not null, default: `now()`]
  atualizado_em timestamptz   [not null, default: `now()`]
}
```

## 🔁 De `tbUsuario` para `usuario`

| Hoje | Proposto | Por quê |
|---|---|---|
| `tbUsuario` | `usuario` | Três convenções em duas tabelas hoje (`tbUsuario` × `markers`). A escolha importa menos que a consistência |
| `user_name` | `nome` | Coluna em inglês numa tabela em português; e o valor é nome de exibição, não login técnico |
| `user_email` | `email` | idem |
| `user_password` | `senha_hash` | O nome diz o que está guardado. `password` sugere que dá para comparar por igualdade |
| — | `papel` | Novo |
| — | `avatar_url`, `bio`, `bairro` | Novos |
| sem UNIQUE | `UNIQUE (nome)`, `UNIQUE (email)` | Item aberto do backlog de segurança: sem isso o login por `FirstOrDefault` devolve sempre a primeira linha duplicada |

## 🔑 `papel` é o que destrava três gates

Hoje **qualquer sessão válida** entra em `/admin`, edita e exclui qualquer
ponto, e aprova qualquer sugestão. Com `papel`:

- `middleware.ts` barra `/admin/*` para quem não é `admin`;
- `/places/[id]` só mostra editar/excluir para `admin` (ou para `ponto.autor_id`);
- `decisao_moderacao.moderador_id` passa a ter significado — hoje "quem
  decidiu" seria qualquer um.

Dois valores só, e nenhum "dono de estabelecimento": esse papel saiu do produto
em 2026-08-19, junto com `tipoUsuario`, `CNPJ` e toda a validação de documento
que ele arrastava.

## 🚫 O que não está aqui

**`pontuacao` e `nivel`.** O modelo lógico do TCC os previa; a decisão de
2026-08-12 os cortou. O título ao lado do nome sai de
`COUNT(ganha_conquista)`, calculado por
[`src/constants/explorer-titles.ts`](../../../src/constants/explorer-titles.ts).
Coluna acumuladora de XP erra para sempre quando concedida duas vezes; contagem
não tem como errar.

**Selo de verificado.** `isVerifiedExplorer` continua função pura sobre
contadores (`visitas`, `avaliacoes`, `avaliacoesRemovidas`, meses de conta) —
todos derivados de `visita`, `analise` e `criado_em`. Mudar a régua é mexer em
[`src/constants/verification.ts`](../../../src/constants/verification.ts), sem
migration. Coluna derivada só entra se a consulta doer.

**Tabela de sessão.** O cookie é JWT HS256 assinado pelo Next
(`src/lib/session.ts`), validado no runtime Edge sem round-trip. Não há
sessão para persistir.

**`Segue`.** Fora do produto desde 2026-08-17 — ver
[ADR 0002](../../adr/user/0002-feed-sem-grafo-social.md).

## 🎛️ Convenção: `Enum` no diagrama, `varchar` + `CHECK` na migration

Todo `Enum` do `.dbml` (aqui, `papel_usuario`) existe para o diagrama ficar
legível. Na migration ele vira `varchar` com `CHECK`:

```sql
papel varchar(16) NOT NULL DEFAULT 'explorador'
  CHECK (papel IN ('explorador', 'admin'))
```

**Motivo:** as listas fechadas deste produto mudam mais que as tabelas — motivo
de rejeição, motivo de remoção, tipo de feedback. Adicionar valor a um `TYPE`
do Postgres é `ALTER TYPE`; **remover** exige recriar o tipo e reescrever toda
coluna que o usa. `CHECK` é um `ALTER TABLE` nos dois sentidos, e o EF Core
mapeia `varchar` para `enum` C# sem provider extra.

## ➡️ Próxima página

[02 — Lugar e catálogo](./02-lugar-e-catalogo.md)
