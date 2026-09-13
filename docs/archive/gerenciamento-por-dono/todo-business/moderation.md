# 🛡️ Moderação

> Área: estabelecimento · Rota: `/business/moderation` · Status: 💤 não iniciado

## Ideia

Fila de aprovação dos **comentários recebidos nas respostas do dono** ao próprio ponto: quando alguém responde a uma resposta oficial (ver [Responder avaliações](./reviews.md)), o comentário nasce `pendente` e só fica público depois do dono aprovar ou ocultar. É moderação de escopo próprio — não tem poder sobre a avaliação em si, só sobre a conversa que se forma em cima da resposta dele.

Diferente de [admin/moderation](../../../todo/admin/moderation.md), que é fila de aprovação de **pontos novos** no mapa inteiro: aqui o raio de ação é só o `Comentario` pendurado nas próprias respostas, e quem modera é o dono, não o admin.

## Por que vale

- Sem isso, "Responder avaliações" vira canal de exposição sem controle — qualquer um responde ao dono publicamente, sem filtro.
- Dá ao Augusto (persona) o mesmo tipo de controle que já espera de redes sociais: aprovar o que aparece embaixo do que ele escreveu.
- Escopo pequeno de propósito: não compete com a fila do admin, só complementa o módulo de resposta.

## Dependências

| O quê | Situação |
|---|---|
| `Comentario` (decisão do TCC: pendurado em `Analise`) | ❌ não existe — já modelada |
| Coluna `status` em `Comentario` (`pendente`/`aprovado`/`oculto`) | ❌ — não prevista no modelo original, extensão necessária |
| `tipoUsuario` + FK `UsuarioDono` (identificar o dono) | ❌ |
| [Responder avaliações](./reviews.md) (é onde o comentário nasce) | 💤 |

## Escopo inicial

- Fila com padrão de tabela: comentário, autor, avaliação de origem, data
- Aprovar / ocultar (server actions)
- Contador de pendências (alimenta o atalho do painel do negócio)

## Fora do escopo inicial

- Denúncia formal com motivo (isso é [Denúncias](../../../todo/admin/reports.md), escopo do admin)
- Banimento de usuário comentarista
