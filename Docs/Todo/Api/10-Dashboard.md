# 📊 10. Dashboard

> **Status:** 💤 · **Depende de:** [04](./04-moderacao.md), [05](./05-contribuicao.md) (idealmente [08](./08-denuncias-e-feedback.md)) · [Índice](./README.md)

`/admin/dashboard` não tem tabela própria: é agregado sobre o que já existe.
Um endpoint só evita o front fazer N chamadas de listagem apenas para contar.

---

## 🔌 API

| Método | Rota | Acesso | Faz |
|---|---|---|---|
| `GET` | `/api/admin/stats?from=&to=` | admin | Tudo o que o dashboard mostra, numa resposta |

Conteúdo da resposta:

| Bloco | Origem |
|---|---|
| Filas de atenção | `places` `pending` (e idade da mais antiga), `reports` `open`, `feedback` `new`, `stories` `draft` |
| Cards de número | Usuários, pontos aprovados, avaliações publicadas e decisões de moderação no período, com variação sobre o período anterior |
| Série temporal | Contagem diária de novos pontos e avaliações (preencher dias sem registro com zero) |
| Moderação | Decididos hoje e tempo médio de decisão, a partir de `moderation_decisions` |

- [ ] Datas em UTC na API; o front formata
- [ ] Se a consulta pesar, *materialized view* com refresh — nunca coluna contadora mantida à mão

---

## 🖥️ Front

- [ ] `/admin/dashboard` sobre a API (filas, quatro cards, dois gráficos Recharts)

Mock que sai: `src/mocks/admin-dashboard.ts` — o último mock de admin.

---

## ✅ Verificação

- [ ] Aprovar um ponto reduz a fila e incrementa "decididos hoje"
- [ ] Série de um período sem atividade vem com zeros, não com dias faltando
- [ ] Explorador chamando o endpoint: `403`
