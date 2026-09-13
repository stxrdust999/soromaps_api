# 📰 Feed

> Área: usuário público · Rota: `/feed` · Status: 🟡 parcial — tela pronta, dados em mock

## Ideia

Página de atividade **sem grafo social**: não existe seguir, seguidor nem aba
"Seguindo". Todo item entra por um vínculo com a cidade, e o card diz qual foi.

Cinco fontes, cada uma virando um chip de filtro:

| Motivo | Entra quando | Deriva de |
|---|---|---|
| `perto` | o lugar está no seu bairro ou no raio configurado | `lat`/`lng` do ponto + posição do usuário |
| `salvo` | o lugar é um que você acompanha | `Favorita` |
| `categoria` | é da categoria/vibe que você mais explora | `Visita` + `Analise` agregadas |
| `cidade` | é movimento relevante em Sorocaba | qualquer evento público |
| `curadoria` | é roteiro montado pela equipe | conteúdo editorial |

## Por que vale

- **O cold start deixa de existir.** Feed de grafo abre vazio para quem acabou
  de chegar; feed de cidade abre cheio no primeiro acesso e vai ficando pessoal
  conforme a pessoa salva lugar e registra visita.
- **Não precisa de tabela nova.** `Segue` seria auto-relacionamento N:N só para
  o feed; os cinco motivos saem de dados que o produto já precisa ter.
- **Tira do escopo a moderação de vínculo social** — bloqueio, perfil privado,
  denúncia de perseguição.
- **Mantém o assunto em lugar, não em gente**, que é a tese do produto.
- É onde o comentário-com-resposta (RF-09) ganha palco, quando existir.

## Dependências

| O quê | Situação |
|---|---|
| `Analise` (avaliação, rajada, marco) | ❌ não existe — já modelada no TCC |
| `Visita` (rajada de visitas, motivo `categoria`) | ❌ |
| `Favorita` (motivo `salvo` e o "acompanhar lugar") | ❌ |
| `GanhaConquista` (item de conquista) | ❌ |
| Coluna `status` em `markers` (ponto novo sai da moderação) | ❌ |
| Posição do usuário / bairro no perfil (motivo `perto`) | ❌ |
| Entidade de pauta editorial (item de curadoria) | ❌ nem modelada |
| `Segue` | 🚫 fora de escopo por decisão |

## Escopo restante

- `motivo` e `relevancia` vindos do backend — é a consulta que sabe o que casou
- Agregação de rajada como `GROUP BY` no servidor (mesmo tipo, mesmo lugar,
  mesma janela), não na tela
- Paginação por cursor (`created_at` + id) no lugar do "Carregar mais" que
  fatia array
- "Ver menos disso" como preferência persistida; hoje vive só na sessão
- Recorte do feed no painel da home ("3 novidades — ver feed")
- Migrar os cinco lugares que ainda mostram "Nível N" para `explorerCredential`

## Fora do escopo

- Seguir usuário, seguidores, perfil privado — não vão existir
- Comentar item direto na lista: a conversa mora na avaliação
- Notificação de atividade — é o módulo [Notificações](./notifications.md)
- Publicar do próprio feed: registrar visita e avaliar continuam em
  `/places/[id]`

> Decisões já tomadas e componentes existentes em
> [adr/user/0002-feed-sem-grafo-social.md](../../adr/user/0002-feed-sem-grafo-social.md)

## Fronteira com as telas vizinhas

- **[Descobrir](./explore.md) (`/discover`)**: lá o eixo é catálogo (o que
  existe, como filtrar); aqui é cronologia (o que mudou). Os dois personalizam,
  mas por critérios diferentes — vínculo com lugar aqui, histórico de visita lá.
- **[Comunidade](./community.md) (`/community`)**: as pautas aparecem nos dois,
  como card de curadoria aqui e como vitrine lá, e as duas telas levam à mesma
  rota `/pautas/[slug]`.
