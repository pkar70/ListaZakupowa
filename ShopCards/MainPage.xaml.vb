Imports vb14 = Vblib.pkarlibmodule14
Imports pkar.DotNetExtensions
Imports Vblib
Imports mygeo = pkar.BasicGeopos
' tylko z tym przejsciem przez mygeo działa, inaczej nie widzi?

Public NotInheritable Class MainPage
    Inherits Page

    Private msNavigatedParam As String = ""
    Protected Overrides Sub onNavigatedTo(e As NavigationEventArgs)

        If e.Parameter IsNot Nothing Then
            msNavigatedParam = e.Parameter.ToString
        Else
            msNavigatedParam = ""
        End If

    End Sub


    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Me.ProgRingInit(True, False)
        Me.ShowAppVers

        'If vb14.GetSettingsBool("uiOneDrive") AndAlso NetIsIPavailable() Then
        '    Me.ProgRingShow(True)
        '    If App.mODroot Is Nothing Then App.mODroot = Await ODclient.GetRootAsync()
        '    Me.ProgRingShow(False)
        'End If

        If App.moSklepy IsNot Nothing Then
            '' wróciliśmy tutaj z wizyty w sklepie, może coś się zmieniło
            '' tam mógł być nie OK, ale Back - a i tak trzeba zapisać (zmiany w kartach)
            'Await App.moSklepy.SaveAsync
        Else
            App.moSklepy = New ListaSklepow
            Me.ProgRingShow(True)
            Await App.moSklepy.LoadAsync(False) ' vb14.GetSettingsBool("uiOneDrive") And NetIsIPavailable())
            If App.moSklepy.AnyIconMissing() Then
                If NetIsIPavailable() Then
                    If Await vb14.DialogBoxYNAsync("Some icons are missing. Download?") Then
                        Await App.moSklepy.DownloadMissingIcons()
                    End If
                Else
                    Await vb14.DialogBoxAsync("Some icons are missing, but we have no Internet connection.")
                End If
            End If
                Me.ProgRingShow(False)
        End If

        If msNavigatedParam <> "" Then
            ' czyli wywołanie z linkiem do dodawania kart
            Await EwentualnieDodajKarte(msNavigatedParam)
        End If

        uiSync.IsEnabled = pkar.NetIsIPavailable

        ' pierwsze pokazanie - czekamy na ściągnięcie ikonek?
        Me.ProgRingShow(True)
        PokazListe()
        Me.ProgRingShow(False)


    End Sub

    Private Async Function EwentualnieDodajKarte(msNavigatedParam As String) As Task
        If Not msNavigatedParam.ToLower.StartsWith("shcards://card?") Then Return

        Dim sSklepName As String = ""   ' s=
        Dim sCardNum As String = "" ' n=
        Dim iCardType As Integer = 0 ' t=

        ' System.Web.HttpUtility.ParseQueryString 
        Dim aParams As String() = msNavigatedParam.Split("&")
        For Each sParam As String In aParams
            If sParam.StartsWith("s=") Then sSklepName = sParam.Substring(2)
            If sParam.StartsWith("n=") Then sCardNum = sParam.Substring(2)
            If sParam.StartsWith("t=") Then Integer.TryParse(sParam.Substring(2), iCardType)
        Next

        If sSklepName = "" Or sCardNum = "" Or iCardType = 0 Then
            vb14.DialogBox("Nieprawidłowy Uri aktuwyjący app")
            Return
        End If

        For Each oSklep As VBlib_Karty.JedenSklep In App.moSklepy.GetList
            If oSklep.sName = sSklepName Then
                ' dodaj kartę
                Await EditSklep.DodajKarteWedleKodu(oSklep, iCardType, sCardNum, True)
                Return
            End If
        Next

        ' *TODO* nieznay sklep
        vb14.DialogBox("Nie umiem dodać karty gdy nie znam sklepu - najpierw dodaj sklep")

    End Function

    Private Sub PokazListe()
        uiLista.ItemsSource = App.moSklepy.GetOrderedList
    End Sub

#Region "sklep context menu"

    Private Function GetJedenSklep(sender As Object) As VBlib_Karty.JedenSklep
        Dim oFE As FrameworkElement = TryCast(sender, FrameworkElement)
        If oFE Is Nothing Then Return Nothing

        Return TryCast(oFE.DataContext, VBlib_Karty.JedenSklep)

    End Function

    Private Sub uiGoWeb_Click(sender As Object, e As RoutedEventArgs)
        ' otwarcie strony sklepu

        Dim oItem As VBlib_Karty.JedenSklep = GetJedenSklep(sender)
        If oItem Is Nothing Then Return

        Dim oUri As New Uri(oItem.sSklepUrl)
        oUri.OpenBrowser()
    End Sub


    Private Sub uiGoShop_Click(sender As Object, e As RoutedEventArgs)
        ' otwarcie danych/edit sklepu 

        Dim oItem As VBlib_Karty.JedenSklep = GetJedenSklep(sender)
        If oItem Is Nothing Then Return

        Me.Navigate(GetType(EditSklep), oItem.sName)

    End Sub

    Private Sub uiShowKarta_Click(sender As Object, e As RoutedEventArgs)
        ' pokazanie karty

        Dim oItem As VBlib_Karty.JedenSklep = GetJedenSklep(sender)
        If oItem Is Nothing Then Return

        Me.Navigate(GetType(KartySklepu), oItem.sName)

    End Sub
    Private Sub uiListaZakupowa_Click(sender As Object, e As RoutedEventArgs)
        ' *TODO* przeskok do ListaZakupowa

        Dim oItem As VBlib_Karty.JedenSklep = GetJedenSklep(sender)
        If oItem Is Nothing Then Return

        vb14.DialogBox("tego jeszcze nie umiem")
        ' AskConfirm
        ' via RemoteSystem - open dany sklep
    End Sub

    Private Async Sub uiDeleteSklep_Click(sender As Object, e As RoutedEventArgs)
        Dim oItem As VBlib_Karty.JedenSklep = GetJedenSklep(sender)
        If oItem Is Nothing Then Return

        If Not Await vb14.DialogBoxYNAsync("Sure usunąć sklep " & oItem.sName & "?") Then Return

        App.moSklepy.Delete(oItem.sName)
        Me.ProgRingShow(True)
        Await App.moSklepy.SaveAsync
        Me.ProgRingShow(False)
        PokazListe()
    End Sub



#End Region

#Region "cmd bar"

    Private Sub uiAddShop_Clicked(sender As Object, e As RoutedEventArgs)
        ' dodanie sklepu

        Me.Navigate(GetType(EditSklep), "")

    End Sub

#Region "sync files"

    Private Async Function CopyOneFileFromOneDriveIfNewer(oDstFolder As Windows.Storage.StorageFolder, sFilename As String, dLastSync As DateTimeOffset) As Task(Of Boolean)
        vb14.DumpCurrMethod(sFilename & " do folderu " & oDstFolder.Path)

        Dim oRoamFile As Windows.Storage.StorageFile
        Dim oODfile As ODfile = Nothing
        oODfile = Await App.mODroot.GetFileAsync(sFilename)
        If oODfile Is Nothing Then
            vb14.DumpMessage("tego pliku nie ma w OneDrive")
            App.gsLastSyncSummary &= $"Skipping {sFilename}: not exist on OD{vbCrLf}"
            Return False ' nie ma pliku w OneDrive, to go nie kopiujemy
        End If

        Dim oDTO As DateTimeOffset = oODfile.GetLastModDate

        If Not Await oDstFolder.FileExistsAsync(sFilename) Then
            vb14.DumpMessage("kopiuję - bo tego pliku nie ma lokalnie")
            App.gsLastSyncSummary &= $"Downloading {sFilename}, as there is no such file locally{vbCrLf}"
        Else

            If dLastSync.AddSeconds(2) > oDTO Then
                vb14.DumpMessage("nie kopiuję bo last sync " & dLastSync.ToString & ", onedrive: " & oDTO.ToString)
                App.gsLastSyncSummary &= $"Skipping {sFilename}, OD date is older than last sync{vbCrLf}"
                Return False ' plik w OneDrive jest starszy niż last sync
            End If

            oRoamFile = Await oDstFolder.TryGetItemAsync(sFilename)
            Dim oRoamProp As Windows.Storage.FileProperties.BasicProperties = Await oRoamFile.GetBasicPropertiesAsync

            If oRoamProp.DateModified.AddSeconds(2) > oDTO Then
                App.gsLastSyncSummary &= $"Skipping {sFilename}, OD is older than local{vbCrLf}"
                vb14.DumpMessage("nie kopiuję bo lokalnie " & oRoamProp.DateModified.ToString & ", onedrive: " & oDTO.ToString)
                Return False ' plik w OneDrive jest starszy
            End If
            vb14.DumpMessage("kopiuję: lokalnie " & oRoamProp.DateModified.ToString & " < onedrive: " & oDTO.ToString)
            App.gsLastSyncSummary &= $"Downloading {sFilename}, OD is newer than local{vbCrLf}"
        End If
        oRoamFile = Await oDstFolder.CreateFileAsync(sFilename, Windows.Storage.CreationCollisionOption.ReplaceExisting)

        ' no to kopiujemy
        Using oStreamOneDrive = Await oODfile.GetStreamAsync
            Using oStreamRoaming = Await oRoamFile.OpenStreamForWriteAsync()
                oStreamOneDrive.CopyTo(oStreamRoaming)
                oStreamRoaming.Flush()
            End Using
        End Using

        Dim sPath As String = oRoamFile.Path
        oRoamFile = Nothing ' tak, żeby plik był zamknięty, zasoby zwolnione, i w ogóle - żeby zadziałała zmiana daty

        ' Dim oFileProp As Windows.Storage.FileProperties.BasicProperties = Await oRoamFile.GetBasicPropertiesAsync
        ' oFileProp.DateModified = oODfile.GetLastModDate
        IO.File.SetLastWriteTime(sPath, New Date(oDTO.Ticks))

        Return True

    End Function

    Private Async Function CopyAllFilesFromOneDriveIfNewer(dLastSync As DateTimeOffset) As Task
        Dim bODnewer As Boolean = Await CopyOneFileFromOneDriveIfNewer(App.GetJsonFolder, ListaSklepow._mFileNameBase, dLastSync)

        If bODnewer Then
            ' plik jest zaktualizowany - wczytaj go na nowo
            Await App.moSklepy.LoadAsync(False) ' nie z OneDrive, bo wlasnie sobie skopiowalismy aktualną wersję
        End If

        ' teraz kopiowanie obrazków kart
        For Each oSklep As VBlib_Karty.JedenSklep In App.moSklepy.GetList
            For Each oKarta As VBlib_Karty.JednaKarta In oSklep.lKarty
                ' karta może być bez obrazka, a z generowaniem z numeru - wtedy nie ma co kopiować
                If oKarta.sPicFilename <> "" Then
                    Await CopyOneFileFromOneDriveIfNewer(App.GetPickiFolder, oKarta.sPicFilename, dLastSync)
                End If
            Next
        Next

    End Function

    Private Async Function CopyOneFileToOneDriveIfNewer(oSrcFolder As Windows.Storage.StorageFolder, sFilename As String) As Task
        vb14.DumpCurrMethod(sFilename & " z folderu " & oSrcFolder.Path)

        Dim oRoamFile As Windows.Storage.StorageFile = Await oSrcFolder.TryGetItemAsync(sFilename)

        If oRoamFile Is Nothing Then
            vb14.DumpMessage("tego pliku nie ma lokalnie")
            Return
        End If

        Dim oODfile As ODfile = Await App.mODroot.GetFileAsync(sFilename)

        If oODfile Is Nothing Then
            vb14.DumpMessage("kopiuję - tego pliku nie ma w OneDrive")
            App.gsLastSyncSummary &= $"Uploading {sFilename}: not exist on OD{vbCrLf}"
            Await App.mODroot.CopyFileToOneDriveAsync(oRoamFile)
        Else
            Dim oRoamProp As Windows.Storage.FileProperties.BasicProperties = Await oRoamFile.GetBasicPropertiesAsync
            Dim oDTO As DateTimeOffset = oODfile.GetLastModDate
            If oRoamProp.DateModified < oODfile.GetLastModDate.AddSeconds(2) Then
                App.gsLastSyncSummary &= $"Skipping {sFilename}, OD is newer or same as local{vbCrLf}"
                vb14.DumpMessage("nie kopiuję bo lokalnie " & oRoamProp.DateModified.ToString & ", onedrive: " & oDTO.ToString)
                Return ' plik w OneDrive jest nowszy
            End If

            vb14.DumpMessage("kopiuję: lokalnie " & oRoamProp.DateModified.ToString & " > onedrive: " & oDTO.ToString)
            App.gsLastSyncSummary &= $"Uploading {sFilename}, OD is older than local{vbCrLf}"
            Await App.mODroot.CopyFileToOneDriveAsync(oRoamFile)
        End If

    End Function

    Private Async Function CopyAllFilesToOneDriveIfNewer() As Task
        Await CopyOneFileToOneDriveIfNewer(App.GetJsonFolder, ListaSklepow._mFileNameBase)

        ' kopiowanie obrazków kart
        For Each oSklep As VBlib_Karty.JedenSklep In App.moSklepy.GetList
            For Each oKarta As VBlib_Karty.JednaKarta In oSklep.lKarty
                ' karta może być bez obrazka, a z generowaniem z numeru - wtedy nie ma co kopiować
                If oKarta.sPicFilename <> "" Then
                    Await CopyOneFileToOneDriveIfNewer(App.GetPickiFolder, oKarta.sPicFilename)
                End If
            Next
        Next

    End Function




#End Region

    Private Async Sub uiSync_Clicked(sender As Object, e As RoutedEventArgs)
        ' synchronizacja z OneDrive

        If Not pkar.NetIsIPavailable Then
            vb14.DialogBox("ale nie masz sieci...")
            Return
        End If

        App.gsLastSyncSummary = "Summary of syncing @" & Date.Now.ToString("yyyy.MM.dd HH:mm") & vbCrLf & vbCrLf

        Dim dLastSync As DateTimeOffset = vb14.GetSettingsDate("lastSync")
        App.gsLastSyncSummary &= "Last sync time: " & dLastSync.ToString("yyyy.MM.dd HH:mm") & vbCrLf

        Me.ProgRingShow(True)
        If App.mODroot Is Nothing Then App.mODroot = Await ODclient.GetRootAsync()
        App.gsLastSyncSummary &= "From OneDrive to local" & vbCrLf
        Await CopyAllFilesFromOneDriveIfNewer(dLastSync)
        App.gsLastSyncSummary &= $"{vbCrLf}From local to OneDrive{vbCrLf}"
        Await CopyAllFilesToOneDriveIfNewer()
        Await App.moSklepy.DownloadMissingIcons
        Me.ProgRingShow(False)

        PokazListe()

        vb14.SetSettingsCurrentDate("lastSync")

        If vb14.GetSettingsBool("uiShowSyncSummary") Then
            Await vb14.DialogBoxAsync(App.gsLastSyncSummary)
        End If

        ' sprawdź czy OD nie ma czegoś nowszego
        ' wyślij do OD wszystko
        ' aktualizacja zmiennej "lastSave"
    End Sub

    Private Async Sub uiGPS_Clicked(sender As Object, e As RoutedEventArgs)
        ' wybór sklepu według GPS

        ' w tym są permissiony
        Dim oCurrGeo As mygeo = Await GetCurrentPointAsync(10)

        ' szukaj w App.moSklepy
        Dim lLista As New List(Of VBlib_Karty.JedenSklep)
        For Each oSklep In App.moSklepy.GetList
            For Each oLoc In oSklep.lLocations
                If oCurrGeo.DistanceTo(oLoc.oGeo) < 100 Then
                    lLista.Add(oSklep)
                    Exit For
                End If
            Next
        Next

        ' jesli jeden - przejdz do sklepu
        If lLista.Count = 1 Then
            Me.Navigate(GetType(KartySklepu), lLista.ElementAt(0).sName)
            Return
        End If

        ' *TODO* jesli wiecej - stworz MenuFlyout do wyboru
        If lLista.Count > 1 Then
            vb14.DialogBox("więcej sklepów jest tutaj - tego jeszcze nie umiem")
            Return
        End If

        ' *TODO* jesli nie ma, to zapytaj czy uzyc OpenStreetMap
        If Await vb14.DialogBoxYNAsync("Nie znam takiej lokalizacji, sprawdzić w OSM?") Then
            ' gdy tak, sciagnij POI z OSM dla point
            vb14.DialogBox("jeszcze nie umiem obsługiwać OSM")
        End If

    End Sub

    Private Sub uiSettings_Clicked(sender As Object, e As RoutedEventArgs)
        Me.Navigate(GetType(Settings))
    End Sub
#End Region

    Private Shared Function GetMaxLocTime() As TimeSpan
        ' promień 100 metrów, przy podanej szybkości - ile sekund
        Dim iSpeed As Integer = vb14.GetSettingsInt("uiWalkSpeed")
        Dim dMetersPerSecond As Double = iSpeed * 1000 / 3600

        Return TimeSpan.FromSeconds(50 / dMetersPerSecond)

    End Function

    ''' <summary>
    ''' zwraca NULL lub współrzędne lokalizacji
    ''' </summary>
    ''' <param name="iSecTimeout"></param>
    ''' <returns></returns>
    Public Shared Async Function GetCurrentPointAsync(iSecTimeout As Integer) As Task(Of mygeo)

        Dim rVal As Windows.Devices.Geolocation.GeolocationAccessStatus
        rVal = Await Windows.Devices.Geolocation.Geolocator.RequestAccessAsync()

        If rVal <> Windows.Devices.Geolocation.GeolocationAccessStatus.Allowed Then
            Await vb14.DialogBoxResAsync("resErrorNoGPSAllowed")
            Return Nothing
        End If

        ' https//stackoverflow.com/questions/33865445/gps-location-provider-requires-access-fine-location-permission-for-android-6-0/33866959'
        Dim oDevGPS As New Windows.Devices.Geolocation.Geolocator()
        Dim oTimeout As TimeSpan = New TimeSpan(0, 0, iSecTimeout) ' ;    // timeout 

        Dim oPos As Windows.Devices.Geolocation.Geoposition = Nothing

        oDevGPS.DesiredAccuracyInMeters = vb14.GetSettingsInt("uiGPSPrec") ' ; // dla 4 km/h; 100 m = 90 sec, 75 m = 67 sec
        Dim sErr As String = ""

        Try
            oPos = Await oDevGPS.GetGeopositionAsync(GetMaxLocTime, oTimeout)
            Return mygeo.FromObject(oPos.Coordinate.Point.Position)

        Catch e As Exception
            sErr = e.Message
        End Try

        Await vb14.DialogBoxResAsync("resErrorGettingPos")

        Return Nothing

    End Function

End Class


#Region "konwertery"
Public Class KonwersjaEnabledWebPage
    Implements IValueConverter

    ' dla IsEnabled, false gdy nie ma stringu

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        Dim sTmp As String = CType(value, String)
        If String.IsNullOrEmpty(sTmp) Then Return False
        Return True
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

Public Class KonwersjaEnabledShowCard
    Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        Dim vTmp As String = CType(value, Integer)
        If vTmp > 0 Then Return True
        Return False
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

#End Region