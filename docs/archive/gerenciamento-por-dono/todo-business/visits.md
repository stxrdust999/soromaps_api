# 📍 Visitas

> Área: estabelecimento · Rota: `/business/visits` · Status: 💤 não iniciado

## Ideia

Espelho de [Minhas visitas](../../../todo/user/visits.md), visto do lado do dono: lista de quem passou pelo ponto, quando, e se avaliou na ocasião. Agrupado por semana, com um resumo no topo ("32 visitas · 8 avaliações · pico às sextas").

Não é timeline pessoal — é tráfego. A pergunta que responde é "meu ponto está sendo procurado?", não "o que esse usuário fez". Reusa a mesma tabela `Visita` do lado do explorador; a diferença é o filtro (`PontoID = meu ponto`) e a agregação.

## Por que vale

- É o dado bruto do card "visitas na semana" no [Painel do negócio](./dashboard.md) — sem esta tela o número do dashboard não tem onde se aprofundar.
- Fecha o RF-10 do lado do estabelecimento: estatística de uso não é só do usuário que visita, é também de quem é visitado.
- Diferencia visita de avaliação: mostra que gente passa e não avalia — sinal que falta em qualquer analytics genérico de review.

## Dependências

| O quê | Situação |
|---|---|
| Tabela `Visita` (mesma de [user/visits](../../../todo/user/visits.md)) | ❌ não existe — já modelada no TCC |
| `tipoUsuario` + FK `UsuarioDono` em `markers` (qual ponto é meu) | ❌ |
| [Painel do negócio](./dashboard.md) (navegação-mãe) | 💤 |
| Autenticação na API | 🔴 pré-requisito |

## Escopo inicial

- Lista de visitas do ponto, com data e indicação de avaliação vinculada (se houver)
- Agrupamento por semana + resumo no topo
- Gráfico simples de volume por dia da semana

## Fora do escopo inicial

- Comparativo com outros pontos/categoria
- Exportar CSV
