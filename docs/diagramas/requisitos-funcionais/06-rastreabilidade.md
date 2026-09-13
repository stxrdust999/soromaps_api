# 06. Rastreabilidade

Ligação entre os 13 requisitos de alto nível do TCC
([`archive/wiki-trilha-projetada/02-requisitos.md`](../../archive/wiki-trilha-projetada/02-requisitos.md)) e os requisitos que o
sistema executa hoje.

## RF do TCC → requisitos atuais

| RF-TCC | Enunciado original | Requisitos atuais | Situação real |
|---|---|---|---|
| RF-01 | Cadastro de contas e exibição de perfis | RF-SES-01, 02, 03, 04, RF-PER-02, RF-ADM-33 | 🟢 no cadastro e na sessão; o perfil exibido é 🟡 |
| RF-02 | Configuração do perfil após a criação | RF-SES-06, RF-PER-01, RF-ADM-34, RF-ADM-36 | 🔴 para o próprio usuário — só o admin edita, e reenviando a senha |
| RF-03 | Mapa interativo | RF-MAP-01, RF-MAP-02 | 🟢 |
| RF-04 | Acesso a GPS | RF-MAP-03, RF-MAP-04, RF-MAP-05, RF-MAP-06 | 🟡 — a posição é lida, nunca persistida nem usada para ordenar |
| RF-05 | Criação de pontos no mapa | RF-PTO-01, RF-PTO-02 | 🟡 — grava 3 dos 8 campos coletados |
| RF-06 | Configuração de um ponto | RF-PTO-03, 04, 05, 07, RF-MAP-06, RF-DES-05 | 🟢 nos 3 campos que existem |
| RF-07 | Avaliação de um ponto | RF-ADM-19, 20, 22 | 🔴 — ninguém avalia; o admin modera avaliação fictícia |
| RF-08 | Upload de fotos | RF-PTO-08 | 🔴 |
| RF-09 | Comentários com resposta | RF-FED-06, RF-ADM-21 | 🔴 — só "útil" no feed, sobre mock |
| RF-10 | Estatísticas e tendências | RF-ADM-01..04, RF-COM-03, RF-PER-04, 08, 10 | 🟡 — quatro superfícies prontas, nenhuma sobre dado real |
| RF-11 | Busca de pontos | RF-DES-01, 02, 06, RF-MAP-08 | 🟡 na tela, 🔴 na consulta |
| RF-12 | Filtros de busca | RF-DES-03, 04, RF-FED-04, 08, RF-ADM-24..26 | 🟡 — filtra mock; `Categoria` não existe como tabela |
| RF-13 | Interações entre usuários | RF-FED-07, RF-COM-01, 02, 03 | ⛔ reinterpretado — vínculo com lugar no lugar de grafo social |

> A coluna "Hoje" de `archive/wiki-trilha-projetada/02-requisitos.md` está **defasada**: foi escrita
> antes de `/discover`, `/feed`, `/community` e `/profile` existirem, e ainda
> marca RF-11, RF-12 e RF-13 como não iniciados. Esta tabela é a leitura
> corrente.

## RF-13: o que foi cancelado e o que ficou no lugar

| Do TCC | Situação | Substituto |
|---|---|---|
| Seguir usuário | ⛔ cancelado em 2026-08-17 | RF-FED-07 — acompanhar **lugar** |
| Deixar de seguir | ⛔ cancelado | — |
| Feed de quem sigo | ⛔ cancelado | RF-FED-02 — cinco vínculos com a cidade |
| Perfil de outro usuário | ✅ mantido | RF-COM-02 |

O custo de `Segue` nunca foi a tabela: era o cold start do feed de grafo,
bloqueio, perfil privado e denúncia de perseguição entrando no escopo de um
TCC, e o assunto se deslocando para gente quando a tese do produto é lugar.

## Persona cancelada

O grupo `business/` inteiro e `/admin/businesses` saíram em 2026-08-19 — dono de
estabelecimento não gerencia nada dentro do app. Não há requisito ativo para
`tipoUsuario`, CNPJ, reivindicação de ponto nem painel de dono. Material em
[`archive/gerenciamento-por-dono/`](../../archive/gerenciamento-por-dono/).

## O que falta para cada 🟡 virar 🟢

| Tabela ausente | Destrava |
|---|---|
| `Analise` | RF-07, RF-ADM-19..23, o motivo `cidade` do feed, a nota nos cards de `/discover` |
| `Visita` | RF-DES-06, RF-PER-04, RF-PER-08, RF-PER-10, RF-COM-03, RF-COM-04, o motivo `perto` |
| `Favorita` | RF-FED-07, RF-PER-05, RF-PER-09, o motivo `salvo` |
| `Categoria` | RF-DES-03, RF-ADM-24..26, o motivo `categoria` |
| `Conquista` + `GanhaConquista` | RF-PER-06, RF-PER-07, RF-ADM-27..31 |
| `Comentario` | RF-09, RF-ADM-21 |
| `status` em `markers` | RF-ADM-05..13 |
| Entidade de pauta | RF-COM-08, 09, 10 |
| Papel de usuário | RF-ADM-37, RF-PTO-06, RF-COM-12 |

Dívida técnica correspondente em
[`wiki/09-backlog.md`](../../wiki/09-backlog.md)
e no backlog do `/CLAUDE.md`.
