@echo off
setlocal

REM Get the source and target file paths from arguments
set "FROM_PATH=%~1"
set "TO_PATH=%~2"

echo Replacing contents of "%TO_PATH%" with "%FROM_PATH%"

REM Check if source file exists
IF NOT EXIST "%FROM_PATH%" (
    echo ERROR: Source file does not exist: %FROM_PATH%
    exit /b 1
)

REM Check if target file exists
IF NOT EXIST "%TO_PATH%" (
    echo ERROR: Target file does not exist: %TO_PATH%
    exit /b 1
)

REM Perform the replacement
copy /Y "%FROM_PATH%" "%TO_PATH%"

IF %ERRORLEVEL% EQU 0 (
    echo Replacement successful.
) ELSE (
    echo ERROR: Failed to replace file.
    exit /b 1
)

endlocal
