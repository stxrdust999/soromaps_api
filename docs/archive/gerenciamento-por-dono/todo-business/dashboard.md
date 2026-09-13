# 📈 Painel do negócio

> Área: estabelecimento · Rota: `/business/dashboard` · Status: 💤 não iniciado

## Ideia

O dashboard do dono de estabelecimento: média de estrelas e evolução no tempo, avaliações recentes, visitas por semana, e comparativo com a média da categoria ("sua nota: 4,2 · média de lanchonetes: 3,8"). É o **RF-10 visto pelo lado do estabelecimento** — e a persona do Augusto inteira, que hoje não tem uma única tela no sistema.

Este módulo é o que transforma o Soromaps de "app de avaliações" em **plataforma de dois lados**: o estabelecimento deixa de ser objeto avaliado e vira usuário com valor próprio. É também a fonte de monetização futura definida no modelo de negócio.

## Por que vale

- Ataca a dor do Augusto na raiz: "sinto falta de uma plataforma centralizada" pra feedback dos clientes.
- É o argumento de aquisição B2B: com painel, faz sentido o comércio pedir avaliação aos clientes (QR no balcão) — o que alimenta o lado B2C. Roda de crescimento.
- Pra banca do TCC: demonstra que as personas não eram enfeite de slide.

## Dependências

| O quê | Situação |
|---|---|
| `tipoUsuario` + `CNPJ` em `tbUsuario` | ❌ — a distinção normal/estabelecimento não existe no banco |
| FK `UsuarioDono` em `markers` (qual ponto é meu?) | ❌ |
| `Analise` (a matéria-prima de tudo) | ❌ |
| `Visita` (métrica de tráfego) | ❌ |
| Guarda de rota por tipo (`/business/*` só pra estabelecimento) | ❌ — o middleware hoje só checa sessão |

É o módulo com mais dependências do sistema — e por isso o melhor **teste de integração conceitual**: quando ele funcionar, o modelo de dados inteiro funcionou.

## Escopo inicial

- Cards de métrica: nota média, total de avaliações, visitas na semana
- Lista das 10 avaliações mais recentes, com link pra responder
- Gráfico de nota ao longo do tempo (mensal)

## Fora do escopo inicial

- Comparativo com a categoria (precisa massa de dados)
- Múltiplos pontos por dono (rede/filiais)
- Plano pago / recursos premium
