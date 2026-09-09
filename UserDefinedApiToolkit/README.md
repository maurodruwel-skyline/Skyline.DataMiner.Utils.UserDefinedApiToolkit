Skyline.DataMiner.Utils.UserDefinedApiToolkit
===

## About

Quickly build REST APIs in DataMiner using an attribute/controller-based approach similar to ASP.NET Core.
This package builds upon the [User-Defined API](https://aka.dataminer.services/about-dataminer) actions
available in DataMiner and makes them easier to use, providing attribute routing, dependency injection,
typed results, and automatic OpenAPI specification generation.

```csharp
using Microsoft.Extensions.DependencyInjection;

using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Net.Apps.UserDefinableApis.Actions;
using Skyline.DataMiner.Utils.UserDefinedApiToolkit;

/// <summary>
///     DataMiner Script Class.
/// </summary>
public static class Script
{
	private static IUserDefinedApi _api;

	/// <summary>
	///     The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLScripting process.</param>
	/// <param name="requestData">The incoming API request.</param>
	[AutomationEntryPoint(AutomationEntryPointType.Types.OnApiTrigger)]
	public static ApiTriggerOutput OnApiTrigger(IEngine engine, ApiTriggerInput requestData)
	{
		// Build the API once and cache it; AddControllers() scans the calling assembly and
		// registers every public ControllerBase implementation decorated with [Route].
		if (_api is null)
		{
			_api = UserDefinedApi.CreateBuilder()
				.AddControllers()
				.ConfigureServices(services => services.AddScoped<IUserRepository, UserRepository>())
				.Build();
		}

		return _api.Run(engine, requestData);
	}
}

// You can define your endpoints by inheriting from the ControllerBase class
[ApiController]
[Route("v1/users")]
public class UsersController : ControllerBase
{
	private readonly IUserRepository _repository;

	public UsersController(IUserRepository repository)
	{
		_repository = repository;
	}

	[HttpGet]
	[Produces("application/json")]
	public ApiResult<List<UserDto>, string> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
	{
		var users = _repository.GetAll(page, pageSize);
		return Ok(users);
	}

	[HttpGet("{id}")]
	[Produces("application/json")]
	public ApiResult<UserDto, string> GetById(int id)
	{
		var user = _repository.GetById(id);
		return user is null ? NotFound("User not found.") : Ok(user);
	}

	[HttpPost]
	[Consumes("application/json")]
	public ApiResult<UserDto, string> Create([FromBody] UserDto dto)
	{
		_repository.Create(dto);
		return Created(dto);
	}
}
```

If you have questions, you can post them to
our [DataMiner community platform](https://community.dataminer.services/questions/).

### Installation

```bash
dotnet add package Skyline.DataMiner.Utils.UserDefinedApiToolkit
```

## Features

| Feature | Description |
| --- | --- |
| **Attribute routing** | `[ApiController]`, `[Route]`, `[HttpGet]`/`[HttpPost]`/`[HttpPut]`/`[HttpDelete]`, path variables (`{id}`) |
| **Parameter binding** | `[FromRoute]`, `[FromQuery]` and `[FromBody]` |
| **Typed results** | `ApiResult<TSuccess>` / `ApiResult<TSuccess, TError>` plus helpers such as `Ok`, `NotFound`, `BadRequest`, `Created`, `Conflict`, `StatusCode`, ... |
| **Dependency injection** | Built-in DI container via `ConfigureServices` and constructor injection in controllers |
| **OpenAPI generation** | Generates an OpenAPI 3.0 spec from your controllers at build time |

### Generating an OpenAPI specification

Add the following to your API project's `.csproj` to generate an `openapi.yaml` (or `.json`) file in your build output whenever you build:

```xml
<PropertyGroup>
  <GenerateOpenApi>True</GenerateOpenApi>
  <OpenApiFormat>yaml</OpenApiFormat> <!-- yaml (default) or json -->
  <OpenApiInfoTitle>Sample User API</OpenApiInfoTitle>
  <OpenApiInfoVersion>1.0.0</OpenApiInfoVersion>
  <OpenApiInfoDescription>This is a **global description** of the API.</OpenApiInfoDescription>
</PropertyGroup>
```

The generated document includes every controller's routes, HTTP methods, request/response schemas, and (when `GenerateDocumentationFile` is enabled) the XML doc comments on your actions.
`OpenApiInfoDescription` is optional; when configured, it is emitted as the top-level `info.description` value.

### Path variables

Route templates support ASP.NET-Core-style `{placeholder}` segments on the `[HttpGet]`/`[HttpPost]`/
`[HttpPut]`/`[HttpDelete]` attributes. The method-level template is appended to the controller's
`[Route]` template to form the full route (e.g. `"v1/items"` + `"{id}"` → `"v1/items/{id}"`).

A parameter binds from a placeholder either **implicitly** (its name matches the placeholder) or
**explicitly** via `[FromRoute]` (with an optional `Name` override when the C# parameter name
differs from the placeholder name):

```csharp
[ApiController]
[Route("v1/items")]
public class ItemsController : ControllerBase
{
	// Implicit binding: the "id" parameter name matches the "{id}" placeholder.
	[HttpGet("{id}")]
	public IApiResult GetById(int id)
	{
		return Ok(id);
	}

	// Explicit [FromRoute(Name = ...)] override: the C# parameter name ("itemId") differs
	// from the placeholder name ("id") in the route template.
	[HttpGet("{id}/details")]
	public IApiResult GetDetails([FromRoute(Name = "id")] int itemId)
	{
		return Ok(itemId);
	}
}
```

A literal route segment always outranks a placeholder segment for the same request (e.g. a request
to `v1/items/count` matches a literal `[HttpGet("count")]` action over `[HttpGet("{id}")]`).

Route parameter values are converted using the same conversion logic as `[FromQuery]` parameters,
so `InvalidParameterException` is thrown if a route value can't be converted to the parameter's
type. Every placeholder must have a matching bound parameter (implicit or `[FromRoute]`), and every
`[FromRoute]` parameter must reference a placeholder that actually exists in the combined route
template — mismatches throw `InvalidRouteException` eagerly, at `Build()` time.

`[FromQuery]` also supports a `Name` override (e.g. `[FromQuery(Name = "q")]`), for the same
use case with query string parameters.

Route constraints (e.g. `{id:int}`) and catch-all/wildcard segments (e.g. `{*path}`) are not
supported.

### Access API Context

Access the underlying API request and response through the `ApiContext` property:

```csharp
public class MyController : ControllerBase
{
    public IApiResult MyAction()
    {
        var request = this.Request;  // ApiTriggerInput
        var response = this.Response;  // ApiTriggerOutput
        // ...
    }
}
```

## About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

## About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.

