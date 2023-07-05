
' odwolanie do Vblib potrzebne dla linii (dwukrotnie)
' Select Case Await Vblib.SelectOneContentChoose(dODdate, dLocalDate,, Vblib.GetSettingsBool("LastSaveNoOD_" & _mFileNameBase))

Public Class JedenSklep
    Public Property sName As String
    Public Property sIconUri As String
    Public Property sIconFilename As String
    Public Property bJestShoplist As Boolean = False
    Public Property sSklepUrl As String
    Public Property lKarty As List(Of JednaKarta)
    Public Property lLocations As List(Of JednaLocation)
    <Newtonsoft.Json.JsonIgnore>
    Public Property sIconPathname As String
End Class

Public Class JednaKarta
    Public Property sPicFilename As String
    Public Property sCzyja As String
    Public Property sNumber As String   ' do generacji zamiast obrazka
    Public Property iCodeType As Integer
End Class
Public Class JednaLocation
    Public Property sName As String
    'Public Property dLat As Double
    'Public Property dLon As Double
    Public Property oGeo As pkar.BasicGeopos
End Class

Public Class ListaSklepow
    Private _Itemy As ObjectModel.ObservableCollection(Of JedenSklep)
    Protected _bDirty = False
    Public Const _mFileNameBase As String = "sklepy.json"
    Private Shared _sRoamingFilePath As String
    Protected _bUseOneDrive As Boolean = False

    Public Sub New(sRoamingRootPath As String)
        _Itemy = New ObjectModel.ObservableCollection(Of JedenSklep)
        _sRoamingFilePath = IO.Path.Combine(sRoamingRootPath, _mFileNameBase)
    End Sub

    Public Sub Add(oNew As JedenSklep)
        _Itemy.Add(oNew)
        _bDirty = True
    End Sub

    Public Sub Delete(sName As String)
        _bDirty = True
        For Each oDel As JedenSklep In _Itemy
            If oDel.sName = sName Then
                _Itemy.Remove(oDel)
                Exit Sub
            End If
        Next
    End Sub

    Public Function GetItem(sName As String) As JedenSklep
        For Each oItem As JedenSklep In _Itemy
            If oItem.sName = sName Then Return oItem
        Next
        Return Nothing
    End Function

    Public Function Count() As Integer
        Return _Itemy.Count
    End Function

    Public Function GetList() As ICollection(Of JedenSklep)
        Return _Itemy
    End Function

    Public Sub Clear()
        _Itemy.Clear()
    End Sub

    Public Function GetOrderedList() As IOrderedEnumerable(Of JedenSklep)
        Dim groups As IOrderedEnumerable(Of JedenSklep) = From c In _Itemy Order By c.sName
        Return groups
    End Function

    Protected Sub LoadJSON(sContent As String)
        Try
            _Itemy = Newtonsoft.Json.JsonConvert.DeserializeObject(sContent, GetType(ObjectModel.ObservableCollection(Of JedenSklep)))
        Catch ex As Exception

        End Try

        If _Itemy Is Nothing Then
            _Itemy = New ObjectModel.ObservableCollection(Of JedenSklep)
        Else
            ' tu mogłoby być przetworzenie ze starej formy, dlat, dlon na mygeo; ale ponieważ app jest nieopublikowana a ja nie używam jeszcze listy sklepów...
        End If
    End Sub

    Protected Function ExportJSON() As String
        Return Newtonsoft.Json.JsonConvert.SerializeObject(_Itemy, Newtonsoft.Json.Formatting.Indented)
    End Function

    ''' <summary>
    ''' zwraca tekst do zapisania do OneDrive, po zapisaniu do Roaming
    ''' </summary>
    ''' <returns></returns>
    Protected Function Save() As String

        Dim sContent As String = ExportJSON()

        IO.File.WriteAllText(_sRoamingFilePath, sContent)

        Return sContent

    End Function

    ''' <summary>
    ''' wczytaj dane; zwraca TRUE gdy powinno się zapisać (bo było z XML)
    ''' </summary>
    ''' <param name="sODcontent">zawartość pliku z OneDrive, bądź "" gdy pliku nie ma</param>
    ''' <param name="dODdate">data tego pliku (nieistotna gdy pliku nie było)</param>
    Protected Async Function LoadLibAsync(sODcontent As String, dODdate As DateTimeOffset) As Task(Of Boolean)
        Dim bShouldSave As Boolean = False

        Dim sContent As String = ""
        Dim dLocalDate As DateTimeOffset = New DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.FromSeconds(0))

        If IO.File.Exists(_sRoamingFilePath) Then
            sContent = IO.File.ReadAllText(_sRoamingFilePath)
            dLocalDate = IO.File.GetLastWriteTime(_sRoamingFilePath)
        End If

        If _bUseOneDrive AndAlso sODcontent <> "" Then
            ' porownaj daty, z tolerancją 20 sekund
            Select Case Await Vblib.SelectOneContentChoose(dODdate, dLocalDate, Vblib.GetSettingsBool("LastSaveNoOD_" & _mFileNameBase))
                Case 0
                    ' oba takie same, przyjmuję bez zmian
                Case 1
                    ' użyj OneDrive
                    sContent = sODcontent
                Case 2
                    ' użyj lokalne
                    bShouldSave = True
            End Select

        End If

        LoadJSON(sContent)

        KonwersjaZPoprzedniejWersji2to3()

        Return bShouldSave

    End Function

    Private Sub KonwersjaZPoprzedniejWersji1to2()
        For Each oItem As JedenSklep In _Itemy
            If oItem.sIconUri = "" Then
                ' stara wersja
                oItem.sIconUri = oItem.sIconFilename
                oItem.sIconUri = ""
            End If
        Next
    End Sub

    Private Sub KonwersjaZPoprzedniejWersji2to3()
        For Each oItem As JedenSklep In _Itemy
            Dim iInd As Integer = oItem.sIconFilename.LastIndexOf("\")
            If iInd > 0 Then oItem.sIconFilename = oItem.sIconFilename.Substring(iInd + 1)
        Next
    End Sub

    ''' <summary>
    ''' tworzy sIconPathname z sCacheDir \ sIconFilename
    ''' </summary>
    ''' <param name="sCacheDir"></param>
    Protected Sub FillIconFileName(sCacheDir As String)
        For Each oItem As JedenSklep In _Itemy
            If oItem.sIconFilename <> "" Then
                oItem.sIconPathname = IO.Path.Combine(sCacheDir, oItem.sIconFilename)
            End If
        Next
    End Sub

    Public Function AnyIconMissing() As Boolean
        For Each oItem As JedenSklep In _Itemy
            If oItem.sIconPathname = "" Then Return True
            If Not IO.File.Exists(oItem.sIconPathname) Then Return True
        Next

        Return False
    End Function


End Class



