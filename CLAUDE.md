# Project: Takwene Track Management
Full-stack take-home: .NET Web API (Clean Architecture) + Angular SPA.

## Backend conventions
- .NET 8 (LTS), solution in /backend, projects: Domain, Application, Infrastructure, Api
- Layering (ABP-style DDD inside Clean Architecture). Project references:
  - Domain → nothing (no project or NuGet references).
  - Application → Domain.
  - Infrastructure → Domain only (never Application).
  - Api → Application, Infrastructure (composition root).
- Layer responsibilities:
  - Domain: aggregates/entities, value objects, repository interfaces (next to their aggregate), IUnitOfWork, domain managers, DomainException hierarchy.
  - Application: application services, DTOs, FluentValidation validators. Registers domain managers in AddApplication (Domain has no DI).
  - Infrastructure: AppDbContext, EF configurations, repository + UnitOfWork implementations, migrations.
  - Api: controllers, DI composition, exception → ProblemDetails mapping, auth (JWT later).
- EF Core + SQL Server. Entity configs via IEntityTypeConfiguration, never data annotations on entities.
- Enums stored as strings in the DB.
- All dates UTC (DateTime with Kind Utc / DateTimeOffset); release_date is DateOnly.
- Controllers are thin: no business logic, no DbContext access. Use Application services.
- Never expose entities from the API. Use request/response DTOs.
- Validation: FluentValidation. Errors returned as RFC 7807 ProblemDetails.
- Async all the way, pass CancellationToken.
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
- Domain managers (ArtistManager, TrackManager) hold only rules that need repositories (artist exists, ISRC unique, email unique). Single-aggregate rules stay on the entity. Managers don't save; application services commit via IUnitOfWork.
  - Factories/methods guarded by a manager are `internal` (Artist.Create, Artist.ChangeEmail, Track.Create) so callers must go through the manager.
  - Dsp name uniqueness: unique index + IDspRepository.NameExistsAsync check in the application service (no DspManager).
- Exceptions (Domain/Common/Exceptions): DomainException (abstract base), BusinessRuleViolationException, EntityNotFoundException, DuplicateEntityException.
- Domain project has no reference to EF Core or any NuGet package.