@echo off
REM Build script for Local Trading Server
REM Compiles Java source files with required dependencies

echo ==================================
echo Local Trading Server Build Script
echo ==================================
echo.

REM Check if Java is installed
echo Checking Java installation...
java -version >nul 2>&1
if errorlevel 1 (
    echo Error: Java not found. Please install JDK 8 or higher.
    echo Download from: https://www.oracle.com/java/technologies/downloads/
    pause
    exit /b 1
)
echo Java found.
echo.

REM Create directories
if not exist "libs" mkdir libs

REM Build classpath
echo Building classpath...
set CLASSPATH=.
for %%i in (libs\*.jar) do set CLASSPATH=!CLASSPATH!;%%i
echo Classpath built.
echo.

REM Compile Java files
echo Compiling Java files...

javac -cp "%CLASSPATH%" -d . -encoding UTF-8 MoneyManager.java
if errorlevel 1 goto error

javac -cp "%CLASSPATH%" -d . -encoding UTF-8 CompanyDataManager.java
if errorlevel 1 goto error

javac -cp "%CLASSPATH%" -d . -encoding UTF-8 NewsManager.java
if errorlevel 1 goto error

javac -cp "%CLASSPATH%" -d . -encoding UTF-8 LocalServer.java
if errorlevel 1 goto error

echo.
echo ==================================
echo Build Successful!
echo ==================================
echo.
echo Compiled classes are in the current directory
echo.
echo To run the server manually:
echo   java -cp ".;%CLASSPATH%" LocalServer
echo.
echo Or use StartLocalServer.cs in Unity to start the server automatically.
pause
exit /b 0

:error
echo.
echo ==================================
echo Build Failed!
echo ==================================
echo.
echo Please check the errors above and try again.
pause
exit /b 1
