# 03. Descoberta e feed — descrições

Diagramas em [`baixo-nivel/03-descoberta-e-feed.md`](../baixo-nivel/03-descoberta-e-feed.md).
Requisitos em [`requisitos-funcionais/03-descoberta-e-feed.md`](../../requisitos-funcionais/03-descoberta-e-feed.md).

> Nenhum caso desta página faz requisição: as duas telas rodam sobre
> `src/mocks/markers.ts` e `src/mocks/feed.ts`. Por isso toda pós-condição
> termina no que aparece na tela, não no banco.

---

## RF-DES-01 · Descobrir lugares

| Nome do Caso de Uso | Descobrir lugares |
|---|---|
| Caso de Uso Geral | Buscar ponto |
| Ator Principal | Explorador |
| Ator Secundário | — |
| Resumo | Vitrine da cidade em três trilhas — perto de você, melhor avaliados e recém-adicionados — com busca e filtros que se combinam. |
| Pré-Condição | Sessão válida. |
| Pós-Condição | Cards exibidos conforme o recorte escolhido. **Nada é persistido**; o recorte vive na sessão da aba. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/discover` | |
| | 2. Carregar a lista de lugares |
| | 3. Montar as três trilhas, cada uma com o critério visível |
| 4. Digitar na busca | |
| 5. Marcar chips de categoria e de vibe | |
| | 6. Combinar busca, categoria e vibe — os três se somam, não se substituem |
| | 7. Renderizar os cards que sobraram |
| | 7a. Se nada sobrar, exibir o estado vazio explicando qual filtro remover |
| 8. Clicar em um card | |
| | 9. Navegar para `/places/[id]` |
| Restrições / Validação | Os critérios combinam entre si, porque critério é seção e não rota (🟢) · A trilha "perto de você" deveria ordenar pela posição real (🔴 `RF-MAP-04`: a posição é lida para centrar o mapa e descartada) · Nota e foto vêm de mock (🔴 `RNF-DAD-17`) |

> ⚠️ **Lacunas.** Falta a quarta trilha, a personalizada — "você esteve aqui" e
> recomendação por tag (`RF-DES-06`). É o que justifica o nome da tela, e
> espera a tabela `Visita`.

---

## RF-FED-01 · Ler o feed

| Nome do Caso de Uso | Ler o feed |
|---|---|
| Caso de Uso Geral | Acompanhar a cidade |
| Ator Principal | Explorador |
| Ator Secundário | — |
| Resumo | Linha do tempo de seis tipos de item, montada por vínculo com lugar em vez de por grafo social. Todo card diz por que apareceu. |
| Pré-Condição | Sessão válida. |
| Pós-Condição | Itens exibidos na ordem escolhida. **Nada é persistido**: filtro, ordenação, silêncio e reação vivem só na sessão da aba. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/feed` | |
| | 2. Reunir os itens das cinco fontes — `perto`, `salvo`, `categoria`, `cidade` e `curadoria` |
| | 3. Agregar as avaliações em rajada num item único por lugar |
| | 4. Descartar o que casa com alguma regra de silêncio ativa |
| | 5. Ordenar por relevância |
| | 6. Despachar cada item pelo `kind` para um dos cinco cards |
| | 7. Renderizar cada card dentro da moldura, com o motivo e o menu "ver menos disso" |
| 8. Marcar chips de fonte | |
| | 9. Restringir aos motivos marcados e reordenar, sem nova busca |
| 10. Trocar para a ordenação cronológica | |
| | 11. Reagrupar os itens por faixa de tempo |
| 12. Clicar em "Carregar mais" | |
| | 13. Acrescentar itens mantendo a posição de leitura |
| | 4a. Se as regras removerem tudo, exibir o estado vazio com os chips de regra visíveis para desfazer |
| Restrições / Validação | Todo item declara por que apareceu — `FeedCardFrame` exige `reason` no tipo, então card sem motivo não compila (🟢 `RF-FED-02`) · Rajada é exibida agregada, nunca item a item (🟢) · Não existe seguir, seguidor nem aba "Seguindo" (🟢, `Segue` cancelado em 2026-08-17) · Motivo e relevância deveriam vir do servidor (🔴 `RF-FED-10`) |

> ⚠️ **Lacunas.** `relevancia` é escrita à mão no mock. Quando a consulta
> existir, é ela que sabe o que casou — o front só desenha e deixa corrigir.

---

## RF-FED-08 · Ver menos disso

| Nome do Caso de Uso | Ver menos disso |
|---|---|
| Caso de Uso Geral | Acompanhar a cidade |
| Ator Principal | Explorador |
| Ator Secundário | — |
| Resumo | Silencia um bairro, uma categoria ou um tipo de item a partir do card que incomodou, e mostra a regra criada como chip removível. |
| Pré-Condição | Ler o feed, com pelo menos um item na tela. |
| Pós-Condição | Itens que casam com a regra somem, e a regra aparece como chip com contador no topo. **Não sobrevive a um F5.** |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Abrir o menu do card | |
| | 2. Exibir os três escopos possíveis, preenchidos com os valores daquele card |
| 3. Escolher bairro, categoria ou tipo | |
| | 4. Criar a regra de silêncio |
| | 5. Remover da lista os itens que casam com ela |
| | 6. Exibir a regra como chip no topo, com o contador do que foi escondido |
| 7. Clicar no X do chip | |
| | 8. Remover a regra e trazer os itens de volta na mesma renderização |
| Restrições / Validação | Filtro criado pelo usuário precisa ser visível e reversível (🟢 `RNF-USA-07`) · A regra deveria ser persistida por conta (🔴: é estado de componente, some no recarregamento) |

> ⚠️ **Lacunas.** É o recurso que mais perde por não persistir: silenciar algo
> serve para o uso repetido, e recarregar a página desfaz tudo.

---

## RF-FED-07 · Acompanhar lugar

| Nome do Caso de Uso | Acompanhar lugar |
|---|---|
| Caso de Uso Geral | Acompanhar a cidade |
| Ator Principal | Explorador |
| Ator Secundário | — |
| Resumo | Marca um lugar como acompanhado, o que passa a alimentar o motivo `salvo` do feed. Substitui "seguir usuário", que saiu do produto. |
| Pré-Condição | Sessão válida, com o lugar visível num card. |
| Pós-Condição | Botão alternado para "Acompanhando" e aviso em toast. **Nada é persistido.** |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Clicar em "Acompanhar lugar" | |
| | 2. Alternar o estado do item no array em memória |
| | 3. Trocar o rótulo do botão para "Acompanhando" |
| | 4. Avisar em toast que o lugar passou a ser acompanhado |
| 5. Clicar de novo | |
| | 6. Desfazer e avisar "Você parou de acompanhar" |
| Restrições / Validação | Acompanhar é sobre lugar, nunca sobre pessoa (🟢, decisão de 2026-08-17) · O vínculo deveria virar linha em `Favorita` (🔴, tabela inexistente) |

---

➡️ [04 — Comunidade e perfil](./04-comunidade-e-perfil.md)
