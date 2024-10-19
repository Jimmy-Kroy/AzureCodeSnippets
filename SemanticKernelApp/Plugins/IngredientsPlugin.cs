using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.SemanticKernel;

namespace SemanticKernelApp.Plugins
{
    public class IngredientsPlugin
    {
        [KernelFunction, Description("Gets a list of the user's available ingredients")]
        public static string GetIngredients()
        {
            string dir = Directory.GetCurrentDirectory();
            string content = File.ReadAllText($"{dir}/data/ingredients.json");
            //Console.WriteLine("GetIngredients: " + content);
            return content;
        }

        [KernelFunction, Description("Gets a list of ingredients for a given recipe")]
        public static string GetRecipe([Description("The user's recipe")] string recipe)
        {
            string ingredients = string.Empty;
            //Console.WriteLine("GetRecipe recipe: " + recipe);
            string dir = Directory.GetCurrentDirectory();
            string content = File.ReadAllText($"{dir}/data/recipes.json");

            // Parse the JSON content
            JsonArray recipes = (JsonArray)JsonNode.Parse(content);

            foreach (JsonNode recipeNode in recipes)
            {
                if(recipeNode["name"].ToString() == recipe)
                {
                    ingredients = recipeNode["ingredients"].ToString();
                }
            }

            return ingredients;
        }
    }
}
