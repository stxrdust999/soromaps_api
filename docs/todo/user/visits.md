# 📍 Minhas visitas

> Área: usuário público · Rota: `/profile/visits` (aba de [Perfil](./profile.md)) · Status: 🟡 tela pronta sobre `src/mocks/profile.ts` — falta `Visita`

## Ideia

Timeline pessoal do uso da cidade: cada check-in vira um item com data, lugar e (se houver) a avaliação feita na ocasião. Agrupado por mês, com um resumo no topo ("14 lugares · 5 bairros · 3 categorias").

Visita é **evento repetível** (decisão do TCC: `data` na PK de `Visita`) — o mesmo lugar aparece quantas vezes foi visitado, e é isso que torna a timeline honesta: ela conta a história real, não uma lista deduplicada.

## Por que vale

- É a matéria-prima da gamificação: conquistas ("5 cafés visitados") e estatísticas derivam daqui.
- Check-in validado por GPS (proximidade real do ponto) é o que sustenta "avaliação verificada — esteve lá", atacando a dor nº 2 (confiança) na raiz.
- Memória afetiva é retenção: rever "onde eu estava em março" traz o usuário de volta sem push.

## Dependências

| O quê | Situação |
|---|---|
| Tabela `Visita` (`data`, `PontoID`, `UsuarioID` — PK composta) | ❌ não existe — já modelada no TCC |
| Check-in no front (botão no popup + validação de proximidade via `geolocation`) | ❌ |
| Autenticação na API | 🔴 pré-requisito (dado por usuário) |

## Escopo inicial

- Botão "fazer check-in" no popup do marcador, habilitado só a < N metros (Haversine no client, revalidado no server)
- `/visits` com timeline agrupada por mês
- Contador de visitas no perfil

## Fora do escopo inicial

- Check-in retroativo/manual (abre porta pra fraude — decidir depois)
- Streak de dias consecutivos

> A rota antiga redireciona para a aba desde 2026-08-19. Decisões de
> implementação em [adr/user/0004-perfil-hub-com-abas.md](../../adr/user/0004-perfil-hub-com-abas.md)
