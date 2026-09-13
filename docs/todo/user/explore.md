# 🧭 Descobrir

> Área: usuário público · Rota: `/discover` · Status: 🟡 tela pronta sobre `src/mocks/markers.ts`

## Ideia

A tela onde se navega a cidade sem destino definido. Trilhas horizontais sobre
a mesma lista de pontos, cada uma com um critério diferente — **Perto de
você**, **Em alta**, **Nota máxima da galera**, **Joias escondidas** e
**Recém-adicionados** — mais dois eixos de recorte no topo: chips de
**categoria** e chips de **vibe**.

O recorte por vibe é a parte que não existe em app de review comum: categoria
responde "o que é", vibe responde "pra quê". Um bar e um parque podem servir à
mesma noite com a galera, e é assim que as pessoas decidem — não por taxonomia.

## Fusão com `/places` — 2026-08-17

Esta tela **era** `/places` ("Explorar lugares"). `/discover` existia em
paralelo no papel, prometendo recomendação derivada do histórico de visitas.

As duas respondiam à mesma pergunta do usuário — "que lugar vale hoje?" — com
critérios diferentes, e critério é seção, não rota. A personalização prevista
para `/discover` entra aqui como **mais uma trilha** ("Você esteve aqui" e o
que combina com isso), no dia em que `Visita` existir. Enquanto isso, a tela já
funciona com os critérios públicos.

O que sobrou de `/places` é o que tem prefixo próprio de verdade:
`/places/[id]` (detalhe do ponto) e `/places/new` (criação). `/places` sozinha
redireciona para cá, porque a URL já tinha sido publicada.

Documento original em
[`docs/archive/telas-fundidas/places.md`](../../archive/telas-fundidas/places.md).

## Por que vale

- Ataca a dor nº 1 (descobrir lugares novos) por um caminho mais escaneável que
  o mapa, que exige saber onde procurar antes de procurar.
- É a página com melhor SEO em potencial: o mapa é um canvas opaco para
  crawler; cards com nome, categoria e bairro indexam.
- "Joias escondidas" inverte o viés de popularidade que todo agregador tem — dá
  visibilidade a estabelecimento pequeno, que é a tese do produto.
- Com `Visita`, vira a única tela em que duas pessoas veem coisas diferentes
  pelo histórico — o feed faz isso pelo vínculo com lugar, não pelo que você
  já visitou.

> Decisões e componentes da versão atual em
> [adr/user/0001-explorar-lugares-sobre-mock.md](../../adr/user/0001-explorar-lugares-sobre-mock.md)

## Dependências para sair do parcial

| O quê | Situação |
|---|---|
| Foto, descrição e categoria no ponto | ❌ colunas ausentes em `markers` — spec em `docs/propostas/2026-08-03-expansao-modelo-ponto.md` |
| `Categoria` como tabela (hoje os chips são constante no front) | ❌ |
| `tags` no modelo do ponto (alimentam vibe e recomendação) | ❌ só existem no mock |
| `Analise` — nota e total de avaliações | ❌ |
| `Visita` — "Em alta" real e a trilha personalizada | ❌ hoje "Em alta" é nota, não movimento |
| Geolocalização do usuário para a distância real | ❌ `distancia` é número fixo no mock |
| `segredoLocal` / `melhorHorario` persistidos | ❌ previstos na proposta do modelo de Ponto |
| Paginação / bounding box em `GET /api/markers` | ❌ a rota devolve a tabela inteira |

## Escopo restante

- Trilha **"Você esteve aqui"** com as visitas recentes, e recomendação por
  proximidade de atributo (mesma categoria ou tag do que já foi visitado,
  excluindo o que já foi) — é o que `/discover` prometia
- Trilha de **segredos locais** dos lugares recomendados: é o tipo de
  informação que não cabe em ranking
- Trocar os critérios fictícios pelos reais conforme as tabelas nascerem
- Sheet de filtro por `tags`, que os chips não cobrem
- Estado vazio por trilha (hoje a trilha some inteira quando não há item)
- Skeleton com `Suspense`, quando a busca deixar de ser uma chamada só

## Fora do escopo

- Recomendação por comportamento coletivo ("quem foi aqui também foi ali") —
  precisa de massa de dados
- Peso configurável entre os critérios
- Listagem administrativa de pontos, com aprovação — é
  [Moderação](../admin/moderation.md)

## Fronteira com as telas vizinhas

- **Home (`/home`)**: o painel sobre o mapa mostra um recorte — "Perto de você"
  e uma prévia de "Em alta" com link para cá. Proximidade só faz sentido colada
  ao mapa.
- **[Feed](./feed.md) (`/feed`)**: aqui o eixo é catálogo (o que existe, como
  filtrar); lá é cronologia (o que mudou). O feed também personaliza, mas por
  vínculo com lugar — bairro, lugar acompanhado, categoria —, não pelo
  histórico de visitas.
