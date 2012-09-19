using NDiffStatLib.DiffParsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace TestProject
{
    
    
    /// <summary>
    ///Classe de test pour HgDiffParserTest, destinée à contenir tous
    ///les tests unitaires HgDiffParserTest
    ///</summary>
	[TestClass()]
	public class HgDiffParserTest
	{


		private TestContext testContextInstance;

		/// <summary>
		///Obtient ou définit le contexte de test qui fournit
		///des informations sur la série de tests active ainsi que ses fonctionnalités.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Attributs de tests supplémentaires
		// 
		//Vous pouvez utiliser les attributs supplémentaires suivants lorsque vous écrivez vos tests :
		//
		//Utilisez ClassInitialize pour exécuter du code avant d'exécuter le premier test dans la classe
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Utilisez ClassCleanup pour exécuter du code après que tous les tests ont été exécutés dans une classe
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Utilisez TestInitialize pour exécuter du code avant d'exécuter chaque test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Utilisez TestCleanup pour exécuter du code après que chaque test a été exécuté
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///Test pour parse_diff_revision
		///</summary>
		[TestMethod()]
		public void parse_diff_revision_new_file_test()
		{
			// Testing HgDiffParser revision parsing with a patch that creates a new file
			Assert.AreEqual<Revision>(
				new Revision(Revision.PRE_CREATION),
				HgDiffParser.parse_diff_revision("/dev/null", "bf544ea505f8").revision
			);
		}

		/// <summary>
		///Test pour parse_diff_revision
		///</summary>
		[TestMethod()]
		public void parse_diff_revision()
		{
			// Testing HgDiffParser revision number parsing
			RevisionParseResult rev_result = HgDiffParser.parse_diff_revision("doc/readme", "bf544ea505f8");
			Assert.AreEqual("doc/readme", rev_result.file_str);
			Assert.AreEqual("bf544ea505f8", rev_result.revision.rev_str);

			rev_result = HgDiffParser.parse_diff_revision("/dev/null", "bf544ea505f8");
			Assert.AreEqual("/dev/null", rev_result.file_str);
			Assert.AreEqual(Revision.PRE_CREATION, rev_result.revision.rev_str);
		}

		/// <summary>
		///Test pour parse
		///</summary>
		[TestMethod()]
		public void parse_diff_new_file_test()
		{
			// Testing HgDiffParser with a diff that creates a new file
			string diffContents = "diff -r bf544ea505f8 readme\n" 
				+ "--- /dev/null\n" 
				+ "+++ b/readme\n";
			using (StringReader sr = new StringReader(diffContents)) {
				HgDiffParser parser = new HgDiffParser(sr);
				List<FileDiff> fileDiffs = parser.parse();
				Assert.AreEqual(1, fileDiffs.Count);
				Assert.AreEqual("readme", fileDiffs[0].origFile);
			}
		}

		/// <summary>
		///Test pour parse
		///</summary>
		[TestMethod()]
		public void parse_diff_uncommitted_test()
		{
			// Testing Testing HgDiffParser with a diff with an uncommitted change
			string diffContents = "diff -r bf544ea505f8 readme\n" 
				+ "--- a/readme\n" 
				+ "+++ b/readme\n";
			using (StringReader sr = new StringReader(diffContents)) {
				HgDiffParser parser = new HgDiffParser(sr);
				List<FileDiff> fileDiffs = parser.parse();
				Assert.AreEqual(1, fileDiffs.Count);
				Assert.AreEqual("bf544ea505f8", fileDiffs[0].origInfo);
				Assert.AreEqual("readme", fileDiffs[0].origFile);
				Assert.AreEqual("Uncommitted", fileDiffs[0].newInfo);
				Assert.AreEqual("readme", fileDiffs[0].newFile);
			}
		}
		
		/// <summary>
		///Test pour parse
		///</summary>
		[TestMethod()]
		public void parse_diff_committed_test()
		{
			// Testing HgDiffParser with a diff between committed revisions
			string diffContents = "diff -r 356a6127ef19 -r 4960455a8e88 readme\n" 
				+ "--- a/readme\n" 
				+ "+++ b/readme\n";
			using (StringReader sr = new StringReader(diffContents)) {
				HgDiffParser parser = new HgDiffParser(sr);
				List<FileDiff> fileDiffs = parser.parse();
				Assert.AreEqual(1, fileDiffs.Count);
				Assert.AreEqual("356a6127ef19", fileDiffs[0].origInfo);
				Assert.AreEqual("readme", fileDiffs[0].origFile);
				Assert.AreEqual("4960455a8e88", fileDiffs[0].newInfo);
				Assert.AreEqual("readme", fileDiffs[0].newFile);
			}
		}

		/// <summary>
		///Test pour parse
		///</summary>
		[TestMethod()]
		public void parse_diff_with_preamble_junk()
		{
			// Testing HgDiffParser with a diff that contains non-diff junk test as a preamble
			string diffContents = 
				   "changeset:   60:3613c58ad1d5\n"
				+  "user:        Michael Rowe <mrowe@mojain.com>\n"
				+  "date:        Fri Jul 27 11:44:37 2007 +1000\n"
				+  "files:       readme\n"
				+  "description:\n"
				+  "Update the readme file\n"
				+  "\n"
				+  "\n"
				+  "diff -r 356a6127ef19 -r 4960455a8e88 readme\n"
				+  "--- a/readme\n"
				+  "+++ b/readme\n";

			using (StringReader sr = new StringReader(diffContents)) {
				HgDiffParser parser = new HgDiffParser(sr);
				List<FileDiff> fileDiffs = parser.parse();
				Assert.AreEqual(1, fileDiffs.Count);
				Assert.AreEqual("356a6127ef19", fileDiffs[0].origInfo);
				Assert.AreEqual("readme", fileDiffs[0].origFile);
				Assert.AreEqual("4960455a8e88", fileDiffs[0].newInfo);
				Assert.AreEqual("readme", fileDiffs[0].newFile);
			}
		}

		/// <summary>
		///Test pour parse
		///</summary>
		[TestMethod()]
		public void parse_diff_git()
		{
			// Testing HgDiffParser git diff support
			string diffContents = 
				   "# Node ID 4960455a8e88\n"
				+  "# Parent bf544ea505f8\n"
				+  "diff --git a/path/to file/readme.txt "
				+  "b/new/path to/readme.txt\n"
				+  "--- a/path/to file/readme.txt\n"
				+  "+++ b/new/path to/readme.txt\n";

			using (StringReader sr = new StringReader(diffContents)) {
				HgDiffParser parser = new HgDiffParser(sr);
				List<FileDiff> fileDiffs = parser.parse();
				Assert.AreEqual(1, fileDiffs.Count);
				Assert.AreEqual("bf544ea505f8", fileDiffs[0].origInfo);
				Assert.AreEqual("path/to file/readme.txt", fileDiffs[0].origFile);
				Assert.AreEqual("4960455a8e88", fileDiffs[0].newInfo);
				Assert.AreEqual("new/path to/readme.txt", fileDiffs[0].newFile);
			}
		}
	}
}
