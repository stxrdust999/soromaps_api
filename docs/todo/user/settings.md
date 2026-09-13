# ⚙️ Configurações

> Área: usuário público · Rota: `/settings` · Status: 💤 não iniciado

## Ideia

Página de conta e preferências, em seções: **Perfil** (nome, e-mail, foto), **Segurança** (trocar senha — com a senha atual exigida), **Aparência** (tema claro/escuro/sistema, que o app já suporta via `next-themes` mas não expõe em lugar nenhum) e **Zona de perigo** (excluir conta).

## Por que vale

- Todo app precisa; a ausência grita mais que a presença brilha.
- É o lugar natural de crescer: privacidade do perfil, preferências de notificação e idioma moram aqui depois.
- Excluir a própria conta é obrigação básica de respeito ao dado do usuário (e LGPD manda).

## Dependências

| O quê | Situação |
|---|---|
| `PATCH`/update parcial de usuário na API | 🔴 **bloqueador direto** — o `PUT` atual re-hasheia a senha em toda chamada; editar o nome não pode redefinir a senha |
| Trocar senha exigindo a atual | ❌ endpoint novo (`POST /api/users/{id}/change-password` verificando com BCrypt antes) |
| Excluir conta | 🟡 `DELETE` existe, mas sem autenticação qualquer um exclui qualquer conta |
| Foto de perfil | ❌ depende de upload (RF-08) |

## Escopo inicial

- Formulário de perfil (nome, e-mail) no padrão RHF + Zod + server action
- Troca de senha em card separado, com senha atual
- Seletor de tema
- Excluir conta com confirmação digitada (padrão do modal de exclusão)

## Fora do escopo inicial

- Foto de perfil (espera RF-08)
- Preferências de notificação (espera [Notificações](./notifications.md))
- Exportar meus dados
