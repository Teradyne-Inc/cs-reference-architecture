@echo off
echo ============================================
echo   Generating Visual Studio Solution and Test Program
echo ============================================

set "tpname=Demo"
set "slnFile=%tpname%_Generated"

IGLinkCL -i "%tpname%.igxlproj" -w "%tpname%" -g "%tpname%.igxl" --Compile --SlnFile "%slnFile%.sln" -l

if errorlevel 1 (
    echo ERROR: IGLinkCL command failed. Aborting script.
    exit /b 1
)

@REM REM Remove the .sln and build.log files
@REM del "%slnFile%.sln" /Q
if exist "%slnFile%_build.log" del "%slnFile%_build.log" /Q

echo ============================================
echo   Done! Solution and test program generated.
echo ============================================
