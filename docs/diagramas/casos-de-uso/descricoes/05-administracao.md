# 05. Administração — descrições

Diagramas em [`baixo-nivel/05-administracao.md`](../baixo-nivel/05-administracao.md).
Requisitos em [`requisitos-funcionais/05-administracao.md`](../../requisitos-funcionais/05-administracao.md).

> ⚠️ Toda pré-condição desta página deveria incluir "o ator tem papel de
> administrador". Nenhuma inclui, porque o sistema não verifica: qualquer
> sessão válida abre `/admin` (🔴 `RF-ADM-37`).

---

## RF-ADM-05 · Moderar ponto da fila

| Nome do Caso de Uso | Moderar ponto da fila |
|---|---|
| Caso de Uso Geral | Moderar conteúdo |
| Ator Principal | Administrador |
| Ator Secundário | — |
| Resumo | Fila mestre-detalhe onde a decisão é a própria tela: aprovar, rejeitar com motivo ou tratar como duplicata, sem sair da lista. |
| Pré-Condição | Sessão válida. Existir ponto pendente na fila. |
| Pós-Condição | Item fora da fila e registrado no histórico. **Nada persiste** — falta a coluna `status` em `markers`. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/admin/moderation` | |
| | 2. Carregar os pontos pendentes e ordenar por tempo de espera |
| 3. Selecionar um item da fila | |
| | 4. Abrir o detalhe ao lado, mantendo a fila visível |
| | 5. Exibir os campos enviados, o autor com o histórico dele e o bairro no mini-mapa SVG |
| 6. Clicar em "Aprovar" | |
| | 7. Tirar o item da fila e gravar a decisão no histórico |
| | 8. Mover a seleção para o próximo item |
| 6a. Clicar em "Rejeitar" | |
| | 6b. Exigir um motivo antes de aceitar a decisão, e só então registrar |
| 6c. Clicar em "Comparar duplicata" | |
| | 6d. Abrir o diálogo lado a lado com o ponto existente, de onde também dá para rejeitar |
| 6e. Selecionar vários itens | |
| | 6f. Aplicar a mesma decisão a todos de uma vez |
| 6g. Operar pelo teclado | |
| | 6h. Percorrer a fila e disparar as decisões sem mouse |
| | 2a. Se a fila estiver vazia, exibir o estado vazio com atalho para o histórico |
| Restrições / Validação | Rejeição sem motivo não é aceita (🟢) · A decisão não pode exigir sair da fila (🟢, mestre-detalhe) · Toda decisão vira linha de histórico (🟡, sobre mock) · O ponto só deveria aparecer no mapa depois de aprovado (🔴: não há `status`, o ponto entra publicado) · Só administrador deveria moderar (🔴 `RF-ADM-37`) |

> ⚠️ **Lacunas.** Faltam o passo inicial de **verificar o papel** e o passo
> final de **gravar a decisão**. Sem o primeiro a tela é pública; sem o segundo
> ela não modera nada.

---

## RF-ADM-17 · Remover conteúdo denunciado

| Nome do Caso de Uso | Remover conteúdo denunciado |
|---|---|
| Caso de Uso Geral | Moderar conteúdo |
| Ator Principal | Administrador |
| Ator Secundário | — |
| Resumo | Trata a fila de denúncias agrupada por alvo e remove o conteúdo com um motivo do catálogo. O mesmo caso atende `/admin/reports` e `/admin/reviews`. |
| Pré-Condição | Sessão válida. Existir denúncia aberta. |
| Pós-Condição | Conteúdo marcado como removido e denúncias do alvo encerradas. **Nada persiste.** |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/admin/reports` | |
| | 2. Agrupar as denúncias pelo alvo, para o mesmo conteúdo virar um item só |
| | 3. Calcular e exibir o selo de denúncia coordenada onde o padrão indicar |
| 4. Abrir um alvo | |
| | 5. Renderizar o conteúdo conforme o tipo — avaliação, comentário ou ponto |
| 6. Clicar em "Remover" | |
| | 7. Abrir o diálogo com os motivos de `REMOVAL_REASONS` |
| 8. Escolher o motivo e confirmar | |
| | 9. Aplicar a remoção e encerrar as denúncias daquele alvo |
| 6a. Concluir que não infringe | |
| | 6b. Manter o conteúdo e encerrar as denúncias como improcedentes |
| 10. Abrir a aba de feedback | |
| | 11. Exibir a triagem de feedback, separada da fila de denúncia |
| Restrições / Validação | Remoção exige motivo do catálogo, nunca texto livre (🟢 `RNF-USA-06`) · O catálogo e o diálogo são compartilhados com `/admin/reviews`, para as duas telas não divergirem (🟢) · Quem removeu, quando e por quê deveria ficar registrado (🔴 `RNF-OPE-14`) |

---

## RF-ADM-26 · Excluir categoria com reatribuição

| Nome do Caso de Uso | Excluir categoria |
|---|---|
| Caso de Uso Geral | Manter categorias |
| Ator Principal | Administrador |
| Ator Secundário | — |
| Resumo | Remove uma categoria do catálogo exigindo escolher para onde vão os pontos que a usavam. |
| Pré-Condição | Sessão válida. A categoria existe e há ao menos uma outra para receber os pontos. |
| Pós-Condição | Categoria fora do catálogo e pontos reatribuídos. **Nada persiste.** |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/admin/categories` | |
| | 2. Exibir a listagem com o pin renderizado de cada categoria |
| 3. Acionar "Excluir" na linha | |
| | 4. Contar os pontos que usam a categoria |
| | 5. Abrir o diálogo pedindo a categoria de destino |
| 6. Escolher o destino e confirmar | |
| | 7. Reatribuir os pontos e remover a categoria |
| | 5a. Se nenhum ponto usar a categoria, remover sem pedir destino |
| | 6a. Se o ator cancelar, fechar sem efeito |
| Restrições / Validação | Categoria com pontos nunca é removida sem destino — senão o ponto fica órfão (🟢) · Cor repetida entre categorias dispara alerta antes de salvar (🟢 `RF-ADM-25`) · O diálogo é local, não rota interceptada, porque com o catálogo em `useState` a rota leria o mock original (🟢, migra junto com a API) · Falta a tabela `Categoria` (🔴) |

---

## RF-ADM-33 · Criar usuário

| Nome do Caso de Uso | Criar usuário |
|---|---|
| Caso de Uso Geral | Manter usuários |
| Ator Principal | Administrador |
| Ator Secundário | API Soromaps |
| Resumo | Cadastra um usuário pelo painel, em modal de rota interceptada. É o único CRUD de admin sobre dado real. |
| Pré-Condição | Sessão válida. |
| Pós-Condição | Registro criado em `tbUsuario` e tabela já atualizada quando o modal fecha. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/admin/users` | |
| | 2. Buscar `GET /api/users` com a tag de cache e montar a tabela pelo `useTableConfig` |
| 3. Clicar em "Novo usuário" | |
| | 4. Interceptar a rota `/admin/users/create` e abri-la como modal sobre a listagem |
| | 5. Exibir o formulário |
| 6. Preencher nome, e-mail e senha | |
| 7. Salvar | |
| | 8. Validar com Zod na Server Action |
| | 9. Enviar `POST /api/users` |
| | 10. Invalidar `USERS_LIST_TAG` com `updateTag` |
| | 11. Fechar o modal, com a tabela já refletindo o novo registro |
| | 8a. Se a validação falhar, devolver os erros por campo sem fechar o modal |
| | 9a. Se a API recusar, exibir mensagem genérica em toast |
| 12. Acessar a URL do modal direto ou dar F5 | |
| | 13. Servir a rota espelho, que renderiza a mesma tela em página cheia |
| Restrições / Validação | Modal é rota: URL compartilhável, botão voltar e F5 no lugar certo (🟢 `RNF-USA-08`) · A invalidação usa `updateTag`, que dá *read-your-own-writes* dentro da action — é o que substituiu o `router.refresh()` (🟢) · A resposta da API não deveria trazer `user_password` (🔴 `RNF-SEG-10`) · Nome e e-mail deveriam ser únicos (🔴 `RNF-SEG-13`) · Só administrador deveria criar (🔴 `RF-ADM-37`) |

> ⚠️ **Lacunas.** O caso irmão **Editar usuário** carrega um passo que não
> deveria existir: reinformar a senha. `PUT /api/users/{id}` re-hasheia sempre,
> então alterar só o e-mail obriga a redigitar a senha (🔴 `RF-ADM-36`).

---

⬅️ [README das descrições](./README.md) · ⬆️
[Catálogo de casos](../05-catalogo.md)
