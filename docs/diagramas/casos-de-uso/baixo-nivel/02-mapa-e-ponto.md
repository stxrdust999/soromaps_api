# 02. Mapa e ponto — baixo nível

Detalha as seções Mapa e Ponto de [02 — Explorador](../02-explorador.md).

## Explorar o mapa

```mermaid
flowchart LR
    EXP(("👤 Explorador"))
    CARTO[["🖥️ CARTO basemaps"]]
    API[["🖥️ API Soromaps"]]

    EXP --- MAPA(["Explorar o mapa ✅"])

    MAPA -.->|include| MONTA[/"Montar a instância MapLibre ✅"/]
    MONTA -.->|include| ESTILO[/"Escolher positron ou dark-matter pelo tema ✅"/]
    MONTA -.->|include| CENTRO[/"Centrar em SOROCABA_VIEWPORT ✅"/]

    MAPA -.->|extend| PAN(["Arrastar e dar zoom ✅"])
    PAN -.->|include| MOVEEND[/"Aguardar o fim do gesto ✅"/]
    MOVEEND --> DEC{"Zoom ≥ 14?"}
    DEC -->|"passou a sim"| BUSCA[/"Buscar a lista de marcadores ✅"/]
    DEC -->|"passou a não"| LIMPA[/"Limpar os marcadores da tela ✅"/]
    DEC -->|"não mudou"| NADA[/"Não fazer nada ✅"/]
    BUSCA -.->|extend| ABORTA[/"Abortar a busca anterior em curso ✅"/]

    MAPA -.->|extend| LOCALIZA(["Centrar na própria posição 🟡"])
    MAPA -.->|extend| TEMA(["Trocar o tema ✅"])
    TEMA -.->|include| ESTILO

    ESTILO --- CARTO
    BUSCA --- API
```

**O ramo `não mudou` é o requisito, não um detalhe.** O efeito depende do
booleano `zoom >= minZoom`, não do float do zoom: arrastar e dar zoom dentro da
mesma faixa não dispara requisição nenhuma. A versão anterior dependia de
`viewport.zoom` no `useEffect` e do evento `move`, que dispara a cada quadro —
dezenas de `GET /api/markers` por gesto (`RNF-DES-01`).

**`Centrar na própria posição` é 🟡** porque a coordenada é usada para mover a
câmera e descartada: nada é persistido nem ordenado por proximidade real.

## Ver e abrir um marcador

```mermaid
flowchart LR
    EXP(("👤 Explorador"))

    EXP --- HOVER(["Passar o cursor sobre o pino ✅"])
    HOVER -.->|include| TOOLTIP[/"Exibir o rótulo de uma linha ✅"/]
    TOOLTIP -.->|extend| SOME[/"Remover no mouseleave ✅"/]

    EXP --- CLIQUE(["Clicar no pino ✅"])
    CLIQUE -.->|include| POPUP[/"Abrir o card de display ✅"/]
    POPUP -.->|extend| DETALHE(["Ir para /places/[id] ✅"])
```

**O rótulo de hover é `pointer-events-none` e não existe no toque.** Nada dentro
dele é clicável, nunca — por isso ele carrega uma linha, e toda ação mora no
card ou na página cheia.

## Cadastrar ponto

```mermaid
flowchart LR
    EXP(("👤 Explorador"))
    API[["🖥️ API Soromaps"]]

    EXP --- CRIAR(["Cadastrar ponto 🟡"])

    CRIAR -.->|include| ABRE[/"Abrir /places/new no estágio picking ✅"/]
    ABRE -.->|include| ARRASTA(["Arrastar o pino até o lugar ✅"])
    ARRASTA -.->|include| COORD[/"Ler lat e lng da posição do pino ✅"/]
    COORD -.->|include| FORM[/"Passar ao estágio form, mantendo o mapa visível ✅"/]

    FORM -.->|include| CAMPOS(["Preencher os 8 campos 🟡"])
    CAMPOS -.->|extend| VOLTA(["Trocar de lugar sem perder o preenchido ✅"])
    VOLTA -.->|include| ARRASTA

    CAMPOS -.->|include| ZODCLI[/"Validar no navegador com zodResolver 🟡"/]
    ZODCLI -.->|include| ENVIA[/"Enviar o FormData à Server Action ✅"/]
    ENVIA -.->|include| CONVERTE[/"Converter lat e lng para número ✅"/]
    CONVERTE -.->|include| ZODSRV[/"Revalidar com createMarkerSchema ✅"/]
    ZODSRV -.->|include| DESCARTA[/"Descartar os 5 campos sem coluna 🔴"/]
    DESCARTA -.->|include| POST[/"POST /api/markers com nome, lat e lng ✅"/]
    POST -.->|include| TAG[/"Invalidar MARKERS_LIST_TAG ✅"/]
    TAG -.->|include| TOAST[/"Avisar em toast e sair da tela ✅"/]

    ZODSRV -.->|extend| ERRO_CAMPO(["Ver erro por campo ✅"])
    POST -.->|extend| ERRO_REQ(["Ver falha de requisição ✅"])

    POST --- API
```

**`Descartar os 5 campos sem coluna` está desenhado de propósito.** É o passo
que o fluxo executa e que nenhum documento de alto nível mostra: foto, sobre,
categoria, wifi, petfriendly, melhor horário e segredo local são validados no
navegador e param em `toMarkerInput`. Ver `RNF-DAD-16`.

**A conversão para número mora na action, não no schema.** `z.coerce.number()`
deixa o tipo de entrada `unknown` e quebra o `zodResolver` do formulário.

## Editar e excluir ponto

```mermaid
flowchart LR
    EXP(("👤 Explorador"))
    ADM(("👤 Administrador"))
    API[["🖥️ API Soromaps"]]

    EXP --- VER(["Ver detalhes do ponto 🟡"])
    VER -.->|include| BUSCA1[/"Buscar o ponto por id com cache tag ✅"/]
    VER -.->|include| MOCKS[/"Completar a ficha com dado de exemplo 🟡"/]
    MOCKS -.->|include| AVISA[/"Avisar quais campos são exemplo ✅"/]

    VER -.->|extend| EDITAR(["Editar ponto ✅"])
    EDITAR -.->|include| PUT[/"PUT /api/markers/{id} ✅"/]
    PUT -.->|include| TAG2[/"Invalidar a lista e o item ✅"/]

    VER -.->|extend| EXCLUIR(["Excluir ponto ✅"])
    EXCLUIR -.->|include| CONFIRMA(["Confirmar no diálogo ✅"])
    CONFIRMA -.->|include| DEL[/"DELETE /api/markers/{id} ✅"/]
    DEL -.->|include| TAG2

    ADM -.->|"passo ausente: verificar papel 🔴"| EDITAR
    ADM -.->|"passo ausente: verificar papel 🔴"| EXCLUIR

    PUT --- API
    DEL --- API
```

`Buscar o ponto por id` usa `markerShowTag(id)`, e a escrita invalida **as
duas** tags — a do item e a da lista —, para o mapa e a página não discordarem.

---

## Descrições dos casos

As descrições em template expandido — ator, pré e pós-condição, ações do
ator × ações do sistema numeradas e restrições — vivem em
[`descricoes/02-mapa-e-ponto.md`](../descricoes/02-mapa-e-ponto.md): Explorar o mapa, Cadastrar ponto, Editar ponto e Excluir ponto.

---

➡️ [03 — Descoberta e feed](./03-descoberta-e-feed.md)
