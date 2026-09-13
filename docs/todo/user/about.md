# ℹ️ Sobre

> Área: usuário público · Rota: `/about` · Status: 💤 não iniciado

## Ideia

Página institucional do projeto: o que é o Soromaps, os três pilares, o time (a tabela de integrantes do TCC), o contexto acadêmico (FATEC Sorocaba) e links — repositórios, protótipo no Figma, vídeos de apresentação. Um bloco "como funciona" com 3 passos ilustrados (explore o mapa → visite e avalie → ganhe conquistas).

Pode ser **pública** (fora do guard do middleware) — é a única página apresentável a quem não tem conta, junto do login.

## Por que vale

- Custo mínimo, acabamento máximo: é a diferença entre "projeto de faculdade" e "produto com identidade" na hora da banca.
- Único lugar onde o trabalho acadêmico (personas, dores, pilares) aparece **dentro** do produto, fechando o círculo com a documentação.
- Rota pública dá um destino pro visitante curioso antes do cadastro — micro-landing page.

## Dependências

Nenhuma. Zero tabela, zero endpoint. Conteúdo já existe todo em `/docs`.

## Escopo inicial

- Hero com a proposta em uma frase
- Os três pilares em cards
- "Como funciona" em 3 passos
- Time + links (repos, Figma, vídeos)
- Adicionar `/about` às rotas públicas do `middleware.ts`

## Fora do escopo inicial

- Página de imprensa/contato
- Changelog público
