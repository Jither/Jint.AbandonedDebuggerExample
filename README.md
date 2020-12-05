Jint Debugger Example
=====================
This is an example of how to implement a debugger user interface for [Jint](https://github.com/sebastienros/Jint).

Currently in *very* early stages (no actual Jint-related code yet), it is intended to be cross platform. To that end, it's console based -
however, the interfacing with Jint (including running the debugger on a non-UI thread) should be easily adaptable to e.g. WPF or other GUI frameworks.

Currently this builds against .NET Core 3.1 and the `dev` branch of Jint.
