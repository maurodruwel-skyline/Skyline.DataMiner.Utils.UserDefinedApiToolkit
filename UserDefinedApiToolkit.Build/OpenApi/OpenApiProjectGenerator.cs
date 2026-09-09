namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi
{
	using System;
	using System.Collections.Generic;
	using System.IO;

	using Microsoft.OpenApi;

	/// <summary>
	/// Builds an <see cref="OpenApiDocument"/> from resolved user-defined API controllers and
	/// formats it as JSON or YAML.
	/// </summary>
	/// <remarks>
	/// This class contains the actual OpenAPI generation logic and has no dependency on
	/// MSBuild (Microsoft.Build.Framework/Utilities). <see cref="OpenApiTask"/> is a thin
	/// adapter around it that wires up MSBuild task properties and logging; keeping the two
	/// separate allows the generation logic to be used and unit tested independently of the
	/// MSBuild task infrastructure.
	/// </remarks>
	internal static class OpenApiProjectGenerator
	{
		/// <summary>
		/// Builds an OpenAPI document from the resolved user-defined API controllers.
		/// </summary>
		/// <param name="controllers">The resolved user-defined API controllers to document.</param>
		/// <param name="projectName">Used as the OpenAPI document title.</param>
		/// <param name="projectVersion">Used as the OpenAPI document version.</param>
		/// <param name="log">Optional logger used to report progress and diagnostics.</param>
		/// <param name="projectDescription">Optional description of the OpenAPI document.</param>
		/// <returns>The generated <see cref="OpenApiDocument"/>.</returns>
		public static OpenApiDocument CreateDocument(
			IList<ControllerUnit> controllers,
			string? projectName,
			string? projectVersion,
			IBuildLogger? log = null,
			string? projectDescription = null)
		{
			var doc = OpenApiGenerator.Create(controllers, log);
			doc.Info.Title = projectName ?? "User Defined API";
			doc.Info.Version = projectVersion ?? "1.0.0";
			if (!string.IsNullOrWhiteSpace(projectDescription))
			{
				doc.Info.Description = projectDescription;
			}

			return doc;
		}

		/// <summary>
		/// Serializes an <see cref="OpenApiDocument"/> as either JSON or YAML.
		/// </summary>
		/// <param name="doc">The document to serialize.</param>
		/// <param name="format">"json" for JSON output; anything else defaults to YAML.</param>
		/// <returns>The output file name and its serialized content.</returns>
		public static (string FileName, string Content) FormatDocument(OpenApiDocument doc, string format)
		{
			using var sw = new StringWriter();
			if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
			{
				var writer = new OpenApiJsonWriter(sw);
				doc.SerializeAsV3(writer);
				return ("openapi.json", sw.ToString());
			}
			else
			{
				var writer = new OpenApiYamlWriter(sw);
				doc.SerializeAsV3(writer);
				return ("openapi.yaml", sw.ToString());
			}
		}
	}
}
