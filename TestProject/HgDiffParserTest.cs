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
			// Testing HgTool with a patch that creates a new file
			Assert.AreEqual(
				new Revision(Revision.PRE_CREATION),
				SvnDiffParser.parse_diff_revision("/dev/null", "bf544ea505f8").revision
			);
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
				SvnDiffParser parser = new SvnDiffParser(sr);
				List<FileDiff> fileDiffs = parser.parse();
				Assert.AreEqual(1, fileDiffs.Count);
				Assert.AreEqual("readme", fileDiffs[0].origFile
				);
			}

		}
	}
}
