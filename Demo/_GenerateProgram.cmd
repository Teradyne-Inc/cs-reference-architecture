@echo off
set "versionCheck=..\..\tools\IgxlVersionCheck\IgxlVersionCheck.cmd"
if exist "%versionCheck%" ( call "%versionCheck%" )
set "tpname=Demo"
set "slnName=Demo_Generated_DoNotUse"
REM Check if PortBridgeEnabled.txt exists in the src folder (next to the Visual Studio solution)
if exist "..\PortBridgeEnabled.txt" (
    echo PortBridgeEnabled.txt found. Running IGLinkCL with PortBridge selector.
    IGLinkCL -i "%tpname%.igxlproj" -w "%tpname%" -g "%tpname%.igxl" --Compile --SlnFile "..\%slnName%.sln" --Selectors "PortBridge"
) else (
    echo PortBridgeEnabled.txt not found. Running IGLinkCL without PortBridge selector.
    IGLinkCL -i "%tpname%.igxlproj" -w "%tpname%" -g "%tpname%.igxl" --Compile --SlnFile "..\%slnName%.sln"
)

REM Remove the .sln and build.log files
del "..\%slnName%.sln" /Q
del "..\%slnName%_build.log" /Q