using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NDiffStatLib.DiffParsers
{

	/// <summary>
	/// TextReader "customisé" permettant de lire le contenu d'un TextReader 
	/// en gardant en mémoire les trois dernière lignes lues.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CustomTextReader
	{
		private readonly TextReader reader;
		/// <summary>
		/// file d'attente à trois éléments
		/// - lineBuffer[2] = dernière ligne lue
		/// - lineBuffer[1] = ligne précédente
		/// - lineBuffer[0] = ligne précédente++
		/// </summary>
		private Queue<string> linesBuffer;

		private int currentLineIndex;

		private string lastLineRead;

		public CustomTextReader( TextReader reader )
		{
			this.reader = reader;
			this.currentLineIndex = 0;
			this.linesBuffer = new Queue<string>(3);
			linesBuffer.Enqueue(null);
			linesBuffer.Enqueue(null);
			lastLineRead = reader.ReadLine();
			linesBuffer.Enqueue(lastLineRead);
		}


		/// <summary>
		/// Numéro de ligne correspondant à CurrentLine 
		/// (1 si CurrentLine est la 1ère ligne lue dans le TextReader)
		/// </summary>
		public int CurrentLineIndex
		{
			get
			{
				return currentLineIndex;
			}
		}


		public string PreviousLine
		{
			get { return linesBuffer.Peek(); }
		}

		public string CurrentLine
		{
			get { return linesBuffer.ElementAt(1); }
		}

		public string NextLine
		{
			get { return linesBuffer.ElementAt(2); }
		}

		/// <summary>
		/// Avance d'une ligne dans le texte et renvoie true si NextLine est non null
		/// Ne fait rien sinon et retourne false
		/// </summary>
		/// <returns></returns>
		public bool MoveFoward()
		{
			if (lastLineRead == null) {
				return false;
			}
			linesBuffer.Dequeue();
			lastLineRead = reader.ReadLine();
			linesBuffer.Enqueue(lastLineRead);
			currentLineIndex++;
			return true;
		}
	}
}

