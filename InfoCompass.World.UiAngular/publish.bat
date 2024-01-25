cd "d:\Projects\CraftSynth.BookGenerator\SourceCode\CraftSynth.BookGenerator.UiAngular"
CALL angular-build-info --init 
CALL ng build --configuration=production
CALL "C:\PROGRA~1\Wad\WAD.exe" -profile:BookGenerator.Craftsynth.com -inNewProcess -delayInSeconds:3