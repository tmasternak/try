# Try .NET <img src ="https://user-images.githubusercontent.com/2546640/56708992-deee8780-66ec-11e9-9991-eb85abb1d10a.png" width="80px" alt="dotnet bot in space" align ="right">
|| [**Basics**](#basics) • [**Experiences**](#experiences) || [**Setup**](#setup) • [**Getting Started**](#getting-started) || [**Samples**](Samples) ||

![Try_.NET Enabled](https://img.shields.io/badge/Try_.NET-Enabled-501078.svg)

## Basics
**What is Try .NET**: Try .NET is an interactive documentation generator for .NET Core.

**Please Note**: At the moment Try .NET only works with C# documentation. 

**What is the repo for?**
- A place where you can post issues about [Try .NET](https://github.com/dotnet/try/issues).
- **Note:** The code for Try .NET is not currently hosted on GitHub.*(Coming soon)*

## Experiences 
 Use Try .NET to create executable C# snippets for your websites or,  interactive markdown files that users can run on their machine. 

**Websites** 

Microsoft Docs uses Try .NET to create interactive documentation. Users can run and edit code al in the browser.
![Try NET_online](https://user-images.githubusercontent.com/2546640/57144765-c850cc00-6d8f-11e9-982d-50d2b6dc3591.gif)

**Interactive .NET documentation**

Try .NET enables .NET developers to create interactive markdown files.
To make your markdown files interactive, you will need to [.NET Core 3.0 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.0), the dotnet try global tool(*coming soon*) and [Visual Studio](https://visualstudio.microsoft.com/) / [VS Code](https://code.visualstudio.com/)(or any other editor of your choice). 
![interactive_doc](https://user-images.githubusercontent.com/2546640/57158389-47a2c780-6db1-11e9-96ad-8c6e9ab52853.png)

## Setup
Before you get can start creating interactive documentation, you will need to install the following: 
- [.NET Core 3.0 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.0)
- [dotnet try global tool](*coming soon*)
Once you have successfully installed `dotnet try` global tool, enter the command `dotnet try -h` you will see a list of commands:

| Command        | Purpose                                |
|----------------|----------------------------------------|
| `demo`         | launches getting started documentation |
| `verify`       | compiler for documentation             |
## Getting Started
