# Testing

> Names marked † are illustrative: they show how something would be named or used, but they don't exist in this repository. Examples about cars (`Car`, `GetCar`, `CarsController`, `Engine`, …) come from the DotNetExample template these conventions are taken from and are all illustrative too; this repository's real feature is Feedback. Code blocks are examples unless they name the file they come from.

## Running Tests

```bash
dotnet test HomepageApi.slnx                                                     # unit and integration tests
dotnet test HomepageApi.Tests                                                    # unit tests only (fast; use with dotnet watch)
dotnet test HomepageApi.IntegrationTests                                         # wrapper integration tests only
dotnet test --filter "FullyQualifiedName~FeedbackControllerTests.CreateShould.ReturnOk_AndStoreFeedback"   # single test
```

Tests run in parallel at method level (`MSTestSettings.cs`), so tests must not share mutable state.

## Stack

- **MSTest** via `MSTest.Sdk`
- **NSubstitute** for mocking
- **FluentAssertions** for assertions
- **RandomTestValues** for randomized test data
- **ArchUnitNET** for architecture tests that need type-level dependencies
- **Microsoft.AspNetCore.App** framework reference for API integration tests. `Program` is declared `public partial class Program` in the IoC project so `WebApplicationFactory<Program>` can reach it.

## Test Driven Development

Every change follows the **Red/Green/Refactor** cycle. Implementation code may not be written without a verified failing test. Tests for behavior that already exists follow [Tests that pass on the first run](#tests-that-pass-on-the-first-run) instead.

### 1. Red — write a failing test

- Create the test file following the [naming rules](#file-organization--naming).
- Use randomized data from `RandomTestValues` instead of constants, so hardcoded implementations (e.g. `return 0`) can't pass by accident.
- Run it with `dotnet test --filter <TestName>`. It **must fail**. If the code doesn't exist yet, add stubs that throw `NotImplementedException` so it compiles, then confirm the failure.

### 2. Green — simplest implementation

- Write the minimum code needed to pass. Don't implement the full algorithm yet; if `Add(0,0)` expects `0`, return `0`. This forces the next test case.
- Run the test. It must pass.

### 3. Refactor — improve structure only

- **Refactoring must never change a test's Assert statements.** If an assert has to change, you're in a new Red phase.
- Use expressive names, remove magic strings, extract methods, and use `TestBase` classes to reduce setup boilerplate.

### 4. Iterate

Add the next test case to evolve the logic. Use `[DataRow]` to add permutations to existing tests efficiently.

### Tests that pass on the first run

Some required tests pass as soon as they're written, because the behavior they check already exists. For example:

- ASP.NET Core already returns 500 for an unhandled exception.
- `[ApiController]` already returns 400 for a request DTO that another endpoint's tests already validate.
- A contract test can describe a wrapper that's already written.

These are **coverage tests**. They pin down behavior that [Coverage Requirements](#coverage-requirements) demands; they don't drive new code. They're allowed, under three rules:

1. **Prove the test can fail by breaking the code, never the test.** Temporarily break the behavior it covers, then run it and watch it fail with a message that names the problem. Restore the code afterwards. Ways to break the behavior include removing the validation attribute, catching the exception and returning 200, or making the repository build a different URI. Don't edit the assertion to see red: that only proves the assertion can fail, not that the test checks the behavior.
2. **Write no production code for it.** If a coverage test fails, it has found a bug, and you're in a normal Red phase. For example, a concurrency contract test can expose a race in a wrapper that was assumed to be thread-safe.
3. **Say so.** In the commit or PR, note which tests passed on their first run and how each was shown to fail.

If a test passes immediately and no change to the code makes it fail, it isn't checking anything. Fix it or delete it.

## Coverage Requirements

Every feature must cover these scenarios where applicable:

| Path | Status | Scenario |
|---|---|---|
| Happy | 200 OK | Request succeeded with data |
| Happy | 201 Created | Resource created |
| Happy | 204 No Content | Update/Delete succeeded |
| Unhappy | 400 Bad Request | Validation failure (invalid model, missing fields) |
| Unhappy | 404 Not Found | Resource doesn't exist (Get/Update/Delete) |
| Unhappy | 500 Internal Server Error | Unexpected downstream failure (exception thrown from a mocked wrapper) |

## Mocking Rules

Mock as little as possible. Put the mock at the lowest point that has to be faked: the call that actually leaves the process (network, database, clock, file system). Everything above that point runs for real in the test: query building, parameter mapping, response parsing.

- **Allowed:** mocking infrastructure wrappers (`ISqlDatabaseWrapper`, `IHttpClientWrapper`†, `IDocumentDatabaseWrapper`†, `IDateTimeWrapper`†, …), and only at the method that performs the external call.
- **Forbidden:** mocking Domain repository interfaces (e.g. `IFeedbackRepository`) in logic tests. When testing Commands/Queries, use the **real** repository injected with a **mocked** wrapper.
- **Forbidden:** mocking domain entities or value objects. Use real instances.

### Mock at the boundary, not above it

- A wrapper is a thin pass-through to the driver. It takes the final value that goes to the external system (the full request URI, the SQL text, the stored procedure name and its parameters, the document collection and id or filter). It contains no business logic and builds no queries. The only thing it may do is translate the driver's result into the wrapper's contract, e.g. turning MongoDB's matched count into the `bool` that `Replace` returns. That translation is covered by the wrapper's [integration tests](#integration-tests), not by unit tests.
- The wrapper interface is named after the kind of store (`ISqlDatabaseWrapper`), not the technology behind it (`Dapper…`, `SqlServer…`), so the same tests keep working when the implementation changes. See [Wrapper Interfaces](CONTRIBUTING.md#wrapper-interfaces).
- The repository builds that value. Because the test uses the real repository, the query is exercised, not hidden.
- Set the mock to respond **only to the exact expected value**, e.g. `SqlDatabase.Received(1).ExecuteProcedureAsync("StoreRequest", ExpectedParameters(request))` with a parameters record compared by value, not `Arg.Any<string>()`. If the repository builds the wrong URI or query, the mock returns nothing and the test fails.
- **Forbidden:** wrapper methods that take domain intent instead of the raw call, e.g. `InsertFeedbackAsync(submission)`† or `ExecuteStoreRequestProc(name, message)`†. They move the query out of reach of tests.
- **Forbidden:** `Arg.Any<T>()` for the value that identifies the call (URI, query, procedure name). Use it only for arguments that don't matter to the behavior under test, like a `CancellationToken`.

## Assertions

- Assert on return values or state changes whenever possible.
- Use `Received()` interaction checks only for void methods where no return value or state change can be observed (e.g. logging, or the final boundary call).

## Test Style

- **Required:** `// Arrange`, `// Act`, `// Assert` comments delimiting each phase.
- **Forbidden:** other inline comments explaining what or why. Extract logic into well-named private methods instead.
- **Forbidden:** anonymous types, and lambdas longer than a single one-line expression, including in test data (see [No Anonymous Types; Keep Lambdas Short](CONTRIBUTING.md#no-anonymous-types-keep-lambdas-short)). Short lambdas such as `Arg.Is<Car>(car => car.Id == id)` are fine. For data-driven cases, either is fine: short lambdas (`("Make empty", request => request with { Make = "" })`) or named methods passed by name (`("Make empty", WithEmptyMake)`).

## File Organization & Naming

- Pattern: `HomepageApi.Tests/<Namespace>/<ClassName>Tests/<MethodName>Should.cs`
- **One file per method under test.** The file holds one `[TestClass]` named `<MethodName>Should`, and every scenario for that method is a test method in it, named `<Outcome>_<Condition>`. Together they read as a sentence: `GetShould.ReturnNotFound_WhenCarDoesNotExist`.
- Examples:
  - `Api/FeedbackControllerTests/CreateShould.cs` → `ReturnOk_AndStoreFeedback`, `ReturnBadRequest_WhenRequestIsInvalid`, `ReturnInternalServerError_WhenDatabaseFails`
  - `Api/CarsControllerTests/GetShould.cs` → `ReturnOk_WhenCarExists`, `ReturnNotFound_WhenCarDoesNotExist`
- Share setup across a class's method files with an abstract `<ClassName>TestBase` in the same folder (e.g. `FeedbackControllerTestBase.cs`) that holds `[TestInitialize]`, `[TestCleanup]`, and test-data builders. Each `<MethodName>Should` class inherits from it.
- **Before adding a new test file, read the existing test base class first** so you don't redefine `[TestInitialize]` or duplicate setup logic.

## Architecture Tests

`HomepageApi.Tests/Architecture/` holds tests that enforce the conventions in [CONTRIBUTING.md](CONTRIBUTING.md) that a style analyzer can't see. They read the compiled assemblies, so they check what the code actually does, not just how it looks. Where the assemblies can't show something, they read the project files or the source.

| File | Enforces |
|---|---|
| `LayersShould.cs` | References between layers, both declared in `.csproj` files and compiled into the assemblies; no packages in Domain |
| `InfrastructureShould.cs` | Only `DataAccess/Wrappers` depends on infrastructure namespaces (uses ArchUnitNET to read method bodies) |
| `CodeShould.cs` | Banned libraries (as packages and in code), no anonymous types, lambdas limited to one-line expressions, `Async` suffix |
| `FeatureFoldersShould.cs` | No types directly in a feature folder root |
| `ApiShould.cs` | Controllers and API models expose no Domain types |
| `HandlersShould.cs` | Handler interface, method count, return types, and method naming |

- Files are named `<Subject>Should.cs`, with one test per rule, so a failure reads as a sentence: `HandlersShould.ImplementTheirOwnInterface`. This is an exception to the `<ClassName>Tests/<MethodName>Should.cs` pattern, because there's no single class under test.
- Each test collects *every* violation and asserts `ViolationReport.Of(violations).Should().BeEmpty()`, so one run reports all offending types or methods by name. Don't assert `violations.Should().BeEmpty()` directly: on a list, FluentAssertions names only the first offender.
- **When you add a convention that can be checked mechanically, add a rule here.** Before trusting it, prove it can fail: plant a violation, watch the test fail with a message that names it, then remove the violation. A rule that has never failed may not be checking anything.
- **Check references in the project files as well as the assemblies.** The compiler leaves an unused project or package reference out of the assembly metadata, so a rule that reads only `GetReferencedAssemblies()` or type dependencies can't see a reference until code uses it. `ProjectFiles` reads the `ProjectReference` and `PackageReference` items of every project in the solution, so `LayersShould.DeclareProjectReferencesOnlyInward` and `CodeShould.NotReferenceBannedPackages` catch the reference as soon as it's added.
- Anonymous types are detected in the compiled assemblies, by the names the compiler generates for them (`<>f__AnonymousType…`). Lambda length can't be seen after compilation, so `CodeShould.KeepLambdasToOneLineExpressions` parses the source files with Roslyn (`Microsoft.CodeAnalysis.CSharp`). It flags block-bodied lambdas, lambdas that span lines, and `delegate { … }` blocks, and reports each by file and line.

## Integration Tests

Unit tests mock wrappers, so nothing in `HomepageApi.Tests` proves a wrapper actually works. Every mocked setup encodes an assumption about the real dependency, e.g. `Replace(...).Returns(true)` assumes `true` means "found and replaced". `HomepageApi.IntegrationTests` checks those assumptions by running each wrapper against its real dependency.

### Contract tests

Test each wrapper **interface** as a contract, written once and run against every implementation:

- `<WrapperInterface>Contract` (e.g. `SqlDatabaseWrapperContract`†) is an abstract class holding the `[TestMethod]`s. Together they describe the behavior that repositories and unit-test mocks rely on: what's returned when something is missing, what throws, and what's isolated from what. It creates the wrapper through an abstract method and has no `[TestClass]` attribute.
- `<Implementation>Should` (e.g. `DapperSqlDatabaseWrapperShould`†) is a `[TestClass]` that inherits the contract and supplies a real instance wired to a real dependency. MSTest runs every inherited test against it.
- **A new implementation is done when it passes the existing contract.** That's what makes swapping technology within a kind safe (see [Wrapper Interfaces](CONTRIBUTING.md#wrapper-interfaces)). If the new driver behaves differently, the wrapper translates the result into the contract; the contract doesn't change to fit the driver.
- When a unit test starts relying on a wrapper behavior the contract doesn't cover yet, add that case to the contract.
- `WrappersShould.HaveAnIntegrationTest` fails if any class in `DataAccess/Wrappers` lacks a matching `<Implementation>Should` class.
- **Current exemption:** `DapperSqlDatabaseWrapper` is a stub that throws `NotImplementedException` until a database is chosen, so `WrappersShould` skips it (see the comment there). When it gets a real implementation, write `SqlDatabaseWrapperContract` and `DapperSqlDatabaseWrapperShould` against a Testcontainers database and remove the exemption.

These files follow the contract instead of the `<MethodName>Should.cs` pattern. A contract describes how several methods interact (insert, then find), so splitting it per method would scatter one specification across many files.

### Real dependencies, local and disposable

| Dependency | Use |
|---|---|
| In-memory store, clock | The real instance; nothing external to start |
| HTTP API | An in-process Kestrel server on a random local port, serving known responses. Never call real third-party APIs. |
| SQL Server, PostgreSQL, MongoDB, … | [Testcontainers](https://dotnet.testcontainers.org/): one throwaway container per test class or assembly, started in `[ClassInitialize]`/`[AssemblyInitialize]` |

- **Isolate every test.** Use random collection, table or database names from `RandomTestValues`, so tests can run in parallel against a shared container.
- **Mock nothing.** An integration test that mocks a dependency is a unit test with extra steps.
- **Keep them in their own project.** `HomepageApi.Tests` stays fast and Docker-free for `dotnet watch test`. CI runs `HomepageApi.IntegrationTests` as its own job, on agents with Docker.
