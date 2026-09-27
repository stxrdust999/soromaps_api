# Diagrama de Casos de Uso — Módulo de Usuários

Este diagrama descreve as interações funcionais dos atores (**Visitante Anônimo**, **Explorador Autenticado** e **Administrador**) com o módulo de usuários da API.

```mermaid
flowchart LR
    Visitante["👤 Visitante Anônimo"]
    Explorador["🧭 Explorador"]
    Admin["🛡️ Administrador"]

    subgraph ModuloUsuarios ["Módulo de Usuários"]
        UC01["UC01: Cadastrar Conta (POST /api/users)"]
        UC02["UC02: Consultar Usuário por ID (GET /api/users/:id)"]
        UC03["UC03: Atualizar Próprio Perfil (PATCH /api/users/:id)"]
        UC04["UC04: Listar Todos os Usuários (GET /api/users)"]
        UC05["UC05: Remover Usuário (DELETE /api/users/:id)"]
        UC06["UC06: Validar Duplicidade de E-mail (Regra de Negócio)"]
    end

    Visitante --> UC01
    Visitante --> UC02

    Explorador --> UC02
    Explorador --> UC03

    Admin --> UC04
    Admin --> UC05
    Admin --> UC03

    UC01 -.->|include| UC06
```
