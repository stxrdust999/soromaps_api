# 🔬 Casos de uso — baixo nível

> Detalhamento dos casos desenhados em
> [`casos-de-uso/`](../README.md). Mesma leitura do **sistema atual**, mesma
> divisão por área, um nível abaixo.

## Páginas

Espelham 1:1 as do alto nível e a divisão dos
[requisitos funcionais](../../requisitos-funcionais/README.md):

| Arquivo | Alto nível correspondente | Requisitos |
|---|---|---|
| [01 — Sessão e acesso](./01-sessao-e-acesso.md) | [02 — Explorador](../02-explorador.md), seção Sessão | `RF-SES-*` |
| [02 — Mapa e ponto](./02-mapa-e-ponto.md) | [02 — Explorador](../02-explorador.md), seções Mapa e Ponto | `RF-MAP-*`, `RF-PTO-*` |
| [03 — Descoberta e feed](./03-descoberta-e-feed.md) | [02](../02-explorador.md) e [03](../03-explorador-social.md) | `RF-DES-*`, `RF-FED-*` |
| [04 — Comunidade e perfil](./04-comunidade-e-perfil.md) | [03 — Explorador social](../03-explorador-social.md) | `RF-COM-*`, `RF-PER-*` |
| [05 — Administração](./05-administracao.md) | [04 — Administrador](../04-administrador.md) | `RF-ADM-*` |

---

## O que muda do alto nível para cá

| | Alto nível | Baixo nível |
|---|---|---|
| **Granularidade** | "Cadastrar ponto" | Abrir a tela, posicionar o pino, ler as coordenadas, preencher, validar, enviar, invalidar cache, redirecionar |
| **Pergunta que responde** | Quem faz o quê no sistema | Como esse "o quê" acontece, passo a passo |
| **O que aparece** | Ator ↔ caso, `include`/`extend` | Os mesmos, mais o **sistema como executor** e os fluxos alternativos |
| **Serve para** | Enxergar o produto de uma vez | Implementar, testar e revisar um fluxo específico |

Cada página traz o **diagrama de decomposição** de cada caso de alto nível,
mais as notas do que o desenho não diz. As **descrições em template expandido**
— ator, pré e pós-condição, ações do ator × ações do sistema, restrições —
ficam em [`../descricoes/`](../descricoes/README.md), uma página por área, com
os mesmos nomes de arquivo.

## Notação

A do alto nível, mais dois elementos:

| Forma | Significado |
|---|---|
| `(("👤 Nome"))` | Ator primário |
| `[["🖥️ Nome"]]` | Ator de sistema |
| `(["Texto"])` | Caso de uso de baixo nível |
| `[/"Texto"/]` | **Passo executado pelo sistema**, sem intervenção do ator |
| `{"Texto?"}` | **Ponto de decisão** que abre fluxo alternativo |
| `-.->\|include\|` · `-.->\|extend\|` | As mesmas relações |

Cada caso de baixo nível leva o estado do dado (✅ persistido / 🟡 sobre mock)
e o `RF-*` que atende.

---

## 🧭 Decisões deste detalhamento

### Baixo nível descreve o fluxo real, não o fluxo ideal
**Decisão:** quando o código faz diferente do que deveria, o diagrama desenha o
que o código faz, e a divergência vira nota.
**Motivo:** um detalhamento que corrige o sistema no papel não serve para
implementar nem para testar — e mascararia justamente os pontos em que o
produto precisa mudar. É por isso que "Verificar papel do usuário" aparece como
passo **ausente**, e não como passo cumprido.

### Passo do sistema é forma distinta do passo do ator
**Decisão:** `[/"assinar o cookie"/]` em vez de `(["assinar o cookie"])`.
**Motivo:** em diagrama de casos de uso tudo é "caso", e no baixo nível isso
achata a informação mais útil — quem age. Separar as duas formas deixa ler de
relance onde a pessoa decide e onde o sistema apenas executa, que é a diferença
entre um passo testável por interface e um testável por unidade.

### Diagrama aqui, descrição em `descricoes/`
**Decisão:** o passo a passo aparece uma vez só — desenhado aqui, escrito lá.
**Motivo:** os dois formatos descrevem o mesmo fluxo, e mantê-los lado a lado
garantiria divergência no primeiro ajuste. Vale a regra que atravessa `/docs`:
nenhum `.md` repete o texto de outro, linka.
