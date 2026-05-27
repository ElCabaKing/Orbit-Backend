# Orbit Social - Claude Context Docs

---

# 01_PROJECT_OVERVIEW.md

```md
# Orbit Social

Orbit Social es una red social inspirada en X/Twitter desarrollada en .NET 9.

El proyecto está diseñado con enfoque de producción y arquitectura escalable.

## Objetivos

- Crear una red social moderna con soporte realtime.
- Mantener arquitectura limpia y mantenible.
- Implementar comunidades, chats privados y sistema de notificaciones.
- Mantener separación clara entre dominio, infraestructura y aplicación.
- Permitir escalabilidad futura.

## Tecnologías Principales

- .NET 9
- ASP.NET Core
- Entity Framework Core
- SQL Server
- Redis
- SignalR
- Hangfire
- Cloudinary
- Docker
- Serilog

## Características Principales

- JWT Authentication
- Refresh Tokens
- Password Recovery (tokens en Redis con caducidad)
- Comunidades
- Reposts
- Threads
- Likes
- Bookmarks
- Chats privados
- WebSockets realtime
- Notificaciones
- Multimedia
- Moderación
- Reports
- Soft Delete
- Rate Limiting
- Background Jobs

## Arquitectura

El proyecto usa una arquitectura multicapa:

- ApiWeb
- Application
- Domain
- Infrastructure
- Shared

## Filosofía

- DTOs obligatorios
- Controllers estándar de ASP.NET Core
- Result Pattern
- Code First
- Fluent API
- Repositories específicos
- Uso extensivo de Redis
- Cronología simple para feeds
```

---

# 02_ARCHITECTURE.md

```md
# Arquitectura del Proyecto

## Estructura de Solución

src/
│
├── Orbit.ApiWeb
├── Orbit.Application
├── Orbit.Domain
├── Orbit.Infrastructure
└── Orbit.Shared

---

# Orbit.ApiWeb

Contiene:

- Controllers
- Middleware
- Decorators
- Request DTOs
- Configuración JWT
- Configuración SignalR
- Configuración CORS
- Startup del proyecto
- Swagger
- Dependency Injection

NO debe contener:

- lógica de negocio
- acceso directo a EF
- queries complejas

---

# Orbit.Application

Contiene:

- Casos de uso
- Helpers
- Response DTOs
- Interfaces de servicios
- Validators
- Result Pattern
- Features

Responsabilidad:

Coordinar la lógica de aplicación.

---

# Orbit.Domain

Contiene:

- Entidades
- ValueObjects
- Enums
- Excepciones
- Interfaces de repositorios
- Constantes

Las entidades serán implementadas mediante Code First.

NO debe depender de Infrastructure.

---

# Orbit.Infrastructure

Contiene:

- DbContext
- Configuraciones EF
- Repositories
- Servicios externos
- Redis
- Cloudinary
- SignalR
- JWT
- Hangfire
- Persistencia

---

# Orbit.Shared

Contiene:

- Utilities
- Extensions
- Funciones reutilizables
- Helpers genéricos

NO debe contener lógica de negocio.
```

---

# 03_DATABASE_RULES.md

```md
# Reglas de Base de Datos

## ORM

Se usará Entity Framework Core.

## Estrategia

Code First.

Las entidades serán creadas manualmente en C#.

Las tablas serán generadas mediante migrations.

## Reglas

- NO usar Database First.
- NO modificar tablas manualmente en producción.
- Usar Fluent API.
- Evitar DataAnnotations EF.
- Configuraciones separadas por entidad.

## Convenciones

- SQL en snake_case.
- C# en PascalCase.
- GUIDs como PK.
- Soft delete mediante deleted_at.
- Índices en campos críticos.

## Conteos

Los conteos importantes serán desnormalizados:

- followers_count
- following_count
- likes_count
- replies_count
- reposts_count
- member_count

NO usar vistas para conteos críticos.

## Relaciones importantes

- follows
- user_bans
- muted_users
- posts
- post_media
- communities
- conversations
- messages
- notifications
```

---

# 04_AUTH_AND_SECURITY.md

```md
# Autenticación y Seguridad

## Sistema de autenticación

- JWT Access Token
- Refresh Tokens

## Reglas

- JWT enviado mediante Authorization Header.
- Refresh token rotation.
- Revocación de sesiones.
- CORS configurado únicamente para frontend autorizado.

## Password Hashing

Se usa **BCrypt** (vía `BCrypt.Net-Next`) con `EnhancedHashPassword` y work factor **13**.

### Interface (`IPasswordHasher`)

- `Hash(password)` → retorna el hash con salt incorporado.
- `Verify(password, hash)` → verifica contraseña contra el hash.

### Reglas

- Nunca almacenar contraseñas en texto plano.
- El hash incluye el salt internamente (formato BCrypt).
- Work factor 13 como balance seguridad/rendimiento.
- La implementación concreta (`BCryptPasswordHasher`) está en Infrastructure; la interfaz en Application.

## Password Recovery

El sistema de recuperación de contraseña funciona mediante tokens generados como strings aleatorios seguros.

### Flujo

1. Usuario solicita recuperación → se genera un token único (string aleatorio).
2. El token se almacena en **Redis** con una **caducidad** definida (ej: 15 minutos).
3. Se envía un enlace con el token al email del usuario.
4. Usuario envía token + nueva contraseña → se valida en Redis → se actualiza la contraseña.
5. El token se elimina de Redis tras usarse (uso único).

### Reglas

- Los tokens se almacenan exclusivamente en Redis (NO en SQL).
- TTL obligatorio en cada token.
- Un solo token activo por usuario a la vez (se revoca el anterior).
- Token de un solo uso: se invalida al cambiarla contraseña.
- El token debe ser un string seguro generado con criptografía aleatoria.

## Rate Limiting

Implementado con Redis.

## Límites iniciales

| Acción | Límite |
|---|---|
| Posts | 5/min |
| Likes | 30/min |
| Reposts | 20/min |
| Follows | 10/min |
| Mensajes | 20/min |
| Uploads | 10/min |
| Login attempts | 5/5min |

## Bloqueos

Si un usuario bloquea a otro:

El bloqueado NO podrá:

- ver perfil
- ver posts
- interactuar
- comentar
- dar like
- repostear
- enviar mensajes
- seguir
- mencionar

## Middleware

Habrá middleware para:

- manejo global de errores
- JWT validation
- logging
- request tracking
- rate limiting
```

---

# 05_REALTIME_AND_NOTIFICATIONS.md

```md
# Realtime y Notificaciones

## Tecnología

SignalR.

## Redis

Redis será usado para:

- caché
- websocket scaling
- rate limiting
- unread counts
- online users
- notification queues

## Realtime Features

- chats privados
- notificaciones
- mentions
- unread messages
- follows
- likes
- reposts

## Background Jobs

Se usará Hangfire.

## Jobs

- envío de notificaciones
- menciones
- procesamiento multimedia
- feeds
- tareas diferidas

## Tipos de notificaciones

- like
- reply
- mention
- follow
- repost
- message
- community_invite
- sensitive_content
```

---

# 06_MEDIA_RULES.md

```md
# Multimedia

## Proveedor

Cloudinary.

## Estrategia

El backend NO almacenará archivos binarios.

Solo se almacenará:

- URL
- public_id
- metadata

## Límites

### Imágenes

- máximo: 5MB

### Videos

- máximo: 15MB

## Validaciones

Se usarán decoradores/attributes personalizados.

Ejemplos:

- MaxFileSize
- AllowedExtensions
- AllowedMimeTypes

## Tipos permitidos

### Imágenes

- jpg
- jpeg
- png
- webp

### Videos

- mp4
- webm
```

---

# 07_FEED_AND_COMMUNITIES.md

```md
# Feed y Comunidades

## Feed

El feed será cronológico.

## Tipos de contenido

- posts
- reposts
- replies
- quote posts
- publicaciones de comunidades

## Comunidades

Las comunidades tendrán:

- líder
- co-líderes opcionales
- miembros

## Permisos

Líderes y co-líderes podrán:

- expulsar miembros
- eliminar posts dentro de la comunidad

## Privacidad

Comunidades privadas:

- solo miembros pueden visualizar contenido.

## Moderación

Moderadores globales podrán:

- bloquear usuarios
- marcar contenido sensible
- eliminar contenido
```

---

# 08_DEVELOPMENT_STANDARDS.md

```md
# Estándares de Desarrollo

## Controllers

Usar Controllers estándar de ASP.NET Core.

## DTOs

Todos los endpoints usarán DTOs.

NO retornar entidades EF.

## Result Pattern

Todos los endpoints retornarán:

- success
- message
- data
- errors opcionales

## Paginación

Máximo 10 elementos por página.

Preferir cursor pagination.

## Logging

Usar Serilog.

## Testing

Solo testing crítico:

- auth
- chats
- posts
- follows
- moderación

## Docker

Todo el sistema debe ser dockerizable.

## Deploy

Render.

## Naming

- PascalCase en C#
- snake_case en SQL
- REST conventions en endpoints

## Soft Delete

Usar `is_active` (BIT) con query filter global.
`BaseEntity` + `ISoftDeletable` en Domain. `GenericRepository` aplica soft delete automáticamente.

## Código

- evitar lógica duplicada
- evitar controllers gigantes
- usar servicios y casos de uso
- mantener separación de capas
```

---

# 09_INITIAL_MVP.md

```md
# MVP Inicial

## Features prioritarias

### Auth

- register
- login
- refresh token
- logout
- password recovery (solicitar reset, resetear password)

### Profiles

- editar perfil
- seguir usuarios
- bloquear usuarios
- mute users

### Posts

- crear posts
- replies
- reposts
- likes
- bookmarks
- hashtags
- mentions
- multimedia

### Communities

- crear comunidad
- unirse
- expulsar miembros
- eliminar posts

### Chats

- chat privado
- unread count
- editar mensajes
- eliminar mensajes

### Notifications

- realtime notifications
- mentions
- likes
- follows
- chats

## Infraestructura mínima

- SQL Server
- Redis
- SignalR
- Hangfire
- Cloudinary
- Docker
```

