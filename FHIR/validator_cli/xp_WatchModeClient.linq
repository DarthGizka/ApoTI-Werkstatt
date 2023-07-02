<Query Kind="Program" />

// xp_WatchModeClient.linq
// 2023-06-26 = xp_WatchModeClient.linq
// 2023-06-28 * trigger file dropped (little effect because *any* change triggers revalidation)
//
// A simple class that demonstrates how to use the HL7 FHIR core validator (validator_cli.jar)
// in watch mode as a server, using the filesystem as IPC layer for sending validation requests
// and retrieving the results.
//
// The focus here is on showcasing the steps needed to handle the communication reliably and
// robustly, and to show tried and trusted code for things that are easily to get subtly wrong.
//
// Validator instances have to be started manually, with the right incantation for the desired
// configuration. The WatchModeClient class can show what the parameters needed for communication
// look like, but you have to tweak and execute the command yourself.
//
// The good news is that once a validator instances are running, all you need to start validating
// is to instantiate a client for the configuration name you chose (with some additional parameters
// if the input format is XML instead of JSON and/or you picked a validator output file format that
// is different from `eslint-compact`).
//
// NB: each running validator instance must use different names for its input and output files.
// If these files share a common prefix then that prefix can de facto be used like a configuration
// name (because it identifies a validator instance with a specific configuration). That's what
// the filename logic of the client class is based on.
//
// E.g. if I have validator instance that is configured with the profile packages for the subject
// area of electronic prescriptions in Teutonia (in the versions that are normative starting with
// the 3rd quarter), then I can use 'ERezept2023Q3' as the configuration name. The resulting 
// filenames are 'x:\ERezept2023Q3-input.xml' and 'x:\ERezept2023Q3-output.eslint-compact', for 
// RAM disk x:\ and InputType.XML.
//
// The client does all the magic if you give it the right parameters, so you can simply copy the
// resulting validator command to a shell window and execute it.
//
// The `Main()` function below contains a couple of different experiments; select the one you want
// to run by strategically placing or removing '!' in the #conditionals, and/or configure magic 
// toggle comments via the placement/removal of '*' (like the one for the EXCHANGE_DIRECTORY value).
//
// The communication overhead is fairly small compared to the actual time needed for validation.
// On my notebook I get a throughput of about 1000/s against a simple mock server (which I used to
// iron out the kinks in the communication system), a bit more than 100/s for validating an empty
// Patient resource, and 19/s for validating the 4581 resources in hl7.fhir.r4.core#4.0.1.

void Main ()
{
	// set this to your RAM disk, if you have one; set null to use the Windows %TEMP% directory (slow!)
	const string EXCHANGE_DIRECTORY = /**/ "f:\\WatchModeServer" /*/ null /**/;

#if !true // first steps

	var client = new WatchModeClient(ExchangeDirectory: EXCHANGE_DIRECTORY).Dump();

	Console.WriteLine("validator command as a single line:");
	Console.WriteLine();
	client.ValidatorCommandLine()/**/.Replace("^" + Environment.NewLine + "\t", "")/**/.Dump();
	Console.WriteLine();
	Console.WriteLine("validator command with line continuations:");
	Console.WriteLine();
	client.ValidatorCommandLine().Dump();

	var result = client.Validate(WatchModeClient.PATIENT_42_JSON).Dump();
	
	Console.WriteLine("`ESLintMessage.Parse() with input filename translation:");
	ESLintMessage.Parse(result.Text, client.InputFilename, "Patient_42.json").Dump();

	Console.WriteLine("`client.`Validate()` with input filename translation:");
	client.Validate(WatchModeClient.PATIENT_42_JSON, "John Doe #42.json").Dump();

#elif true // validate 1000 x empty patient

	validate_PATIENT_42_JSON_repeatedly_(EXCHANGE_DIRECTORY /*/, 100000 /**/);

#elif true // validate all *.json in hl7.fhir.r4.core#4.0.1

	validate_JSON_in_cached_package_(EXCHANGE_DIRECTORY, /**/ "hl7.fhir.r4.core#4.0.1" /*/ "hl7.fhir.r5.core#5.0.0" /**/);

#else // free experimentation

	var client = new WatchModeClient(ExchangeDirectory: "x:\\" /**/, InputType: InputType.JSON/**/).Dump();
	client.ValidatorCommandLine().Replace("^" + Environment.NewLine + "\t", "").Dump();
	var filename = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		".fhir/packages/hl7.fhir.r4.core#4.0.1/package/.index.json" );
	var json_text_to_validate = /**/ File.ReadAllText(filename) /*/ WatchModeClient.PATIENT_42_JSON /**/;

	var result = client.Validate(json_text_to_validate, filename).Dump();
	var messages = ESLintMessage.Parse(result.Text, client.InputFilename, filename).Dump();

#endif
}

//-----------------------------------------------------------------------------------------------------------

static void validate_PATIENT_42_JSON_repeatedly_ (string exchange_directory, int repetitions = 1000)
{
	var client = new WatchModeClient(
		ExchangeDirectory: exchange_directory,
		InputType: InputType.JSON,
		OutputType: OutputType.ESLint ).Dump();

	client.ValidatorCommandLine().Dump();
	Console.WriteLine();

	var sw = Stopwatch.StartNew();

	for (var i = 0; i < repetitions; ++i)
	{
		var text = WatchModeClient.PATIENT_42_JSON.Replace("42", i.ToString());
		var start_time = DateTime.Now;
		var result = client.Validate(text);
		var a = result.Text.Split(new [] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		var ok = a.Length == 1
			&& a[0].TrimEnd().EndsWith(": line -1, col-1, Information - All OK");

		Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffff} (#{1:D5}) {2,6:F2} +{3,6:F2} ms  {4}",
			start_time, i, result.PrepTimeMS, result.WaitTimeMS, ok ? "" : "WTF?!" );

		if (!ok)
		{
			result.Dump();
			throw new Exception("unexpected result received");
		}
	}

	var ms = sw.Elapsed.TotalMilliseconds;

	Console.WriteLine();
	Console.WriteLine("{0:F0} ms for {1} validation(s) -> {2:F0}/s", 
		ms, repetitions, repetitions * 1e3 / ms );
}

//-----------------------------------------------------------------------------------------------------------

static void validate_JSON_in_cached_package_ (string exchange_directory, string package_name)
{
	var client = new WatchModeClient(
		ExchangeDirectory: exchange_directory,
		InputType: InputType.JSON,
		OutputType: OutputType.ESLint ).Dump();

	client.ValidatorCommandLine().Dump();
	Console.WriteLine();

	var source_directory = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
		".fhir",
		"packages",
		package_name,
		"package" );
	var source_filenames = Directory.GetFiles(source_directory, "*.json")
		.Where(f => !is_JSON_file_known_not_to_contain_FHIR_(Path.GetFileName(f)))
		.ToList();

	Console.WriteLine("validating {0} JSON file(s) in {1} ...", source_filenames.Count, source_directory);
	Console.WriteLine();

	var problematic_results = new List<WatchModeClient.ValidationResult>();
	var sw = Stopwatch.StartNew();
	var i = 0;

	foreach (var source_filename in source_filenames/*/.Take(50)/**/)
	{
		var text = File.ReadAllText(source_filename);
		var start_time = DateTime.Now;
		var result = client.Validate(text, Path.GetFileName(source_filename));
		var messages = ESLintMessage.Parse(result.Text, client.InputFilename, Path.GetFileName(source_filename));
		var ok = messages.Length == 1 && messages[0].IsAllOK();
		var f_e_w_i = "----";

		if (!ok)
		{
			f_e_w_i = 
				(messages.Any(m => m.Severity == Severity.Fatal      ) ? "F" : "-") +
				(messages.Any(m => m.Severity == Severity.Error      ) ? "E" : "-") +
				(messages.Any(m => m.Severity == Severity.Warning    ) ? "w" : "-") +
				(messages.Any(m => m.Severity == Severity.Information) ? "i" : "-");

			problematic_results.Add(result);
		}

		Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffff} (#{1:D5}) {2,6:F2} +{3,5:F0} ms {4,4} {5}  {6}",
			start_time, i++, result.PrepTimeMS, result.WaitTimeMS, 
			ok ? "-" : messages.Length.ToString(),
			f_e_w_i,
			Path.GetFileName(source_filename) );
	}

	var ms = sw.Elapsed.TotalMilliseconds;

	Console.WriteLine();
	Console.WriteLine("{0:F0} ms for {1} validation(s) -> {2:F0}/s", 
		ms, source_filenames.Count, source_filenames.Count * 1e3 / ms );

	if (problematic_results.Count > 0)
	{
		Console.WriteLine();
		Console.WriteLine("{0} problematic results:", problematic_results.Count);
		problematic_results.Dump();
	}
}

//-----------------------------------------------------------------------------------------------------------

static bool is_JSON_file_known_not_to_contain_FHIR_ (string filename_without_path)
{
	switch (filename_without_path.ToLowerInvariant())
	{
		// standard FHIR package meta data:
		case ".index.json":
		case "package.json":
		// mouse droppings left by Firely:
		case ".inflator.json":
		case "fhirpkg.lock.json": return true;
	}

	return false;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////
///// WatchModeClient ////////////////////////////////////////////////////////////////////// 2023-06-28 /////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////

public enum InputType { JSON, XML  };

/** supported validator output formats (all those which consist of a single file) */
public enum OutputType { JSON, XML, CSV, Compact, ESLint };

class WatchModeClient
{
	/** a name like 'PlainCoreR4' or 'ERezept2023Q3' that stands for a validator running in watch mode with
	 * a certain configuration (like a FHIR version and a set of mutually compatible profile packages); this
	 * is used for naming the files over which this client communicates with the watch-mode validator
	 * <br/><br/>
	 *
	 * NB: it is better to treat the input file type - i.e. XML or JSON - as part of the configuration. This
	 * is much easier to set up and manage than a channel that can handle multiple file types. To keep things
	 * simple, this client will append '-JSON' to the configuration name if InputType.JSON is specified. */
	public readonly string ConfigurationName;

	/** the directory where the communication with the validator takes place (ideally a RAM disk) */
	public readonly string ExchangeDirectory;

	public readonly InputType InputType;
	public readonly OutputType OutputType;

	public readonly string InputFilename;
	public readonly string OutputFilename;

	// these used to be necessary for the protocol variant involving a trigger file;
	// now they're only here for convenience
	public const string PATIENT_42_JSON = "{\"resourceType\":\"Patient\",\"id\":\"42\"}";
	public const string PATIENT_42_XML = "<Patient xmlns='http://hl7.org/fhir'><id value='42'/></Patient>";

	// generally using `internal` instead of `private` here in order to facilitate experimentation
	
	/** file timestamps need to differ by at least one millisecond in order for Java's `File.lastModified()`
	 * to register a change */
	internal const int MIN_TIMESTAMP_DELTA_TICKS = 10000;

	/** parameter names are spelt like the property names, to simplify the use of named parameters<br/><br/>
	 *
	 * If `ExchangeDirectory` is null then the equivalent of '%TEMP%/WatchModeServer' will be used. 
	 */
	public WatchModeClient (
		string ConfigurationName = "PlainCoreR4",  // configuration for R4 without additional profile packages
		string ExchangeDirectory = null,
		InputType InputType = InputType.JSON,
		OutputType OutputType = OutputType.ESLint )
	{
		if (ExchangeDirectory == null)
			ExchangeDirectory = Path.Combine(Path.GetTempPath(), "WatchModeServer");

		this.ConfigurationName = InputType == InputType.JSON
			? ConfigurationName + "-JSON"
			: ConfigurationName;
		this.ExchangeDirectory = Path.GetFullPath(ExchangeDirectory);
		this.InputType = InputType;
		this.OutputType = OutputType;

		Directory.CreateDirectory(this.ExchangeDirectory);
		Trace.Assert(Directory.Exists(this.ExchangeDirectory), $"ExchangeDirectory '{this.ExchangeDirectory}'");

		var common_prefix = Path.Combine(this.ExchangeDirectory, this.ConfigurationName);

		InputFilename = $"{common_prefix}-input.{file_extension_for_(InputType)}";
		OutputFilename = $"{common_prefix}-output.{parameter_for_(OutputType)}";  
	}

	/** shows what the validator_cli command line should look like for serving this client */
	public string ValidatorCommandLine ()
	{
		var dot = InputFilename.LastIndexOf('.');
		// the validator's processing path for directly specified filenames has an uncanny ability for sniffing 
		// out files that disappear for a microsecond, and it likes to explode when it does; the wildcard processing
		// path is somewhat less prone to problems with disappearing files
		var wildcarded_input_filename = InputFilename.Substring(0, dot) + "*" + InputFilename.Substring(dot);
		var sb = new StringBuilder("java -server -Dfile.encoding=UTF-8 -Duser.language=en-US -jar validator_cli-6.0.17.jar");	

		sb.AppendLine(" ^").Append("\t-version 4.0.1");  // match this to your core version
		sb.AppendLine(" ^").Append("\t\"").Append(wildcarded_input_filename).Append('"');
		sb.AppendLine(" ^").Append("\t-watch-mode all");
		// 1 for avoiding rare mysterious problems with output file deletion when validating in a tight loop
		sb.AppendLine(" ^").Append("\t-watch-scan-delay 1");
		// 1 for avoiding rare sharing violations ('cannot access the file because it is being used by another process')
		sb.AppendLine(" ^").Append("\t-watch-settle-time 1");
		sb.AppendLine(" ^").Append("\t-output-style ").Append(parameter_for_(OutputType));
		sb.AppendLine(" ^").Append("\t-output \"").Append(OutputFilename).Append('"');

		return sb.ToString();
	}

	/** the output text produced by the validator, plus some timings */
	public class ValidationResult
	{
		/** configured validator output file format (eslint-compact supported by companion class ESLintMessage) */
		public readonly OutputType Format;
		/** the actual validator output, as read back from the deaddrop file */
		public readonly string Text;
		/** time spent preparing for the validation (writing the input file and so on) */
		public double PrepTimeMS => (m_T1 - m_T0) * 1e3 / Stopwatch.Frequency;
		/** time spend waiting for the validator output to become available */
		public double WaitTimeMS => (m_T2 - m_T1) * 1e3 / Stopwatch.Frequency;

		internal readonly long m_T0;
		internal readonly long m_T1;
		internal readonly long m_T2;

		public ValidationResult (OutputType format, string text, long t0, long t1, long t2)
		{
			Format = format;
			Text = text;
			m_T0 = t0;
			m_T1 = t1;
			m_T2 = t2;
		}
	}

	internal static Encoding s_UTF8_without_BOM_ = new UTF8Encoding(/*BOM*/ false, /*validate*/ true);

	/** validates the given resource; the filename, if provided, is used to substitute occurrences of
	 * the input deaddrop filename (i.e. this.InputFilename) in the validator output */
	public ValidationResult Validate (string resource_to_validate, string filename = null)
	{
		var bytes = s_UTF8_without_BOM_.GetBytes(resource_to_validate);

		return Validate(bytes, filename);
	}

	/** validates the given resource; the filename, if provided, is used to substitute occurrences of
	 * the input deaddrop filename (i.e. this.InputFilename) in the validator output */
	public ValidationResult Validate (byte[] resource_to_validate, string filename = null)
	{
		var t0 = Stopwatch.GetTimestamp();

		delete_with_retry_(OutputFilename);

		for (var i = 0; File.Exists(OutputFilename); )
		{
			Console.WriteLine("{0:yyyy-MM-dd HH:mm:ss.ffff} could not delete {1}; retrying ...", DateTime.Now, OutputFilename);
			Thread.Sleep(0);
			delete_with_retry_(OutputFilename);
		}

		// We need to set up the file change notification *before* we trigger the validation.
		// Otherwise there would be a possible race condition whereby the output file appears
		// before the watcher is set up and registering events. If that happens and there is no
		// noticeable change in watchable attributes (e.g. on a filesystem like FAT with low 
		// timestamp resolution) then the watcher waits forever.
	 	using (var watcher = new FileSystemWatcher())
		{
			var output_file_sighted = false;

			// FileSystemWatcher does not buffer events, so creating it early does not really change
			// anything as far as `WaitForChanged()` is concerned; hence we need to hook events
			watcher.Path = Path.GetDirectoryName(OutputFilename);
			watcher.Filter = Path.GetFileName(OutputFilename);
			// of no use here: CreationTime DirectoryName LastAccess Security
	  		watcher.NotifyFilter = 
				NotifyFilters.Attributes | 
				NotifyFilters.FileName | 
				NotifyFilters.LastWrite |
				NotifyFilters.Size;
			FileSystemEventHandler record_sighting = (o,e) => output_file_sighted = true;
			watcher.Created += record_sighting;
			watcher.Changed += record_sighting;
			watcher.EnableRaisingEvents = true;

			create_or_replace_outgoing_deaddrop_(InputFilename, resource_to_validate);

			var t1 = Stopwatch.GetTimestamp();

			if (!output_file_sighted)
			{
				watcher.WaitForChanged(WatcherChangeTypes.All);
			}

			watcher.EnableRaisingEvents = false;

			var output_text = read_file_when_closed_by_writer_(OutputFilename);

			if (filename != null)
			{
				var filename_to_translate =
					InputFilename.Substring(0, 1).ToLowerInvariant() +
					InputFilename.Substring(1);

				output_text = output_text.Replace(filename_to_translate, filename);
			}

			return new ValidationResult(OutputType, output_text, t0, t1, Stopwatch.GetTimestamp());
		}
	}

	internal static void delete_with_retry_ (string filename)
	{
		try
		{
			File.Delete(filename);  // no-op if the file does not exist
		}
		catch (IOException iox) when (iox.HResult == unchecked((int) 0x80070020))
		{
			// it can't be the validator who is keeping the file open (probably that MS Defender junk)
			Console.WriteLine(
				"{0:yyyy-MM-dd HH:mm:ss.ffff} {1} when deleting the output file; retrying",
				DateTime.Now,
				iox.GetType().Name );
			File.Delete(filename);
		}		
	}

	/** returns the appropriate file extension for the given <tt>InputType</tt> */
	internal static string file_extension_for_ (InputType input_type)
	{
		return input_type.ToString().ToLowerInvariant();
	}

	/** the text expected by the `-output-style` parameter; we also use this as the output file extension */
	internal static string parameter_for_ (OutputType output_type)
	{
		return output_type == OutputType.ESLint
			? "eslint-compact"
			: output_type.ToString().ToLowerInvariant();
	}

	/** creates the file if it does not exist; otherwise it writes the new file contents and 
	 * ensures that its new timestamp has it at least a millisecond younger than it was before
	 */
	internal static void create_or_replace_outgoing_deaddrop_ (string filename, byte[] bytes_to_write)
	{		
		// will be DateTime.MinValue if the file does not exist yet
		var old_timestamp = File.GetLastWriteTimeUtc(filename);

		using (var fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))	
		{
			fs.Write(bytes_to_write, 0, bytes_to_write.Length);
			fs.SetLength(fs.Position);
		}

		// ensure that Java actually sees a change in the last-write timestamp
		var new_timestamp = File.GetLastWriteTimeUtc(filename);
		var difference_in_ticks = (new_timestamp - old_timestamp).Ticks;

		if (difference_in_ticks < MIN_TIMESTAMP_DELTA_TICKS)
		{
			File.SetLastWriteTimeUtc(filename, old_timestamp.AddTicks(MIN_TIMESTAMP_DELTA_TICKS));
		}
	}

	/** while Java has its `FileOutputStream` still open, other processes cannot open the file with a share
	 * mode that denies writing, and this is precisely what `File.ReadAllText()` does; hence we only have 
	 * to wait until the exceptions stop flying
	 */
	internal static string read_file_when_closed_by_writer_ (string file_to_read)
	{
		while (true)
		{
			try
			{
				return File.ReadAllText(file_to_read);
			}
			catch (IOException iox) when (iox.HResult == unchecked((int) 0x80070020))
			{
				Thread.Sleep(0);
			}
		}
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////
///// ESLintMessage //////////////////////////////////////////////////////////////////////// 2023-06-28 /////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////

enum Severity { Information, Warning, Error, Fatal };

/** the information contained in a line of the eslint-compact validator output format */
class ESLintMessage
{
	public readonly string   Filename;
	public readonly int      Line;
	public readonly int      Column;
	public readonly Severity Severity;
	public readonly string   Message;

	public ESLintMessage (string message, string filename_to_match = null, string filename_to_substitute = null)
	{
		var match = ESLINT_REGEX.Match(message);

		if (!match.Success)
		{
			throw new FormatException("not an eslint message line: " + message);
		}

		Filename = SubstituteMatchingFilename(match.Groups[1].Value, filename_to_match, filename_to_substitute);
		Line     = int.Parse(match.Groups[2].Value);
		Column   = int.Parse(match.Groups[3].Value);
		Severity = (Severity) Enum.Parse(typeof(Severity), match.Groups[4].Value);  // not generic in Fwk
		Message  = match.Groups[5].Value.Trim();
	}

	public bool IsAllOK ()
	{
		if (Line == -1 && Column == -1 && Severity == Severity.Information)
		{
			switch (Message)
			{
				case "Alles OK":
				case "All OK": return true;
			}
		}

		return false;
	}

	public const string ESLINT_PATTERN = 
		"^(.*): line (-?[1-9][0-9]*), col(-?[1-9][0-9]*), (Information|Warning|Error|Fatal) - (.*)$";
	public static readonly Regex ESLINT_REGEX = new Regex(ESLINT_PATTERN, RegexOptions.Compiled);

	/** returns `filename` unchanged unless it equals `filename_to_match` in a case-insensitive manner,
	 * in which case `filename_to_substitute` is returned instead
	 */
	public static string SubstituteMatchingFilename (
		string filename,
		string filename_to_match, 
		string filename_to_substitute )
	{
		if (filename_to_match != null)
			if (string.Compare(filename, filename_to_match, StringComparison.InvariantCultureIgnoreCase) == 0)
				return filename_to_substitute;

		return filename;
	}

	private static readonly char[] ASCII_LF = { '\n' };

	/** parses a multi-line validator output file in eslint-compact format; if `filename_to_match` is not null
	 * then all of its occurrences in the result are be replaced with `filename_to_substitute`
	 */
	public static ESLintMessage[] Parse (
		string multiline_eslint_text,
		string filename_to_match = null,
		string filename_to_substitute = null )
	{
		var lines = multiline_eslint_text.Split(ASCII_LF, StringSplitOptions.RemoveEmptyEntries);
		var messages = new ESLintMessage[lines.Length];

		for (var i = 0; i < lines.Length; ++i)
		{
			messages[i] = new ESLintMessage(lines[i], filename_to_match, filename_to_substitute);
		}

		return messages;
	}
}