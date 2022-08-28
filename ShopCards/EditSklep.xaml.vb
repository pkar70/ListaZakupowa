Imports vb14 = Vblib.pkarlibmodule14
Imports Vblib.Extensions

Public NotInheritable Class EditSklep
    Inherits Page

    Private moItem As VBlib_Karty.JedenSklep
    Private mbAdding As Boolean = False

    Protected Overrides Sub onNavigatedTo(e As NavigationEventArgs)

        Dim sName As String = e.Parameter.ToString
        If String.IsNullOrEmpty(sName) Then
            moItem = New VBlib_Karty.JedenSklep
            mbAdding = True
        Else
            moItem = If(App.moSklepy.GetItem(sName), New VBlib_Karty.JedenSklep)
        End If

    End Sub

    Private Sub WypelnDefaultami(sName As String)
        moItem.sName = sName
        moItem.sIconFilename = ""
        moItem.sIconPathname = ""
        moItem.sIconUri = ""
        moItem.bJestShoplist = False    ' *TODO* na razie nieobsługiwane
        moItem.sSklepUrl = "https://www." & sName & ".pl"
        moItem.lKarty = New List(Of VBlib_Karty.JednaKarta)
        moItem.lLocations = New List(Of VBlib_Karty.JednaLocation)
    End Sub

    Private Sub ShowDane()
        ' pokazanie danych na ekranie (z moItem)

        uiNazwa.Text = moItem.sName
        uiIkonka.Text = moItem.sIconUri
        uiUrl.Text = moItem.sSklepUrl
        uiUseZakupy.IsChecked = moItem.bJestShoplist
        uiListaKart.ItemsSource = Nothing ' bez tego nie odświeża po dodaniu karty
        uiListaKart.ItemsSource = moItem.lKarty
        uiListaLokalizacji.ItemsSource = Nothing
        uiListaLokalizacji.ItemsSource = moItem.lLocations


        Dim sIkonka As String = moItem.sIconPathname
        If sIkonka = "" Then sIkonka = moItem.sIconUri

        If Not String.IsNullOrEmpty(sIkonka) Then
            Try
                uiIkonkaPic.Source = New BitmapImage(New Uri(sIkonka))
                uiIkonkaPic.MaxHeight = 60
                uiIkonkaPic.MaxWidth = 60
                uiIkonkaPic.Stretch = Stretch.Uniform
                uiIkonkaPic.Visibility = Visibility.Visible
            Catch ex As Exception
                uiIkonkaPic.Visibility = Visibility.Collapsed
            End Try
        End If

    End Sub

    Private Async Sub uiOK_Click(sender As Object, e As RoutedEventArgs)
        moItem.sName = uiNazwa.Text
        moItem.sIconUri = uiIkonka.Text
        moItem.sSklepUrl = uiUrl.Text
        moItem.bJestShoplist = uiUseZakupy.IsChecked

        If mbAdding Then App.moSklepy.Add(moItem)

        Me.ProgRingShow(True)
        Await App.moSklepy.SciagnijIkonke(moItem)
        Me.ProgRingShow(False)

        Await App.moSklepy.SaveAsync

        Me.GoBack
    End Sub

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Me.ProgRingInit(True, False)

        ' ZXing.Net.Mobile.Forms.WindowsUniversal.Platform.Init()

        If App.mbInScanCode = True Then
            Await vb14.DialogBoxAsync("bad return from scanning")
        End If

        If String.IsNullOrEmpty(moItem.sName) Then
            Dim sName As String = Await vb14.DialogBoxInputAllDirectAsync("Podaj nazwę sklepu:", "", "OK", "Cancel")
            If sName = "" Then Me.GoBack
            WypelnDefaultami(sName)
        End If

        ' teraz już mamy w miarę OK dane
        ShowDane()

    End Sub

    Private Sub uiTryWeb_Click(sender As Object, e As RoutedEventArgs)
        Dim oUri As New Uri(uiUrl.Text)
        oUri.OpenBrowser
    End Sub

    Private Async Function ZnajdzLinkIkonki(sUri As String) As Task(Of String)

        Dim oUri As New Uri(sUri)
        Dim sPage As String = Await vb14.HttpPageAsync(oUri)

        If sPage = "" Then Return ""

        Dim oHtmlDoc As New HtmlAgilityPack.HtmlDocument()
        oHtmlDoc.LoadHtml(sPage)

        Dim oHead As HtmlAgilityPack.HtmlNode = oHtmlDoc.DocumentNode.SelectSingleNode("//head")
        If oHead Is Nothing Then Return ""

        Dim iDocelowe As Integer = 60
        Dim sBestLink As String = ""
        Dim iBestSize As Integer = 1000


        Dim oNodes As HtmlAgilityPack.HtmlNodeCollection = oHead.SelectNodes("//link")
        If oNodes IsNot Nothing Then

            ' 1) najpierw wedle wielkości, celuj w iDocelowe
            For Each oLink As HtmlAgilityPack.HtmlNode In oNodes
                If oLink.Attributes("rel").Value.ToLower.Contains("icon") Then
                    Dim sSizes As String = oLink.Attributes("sizes")?.Value

                    If Not String.IsNullOrEmpty(sSizes) Then
                        Dim iInd As Integer = sSizes.IndexOf("x")
                        If iInd > 0 Then sSizes = sSizes.Substring(0, iInd)
                        Dim iCurrSize As Integer
                        If Integer.TryParse(sSizes, iCurrSize) Then
                            If Math.Abs(iBestSize - iDocelowe) > Math.Abs(iCurrSize - iDocelowe) Then
                                sBestLink = oLink.Attributes("href").Value
                                If Not sBestLink.StartsWith("http") Then
                                    sBestLink = oUri.Scheme & "://" & oUri.Host & "/" & sBestLink
                                End If
                                iBestSize = iCurrSize
                            End If
                        End If
                    End If
                End If
            Next


            ' 2) skoro nie ma wedle wielkości, to pierwsze co znajdzie
            If sBestLink <> "" Then Return sBestLink
            For Each oLink As HtmlAgilityPack.HtmlNode In oNodes
                If oLink.Attributes("rel").Value.ToLower.Contains("icon") Then
                    sBestLink = oLink.Attributes("href").Value
                    If Not sBestLink.StartsWith("http") Then
                        sBestLink = oUri.Scheme & "://" & oUri.Host & "/" & sBestLink
                    End If
                    Return sBestLink
                End If
            Next

        End If

        ' 3) na razie nic, to próbujemy czy istnieje favicon (case: Castorama)
        Dim oUriFallBack As New Uri(oUri.Scheme & "://" & oUri.Host & "/favicon.ico")
        sPage = Await vb14.HttpPageAsync(oUriFallBack)
        If Not String.IsNullOrEmpty(sPage) AndAlso sPage.Length > 8192 Then Return oUriFallBack.ToString

        ' LIDL
        ' <link rel="icon" type="image/x-icon" href="/cdn/assets/logos/1.0.1/favicon.ico">
        ' <link rel="icon" type="image/png" sizes="96x96" href="/cdn/assets/logos/1.0.1/favicon-96x96.png">

        ' GOODLOOD
        ' <link rel="icon" href="https://d1paendm4c66vp.cloudfront.net/uploads/cropped-favi-32x32.png" sizes="32x32">

        ' Rossman
        ' <link rel="SHORTCUT ICON" href="https://www.ros.net.pl/Portals/0/favicon.ico" type="image/x-icon">
        ' <link rel="apple-touch-icon" href="/_next/static/public/6629fe9ad072a9420fb229b1f25aa983.png" sizes="120x120">

        ' stokrotka
        ' <link rel="shortcut icon" href="https://stokrotka.pl/template/default/favicon_stokrotka.ico">
        Return ""
    End Function

    Private Async Sub uiTryWebIcon_Click(sender As Object, e As RoutedEventArgs)
        ' wczytanie strony glownej, wyszukanie w HEAD ikony

        Me.ProgRingShow(True)
        Dim sLink As String = Await ZnajdzLinkIkonki(uiUrl.Text)
        Me.ProgRingShow(False)
        If sLink = "" Then Return
        moItem.sIconUri = sLink

        ShowDane()
    End Sub


    Private Sub uiTryRemSys_Click(sender As Object, e As RoutedEventArgs)
        ' *TODO* sprawdzanie czy jest RemSys do ListaZakupowe dostępne
    End Sub

#Region "dodawanie lokalizacji"
    Private Sub uiLocatMap_Click(sender As Object, e As RoutedEventArgs)
        ' dodaj przez wybranie z mapy
        Me.Navigate(GetType(ShowMap), moItem.sName)

    End Sub

    Private Async Sub uiLocatGPS_Click(sender As Object, e As RoutedEventArgs)
        ' dodaj przez GPS

        ' w tym są permissiony
        Dim oPos As Windows.Devices.Geolocation.BasicGeoposition? = Await MainPage.GetCurrentPointAsync(15)
        If oPos Is Nothing Then Return

        Dim oNew As New VBlib_Karty.JednaLocation
        oNew.dLat = oPos.Value.Latitude
        oNew.dLon = oPos.Value.Longitude

        oNew.sName = Await vb14.DialogBoxInputAllDirectAsync("Podaj nazwę dla punktu:")
        If oNew.sName = "" Then Return

        moItem.lLocations.Add(oNew)

    End Sub
#End Region

#Region "dodawanie karty"

    Private Async Sub uiKartaScan_Click(sender As Object, e As RoutedEventArgs)
        ' zeskanuj do numeru, a potem - jak z numerem

        Await ScanBarCode()

    End Sub

    Private Async Sub uiKartaNumer_Click(sender As Object, e As RoutedEventArgs)
        ' zapytaj o numer, typ kodu (paskowy, QR) i dodaj do zmiennej

        ' *TODO* możliwość wyboru typu kodu paskowego

        Dim sNumber As String = Await vb14.DialogBoxInputAllDirectAsync("Podaj numer (EAN_13):", "", "Ok", "Cancel")
        If sNumber = "" Then Return

        Await DodajKarteWedleKodu(moItem, ZXing.BarcodeFormat.EAN_13, sNumber)

        ShowDane()

    End Sub

    Private Async Sub uiKartaFotka_Click(sender As Object, e As RoutedEventArgs)
        ' zrób fotkę, zapisz

        Dim captureUI As New Windows.Media.Capture.CameraCaptureUI
        captureUI.PhotoSettings.Format = Windows.Media.Capture.CameraCaptureUIPhotoFormat.Jpeg
        ' captureUI.PhotoSettings.CroppedSizeInPixels = New Size(200, 200)

        Dim oPhotoFile As Windows.Storage.StorageFile = Await captureUI.CaptureFileAsync(Windows.Media.Capture.CameraCaptureUIMode.Photo)
        If oPhotoFile Is Nothing Then Return


        ' zapytaj też o nazwę (czyja karta)
        Dim sCzyja As String = Await vb14.DialogBoxInputAllDirectAsync("Czy to czyjaś karta?", "", "Tak", "Moja!")

        ' sprawdz czy karta + osoba juz istnieje
        For Each oCurrKarta As VBlib_Karty.JednaKarta In moItem.lKarty
            If oCurrKarta.sCzyja = sCzyja Then
                If Not Await vb14.DialogBoxYNAsync("Już jest taka karta dla tej osoby, overwrite?") Then Return
            End If
        Next

        ' kopiuj plik do siebie, w takie miejsce zeby potem Image potrafilo wziac :)
        Dim sFilename As String = moItem.sName
        If sCzyja <> "" Then sFilename = sFilename & "." & sCzyja
        sFilename = sFilename & ".jpg"
        sFilename = sFilename.ToValidPath

        Await oPhotoFile.CopyAsync(App.GetPickiFolder, sFilename, Windows.Storage.NameCollisionOption.ReplaceExisting)
        Await oPhotoFile.DeleteAsync()

        Dim oKarta As New VBlib_Karty.JednaKarta
        oKarta.sCzyja = sCzyja
        oKarta.sPicFilename = sFilename

        moItem.lKarty.Add(oKarta)

        ShowDane()

        ' zapytaj też o nazwę (czyja karta)
        'uiCameraFlyout.ShowAt(sender)
        'KamerkaStart()
    End Sub



    Private Async Sub uiKartaBrowse_Click(sender As Object, e As RoutedEventArgs)
        ' wybierz istniejącą fotkę, skopiuj

        Dim picker As New Windows.Storage.Pickers.FileOpenPicker()
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
        picker.FileTypeFilter.Add(".jpg")
        picker.FileTypeFilter.Add(".jpeg")
        picker.FileTypeFilter.Add(".png")

        Dim oFile As Windows.Storage.StorageFile
        oFile = Await picker.PickSingleFileAsync
        If oFile Is Nothing Then Exit Sub

        ' zapytaj też o nazwę (czyja karta)
        Dim sCzyja As String = Await vb14.DialogBoxInputAllDirectAsync("Czy to czyjaś karta?", "", "Tak", "Moja!")

        ' sprawdz czy karta + osoba juz istnieje
        For Each oCurrKarta As VBlib_Karty.JednaKarta In moItem.lKarty
            If oCurrKarta.sCzyja = sCzyja Then
                If Not Await vb14.DialogBoxYNAsync("Już jest taka karta dla tej osoby, overwrite?") Then Return
            End If
        Next

        ' kopiuj plik do siebie, w takie miejsce zeby potem Image potrafilo wziac :)
        Dim sFilename As String = moItem.sName
        If sCzyja <> "" Then sFilename = sFilename & "." & sCzyja
        sFilename = sFilename & oFile.FileType  ' FileType ma w sobie kropkę!
        sFilename = sFilename.ToValidPath

        Await oFile.CopyAsync(App.GetPickiFolder, sFilename, Windows.Storage.NameCollisionOption.ReplaceExisting)

        Dim oKarta As New VBlib_Karty.JednaKarta
        oKarta.sCzyja = sCzyja
        oKarta.sPicFilename = sFilename

        moItem.lKarty.Add(oKarta)

        ShowDane()
    End Sub

    Private Sub uiKartaImport_Click(sender As Object, e As RoutedEventArgs)
        ' *TODO* jakoś wczytywanie z email, linku OneDrive, itp.
        ' zapytaj też o nazwę (czyja karta)
    End Sub

#End Region

    '#Region "kamerka podgląd"

    '    Dim moDisplayRequest As Windows.System.Display.DisplayRequest
    '    Dim moMediaCapture As Windows.Media.Capture.MediaCapture
    '    Dim mbStarted As Boolean = False
    '    Dim miObrot0 As Integer = 90


    '    ' kopia z Lupka
    '    Private Async Sub KamerkaStop()
    '        Try
    '            Await moMediaCapture.StopPreviewAsync
    '            moDisplayRequest.RequestRelease()
    '            uiCameraCapture.Source = Nothing
    '            moMediaCapture.Dispose()
    '            moDisplayRequest = Nothing
    '        Catch ex As Exception

    '        End Try
    '    End Sub

    '    Private Async Sub KamerkaStart()
    '        mbStarted = False

    '        Try

    '            moDisplayRequest = New Windows.System.Display.DisplayRequest
    '            moMediaCapture = New Windows.Media.Capture.MediaCapture

    '            Dim allVideoDevices As DeviceInformationCollection = Await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)
    '            For Each oVideo As DeviceInformation In allVideoDevices
    '                Dim oMItem As ToggleMenuFlyoutItem = New ToggleMenuFlyoutItem
    '                oMItem.Text = oVideo.Name
    '                If msCameraId = "" Then msCameraId = oVideo.Id
    '                If bFirst Then
    '                    If oVideo.EnclosureLocation IsNot Nothing AndAlso oVideo.EnclosureLocation.Panel = Windows.Devices.Enumeration.Panel.Back Then
    '                        bFirst = False
    '                        If msCameraId <> "" Then
    '                            For Each oOldItems As MenuFlyoutItemBase In uiFlyoutCameras.Items
    '                                TryCast(oOldItems, ToggleMenuFlyoutItem).IsChecked = False
    '                            Next
    '                        End If
    '                        msCameraId = oVideo.Id
    '                        oMItem.IsChecked = True
    '                    End If
    '                End If
    '                oMItem.AddHandler(TappedEvent, New TappedEventHandler(AddressOf MenuKamerekClick), False)
    '                uiFlyoutCameras.Items.Add(oMItem)
    '            Next



    '            Dim settings As New Windows.Media.Capture.MediaCaptureInitializationSettings
    '            ' settings.VideoDeviceId = msCameraId
    '            settings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Video  ' bez μfonu

    '            Await moMediaCapture.InitializeAsync(settings)

    '            Dim iMaxX As Integer = 0
    '            Dim iMaxY As Integer = 0
    '            Dim oMEP As Windows.Media.MediaProperties.IMediaEncodingProperties = Nothing
    '            For Each oSP As Windows.Media.MediaProperties.IMediaEncodingProperties In moMediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(Windows.Media.Capture.MediaStreamType.VideoPreview)
    '                With TryCast(oSP, Windows.Media.MediaProperties.VideoEncodingProperties)
    '                    Dim iTmp As Integer = .Width
    '                    If iTmp > iMaxX Then
    '                        oMEP = oSP
    '                        iMaxX = iTmp
    '                        iMaxY = .Height
    '                    End If
    '                End With
    '            Next
    '            If iMaxX > 0 Then Await moMediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(Windows.Media.Capture.MediaStreamType.VideoPreview, oMEP)
    '            ' Lumia 532: 640x480 oraz 800x532
    '            Dim dMinZoom As Double
    '            'If sWybrana.IndexOf("Back") > 0 Then miObrot0 = 90
    '            'If sWybrana.IndexOf("Front") > 0 Then miObrot0 = 270

    '            Select Case miObrot0
    '                Case 0, 180
    '                    dMinZoom = Math.Min(uiCameraScroll.ActualWidth / iMaxX, uiCameraScroll.ActualHeight / iMaxY)
    '                Case 90, 270
    '                    dMinZoom = Math.Min(uiCameraScroll.ActualWidth / iMaxY, uiCameraScroll.ActualHeight / iMaxX)
    '            End Select
    '            uiCameraScroll.MinZoomFactor = dMinZoom

    '            moDisplayRequest.RequestActive()
    '            uiCameraCapture.Source = moMediaCapture

    '            Await moMediaCapture.StartPreviewAsync()

    '            ' Await KamerkaObrot(0)
    '        Catch ex As Exception
    '            mbStarted = False
    '        End Try

    '        If Not mbStarted Then
    '            vb14.DialogBox("Cannot start capture")
    '        End If
    '    End Sub

    '    Protected Overloads Sub OnNavigatedFrom(e As NavigationEventArgs)
    '        KamerkaStop()
    '    End Sub

    '    Private Async Function Kapturnij() As Task(Of String)
    '        ' zrób fotkę

    '        Dim myPictures As Windows.Storage.StorageFolder = App.GetPickiFolder

    '        Dim sFName As String = "Lupka_" & Date.Now.ToString("yyyyMMdd-HHmmss") & ".jpg"
    '        Dim oFile As Windows.Storage.StorageFile = Await myPictures.CreateFileAsync(sFName)
    '        sFName = oFile.Name ' bo jakby zadzialalo GenerateUniq, to nie bedzie takie jak dwie linijki wyzej
    '        'oFile path: d:\pictures\lupka\...

    '        Dim captureStream As New Windows.Storage.Streams.InMemoryRandomAccessStream
    '        ' ponizsza funkcja robi dzwiek migawki
    '        ' jednak nie JPG, bo skoro potem przekodowujemy, to po co kompresowac, jak zaraz bitmapa bedzie potrzebna?
    '        Await moMediaCapture.CapturePhotoToStreamAsync(Windows.Media.MediaProperties.ImageEncodingProperties.CreateBmp(), captureStream)
    '        Dim decoder As Windows.Graphics.Imaging.BitmapDecoder = Await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(captureStream)
    '        Dim fileStream As Windows.Storage.Streams.IRandomAccessStream = Await oFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite)
    '        Dim encoder As Windows.Graphics.Imaging.BitmapEncoder = Await Windows.Graphics.Imaging.BitmapEncoder.CreateForTranscodingAsync(fileStream, decoder)

    '        'BitmapTransform w tej kolejnosci robi: scale, flip, rotation, crop

    '        ' 1: obrót
    '        Dim iObrot As Integer = miObrot0
    '        If iObrot = 360 Then iObrot = 0
    '        Select Case iObrot
    '            Case 0
    '                encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.None
    '            Case 90
    '                encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees
    '            Case 180
    '                encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise180Degrees
    '            Case 270
    '                encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise270Degrees
    '        End Select

    '        ' limit powinien byc z uiScroll.MinZoom, ale na wszelki wypadek
    '        ' normalnie: Actual = 320x456
    '        Dim dZoomH As Double = Math.Min(1, uiCameraScroll.ActualHeight / uiCameraScroll.ExtentHeight)
    '        Dim dZoomW As Double = Math.Min(1, uiCameraScroll.ActualWidth / uiCameraScroll.ExtentWidth)

    '        ' ewentualne polozenie obrazka
    '        ' normalnie: Pixel = 1936x2592 
    '        Dim iPicH, iPicW As Integer ' obrocenie x i y z decoder
    '        Dim iW, iH As Integer       ' wielkosc obszaru (zoom)
    '        Select Case iObrot
    '            Case 0, 180
    '                iPicH = decoder.PixelHeight
    '                iPicW = decoder.PixelWidth
    '                iW = iPicH * dZoomW
    '                iH = iPicW * dZoomH
    '            Case 90, 270
    '                iPicH = decoder.PixelWidth
    '                iPicW = decoder.PixelHeight
    '                iW = iPicW * dZoomW
    '                iH = iPicH * dZoomH
    '        End Select

    '        Dim iMaxX, iMaxY As Integer ' limity wielkosci (zeby nie bylo crop poza zakresem)
    '        iMaxX = iPicW   ' limity
    '        iMaxY = iPicH

    '        Dim iX, iY As Integer ' poczatek obszaru (po obrocie, wiec tak jak na ekranie)
    '        iX = iPicW * uiCameraScroll.HorizontalOffset / uiCameraScroll.ExtentWidth
    '        iY = iPicH * uiCameraScroll.VerticalOffset / uiCameraScroll.ExtentHeight

    '        ' https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/basic-photo-video-and-audio-capture-with-mediacapture
    '        ' https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/imaging
    '        'iW = 0
    '        If iW <> 0 Then
    '            iX = Math.Min(iMaxX - 10, Math.Abs(iX))
    '            iY = Math.Min(iMaxY - 10, Math.Abs(iY))
    '            iW = Math.Max(0, Math.Min(iMaxX - iX, iW))
    '            iH = Math.Max(0, Math.Min(iMaxY - iY, iH))
    '            ' This rectangle is defined in the coordinate space after scale, rotation, and flip are applied
    '            Dim oBbound As New Windows.Graphics.Imaging.BitmapBounds With {.X = iX, .Y = iY, .Width = iW, .Height = iH}
    '            encoder.BitmapTransform.Bounds = oBbound
    '        End If

    '        ' You could use the scrollviewer (uiScroll) data ExtentWidth/ExtentHeight to figure out the sizes, and HorizontalOffset and VerticalOffset to figure out the position of the viewable area
    '        Try
    '            Await encoder.FlushAsync()
    '            Await fileStream.FlushAsync
    '        Catch ex As Exception
    '            ' nie psuj jak sie nie uda - bo jak wyleci, to bedzie zielone a nie kamerka
    '            sFName = ""
    '        End Try

    '        ' takie rozbicie, zeby Dispose bylo takze wtedy gdy Catch poprzednie bedzie na Flush
    '        Try
    '            fileStream.Dispose()
    '        Catch ex As Exception
    '            ' nie psuj jak sie nie uda - bo jak wyleci, to bedzie zielone a nie kamerka
    '            sFName = ""
    '        End Try


    '        Return sFName   ' empty z errora, a inaczej - filename
    '    End Function

    '    Private Sub uiCameraMigawka_Click(sender As Object, e As RoutedEventArgs) Handles uiCameraMigawka.Click
    '        Kapturnij()
    '    End Sub

    '#End Region

    Private Async Function ScanBarCode() As Task
        ' tu bedzie najciekawsze :)
        ' Camera-based
        'https://github.com/Microsoft/Windows-universal-samples/tree/master/Samples/BarcodeScanner
        ' nieprawda, jest tylko gdy skaner ma mozliwosc preview
        'Dim oWatch = Windows.Devices.Enumeration.DeviceInformation.CreateWatcher(Windows.Devices.PointOfService.BarcodeScanner.GetDeviceSelector())
        'Dim oList = Await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.PointOfService.BarcodeScanner.GetDeviceSelector())
        '        Dim oScanColl As Collection(Of Windows.Devices.PointOfService.BarcodeScanner.BarcodeScannerInfo)

        'Github biblioteka
        'https://github.com/nblockchain/ZXing.Net.Xamarin

        App.mbInScanCode = True

        Dim oScanner As New ZXing.Mobile.MobileBarcodeScanner(Me.Dispatcher)
        'Tell our scanner to use the default overlay 
        oScanner.UseCustomOverlay = False
        ' //We can customize the top And bottom text of our default overlay 
        oScanner.TopText = "Hold camera up to barcode"
        oScanner.BottomText = "Camera will automatically scan barcode" & vbCrLf & "Press the 'Back' button to Cancel"

        Dim oRes As ZXing.Result = Nothing
        Dim sMsg As String = ""
        oRes = Await oScanner.Scan()

        'If sMsg <> "" Then
        '    vb14.DialogBox("CATCH z oScanner.Scan: " & sMsg)
        '    Return
        'End If

        If oRes Is Nothing Then
            vb14.DialogBox("oScanner.Scan returned NULL")
            Return
        End If

        ' bo mi źle zeskanowało Lewiatana, więc lepiej się upewniać
        Dim sPytanie As String

        sPytanie = $"Sprawdź numer... ({oRes.BarcodeFormat})"
        'If sMsg <> "" Then
        '    vb14.DialogBox("CATCH z sPytanie: " & sMsg)
        '    Return
        'End If

        Dim sNumber As String = Await vb14.DialogBoxInputAllDirectAsync(sPytanie, oRes.Text, "Ok", "Cancel")
        If sNumber = "" Then Return

        Await DodajKarteWedleKodu(moItem, oRes.BarcodeFormat, sNumber)

        ShowDane()

        App.mbInScanCode = False


    End Function

    Public Shared Async Function DodajKarteWedleKodu(oItem As VBlib_Karty.JedenSklep, iCodeType As Integer, sNumber As String, Optional bCzyjas As Boolean = False) As Task
        Dim oNew As New VBlib_Karty.JednaKarta
        oNew.iCodeType = iCodeType

        oNew.sNumber = sNumber

        ' zapytaj też o nazwę (czyja karta)
        Dim sCzyja As String
        If bCzyjas Then
            sCzyja = Await vb14.DialogBoxInputAllDirectAsync("Czyja to karta?", "", "Ok", "Cancel")
        Else
            sCzyja = Await vb14.DialogBoxInputAllDirectAsync("Czy to czyjaś karta?", "", "Tak", "Moja!")
        End If

        If bCzyjas AndAlso sCzyja = "" Then
            vb14.DialogBox("Karta wczytywana z zewnątrz nie może być własną (bez nazwy")
            Return
        End If

        ' sprawdz czy karta + osoba juz istnieje
        For Each oCurrKarta As VBlib_Karty.JednaKarta In oItem.lKarty
            If oCurrKarta.sCzyja = sCzyja Then
                If Not Await vb14.DialogBoxYNAsync("Już jest taka karta dla tej osoby, overwrite?") Then Return
            End If
        Next

        ' sprawdz czy karta + osoba juz istnieje
        For Each oCurrKarta As VBlib_Karty.JednaKarta In oItem.lKarty
            If oCurrKarta.sNumber = sNumber Then
                If Not Await vb14.DialogBoxYNAsync($"Karta o tym numerze już jest ({oCurrKarta.sCzyja}), overwrite?") Then Return
            End If
        Next


        oNew.sCzyja = sCzyja

        oItem.lKarty.Add(oNew)

    End Function


#Region "context na karcie"
    Private Async Sub uiDelKarta_Click(sender As Object, e As RoutedEventArgs)
        ' ask, i usunięcie karty
        Dim oFE As FrameworkElement = TryCast(sender, FrameworkElement)
        Dim oKarta As VBlib_Karty.JednaKarta = TryCast(oFE?.DataContext, VBlib_Karty.JednaKarta)
        If oKarta Is Nothing Then Return

        If Not Await vb14.DialogBoxYNAsync("Na pewno usunąć kartę " & oKarta.sCzyja & "?") Then Return

        If oKarta.sPicFilename <> "" Then
            If Await vb14.DialogBoxYNAsync("Usunąć jej obrazek?") Then
                IO.File.Delete(oKarta.sPicFilename)
            End If

        End If

        moItem.lKarty.Remove(oKarta)
        ShowDane()

    End Sub

    Private msLink As String = ""   ' do Share
    Private Sub uiSendKarta_Click(sender As Object, e As RoutedEventArgs)
        ' wysłanie karty komuś (jeszcze nie wiem jak)

        Dim oFE As FrameworkElement = TryCast(sender, FrameworkElement)
        Dim oKarta As VBlib_Karty.JednaKarta = TryCast(oFE?.DataContext, VBlib_Karty.JednaKarta)
        If oKarta Is Nothing Then Return

        If oKarta.sNumber = "" Then
            vb14.DialogBox("Nie umiem wysłać karty obrazkowej") ' *TODO*
            Return
        End If

        ' https://inthehand.com/2015/08/20/add-sharing-to-your-uwp-app/
        AddHandler DataTransfer.DataTransferManager.GetForCurrentView.DataRequested, AddressOf SzarnijDane
        DataTransfer.DataTransferManager.ShowShareUI()


        msLink = $"shcard://card?s={moItem.sName}&n={oKarta.sNumber}&t={oKarta.iCodeType}"

    End Sub

    Private Sub SzarnijDane(sender As DataTransfer.DataTransferManager, args As DataTransfer.DataRequestedEventArgs)
        ' jeszcze nie mam nic do sharowania - błąd wywołania?
        If String.IsNullOrEmpty(msLink) Then
            args.Request.FailWithDisplayText("Nothing to share")
            Return
        End If

        args.Request.Data.SetHtmlFormat(msLink)
        args.Request.Data.Properties.Title = Windows.ApplicationModel.Package.Current.DisplayName

    End Sub


    Private Sub uiShowKarta_Click(sender As Object, e As RoutedEventArgs)
        Dim oFE As FrameworkElement = TryCast(sender, FrameworkElement)
        Dim oKarta As VBlib_Karty.JednaKarta = TryCast(oFE?.DataContext, VBlib_Karty.JednaKarta)
        If oKarta Is Nothing Then Return

        Me.Navigate(GetType(KartySklepu), moItem.sName & "|" & oKarta.sCzyja)
    End Sub

#End Region

#Region "context na lokalizacji"
    Private Async Sub uiShowOnMap_Click(sender As Object, e As RoutedEventArgs)
        Dim oFE As FrameworkElement = TryCast(sender, FrameworkElement)
        Dim oLoc As VBlib_Karty.JednaLocation = TryCast(oFE?.DataContext, VBlib_Karty.JednaLocation)
        If oLoc Is Nothing Then Return

        'Await App.moSklepy.SaveAsync ' zapisz zmiany jak były, bo powrót będzie miał utratę danych chyba?
        Me.Navigate(GetType(ShowMap), oLoc)
    End Sub

    Private Sub uiDelLoc_Click(sender As Object, e As RoutedEventArgs)
        Dim oFE As FrameworkElement = TryCast(sender, FrameworkElement)
        Dim oLoc As VBlib_Karty.JednaLocation = TryCast(oFE?.DataContext, VBlib_Karty.JednaLocation)
        If oLoc Is Nothing Then Return

        moItem.lLocations.Remove(oLoc)
        ShowDane()

    End Sub


#End Region
End Class


Public Class KonwersjaKartaName
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        Dim sTmp As String = CType(value, String)
        If Not String.IsNullOrEmpty(sTmp) Then Return sTmp
        Return "(mine)"     ' '*TODO* wedle GetLangString 
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class

Public Class KonwersjaKartaSend
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.Convert
        Dim sTmp As String = CType(value, String)
        If String.IsNullOrEmpty(sTmp) Then Return False
        Return True
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, language As String) As Object Implements IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class