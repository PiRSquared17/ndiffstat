using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDiffStatLib;
using System.IO;
using NDiffStatLib.Utils;

namespace ConsoleApp
{
	class Program
	{
		static void Main( string[] args )
		{
			DiffStatOptions options = new DiffStatOptions();
			string fileName = null;
			int i=-1;
			while (++i < args.Length) {
				if (args[i].In("/?", "--help")) {
					DisplayUsage();
					return;
				} 
				if (args[i].In("-m", "/m")) options.merge_opt = true;
				else if (args[i].In("-f", "/f")) {
					options.format_opt = (FormatOption)int.Parse(args[i+1]);
					++i;
				} else if (args[i].Length == 2 && args[i][0] == '-') {
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.WriteLine("option " + args[i] + " not recognized");
					Console.ResetColor();
					Environment.Exit(1);
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
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.WriteLine("file " + fileName + " does not exists");
					Console.ResetColor();
					DisplayUsage();
					Environment.Exit(1);
					return;
				}
				using (FileStream fs = FileUtils.GetReadonlyStream(file.FullName)) {
					using (StreamReader sr = new StreamReader(fs, Encoding.Default)) {
						DiffStat diffStat = new DiffStat(sr, options);
						Console.WriteLine(diffStat.ToString());
					}
				}
			} else {
				// read diff from stdin
				using (Console.In) {
					DiffStat diffStat = new DiffStat(Console.In, options);
					Console.WriteLine(diffStat.ToString());
				}
			}
		}

		public static void DisplayUsage()
		{
			string usage = @"Usage : ndiffstat [-m] [-f 4] [DIFF_FILE]";
			Console.WriteLine(usage);

		}
	}
}
