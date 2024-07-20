# Project Setup
1. Clone the newly created project to your desktop app and open its directory using Ctrl-Shift-F.
2. Choose a project name that's compliant with pascal case (no spaces -> first letters of each word capitalized).
3. Use it to rename/modify the following files:
	- Under Root Folder
		- [ ] .sln file
		- [ ] All mentions of "VSModTemplate" inside of the .sln file   *(open in Notepad++ or other external editor)*
		- [ ] main mod folder
	- Under Mod Folder
		- [ ] .csproj file
		- [ ] modinfo.json   *(mod name can be anything, the mod id must follow camel case)*
		- [ ] assets/mod-folder   *(make sure this one matches the modid in modinfo.json)*

The rest will depend on the type of mod you're planning to make, outlined below. Be sure to delete this README when you're done :) 


# How To Refactor, Debug, and Package A Code Mod
**To Fully Refactor:** 
1. Open the .sln file in Visual Studio and do the following:
2. Use Ctrl-F on the empty editor to open the 'Find and Replace' tool. Navigate to the 'Replace in Files' tab and check the following options:
	- [ ] 'Match case'
	- [ ] 'Match whole word'
	- [ ] 'Look In' -> Entire Solution *(ignore the sub options)*
3. Click 'Replace All' to replace all mentions of `VSModTemplate` in the code with your new pascal-case project name
4. Open ModConstants.cs and update the name/id variables with the appropriate naming conventions
5. Navigate to your lang file in the assets folder and quickly update the mod title
6. Try to run CakeBuild in debug mode. If it runs with no errors then the refactor was a success!

**To Test:** 
- Change the run mode from 'CakeBuild' to 'Client' to launch a test world using any mods from your base game's mod folder.
- *(Tip)* If it ever crashes with an error stating that it can't find the Vintagestory.exe, try to run it as 'Server' first, then swap back to 'Client'

**To Package:**
- Simply set the CakeBuild mode to 'Release' instead of 'Debug' to have it compile the code into a dll and package all the files into a zip.
- You can find the zip under `Releases/` at the repo's root folder.


# How To Refactor, Debug, and Package A Content Mod
**To Fully Refactor:** 
- Just change the mod type to "content" in the modinfo file! As long as you renamed all the core files listed above and updated the paths in your .sln file, you should be able to open the project in Visual Studio and freely edit all your json files.

**To Test:** 
- Delete the entire src folder to remove all the code before launching the test client. Alternatively you can follow the code refactoring steps above if you'd like to keep it as a template for future plans, the game will ignore any dlls found packaged within a content mod.

**To Package:** 
- Manually package both the assets folder and modinfo.json into a zip folder using winrar or 7-zip. Using CakeBuild to package a content mod, even one that includes no source code, will still include an empty dll.