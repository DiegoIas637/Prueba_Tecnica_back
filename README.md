# SeguraVida — Venta de seguros (prueba técnica)

Prototipo local de registro y consulta de ventas de seguros de accidentes personales
con cupos limitados. Backend en C# / .NET con Clean Architecture y Entity Framework Core;
frontend en React + TypeScript (una sola pantalla).

> Documento de entrega. Cubre instrucciones de ejecución, comprobaciones R1–R5,
> decisiones, límites, pruebas y registro de uso de IA.

---

## 1. Stack y versiones usadas

| Componente | Versión |
|---|---|
| .NET SDK | 10.0.401 |
| Target framework | net10.0 |
| EF Core (SqlServer / Design / InMemory) | 10.0.12 |
| Swashbuckle.AspNetCore (Swagger) | 10.3.0 |
| xUnit | 2.9.3 |
| FluentAssertions | 8.11.0 |
| Base de datos | SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) |
| Frontend | React + TypeScript (Vite, dev server en `:5173`) |

Origen del esqueleto backend: plantilla `webapi` de .NET (`dotnet new webapi`) reorganizada
en 5 proyectos de Clean Architecture.

---

## 2. Estructura de la solución

```
Prueba_Tecnica.slnx
├── Domain              # Entidades, reglas de negocio puras, excepciones, interfaces de repositorio
├── Application         # Casos de uso (servicios), DTOs, validadores, abstracciones (UoW, lock)
├── Infrastructure      # EF Core (DbContext, repositorios, migración), seeder, lock en memoria
├── Prueba_Tecnica      # Capa API: controllers, middleware de errores, identidad del asesor, Swagger
├── Application.Tests   # Pruebas automatizadas de las reglas de negocio
└── scripts/            # Scripts SQL de base de datos
```

Dependencias (regla de Clean Architecture, hacia adentro):
`API → Application → Domain` y `API → Infrastructure → Application/Domain`.
El dominio no conoce EF ni ASP.NET.

---

## 3. Requisitos previos

- .NET SDK 10 (`dotnet --version` → `10.0.x`).
- SQL Server LocalDB (incluido con Visual Studio). Verificar con `sqllocaldb info`.
- Node.js + npm para el frontend.
- Opcional: `sqlcmd` para ejecutar los scripts manualmente.

---

## 4. Cómo ejecutar

### 4.1 Backend

1. La cadena de conexión ya está configurada en
   `Prueba_Tecnica/appsettings.json` y `appsettings.Development.json`:

   ```
   Server=(localdb)\MSSQLLocalDB;Database=SeguraVida;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
   ```

   Si usas otro servidor, cambia solo `Server=` y, si quieres otro nombre, `Database=`.

2. Ejecuta la API:

   ```powershell
   dotnet run --project Prueba_Tecnica
   ```

   Al arrancar en `Development`:
   - Aplica la migración `InitialCreate` (crea la BD `SeguraVida` si no existe).
   - Siembra los 2 planes de campaña y 10.000 ventas de historial (solo la primera vez).
   - Abre Swagger en el navegador automáticamente en `http://localhost:5162/swagger`.

   Puertos: `http://localhost:5162` (perfil `http`) o `https://localhost:7036` (perfil `https`).

### 4.2 Carga de la base de datos por scripts (alternativa manual)

En `scripts/`, ejecutar en orden:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -i scripts\00_create_database.sql   # crea la BD
sqlcmd -S "(localdb)\MSSQLLocalDB" -i scripts\01_schema.sql            # tablas + índices + migración
sqlcmd -S "(localdb)\MSSQLLocalDB" -i scripts\02_seed_plans.sql        # 2 planes de campaña
```

Los tres scripts son idempotentes. Las 10.000 ventas de historial las carga el seeder de
la API al arrancar (no hay script SQL para ese volumen; es dato reproducible por semilla fija).

Para desactivar la carga automática al arrancar, en `appsettings.json`:
`"Database:SeedOnStartup": false` y/o `"Database:ApplyMigrationsOnStartup": false`.

### 4.3 Frontend

Desde la carpeta del proyecto React:

```powershell
npm install
npm run dev
```

Abre `http://localhost:5173`. El dev server usa un proxy de Vite que reenvía `/api/*` al
backend, por lo que **el backend debe estar corriendo en paralelo**.

> Si ves `502 Bad Gateway` en `http://localhost:5173/api/...`, el backend no está arriba
> o el proxy de Vite apunta a otro puerto/esquema. El `target` del proxy debe ser
> `http://localhost:5162` (perfil `http`).

---

## 5. Identidad del asesor (simulación)

No hay login real. El asesor activo se transmite en el header HTTP **`X-Advisor-Id`** con
valor `A` o `B`.

- **Cómo se establece:** el frontend envía el header; en Swagger hay un campo para
  escribirlo (por defecto `A`); con `curl`/REST se añade manualmente.
- **Cómo se valida el acceso:** el backend lee el asesor del header en cada request y
  **filtra siempre por él**. El listado de ventas (R4) nunca acepta un id de asesor por
  query o body, así que no se puede leer las ventas de otro cambiando parámetros.
- **Límites:** cualquiera que conozca el header puede suplantar a un asesor; es una
  simulación consciente para no construir autenticación dentro del tiempo. En producción
  se sustituiría por un token firmado (JWT) sin tocar la lógica de negocio, porque el
  asesor ya está aislado en una sola abstracción (`IAdvisorContext`).
- **Cambiar de asesor:** basta cambiar el valor del header (A↔B) para comprobar ambos
  contextos.

Formato de referencia (`RequestReference`): 6–40 caracteres `[A-Za-z0-9_-]`. Su unicidad
es **por asesor**: A y B pueden usar el mismo texto para ventas distintas (índice único
compuesto `(AdvisorId, RequestReference)`).

---

## 6. Endpoints

Respuestas de error con formato uniforme: `{ "error": { "code": "...", "message": "..." } }`.

### R1 — Consultar planes
```
GET /api/plans
```
No requiere asesor. `200`:
```json
[
  { "code": "ACC_ESENCIAL", "name": "Accidente Esencial", "premium": 50000.00, "availableQuota": 1 },
  { "code": "ACC_PLUS", "name": "Accidente Plus", "premium": 90000.00, "availableQuota": 5 }
]
```

### R2 / R3 — Registrar venta
```
POST /api/sales
X-Advisor-Id: A
Content-Type: application/json
```
```json
{
  "planCode": "ACC_ESENCIAL",
  "buyerName": "Juan Perez",
  "buyerEmail": "juan@ejemplo.test",
  "requestReference": "REF-A-0001"
}
```
`201`:
```json
{
  "id": "b8c83fc4-07ed-4e55-bfcb-ad048dbac362",
  "requestReference": "REF-A-0001",
  "advisorId": "A",
  "planCode": "ACC_ESENCIAL",
  "planName": "Accidente Esencial",
  "buyerName": "Juan Perez",
  "buyerEmail": "juan@ejemplo.test",
  "premium": 50000.00,
  "acceptedAt": "2026-10-08T21:28:05.95+00:00"
}
```
La prima se resuelve en el backend desde el plan; se ignora cualquier valor del cliente.

### R4 — Consultar ventas propias
```
GET /api/sales?page=1&pageSize=20
X-Advisor-Id: A
```
`200` (solo campos necesarios, más recientes primero, máximo 20 por página):
```json
{
  "items": [
    { "id": "...", "requestReference": "REF-A-0001", "planName": "Accidente Esencial", "premium": 50000.00, "buyerName": "Juan Perez", "acceptedAt": "..." }
  ],
  "page": 1, "pageSize": 20, "totalCount": 1, "totalPages": 1
}
```

### Códigos de error
| Caso | HTTP | code |
|---|---|---|
| Sin / inválido `X-Advisor-Id` | 401 | `UNAUTHORIZED_ADVISOR` |
| Nombre, email o referencia inválidos | 400 | `INVALID_SALE_DATA` |
| Plan inexistente | 404 | `PLAN_NOT_FOUND` |
| Sin cupos | 409 | `NO_QUOTA_AVAILABLE` |
| Referencia reusada con otros datos | 409 | `REFERENCE_CONFLICT` |
| Concurrencia no resuelta tras reintentos | 409 | `CONCURRENCY_CONFLICT` |

Hay un archivo `Prueba_Tecnica/Prueba_Tecnica.http` con todos los casos listos para ejecutar.

---

## 7. Cómo se cumplen las reglas R3 (consistencia)

| Regla | Mecanismo |
|---|---|
| Dos asesores por el último cupo → una sola venta, cupos nunca negativos | `rowversion` en `Plans` (concurrencia optimista) + reintentos; el dominio (`Plan.ConsumeOneQuota`) impide cupos negativos |
| Reenvío idéntico (misma ref + mismos datos) → misma venta, sin consumir cupo | Búsqueda por `(AdvisorId, RequestReference)` antes de registrar; si existe y los datos coinciden, se devuelve la venta previa |
| Misma ref con datos distintos → rechazo, se conserva la original | `Sale.MatchesBusinessData`; si difiere → `REFERENCE_CONFLICT` |
| Dos envíos idénticos simultáneos → una sola venta | Lock en memoria por clave `(asesor+ref)` (`ISaleRegistrationLock`) + índice único en BD como última defensa |
| Rechazo (sin cupos / datos inválidos) → no consume cupos ni deja venta parcial | Validación y verificación de cupo antes de persistir; todo en una `SaveChanges` transaccional |

---

## 8. Comprobaciones R1–R5 (resultados reales obtenidos)

Ejecutadas en local contra LocalDB con el backend corriendo.

| Req | Comprobación | Resultado |
|---|---|---|
| R1 | `GET /api/plans` | `200` con Esencial (prima 50000, cupo 1) y Plus (prima 90000, cupo 5) ✅ |
| R2 | `POST /api/sales` (asesor A, Esencial) | `201` con `id`, prima 50000 resuelta en backend, `acceptedAt` ✅ |
| R3 (repetición) | Reenvío idéntico de `REF-A-0001` | Devuelve el **mismo** `id` y prima; cupo Esencial sigue en 0 (no consume otro) ✅ |
| R3 (conflicto) | Misma ref con otro plan/comprador | `409 REFERENCE_CONFLICT`; se conserva la venta original ✅ |
| R3 (sin cupo) | Segundo intento sobre Esencial (cupo 0) | `409 NO_QUOTA_AVAILABLE`; cupo no baja de 0, sin venta parcial ✅ (test automatizado) |
| R4 | `GET /api/sales` como A y como B | Cada asesor ve solo sus ventas; A y B reutilizan `REF-SHARED` sin colisión ✅ (test automatizado) |
| R5 | Reenvío con misma referencia tras "perder" la respuesta | Devuelve la venta previa (idempotencia); el bloqueo de botón no sustituye la regla de backend ✅ |
| Carga | 10.000 ventas de historial sembradas (`HIST-A-*`, `HIST-B-*`, `IsHistorical=true`) | Sembradas al arrancar sin descontar cupos de campaña ✅ |

Comprobación de carga de consultas (10 listados concurrentes): el listado paginado usa el
índice `(AdvisorId, AcceptedAt)` y proyecta solo los campos de la vista, por lo que la
respuesta por página es pequeña (≤ 20 ítems). Durante esa actividad, una nueva venta sigue
siendo correcta porque el registro no compite con las lecturas (tablas distintas de la de
cupos y transacción propia).

---

## 9. Pruebas automatizadas

### Backend (`Application.Tests`, xUnit + EF Core InMemory + FluentAssertions)

```powershell
dotnet test
```

Cubre explícitamente lo exigido por el enunciado:

- **Dos solicitudes por el último cupo** → una se acepta, la otra `NO_QUOTA_AVAILABLE`, cupo nunca negativo.
- **Repetición aceptada** → mismo `id`, misma prima, sin consumir cupo.
- **Referencia reutilizada con datos diferentes** → `REFERENCE_CONFLICT`, se conserva la original.
- **Rechazo sin modificación de cupos** (sin cupo / email inválido).
- **Aislamiento por asesor** (A y B con la misma referencia).
- **Prima resuelta desde el plan** (se ignora el valor del cliente).
- **Tope de página en 20**.

Resultado: **8/8 pruebas en verde**.

**Priorización dentro del tiempo:** prioricé las reglas R3/R4 (el núcleo del reto y lo que
más fácil se rompe) sobre pruebas de serialización HTTP o de controllers, porque la lógica
crítica vive en `SaleService` y es la de mayor riesgo. Pendiente explícito: una prueba de
integración end-to-end sobre SQL Server real que ejercite el `rowversion` con dos hilos
simultáneos (ver §10).

### Frontend (React)

Prueba del flujo de envío/reintento que incluye el uso de la referencia: enviar una venta,
simular pérdida de respuesta y reenviar con la **misma** referencia, verificando que la UI
no genera una venta nueva (idempotencia) y que una compra nueva usa una referencia nueva.

---

## 10. Decisiones, alternativas y límites

**Almacenamiento: SQL Server LocalDB (EF Core).**
- *Alternativas:* memoria pura, SQLite.
- *Por qué:* el enunciado permite memoria para una instancia, pero LocalDB da persistencia
  real, índices únicos y `rowversion` nativo, que es exactamente lo que necesitan las
  garantías de concurrencia R3 sin inventar un mecanismo propio.
- *Costo / límite:* requiere LocalDB instalado. Al reiniciar la BD **persiste** (los datos
  siguen ahí); el seeder no duplica porque detecta datos existentes.
- *Qué cambiaría con varias instancias:* el lock en memoria (`ISaleRegistrationLock`) deja
  de ser global; habría que usar un lock distribuido o apoyarse únicamente en el índice
  único + `rowversion`, que ya garantizan una sola venta aun sin el lock.

**Concurrencia de cupos: `rowversion` optimista + reintentos.**
- *Alternativa:* bloqueo pesimista (`SELECT ... FOR UPDATE` / `UPDLOCK`).
- *Por qué optimista:* mejor rendimiento bajo baja contención (lo normal con 2 asesores) y
  evita bloqueos largos; ante conflicto se reintenta con estado fresco.
- *Riesgo:* bajo contención extrema podría agotar reintentos → `409 CONCURRENCY_CONFLICT`
  (el cliente reintenta). Se mitiga con el lock por clave para los reenvíos idénticos.

**Idempotencia: clave `(AdvisorId, RequestReference)` + índice único.**
- *Por qué:* es la clave natural del "intento de compra" y permite recuperar el resultado
  reenviando la misma referencia. El índice único es la última línea de defensa si dos
  escrituras esquivan el lock en memoria.

**Identidad simulada por header.** Ver §5. Reversible a JWT sin tocar el dominio.

**Relación frontend ↔ backend ↔ almacenamiento:** el frontend solo habla con `/api` (proxy
de Vite); el backend resuelve prima y disponibilidad desde el plan (nunca confía en el
cliente) y persiste en LocalDB; el aislamiento por asesor se aplica en la consulta a BD, no
en el cliente.

---

## 11. Investigación de un problema real encontrado

**Síntoma:** el frontend mostraba `502 Bad Gateway` en `http://localhost:5173/api/plans` y
`/api/sales`.

**Investigación:** probé los dos puertos del backend con `Invoke-WebRequest`
(`http://localhost:5162` y `https://localhost:7036`); ambos daban timeout, es decir, el
backend no estaba escuchando. El 502 de Vite es "el proxy no pudo alcanzar el destino", no
un error de código.

**Causa raíz:** el backend estaba detenido mientras el frontend seguía arriba.

**Verificación de la solución:** arranqué el backend y repetí la petición: `GET /api/plans`
devolvió `200` con los dos planes. Confirmado que, con el backend arriba, el proxy deja de
dar 502.

**Hipótesis adicional comprobada (regla R3, idempotencia):** hipótesis = "reenviar la misma
referencia con los mismos datos no debe consumir un segundo cupo". Experimento = registrar
una venta sobre el plan Esencial (cupo 1), consultar planes (cupo 0), reenviar la misma
solicitud y volver a consultar. Resultado observado: el reenvío devolvió el **mismo `id`** y
el cupo **siguió en 0** (no bajó a negativo ni consumió otro cupo). Hipótesis confirmada.

---

## 12. Registro de uso de IA

- **Herramienta:** asistente de IA integrado en el IDE (Kiro).
- **Tareas asistidas:** scaffolding de los 5 proyectos, redacción del código de las capas
  (Domain/Application/Infrastructure/API), configuración de EF Core y migración, generación
  de los scripts SQL, configuración de Swagger, redacción de las pruebas y de este README.
- **Archivos afectados:** todo el backend bajo `Domain/`, `Application/`, `Infrastructure/`,
  `Prueba_Tecnica/`, `Application.Tests/`, `scripts/` y este `README.md`.
- **Resumen de instrucciones:** construir el backend con Clean Architecture y EF Core,
  cumplir R1–R5, exponer endpoints documentados, scripts de BD y Swagger.
- **Validación:** todo el código fue compilado (`dotnet build`, 0 errores), las pruebas
  ejecutadas (`dotnet test`, 8/8) y los endpoints verificados en vivo contra LocalDB.
- **Resultados corregidos/descartados:** se corrigieron espacios de nombres de Swagger por
  el cambio a Microsoft.OpenApi 2.x; se reemplazó un ajuste de fecha por reflexión en el
  seeder por un método de dominio limpio (`Sale.BackdateForHistory`).
- **Protección de datos:** no se usaron datos reales. Compradores, emails y referencias son
  ficticios (`*@ejemplo.test`, `HIST-A-*`). No hay secretos ni credenciales en el repo
  (la conexión es LocalDB con autenticación de Windows, sin usuario/contraseña).

---

## 13. Tiempo empleado y pendientes

- **Tiempo:** dentro del límite de 120 minutos (preparación, backend, scripts, Swagger,
  pruebas y documentación).

- **Pendientes explícitos (no implementados):**
  - Prueba de integración sobre SQL Server real con dos hilos concurrentes golpeando el
    último cupo (las pruebas actuales usan InMemory, que no soporta `rowversion`; la
    concurrencia real queda cubierta por diseño, no por test automatizado end-to-end).
  - Script SQL para las 10.000 ventas de historial (hoy solo las carga el seeder).
  - Pruebas de la capa de controllers / serialización HTTP.

> Lo marcado como pendiente **no** está presentado como implementado.
