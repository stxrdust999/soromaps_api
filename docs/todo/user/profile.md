# 👤 Perfil

> Área: usuário público · Rota: `/profile` (com as abas `visits`, `favorites`, `achievements`, `stats`) · Status: 🟡 telas prontas, dados em mock

## Ideia

O hub pessoal do explorador: quem ele é na plataforma, o que já fez nela e o
que falta para o próximo passo. Cinco abas sobre o mesmo cabeçalho —
**Visão geral**, [Visitas](./visits.md), [Favoritos](./favorites.md),
[Conquistas](./achievements.md) e [Estatísticas](./stats.md).

Não é o espelho privado do perfil público. `/community/[id]` responde "dá para
confiar nessa pessoa?"; aqui a pergunta é "o que eu já fiz e o que falta?" — e
é por isso que o selo aparece como régua item a item, e a conquista travada
aparece com o quanto falta.

## Por que vale

- É onde a gamificação vira visível para o dono dela. Conquista sem vitrine não
  retém ninguém.
- Fecha o ciclo: o usuário precisa ver o próprio perfil antes de entender o que
  os outros veem dele em [Comunidade](./community.md).
- Identidade já existe de verdade — nome e e-mail viajam no cookie de sessão.

## Dependências

| O quê | Situação |
|---|---|
| Nome, e-mail (cabeçalho) | 🟢 já vêm de `getSession()` |
| `Visita` (timeline, cobertura, gráfico) | ❌ ver [Visitas](./visits.md) |
| `Favorita` (aba de salvos) | ❌ ver [Favoritos](./favorites.md) |
| `Analise` (contador e "minhas avaliações") | ❌ |
| `Conquista` + `GanhaConquista` (galeria) | ❌ ver [Conquistas](./achievements.md) |
| Selo de verificado como dado, não cálculo sobre mock | ❌ critério já publicado em `src/constants/verification.ts` |
| Avatar | ❌ sem upload e sem coluna (RF-08) |
| Data de cadastro | 🟡 existe em `tbUsuario`, mas "desde" ainda sai do mock |

## Fora do escopo

- Edição de dados e preferências — é [Configurações](./settings.md)
- Perfil de outro usuário — é [Comunidade](./community.md)
- Pontuação e nível — descartados em 2026-08-12, não adiados

> Decisões já tomadas e componentes existentes em
> [adr/user/0004-perfil-hub-com-abas.md](../../adr/user/0004-perfil-hub-com-abas.md)

## Distinção que precisa ficar clara

Três telas falam de usuário e é fácil confundir: **`/profile`** é o hub de
quem sou e do que fiz, **`/settings`** é onde altero dados e preferências, e
**`/community/[id]`** é como os outros me veem — com menos campo, sem e-mail.
