# ADR-0003: Arquitetura em Camadas por Tipo e Padronização de Responsabilidades

## Status

Accepted

## Data

2026-10-10

## Rodada / Módulo

Refatoração Geral (Fundação, Usuários, Autenticação, Lugares, Avaliações, Comentários, Categorias e Conquistas)

---

## Contexto

A base de código continha implementações com acoplamento excessivo nos Controllers, que realizavam diretamente acesso a banco via `AppDbContext`, instanciação manual de DTOs e entidades, manipulação direta de `ProblemDetails`, ausência de verificação de autoria (vulnerabilidades IDOR em User, Place, Review e Comment), além de controllers com problemas de compilação ou descoberta (como `AchievementesController` aninhado) e migrations com deleção em cascata perigosa em todas as relações.

Havia necessidade de estabelecer uma divisão estrita de responsabilidades, manter boas práticas de código, padronizar o tratamento de erros sem uso de exceções de controle de fluxo e garantir integridade referencial com anonimização de conteúdo.

---

## Decisões Tomadas

1. **Organização Física por Tipo de Componente:**
   - Adotada estrutura organizada estritamente por tipo: `Controllers/`, `Services/`, `DTOs/`, `Mappings/`, `Models/`, `Data/`, `Results/`, `Errors/`, `Options/`, `Constants/` e `Extensions/`.
   - Um arquivo por classe/record/interface, eliminando agrupamentos em classes wrapper.

2. **Acesso a Dados Direto nos Services (Sem Repositórios Genéricos):**
   - Os Services utilizam o `AppDbContext` diretamente, adotando a recomendação oficial da Microsoft para EF Core, onde `DbContext` já atua como Unidade de Trabalho e `DbSet<T>` como Repositório.
   - Controllers dependem exclusivamente de interfaces de serviço (`IPlaceService`, `IAuthService`, etc.), sendo proibido injetar `AppDbContext` na camada HTTP.

3. **Result Pattern para Fluxo de Erros de Negócio:**
   - Implementadas as estruturas `Result` e `Result<TValue>` acompanhadas de `Error` e `ErrorType` (`Validation`, `Unauthorized`, `Forbidden`, `NotFound`, `Conflict`).
   - Controllers herdam de `ApiControllerBase` e traduzem erros de negócio centralizadamente para `ProblemDetails` com mensagens amigáveis em português e códigos em inglês.
   - Exceções são reservadas apenas para falhas inesperadas do sistema (500).

4. **Padronização de Chaves Primárias e Estrangeiras:**
   - Todas as chaves primárias foram padronizadas como `Id` (incluindo `Session.Id`).
   - Todas as chaves estrangeiras foram padronizadas como `{Entidade}Id` (ex: `UserId`, `PlaceId`, `CategoryId`, `AuthorId`).

5. **Integridade Referencial e Anonimização de Dados (LGPD):**
   - Ao excluir um usuário, seus registros dependentes de valor público (`Places`, `Reviews`, `Comments`) são anonimizados via `ON DELETE SET NULL`, preservando a consistência das contribuições para a comunidade enquanto os dados do autor se tornam `null`.
   - `Sessions` são excluídas em cascata (`ON DELETE CASCADE`).
   - Exclusão de categoria com lugares vinculados é bloqueada via `ON DELETE RESTRICT`, retornando conflito amigável.
   - Restrição de unicidade aplicada para 1 review por usuário por lugar, com verificação de nota entre 1 e 5 via Check Constraint.

6. **Autorização Enxuta e Acesso Público:**
   - Endpoints de leitura (`GET` para lugares aprovados, categorias, reviews, comentários e conquistas) permitem acesso anônimo (`[AllowAnonymous]`).
   - Papéis restritos a `Explorer` e `Admin`. Operações administrativas e de alteração de status de lugares exigem papel `Admin`.
   - Usuários editam seus próprios lugares sem que o status regrida para `Pending`, mantendo as avaliações e visualização ativas.

7. **Projeções de Consulta Otimizadas:**
   - Mapeamentos de entidade para DTO isolados em `Mappings/` usando expressões `Expression<Func<T, TDto>>` estáticas, garantindo que o EF Core gere queries SQL contendo apenas as colunas necessárias (`SELECT ...`).

---

## Alternativas Consideradas e Descartadas

- **Camada de Repositório Genérico (`IRepository<T>`):** Descartada por duplicar a abstração já fornecida pelo Entity Framework Core, limitar o uso de queries assíncronas com `Include` e projeções personalizadas, e inflar a contagem de arquivos sem benefício técnico para o escopo do projeto.
- **Tratamento de Erros via Exceções de Domínio Customizadas:** Descartada por poluir o fluxo de execução com custo de captura de stack trace e tornar os contratos dos métodos implícitos.
- **Usuário Fallback "Guest" no Banco:** Descartada por ser considerada gambiarra conceitual que polui a tabela de usuários; a anulação da chave estrangeira com resposta desacoplada no DTO atende com maior clareza e conformidade com a LGPD.

---

## Consequências e Trade-offs

### Positivas (Ganhos)
- Eliminação total de vulnerabilidades IDOR e Mass Assignment nos fluxos de usuários, lugares e avaliações.
- Manutenibilidade elevada através de controllers enxutos (média de 30-50 linhas) focados unicamente no protocolo HTTP.
- Queries mais rápidas e com menos tráfego de memória devido à projeção explícita em DTOs e paginação obrigatória.
- Compatibilidade e consistência total nos nomes de colunas, constraints e índices do PostgreSQL.

### Negativas / Limitações (Custos aceitos)
- Alterações em propriedades dos contratos de API (`placeName` -> `name`, `lat`/`lng` -> `latitude`/`longitude`, etc.) que requerem atualização dos clientes consumidores no front-end.

---

## Referências e Dependências
- [ADR-0001: Fundação da API e Módulo de Usuários](0001-FundacaoEModuloUsuarios.md)
- [ADR-0002: Módulo de Autenticação e Sessões](0002-ModuloAutenticacaoESessoes.md)
