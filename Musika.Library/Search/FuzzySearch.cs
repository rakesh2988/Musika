using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Musika.Library.Search
{
    /// <summary>
    /// Provides methods for fuzzy string searching.
    /// </summary>
    public static class FuzzySearch
    {
        /// <summary>
        /// Calculates the Levenshtein-distance of two strings.
        /// </summary>
        /// <param name="src">
        /// 1. string
        /// </param>
        /// <param name="dest">
        /// 2. string
        /// </param>
        /// <returns>
        /// Levenshstein-distance
        /// </returns>
        /// <remarks>
        /// See 
        /// <a href='http://en.wikipedia.org/wiki/Levenshtein_distance'>
        /// http://en.wikipedia.org/wiki/Levenshtein_distance
        /// </a>
        /// </remarks>
        public static int LevenshteinDistance(string src, string dest)
        {
            int[,] d = new int[src.Length + 1, dest.Length + 1];
            int i, j, cost;
            char[] str1 = src.ToCharArray();
            char[] str2 = dest.ToCharArray();

            for (i = 0; i <= str1.Length; i++)
            {
                d[i, 0] = i;
            }
            for (j = 0; j <= str2.Length; j++)
            {
                d[0, j] = j;
            }
            for (i = 1; i <= str1.Length; i++)
            {
                for (j = 1; j <= str2.Length; j++)
                {

                    if (str1[i - 1] == str2[j - 1])
                        cost = 0;
                    else
                        cost = 1;

                    d[i, j] =
                        Math.Min(
                            d[i - 1, j] + 1,					// Deletion
                            Math.Min(
                                d[i, j - 1] + 1,				// Insertion
                                d[i - 1, j - 1] + cost));		// Substitution

                    if ((i > 1) && (j > 1) && (str1[i - 1] == str2[j - 2]) && (str1[i - 2] == str2[j - 1]))
                    {
                        d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                    }
                }
            }

            return d[str1.Length, str2.Length];
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Fuzzy searches a list of strings.
        /// </summary>
        /// <param name="word">
        /// The word to find.
        /// </param>
        /// <param name="wordList">
        /// A list of word to be searched.
        /// </param>
        /// <param name="fuzzyness">
        /// Ration of the fuzzyness. A value of 0.8 means that the 
        /// difference between the word to find and the found words
        /// is less than 20%.
        /// </param>
        /// <returns>
        /// The list with the found words.
        /// </returns>
        /// <example>
        /// 
        /// </example>
        public static List<string> Search(
            string word,
            List<string> wordList,
            double fuzzyness)
        {
#if !LINQ
            List<string> foundWords = new List<string>();

            foreach (string s in wordList)
            {
                // Calculate the Levenshtein-distance:
                int levenshteinDistance =
                    LevenshteinDistance(word, s);

                // Length of the longer string:
                int length = Math.Max(word.Length, s.Length);

                // Calculate the score:
                double score = 1.0 - (double)levenshteinDistance / length;

                // Match?
                if (score > fuzzyness)
                    foundWords.Add(s);
            }
#else
			// Tests have prove that the !LINQ-variant is about 3 times
			// faster!
			List<string> foundWords =
				(
					from s in wordList
					let levenshteinDistance = LevenshteinDistance(word, s)
					let length = Math.Max(s.Length, word.Length)
					let score = 1.0 - (double)levenshteinDistance / length
					where score > fuzzyness
					select s
				).ToList();
#endif
            return foundWords;
        }



        public static double Search(
           string word,
           string word2)
        {
#if !LINQ
            List<string> foundWords = new List<string>();

           
                // Calculate the Levenshtein-distance:
                int levenshteinDistance =
                    LevenshteinDistance(word, word2);

                if (word2.ToLower().Contains("plaza de toros"))
                { 
                string _st="hello";
                }

                if (word.ToLower().Contains("palau sant jordi"))
                {
                    string _st = "hello";
                }

                // Length of the longer string:
                int length = Math.Max(word.Length, word2.Length);

                // Calculate the score:
                double score = 1.0 - (double)levenshteinDistance / length;

                if (score >= 0.55)
                {
                    return score;   
                }
                return score;
            
#else
			// Tests have prove that the !LINQ-variant is about 3 times
			// faster!
			List<string> foundWords =
				(
					from s in wordList
					let levenshteinDistance = LevenshteinDistance(word, s)
					let length = Math.Max(s.Length, word.Length)
					let score = 1.0 - (double)levenshteinDistance / length
					where score > fuzzyness
					select s
				).ToList();
#endif
        }
    }
}
