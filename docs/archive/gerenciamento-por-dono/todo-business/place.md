# 🏪 Meu ponto

> Área: estabelecimento · Rota: `/business/place` · Status: 💤 não iniciado

## Ideia

Auto-serviço do perfil do estabelecimento no mapa: descrição, contato, horário de funcionamento, categoria, fotos e ajuste fino da posição do pin. Hoje um ponto é `nome + lat + lng` criado por qualquer usuário — este módulo é onde o dono **reivindica e enriquece** o próprio ponto.

Inclui o fluxo de reivindicação ("este estabelecimento é meu"): usuário-estabelecimento encontra o ponto no mapa, solicita posse, admin aprova (via [Moderação](../../../todo/admin/moderation.md)), e o ponto ganha `UsuarioDono`.

## Por que vale

- Conteúdo rico de ponto (foto, horário, descrição) é o que faz o mapa valer a pena **antes** da massa de avaliações chegar — resolve metade do cold start.
- Horário de funcionamento destrava o filtro "aberto agora" no mapa e no [Descobrir](../../../todo/user/explore.md).
- Dono cuidando do próprio perfil = conteúdo curado de graça.

## Dependências

| O quê | Situação |
|---|---|
| Colunas ricas em `markers` (`descricao`, `contato`, `foto`, `horario`, `endereco`, `CategoriaID`) | ❌ — o modelo lógico do TCC já as previa em `PontoNoMapa` |
| FK `UsuarioDono` + fluxo de reivindicação | ❌ |
| `tipoUsuario` (só estabelecimento reivindica) | ❌ |
| Upload de foto (RF-08) | ❌ |
| Tabela `Categoria` ([Admin: Categorias](../../../todo/admin/categories.md)) | ❌ |

## Escopo inicial

- Formulário de edição do ponto (sem foto, enquanto RF-08 não existe)
- Horário por dia da semana (estrutura JSON simples)
- Fluxo de reivindicação com aprovação do admin

## Fora do escopo inicial

- Múltiplas fotos / galeria
- Cardápio
- Verificação automática por CNPJ (validação real na Receita)
