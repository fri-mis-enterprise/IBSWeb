# AGENTS.md

## Purpose

This file defines mandatory working instructions for coding agents operating in this repository.

This repo is a layered ASP.NET Core MVC business and accounting system. It uses Razor Views, area-based controllers, EF Core with PostgreSQL, session-aware middleware patterns, Serilog, SignalR, and cloud storage integrations.

The agent should make the smallest clean change that matches the existing codebase and keeps the app easy to maintain.

## Mandatory Agent Workflow

For every implementation task, follow this workflow:

1. Read this `AGENTS.md` before making changes.
2. Read the applicable `.editorconfig` rules for the files that may be changed.
3. Understand the requested behavior before editing code.
4. Inspect the existing implementation, related code paths, and surrounding conventions.
5. Identify the smallest set of files that needs to change.
6. Follow existing repository patterns unless the task explicitly requires changing them.
7. Make the smallest clean change that fully satisfies the requirement.
8. Do not perform unrelated refactoring, formatting, renaming, cleanup, or architectural changes.
9. Run the relevant build, tests, or targeted verification when possible.
10. Review the complete diff before finishing.
11. Check all changed code against this `AGENTS.md` and the applicable `.editorconfig`.
12. Remove unintended changes and fix instruction violations before reporting completion.

Do not skip the final self-review merely because the implementation builds successfully.

If an instruction cannot be followed, explicitly report which instruction could not be followed and why.

## Working Defaults

* Prefer simple, explicit code over flexible abstractions.
* Prefer small focused edits over broad rewrites.
* Follow existing repository patterns unless they are clearly harmful.
* Keep controllers thin and put business logic in services.
* Keep data access in EF Core and repository classes already used by the project.
* Avoid introducing new packages unless the platform or current dependencies cannot solve the problem well.
* Do not add comments that restate obvious code.
* Do not change unrelated code while implementing a task.
* Do not proactively clean up surrounding code unless the cleanup is required for the requested change.

## Repository Shape

Use the current solution structure as the default:

* `IBSWeb/` for the ASP.NET Core web app, `Program.cs`, SignalR hubs, Razor views, and area-based MVC controllers
* `IBSWeb/Areas/` for module-specific MVC entry points and views such as `Admin`, `User`, `Bienes`, `Filpride`, and `Identity`
* `IBS.Services/` for business logic, middleware, scheduling, and external integrations
* `IBS.DataAccess/` for `ApplicationDbContext`, EF Core configuration, repositories, and migrations
* `IBS.Models/` for domain models and entity types
* `IBS.DTOs/` for DTOs and request/response transport models
* `IBS.Utility/` for shared helpers and cross-cutting utilities

Place changes in the project that already owns the behavior instead of collapsing code back into the web project.

## Architectural Rules

* Keep business rules out of Razor views and controllers.
* Respect existing project boundaries between web, services, data access, models, DTOs, and utility code.
* Put validation close to the request boundary or service boundary where it is easiest to find.
* Use interfaces where the repo already benefits from them, especially for service seams and external integrations.
* Do not create interfaces for every class by default.
* Do not add pass-through service layers with no behavior.
* Do not add generic repository wrappers over EF Core.
* Prefer concrete, readable flows over clever generic helpers.
* Reuse established architectural patterns before introducing new ones.
* Do not introduce a new architectural pattern solely because it is theoretically cleaner.

## Design Principles: SOLID, KISS, and DRY

Apply these principles to new and changed code within the requested scope. Use them to improve correctness and maintainability without expanding the task into unrelated refactoring.

### SOLID

* **Single Responsibility:** Give each class or module one cohesive responsibility and one main reason to change. Separate request handling, business rules, persistence, and external integrations using the existing project boundaries.
* **Open/Closed:** Use an existing extension point when adding a supported variation. Introduce a new one only when a concrete requirement justifies it; a small direct edit is appropriate when no reusable variation exists.
* **Liskov Substitution:** Implementations must honor their interface or base-class contracts, including accepted inputs, results, exceptions, and side effects. Do not require caller-specific type checks or leave required operations unsupported; prefer composition when inheritance cannot preserve the contract.
* **Interface Segregation:** Keep new or changed interfaces focused on the operations their consumers need. Avoid forcing consumers to depend on unrelated methods while preserving existing contracts outside the task's scope.
* **Dependency Inversion:** Keep business policy independent of concrete storage and external integration details through appropriate service or repository contracts. Use the existing dependency injection setup to supply implementations; do not resolve dependencies through a service locator or add interfaces to simple helpers without a concrete need.

### KISS (Keep It Simple)

* Choose the simplest design that satisfies the current requirement and preserves accounting, security, and operational behavior.
* Favor readable control flow over fewer lines or clever indirection.
* Add a helper, abstraction, configuration option, or design pattern only when it reduces current complexity or supports a required variation.
* Do not build for hypothetical future requirements.
* Do not turn a small change into a framework or generalized system without a current requirement.

### DRY (Don't Repeat Yourself)

* Keep each business rule, calculation, and policy in one authoritative implementation within its owning layer.
* Look for existing logic before adding another implementation, especially across create/edit, reports, and exports.
* Extract shared behavior when callers represent the same rule and should change together.
* Similar-looking code alone is insufficient reason to combine distinct company rules or workflows; keep them separate when they can evolve independently.
* Prefer a small, clearly named method or existing service over a generic framework.
* Preserve necessary validation at trust boundaries.
* Do not edit generated code or historical migrations merely to remove repetition.

## Coding Style

* Read the applicable `.editorconfig` rules before editing a file.
* `.editorconfig` is the source of truth for formatting, naming, line endings, and C# style; it takes precedence over style guidance in this file or surrounding code.
* Follow the effective rules for the file, including file-pattern sections and any nested `.editorconfig` overrides.
* Do not duplicate `.editorconfig` settings here so future configuration changes remain authoritative.
* Apply configured style preferences to new and changed code, including preferences with suggestion or silent severity. Severity controls diagnostics, not whether the preference applies.
* Use existing conventions and the simplicity guidelines below where `.editorconfig` does not specify a preference.
* Use clear names and shallow control flow.
* Prefer guard clauses and early returns.
* Keep methods focused.
* Preserve nullable correctness.
* Use async end-to-end for I/O-bound work.
* Do not block async code with `.Result`, `.Wait()`, or similar patterns.
* Follow `.editorconfig` for namespace declarations, braces, constructors, expression bodies, imports, and other syntax choices.
* Keep one public type per file unless a small local grouping is clearly simpler.
* Do not reformat untouched code merely to make it match personal or agent preferences.

## MVC and HTTP Guidance

* This repo is controller-and-view based. Default to extending existing controllers and Razor views.
* Prefer the existing `Areas` structure and keep new endpoints, views, and related code in the appropriate module.
* Keep controller actions focused on request handling, authorization decisions, model binding, and choosing the response.
* Put non-trivial query logic, storage workflows, and access rules into services or existing repository classes.
* Preserve existing route patterns unless the task requires a route change.
* Return consistent HTTP results and avoid inventing custom response wrappers unless the repo already expects them.

## Data and Persistence

* Prefer EF Core patterns already present in the repo.
* Reuse the existing repository and unit-of-work patterns already present in `IBS.DataAccess` where the surrounding code depends on them.
* Keep queries explicit and easy to reason about.
* Look for existing queries, calculations, and persistence workflows before introducing another implementation.
* Add migrations only when schema changes are required for the task.
* Keep each migration focused on one logical schema change.
* Make destructive schema changes explicit.
* Respect PostgreSQL behavior already configured by the app.
* Do not modify historical migrations unless the task specifically requires correcting one and the implications are understood.

## Security and Operations

* Validate and sanitize external input.
* Do not log secrets, tokens, credentials, or connection strings.
* Preserve authentication, authorization, maintenance-mode behavior, scheduled job endpoints, and SignalR flows unless the task requires changing them.
* Be careful with file handling and cloud storage paths. Prefer existing workflow services over duplicating storage logic.
* Preserve audit, accounting, and approval-related flows unless the task explicitly changes them.
* Treat changes affecting financial calculations, posting, approvals, audit trails, or authorization as high-impact changes and inspect the surrounding workflow before modifying them.

## Dependencies

* Prefer the .NET base class library and existing Microsoft packages first.
* Reuse existing packages already in the repo when reasonable.
* Add a third-party package only with clear justification.
* Do not add a dependency when the existing platform or dependencies already solve the requirement adequately.
* Remove unused code or dependencies only when directly relevant to the task.

## Tests and Verification

There is currently no separate test project in this repository. When changing behavior:

* Add automated tests if a test project exists or if the task includes creating one.
* If no test project exists, do not fabricate a large test harness just to satisfy process.
* At minimum, run the relevant build or targeted verification commands when possible.
* Prefer targeted verification in addition to a full build when the changed behavior can be checked directly.
* Do not claim that behavior was verified if only compilation was checked.

Default validation commands:

* `dotnet build "Integrated Business System.sln"`
* `dotnet test` only if a test project exists

If a schema change is made, also verify the relevant EF Core migration artifacts.

If validation cannot be run, state that clearly in the final response rather than implying that validation passed.

## Change Discipline

When making changes, follow this order:

1. Understand the requirement.
2. Inspect the existing code path and conventions.
3. Determine which layer currently owns the behavior.
4. Search for existing implementations of the same business rule or pattern.
5. Identify the minimum files and code that need to change.
6. Make the smallest clean change that solves the problem.
7. Update tests when practical and appropriate.
8. Run relevant validation.
9. Review the complete diff.
10. Check changed code against this `AGENTS.md` and the applicable `.editorconfig`.
11. Remove unrelated changes and correct instruction violations.
12. Report validation results and any remaining limitations.

Before finishing, check changed code against the applicable `.editorconfig` rules, including encoding, line endings, indentation, and final newlines.

Keep formatting changes limited to the files and code involved in the task. Do not reformat the repository solely to fix pre-existing style differences.

A successful build does not replace the required diff and instruction review.

## Scope Control

Stay within the scope of the user's request.

Do not perform any of the following unless required to complete the task:

* Refactor unrelated code.
* Rename unrelated classes, methods, variables, files, or folders.
* Reformat unrelated code.
* Reorganize folders.
* Replace an existing architectural pattern.
* Introduce speculative abstractions.
* Change public contracts unnecessarily.
* Change database schema for convenience.
* Fix unrelated warnings or diagnostics.
* Modify unrelated configuration.
* Upgrade packages or framework versions.
* Perform general cleanup simply because an opportunity was noticed.

If an unrelated issue is discovered, report it separately instead of fixing it unless it blocks the requested work.

## Avoid By Default

Do not introduce these unless clearly justified by the task:

* Generic repository abstractions over EF Core
* Marker interfaces
* Deep inheritance hierarchies
* Static global mutable state
* Reflection-heavy or dynamic designs
* Placeholder abstractions for future requirements
* Broad folder reorganizations
* Cosmetic-only churn
* New architectural patterns when an existing repository pattern is sufficient
* Large refactors to implement small behavioral changes

## Final Self-Review

Before reporting a task as complete, review the complete diff and confirm:

* [ ] The requested behavior is fully implemented.
* [ ] The existing implementation was inspected before making changes.
* [ ] Existing repository patterns were followed where appropriate.
* [ ] The change is limited to the necessary scope.
* [ ] No unrelated files or code were changed.
* [ ] No unnecessary abstraction or dependency was introduced.
* [ ] Business logic remains in the appropriate layer.
* [ ] Existing accounting, approval, audit, security, and operational behavior was preserved unless intentionally changed.
* [ ] Applicable `.editorconfig` rules were followed.
* [ ] Encoding, line endings, indentation, and final newlines were checked.
* [ ] Relevant build, tests, or targeted verification were run when possible.
* [ ] Validation results are accurately reported.
* [ ] The complete diff was reviewed for unintended changes.

If any item cannot be satisfied, explicitly report it and explain why.

## Final Rule

Prefer the solution that is easiest for the next maintainer to read, understand, test, and change while staying aligned with this repository's current MVC, EF Core, and layered solution structure.

Correctness comes first, but correctness alone is not sufficient. The final change should also be scoped, maintainable, consistent with the repository, and free from unintended changes.
