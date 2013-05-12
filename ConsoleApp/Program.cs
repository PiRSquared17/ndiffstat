using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDiffStatLib;
using System.IO;
using NDiffStatLib.Utils;
using System.Threading;
using Mono.Options;

namespace ConsoleApp
{
	class Program
	{
		static int Main( string[] args )
		{		
			DiffStatOptions options = new DiffStatOptions();
			bool show_help = false;
			OptionSet opts = new OptionSet() {
				{ "m|merge-opt", "true if we merge ins/del as modified", 
								  v => options.merge_opt = (v != null) },
				{ "f|format-opt=", "text output formatting options\r\n"
								+ "1=normal - displays only total changes per file\r\n"
								+ "4=all values - display added/removed/modified lines counts per file",
								  new Action<int>(v => options.format_opt = (FormatOption)v) },
				{ "?|help", "show this message and exit", 
						     v => show_help = v != null } 
			};
			List<string> extra;
			try {
				extra = opts.Parse(args);
			} catch (OptionException e) {
				DisplayError(e.Message);
				return 1;
			}

			foreach (string s in extra.Where(s => s.Length > 0)) {
				if (s[0].In('-', '/')) {
					DisplayError("Switch " + s + " not recognized");
					DisplayUsage();
					return 1;
				}
			}

			if (show_help) {
				ShowHelp(opts);
				return 0;
			}

			// assume the first non-option arg is the diff fileName
			string fileName = extra.FirstOrDefault();

			if (fileName != null) {
				FileInfo file;
				if (File.Exists(fileName)) {
					file = new FileInfo(fileName);
				} else if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName))) {
					file = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
				} else {
					DisplayError("file " + fileName + " does not exists");
					DisplayUsage();
					return 1;
				}
				try {
					using (FileStream fs = FileUtils.GetReadonlyStream(file.FullName)) {
						using (StreamReader sr = new StreamReader(fs, Encoding.GetEncoding(1252), true)) {
							DiffStat diffStat = new DiffStat(options);
							diffStat.ParseDiff(sr);
							Console.WriteLine(diffStat.ToString());
							return 0;
						}
					}
				} catch (EmptyDiffException ex) {
					DisplayError(ex.Message);
					return 1;
				}
			} else {
				// read diff from stdin
				bool readFromStdIn = false;
				try {
					bool key = Console.KeyAvailable;
				} catch (InvalidOperationException) {
					// Console.KeyAvailable raise InvalidOperationException
					// when console input has been redirected from a file.
					// We use this mechanism to detect a redirect from file
					// http://stackoverflow.com/questions/3961542/checking-standard-input-in-c-sharp
					readFromStdIn = true;
				}
				if (readFromStdIn) {
					DiffStat diffStat = new DiffStat(options);
					diffStat.ParseDiff(Console.In);
					Console.WriteLine(diffStat.ToString());
					return 0;
				} else {
					// the program has been called with no arguments
					// and the console input has not been redirected from a file
					// --> let's tell the user he should supply a diff file
					DisplayError("A diff file must be specified");
					DisplayUsage();
					return 1;
				}
			}
		}

		public static void DisplayUsage()
		{
			string usage = @"Usage : ndiffstat [-m] [-f 4] DIFF_FILE";
			Console.WriteLine(usage);
			
		}

		public static void ShowHelp( OptionSet opts )
		{
			DisplayUsage();
			Console.WriteLine("Options:");
			opts.WriteOptionDescriptions(Console.Out);
		}

		public static void DisplayError( string errorMsg )
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine(errorMsg);
			Console.ResetColor();
		}
	}
}
