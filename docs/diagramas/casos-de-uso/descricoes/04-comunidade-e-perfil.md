# 04. Comunidade, pauta e perfil — descrições

Diagramas em [`baixo-nivel/04-comunidade-e-perfil.md`](../baixo-nivel/04-comunidade-e-perfil.md).
Requisitos em [`requisitos-funcionais/04-comunidade-e-perfil.md`](../../requisitos-funcionais/04-comunidade-e-perfil.md).

---

## RF-COM-01 · Buscar exploradores

| Nome do Caso de Uso | Buscar exploradores |
|---|---|
| Caso de Uso Geral | Participar da comunidade |
| Ator Principal | Explorador |
| Ator Secundário | — |
| Resumo | Encontra outros exploradores pelo nome e abre o perfil público de quem contribui na cidade. Não há botão de seguir. |
| Pré-Condição | Sessão válida. |
| Pós-Condição | Lista filtrada em tela, ou perfil público aberto. Nada é persistido. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/community` | |
| | 2. Carregar os exploradores com os contadores de cada um |
| | 3. Aplicar `isVerifiedExplorer` aos contadores e marcar quem passou na régua |
| | 4. Derivar o título de cada um pela contagem de conquistas |
| 5. Digitar um nome na busca | |
| | 6. Filtrar a lista enquanto se digita |
| 7. Abrir um explorador | |
| | 8. Navegar para `/community/[id]` com contribuição, selo e lugares |
| | 6a. Se nada casar, exibir o estado vazio |
| Restrições / Validação | O selo é função pura sobre contadores, nunca chancela manual (🟢 `RF-COM-04`) · O título vem de `COUNT(conquistas)`, sem XP nem coluna `nivel` (🟢, decisão de 2026-08-12) · O perfil público não expõe e-mail (🟡 `RNF-DAD-22`, não verificado sobre dado real) · Não existe seguir (🟢, `Segue` cancelado) |

---

## RF-COM-06 · Gerar rascunho de pauta

| Nome do Caso de Uso | Gerar rascunho de pauta |
|---|---|
| Caso de Uso Geral | Publicar pauta |
| Ator Principal | Explorador — **deveria ser um papel editorial** (`RF-COM-12`) |
| Ator Secundário | Gemini API (`gemini-2.5-flash`, sobrescrito por `GEMINI_MODEL`) |
| Resumo | Pede ao modelo um rascunho de pauta sobre lugares escolhidos na tela. Geração é etapa de autoria: nada roda durante a navegação do leitor. |
| Pré-Condição | Sessão válida. `GEMINI_API_KEY` configurada no servidor — sem ela o caso termina no aviso de desligado. |
| Pós-Condição | Rascunho devolvido à tela com o nome do modelo e o aviso de revisar. **Nada é persistido**: não existe entidade de pauta. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Abrir o gerador no cabeçalho da vitrine de pautas | |
| | 2. Exibir o campo de tema e a lista de lugares para marcar |
| 3. Informar o tema | |
| 4. Marcar os lugares que a pauta vai cobrir | |
| 5. Clicar em "Gerar rascunho" | |
| | 6. Validar tema e ids com `generateStorySchema` |
| | 7. Montar a ficha de fatos de cada lugar, na ordem marcada |
| | 8. Chamar o modelo com a instrução de sistema, o prompt com as fichas, o `responseSchema` e temperatura 0.4 |
| | 9. Revalidar o JSON devolvido com `storyDraftSchema` |
| | 10. Exibir o rascunho, o nome do modelo e o aviso de revisar antes de publicar |
| 11. Copiar o texto para publicar à mão | |
| | 6a. Se o tema estiver vazio ou nenhum lugar marcado, exibir erro por campo sem chamar o modelo |
| | 7a. Se algum id não virar ficha, recusar **antes** da chamada — o modelo nunca recebe lista incompleta |
| | 8a. Se `GEMINI_API_KEY` faltar, avisar que a geração está desligada e manter a tela funcionando |
| | 8b. Se estourar 30s, falhar a rede ou a API recusar, exibir a mensagem de falha |
| | 9a. Se o JSON vier fora do schema, exibir "formato inesperado, gere de novo" e não mostrar nada |
| Restrições / Validação | O modelo não escolhe o assunto: recebe lista fechada, na ordem dada (🟢) · É proibido inventar preço, horário, endereço, telefone, prato, pessoa, prêmio ou história — enumerado na instrução de sistema (🟢) · Saída de modelo é entrada não confiável, revalidada com Zod (🟢 `RNF-DAD-04`) · A chave nunca vai para o bundle (🟢 `RNF-SEG-11`) · Só quem publica deveria gerar (🔴 `RF-COM-12`) · A geração deveria ser auditável (🔴 `RNF-DAD-10`) |

> ⚠️ **Lacunas.** Falta o passo de **gravar o rascunho** com
> `status: "rascunho"`, `slug`, `origem` e `updateTag` — hoje fechar a aba perde
> o texto, e o passo 11 é copiar à mão. E o gerador está aberto a qualquer
> sessão, queimando cota de um modelo pago a cada clique.

---

## RF-PER-01 · Ver o próprio perfil

| Nome do Caso de Uso | Ver o próprio perfil |
|---|---|
| Caso de Uso Geral | Configurar perfil |
| Ator Principal | Explorador |
| Ator Secundário | — |
| Resumo | Hub pessoal em cinco abas de rota: visão geral com a régua do selo, visitas, favoritos, conquistas e estatísticas. Responde "o que falta para o próximo passo?", não "dá para confiar nessa pessoa?" — essa é a pergunta do perfil público. |
| Pré-Condição | Sessão válida. |
| Pós-Condição | Aba renderizada, com URL própria. Nada é escrito. |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Acessar `/profile` | |
| | 2. Ler nome e e-mail do cookie de sessão |
| | 3. Derivar os números a partir das mesmas 24 visitas que alimentam o ranking público |
| | 4. Montar as cinco abas de `PROFILE_TABS` |
| | 5. Exibir a visão geral com a régua do selo item a item, marcando o que falta |
| 6. Abrir "Visitas" | |
| | 7. Agrupar as visitas por mês, formatando em UTC |
| 8. Abrir "Conquistas" | |
| | 9. Exibir as obtidas com a data e as travadas com o quanto falta |
| 10. Abrir "Estatísticas" | |
| | 11. Exibir o placar e a cobertura da cidade |
| Restrições / Validação | Cada aba é segmento de rota, com URL compartilhável e botão voltar (🟢 `RNF-USA-09`) · O número exibido aqui e no perfil público sai do mesmo cálculo sobre a mesma base (🟢 `RNF-DAD-13`) · Data em mock usa fuso fixo UTC, senão servidor e navegador renderizam dias diferentes e a hidratação quebra (🟢 `RNF-MAN-11`) · Conquista de evento `seguir` não aparece, porque a plataforma não tem seguir (🟢) · Identidade é real; todo o resto é mock (🔴 `RNF-DAD-17`) |

> ⚠️ **Lacunas.** Três ações não existem: **editar os próprios dados** (é
> `/settings`, travado pelo `PUT` que re-hasheia a senha), **remover favorito**
> (nasce com a tabela `Favorita`) e o **mini-mapa da mancha explorada**.

---

➡️ [05 — Administração](./05-administracao.md)
