using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDiffStatLib.ApacheAnt;
using NDiffStatLib.Utils;
using System.Linq;

namespace TestProject.Apache_Ants
{
	[TestClass]
	public class SelectorUtilsTest
	{
		SelectorUtils selectorUtils = SelectorUtils.getInstance();
		Counter counter = new Counter();
		
		[TestMethod]
		public void MatchPathTest()
		{
			Stopwatch chrono = new Stopwatch();
			chrono.Start();

			string pattern = @"*.java";
			string[] matchingStrings = new string[] { "Person.java" };
			string[] nonMatchingString = new string[] { "Person.class" };
			TestPattern(pattern, matchingStrings, nonMatchingString);

			pattern = @"Person*.java";
			matchingStrings = new string[] { "Person.java", "PersonA.java", "PersonBoss.java" };
			nonMatchingString = new string[] { "P.java", "BossPerson.java" };
			TestPattern(pattern, matchingStrings, nonMatchingString);

			pattern = @"Test?.java";
			matchingStrings = new string[] { "TestA.java" };
			nonMatchingString = new string[] { "Test.java", "TestOne.java" };
			TestPattern(pattern, matchingStrings, nonMatchingString);

			pattern = @"**/*.txt";
			matchingStrings = new string[] { "a.txt", "src/a.txt", "src/com/oreilly/b.txt" };
			nonMatchingString = new string[] { "a.pdf", "src/a.pdf" };
			TestPattern(pattern, matchingStrings, nonMatchingString);

			pattern = @"src/**/*.java";
			matchingStrings = new string[] { "src/A.java", "src/com/oreilly/File.java" };
			nonMatchingString = new string[] { "B.java", "src/com/oreilly/C.class" };
			TestPattern(pattern, matchingStrings, nonMatchingString);

			pattern = @"**/doc/**";
			matchingStrings = new string[] { "doc", "src/doc/File.txt" };
			nonMatchingString = new string[] { "src/bin/C.class" };
			TestPattern(pattern, matchingStrings, nonMatchingString);

			chrono.Stop();

			Trace.WriteLine(string.Format("{0} combinations of string / pattern tested in {1:#0.##} milliseconds", counter.Value, (double)chrono.ElapsedTicks / TimeSpan.TicksPerMillisecond));

		}

		[TestMethod]
		public void MatchPathTest2()
		{
			// test case from stackoverflow
			// http://stackoverflow.com/questions/69835/how-do-i-use-nant-ant-naming-patterns
			string[] patterns = new string[] { "*.c", "src/*.c", "*/*.c", "**/*.c", "bar.*", "**/bar.*", "**/bar*.*", "src/ba?.c" };
			string[] files = new string[] { "bar.txt", "src/bar.c", "src/baz.c", "src/test/bartest.c" };
			
			bool[][] matches = new bool[patterns.Length][];
			for (int i=0 ; i<patterns.Length ; i++) {
				matches[i] = new bool[files.Length];
				for (int j=0 ; j<files.Length ; j++) {
					matches[i][j] = selectorUtils.matchPath(patterns[i], files[j]);
				}
			}
			// Checking matching results
			// *.c			matches nothing (there are no .c files in the current directory)
			Assert.IsTrue(matches[0].All(b => !b));
			// src/*.c		matches 2 and 3
			Assert.IsFalse(matches[1][0]); // file n°1
			Assert.IsTrue(matches[1][1]); // file n°2
			Assert.IsTrue(matches[1][2]); // file n°3
			Assert.IsFalse(matches[1][3]); // file n°4
			// */*.c		matches 2 and 3 (because * only matches one level)
			Assert.IsFalse(matches[2][0]); 
			Assert.IsTrue(matches[2][1]);
			Assert.IsTrue(matches[2][2]); 
			Assert.IsFalse(matches[2][3]); 
			// **/*.c		matches 2, 3, and 4 (because ** matches any number of levels)
			Assert.IsFalse(matches[3][0]);
			Assert.IsTrue(matches[3][1]);
			Assert.IsTrue(matches[3][2]);
			Assert.IsTrue(matches[3][3]); 
			// bar.*		matches 1
			Assert.IsTrue(matches[4][0]);
			Assert.IsFalse(matches[4][1]);
			Assert.IsFalse(matches[4][2]);
			Assert.IsFalse(matches[4][3]); 
			// **/bar.*		matches 1 and 2
			Assert.IsTrue(matches[5][0]);
			Assert.IsTrue(matches[5][1]);
			Assert.IsFalse(matches[5][2]);
			Assert.IsFalse(matches[5][3]); 
			// **/bar*.*	matches 1, 2, and 4
			Assert.IsTrue(matches[6][0]);
			Assert.IsTrue(matches[6][1]);
			Assert.IsFalse(matches[6][2]);
			Assert.IsTrue(matches[6][3]); 
			// src/ba?.c	matches 2 and 3
  			Assert.IsFalse(matches[7][0]); 
			Assert.IsTrue(matches[7][1]);
			Assert.IsTrue(matches[7][2]); 
			Assert.IsFalse(matches[7][3]); 
		}

		private void TestPattern( string pattern, string[] matchingStrings, string[] nonMatchingString )
		{
			foreach (string s in matchingStrings) {
				bool result = selectorUtils.matchPath(pattern, s);
				this.counter.Increment();
				Assert.IsTrue(result);
			}
			foreach (string s in nonMatchingString) {
				bool result = selectorUtils.matchPath(pattern, s);
				this.counter.Increment();
				Assert.IsFalse(result);
			}
		}
	}
}
