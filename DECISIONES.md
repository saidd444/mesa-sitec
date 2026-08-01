# Decisiones Técnicas

 1. JWT en lugar de sesiones guardadas en BD

Usé JWT porque es stateless: el servidor no guarda nada, solo valida el token. Para multitenant es más simple que sincronizar sesiones entre tablas. El token contiene quién eres, tu empresa y tu rol.

---

2. Devolver DTOs en lugar de los modelos de EF directamente

Los modelos tienen `PasswordHash` y campos internos que el frontend no debería ver. Los DTOs devuelven solo lo necesario. Es más seguro y desacopla el frontend del modelo.

---

3. Máquina de estados con switch en lugar de if/else

Pude usar if/else o una librería. Con switch ves todas las transiciones válidas (Nueva → Asignada, etc.) en un lugar. Es más mantenible.

---

 Qué hice con IA y qué hice yo

**Claude**
- Cómo estructurar un proyecto .NET 8 con controllers
- Cómo usar EF Core y migraciones
- Templates básicos de DTOs y modelos
- Cómo armar un proyecto Vue 3 con router y Pinia
- Ejemplos de cómo conectar un frontend a una API
 - Frontend (mayormente IA):** No sé nada de Vue. Claude generó la estructura de componentes, el router, Pinia, los formularios.

**Yo**
- Typos en los DTOs por ejemplo (`Titutlo` en lugar de `Titulo`) — me costó encontrarlo
- El problema de los claims en JWT (por qué `/me` devolvía 401)
- Entender por qué no se comparaban bien los Guids como strings
- Todos los errores de compilación
- Conectar el apiClient para inyectar el token automáticamente
- Agregué los `data-testid`, conecté con la API, debuggué errores de integración y adapté componentes.

En resumen: Claude me enseñó la sintaxis y estructura de .NET y Vue porque mi lenguaje principal es Java. Yo construí la lógica, entendí cómo funciona cada cosa, y arreglé todos los errores que salieron.

---

 Dónde me atasqué

**El problema:** El endpoint `/me` devolvía 401 Unauthorized aunque mandaba un JWT válido.

**Lo que hice:** Agregué un `Console.WriteLine()` para ver qué claims llegaban al controller. Descubrí que .NET había convertido mi claim `"sub"` en una URL XML larga como `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`.

Cuando intentaba hacer `User.FindFirst("sub")`, no lo encontraba porque ahora se llamaba de otra forma.

**La solución:** Encontré en Stack Overflow que hay que agregar MapInboundClaims = false en la configuración de JWT.

options.MapInboundClaims = false;

*Qué aprendí:* NET remapea automáticamente los claims por compatibilidad con estándares viejos. Se pierden horas debuggeando. Fue un buen reminder de que los frameworks hacen cosas implícitas.

---

## Si tuviera una semana más

- Escribir los 8 tests unitarios en xUnit que pide el enunciado
- Implementar los modales para resolver, cancelar y asignar solicitudes (ahora solo tengo los botones)
- Hacer que los botones de acción se muestren o no según el rol y estado (RN-02 y RN-03)
- Paginación real con navegación entre páginas
- Validaciones más estrictas usando FluentValidation
- Logging con Serilog para trackear errores en producción
- Docker Compose para que todo levante sin instalar nada
- Reorganizar la estructura en carpetas `backend/` y `frontend/` separadas