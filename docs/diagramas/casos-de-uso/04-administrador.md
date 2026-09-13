# 04. Administrador — as sete telas de `/admin`

> ⚠️ Nenhum caso desta página tem checagem de papel. Toda sessão válida abre
> `/admin`. O ator existe no desenho, não no `middleware.ts` nem na API.

## Moderação e denúncias

```mermaid
flowchart LR
    ADM(("👤 Administrador"))

    ADM --- MOD(["Moderar fila de pontos 🟡"])
    ADM --- DEN(["Tratar denúncias 🟡"])

    MOD -.->|include| REVISA(["Revisar a ficha do ponto 🟡"])
    MOD -.->|extend| APROVA(["Aprovar ponto 🟡"])
    MOD -.->|extend| REJEITA(["Rejeitar com motivo 🟡"])
    MOD -.->|extend| DUPL(["Comparar duplicata 🟡"])
    MOD -.->|extend| LOTE(["Decidir em lote 🟡"])
    MOD -.->|extend| HIST(["Consultar histórico de decisões 🟡"])
    REVISA -.->|include| AUTOR(["Ver o autor do envio 🟡"])
    REVISA -.->|include| MINI(["Ver o bairro no mini-mapa 🟡"])

    DEN -.->|include| AGRUPA(["Agrupar denúncias por alvo 🟡"])
    DEN -.->|extend| COORD(["Ver selo de denúncia coordenada 🟡"])
    DEN -.->|extend| REMOVE(["Remover conteúdo com motivo 🟡"])
    DEN -.->|extend| TRIA(["Triar feedback 🟡"])
```

A moderação é mestre-detalhe, não tabela + modal: a decisão **é** a tela. O
mini-mapa é SVG, não MapLibre. Detalhe em
[`adr/admin/0003-moderacao-mestre-detalhe.md`](../../adr/admin/0003-moderacao-mestre-detalhe.md).

Falta a coluna `status` em `markers` e a tabela de decisão — daí o 🟡 em tudo.

## Avaliações

```mermaid
flowchart LR
    ADM(("👤 Administrador"))

    ADM --- AVA(["Revisar avaliações 🟡"])

    AVA -.->|include| KPI(["Ver os quatro indicadores 🟡"])
    AVA -.->|include| SINAL(["Ler os sinais formais 🟡"])
    AVA -.->|extend| COMENT(["Expandir os comentários da linha 🟡"])
    AVA -.->|extend| PIVOT(["Pivotar por autor ou por local 🟡"])
    AVA -.->|extend| REMOVE2(["Remover com motivo 🟡"])
    REMOVE2 -.->|extend| LOTE2(["Remover em lote 🟡"])
```

`Remover com motivo` é **o mesmo caso** de `/admin/reports`: `RemovalDialog` e
`REMOVAL_REASONS` já são compartilhados (`src/components/blocks/`,
`src/constants/content-removal.ts`), então quando a Server Action nascer as
duas telas falam igual. Sinais implementados: spam, duplicada, discrepante.

## Catálogos

```mermaid
flowchart LR
    ADM(("👤 Administrador"))
    API[["🖥️ API Soromaps"]]

    ADM --- CAT(["Manter categorias 🟡"])
    ADM --- CONQ(["Manter conquistas 🟡"])
    ADM --- USR(["Manter usuários ✅"])

    CAT -.->|extend| CATN(["Criar categoria 🟡"])
    CAT -.->|extend| CATE(["Editar categoria 🟡"])
    CAT -.->|extend| CATD(["Excluir com reatribuição 🟡"])
    CATN -.->|include| COR(["Checar colisão de cor 🟡"])
    CATE -.->|include| COR
    CATN -.->|include| PIN(["Ver o pin em prévia 🟡"])

    CONQ -.->|extend| CRIT(["Montar critério declarativo 🟡"])
    CONQ -.->|extend| ALC(["Estimar alcance 🟡"])
    CONQ -.->|extend| PREV(["Prever o desbloqueio 🟡"])
    CONQ -.->|extend| DESA(["Desativar conquista 🟡"])
    CONQ -.->|extend| CALI(["Calibrar a faixa 🟡"])

    USR -.->|extend| USRN(["Criar usuário ✅"])
    USR -.->|extend| USRE(["Editar usuário ✅"])
    USR -.->|extend| USRD(["Excluir usuário ✅"])
    USR -.->|include| LIST(["Ordenar, filtrar, paginar ✅"])

    USRN --- API
    USRE --- API
    USRD --- API
    USR --- API
```

`/admin/users` é a **única** tela de admin sobre dado real, e a única ✅ do
`todo/README.md`. Os três casos de escrita abrem em rota interceptada no slot
`(app)/@modals` — modal é rota, com URL compartilhável e botão voltar.

`Editar usuário` exige a senha porque o `PUT` da API re-hasheia sempre; a
atualização parcial é item do backlog.

## Dashboard

```mermaid
flowchart LR
    ADM(("👤 Administrador"))

    ADM --- DASH(["Acompanhar indicadores 🟡"])

    DASH -.->|include| FILAS(["Ver filas de atenção 🟡"])
    DASH -.->|include| CARDS(["Ver os quatro cards de número 🟡"])
    DASH -.->|include| GRAF(["Ver cadastros e qualidade do dado 🟡"])
    FILAS -.->|extend| PULA(["Pular para a fila de moderação 🟡"])
```

Espera `GET /api/admin/stats` — um agregado, para o dashboard não fazer N
chamadas de lista só para contar. A série temporal do gráfico é ruído
determinístico sobre data-âncora fixa: `Math.random()` ou `Date.now()` em mock
renderiza diferente no servidor e no cliente e quebra a hidratação.

---

➡️ [05 — Catálogo](./05-catalogo.md)
