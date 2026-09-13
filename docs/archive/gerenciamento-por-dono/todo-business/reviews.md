# 💬 Responder avaliações

> Área: estabelecimento · Rota: `/business/reviews` · Status: 💤 não iniciado

## Ideia

Fila de trabalho do dono: todas as avaliações do estabelecimento, com filtro **respondidas / sem resposta**, e um campo de resposta inline. A resposta aparece na avaliação com selo "Resposta do estabelecimento", visualmente distinta dos comentários de usuários comuns.

A modelagem já suporta: `Comentario` pendura em `Analise` (decisão do TCC — comentário é conversa em torno de uma avaliação), e o dono é só um usuário comentando. O que este módulo adiciona é o **selo** (comentário cujo autor é o `UsuarioDono` do ponto ganha destaque) e a visão de fila.

## Por que vale

- É o RF-09 com o twist que interessa: resposta oficial é o que dá ao comércio voz na própria reputação — a metade esquecida dos apps de review.
- Avaliação negativa **respondida com cuidado** converte mais que nota alta sem resposta; é o que o Augusto faria com a ferramenta.
- Taxa de resposta é métrica de engajamento B2B pronta pro painel.

## Dependências

| O quê | Situação |
|---|---|
| `Analise` + `Comentario` | ❌ não existem — já modeladas |
| `tipoUsuario` + FK `UsuarioDono` (identificar o dono) | ❌ |
| [Painel do negócio](./dashboard.md) (navegação-mãe) | 💤 |

## Escopo inicial

- Lista com filtro respondidas/sem resposta e ordenação por data/nota
- Resposta inline (RHF + server action, padrão do projeto)
- Selo visual da resposta oficial onde a avaliação for exibida
- Uma resposta oficial por avaliação (editável, não múltipla)

## Fora do escopo inicial

- Templates de resposta
- Alertas de avaliação negativa (vira tipo em [Notificações](../../../todo/user/notifications.md))
