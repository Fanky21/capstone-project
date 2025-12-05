@echo off
echo ====================================
echo FORCE REBUILD APK - COMPLETE CLEAN
echo ====================================
echo.

echo [1] Stopping Unity if running...
taskkill /F /IM Unity.exe 2>nul
timeout /t 2 /nobreak >nul

echo.
echo [2] Deleting ALL Unity caches...
if exist "Library\Bee\Android" rmdir /S /Q "Library\Bee\Android" && echo   - Deleted Library\Bee\Android
if exist "Library\Bee\artifacts" rmdir /S /Q "Library\Bee\artifacts" && echo   - Deleted Library\Bee\artifacts
if exist "Library\ScriptAssemblies" rmdir /S /Q "Library\ScriptAssemblies" && echo   - Deleted Library\ScriptAssemblies
if exist "Library\il2cpp_cache" rmdir /S /Q "Library\il2cpp_cache" && echo   - Deleted Library\il2cpp_cache

echo.
echo [3] Deleting old APK...
if exist "*.apk" del /Q "*.apk" && echo   - Deleted old APK files

echo.
echo ====================================
echo DONE! Caches deleted.
echo ====================================
echo.
echo NEXT STEPS:
echo 1. Open Unity
echo 2. File - Build Settings
echo 3. Click "Build" (NOT "Build And Run")
echo 4. Save as "trading.apk"
echo 5. Run: install-apk.bat
echo.
pause
