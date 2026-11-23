# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

This is the homepage API for fztgreen.dev, a .NET 10 solution following Clean Architecture + CQRS + centralized IoC, built with strict TDD. The project rules live in the docs imported below; they are binding, not suggestions.

## Commands

```bash
dotnet build HomepageApi.slnx              # warnings and .editorconfig style errors fail the build
dotnet test HomepageApi.slnx
dotnet test --filter "FullyQualifiedName~Architecture"   # architecture rules only
dotnet test --filter "FullyQualifiedName~FeedbackControllerTests.CreateShould.ReturnOk_AndStoreFeedback"   # single test
dotnet run --project HomepageApi.IoC       # the IoC project is the executable
```

## Working Rules for Claude

- **TDD is mandatory.** Write a failing test, run it and confirm it fails, then write the minimum code to pass, then refactor without touching asserts. Never write implementation code first. A test that passes immediately because the behavior already exists is a coverage test: prove it can fail by temporarily breaking the code (never the assertion), and report it (see [TESTING.md](TESTING.md#tests-that-pass-on-the-first-run)).
- **Small steps.** After every file change or test run, stop and evaluate the next step. Don't generate large amounts of code at once or anticipate future requirements.
- **Mock only wrappers.** When testing Commands/Queries, use the real repository with a mocked wrapper (e.g. `ISqlDatabaseWrapper`).
- **Feature folders.** Put new code in a per-feature folder inside its layer (e.g. `Domain/Commands/Feedback/`) with a matching namespace. Only controllers and shared wrappers stay flat.
- **One test file per method under test** (`CreateShould.cs`). Before adding one, read the existing `<ClassName>TestBase` to avoid redefining `[TestInitialize]`.
- Register every new handler/repository/wrapper in `HomepageApi.IoC/RegisterDependencies.cs`.
- **New wrappers get contract tests**, in `HomepageApi.IntegrationTests` against the real dependency, never mocked (see [TESTING.md](TESTING.md#integration-tests)).
- **Never weaken enforcement to get green.** If a style error or an architecture test fails, fix the code. Don't lower an `.editorconfig` severity, add a `NoWarn`, or add an exemption to `Tests/Architecture/` unless the user agrees.
- **Use Conventional Commits** for commit messages and PR titles (see [CONTRIBUTING.md](CONTRIBUTING.md#commit-messages)).

## Reference Implementation

The Feedback feature (`FeedbackController`) is the reference: copy its shape for new features. The cars examples in the docs come from the DotNetExample template and don't exist here.

- Tests are integration tests over `WebApplicationFactory<Program>` (see `Tests/Api/FeedbackControllerTests/FeedbackControllerTestBase.cs`), so `[ApiController]` validation (400) and unhandled exceptions (500) are exercised for real. Only `ISqlDatabaseWrapper` is replaced with a substitute, and mocks match the exact procedure name and parameters record.
- Storage goes through `ISqlDatabaseWrapper` (named for the kind of store). The repository owns the procedure name (`StoreRequest`) and builds a named parameters record (`StoreFeedbackParameters`). See [Wrapper Interfaces](CONTRIBUTING.md#wrapper-interfaces).
- The API speaks only its own models: `FeedbackRequest` in, mapped with `request.ToFeedbackSubmission()`. Domain types never cross the API boundary (see [API Models](CONTRIBUTING.md#api-models)).
- Don't name a type after its feature folder: the entity is `FeedbackSubmission`, because `Feedback` would resolve to the namespace.
- `DapperSqlDatabaseWrapper` is a stub that throws `NotImplementedException`; `IntegrationTests/Architecture/WrappersShould` exempts it, with the user's agreement, until a database is chosen. Remove the exemption when it gets a real implementation and a contract test.
- FluentAssertions is pinned to 7.x: 8.x requires a commercial license.

## Project Docs

@README.md
@CONTRIBUTING.md
@TESTING.md
@REVIEW.md
