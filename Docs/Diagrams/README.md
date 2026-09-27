# 🗺️ Diagramas do Sistema — Soromaps API

Este diretório contém a modelagem visual completa da API e do banco de dados, utilizando **Markdown + Mermaid** e **DBML (Database Markup Language)**.

## 📁 Estrutura de Pastas

```text
Docs/Diagrams/
├── ClassDiagrams/             # Diagramas de Classes (C# / Arquitetura de Software)
│   ├── Complete.md            # Visão completa das entidades, DTOs e Controllers
│   └── Modules/               # Diagramas isolados por contexto
│       └── Users.md           # Módulo de Usuários
├── UseCaseDiagrams/           # Diagramas de Casos de Uso (Interações dos Atores)
│   ├── Complete.md            # Visão completa do sistema (Explorador, Admin)
│   └── Modules/
│       └── Users.md           # Casos de uso do módulo de Usuários
├── EntityRelationship/        # Diagramas ER (Modelo Lógico e Físico de Banco)
│   ├── Complete.md            # DER geral de todas as tabelas do banco
│   └── Modules/
│       └── Users.md           # DER isolado em torno da tabela users
└── Dbml/                      # Especificações em formato DBML para o dbdiagram.io
    ├── README.md              # Instruções de importação no dbdiagram.io
    ├── Schema.dbml            # DBML consolidado do banco completo
    └── Modules/
        └── Users.dbml         # DBML específico do módulo de usuários
```

Todos os diagramas utilizam blocos de código nativos do GitHub/GitLab com renderização automática via Mermaid.
