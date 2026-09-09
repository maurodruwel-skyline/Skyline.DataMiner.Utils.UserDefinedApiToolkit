namespace Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;

	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.Logging;
	using Skyline.DataMiner.Utils.UserDefinedApiToolkit.Build.OpenApi;

	public class OpenApiTask : Task
	{
		[Required]
		public string? OutputPath { get; set; }

		/// <summary>
		/// Gets or sets the path to the compiled assembly that will be analyzed for OpenAPI generation.
		/// </summary>
		[Required]
		public string? TargetPath { get; set; }

		/// <summary>
		/// Gets or sets the name of the project, used as the OpenAPI document title.
		/// </summary>
		[Required]
		public string? ProjectName { get; set; }

		/// <summary>
		/// Gets or sets the version of the project, used as the OpenAPI document version.
		/// </summary>
		[Required]
		public string? ProjectVersion { get; set; }

		/// <summary>
		/// Gets or sets the optional description of the OpenAPI document.
		/// </summary>
		public string? ProjectDescription { get; set; }

		/// <summary>
		/// Gets or sets the assembly references required for loading types during OpenAPI generation.
		/// </summary>
		[Required]
		public ITaskItem[] References { get; set; } = [];

		/// <summary>
		/// Gets or sets the format of the OpenAPI output file. Default is "yaml".<br/>
		/// Options:<br/>
		/// json - Outputs the OpenAPI specification in JSON format.<br/>
		/// yaml - Outputs the OpenAPI specification in YAML format.
		/// </summary>
		public string Format { get; set; } = "yaml";

		/// <summary>
		/// Gets or sets the path to the XML documentation file containing the assembly's documentation comments.
		/// Used to populate descriptions in the OpenAPI specification.
		/// </summary>
		public string? DocumentationFile { get; set; }

		/// <summary>
		/// Executes the OpenAPI generation task.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the OpenAPI file was generated successfully; otherwise, <c>false</c>.
		/// </returns>
		public override bool Execute()
		{
			try
			{
				Log.LogMessage(MessageImportance.Normal, $"Generating OpenAPI file for project '{ProjectName}' version '{ProjectVersion}'...");

				var logger = new MsBuildLogger(Log);

				using var resolver = new ControllerResolver(TargetPath, References.Select(r => r.ItemSpec), DocumentationFile, logger);
				var controllers = resolver.Resolve();
				var doc = OpenApiProjectGenerator.CreateDocument(
					 controllers,
					 ProjectName,
					 ProjectVersion,
					 logger,
					 ProjectDescription);

				var (fileName, content) = OpenApiProjectGenerator.FormatDocument(doc, Format);

				var outputPath = Path.Combine(OutputPath, "openapi", fileName);
				Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
				File.WriteAllText(outputPath, content);

				Log.LogMessage(MessageImportance.Normal, $"Generated OpenAPI document with {doc.Paths.Count} path(s).");
				Log.LogMessage(MessageImportance.Normal, $"OpenApi file generated at: {outputPath}");
				return true;
			}
			catch (Exception ex)
			{
				// showStackTrace: true ensures the full exception (including stack trace and any
				// inner exceptions) ends up as a build ERROR, not just a low-visibility message —
				// otherwise consumers only see the bare ex.Message on build failure.
				Log.LogErrorFromException(ex, showStackTrace: true, showDetail: true, file: null);
				return false;
			}
		}
	}
}