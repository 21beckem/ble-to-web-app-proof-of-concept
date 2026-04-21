@ECHO OFF

dotnet publish -c Release -o ./publish -p:PublishSingleFile=true --self-contained true

@REM Delete everything in the folder except the .exe file
FOR %%f IN (publish\*) DO (
    IF NOT "%%~xf"==".exe" (
        DEL "%%f"
    )
)