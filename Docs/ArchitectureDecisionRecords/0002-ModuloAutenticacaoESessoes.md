# ADR-0002: Módulo de Autenticação e Sessões (JWT + Refresh Token Rotativo)

## Status

Accepted

## Data

2026-10-04

## Rodada / Módulo

01 Autenticação

---

## Contexto

A API do Soromaps necessitava de um mecanismo de autenticação robusto e desacoplado para proteger endpoints, autorizar chamadas do frontend (`soromaps_web`) e revogar sessões ativas com segurança.

Antes desta fase, os endpoints eram públicos por padrão, não havia controle de sessões ativas e tentativas de login estavam suscetíveis a ataques de força bruta e enumeração de contas via timing attack.

---

## Decisões Tomadas

### 1. Modelo de Sessões e Refresh Token Rotativo
- **Tabela `sessions`:** Chave primária `bigint`, vinculada com `user_id` (`Guid`) via deleção em cascata.
- **Hash Criptográfico SHA-256:** O refresh token bruto (gerado com 32 bytes de entropia via `RandomNumberGenerator`) é entregue ao cliente, mas o banco armazena exclusivamente o hash em hexadecimal (`char(64)` fixo com índice `UNIQUE`).
- **Rotação de Refresh Tokens:** A cada renovação em `POST /api/auth/refresh`, a sessão atual é invalidada (`revoked_at = DateTime.UtcNow`) e um par completamente novo de access/refresh tokens é emitido, mitigando riscos de reutilização ou interceptação de tokens.

### 2. Access Token JWT
- Assinado com HMAC-SHA256 (`Jwt:Key` gerado com chave de 256 bits configurada no User Secrets em desenvolvimento).
- Tempo de vida curto de 15 minutos (`ExpiresIn: 900`), com tolerância de clock zerada (`ClockSkew = TimeSpan.Zero`).
- Claims essenciais: `sub` (UUID do usuário), `email`, `role` (papel de acesso) e `jti` (nonce único).

### 3. Mitigação de Timing Attack no Login
- Quando um e-mail não é encontrado no banco, o algoritmo `BCrypt.Verify` é executado contra um hash BCrypt custo 12 pré-computado (`DummyHash`).
- Isso equaliza o tempo de resposta das requisições em ~100ms, impedindo invasores de enumerarem usuários válidos medindo a latência da API.

### 4. Fallback Policy Global e Rate Limiting
- **Fallback Policy:** Configuração de autorização global (`options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()`). Todos os endpoints da API são automaticamente privados por padrão, exigindo anotação explícita de `[AllowAnonymous]` apenas onde necessário.
- **Rate Limiting:** Política `LoginRateLimit` no `POST /api/auth/login` limitando o tráfego a 5 tentativas por janela fixa de 1 minuto por IP (`RemoteIpAddress`), rejeitando requisições excedentes com status `429 Too Many Requests`.

---

## Consequências e Trade-offs

### Positivas
- Segurança elevada: a API agora é fechada por padrão, impedindo que novos controllers fiquem expostos por descuido.
- Sessões auditáveis e revogáveis individualmente (`/logout`) ou em lote (`/logout-all`).
- Proteção contra força bruta no endpoint de login sem dependência de middlewares externos.

### Negativas / Limitações
- Chamadas públicas precisam ser marcadas explicitamente com `[AllowAnonymous]`.
- Refresh tokens exigem chamada ao banco para validação do hash (mitigado pelo índice único em `token_hash`).
