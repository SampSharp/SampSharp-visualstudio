SampSharp.VisualStudio
======================

A Visual Studio extension for building and debugging SampSharp gamemodes.

The extension is already operational, but still in beta. To use this plugin with your project, you must
change the project type of your `.csproj` file. Simply open the file in a text editor and add
`<ProjectTypeGuids>{629CB73E-1FBE-4FA1-81F4-F7C15FCA9590};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>`
in the first `<PropertyGroup>` block.

Installation
------------
You can install this Visual Studio trough Visual Studio's Extensions manager ( `Tools > Extensions and Updates...` ). It's listed under the name `SampSharp Plugin`.
