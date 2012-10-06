using NDiffStatLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace TestProject
{
    
    
    /// <summary>
    ///Classe de test pour DiffStatTest, destinée à contenir tous
    ///les tests unitaires DiffStatTest
    ///</summary>
	[TestClass()]
	public class DiffStatTest
	{

		const string CRLF = "\r\n"; 
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
		///Test pour ParseDiff
		///</summary>
		[TestMethod()]
		public void ParseDiffAddedFileTest()
		{
			// Svn diff format
			StringBuilder sb = new StringBuilder()
			.AppendLine("Index: bidon.txt")
			.AppendLine("===================================================================")
			.AppendLine("--- bidon.txt	(revision 0)")
			.AppendLine("+++ bidon.txt	(revision 23)")
			.AppendLine("@@ -0,0 +1,5 @@")
			.AppendLine("+ceci")
			.AppendLine("+est")
			.AppendLine("+  un")
			.AppendLine("+ fichier")
			.AppendLine("+bidon")
			.AppendLine("\\ No newline at end of file");

			using (StringReader sr = new StringReader(sb.ToString())) {
				DiffStat diffStat = new DiffStat(new DiffStatOptions() { merge_opt = false });
				diffStat.ParseDiff(sr);
				Assert.AreEqual(5, diffStat.total_adds);
				Assert.AreEqual(0, diffStat.total_removes);
			}

			// Mercurial diff format
			sb.Clear()
			.Append("diff -r 000000000000 -r b05c69b71d52 bidon.txt\n")
			.Append("--- /dev/null	Thu Jan 01 00:00:00 1970 +0000\n")
			.Append("+++ b/bidon.txt	Mon Sep 24 14:32:07 2012 +0200\n")
			.Append("@@ -0,0 +1,5 @@\n")
			.Append("+ceci" + CRLF)
			.Append("+est" + CRLF)
			.Append("+  un" + CRLF)
			.Append("+ fichier" + CRLF)
			.Append("+bidon\n")
			.Append("\\ No newline at end of file\n");

			using (StringReader sr = new StringReader(sb.ToString())) {
				DiffStat diffStat = new DiffStat(new DiffStatOptions() { merge_opt = false });
				diffStat.ParseDiff(sr);
				Assert.AreEqual(5, diffStat.total_adds);
				Assert.AreEqual(0, diffStat.total_removes);
			}
			
		}

		/// <summary>
		///Test pour ParseDiff
		///</summary>
		[TestMethod()]
		public void ParseDiffModifiedFileTest()
		{
			// SVN diff format
			StringBuilder sb = new StringBuilder()
			.AppendLine("Index: bidon.txt")
			.AppendLine("===================================================================")
			.AppendLine("--- bidon.txt	(revision 23)")
			.AppendLine("+++ bidon.txt	(revision 24)")
			.AppendLine("@@ -1,5 +1,5 @@")
			.AppendLine("+blah...")
			.AppendLine(" ceci")
			.AppendLine("-est")
			.AppendLine("   un")
			.AppendLine("  fichier")
			.AppendLine("-bidon")
			.AppendLine("\\ No newline at end of file")
			.AppendLine("+bidon!")
			.AppendLine("\\ No newline at end of file");

			using (StringReader sr = new StringReader(sb.ToString())) {
				DiffStat diffStat = new DiffStat(new DiffStatOptions() { merge_opt = false });
				diffStat.ParseDiff(sr);
				ParseDiffModified_CheckStats(diffStat);
			}

			using (StringReader sr = new StringReader(sb.ToString())) {
				DiffStat diffStat = new DiffStat(new DiffStatOptions() { merge_opt = true });
				diffStat.ParseDiff(sr);
				ParseDiffModified_CheckStats(diffStat);

			}

			// Mercurial diff format
			sb.Clear()
			.Append("diff -r b05c69b71d52 -r 17b4b214771e bidon.txt\n")
			.Append("--- a/bidon.txt	Mon Sep 24 14:32:07 2012 +0200\n")
			.Append("+++ b/bidon.txt	Mon Sep 24 14:35:12 2012 +0200\n")
			.Append("@@ -1,5 +1,5 @@\n")
			.Append("+blah..." + CRLF)
			.Append(" ceci" + CRLF)
			.Append("-est" + CRLF)
			.Append("   un" + CRLF)
			.Append("  fichier" + CRLF)
			.Append("-bidon\n")
			.Append("\\ No newline at end of file\n")
			.Append("+bidon!\n")
			.Append("\\ No newline at end of file\n");

			using (StringReader sr = new StringReader(sb.ToString())) {
				DiffStat diffStat = new DiffStat(new DiffStatOptions() { merge_opt = false });
				diffStat.ParseDiff(sr);
				ParseDiffModified_CheckStats(diffStat);
			}

			using (StringReader sr = new StringReader(sb.ToString())) {
				DiffStat diffStat = new DiffStat(new DiffStatOptions() { merge_opt = true });
				diffStat.ParseDiff(sr);
				ParseDiffModified_CheckStats(diffStat);
			}
		}

		private void ParseDiffModified_CheckStats( DiffStat diffStat )
		{
			if (diffStat.options.merge_opt) {
				Assert.AreEqual(1, diffStat.total_adds);
				Assert.AreEqual(1, diffStat.total_removes);
				Assert.AreEqual(1, diffStat.total_modifs);
			} else {
				Assert.AreEqual(2, diffStat.total_adds);
				Assert.AreEqual(2, diffStat.total_removes);
			}

		}


		/// <summary>
		///Test pour ParseDiff
		///</summary>
		[TestMethod()]
		public void ParseSvnDiffDeletedFileTest()
		{
			// Svn diff format
			StringBuilder sb = new StringBuilder()
			.AppendLine("Index: bidon.txt")
			.AppendLine("===================================================================")
			.AppendLine("--- bidon.txt	(revision 24)")
			.AppendLine("+++ bidon.txt	(revision 25)")
			.AppendLine("@@ -1,5 +0,0 @@")
			.AppendLine("-blah...")
			.AppendLine("-ceci")
			.AppendLine("-  un")
			.AppendLine("- fichier")
			.AppendLine("-bidon!")
			.AppendLine("\\ No newline at end of file");

			using (StringReader sr = new StringReader(sb.ToString())) {
				DiffStat diffStat = new DiffStat(new DiffStatOptions() { merge_opt = false });
				diffStat.ParseDiff(sr);
				Assert.AreEqual(0, diffStat.total_adds);
				Assert.AreEqual(5, diffStat.total_removes);
			}

			// Mercurial diff format
			sb.Clear()
			.Append("diff -r 17b4b214771e -r b765c60314f5 bidon.txt\n")
			.Append("--- a/bidon.txt	Mon Sep 24 14:35:12 2012 +0200\n")
			.Append("+++ /dev/null	Thu Jan 01 00:00:00 1970 +0000\n")
			.Append("@@ -1,5 +0,0 @@\n")
			.Append("-blah..." + CRLF)
			.Append("-ceci" + CRLF)
			.Append("-  un" + CRLF)
			.Append("- fichier" + CRLF)
			.Append("-bidon!\n")
			.Append("\\ No newline at end of file\n");

			using (StringReader sr = new StringReader(sb.ToString())) {
				DiffStat diffStat = new DiffStat(new DiffStatOptions() { merge_opt = false });
				diffStat.ParseDiff(sr);
				Assert.AreEqual(0, diffStat.total_adds);
				Assert.AreEqual(5, diffStat.total_removes);
			}
		}
	}
}
