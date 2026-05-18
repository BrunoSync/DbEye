# Changelog

## [1.3.0] - 2026-05-18

### Added
- Configurable N+1 detection threshold globally and per endpoint
- `AllowedEnvironments` validation — app throws on startup if current environment is not allowed

### Changed
- Extracted delay testing logic to its own middleware

### Removed
- Unused database providers from Docker Compose

## [1.2.0] - 2026-05-15

### Added
- Multi-database support (PostgreSQL, SQL Server, SQLite)
- Endpoint exclusion via `ExcludeEndpoints`, `ExcludeScalar` and `ExcludeSwagger`
- Unit tests for middleware and collector

## [1.1.0] - 2026-05-12

### Added
- Success message when no issues are detected
- Per-endpoint millisecond threshold configuration
- Middleware now only runs in development environment (IsDevelopment)

### Changed
- Messages translated to English

---

## [1.0.0] - 2026-05-11

### Added
- N+1 query detection
- Slow query detection
- Global millisecond threshold configuration (default: 500ms)
