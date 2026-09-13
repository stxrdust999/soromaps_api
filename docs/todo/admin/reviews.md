# ⭐ Avaliações

> Área: admin · Rota: `/admin/reviews` · Status: 🟡 parcial — tela pronta, dados em mock

## Ideia

A visão administrativa de todas as avaliações da plataforma, em listagem única com busca por local ou autor, filtro por nota e ordenação por data. O admin lê, e remove o que viola as regras — com o motivo registrado.

É a única tela que enxerga avaliação **fora** do contexto do local. A [Moderação](./moderation.md) vê só as que alguém denunciou. (A terceira visão — o dono vendo as do próprio ponto — foi cancelada em 19/08/2026 com o [gerenciamento por dono](../../archive/gerenciamento-por-dono/).)

## Por que vale

- Avaliação é o conteúdo mais sensível do produto: é o que sustenta a confiança que é a tese, e o que mais atrai abuso (nota comprada, vingança contra concorrente).
- Padrão de nota anômalo — dez avaliações 5 estrelas no mesmo dia, no mesmo lugar — só aparece olhando o conjunto, nunca uma a uma.
- Fecha o caso de uso "remover conteúdo" do ator Administrador do TCC.

## Dependências

| O quê | Situação |
|---|---|
| `Analise` | ❌ não existe — já modelada no TCC |
| `Comentario` (a conversa pendurada na avaliação) | ❌ |
| Papel de admin checado na API | 🔴 pré-requisito |
| Padrão de listagem | 🟢 pronto, portado em `/admin/users` |
| Registro de quem removeu e por quê | ❌ decidir se vira log próprio ou coluna de exclusão lógica |

## Escopo inicial

- Tabela com busca por local/autor, filtro por nota e ordenação por data
- Remoção com motivo obrigatório, por server action
- Exclusão lógica, não física: avaliação removida some da vitrine mas fica auditável

## Fora do escopo inicial

- Detecção automática de avaliação suspeita
- Edição do texto de avaliação alheia — admin remove, nunca reescreve
- Fila de denúncias, que é [Denúncias e feedback](./reports.md)

> Decisões já tomadas e componentes existentes em
> [adr/admin/0007-avaliacoes-remocao-unificada.md](../../adr/admin/0007-avaliacoes-remocao-unificada.md)

## Fronteira com as telas vizinhas

Esta tela e [Denúncias e feedback](./reports.md) removem a mesma coisa por caminhos diferentes: aqui o admin varre por conta própria, lá ele reage ao que a comunidade sinalizou. **A remoção precisa ser uma única server action**, consumida pelas duas — duas implementações divergem no primeiro ajuste de regra, e o motivo deixa de ser registrado igual.
