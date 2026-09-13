# 🏷️ Categorias

> Área: admin · Rota: `/admin/categories` · Status: 🟡 parcial — tela pronta, dados em mock

## Ideia

CRUD do catálogo de categorias de ponto (restaurante, café, bar, parque, cultura…): nome, ícone e cor — os dois últimos são o que deixa o mapa legível, com pin visualmente distinto por tipo.

**É o primeiro módulo a fazer, de propósito:** é a tabela mais simples do modelo (`CategoriaID` + descrição, mais ícone/cor), destrava categoria em todos os outros módulos (filtros do RF-11/12, cards do Descobrir, comparativo do painel do negócio) e é o caso de uso perfeito pra **reusar o padrão de tabela** recém-portado — a prova de que a próxima tela custa só `columns.tsx` + `table.tsx`.

## Por que vale

- Pré-requisito de meio produto: busca com filtro, mapa com pins distintos, "aberto agora" por tipo.
- Valida o investimento no padrão de listagem: se esta tela sair rápido, o padrão pagou.
- Primeiro passo concreto do caminho `markers` → `PontoNoMapa` completo.

## Dependências

| O quê | Situação |
|---|---|
| Tabela `Categoria` | ❌ — a mais simples do modelo do TCC |
| `CategoriaID` FK em `markers` | ❌ — segunda etapa, com backfill dos pontos existentes |
| `CategoriesController` na API | ❌ — CRUD idêntico ao de markers |
| Papel de admin | 🔴 pré-requisito da área toda |

## Escopo inicial

- Migration/DDL da tabela + endpoint CRUD
- Tela no padrão: tabela + modais de criar/editar/excluir no slot global
- Seletor de ícone (subset do lucide) e cor
- Proteção contra excluir categoria em uso (ou reatribuição obrigatória)

## Fora do escopo inicial

- Subcategorias/hierarquia
- Sugestão de categoria por usuário comum (moderada)

> Decisões já tomadas e componentes existentes em
> [adr/admin/0002-categorias-padrao-listagem.md](../../adr/admin/0002-categorias-padrao-listagem.md)
