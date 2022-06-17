Imports vb14 = Vblib.pkarlibmodule14
Imports Vblib.Extensions


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

        If vb14.GetSettingsBool("uiOneDrive") Then
            Me.ProgRingShow(True)
            If App.mODroot Is Nothing Then App.mODroot = Await ODclient.GetRootAsync()
            Me.ProgRingShow(False)
        End If

        If App.moSklepy IsNot Nothing Then
            '' wróciliśmy tutaj z wizyty w sklepie, może coś się zmieniło
            '' tam mógł być nie OK, ale Back - a i tak trzeba zapisać (zmiany w kartach)
            'Await App.moSklepy.SaveAsync
        Else
            App.moSklepy = New ListaSklepow
            Me.ProgRingShow(True)
            Await App.moSklepy.LoadAsync(vb14.GetSettingsBool("uiOneDrive"))
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

    Private Async Function CopyOneFileFromOneDriveIfNewer(oDstFolder As Windows.Storage.StorageFolder, sFilename As String) As Task(Of Boolean)
        vb14.DumpCurrMethod(sFilename & " do folderu " & oDstFolder.Path)

        Dim oRoamFile As Windows.Storage.StorageFile
        Dim oODfile As ODfile = Nothing
        oODfile = Await App.mODroot.GetFileAsync(sFilename)
        If oODfile Is Nothing Then
            vb14.DumpMessage("tego pliku nie ma w OneDrive")
            Return False ' nie ma pliku w OneDrive, to go nie kopiujemy
        End If

        Dim oDTO As DateTimeOffset = oODfile.GetLastModDate

        If Not Await oDstFolder.FileExistsAsync(sFilename) Then
            vb14.DumpMessage("nie kopiuję - tego pliku nie ma lokalnie")
            oRoamFile = Await oDstFolder.CreateFileAsync(sFilename, Windows.Storage.CreationCollisionOption.ReplaceExisting)
        Else
            oRoamFile = Await oDstFolder.TryGetItemAsync(sFilename)
            Dim oRoamProp As Windows.Storage.FileProperties.BasicProperties = Await oRoamFile.GetBasicPropertiesAsync
            If oRoamProp.DateModified.AddSeconds(2) > oODfile.GetLastModDate Then
                vb14.DumpMessage("nie kopiuję bo lokalnie " & oRoamProp.DateModified.ToString & ", onedrive: " & oDTO.ToString)
                Return False ' plik w OneDrive jest starszy
            End If
            vb14.DumpMessage("kopiuję: lokalnie " & oRoamProp.DateModified.ToString & " < onedrive: " & oDTO.ToString)
        End If

        ' no to kopiujemy
        Using oStreamOneDrive = Await oODfile.GetStreamAsync
            Using oStreamRoaming = Await oRoamFile.OpenStreamForWriteAsync()
                oStreamOneDrive.CopyTo(oStreamRoaming)
                oStreamRoaming.Flush()
            End Using
        End Using

        ' Dim oFileProp As Windows.Storage.FileProperties.BasicProperties = Await oRoamFile.GetBasicPropertiesAsync
        ' oFileProp.DateModified = oODfile.GetLastModDate
        IO.File.SetLastAccessTime(oRoamFile.Path, New Date(oDTO.Ticks))

        Return True

    End Function

    Private Async Function CopyAllFilesFromOneDriveIfNewer() As Task
        Dim bODnewer As Boolean = Await CopyOneFileFromOneDriveIfNewer(App.GetJsonFolder, ListaSklepow._mFileNameBase)

        If bODnewer Then
            ' plik jest zaktualizowany - wczytaj go na nowo
            Await App.moSklepy.LoadAsync(False) ' nie z OneDrive, bo wlasnie sobie skopiowalismy aktualną wersję
        End If

        ' teraz kopiowanie obrazków kart
        For Each oSklep As VBlib_Karty.JedenSklep In App.moSklepy.GetList
            For Each oKarta As VBlib_Karty.JednaKarta In oSklep.lKarty
                ' karta może być bez obrazka, a z generowaniem z numeru - wtedy nie ma co kopiować
                If oKarta.sPicFilename <> "" Then
                    Await CopyOneFileFromOneDriveIfNewer(App.GetPickiFolder, oKarta.sPicFilename)
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
            vb14.DumpMessage("nie kopiuję - tego pliku nie ma w OneDrive")
            Await App.mODroot.CopyFileToOneDriveAsync(oRoamFile)
        Else
            Dim oRoamProp As Windows.Storage.FileProperties.BasicProperties = Await oRoamFile.GetBasicPropertiesAsync
            Dim oDTO As DateTimeOffset = oODfile.GetLastModDate
            If oRoamProp.DateModified < oODfile.GetLastModDate.AddSeconds(2) Then
                vb14.DumpMessage("nie kopiuję bo lokalnie " & oRoamProp.DateModified.ToString & ", onedrive: " & oDTO.ToString)
                Return ' plik w OneDrive jest nowszy
            End If

            vb14.DumpMessage("kopiuję: lokalnie " & oRoamProp.DateModified.ToString & " > onedrive: " & oDTO.ToString)
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

    Private Async Function SciagnijIkonki() As Task
        ' ściąga ikonki, których jeszcze nie mamy


        Dim bChanged As Boolean = False
        Dim sPath As String = Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path

        For Each oSklep As VBlib_Karty.JedenSklep In App.moSklepy.GetList
            bChanged = bChanged Or Await SciagnijIkonke(oSklep)
        Next

        If bChanged Then Await App.moSklepy.SaveAsync

    End Function

    ''' <summary>
    ''' Dla podanego sklepu ściąga ikonkę z WWW i daje do Cache
    ''' </summary>
    ''' <param name="oSklep"></param>
    ''' <returns></returns>
    Public Shared Async Function SciagnijIkonke(oSklep As VBlib_Karty.JedenSklep) As Task(Of Boolean)

        Dim bChanged As Boolean = False

        If oSklep.sIconUri = "" Then
            ' stara wersja
            oSklep.sIconUri = oSklep.sIconFilename
            oSklep.sIconFilename = ""
            bChanged = True
        End If

        If IO.File.Exists(oSklep.sIconFilename) Then Return bChanged

        ' aktualizacja ze starej wersji pliku - sprawdzam pole które jest nowe
        Dim sFilename As String = oSklep.sName.ToValidPath
        sFilename &= ".icon"
        ' operacje na nazwach
        If oSklep.sIconUri.ToLowerInvariant.Contains(".png") Then
            sFilename &= ".png"
        ElseIf oSklep.sIconUri.ToLowerInvariant.Contains(".ico") Then
            sFilename &= ".ico"
        Else
            ' biedronka nie ma ico/png, ale to jest .png - zresztą nazwa jest i tak chyba nieważna
            sFilename &= ".png"
        End If

        Dim sPath As String = Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path
        oSklep.sIconFilename = Await SaveUriAsFileAsync(oSklep.sIconUri, sPath, sFilename)

        Return True

    End Function


    Private Shared Async Function SaveUriAsFileAsync(sUri As String, sPath As String, sFileName As String) As Task(Of String)
        ' ściągnięcie ikonki, wedle oSklep.sIconUri, aktualizacja sIconFilename
        ' zwraca nazwę pliku lub "", gdy się nie udało

        Dim oFold As Windows.Storage.StorageFolder = Await Windows.Storage.StorageFolder.GetFolderFromPathAsync(sPath)
        Dim oFile As Windows.Storage.StorageFile

        If IO.File.Exists(IO.Path.Combine(sPath, sFileName)) Then
            oFile = Await oFold.TryGetItemAsync(sFileName)
        Else
            oFile = Await oFold.CreateFileAsync(sFileName)
        End If

        Dim oEngine As New Windows.Networking.BackgroundTransfer.BackgroundDownloader

        Dim oDown = oEngine.CreateDownload(New Uri(sUri), oFile)
        Await oDown.StartAsync
        Return oFile.Path
    End Function

#End Region

    Private Async Sub uiSync_Clicked(sender As Object, e As RoutedEventArgs)
        ' synchronizacja z OneDrive

        If Not pkar.NetIsIPavailable Then
            vb14.DialogBox("ale nie masz sieci...")
            Return
        End If

        Me.ProgRingShow(True)
        If App.mODroot Is Nothing Then App.mODroot = Await ODclient.GetRootAsync()
        Await CopyAllFilesFromOneDriveIfNewer()
        Await CopyAllFilesToOneDriveIfNewer()
        Await SciagnijIkonki()
        Me.ProgRingShow(False)

        PokazListe()

        ' sprawdź czy OD nie ma czegoś nowszego
        ' wyślij do OD wszystko
        ' aktualizacja zmiennej "lastSave"
    End Sub

    Private Async Sub uiGPS_Clicked(sender As Object, e As RoutedEventArgs)
        ' wybór sklepu według GPS

        ' w tym są permissiony
        Dim oPos As Windows.Devices.Geolocation.BasicGeoposition? = Await GetCurrentPointAsync(10)
        If oPos Is Nothing Then Return

        ' szukaj w App.moSklepy
        Dim lLista As New List(Of VBlib_Karty.JedenSklep)
        For Each oSklep In App.moSklepy.GetList
            For Each oLoc In oSklep.lLocations
                If oPos.Value.DistanceTo(oLoc.dLat, oLoc.dLon) < 50 Then
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
        ' *TODO* choćby on/off OneDrive, szybkość chodzenia (z czego wynika timeout cache GPS)
    End Sub
#End Region

    Private Shared Function GetMaxLocTime() As TimeSpan
        ' promień 100 metrów, przy podanej szybkości - ile sekund
        Dim iSpeed As Integer = vb14.GetSettingsInt("walkSpeed", 4)
        Dim dMetersPerSecond As Double = iSpeed * 1000 / 3600

        Return TimeSpan.FromSeconds(50 / dMetersPerSecond)

    End Function

    ''' <summary>
    ''' zwraca NULL lub współrzędne lokalizacji
    ''' </summary>
    ''' <param name="iSecTimeout"></param>
    ''' <returns></returns>
    Public Shared Async Function GetCurrentPointAsync(iSecTimeout As Integer) As Task(Of Windows.Devices.Geolocation.BasicGeoposition?)

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

        oDevGPS.DesiredAccuracyInMeters = vb14.GetSettingsInt("gpsPrec", 75) ' ; // dla 4 km/h; 100 m = 90 sec, 75 m = 67 sec
        Dim sErr As String = ""

        Try
            oPos = Await oDevGPS.GetGeopositionAsync(GetMaxLocTime, oTimeout)
            Return oPos.Coordinate.Point.Position

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