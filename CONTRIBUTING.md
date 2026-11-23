# Contributing

> Names marked † are illustrative: they show how something would be named or used, but they don't exist in this repository. Examples about cars (`Car`, `GetCar`, `CarsController`, `Engine`, …) come from the DotNetExample template these conventions are taken from and are all illustrative too; this repository's real feature is Feedback. Code blocks are examples unless they name the file they come from.

This solution follows the DotNetExample conventions for .NET projects, so every change must preserve its predictable structure. See [README.md](README.md) for the architecture overview, [TESTING.md](TESTING.md) for the required TDD workflow, and [REVIEW.md](REVIEW.md) for what reviewers check.

## Architecture Rules

- **Domain has no dependencies.** It must not reference DataAccess, Api, or external infrastructure packages.
- **Api must not reference DataAccess.** Controllers depend only on Domain interfaces.
- **IoC is the only entry point.** `Program.cs` lives in `HomepageApi.IoC`, never in the Api project.
- **Centralized DI.** All registrations are composed in `HomepageApi.IoC/RegisterDependencies.cs`. Layers may expose `services.Add[LayerName]()` extension methods (e.g. `ApiConfiguration.AddApi()`), which IoC calls.
- **Repository + Wrapper pattern.** DataAccess repositories implement Domain repository interfaces and access the underlying store (HTTP, relational database, document database, clock, …) only through a wrapper interface such as `ISqlDatabaseWrapper`. This lets tests use real repositories with mocked low-level drivers. Wrapper interfaces are named after the *kind* of store, never the product (see [Wrapper Interfaces](#wrapper-interfaces)).

## Design Patterns

### CQRS

All business logic is split into Commands (writes) and Queries (reads):

- **Commands**: `HomepageApi.Domain/Commands/<Feature>`, return `Task`.
- **Queries**: `HomepageApi.Domain/Queries/<Feature>`, return `Task<T>`.
- Name handlers `VerbNoun` (e.g. `GetCar`, `AddCar`), each implementing its own interface (`IGetCar`).
- Inject repository interfaces through the constructor.

#### Handler method names

A handler has one public method. Its name is the word that **completes the handler's name as a sentence**, usually a preposition with the argument as its object, followed by `Async`. Read the call aloud: it should say exactly what happens.

| Call | Reads as |
|---|---|
| `_getCar.ByAsync(id)` | get car by id |
| `_getCars.AllAsync()` | get all cars |
| `_addCar.WithAsync(car)` | add car with these details |
| `_updateCar.WithAsync(car)` | update car with these details |
| `_deleteCar.ByAsync(id)` | delete car by id |
| `_setFoo.ToAsync(value)`† | set foo to value |

- **Be specific.** Never use a generic name like `ExecuteAsync`, `HandleAsync` or `RunAsync`. It says nothing the handler's type doesn't already say.
- **Don't repeat yourself.** The noun is already in the handler name and the argument, so `ByAsync(id)`, not `ByIdAsync(id)`; `WithAsync(car)`, not `WithCarAsync(car)`.

### API Models

The API layer receives and returns its own types, never Domain types. Then the JSON contract clients depend on and the domain model can change independently: renaming a domain property doesn't break clients, and an API change doesn't leak into the domain.

- **Requests** are `<Name>Request` records in `Api/Models/<Feature>`, carrying the validation attributes. They map to the domain with an instance method: `request.ToCar(id)`.
- **Responses** are `<Name>Response` records in the same folder. They're built from the domain with a static factory: `CarResponse.From(car)`.
- **Enums too.** The API declares its own (`Api.Models.Cars.EngineShape`) and maps it to the domain enum with an explicit `switch`, never a cast. That way, renaming or reordering a domain enum can't silently change the API.
- Controller action parameters, `[ProducesResponseType]` types, and the public properties of API models may only use API or .NET types. `Architecture/ApiShould` enforces this.

```csharp
// Don't: the domain entity is the API contract
[ProducesResponseType<Car>(StatusCodes.Status200OK)]
return Ok(car);

// Do: map at the boundary
[ProducesResponseType<CarResponse>(StatusCodes.Status200OK)]
return Ok(CarResponse.From(car));
```

### Feature Folders

Inside each layer, group code into a folder per feature, and make the namespace match the folder path. A reader looking for everything about feedback finds it under a `Feedback/` folder in every layer.

| Kind | Path | Namespace |
|---|---|---|
| Command | `HomepageApi.Domain/Commands/Feedback/StoreFeedback.cs` | `HomepageApi.Domain.Commands.Feedback` |
| Query | `HomepageApi.Domain/Queries/Feedback/GetFeedback.cs`† | `HomepageApi.Domain.Queries.Feedback` |
| Repository interface | `HomepageApi.Domain/DataAccess/Feedback/IFeedbackRepository.cs` | `HomepageApi.Domain.DataAccess.Feedback` |
| Entity / value object | `HomepageApi.Domain/Entities/Feedback/FeedbackSubmission.cs` | `HomepageApi.Domain.Entities.Feedback` |
| Domain exception | `HomepageApi.Domain/Exceptions/Feedback/FeedbackNotFoundException.cs`† | `HomepageApi.Domain.Exceptions.Feedback` |
| Repository | `HomepageApi.DataAccess/Feedback/FeedbackRepository.cs` | `HomepageApi.DataAccess.Feedback` |
| Request/response DTO | `HomepageApi.Api/Models/Feedback/FeedbackRequest.cs` | `HomepageApi.Api.Models.Feedback` |

Don't name a type the same as its feature folder (a `Feedback` record in `Entities/Feedback/`): inside `HomepageApi.Domain.Commands.Feedback`, the bare name `Feedback` resolves to the namespace, not the type. That's why the entity is `FeedbackSubmission`.

Two exceptions stay flat:

- **Controllers** (`Api/Controllers/FeedbackController.cs`): a controller already *is* one feature.
- **Shared infrastructure** that no single feature owns, such as `DataAccess/Wrappers/`.

### Wrapper Interfaces

A wrapper interface has to balance two needs:

- **It should outlive any one technology.** The backing store may change, e.g. from an in-memory store to MongoDB.
- **Its inputs must be the raw query, so tests can match them exactly** (see [Mock at the boundary](TESTING.md#mock-at-the-boundary-not-above-it)). A raw query is always written for a particular kind of store: SQL text is relational, and a collection plus id or filter is a document lookup.

To meet both, **name and shape the interface after the kind of store, and name the implementation after the technology.**

| Kind of store | Interface | Raw input the repository builds | Implementations |
|---|---|---|---|
| Document database | `IDocumentDatabaseWrapper`† | Collection name, id, document | `InMemoryDocumentDatabaseWrapper`†, `MongoDocumentDatabaseWrapper`† |
| Relational database | `ISqlDatabaseWrapper` | Stored procedure name (or SQL text) and a parameters record | `DapperSqlDatabaseWrapper` (a stub until a database is chosen) |
| HTTP API | `IHttpClientWrapper`† | Full request URI | `HttpClientWrapper`† |
| Clock | `IDateTimeWrapper`† | (none) | `SystemDateTimeWrapper`† |

- **No product or hosting names in the interface.** `InMemory`, `Mongo`, `SqlServer` and `Cosmos` belong on the implementation class only.
- **Use the kind's own vocabulary.** A document wrapper has `Find`/`Insert`/`Replace`/`Delete` on a collection; a SQL wrapper takes SQL text or a stored procedure name. Don't flatten every kind into one generic `IDatabaseWrapper`. To hide whether a call is SQL or a document lookup, it would have to accept domain intent (`GetCar(id)`), which moves the query out of reach of tests.
- **Changing technology within a kind** (in-memory → MongoDB) means writing a new implementation and changing one line in `RegisterDependencies.cs`. Repositories and tests don't change.
- **Changing to a different kind** (document → relational) means a different wrapper interface, and the repository changes too, because the query it builds is different. That's expected, because the repository owns the query.

### Preferred Libraries

| Concern | Use | Instead of |
|---|---|---|
| JSON serialization | `System.Text.Json` | `Newtonsoft.Json` |
| SQL data access | Dapper | Entity Framework Core and other full ORMs |

- **System.Text.Json** is built into .NET and is what ASP.NET Core already uses, so the app has one serializer and one set of options. Configure it in `ApiConfiguration.AddApi()`, and have tests deserialize with matching `JsonSerializerOptions`.
- **Dapper** lives only in DataAccess, inside the `ISqlDatabaseWrapper` implementation (`DapperSqlDatabaseWrapper`). The repository writes the SQL text or procedure name and the parameters, and the wrapper hands them straight to Dapper (e.g. `connection.QueryAsync<T>(sql, parameters)`). The query stays in the repository, where tests can match it exactly (see [Wrapper Interfaces](#wrapper-interfaces)). EF Core builds its SQL from LINQ expressions at runtime, which hides the query from tests.
- **Pass Dapper parameters as a named record**, not the usual anonymous object: `new StoreFeedbackParameters(submission.Name, submission.Message, submission.RespondBy)`, not `new { submission.Name, … }`. Dapper maps record properties to `@Id`-style parameters by name.

### Naming

- Interfaces are PascalCase prefixed with `I` (e.g. `IFeedbackRepository`).
- Async methods end with `Async` (e.g. `StoreFeedbackAsync`), including handler methods (`_storeFeedback.WithAsync(submission)`). Two exceptions:
  - **Controller actions** (`Get`, `Create`, …) are named after the HTTP operation. ASP.NET Core strips `Async` from action names by default ([`SuppressAsyncSuffixInActionNames`](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.mvcoptions.suppressasyncsuffixinactionnames)), so an action named `GetAsync` breaks `CreatedAtAction(nameof(GetAsync), …)` at runtime.
  - **Test methods** describe a scenario (`ReturnOk_WhenCarExists`), not an operation.

## Clean Code

Code is read far more often than it is written, so optimize for the reader.

### Readability

- **Code should read like a sentence.** A caller should be able to follow the flow from method and variable names alone, e.g. `if (order.IsOverdue()) await NotifyCustomerAsync(order);`.
- **Keep functions small.** Each function does one thing at one level of abstraction. When a function needs a section break, or you're tempted to explain a block, extract it into a well-named method.
- **Choose descriptive names over short ones.** Names say what something does or represents (`CalculateShippingCost`, `activeCustomers`), not how it's built (`DoWork`, `list2`, `tmp`). Avoid abbreviations and generic names like `Helper`, `Manager`, or `Data`.

### No Anonymous Types; Keep Lambdas Short

Shapes and logic get names. This applies to production code and tests.

**Anonymous types are never allowed.**

- Declare a named record whose properties give the shape (e.g. `record CarRouteValues(Guid Id)`).
- **String-keyed bags count as anonymous.** `new RouteValueDictionary { ["id"] = car.Id }`, `new Dictionary<string, object> { … }` and similar initializers just rebuild an anonymous object with magic-string keys. Its shape is still undeclared, so declare a record instead.

**Lambdas are fine when they're short:** a single expression on a single line. That covers the idiomatic cases, where a lambda is what any .NET reader expects:

- LINQ predicates and selectors: `cars.Where(car => car.Engine.Cylinders > 6)`
- options and builder configuration: `AddJsonOptions(options => options.AllowTrailingCommas = true)`
- libraries that need an expression tree, which a named method can't provide: MongoDB filter builders, `Arg.Is<Car>(car => car.Id == id)`

**Anything longer becomes a named method,** passed as a method group (`x.DoFoo(DoAThing)`) or written as a local function:

- a lambda with a block body: `x => { …; …; }`
- a lambda that wraps onto more than one line
- `delegate { … }` blocks: never use them; write a short lambda or a named method instead

**Why:** once a lambda needs a block or a second line, it's doing enough that it deserves a name. The name says *what* it does, so the call still reads like a sentence (`app.Use(LogRequestDurationAsync)`†) without the reader decoding the body. Named methods also show up by name in stack traces and can be reused.

Expression-bodied members (`public int Count => _items.Count;`) aren't lambdas. They're named members, and they're allowed.

```csharp
// Fine: short and idiomatic
var bigEngines = cars.Where(car => car.Engine.Cylinders > 6);

// Don't: a block-bodied lambda
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    await next(context);
    logger.LogInformation("{Path} took {Elapsed}", context.Request.Path, stopwatch.Elapsed);
});

// Do: name it
app.Use(LogRequestDurationAsync);

// Don't: anonymous types, including string-keyed bags
return CreatedAtAction(nameof(Get), new { id = car.Id }, car);
return CreatedAtAction(nameof(Get), new RouteValueDictionary { ["id"] = car.Id }, car);

// Do: a named record
return CreatedAtAction(nameof(Get), new CarRouteValues(car.Id), car);
```

### Comments

Comments should almost never be needed. If code needs a comment to explain *what* it does, rename or extract until it doesn't.

- **Allowed: comments that explain *why*.** Use them for code that looks wrong or surprising but has to be that way, e.g. unusual startup configuration that works around a parsing issue, or an ordering that a library requires. Link to the issue or docs where possible.
- **Allowed: documentation on public-facing APIs.** XML doc comments (`///`) on controller actions (they feed Swagger) and on public members of packages consumed by others, such as NuGet libraries.
- **Forbidden:** comments that restate the code, commented-out code, and change-log comments. Git keeps the history.

Tests are stricter: only the `// Arrange`, `// Act`, `// Assert` markers are allowed (see [TESTING.md](TESTING.md#test-style)).

## Build Configuration

Shared settings live in [`Directory.Build.props`](Directory.Build.props) at the repository root, which every project picks up automatically:

- `TargetFramework`, `Nullable`, `ImplicitUsings`. `LangVersion` is deliberately not set, so C# stays at the version supported for `net10.0` (C# 14) even though the build uses the newer SDK pinned in `global.json`.
- `TreatWarningsAsErrors`, unconditionally, for every configuration and platform
- `EnforceCodeStyleInBuild`, so the `.editorconfig` rules marked `error` fail `dotnet build`, not just the IDE

Project files should only hold what's specific to that project: SDK, references, and packages. Don't redefine shared settings in a project, and don't silence a rule with `NoWarn` or a lower `.editorconfig` severity without a *why* comment.

Rules that a style analyzer can't see are enforced by architecture tests (see [TESTING.md](TESTING.md#architecture-tests)). [REVIEW.md](REVIEW.md#enforced-automatically) lists every rule and what enforces it.

CI ([`.github/workflows/build.yml`](.github/workflows/build.yml)) runs the build and tests on every pull request, on pushes to `main`, and weekly; it must be a required check (see [Continuous Integration](README.md#continuous-integration)).

## Development Workflow

1. **Understand the requirement** before writing anything.
2. **Plan the first failing test.**
3. **Run the Red/Green/Refactor cycle** described in [TESTING.md](TESTING.md), repeating until the feature is complete. Writing implementation code without a verified failing test is not allowed.
4. **Work in small steps.** Don't generate large amounts of code at once, and don't anticipate future requirements — solve only the current failing test.
5. **Check conformity.** New files must land in the correct layer and [feature folder](#feature-folders) and use the matching namespace.
6. **Register new types** in `RegisterDependencies.cs`.

## Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/) for every commit and PR title:

```text
<type>(<optional scope>): <short summary in the imperative>
```

- Common types: `feat`, `fix`, `refactor`, `test`, `docs`, `build`, `ci`, `chore`.
- Mark breaking changes with `!` after the type (e.g. `feat!: Remove v1 endpoints`) or a `BREAKING CHANGE:` footer.
- Examples: `feat: Add GetFeedback query`, `fix(api): Return 400 when Message is missing`, `docs: Add clean code rules`.
