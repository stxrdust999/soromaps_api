# 📝 Descrições de caso de uso

> A contraparte textual dos diagramas: cada caso descrito no template
> expandido, com ações do ator e do sistema lado a lado.

Diagrama mostra **quem faz o quê**. Descrição mostra **em que ordem, com que
pré-condição e o que impede**. Um não substitui o outro — a banca costuma
pedir os dois, e quem implementa lê a descrição.

## Páginas

Mesma divisão por área do resto da pasta:

| Arquivo | Casos descritos |
|---|---|
| [01 — Sessão e acesso](./01-sessao-e-acesso.md) | Cadastrar conta, Entrar, Sair |
| [02 — Mapa e ponto](./02-mapa-e-ponto.md) | Explorar o mapa, Cadastrar ponto, Editar ponto, Excluir ponto |
| [03 — Descoberta e feed](./03-descoberta-e-feed.md) | Descobrir lugares, Ler o feed, Ver menos disso, Acompanhar lugar |
| [04 — Comunidade e perfil](./04-comunidade-e-perfil.md) | Buscar exploradores, Gerar rascunho de pauta, Ver o próprio perfil |
| [05 — Administração](./05-administracao.md) | Moderar ponto, Remover conteúdo denunciado, Excluir categoria, Criar usuário |

Os diagramas correspondentes estão em [`../baixo-nivel/`](../baixo-nivel/README.md);
o requisito de cada caso, em
[`../../requisitos-funcionais/`](../../requisitos-funcionais/README.md).

---

## Template

| Nome do Caso de Uso | Nome curto, no infinitivo |
|---|---|
| Caso de Uso Geral | O caso de alto nível de que este faz parte |
| Ator Principal | Quem inicia |
| Ator Secundário | Quem o sistema aciona para concluir |
| Resumo | Uma a três linhas: o que o caso entrega |
| Pré-Condição | O que precisa ser verdade antes de começar |
| Pós-Condição | O que passa a ser verdade depois |
| **Ações do Ator** | **Ações do Sistema** |
| 1. Passo de quem usa | |
| | 2. Resposta do sistema |
| Restrições / Validação | Regra que o caso precisa respeitar |

**A numeração é contínua entre as duas colunas** — 1 e 2 do ator, 3 do sistema,
4 do ator, e assim por diante. É o que deixa a ordem legível sem setas.

Abaixo de cada tabela, quando houver, vem um bloco **⚠️ Lacunas** com os passos
que o fluxo deveria ter e não tem. Não entram na tabela porque a tabela
descreve o sistema que existe.

---

## 🧭 Decisões deste documento

### Fluxo alternativo entra na tabela, não em seção separada
**Decisão:** desvio vira passo numerado com a condição no próprio texto
("6a. Se a credencial for recusada..."), na coluna de quem age.
**Motivo:** o template do print tem duas colunas e uma linha de restrições —
não tem casa para uma terceira seção. Numerar `6a` mantém o desvio ancorado no
passo de onde ele sai, que é o que se procura ao ler.

### Restrição diz o estado, não só a regra
**Decisão:** cada linha de "Restrições / Validação" leva 🟢 quando o sistema
cumpre e 🔴 quando não cumpre, com o `RNF-*` correspondente.
**Motivo:** a mesma razão dos requisitos — restrição listada como se fosse
cumprida transforma o documento em intenção. E é aqui que "só administrador
modera" aparece pela primeira vez como algo que o código **não** faz.

### O sistema descrito é o que roda, mock incluído
**Decisão:** onde o dado é fictício, a pós-condição diz isso — "nada é
persistido", e não "o registro é gravado".
**Motivo:** pós-condição é o que se testa ao final do caso. Prometer gravação
onde não há tabela produziria um roteiro de teste que reprova o sistema inteiro
por um motivo que já é conhecido e datado.
