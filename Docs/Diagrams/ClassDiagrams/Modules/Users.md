# Diagrama de Classes — Módulo de Usuários

Este diagrama representa a estrutura de classes C# do módulo de usuários, abrangendo Entidades de Domínio, Configurações de Mapeamento (Fluent API), Data Transfer Objects (DTOs) e a camada de Controladores.

```mermaid
classDiagram
    direction TB

    class User {
        +Guid Id
        +string Name
        +string Email
        +string PasswordHash
        +string Role
        +string? AvatarUrl
        +string? Biography
        +string? Neighborhood
        +DateTime CreatedAt
        +DateTime UpdatedAt
    }

    class UserConfiguration {
        +Configure(EntityTypeBuilder~User~ builder) void
    }

    class RegisterUserDto {
        +string Name
        +string Email
        +string Password
    }

    class UpdateUserDto {
        +string? Name
        +string? AvatarUrl
        +string? Biography
        +string? Neighborhood
    }

    class UserResponseDto {
        +Guid Id
        +string Name
        +string Email
        +string Role
        +string? AvatarUrl
        +string? Biography
        +string? Neighborhood
        +DateTime CreatedAt
        +FromEntity(User user)$ UserResponseDto
    }

    class UsersController {
        -AppDbContext _context
        +GetAll() Task~ActionResult~IEnumerable~UserResponseDto~~~
        +GetById(Guid id) Task~ActionResult~UserResponseDto~~
        +Register(RegisterUserDto request) Task~ActionResult~UserResponseDto~~
        +Update(Guid id, UpdateUserDto request) Task~ActionResult~UserResponseDto~~
        +Delete(Guid id) Task~IActionResult~
    }

    class AppDbContext {
        +DbSet~User~ Users
        #OnModelCreating(ModelBuilder modelBuilder) void
    }

    UserConfiguration ..> User : "configura schema"
    UsersController --> AppDbContext : "injeta"
    AppDbContext --> User : "gerencia DbSet"
    UsersController ..> RegisterUserDto : "consome no POST"
    UsersController ..> UpdateUserDto : "consome no PATCH"
    UsersController ..> UserResponseDto : "retorna nos GET/POST/PATCH"
    UserResponseDto ..> User : "converte via FromEntity"
```
