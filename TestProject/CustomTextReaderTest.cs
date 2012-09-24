using NDiffStatLib.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Diagnostics;
using NDiffStatLib.DiffParsers;

namespace TestProject
{
    /// <summary>
    ///Classe de test pour CustomTextReaderTest, destinée à contenir tous
    ///les tests unitaires CustomTextReaderTest
    ///</summary>
	[TestClass()]
	public class CustomTextReaderTest
	{
		private TestContext testContextInstance;
		private const string CRLF = "\r\n";

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
		///Test pour Constructeur CustomTextReader
		///</summary>
		[TestMethod()]
		public void CustomTextReaderConstructorTest()
		{
			string text = "ceci" + CRLF + "est" + CRLF + "une" + CRLF + "chaîne";

			CustomTextReader reader = new CustomTextReader(new StringReader(text));
			
			// Start position
			// (PreviousLine, CurrentLine, NextLine) = (null, null, ceci)
			Assert.AreEqual(0, reader.CurrentLineIndex);
			Assert.IsNull(reader.PreviousLine);
			Assert.IsNull(reader.CurrentLine);
			Assert.AreEqual("ceci", reader.NextLine);

			// moving forward
			bool foward = reader.MoveFoward();
			Assert.IsTrue(foward);

			// First forward step
			// (PreviousLine, CurrentLine, NextLine) = (null, ceci, est)
			Assert.IsNull(reader.PreviousLine);
			Assert.AreEqual("ceci", reader.CurrentLine);
			Assert.AreEqual("est", reader.NextLine);

			// We make two step forward
			reader.MoveFoward();
			reader.MoveFoward();
			
			// the values should be
			// (PreviousLine, CurrentLine, NextLine) = (est, une, chaîne)
			Assert.AreEqual("est", reader.PreviousLine);
			Assert.AreEqual("une", reader.CurrentLine);
			Assert.AreEqual("chaîne", reader.NextLine);

			// one more step foward
			foward = reader.MoveFoward();
			Assert.IsTrue(foward);

			// NextLine should be null (as "chaîne" is the last line)
			// (PreviousLine, CurrentLine, NextLine) = (une, chaîne, null)
			Assert.AreEqual("une", reader.PreviousLine);
			Assert.AreEqual("chaîne", reader.CurrentLine);
			Assert.IsNull(reader.NextLine);
			// currentLine is the 4th line 
			Assert.AreEqual(4, reader.CurrentLineIndex); 

			// try one more step foward
			foward = reader.MoveFoward();
			// MoveFoward should return false as NextLine = null (there's no more lines to read)
			Assert.IsFalse(foward);

			// (PreviousLine, CurrentLine, NextLine) = (une, chaîne, null)
			// (same as before)
			Assert.AreEqual("une", reader.PreviousLine);
			Assert.AreEqual("chaîne", reader.CurrentLine);
			Assert.IsNull(reader.NextLine);
			// currentLine is always the 4th line 
			Assert.AreEqual(4, reader.CurrentLineIndex);
		}
	}
}
