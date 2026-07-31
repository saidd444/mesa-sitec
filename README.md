# MesaSitec - sistema de mesa de servicio multitenant

sistema SaaS de gestion de solicitudes de servicio
usando .net 8 y frontend vue3
# Stack

- backend : .Net 8, EF Core, Sqlite, jwt
- frontend : vue 3, TypeScript, vite, pinia
- test: Xunit


## ------- LEVANTAR EL PROJECTO ----------
## backend
```bash
cd MesaSitec
dotnet build
dotnet run

Servidor en 'http://localhost:5298'
Swagger en 'http://localhost:5298/swagger'

## credenciales del test 

Email: admin@norte.test
Contraseña: Sitec.2026

### Base de daots

SQlite automatica, datps semilla se crean

## endpoints complteados

 POST /auth/login — Autenticación JWT
 GET /me — Datos del usuario autenticado
 GET /categorias — Listado de categorías
 GET /solicitudes — Listado paginado, filtrado, búsqueda
 POST /Solictudes - Crear solicitudes
 GET /solicitudes{id} - detalle de solicitud
 PUT /solicitudes{id} - editar solicitud
 POST /solicitudes/{id}/transiciones - cambiar estado (maquina de estados)


 ## Arquitectura

- Models: Entidades del dominio
- Services: Lógica de negocio (AuthService)
- Controllers: Endpoints API
- Data: DbContext, migraciones
- Dtos: Objetos de transferencia

**Seguridad:**
- JWT con MapInboundClaims = false (mantiene claims originales)
- Filtro por TenantId en TODO query (RN-01: aislamiento multitenant)
- PasswordHasher de ASP.NET Core (bcrypt)

## Reglas de negocio implementadas

- RN-01: Aislamiento de datos por tenant
- RN-02: Máquina de estados para solicitudes
- RN-04: Cálculo automático de SLA según prioridad y categoría
- RN-07: Código de solicitud secuencial por tenant/año

##Notas
- Backend 100% funcional, robusto
- Todas las decisiones documentadas en DECISIONES.md