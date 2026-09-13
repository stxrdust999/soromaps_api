# 01. Sessão e acesso — baixo nível

Detalha a seção Sessão de [02 — Explorador](../02-explorador.md).
Único fluxo do produto inteiramente sobre dado real, junto com `/admin/users`.

## Entrar

```mermaid
flowchart LR
    VIS(("👤 Visitante"))
    API[["🖥️ API Soromaps"]]

    VIS --- ENTRAR(["Entrar ✅"])

    ENTRAR -.->|include| PREENCHE(["Informar usuário e senha ✅"])
    ENTRAR -.->|include| VALIDA[/"Validar o formulário com Zod ✅"/]
    VALIDA -.->|include| CONFERE[/"Conferir credencial na API ✅"/]
    CONFERE -.->|include| ASSINA[/"Assinar o JWT HS256 ✅"/]
    ASSINA -.->|include| GRAVA[/"Gravar o cookie httpOnly ✅"/]
    GRAVA -.->|include| REDIR[/"Redirecionar para /home ✅"/]

    VALIDA -.->|extend| ERRO_FORM(["Ver erro no campo ✅"])
    CONFERE -.->|extend| ERRO_CRED(["Ver credencial inválida ✅"])
    CONFERE -.->|extend| ERRO_REDE(["Ver falha de conexão ✅"])

    CONFERE --- API
```

`Validar o formulário`, `Conferir credencial`, `Assinar` e `Gravar` são passos
do sistema: acontecem dentro de `loginAction`, sem nova interação.

**Quem confere a senha e quem assina a sessão são sistemas diferentes.** A API
valida com BCrypt e devolve o usuário; o Next assina o JWT e grava o cookie. É
o que deixa o `middleware.ts` decidir acesso no runtime Edge sem ida à API a
cada navegação.

## Cadastrar conta

```mermaid
flowchart LR
    VIS(("👤 Visitante"))
    API[["🖥️ API Soromaps"]]

    VIS --- CAD(["Cadastrar conta ✅"])

    CAD -.->|include| DADOS(["Informar nome, e-mail e senha ✅"])
    CAD -.->|include| VAL2[/"Validar com createUserSchema ✅"/]
    VAL2 -.->|include| CRIA[/"Criar o usuário na API ✅"/]
    CRIA -.->|include| HASH[/"Gerar o hash BCrypt ✅"/]
    CRIA -.->|include| SESSAO[/"Abrir sessão logo após o cadastro ✅"/]

    CRIA -.->|extend| DUPLICADO(["Ver falha de cadastro ✅"])

    CRIA --- API
    HASH --- API
```

**O cadastro já entra logado** — `registerAction` cria o usuário e emite o
cookie na mesma chamada, sem passar pela tela de login.

**`Ver falha de cadastro` é genérico por omissão, não por escolha:** sem
`UNIQUE` em `user_name`/`user_email` (`RNF-SEG-13`), cadastrar um nome que já
existe **não falha** — cria a segunda conta e torna o login ambíguo.

## Sair e guarda de rota

```mermaid
flowchart LR
    EXP(("👤 Explorador"))

    EXP --- SAIR(["Sair ✅"])
    SAIR -.->|include| APAGA[/"Apagar o cookie de sessão ✅"/]

    EXP --- NAVEGA(["Navegar para rota protegida ✅"])
    NAVEGA -.->|include| LE[/"Ler o cookie no middleware ✅"/]
    LE -.->|include| VERIFICA[/"Verificar a assinatura e a expiração ✅"/]
    VERIFICA --> DEC{"Sessão válida?"}
    DEC -->|sim| SEGUE[/"Seguir para a rota ✅"/]
    DEC -->|não| LOGIN[/"Redirecionar para /login ✅"/]

    EXP --- AUTH(["Abrir /login já autenticado ✅"])
    AUTH -.->|include| HOME[/"Redirecionar para /home ✅"/]
```

`Sair` não toca a API: sessão é do Next, então encerrar é apagar o cookie.

---

## Descrições dos casos

As descrições em template expandido — ator, pré e pós-condição, ações do
ator × ações do sistema numeradas e restrições — vivem em
[`descricoes/01-sessao-e-acesso.md`](../descricoes/01-sessao-e-acesso.md): Cadastrar conta, Entrar, Sair e Acessar rota protegida.

---

➡️ [02 — Mapa e ponto](./02-mapa-e-ponto.md)
