# 06. Dados e IA

O Soromaps escreve texto sobre comércio real. Um guia local que inventa preço,
horário ou qualidade causa prejuízo a um negócio de verdade — os requisitos
desta página existem por isso, não por formalidade.

## Uso do modelo

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-DAD-01 | Geração de IA nunca roda no caminho do leitor | Nada chama o modelo durante a navegação: alguém pede o rascunho, o modelo escreve, um humano publica | 🟢 |
| RNF-DAD-02 | O modelo não escolhe o assunto | Recebe lista fechada de lugares com ficha de fatos, vinda da tela | 🟢 |
| RNF-DAD-03 | A instrução de sistema enumera o proibido | Preço, horário, endereço, telefone, prato, pessoa, prêmio e história não podem ser inventados | 🟢 |
| RNF-DAD-04 | A saída do modelo é entrada não confiável | O JSON é revalidado com Zod, porque `responseSchema` garante forma, não conteúdo | 🟢 |
| RNF-DAD-05 | A chamada tem teto de tempo | 30s, para o rascunho longo não estourar a action antes do limite da API | 🟢 |
| RNF-DAD-06 | Temperatura baixa para texto factual | 0.4, abaixo do padrão da API | 🟢 |
| RNF-DAD-07 | Falta de chave é estado, não exceção | `src/lib/gemini.ts` não lança: devolve `{ ok: false, reason: "sem-chave" }` | 🟢 |
| RNF-DAD-08 | Conteúdo de IA é rotulado ao leitor | Pauta de origem `ia` aparece marcada; da equipe, também | 🟡 |
| RNF-DAD-09 | Texto não publicado não vaza | `status: "rascunho"` responde 404 na rota pública | 🟡 |
| RNF-DAD-10 | Geração é auditável | Quem pediu, com que prompt, quando e o que voltou | 🔴 |
| RNF-DAD-11 | Só quem publica pode gerar | O gerador exige papel editorial | 🔴 |

**RNF-DAD-02, 03 e 04 são três defesas nesta ordem**, e a ordem importa: limitar
a entrada, instruir a conduta, validar a saída. A terceira existe porque as duas
primeiras são pedidos, não garantias.

**RNF-DAD-11 tem custo direto.** O gerador está no cabeçalho de `/community`,
aberto a qualquer sessão, e cada clique consome cota de um modelo pago —
combinado com `RNF-SEG-07` (API pública), é a superfície mais cara do produto
hoje.

## Integridade do dado exibido

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-DAD-12 | Dado fictício é declarado na tela | A página do ponto avisa quais campos são exemplo | 🟢 |
| RNF-DAD-13 | O mesmo número não diverge entre telas | Contador exibido em dois lugares sai do mesmo cálculo sobre a mesma base | 🟢 |
| RNF-DAD-14 | Regra de negócio é função pura e publicada | Selo, título e critério de conquista são função sobre contadores, e o critério aparece na tela | 🟢 |
| RNF-DAD-15 | Render é estável entre servidor e cliente | Nenhum valor aleatório ou relativo a `now` em mock; fuso fixo em UTC | 🟢 |
| RNF-DAD-16 | Campo coletado é campo gravado | O formulário não valida o que a API descarta | 🔴 |
| RNF-DAD-17 | Não há mock em caminho de produção | Nenhuma tela publicada lê de `src/mocks/` | 🔴 |

**RNF-DAD-16 é 🔴 por decisão consciente, não por descuido:** o formulário de
ponto coleta 8 campos e a API grava 3. O time aprovou o conceito e quis ver o
fluxo inteiro antes de mexer no banco. A tela avisa; o requisito segue violado
até a expansão do modelo.

**RNF-DAD-17 é 🔴 em onze telas** — seis de admin (todas menos `/admin/users`)
mais `/discover`, `/feed`, `/community`, `/pautas/[slug]` e `/profile` — e é o
requisito mais caro de fechar do documento inteiro, porque depende de todas as
tabelas que faltam. Rastreado em
[`todo/README.md`](../../todo/README.md) com 🟡, e a lista do que cada tabela
destrava está em
[`requisitos-funcionais/06-rastreabilidade.md`](../requisitos-funcionais/06-rastreabilidade.md).

## Privacidade e dado pessoal

| ID | Requisito | Critério de aceite | Estado |
|---|---|---|---|
| RNF-DAD-18 | Só se coleta o necessário | Cadastro pede nome, e-mail e senha — sem CPF, CNPJ nem documento | 🟢 |
| RNF-DAD-19 | O titular consegue excluir a própria conta | Caminho de exclusão na interface do usuário, não só no admin | 🔴 |
| RNF-DAD-20 | Existe política de privacidade e termo de uso | Documento acessível, aceito no cadastro | 🔴 |
| RNF-DAD-21 | Localização não é armazenada | A posição do navegador é usada para centralizar o mapa e descartada | 🟢 |
| RNF-DAD-22 | Perfil público expõe só o que o usuário sabe que é público | Nome, contribuição, selo e ranking; nunca e-mail | 🟡 |

**RNF-DAD-18 melhorou por cancelamento.** Cortar o dono de estabelecimento em
2026-08-19 tirou CNPJ do escopo, e com ele validação de documento, distinção
pessoa física/jurídica e o compliance que isso arrasta.

**RNF-DAD-21 vira 🔴 quando `Visita` nascer.** Histórico de check-in por GPS é
dado de localização com retenção — e o produto todo depende dele. Vale decidir
retenção e visibilidade **antes** da tabela, não depois.

**RNF-DAD-22 é 🟡 por não estar verificado:** o perfil público monta sobre mock,
então o recorte de campo ainda não passou por dado real. É a hora de conferir
que o e-mail não vaza — e o `RNF-SEG-10` (a API devolvendo `user_password`)
mostra que o cuidado com payload de saída não é hipotético neste projeto.

## Evidência

| ID | Onde |
|---|---|
| RNF-DAD-01..07 | `src/lib/gemini.ts`, `src/actions/stories.ts`, `src/validations/stories.ts` |
| RNF-DAD-12 | `(explorer)/places/[id]/page.tsx` |
| RNF-DAD-13 | `src/mocks/profile.ts` derivando de `currentExplorerMock` |
| RNF-DAD-14 | `src/constants/{verification,explorer-titles}.ts` |
| RNF-DAD-15 | `src/mocks/*.ts` — `PROFILE_ANCHOR`, `timeZone: "UTC"` |
| RNF-DAD-16 | `src/validations/markers.ts` × `src/actions/markers.ts` (`toMarkerInput`) |
| RNF-DAD-21 | Controle `showLocate` em `src/components/ui/map.tsx` |

---

➡️ [07 — Placar](./07-placar.md)
