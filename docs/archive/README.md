# 🗃️ Archive — material histórico

> Nada aqui é fonte de verdade. É registro do que o projeto **já foi**, mantido porque o Soromaps é um TCC e a evolução das decisões faz parte da entrega.

Regra da pasta: quando um artefato deixa de descrever o estado atual do projeto mas ainda tem valor de histórico, ele vem pra cá — em uma subpasta por contexto — em vez de ser apagado. A documentação viva (`/docs` e `/docs/wiki`) sempre linka de volta pro original arquivado.

---

## 📁 Conteúdo

| Subpasta | O que guarda | Arquivada em |
|---|---|---|
| [`wiki-trilha-projetada/`](./wiki-trilha-projetada/README.md) | As seis páginas "projetado (TCC)" que a wiki mantinha em paralelo — requisitos RF-01..13, casos de uso, DER, modelo lógico, stack Node/SQL Server/Mapbox, protótipo | 20/08/2026 |
| [`gerenciamento-por-dono/`](./gerenciamento-por-dono/) | O grupo `business/` inteiro e `/admin/businesses` — dono de estabelecimento gerenciando o próprio ponto | 19/08/2026 |
| [`telas-fundidas/`](./telas-fundidas/) | `/places` como vitrine, antes de fundir em `/discover` | 17/08/2026 |
| `diagramas-originais/` | Exports PNG das ferramentas de modelagem do TCC | 28/07/2026 |

---

### `diagramas-originais/`

Exports das ferramentas de modelagem usados na entrega do TCC. Foram convertidos para Mermaid nativo dentro das páginas hoje em [`wiki-trilha-projetada/`](./wiki-trilha-projetada/README.md) — as imagens ficam como referência do traçado original.

| Arquivo | O que é | Onde vive hoje em Mermaid |
|---|---|---|
| `diagrama-conceitual.png` | Diagrama de classes (modelo conceitual OO), com `UsuarioNormal`/`Estabelecimento`, `Feed`, `Badge` e `Endereco` | [`wiki-trilha-projetada/05-modelagem-projetada.md`](./wiki-trilha-projetada/05-modelagem-projetada.md) |
| `Diagrama_Entidade-Relacionamento.png` | DER com entidades, atributos e cardinalidades `(min, max)` | [`wiki-trilha-projetada/05-modelagem-projetada.md`](./wiki-trilha-projetada/05-modelagem-projetada.md) |
| `Modelo-Logico.png` | Modelo lógico: 10 tabelas com tipos, PK e FK | [`wiki-trilha-projetada/05-modelagem-projetada.md`](./wiki-trilha-projetada/05-modelagem-projetada.md) |
| `Diagrama-de-Sequencia.png` | Sequência MVC da criação de ponto (View → Controller → Model → BD) | [`wiki-trilha-projetada/04-arquitetura-projetada.md`](./wiki-trilha-projetada/04-arquitetura-projetada.md) |

---

## 🧭 Decisão

### Mermaid como fonte de verdade, PNG como histórico
**Decisão:** todo diagrama passa a viver em Mermaid dentro do Markdown; o export original vai para `archive/`.
**Motivo:** PNG não é versionável de forma útil — um ajuste de cardinalidade vira um blob novo, sem diff legível. Mermaid entra no code review como texto. Alternativa descartada: manter só o PNG embutido nas páginas, que congelaria os diagramas na primeira versão exportada.
