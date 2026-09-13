> 📐 **Trilha: projetado (TCC).** Status de implementação de cada requisito está na coluna própria — detalhe em [12 — Gap](../../wiki/09-backlog.md).

# 📋 02. Requisitos

Treze requisitos funcionais de alto nível definidos na fase de levantamento. A coluna **Hoje** marca o que já existe no código.

| ID | Requisito | Hoje |
|---|---|---|
| RF-01 | Permitir o cadastro de contas e a exibição dos perfis dos usuários | 🟡 Parcial |
| RF-02 | Possibilitar a configuração dos perfis após a criação da conta | 🟡 Parcial |
| RF-03 | Exibir um mapa interativo | 🟢 Feito |
| RF-04 | Ter acesso a GPS | 🟡 Parcial |
| RF-05 | Permitir a criação de pontos no mapa | 🟢 Feito |
| RF-06 | Permitir a configuração de um ponto | 🟢 Feito |
| RF-07 | Permitir a avaliação de um ponto | 🔴 Não iniciado |
| RF-08 | Possibilitar o upload de fotos | 🔴 Não iniciado |
| RF-09 | Permitir a criação de comentários com possibilidade de resposta | 🔴 Não iniciado |
| RF-10 | Gerar estatísticas baseadas em avaliações e exibir tendências | 🔴 Não iniciado |
| RF-11 | Permitir a busca de pontos | 🔴 Não iniciado |
| RF-12 | Oferecer filtros de busca | 🔴 Não iniciado |
| RF-13 | Permitir interações entre os usuários | 🔴 Não iniciado |

**Legenda:** 🟢 funcionando ponta a ponta · 🟡 existe parcialmente ou só na UI · 🔴 sem código nem tabela.

---

## 🔎 Onde cada requisito implementado vive

| ID | Evidência no código |
|---|---|
| RF-01 | `POST /api/users` (`UsersController.Create`) + tela [`(auth)/register`](../../../src/app/(auth)/register/page.tsx); perfil ainda é placeholder |
| RF-02 | `PUT /api/users/{id}` existe; a tela de perfil ainda não edita |
| RF-03 | [`src/components/ui/map.tsx`](../../../src/components/ui/map.tsx) — MapLibre GL com basemaps CARTO |
| RF-04 | Controle `showLocate` do mapa usa a geolocalização do browser; não há persistência de posição |
| RF-05 | [`(app)/places/new`](../../../src/app/(app)/(explorer)/places/new/page.tsx) → `POST /api/markers` |
| RF-06 | Edição e exclusão do ponto pelo popup do marcador (`PUT`/`DELETE /api/markers/{id}`) |

Detalhe importante para RF-01/RF-02: o cadastro grava `user_name`, `user_email` e hash da senha — **não existem CPF, CNPJ nem `tipoUsuario`**, então a distinção entre usuário comum e estabelecimento, central no modelo, ainda não existe no sistema.

---

## 🔗 Rastreabilidade requisito → caso de uso

| Requisito | Casos de uso relacionados |
|---|---|
| RF-01, RF-02 | Criar perfil, Configurar perfil |
| RF-03, RF-04 | Acessar mapa |
| RF-05, RF-06 | Criar ponto, Configurar ponto |
| RF-07 | Avaliar ponto |
| RF-08 | Carregar foto |
| RF-09 | Criar comentário, Interagir com avaliações |
| RF-10 | Gerar estatísticas (Administrador) |
| RF-11, RF-12 | Buscar ponto, Aplicar filtros |
| RF-13 | Seguir usuário, Deixar de seguir, Interagir com avaliações |

---

## ➡️ Próxima página

[03 — Casos de uso](./03-casos-de-uso.md)
