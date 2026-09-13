# 🚩 Denúncias e feedback

> Área: admin · Rota: `/admin/reports` · Status: 🟡 parcial — tela pronta, dados em mock

## Ideia

A caixa de entrada do admin, em duas abas que dividem a rota mas não o assunto:

**Denúncias** — o canal de report da comunidade: qualquer usuário denuncia avaliação, comentário, ponto ou perfil (botão "denunciar" discreto + motivo). Do lado admin, a fila agrupa denúncias **por alvo** — o item denunciado 5 vezes é um caso, não cinco — com o conteúdo, o histórico do autor e as ações: descartar, remover conteúdo, advertir.

**Feedback** — o que o usuário tem a dizer sobre o *produto*, não sobre conteúdo de outro usuário: bug, sugestão, elogio. Não tem alvo, não tem o que remover, e o desfecho é triagem (`novo` / `lido` / `respondido`), não punição.

As duas convivem na mesma tela porque compartilham a forma — fila que chega sozinha e precisa ser despachada — e porque separá-las em duas rotas criaria um item de menu que fica vazio na maior parte do tempo. **O que não pode é tratá-las com o mesmo modelo:** o alvo, as ações e o significado de "resolvido" são diferentes.

Complementa a [Moderação](./moderation.md): lá é o filtro **preventivo** (conteúdo novo aguardando aprovação); aqui é o **reativo** (conteúdo já público que a comunidade sinalizou). Juntos cobrem os casos de uso de remoção do ator Administrador do TCC.

## Por que vale

- Moderação preventiva não escala pra comentários e avaliações (volume alto, dano baixo) — pra esses, publicar e reagir a report é o custo certo.
- Plataforma com conteúdo público de usuário **precisa** de canal de denúncia — é higiene básica, inclusive legal.
- Denúncias por usuário/alvo geram o sinal de reputação que outros módulos podem consumir depois.
- Feedback é o canal mais barato de descobrir o que está quebrado: num TCC sem analytics, é a única fonte de problema relatado por quem usa.

## Dependências

| O quê | Situação |
|---|---|
| Entidade nova `Denuncia` (`id`, `autorID`, `alvoTipo`, `alvoID`, `motivo`, `status`, `created_at`) | ❌ — **não estava no modelo do TCC**, é adição nossa. Alvo polimórfico (`alvoTipo` + `alvoID`) |
| Entidade nova `Feedback` (`id`, `autorID`, `tipo`, `mensagem`, `status`, `created_at`) | ❌ — sem alvo, e o `autorID` pode ser nulo se aceitar envio anônimo |
| Conteúdo denunciável (`Analise`, `Comentario`) | ❌ — pontos e perfis já existem |
| Papel de admin | 🔴 pré-requisito |

**Decisão tomada (2026-08-09):** feedback **aceita envio anônimo**. A tela trata os dois casos na mesma lista, e a linha anônima mostra "sem como responder" com a ação de responder desabilitada — o custo do anônimo fica visível em vez de virar surpresa. O chip de autoria existe para medir a proporção depois.

## Escopo inicial

- Botão denunciar com motivos fixos (spam, ofensa, informação falsa, outro)
- Fila de denúncias agrupada por alvo, no padrão de tabela
- Ações: descartar / remover conteúdo (soft delete), ambas encerrando o caso
- Anti-abuso mínimo: uma denúncia por usuário por alvo
- Aba de feedback: formulário de envio (bug / sugestão / elogio) e triagem por status

## Fora do escopo inicial

- Advertência/suspensão de conta (precisa de sistema de punição)
- Automação (auto-ocultar após N denúncias)
- Notificar o denunciante do desfecho
- Responder feedback dentro do app (por ora, o contato sai por fora)

> Decisões já tomadas e componentes existentes em
> [adr/admin/0005-denuncias-agrupadas-por-alvo.md](../../adr/admin/0005-denuncias-agrupadas-por-alvo.md)

## Fronteira com as telas vizinhas

- **[Moderação](./moderation.md)**: julga ponto novo antes de publicar; aqui só entra o que já está público.
- **[Avaliações](./reviews.md)**: mesma ação de remover avaliação, entrada diferente — lá o admin varre por conta própria, aqui ele reage a report. **A remoção precisa ser uma server action só**, consumida pelas duas telas, senão as regras divergem.
- ~~**Moderação do estabelecimento**~~: o dono moderaria comentário nas próprias respostas. Cancelado em 19/08/2026 — ver [`archive/gerenciamento-por-dono/`](../../archive/gerenciamento-por-dono/).
