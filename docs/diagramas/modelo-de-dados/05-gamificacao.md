> 🗄️ **Recorte de [`00-schema-completo.dbml`](./00-schema-completo.dbml)**, que é a fonte de verdade. Snippet aqui é cópia para leitura.

# 🏅 05. Gamificação

Duas tabelas, e nenhuma delas guarda pontos: `conquista` (o catálogo) e
`ganha_conquista` (quem tem o quê).

```dbml
Table conquista {
  id         integer             [pk, increment]
  nome       varchar(60)         [not null, unique]
  descricao  varchar(160)        [not null]
  icone      varchar(40)         [not null]
  cor        char(7)             [not null]

  evento     evento_conquista    [not null] // visitar | avaliar | criar | favoritar | sequencia
  quantidade integer             [not null]
  tipo_alvo  tipo_alvo_conquista            // categoria | bairro | null
  alvo       varchar(60)

  ativa      boolean             [not null, default: true]
}

Table ganha_conquista {
  usuario_id   integer
  conquista_id integer
  obtida_em    timestamptz [not null, default: `now()`]
  // PK (usuario_id, conquista_id)
}
```

## 🧮 Critério declarativo: quatro colunas montam a frase

`(evento, quantidade, tipo_alvo, alvo)` produz "Visitar 5 lugares da categoria
Cafeteria" — é o que `formatCriterion` já faz sobre o mock. **Conquista nova é
`INSERT`, não deploy**, e é isso que sustenta a aba de calibragem de
`/admin/achievements`.

`CHECK ((tipo_alvo IS NULL) = (alvo IS NULL))`: escolher o tipo de alvo obriga
o alvo, senão o critério fecharia em "Visitar 5 lugares da categoria …" e o
motor não teria o que comparar. É a mesma regra do `superRefine` em
[`src/validations/achievements.ts`](../../../src/validations/achievements.ts) —
o banco repete porque validação de formulário é UX, não integridade.

## 🚫 O evento `seguir` não existe

O catálogo do mock ainda tem "siga 15 pessoas". O enum
`evento_conquista` **não** inclui `seguir`: `Segue` saiu do produto em
2026-08-17, e cobrar o que a plataforma não faz seria critério impossível de
cumprir. `/profile/achievements` já filtra esse evento fora da galeria; aqui o
banco passa a impedir que ele volte por engano.

## 🔒 PK composta é a decisão inteira

`ganha_conquista` sem `id` próprio, com PK `(usuario_id, conquista_id)`:

- **duplicata é impossível por construção** — o motor pode reprocessar o
  catálogo inteiro sem medo, e reprocessar é o que se faz quando um critério
  nasce errado;
- `obtida_em` fica fora da PK porque só existe uma obtenção;
- `COUNT(*) GROUP BY usuario_id` é o número que vira o título do explorador.

Foi exatamente isso que XP não conseguia entregar: acumulador concedido duas
vezes fica errado para sempre, e torná-lo reprocessável exigiria uma tabela de
transações — o dobro de superfície para a mesma sensação. Ver a decisão de
2026-08-12 no [`CLAUDE.md`](../../../CLAUDE.md).

## 📊 Agregados, não colunas

| Aparece na tela | De onde vem |
|---|---|
| `obtencoes` (quantos ganharam) | `COUNT(ganha_conquista) GROUP BY conquista_id` |
| `raridade` (percentil) | `obtencoes / COUNT(usuario)` |
| Título "Guia local · 8 conquistas" | `COUNT(ganha_conquista)` por usuário |
| Progresso "3 de 5 cafeterias" | Consulta do próprio critério contra `visita`/`analise` |

`obtencoes = 0` é o sinal de calibragem errada que a tela de admin destaca —
por ser derivado, ele nunca mente.

## ⚙️ Onde mora o motor de concessão

Fora do schema, e de propósito. Três gatilhos, herdados de
`ACHIEVEMENT_EVENTS`:

- `metric` — recontagem depois de `visita`, `analise` ou `favorita`;
- `api` — evento explícito (ponto **aprovado**, que só a moderação sabe);
- `streak` — sequência de dias, que precisa varrer `visita` por janela.

Todos são `INSERT ... ON CONFLICT DO NOTHING` em `ganha_conquista`. Nenhum
precisa de coluna nova.

## 🪦 Mock que morre aqui

[`src/mocks/admin-achievements.ts`](../../../src/mocks/admin-achievements.ts) e
`profileAchievements()` em
[`src/mocks/profile.ts`](../../../src/mocks/profile.ts).

## ➡️ Próxima página

[06 — Moderação](./06-moderacao.md)
