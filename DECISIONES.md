Decicion 1 
Elegí JWT porque el enunciado lo pedía, pero también porque es stateless. Significa que el servidor no guarda sesiones. Cada token que devuelvo tiene toda la información: quién eres, qué rol tienes, de qué empresa. Es más simple que guardar sesiones en la BD, especialmente para multitenant.

Decicion 2 [Devolver DTOs en lugar de modelos]
Los modelos tienen campos que no quiero que vea el frontend: PasswordHash, datos internos. Entonces creé DTOs: son objetos que solo tienen lo que el frontend necesita. Es más seguro y el cliente no recibe basura.

Decision 3 [La máquina de estados con switch]
El enunciado pedía una máquina de estados para cambiar el estado de las solicitudes. Pude hacerlo con if/else, pero es más confuso. Con un switch, veo todas las transiciones validas faciles. Nueva > Asignada, Nueva > Cancelada, etc. Es más claro de mantener.

Sobre IA

Mi lenguaje principal es Java (Intermedio), no C#. Usé Claude para aprender la sintaxis de .NET. Pero todos los errores los debuggué yo mismo. Cuando algo no compilaba, leía el error, entendía qué pasaba, y lo corregía. IA me enseñó cómo escribir en C#, pero la lógica y las correcciones fueron mías.

Lo realmente dificil para mi fue

Me trabe en el endpoint /me. El JWT llegaba correctamente, pero no encontraba los datos del usuario. Agregué prints en la terminal para ver qué información tenía el JWT. Descubrí que .NET estaba convirtiendo mi claim 'sub' en una URL extraña. Busqué en Stack Overflow, encontré que necesitaba agregar MapInboundClaims = false, y funcionó. Ese fue un buen aprendizaje sobre cómo .NET maneja los tokens.

Lo que haría diferente con más tiempo

1. Tests unitarios completos (ahora están parciales)
2. Frontend Vue 3 con todas las vistas
3. Validaciones más estrictas en DTOs (FluentValidation)
4. Logging con Serilog
5. Docker para ambiente consistente