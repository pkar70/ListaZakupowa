﻿
' ponieważ NIE DZIAŁA pod Uno.Android wczytywanie pliku (apk nie jest rozpakowany?),
' to w ten sposób przekazywanie zawartości pliku INI
' wychodzi na to samo, edycja pliku defaults.ini albo defsIni.lib.vb

Public Class IniLikeDefaults

    Public Const sIniContent As String = "
[main]
key=value 
uiwalkSpeed=4
uigpsPrec=75

# remark
' remark
; remark
// remark

[debug]
key=value # remark

[app]
; lista z app (bez ustawiania)
uiOneDrive=false
uiSklepySort=false  ; true: sortowanie wedle nazwy
sklepyLastSaveNoOD=false    ; true: ostatni zapis zmian był tylko lokalnie, bez OneDrive

[libs]
; lista z pkarmodule
remoteSystemDisabled=false
appFailData=
offline=false
lastPolnocnyTry=
lastPolnocnyOk=

"

End Class
