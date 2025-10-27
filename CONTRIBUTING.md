# Contribución al Backend (DoctorWare)

Este documento resume las pautas para agregar o modificar endpoints en la API.

## Principios
- Respuestas de éxito: usa `Ok(...)` / `Created(...)`. Para módulos de negocio, se prefieren los helpers de `BaseApiController` con `ApiResponse`.
- Errores: no construyas respuestas a mano. Lanza excepciones y deja que el middleware las formatee.
- Capa de negocio: la lógica va en `Services`; `Repositories` solo acceden a datos (Dapper).
- Transacciones: abre/cierra explícitamente con `IDbConnection` + `IDbTransaction` cuando el caso de uso lo requiera.
- JSON: camelCase global (configurado en `Program.cs`).

## Manejo de errores (obligatorio)
- Lanza excepciones, no retornes objetos de error:
  - `throw new BadRequestException("mensaje");`
  - `throw new NotFoundException("mensaje");`
  - `throw new UnauthorizedAccessException("mensaje");`
- El middleware `ErrorHandlingMiddleware` convierte las excepciones en `ApiResponse` homogéneo.
- 401/403 producidos por JWT también responden JSON (eventos de `JwtBearer`).
- 404/405 sin excepción se devuelven en JSON (`UseStatusCodePages`).

## Controladores
- Marca con `[ApiController]` y `[Route("api/[controller]")]`.
- Preferir heredar de `BaseApiController` y usar sus helpers para respuestas de éxito en endpoints de negocio.
- Validación de modelos: DataAnnotations + `AddStandardizedApiBehavior` (400 uniforme). No formatear manualmente `ModelState`.
- Autenticación/autorización: usa `[Authorize]` donde aplique.
- Patrón recomendado de flujo:
  1. Validaciones de entrada (si no cubre DataAnnotations) → lanzar `BadRequestException`.
  2. Cargar entidades desde repos → si no existen, `NotFoundException`.
  3. Llamar a `Services` para la lógica.
  4. Retornar `OkResponse(...)` / `CreatedResponse(...)`.

## Auth (compatibilidad)
- Endpoints de Auth devuelven éxito en forma plana (sin `ApiResponse`) por compatibilidad con el front:
  - `login`, `refresh`, `me` → `{ token, refreshToken, user, expiresIn }` o `user`.
  - `register` → `201 { message, requiresEmailConfirmation: true }`.
- Todos los errores en Auth se devuelven en JSON estándar por el middleware.

## Repositorios y Servicios
- Repos (`Repositories/*`):
  - Sin lógica de negocio; solo SQL, parámetros y mapeo.
  - Usa `IDbConnectionFactory` para obtener conexiones.
- Servicios (`Services/*`):
  - Orquestan casos de uso, validan reglas, abren transacciones cuando corresponde.
  - Inyecta repositorios y utilidades (p.ej. `IEmailSender`).

## Seguridad
- Nunca loguees credenciales, tokens o datos sensibles.
- Contraseñas: usa `PasswordHasher` del proyecto.
- Confirmación de email:
  - Guarda SOLO el hash (SHA-256) del token.
  - Reenvío con cooldown configurable.
  - Expiración configurable (`EmailConfirmation:TokenMinutes`), `0` = sin vencimiento.

## Configuración
- Usa `appsettings*.json` para Development. En entornos administrados (Render, etc.): variables de entorno.
- Claves importantes (nombre de variable de entorno sugerido entre paréntesis):
  - DB: `ConnectionStrings:ConexionPredeterminada` (`ConnectionStrings__ConexionPredeterminada`).
  - JWT: `Jwt:Secret` (`Jwt__Secret`).
  - CORS: `Cors:AllowedOrigins` (`Cors__AllowedOrigins`).
  - Email SMTP: `Email:Smtp:*` (`Email__Smtp__Host`, `Email__Smtp__User`, ...).
  - Confirmación: `EmailConfirmation:*` (`EmailConfirmation__TokenMinutes`, ...).

## Estilo y organización
- Nombres: PascalCase para clases/métodos; camelCase para parámetros/variables.
- Mantén las firmas y estilos coherentes con el código existente.
- Documenta endpoints con `<summary>` y tipos en Swagger cuando sea útil.

## Checklist al agregar un endpoint
- [ ] DTOs de Request/Response creados bajo `DTOs/`.
- [ ] Validación por DataAnnotations en DTOs (y reglas extra con excepciones si aplica).
- [ ] Lógica en `Services`; acceso a datos en `Repositories`.
- [ ] Controlador lanza excepciones en errores (no construir respuestas manuales).
- [ ] Swagger compila y describe el endpoint.
- [ ] CORS permite el origen esperado en Development.
- [ ] Logs razonables (sin PII).

