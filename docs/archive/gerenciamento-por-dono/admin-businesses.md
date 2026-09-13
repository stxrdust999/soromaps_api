# 🏪 Comércios

> Área: admin · Rota: `/admin/businesses` · Status: 🟡 parcial — tela pronta, dados em mock

## Ideia

A listagem administrativa dos estabelecimentos: quem são, qual ponto do mapa cada um reivindica, e o estado da reivindicação. É a tela que transforma "usuário que marcou que tem comércio" em "dono verificado do ponto X" — e sem ela o [Painel do negócio](./todo-business/dashboard.md) não tem como confiar em quem entra.

Também é onde o admin vê os comércios sem dono: pontos de estabelecimento que ninguém reivindicou, que são alvo natural de prospecção.

**A reivindicação se aprova aqui, não na [Moderação](../../todo/admin/moderation.md).** A pergunta das duas telas é diferente: lá é "este ponto merece estar no mapa?", aqui é "esta pessoa é mesmo dona deste ponto?". A primeira julga conteúdo, a segunda julga vínculo — e um ponto já aprovado há meses pode receber um pedido de posse a qualquer momento.

## Por que vale

- Reivindicação de ponto é a única porta de entrada do lado B2B. Sem verificação, qualquer um se declara dono de qualquer lugar e responde avaliação em nome dele.
- É o par administrativo do [Meu ponto](./todo-business/place.md): o dono pede, o admin concede.
- Reusa o padrão de listagem inteiro (`useTableConfig` + `src/components/table/*`) que já roda em `/admin/users` — é `columns.tsx` + `table.tsx`.

## Dependências

| O quê | Situação |
|---|---|
| `tipoUsuario` + `CNPJ` em `tbUsuario` | ❌ a distinção normal/estabelecimento não existe no banco |
| FK `UsuarioDono` em `markers` | ❌ |
| Estado da reivindicação (`pendente`/`aprovada`/`recusada`) | ❌ entidade ou coluna a definir |
| Papel de admin checado na API | 🔴 pré-requisito — hoje qualquer sessão acessa `/admin` |
| Padrão de listagem | 🟢 pronto, portado em `/admin/users` |

## Escopo inicial

- Tabela de estabelecimentos com busca, filtro por estado e paginação
- Detalhe da reivindicação: dados do usuário, ponto pedido, mini-mapa
- Aprovar / recusar com motivo (server actions + `updateTag`)

## Fora do escopo inicial

- Rede com múltiplos pontos por dono
- Planos pagos e cobrança
- Verificação automática por CNPJ em base externa

> Decisões já tomadas e componentes existentes em
> [adr/admin/0006-comercios-tres-abas.md](./0006-comercios-tres-abas.md)
