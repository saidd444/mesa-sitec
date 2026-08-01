## Requisitos previos

- **.NET 8** — [descargar]
- **Node.js 18+** — [descargar]
- **SQLite** — incluido en .NET
 - Yo use Visual Studio Code

--------------------------------1. Backend .NET-------------------------------- 

 Comandos
    cd MesaSitec
    dotnet build
    dotnet run

Acceso:
- API: http://localhost:5298
- Swagger: http://localhost:5298/swagger
- Health: GET http://localhost:5298/api/v1/health

--------------------------------2. Frontend -------------------------------- 

En otra terminal:

Comandos
    cd MesaSitec-Frontend
    npm install
    npm run dev

    Acceso:
- Frontend: `http://localhost:5173`

-------------------------------- Credenciales de prueba -------------------------------- 

    Email: admin@norte.test
    Contraseña: Sitec.2026
    Rol: Admin

    agente1@norte.test (Agente)
    user1@norte.test (Solicitante)
    admin@sur.test (Admin 2)

-------------------------------- IMPLEMENTADO --------------------------------

- Backend: 9 endpoints (login, me, categorías, solicitudes CRUD, transiciones, health)
- JWT con mapeo de claims correcto
- Multitenant con filtrado por tenant en TODAS las queries
- Máquina de estados (Nueva → Asignada → EnProceso → Resuelta → Cerrada/Cancelada)
- Cálculo automático de SLA según prioridad y categoría
- Frontend: Login, Listado (con 3 estados), Detalle, Crear/Editar solicitudes
- Navbar con logout
- data-testid en todos los elementos

-------------------------------- NO IMPLEMENTADO --------------------------------

- Tests unitarios (xUnit) — Backend funcional pero sin suite completa
- Modales de acciones (asignar, resolver, cerrar) — Frontend muestra botones pero sin modal
- Buttons de acción dinámica según estado/rol — Estructura lista, lógica parcial
- Paginación dinámica — Hardcodeada a página 1

-------------------------------- Estructura --------------------------------

├─ MesaSitec/ (backend .NET 8)
│ ├─ Controllers/
│ ├─ Models/
│ ├─ Dtos/
│ ├─ Services/
│ ├─ Data/
│ └─ Program.cs
├─ MesaSitec-Frontend/ (frontend Vue 3)
│ ├─ src/
│ │ ├─ pages/
│ │ ├─ components/
│ │ ├─ stores/
│ │ ├─ services/
│ │ ├─ types/
│ │ └─ router.ts
│ └─ index.html
├─ README.md
└─ DECISIONES.md

-------------------------------- Proximo --------------------------------

1. Completar suite de tests unitarios (8+ pruebas)
2. Implementar modales para acciones (resolver, cancelar, etc.)
3. Validación más estricta en frontend (FormValidation)
4. Reorganizar con estructura `backend/` y `frontend/` en carpetas separadas
5. Docker Compose para levantar todo con un comando
