# Testing of REPL commands (execute-in-repl/copy-to-repl/reset-from-project)

TODO: write and perform a couple of scenarios to see if the REPL works.
TODO: wire up C# REPL to start from VB projects?

## Scenario 1:

1. Create a fresh VS instance (no opened projects, C# interactive reset).
1. Open C# interactive window.
1. Create a new file `Ctrl+N` of type `Visual C# class`.
1. Select the entire file. Right click and select `Copy to interactive`.
1. Check that a window appeared and contains the contents of the `Class1.cs` file.
1. Check that interactive window is focused. Press `Enter`. This should execute the submission.
1. Replace the contents of a file with:
```
var x = 1;
var y = 2;
Console.WriteLine("{0} + {1} = {2}", x.ToString(), y.ToString(), (x + y).ToString());
```
1. Select the first line, right click and choose `Execute in interactive`.
1. Close the interactive window.
1. Select the second line and third line and use the `Ctrl+e, Ctrl+enter` shortcut.
1. Check that the output is `1 + 2 = 3`.

## Scenario 2:

1. Create a fresh VS instance (no opened projects, C# interactive reset).
1. Create a `C# Class library` project.
1.

## Scenario 3:

1. Create a fresh VS instance (no opened projects, C# interactive reset).
1. Create a WPF Project.

# Things to test:

1. Try to run REPL from VB project
1. Try to execute in REPL from VB project
1. Check that REPL actually gets reset