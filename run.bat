@echo off
setlocal
cd /d "%~dp0"
call gradlew.bat :prepare :compileDotNet
if errorlevel 1 exit /b %errorlevel%
call gradlew.bat :runIde
exit /b %errorlevel%
