@ECHO OFF
IF NOT "%~f0" == "~f0" GOTO :WinNT
@"ir.exe" "Z:/code/IronSP/src/IronSharePoint.IronRuby10/TEMPLATE/Features/IronSP_IronRuby10/Lib/ironruby/gems/1.8/bin/haml" %1 %2 %3 %4 %5 %6 %7 %8 %9
GOTO :EOF
:WinNT
@"ir.exe" "%~dpn0" %*
