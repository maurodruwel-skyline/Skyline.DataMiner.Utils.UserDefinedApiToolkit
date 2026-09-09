# Build and packaging

## OpenAPI generation

The NuGet package includes an MSBuild task that can generate an OpenAPI 3.0 document after the
consumer project builds. Add these properties to the consuming `.csproj`:

```xml
<PropertyGroup>
	<GenerateOpenApi>True</GenerateOpenApi>
	<OpenApiFormat>yaml</OpenApiFormat>
	<OpenApiInfoTitle>Items API</OpenApiInfoTitle>
	<OpenApiInfoVersion>1.0.0</OpenApiInfoVersion>
	<OpenApiInfoDescription>This is a **global description** of the API.</OpenApiInfoDescription>
	<GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

Configuration properties:

| Property | Values/default | Description |
| --- | --- | --- |
| `GenerateOpenApi` | `True` to enable | Runs the generator after the build. |
| `OpenApiFormat` | `yaml` | Output format; `yaml` or `json`. |
| `OpenApiInfoTitle` | Project name | API title in the generated document. |
| `OpenApiInfoVersion` | `$(Version)` | API version in the generated document. |
| `OpenApiInfoDescription` | Empty | Optional global API description in the generated document. Markdown is preserved. |
| `GenerateDocumentationFile` | Project setting | When enabled, XML comments enrich the generated document. |

The generated file is written to the build output directory as `openapi.yaml` or `openapi.json`.
`[Consumes]`, `[Produces]`, and `[ProducesResponseType]` describe request and response metadata in
OpenAPI; they do not change runtime serialization or request handling.

## Centralized installer packaging

The installer package generates the JSON metadata files used to install User-Defined APIs. Add the
installer package to a central DataMiner package project
(`<DataMinerType>Package</DataMinerType>`) and list the UDAPI projects that should be built and
included:

```xml
<PropertyGroup>
	<DataMinerType>Package</DataMinerType>
</PropertyGroup>

<ItemGroup>
	<UdapiProject Include="..\Orders API\Orders API.csproj" />
	<UdapiProject Include="..\Users API\Users API.csproj" />
</ItemGroup>
```

The installer package builds each listed project before the package project, generates a metadata
file named after each project (for example, `OrdersApi.udapi.json`), and copies the files to the
package project's `SetupContent\UDAPI` directory. Independent projects are built in parallel.

`UdapiProject` uses standard MSBuild item syntax, so projects can also be selected with wildcards:

```xml
<ItemGroup>
	<UdapiProject Include="..\Apis\**\*.csproj"
				 Exclude="..\Apis\UdapiInstaller\**\*" />
</ItemGroup>
```

All matched projects are built and must be UDAPI projects. Exclude the central package project if
it is within the wildcard scope. The installer package validates that the selected projects import
the toolkit targets and have matching script XML files before building them.

For the runtime API guides, see [controllers and routing](controllers-and-routing.md) and
[configuration and responses](configuration-and-responses.md).
