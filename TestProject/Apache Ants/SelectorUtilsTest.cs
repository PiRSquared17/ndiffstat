using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDiffStatLib.ApacheAnt;
using NDiffStatLib.Utils;

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
