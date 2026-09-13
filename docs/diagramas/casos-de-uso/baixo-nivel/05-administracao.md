# 05. Administração — baixo nível

Detalha [04 — Administrador](../04-administrador.md).

> ⚠️ Todo diagrama desta página deveria começar por **verificar o papel do
> usuário**. Esse passo não existe: qualquer sessão válida abre `/admin`. Ele
> aparece marcado como ausente, não é omitido.

## Moderar a fila de pontos

```mermaid
flowchart LR
    ADM(("👤 Administrador"))

    ADM -.->|"passo ausente: verificar papel 🔴"| FILA
    ADM --- FILA(["Moderar a fila de pontos 🟡"])

    FILA -.->|include| CARREGA[/"Carregar os pontos pendentes 🟡"/]
    CARREGA -.->|include| ORDENA[/"Ordenar por tempo de espera 🟡"/]
    FILA -.->|include| SELECIONA(["Selecionar um item da fila 🟡"])
    SELECIONA -.->|include| DETALHE[/"Abrir o detalhe ao lado, sem sair da fila 🟡"/]

    DETALHE -.->|include| CAMPOS[/"Mostrar os campos enviados 🟡"/]
    DETALHE -.->|include| AUTOR[/"Mostrar o autor e o histórico dele 🟡"/]
    DETALHE -.->|include| MINI[/"Desenhar o bairro no mini-mapa SVG 🟡"/]

    DETALHE --> DEC{"O que fazer com o ponto?"}
    DEC -->|aprovar| APROVA[/"Marcar como aprovado e tirar da fila 🟡"/]
    DEC -->|rejeitar| MOTIVO(["Escolher o motivo da rejeição 🟡"])
    DEC -->|duplicata| COMPARA(["Comparar com o ponto existente 🟡"])
    MOTIVO -.->|include| REJEITA[/"Registrar a rejeição com o motivo 🟡"/]
    COMPARA -.->|extend| REJEITA

    APROVA -.->|include| HIST[/"Gravar no histórico de decisões 🟡"/]
    REJEITA -.->|include| HIST

    FILA -.->|extend| LOTE(["Selecionar vários e decidir de uma vez 🟡"])
    FILA -.->|extend| TECLADO(["Percorrer e decidir pelo teclado 🟡"])
    FILA -.->|extend| VERHIST(["Consultar o histórico 🟡"])
```

**Mestre-detalhe, não tabela mais modal:** aqui a decisão *é* a tela, e abrir
diálogo a cada item custaria um clique por decisão numa atividade que é
repetitiva por natureza. Por isso `Percorrer pelo teclado` é caso de primeira
classe, não conveniência.

**O mini-mapa é SVG, não MapLibre** — instanciar o mapa a cada troca de item na
fila pagaria carregamento de estilo e tiles para mostrar um bairro.

**`Marcar como aprovado` é 🟡 sem contrapartida real:** falta a coluna `status`
em `markers` e a tabela de decisão, então nada sai de fila nenhuma. O ponto
criado em `/places/new` vai direto ao mapa.

## Remover conteúdo denunciado

```mermaid
flowchart LR
    ADM(("👤 Administrador"))

    ADM -.->|"passo ausente: verificar papel 🔴"| DEN
    ADM --- DEN(["Tratar denúncias 🟡"])

    DEN -.->|include| AGRUPA[/"Agrupar as denúncias pelo alvo 🟡"/]
    AGRUPA -.->|include| SINAL[/"Calcular o selo de denúncia coordenada 🟡"/]
    DEN -.->|include| ABRE(["Abrir um alvo 🟡"])
    ABRE -.->|include| RENDER[/"Renderizar o conteúdo conforme o tipo 🟡"/]

    ABRE --> DEC{"O conteúdo infringe?"}
    DEC -->|não| MANTEM[/"Manter e encerrar as denúncias 🟡"/]
    DEC -->|sim| REM(["Remover com motivo 🟡"])
    REM -.->|include| CATALOGO[/"Escolher em REMOVAL_REASONS 🟡"/]
    CATALOGO -.->|include| APLICA[/"Aplicar a remoção 🟡"/]

    DEN -.->|extend| FEEDBACK(["Triar feedback 🟡"])
```

**`Escolher em REMOVAL_REASONS` é o mesmo passo em `/admin/reviews`.** O
catálogo saiu do mock para `src/constants/content-removal.ts` e o diálogo para
`src/components/blocks/removal-dialog.tsx` justamente para as duas telas não
divergirem no primeiro ajuste de regra — quando a Server Action nascer, ela é
uma só.

## Manter usuários

```mermaid
flowchart LR
    ADM(("👤 Administrador"))
    API[["🖥️ API Soromaps"]]

    ADM -.->|"passo ausente: verificar papel 🔴"| LISTAR
    ADM --- LISTAR(["Listar usuários ✅"])

    LISTAR -.->|include| GET[/"GET /api/users com cache tag ✅"/]
    LISTAR -.->|include| TABELA[/"Montar a tabela pelo useTableConfig ✅"/]
    TABELA -.->|extend| ORD(["Ordenar por coluna ✅"])
    TABELA -.->|extend| BUSCA(["Buscar por coluna ✅"])
    TABELA -.->|extend| FILTRO(["Aplicar o sheet de filtro ✅"])
    TABELA -.->|extend| COLUNAS(["Alternar visibilidade de coluna ✅"])
    TABELA -.->|extend| PAG(["Paginar ✅"])

    LISTAR -.->|extend| CRIAR(["Criar usuário ✅"])
    LISTAR -.->|extend| EDITAR(["Editar usuário ✅"])
    LISTAR -.->|extend| EXCLUIR(["Excluir usuário ✅"])

    CRIAR -.->|include| ROTA[/"Interceptar a rota e abrir como modal ✅"/]
    EDITAR -.->|include| ROTA
    EXCLUIR -.->|include| ROTA
    ROTA -.->|include| SERVIDOR[/"Buscar o registro no servidor ✅"/]
    SERVIDOR -.->|include| ZOD[/"Validar com Zod na Server Action ✅"/]
    ZOD -.->|include| ESCREVE[/"Chamar a API ✅"/]
    ESCREVE -.->|include| TAG[/"Invalidar com updateTag ✅"/]
    TAG -.->|include| FECHA[/"Fechar o modal com a tabela já atualizada ✅"/]

    EDITAR -.->|include| SENHA(["Reinformar a senha 🔴"])

    ESCREVE --- API
    SERVIDOR --- API
```

**`Reinformar a senha` está marcado 🔴 sendo um passo que funciona** porque ele
não deveria existir: `PUT /api/users/{id}` re-hasheia a senha sempre, então
editar o e-mail obriga a redigitar a senha (`RF-ADM-36`).

**`Fechar o modal com a tabela já atualizada` depende do `updateTag`.** No Next
16 o `revalidateTag` exige perfil de cache, e é o `updateTag` que dá
*read-your-own-writes* dentro da action — é o que substituiu o
`router.refresh()` da versão anterior.

---

## Descrições dos casos

As descrições em template expandido — ator, pré e pós-condição, ações do
ator × ações do sistema numeradas e restrições — vivem em
[`descricoes/05-administracao.md`](../descricoes/05-administracao.md): Moderar ponto da fila, Remover conteúdo denunciado, Excluir categoria e Criar usuário.

---

⬅️ [README do baixo nível](./README.md) · ⬆️
[Catálogo de casos](../05-catalogo.md)
