@echo off
setlocal EnableDelayedExpansion
set "versionCheck=..\..\tools\IgxlVersionCheck\IgxlVersionCheck.cmd"
if exist "%versionCheck%" ( call "%versionCheck%" )
set "tpname=Demo"
set "slnName=Demo_Generated_DoNotUse"
REM Auto-detect PortBridge via PBROOT, consistent with IGComponentManagerParser.VerifyPbVersion
set "pbEnabled=false"
if defined PBROOT if exist "%PBROOT%\Teradyne.PortBridge.dll" set "pbEnabled=true"

REM Use delayed expansion (!PBROOT!) inside the block below: PBROOT may contain parentheses
REM (e.g. "C:\Program Files (x86)\..."), which breaks cmd's parser if expanded via %PBROOT%
REM while inside a parenthesized if/else block.
if "%pbEnabled%"=="true" (
    echo PortBridge detected via PBROOT ^(!PBROOT!^). Running IGLinkCL with PortBridge selector.
    IGLinkCL -i "%tpname%.igxlproj" -w "%tpname%" -g "%tpname%.igxl" --Compile --SlnFile "..\%slnName%.sln" --Selectors "PortBridge" -l
) else (
    echo PortBridge not detected ^(PBROOT not set or Teradyne.PortBridge.dll missing^). Running IGLinkCL without PortBridge selector.
    IGLinkCL -i "%tpname%.igxlproj" -w "%tpname%" -g "%tpname%.igxl" --Compile --SlnFile "..\%slnName%.sln" -l
)

REM Remove the .sln and build.log files
del "..\%slnName%.sln" /Q
del "..\%slnName%_build.log" /Q