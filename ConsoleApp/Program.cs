using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDiffStatLib;
using System.IO;
using NDiffStatLib.Utils;
using System.Threading;

namespace ConsoleApp
{
	class Program
	{
		static int Main( string[] args )
		{
			DiffStatOptions options = new DiffStatOptions();
			string fileName = null;
			int i=-1;
			while (++i < args.Length) {
				if (args[i].In("/?", "--help")) {
					DisplayUsage();
					return 0;
				} 
				if (args[i].In("-m", "/m")) options.merge_opt = true;
				else if (args[i].In("-f", "/f")) {
					options.format_opt = (FormatOption)int.Parse(args[i+1]);
					++i;
				} else if (args[i].Length == 2 && args[i][0] == '-') {
					DisplayError("option " + args[i] + " not recognized");
					return 1;
				} else if (fileName == null) {
					fileName = args[i];
				}
			}
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
						using (StreamReader sr = new StreamReader(fs, Encoding.Default)) {
							DiffStat diffStat = new DiffStat(sr, options);
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
					DiffStat diffStat = new DiffStat(Console.In, options);
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
			string usage = @"Usage : ndiffstat [-m] [-f 4] [DIFF_FILE]";
			Console.WriteLine(usage);
		}

		public static void DisplayError( string errorMsg )
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Error.WriteLine(errorMsg);
			Console.ResetColor();
		}
	}
}
