: $watch_mode_server.cmd
: 2023-07-02
:
: cannot use a parameter file for java because it eats parameters containing '#';
: cannot use a parameter file for validator_cli.jar because that is not supported
@echo off

set @@IGS=
goto _%1
goto _done

:_ERezept2023Q2

set @@IGS=^
 -ig de.basisprofil.r4#0.9.13 ^
 -ig de.abda.erezeptabgabedatenbasis#1.2.0 ^
 -ig kbv.basis#1.1.3 ^
 -ig kbv.ita.for#1.0.3 ^
 -ig kbv.ita.erp#1.0.2 ^
 -ig de.gematik.erezept-workflow.r4#1.1.1 ^
 -ig de.abda.erezeptabgabedaten#1.2.0 ^
 -ig de.gkvsv.erezeptabrechnungsdaten#1.2.0

goto _PlainCoreR4

:_ERezept2023Q3

set @@IGS=^
 -ig de.basisprofil.r4#1.3.2 ^
 -ig de.abda.erezeptabgabedatenbasis#1.3.1 ^
 -ig kbv.basis#1.3.0 ^
 -ig kbv.ita.for#1.1.0 ^
 -ig kbv.ita.erp#1.1.1 ^
 -ig de.gematik.erezept-workflow.r4#1.2.1 ^
 -ig de.abda.erezeptabgabedaten#1.3.1 ^
 -ig de.gkvsv.erezeptabrechnungsdaten#1.3.0

:_PlainCoreR4

: pass remaining parameters first because of special processing for things like -v 6.0.20
call $validator_cli.cmd %2 %3 %4 %5 %6 %7 %8 ^
 -watch-mode single ^
 -watch-scan-delay 1 ^
 -watch-settle-time 1 ^
 %@@IGS% ^
 -output-style eslint-compact ^
 -output f:\WatchModeServer\%1-output.eslint-compact ^
 f:\WatchModeServer\%1-input*.xml

:_done
