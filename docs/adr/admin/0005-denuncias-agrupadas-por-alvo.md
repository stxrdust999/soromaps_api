# Denúncias e feedback: mesma rota, modelos diferentes

> Módulo: admin/reports · Rota: `/admin/reports` · Data: 2026-08-09

## O que foi implementado

Tela em `src/app/(app)/admin/reports/`, portada de um mockup do Claude
Design, sobre `src/mocks/admin-reports.ts` (9 casos e 18 envios de feedback).
**As duas abas têm forma de tela diferente**, tradução direta de que
compartilham a forma de trabalho (fila que chega sozinha) mas não o modelo
(alvo, ações, o que significa "resolvido"):

| Aba | Forma | Por quê |
|---|---|---|
| Denúncias | Mestre-detalhe | Decidir exige ler o conteúdo inteiro, os denunciantes e o histórico do autor — não cabe em linha de tabela |
| Feedback | Listagem com chips | Triagem rápida: ler, classificar, seguir. A mensagem expande na própria linha |

| Peça | Arquivo |
|---|---|
| Estado, sinais de risco, agregação de motivos | `_components/use-reports.ts` |
| Fila agrupada por alvo | `_components/report-queue.tsx` |
| Painel de decisão | `_components/report-detail.tsx` |
| Conteúdo denunciado, por tipo de alvo | `_components/reported-content.tsx` |
| Nota em estrelas | `_components/star-rating.tsx` |
| Remoção com motivo | `_components/removal-dialog.tsx` |
| Triagem de feedback | `_components/feedback-table.tsx`, `columns-feedback.tsx` |

## A regra que define a aba de denúncias

**A fila lista alvos, não denúncias.** O item reportado seis vezes é um
caso; seis linhas iguais fariam o admin decidir a mesma coisa seis vezes e
inflariam o tamanho aparente da fila. A contagem vai na linha, os
denunciantes individuais abrem no painel.

**Decisão tomada (2026-08-09): feedback aceita envio anônimo.** A tela trata
os dois casos na mesma lista, e a linha anônima mostra "sem como responder"
com a ação de responder desabilitada — o custo do anônimo fica visível em vez
de virar surpresa. O chip de autoria existe para medir a proporção depois.

## Decisões

### Que o escopo original não previa

- **Selo de denúncia coordenada.** Quando três ou mais denunciantes são
  contas de até 3 dias e *todos* os denunciantes são novos, o caso é marcado
  em vermelho. Sem isso a fila vira arma: cinco contas criadas ontem
  derrubam qualquer conteúdo legítimo. É o análogo do sinal de conflito de
  Comércios — módulo cancelado em 19/08/2026, ADR em
  [`docs/archive/gerenciamento-por-dono/`](../../archive/gerenciamento-por-dono/0006-comercios-tres-abas.md).
- **Selo de motivos divergentes.** Denunciantes que não concordam entre si
  (um diz spam, outro diz ofensa) costumam indicar desavença pessoal, não
  problema no conteúdo. Sinal fraco, marcado como neutro de propósito.
- **Contexto técnico do bug** — rota e dispositivo, coletados no envio. É o
  que separa "não funciona" de relato acionável, e sai de graça.
- **Renderização por tipo de alvo.** Avaliação mostra estrelas, comentário
  mostra a avaliação-mãe recuada, ponto mostra foto e coordenada, perfil
  mostra bio e contadores. Reduzir tudo a um bloco de texto tiraria o que
  sustenta a decisão.

### De implementação

- **Sem "advertir" nem "suspender".** Punição de conta está fora do escopo,
  e prometer o botão seria mentira. O rodapé do painel diz isso
  explicitamente.
- **A fila da esquerda usa `Select` simples, não os chips de filtro.** Numa
  coluna de 380px o chip expansível não tem para onde crescer; os chips
  ficam na aba de feedback, que tem a largura da página.
- **`h-dvh` na página**, mesma razão de
  [`/admin/moderation`](./0003-moderacao-mestre-detalhe.md): mestre-detalhe
  com rolagem por coluna precisa de altura fechada.

## Fronteira com as telas vizinhas

- **[Moderação](./0003-moderacao-mestre-detalhe.md)**: julga ponto novo
  antes de publicar; aqui só entra o que já está público.
- **[Avaliações](./0007-avaliacoes-remocao-unificada.md)**: mesma ação de
  remover avaliação, entrada diferente — lá o admin varre por conta própria,
  aqui ele reage a report. A remoção é uma server action só, consumida pelas
  duas telas.
- **Moderação do estabelecimento** (`todo/business/moderation.md`, ainda não
  implementada): o dono modera comentário nas próprias respostas; nada do
  que ele faz passa por esta fila.

## Pendências conhecidas

- O tempo médio de resolução e "resolvidos esta semana" são fixos no mock
- A observação do diálogo de remoção não é persistida
- Marcar feedback como respondido não abre canal nenhum — o contato sai por
  fora

Sai de 🟡 quando `Denuncia` e `Feedback` existirem — ver
[docs/todo/admin/reports.md](../../todo/admin/reports.md).
