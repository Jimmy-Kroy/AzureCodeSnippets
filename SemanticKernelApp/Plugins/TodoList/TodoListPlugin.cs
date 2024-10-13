using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.SemanticKernel;


namespace SemanticKernelApp.Plugins.TodoList
{
    public class TodoListPlugin
    {
        /* Native functions have a certain format and a recommended file structure to be used by the kernel. 
         * Native functions should use the KernelFunction decorator in their definitions. They also use a 
         * Description field for parameters. */
        [KernelFunction, Description("Mark a todo list item as complete")]
        public static string CompleteTask([Description("The task to complete")] string task)
        {
            // Read the JSON file
            string jsonFilePath = $"{Directory.GetCurrentDirectory()}/data/todo.txt";
            string jsonContent = File.ReadAllText(jsonFilePath);

            // Parse the JSON content
            JsonNode todoData = JsonNode.Parse(jsonContent);

            // Find the task and mark it as complete
            JsonArray todoList = (JsonArray)todoData["todoList"];
            foreach (JsonNode taskNode in todoList)
            {
                if (taskNode["task"].ToString() == task)
                {
                    taskNode["completed"] = true;
                    break;
                }
            }

            // Save the modified JSON back to the file
            File.WriteAllText(jsonFilePath, JsonSerializer.Serialize(todoData));
            return $"Task '{task}' marked as complete.";
        }

        [KernelFunction, Description("Returns a todo list")]
        public static string GetTasks()
        {
            // Read the JSON file
            string jsonFilePath = $"{Directory.GetCurrentDirectory()}/data/todo.txt";
            string jsonContent = File.ReadAllText(jsonFilePath);
            return jsonContent;
        }
    }
}
