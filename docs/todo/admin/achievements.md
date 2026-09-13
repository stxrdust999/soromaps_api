# 🎖️ Conquistas — catálogo

> Área: admin · Rota: `/admin/achievements` · Status: 🟡 parcial — tela pronta, dados em mock

## Ideia

CRUD do catálogo de conquistas: nome, descrição, ícone e — a parte que importa — o **critério declarativo**: tipo de evento (`visita`, `avaliacao`, `seguidores`…), alvo opcional (categoria X, bairro Y) e quantidade. Criar "Visite 5 cafeterias" é preencher três campos, não escrever código.

É o irmão admin do módulo [Conquistas do usuário](../user/achievements.md): a decisão de modelagem do TCC (catálogo `Conquista` + N:N `GanhaConquista`) existe exatamente pra permitir conquista nova **sem migration** — este módulo é o que cobra essa promessa: sem deploy também.

## Por que vale

- Gamificação viva: conquista sazonal ("Inverno em Sorocaba") criada na hora, sem release.
- Critério declarativo força um motor de concessão genérico — arquitetura melhor do que N `if`s espalhados por actions.
- Contador de "quantos usuários têm" por conquista dá feedback imediato de calibragem (conquista que ninguém tira está mal calibrada).

## Dependências

| O quê | Situação |
|---|---|
| Tabela `Conquista` (+ colunas de critério além do modelado no TCC) | ❌ |
| Motor de concessão lendo o catálogo | ❌ — nasce junto com o [módulo do usuário](../user/achievements.md) |
| Eventos-fonte (`Visita`, `Analise`, `Segue`) | ❌ |
| Papel de admin | 🔴 pré-requisito |

## Escopo inicial

- Tela no padrão de tabela + modais
- Form com builder de critério (selects de tipo/alvo/quantidade)
- Ativar/desativar conquista (sem excluir — quem já ganhou mantém)
- Contador de obtenções por conquista

## Fora do escopo inicial

- Critérios compostos ("X e Y")
- Conquistas com janela de tempo
- Upload de ícone próprio (usar lucide por ora)
- **Pontuação/XP por conquista** — descartado na decisão de 2026-08-12 do
  `CLAUDE.md`. Conquista não vale pontos, e o antigo "nível" virou título
  derivado da contagem de conquistas

> Decisões já tomadas e componentes existentes em
> [adr/admin/0004-conquistas-catalogo-trophy-kit.md](../../adr/admin/0004-conquistas-catalogo-trophy-kit.md)
