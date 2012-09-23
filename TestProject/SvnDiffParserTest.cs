using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NDiffStatLib.DiffParsers;

namespace TestProject
{
    /// <summary>
    ///Classe de test pour SvnDiffParserTest, destinée à contenir tous
    ///les tests unitaires SvnDiffParserTest
    ///</summary>
	[TestClass()]
	public class SvnDiffParserTest
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
		public void parse_diff_revision_test()
		{
				Assert.AreEqual(
					new Revision(Revision.HEAD), 
					SvnDiffParser.parse_diff_revision("", "(working copy)").revision
				);
				//Testing revision number parsing
				Assert.AreEqual(
					new Revision(Revision.PRE_CREATION), 
					SvnDiffParser.parse_diff_revision("", "   (revision 0)").revision
				);
				Assert.AreEqual(
					1, 
					SvnDiffParser.parse_diff_revision("", "(revision 1)").revision.revNumber
				);
				Assert.AreEqual(
					23, 
					SvnDiffParser.parse_diff_revision("", "(revision 23)").revision.revNumber
				);
				//odds strins parsing tests
				Assert.AreEqual(
					4, 
					SvnDiffParser.parse_diff_revision("", "\t(revision 4)").revision.revNumber
				);
				Assert.AreEqual(
					10958, 
					SvnDiffParser.parse_diff_revision(
						"",
						"2007-06-06 15:32:23 UTC (rev 10958)"
					).revision.revNumber
				);
		}

		/// <summary>
		///Error handling pour parse_diff_revision
		///</summary>
	   [TestMethod()]
	   [ExpectedException(typeof(SvnDiffParserError))]
       public void parse_diff_revision_test_error() {
			SvnDiffParser.parse_diff_revision("", "bad stuff!");
		}

	   /// <summary>
	   ///Test pour parse_diff_revision
	   ///</summary>
	   [TestMethod()]
	   public void parse_diff_binary_diff_test()
	   {
		   // Testing parsing SVN diff with binary file
		   string diff = 
			     "Index: binfile\n===========================================" 
			   + "========================\nCannot display: file marked as a " 
			   + "binary type.\nsvn:mime-type = application/octet-stream\n";

		   using (StringReader sr = new StringReader(diff)) {
			   SvnDiffParser parser = new SvnDiffParser(sr);
			   List<FileDiff> fileDiffs = parser.parse();
			   Assert.AreEqual(1, fileDiffs.Count);
			   Assert.AreEqual("binfile", fileDiffs[0].origFile);
			   Assert.IsTrue(fileDiffs[0].binary);
		   }
	   }

	   /// <summary>
	   ///Test pour parse_diff_revision
	   ///</summary>
	   [TestMethod()]
	   public void parse_diff_keyword_diff_test()
	   {
		   // Testing parsing SVN diff with binary file
		   string diff = 
			  "Index: Makefile\n" 
			+ "===========================================================" 
			+ "========\n" 
			+ "--- Makefile    (revision 4)\n" 
			+ "+++ Makefile    (working copy)\n" 
			+ "@@ -1,6 +1,7 @@\n" 
			+ " # $Id$\n" 
			+ " # $Rev$\n" 
			+ " # $Revision::     $\n" 
			+ "+# foo\n" 
			+ " include ../tools/Makefile.base-vars\n" 
			+ " NAME = misc-docs\n" 
			+ " OUTNAME = svn-misc-docs\n";

		   using (StringReader sr = new StringReader(diff)) {
			   SvnDiffParser parser = new SvnDiffParser(sr);
			   List<FileDiff> fileDiffs = parser.parse();
			   Assert.AreEqual(1, fileDiffs.Count);
			   Assert.AreEqual("Makefile", fileDiffs[0].origFile);
			   Assert.AreEqual(4, SvnDiffParser.parse_diff_revision("", fileDiffs[0].origInfo).revision.revNumber);
			   Assert.AreEqual(
					new Revision(Revision.HEAD),
					SvnDiffParser.parse_diff_revision("", fileDiffs[0].newInfo).revision
			   );
		   }
	   }
	}
}
