# Project: Takwene Track Management
Full-stack take-home: .NET Web API (Clean Architecture) + Angular SPA.

## Backend conventions
- .NET 8 (LTS), solution in /backend, projects: Shared, Domain, Application, Infrastructure, Api
- Layering (ABP-style DDD inside Clean Architecture). Project references:
  - Shared → nothing (no project references, no NuGet packages).
  - Domain → Shared only (only NuGet package allowed is Microsoft.Extensions.DependencyInjection.Abstractions).
  - Application → Domain, Shared.
  - Infrastructure → Domain, Shared (never Application).
  - Api → Application, Infrastructure, Shared (composition root).
  - Every project that uses Shared references it directly (don't rely on transitive references).
- Layer responsibilities:
  - Shared: only constants and enums, as static classes of `const` fields. ValidationConstants (every length, pattern and limit for catalog data, nested per concept: Track, Artist, Dsp, Status, Isrc, Email, CountryCode), AuthValidationConstants (login and JWT limits), Enums/ (TrackStatus, DistributionStatus). Never hard-code a limit or pattern elsewhere: domain guards, FluentValidation rules and EF column sizes all read these. Changing a length changes the schema, so add a migration.
  - Domain: aggregates/entities, value objects, repository interfaces (next to their aggregate), IUnitOfWork, domain managers, DomainException hierarchy.
  - Application: application services, DTOs, FluentValidation validators.
  - Infrastructure: AppDbContext, EF configurations, repository + UnitOfWork implementations, migrations.
  - Api: controllers, DI composition, exception → ProblemDetails mapping, auth (JWT later).
- DI: each project registers its own types via its own extension method (AddDomain, AddApplication, AddInfrastructure). Never register another project's types. Api's Program.cs only composes: AddDomain().AddApplication().AddInfrastructure(config).
- EF Core + SQL Server. Entity configs via IEntityTypeConfiguration, never data annotations on entities.
- Enums stored as strings in the DB.
- All dates UTC (DateTime with Kind Utc / DateTimeOffset); release_date is DateOnly.
- Controllers are thin: no business logic, no DbContext access. Use Application services.
- Never expose entities from the API. Use request/response DTOs.
- Controller attributes: no [ProducesResponseType] for errors (all errors share the ProblemDetails shape documented below). Only add [ProducesResponseType(StatusCodes.Status201Created)] (or another non-200 success code) where Swagger can't infer it from ActionResult<T>.
- Validation: FluentValidation. Errors returned as RFC 7807 ProblemDetails.
- Async all the way, pass CancellationToken.
- Reads: repositories use AsNoTracking for read paths. Load related aggregates in one batched call (GetByIdsAsync) and map in memory; never query per item (no N+1). Skip the batch call when the id list is empty.
- Writes: load the aggregate tracked (e.g. TrackManager.GetForUpdateAsync), call domain/manager methods, then SaveChangesAsync once per request.
- Time: inject TimeProvider in Application and pass UTC timestamps into the domain. Never call DateTime.UtcNow / DateTimeOffset.UtcNow in Domain.
- Constructor injection uses classic constructors assigning `private readonly` fields (`_camelCase`). No C# 12 primary constructors on classes. Positional records are fine for DTOs.
- No secrets in committed appsettings.json; use appsettings.Development.json (gitignored) or user-secrets.

## Frontend conventions
- Angular (latest), standalone components, signals where natural, no NgModules.
- API base URL from environment files. All HTTP in services, never in components.
- Plain CSS, functional over pretty.

## Workflow
- Do NOT run git commit. I review and commit myself.
- After each task, list files changed and anything I should double-check.

## Domain modeling (DDD tactical patterns inside Clean Architecture)
- Aggregates: Track (root, owns TrackDistribution), Artist, Dsp. Cross-aggregate references by ID only.
- Entities have private setters and a private parameterless ctor for EF. Create via static factory methods.
- All invariants live in the domain. Application handlers never set properties directly; they call domain methods.
- Value objects: Isrc, Email, CountryCode. Immutable records that validate on creation and throw a DomainException subtype. Mapped in EF with value converters.
  - Isrc canonical form CC-XXX-YY-NNNNN (uppercase, stored as char(15)); input accepted with or without hyphens.
- Repositories exist only for aggregate roots (none for TrackDistribution). Interfaces live in Domain.
- TrackDistribution is created only through Track (internal ctor, private list exposed as IReadOnlyCollection).
- Domain managers (ArtistManager, TrackManager) hold the rules that need repositories (artist exists, ISRC unique, email unique) and are the application layer's only way into their aggregate. Single-aggregate rules stay on the entity. A manager's CreateAsync builds the aggregate and registers it via the repository's AddAsync. Managers never call SaveChangesAsync; application services commit once via IUnitOfWork.
- An application service never injects both a manager and the repository of the same aggregate. If an aggregate has a manager, services use only the manager, for writes and reads alike (GetAsync / GetForUpdateAsync throw NotFoundException; GetListAsync, GetByIdsAsync). Repositories are injected directly only for aggregates without a manager (Dsp).
  - Factories/methods guarded by a manager are `internal` (Artist.Create, Artist.ChangeEmail, Track.Create) so callers must go through the manager.
  - Dsp name uniqueness: unique index + IDspRepository.NameExistsAsync check in the application service (no DspManager).
- Exceptions (Domain/Common/Exceptions): only three — DomainException (any rule/value violation), NotFoundException, ConflictException (both derive from DomainException). Don't add a type per situation; put specifics in the message. Use ConflictException (409) when the resource's current state forbids the operation (invalid status transition, already distributed, DSP already present, distribution not Pending); plain DomainException (422) when the input itself is invalid. Don't use BCL exceptions (ArgumentException etc.) for expected failures: those must stay 500s.
- HTTP mapping (Api/ExceptionHandling/GlobalExceptionHandler, kept minimal): FluentValidation ValidationException → 400 ValidationProblemDetails (camelCase keys); NotFoundException → 404; ConflictException and unique-index DbUpdateException → 409; any other DomainException → 422. Everything else is left to the built-in exception handler (500 ProblemDetails, no details). The built-in middleware logs handled exceptions at Error too; accepted noise on .NET 8.
- FluentValidation checks request shape only; value objects/entities still enforce the rules. MVC's implicit [Required] for non-nullable refs is disabled so FluentValidation owns all 400s for DTO fields.
- Domain project has no reference to EF Core or any NuGet package except Microsoft.Extensions.DependencyInjection.Abstractions (for AddDomain); its only project reference is Shared.