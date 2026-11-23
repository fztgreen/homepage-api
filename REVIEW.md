# Code Review Checklist

> Names marked † are illustrative: they show how something would be named or used, but they don't exist in this repository. Examples about cars (`Car`, `GetCar`, `CarsController`, `Engine`, …) come from the DotNetExample template these conventions are taken from and are all illustrative too; this repository's real feature is Feedback. Code blocks are examples unless they name the file they come from.

Use this checklist when reviewing changes. Each item traces back to a rule in [CONTRIBUTING.md](CONTRIBUTING.md) or [TESTING.md](TESTING.md).

## Enforced Automatically

These rules fail `dotnet build` or `dotnet test`, so a green build covers them. That green build is the required CI check (see [Continuous Integration](README.md#continuous-integration)). Don't spend review time re-checking them. Instead, make sure nobody weakened the rule that enforces them (e.g. by lowering a severity in `.editorconfig`, adding to a `NoWarn`, or adding an exemption to an architecture test).

| Rule | Enforced by |
|---|---|
| `var`, braces, file-scoped namespaces, usings placement, no unused usings, readonly fields, `_camelCase` private fields, `I`-prefixed interfaces | `.editorconfig`, enforced in build (`EnforceCodeStyleInBuild`) |
| Namespace matches folder path | `.editorconfig` IDE0130, enforced in build |
| `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors` | `Directory.Build.props` |
| Domain references only the base class library and has no packages; Api and DataAccess don't reference each other or IoC, in the `.csproj` or in compiled code | `Architecture/LayersShould` |
| Only `DataAccess/Wrappers` touches infrastructure (`System.Net`, `System.Data`, `System.IO`, Mongo, Dapper, …) | `Architecture/InfrastructureShould` |
| No Newtonsoft.Json or EF Core, as a package or in code | `Architecture/CodeShould.NotReferenceBannedPackages`, `NotUseBannedLibraries` |
| No anonymous types; lambdas are a single one-line expression; no `delegate { … }` blocks | `Architecture/CodeShould` |
| Async methods end with `Async` (except controller actions and test methods) | `Architecture/CodeShould.NameAsyncMethodsWithAsyncSuffix` |
| No types directly in a feature folder root (e.g. `Domain/Commands/`) | `Architecture/FeatureFoldersShould` |
| The API receives and returns only API models, never Domain types | `Architecture/ApiShould` |
| Every wrapper implementation has an integration test | `IntegrationTests/Architecture/WrappersShould` |
| Handlers implement their own interface with one method; Commands return `Task`, Queries return `Task<T>`; no generic method names or repeated nouns | `Architecture/HandlersShould` |

## Architecture

- [ ] No `Program.cs` or DI wiring outside the IoC project.
- [ ] New handlers, repositories, and wrappers are registered in `RegisterDependencies.cs`.
- [ ] SQL goes through Dapper inside the SQL wrapper, with the query text built in the repository.
- [ ] Wrapper interfaces are named after the kind of store (`ISqlDatabaseWrapper`, `IDocumentDatabaseWrapper`†), not a product or hosting choice; technology names appear only on implementations.
- [ ] New code is in the right feature folder (e.g. `Domain/Commands/Feedback/`, not `Domain/Commands/Contact/`).

## Design & Conventions

- [ ] Writes are Commands and reads are Queries.
- [ ] Handler methods read as a sentence when called (`_storeFeedback.WithAsync(submission)`, `_getCar.ByAsync(id)`).
- [ ] Functions are small and names are descriptive; code reads without explanation.
- [ ] No string-keyed dictionaries standing in for an anonymous type (`new RouteValueDictionary { ["id"] = … }`); use named records.
- [ ] Comments only explain *why*, or document public-facing APIs; no commented-out code.
- [ ] The change solves the current requirement only, with no speculative code.

## Tests

- [ ] Every behavior change has a test that was written first and seen to fail.
- [ ] Tests that passed on their first run are called out, along with how each was shown to fail by temporarily breaking the code (see [TESTING.md](TESTING.md#tests-that-pass-on-the-first-run)).
- [ ] Applicable status codes are covered: 200/201/204, 400, 404, 500.
- [ ] Only infrastructure wrappers are mocked. Repositories, entities, and value objects are real.
- [ ] Mocks match the exact URI/query/procedure the code should send; no logic lives in wrappers.
- [ ] Assertions check return values or state; `Received()` is used only where nothing else is observable.
- [ ] Test data is randomized with `RandomTestValues`, not hardcoded.
- [ ] Tests have `// Arrange`, `// Act`, `// Assert` comments and no other explanatory comments.
- [ ] Test files follow `<Namespace>/<ClassName>Tests/<MethodName>Should.cs`, one file per method under test, with shared setup in a `<ClassName>TestBase` rather than duplicated.
- [ ] Refactor-only changes leave existing Assert statements untouched.
- [ ] A new convention that can be checked mechanically comes with an architecture test (see [TESTING.md](TESTING.md#architecture-tests)).
- [ ] A wrapper behavior that unit-test mocks rely on is covered by the wrapper's contract, and a new wrapper implementation passes the existing contract unchanged (see [TESTING.md](TESTING.md#integration-tests)).

## Build

- [ ] `dotnet build HomepageApi.slnx` passes with no warnings.
- [ ] `dotnet test HomepageApi.slnx` passes.
