The repository now includes an .editorconfig file encoding the project's C# conventions (naming, indentation, file-scoped namespaces, using placement, etc.).

Because there are no .sln or .csproj files in this workspace, automatic formatting with dotnet-format could not be run here.

To apply the conventions locally and produce a patch, run the following on your machine (requires dotnet SDK and dotnet-format tool):

1. If you don't have dotnet-format installed, install it:
   dotnet tool install -g dotnet-format

2. If the repository has a solution or project, run from the repository root:
   dotnet format <path-to-solution-or-project> --fix-style warn --fix-whitespace --fix-analyzers

3. Alternatively, format each project file individually:
   dotnet format ./ProjectFolder/Project.csproj --fix-style warn --fix-whitespace --fix-analyzers

4. After running formatting, generate a patch for review:
   git add -A
   git diff --staged --no-prefix > formatting.patch

Note: This workspace run created formatting.patch containing only the new .editorconfig (because dotnet-format could not run here). After running dotnet-format locally, the same command in step 4 will produce a patch with the code changes.
