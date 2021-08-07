echo F|xcopy "About\About.xml" "..\HumanStoryteller - Release\About\About.xml" /C /Y /K /Q /D
echo F|xcopy "About\Preview.png" "..\HumanStoryteller - Release\About\Preview.png" /C /Y /K /Q /D
echo F|xcopy "LoadFolders.xml" "..\HumanStoryteller - Release\LoadFolders.xml" /C /Y /K /Q /D
xcopy "Assemblies" "..\HumanStoryteller - Release\Assemblies" /S /C /Y /K /I /Q /D
xcopy "v1.3\Assemblies" "..\HumanStoryteller - Release\v1.3\Assemblies" /S /C /Y /K /I /Q /D
xcopy "Defs" "..\HumanStoryteller - Release\Defs" /S /C /Y /K  /I /Q /D
xcopy "Patches" "..\HumanStoryteller - Release\Patches" /S /C /Y /K  /I /Q /D
xcopy "Languages" "..\HumanStoryteller - Release\Languages" /S /C /Y /K  /I /Q /D
xcopy "Sounds" "..\HumanStoryteller - Release\Sounds" /S /C /Y /K  /I /Q /D
xcopy "Textures" "..\HumanStoryteller - Release\Textures" /S /C /Y /K /I /Q /D
