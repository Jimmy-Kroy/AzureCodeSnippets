using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernelApp
{
    public class FileReader
    {
        /// <summary>
        /// Reads lines from a file starting from startLineNr to endLineNr.
        /// </summary>
        /// <param name="filename">The path to the file to read from.</param>
        /// <param name="startLineNr">The line number to start reading from (inclusive).</param>
        /// <param name="endLineNr">The line number to stop reading at (inclusive).</param>
        /// <returns>A string containing the lines read from the file.</returns>
        public static string ReadLinesFromFile(string filename, int startLineNr, int endLineNr)
        {
            // StringBuilder to store the result
            StringBuilder result = new StringBuilder();
            try
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(filename);
                // Loop through the specified range of lines
                for (int i = startLineNr - 1; i < endLineNr && i < lines.Length; i++)
                {
                    // Append each line to the result
                    result.AppendLine(lines[i]);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during file reading
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            // Return the result as a string
            return result.ToString();
        }

    }
}
