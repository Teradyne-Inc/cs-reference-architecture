@echo off
REM Generates the SOC124 customer demo solution.

cd /d "%~dp0"

dotnet new sln -n SOC124 -o "." --force

REM Detect whether .slnx or .sln was created (depends on SDK version)
if exist "SOC124.slnx" (set "SLN=SOC124.slnx") else (set "SLN=SOC124.sln")

dotnet sln "%SLN%" add ..\Core\Csra\Csra.csproj --solution-folder Core
dotnet sln "%SLN%" add ..\Core\Tol\Tol.csproj --solution-folder Core
dotnet sln "%SLN%" add ..\ReferenceLibraries\PortBridge\PortBridgeUtils\PortBridgeUtils.csproj --solution-folder Core

dotnet sln "%SLN%" add ..\ReferenceLibraries\CsTestMethods\CsTestMethods.csproj --solution-folder Examples
dotnet sln "%SLN%" add ..\ReferenceLibraries\CsraTestMethods\CsraTestMethods.csproj --solution-folder Examples
dotnet sln "%SLN%" add ..\ReferenceLibraries\UnitTests\UnitTestExample.csproj --solution-folder Examples
dotnet sln "%SLN%" add ..\ReferenceLibraries\PortBridge\Demo_PB\Demo_PB.csproj --solution-folder Examples

if "%CI%"=="" pause
