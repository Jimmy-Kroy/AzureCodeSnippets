# Exercise2 App Modification Instructions

You need to modify the code of the Exercise2 app to meet the following requirements:

- Use message roles and variables in chat completion prompts.
- Configure few-shot prompting by using the following examples:
  - Can you send a very quick approval to the marketing team?
    - Intent: ContinueConversation
  - Thanks, I’m done for now.
    - Intent: EndConversation

The following comments have been added to the application code to indicate where your changes should be made:

- TODO 2.1
- TODO 2.2
- TODO 2.3
- TODO 2.4

Only modify the code in these sections.

## References

The following references are used:
1. [Templates.cs on GitHub](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/LearnResources/MicrosoftLearn/Templates.cs): Provides examples and templates for using message roles and variables in prompts.
2. [FunctionsWithinPrompts.cs on GitHub](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/LearnResources/MicrosoftLearn/FunctionsWithinPrompts.cs): Demonstrates the use of functions within prompts for few-shot learning.