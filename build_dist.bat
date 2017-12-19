rd .\dist /S /Q
md .\dist
rd .\src\DrumBeatDesigner\bin\Release  /S /Q

REM set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v3.5
set msBuildDir=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
call %msBuildDir%\msbuild.exe  .\src\DrumBeatDesigner.sln /p:Configuration=Release /l:FileLogger,Microsoft.Build.Engine;logfile=dist.log
set msBuildDir=

XCOPY .\src\DrumBeatDesigner\Bin\Release\*.* .\dist\