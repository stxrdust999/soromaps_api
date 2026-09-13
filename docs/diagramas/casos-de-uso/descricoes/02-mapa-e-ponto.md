# 02. Mapa e ponto — descrições

Diagramas em [`baixo-nivel/02-mapa-e-ponto.md`](../baixo-nivel/02-mapa-e-ponto.md).
Requisitos em [`requisitos-funcionais/02-mapa-e-ponto.md`](../../requisitos-funcionais/02-mapa-e-ponto.md).

---

## RF-MAP-01 · Explorar o mapa

| Nome do Caso de Uso | Explorar o mapa |
|---|---|
| Caso de Uso Geral | Acessar mapa |
| Ator Principal | Explorador |
| Ator Secundário | CARTO basemaps · API Soromaps |
| Resumo | Tela principal do produto: mapa de Sorocaba em tela cheia, com os marcadores carregando conforme o zoom e um painel arrastável sobre ele. |
| Pré-Condição | Sessão válida. |
| Pós-Condição | Mapa renderizado no tema atual, com os marcadores visíveis se o zoom estiver em 14 ou acima. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/home` | |
| | 2. Montar a instância MapLibre centrada em `SOROCABA_VIEWPORT` |
| | 3. Carregar o basemap CARTO conforme o tema — `positron` no claro, `dark-matter` no escuro |
| | 4. Renderizar o painel arrastável recolhido sobre o mapa |
| 5. Arrastar o mapa ou dar zoom | |
| | 6. Aguardar o evento `moveend` — o fim do gesto, não cada quadro dele |
| | 7. Comparar `zoom >= 14` com o estado anterior |
| | 8. Se o limiar foi cruzado para cima, buscar `GET /api/markers` e desenhar os pinos |
| | 7a. Se o limiar foi cruzado para baixo, limpar os marcadores da tela |
| | 7b. Se o limiar não mudou, não fazer requisição nenhuma |
| | 8a. Se houver busca em andamento, abortá-la antes de disparar a nova |
| | 8b. Se a busca falhar, registrar em `console.error` e manter o mapa utilizável |
| 9. Trocar o tema | |
| | 10. Trocar o basemap sem recarregar a página e sem desmontar o mapa |
| Restrições / Validação | Um gesto de zoom não pode gerar mais de uma requisição por cruzamento de limiar (🟢 `RNF-DES-01`) · O mapa nunca é desmontado durante a navegação, nem ao expandir o painel (🟢 `RNF-DES-06`) · O viewport não é estado React (🟢 `RNF-DES-07`) · A listagem não deveria trazer a tabela inteira (🔴 `RNF-DES-05`) |

> ⚠️ **Lacunas.** O passo 8 traz **todos** os marcadores em toda chamada — não
> há paginação nem recorte por área visível. E o erro do passo 8b morre no
> console do navegador do usuário (🔴 `RNF-OPE-13`).

---

## RF-PTO-01 · Cadastrar ponto

| Nome do Caso de Uso | Cadastrar ponto |
|---|---|
| Caso de Uso Geral | Manter ponto |
| Ator Principal | Explorador |
| Ator Secundário | API Soromaps |
| Resumo | Cria um ponto no mapa em dois estágios dentro do mesmo card flutuante: primeiro a posição, depois a ficha. O mapa fica visível o tempo todo. |
| Pré-Condição | Sessão válida. |
| Pós-Condição | Registro criado em `markers` com `nome`, `lat` e `lng`; cache da lista invalidado. Os outros cinco campos preenchidos **não são gravados**. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/places/new` | |
| | 2. Abrir a tela no estágio `picking`, com o mapa cheio e um pino arrastável |
| 3. Arrastar o pino até o lugar desejado | |
| 4. Confirmar a posição | |
| | 5. Ler `lat` e `lng` do pino e passar ao estágio `form`, **sem escurecer o mapa** |
| 6. Preencher nome, foto, sobre, categoria, wifi, petfriendly, melhor horário e segredo local | |
| 7. Clicar em "Salvar" | |
| | 8. Validar os campos no navegador com o `zodResolver` |
| | 9. Converter `lat` e `lng` para número em `toMarkerInput` |
| | 10. Revalidar no servidor com `createMarkerSchema` |
| | 11. Enviar apenas `nome`, `lat` e `lng` para `POST /api/markers` |
| | 12. Invalidar `MARKERS_LIST_TAG`, avisar em toast e sair da tela |
| 6a. Clicar em "Trocar de lugar" | |
| | 6b. Voltar ao estágio `picking` **mantendo tudo que já foi preenchido** |
| | 8a. Se algum campo for inválido, exibir a mensagem no campo e não enviar |
| | 10a. Se a revalidação falhar, devolver `FormState` com os erros por campo |
| | 11a. Se a API responder fora da faixa 2xx, exibir mensagem genérica em toast e não tocar o cache |
| Restrições / Validação | O mapa não pode escurecer durante o preenchimento — ver a posição escolhida é o que comunica que dá para voltar e mudar (🟢) · A conversão para número mora na action, não no schema, porque `z.coerce.number()` quebra o `zodResolver` (🟢) · Campo coletado deveria ser campo gravado (🔴 `RNF-DAD-16`) · O ponto deveria ter dono (🔴 `RNF-SEG-09`) |

> ⚠️ **Lacunas.** Faltam dois passos entre o 11 e o 12: **gravar o autor** e
> **marcar o ponto como pendente**. Sem o primeiro não há como autorizar por
> autoria; sem o segundo o ponto entra publicado direto no mapa, e
> `/admin/moderation` não tem o que moderar.

---

## RF-PTO-04 · Editar ponto

| Nome do Caso de Uso | Editar ponto |
|---|---|
| Caso de Uso Geral | Manter ponto |
| Ator Principal | Administrador — **na prática, qualquer sessão** |
| Ator Secundário | API Soromaps |
| Resumo | Altera os dados de um ponto pela página de detalhe. O card do marcador no mapa é só display desde 2026-08-03. |
| Pré-Condição | Sessão válida. O ponto existe. |
| Pós-Condição | Registro atualizado; cache do item e da lista invalidados. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Abrir `/places/[id]` | |
| | 2. Buscar o ponto com a tag `markerShowTag(id)` e exibir a ficha |
| 3. Clicar em "Editar" | |
| | 4. Exibir o formulário preenchido com os dados atuais |
| 5. Alterar os campos e salvar | |
| | 6. Validar com `updateMarkerSchema` |
| | 7. Enviar `PUT /api/markers/{id}` |
| | 8. Invalidar `markerShowTag(id)` **e** `MARKERS_LIST_TAG` |
| | 9. Avisar em toast e exibir a ficha atualizada |
| | 6a. Se a validação falhar, devolver os erros por campo |
| | 7a. Se a API recusar, exibir mensagem genérica e manter o formulário |
| Restrições / Validação | A escrita invalida as duas tags, para o mapa e a página não discordarem (🟢) · Só administrador deveria editar (🔴 `RF-PTO-06`) · Só `nome`, `lat` e `lng` existem para editar (🔴 `RNF-DAD-16`) |

---

## RF-PTO-05 · Excluir ponto

| Nome do Caso de Uso | Excluir ponto |
|---|---|
| Caso de Uso Geral | Manter ponto |
| Ator Principal | Administrador — **na prática, qualquer sessão** |
| Ator Secundário | API Soromaps |
| Resumo | Remove um ponto do mapa em definitivo, com confirmação. |
| Pré-Condição | Sessão válida. O ponto existe. |
| Pós-Condição | Registro removido de `markers`; cache do item e da lista invalidados; navegador fora de `/places/[id]`. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Abrir `/places/[id]` | |
| 2. Clicar em "Excluir" | |
| | 3. Abrir o diálogo de confirmação com o nome do ponto |
| 4. Confirmar | |
| | 5. Enviar `DELETE /api/markers/{id}` |
| | 6. Invalidar `markerShowTag(id)` e `MARKERS_LIST_TAG` |
| | 7. Avisar em toast e sair da página |
| | 4a. Se o ator cancelar, fechar o diálogo sem efeito |
| | 5a. Se a API recusar, exibir mensagem genérica e manter o ponto |
| Restrições / Validação | Ação destrutiva exige confirmação explícita (🟢 `RNF-USA-05`) · Só administrador deveria excluir (🔴 `RF-PTO-06`) · Não há exclusão lógica: o registro some, sem trilha de quem apagou (🔴 `RNF-OPE-14`) |

> ⚠️ **Lacunas.** Nenhum passo verifica papel, e nenhum registra a decisão. Um
> ponto excluído por engano não tem como ser recuperado nem rastreado.

---

➡️ [03 — Descoberta e feed](./03-descoberta-e-feed.md)
