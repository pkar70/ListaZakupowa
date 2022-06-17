' powtorka z PrzypomnijTu
' H:\Home\PIOTR\VStudio\_Vs2017\PrzypomnijTu\PrzypomnijTu\AddGeofence.xaml

' 1) pokazanie na mapie istniejącego punktu (read-only)
' 2) wybór punktu

' na wejściu ma (1) "SHOW|lon|Lat" (albo: BasicGeoposition)
' albo: (2) sName, do którego dopiąć punkt


' *TODO* GetSettings jaka mapa (OSM, itp.)?
' *TODO* Find w OSM: określenie terenu jako CurrPoint z radius 25 km?

Imports vb14 = Vblib.pkarlibmodule14
Imports Vblib.Extensions


Public NotInheritable Class ShowMap
    Inherits Page

    Private msSklepName As String
    Private moLocation As VBlib_Karty.JednaLocation
    Private mbAdding As Boolean = False

    Protected Overrides Sub onNavigatedTo(e As NavigationEventArgs)

        If e.Parameter Is Nothing Then Return

        If TypeOf e.Parameter Is VBlib_Karty.JednaLocation Then
            moLocation = e.Parameter
            mbAdding = False
        Else
            ' zakładamy że string
            msSklepName = e.Parameter.ToString
            mbAdding = True
        End If

    End Sub


    Private Async Sub uiGetGPS_Click(sender As Object, e As RoutedEventArgs)
        vb14.DumpCurrMethod()
        ProgRingShow(True)
        Dim oPos As Windows.Devices.Geolocation.BasicGeoposition = Await MainPage.GetCurrentPointAsync(10)  ' lub default
        ProgRingShow(False)

        uiLat.Text = oPos.Latitude
        uiLon.Text = oPos.Longitude
        uiMapka.Center = New Windows.Devices.Geolocation.Geopoint(oPos)
    End Sub

    Private Async Sub uiAdd_Click(sender As Object, e As RoutedEventArgs)
        vb14.DumpCurrMethod()

        If moLocation IsNot Nothing Then Return   ' to było tylko pokazywanie, więc nic nie zapisujemy

        ' testy nazwy
        If uiNazwa.Text.Length < 2 Then
            vb14.DialogBox("ale jednak jakas nazwa jest potrzebna")
            Return
        End If
        If uiNazwa.Text.Length > 60 Then
            vb14.DialogBox("za dluga nazwa!")
            Return
        End If

        Dim oItem As VBlib_Karty.JedenSklep = App.moSklepy.GetItem(msSklepName)
        If oItem Is Nothing Then
            vb14.DialogBox("nie ma sklepu który był wywołaniem")
            Return
        End If

        For Each oLoc As VBlib_Karty.JednaLocation In oItem.lLocations
            If oLoc.sName = uiNazwa.Text Then
                vb14.DialogBox("taka nazwa już istnieje")
                Return
            End If
        Next

        Dim oNew As New VBlib_Karty.JednaLocation
        oNew.sName = uiNazwa.Text
        oNew.dLat = SprobujParseLat(uiLat.Text)
        oNew.dLon = SprobujParseLon(uiLon.Text)

        oItem.lLocations.Add(oNew)

        Await App.moSklepy.SaveAsync
        Me.GoBack()
    End Sub

    Private Sub TrySetMapCenter(oBGeo As Windows.Devices.Geolocation.BasicGeoposition)
        uiMapka.Center = New Windows.Devices.Geolocation.Geopoint(oBGeo)
    End Sub
    Private Sub TrySetMapCenter(dLat As Double, dLon As Double)
        Dim oBGeo As Windows.Devices.Geolocation.BasicGeoposition = New Windows.Devices.Geolocation.BasicGeoposition
        oBGeo.Latitude = dLat
        oBGeo.Longitude = dLon
        TrySetMapCenter(oBGeo)
    End Sub
    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        vb14.DumpCurrMethod()

        If moLocation IsNot Nothing Then
            ' pokaż na mapie
            uiTitle.Text = "Punkt"
            uiGridFields.Visibility = Visibility.Collapsed
            uiNazwa.IsReadOnly = True
            uiSaveSymbol.Symbol = Symbol.Accept
            TrySetMapCenter(moLocation.dLat, moLocation.dLon)
        Else
            uiTitle.Text = "Dodawanie punktu"
            uiGridFields.Visibility = Visibility.Visible
            uiNazwa.IsReadOnly = False
            uiSaveSymbol.Symbol = Symbol.Add
            Await SprobujZClip()
        End If

        ProgRingInit(True, False)
    End Sub

    Private Function SprobujParseLon(sLon As String) As Double
        vb14.DumpCurrMethod()
        Dim dTemp As Double

        If Not Double.TryParse(sLon, dTemp) Then Return 0
        Return dTemp.Between(0, 360)

    End Function
    Private Function SprobujParseLat(sLat As String) As Double
        vb14.DumpCurrMethod()
        Dim dTemp As Double

        If Not Double.TryParse(sLat, dTemp) Then Return 0
        Return dTemp.Between(-180, 180)
    End Function

    Private Function SprobujParseFi(sFi As String) As Double
        vb14.DumpCurrMethod()
        Dim dTemp As Double

        If Not Double.TryParse(sFi, dTemp) Then Return 0
        Return Math.Max(dTemp, 10)
    End Function

    Private Function SprobujParseZwloka(sFi As String) As Double
        vb14.DumpCurrMethod()
        Dim dTemp As Double

        If Not Double.TryParse(sFi, dTemp) Then Return 0
        Return dTemp.Between(10, 60 * 60)
    End Function

    Private Sub SprobujParse(sLat As String, sLon As String)
        vb14.DumpCurrMethod()
        uiLat.Text = SprobujParseLat(sLat)
        uiLon.Text = SprobujParseLon(sLon)
    End Sub

    Private Async Function SprobujZClip() As Task(Of Boolean)
        vb14.DumpCurrMethod()
        Dim sTxt As String = Await ClipGetAsync()
        Dim iInd As Integer

        If sTxt.StartsWith("https://www.openstreetmap.org/#map=") Then
            'https://www.openstreetmap.org/#map=16/50.0485/19.9155
            sTxt = sTxt.Substring("https://www.openstreetmap.org/#map=".Length)
            iInd = sTxt.IndexOf("/")
            If iInd < 0 Then
                vb14.DumpMessage("SprobujZClip, niby link openstreetmap, ale brakuje pierwszego '/'")
                Return False
            End If
            sTxt = sTxt.Substring(iInd + 1)
            iInd = sTxt.IndexOf("/")
            If iInd < 1 Then
                vb14.DumpMessage("SprobujZClip, niby link openstreetmap, ale brakuje '/' pomiedzy Lat/Lon")
                Return False
            End If

            sTxt = sTxt.Replace(",", ".")
            SprobujParse(sTxt.Substring(0, iInd), sTxt.Substring(iInd + 1))
            Return True
        End If

        If sTxt.Contains(", ") Then
            'openstre   50.0485, 19.9155
            'BING:      50,039696, 19,949188
            iInd = sTxt.IndexOf(", ")
            sTxt = sTxt.Replace(",", ".")
            SprobujParse(sTxt.Substring(0, iInd), sTxt.Substring(iInd + 2))
            Return True
        End If

        Return False
    End Function



    Private Sub uiMapka_Loaded(sender As Object, e As RoutedEventArgs)
        vb14.DumpCurrMethod()

        uiMapka.Center = New Windows.Devices.Geolocation.Geopoint(GetDomekGeopos(0))

        If moLocation IsNot Nothing Then
            If IsFamilyMobile() Then
                uiMapka.ZoomLevel = 14
            Else
                uiMapka.ZoomLevel = 15
            End If
        Else
            If IsFamilyMobile() Then
                uiMapka.ZoomLevel = 10
            Else
                uiMapka.ZoomLevel = 12
            End If
        End If

        uiMapka.PedestrianFeaturesVisible = True
        uiMapka.TransitFeaturesVisible = True
        uiMapka.TransitFeaturesEnabled = True ' // od 14393, ale że Not implemented?

        uiMapka.MapServiceToken = GetServiceToken()
        uiMapka.Style = Maps.MapStyle.Road  ' było tylko dla nie mojego, ale niech będzie zawsze

    End Sub


    Private Sub uiMapka_Holding(sender As Maps.MapControl, args As Maps.MapInputEventArgs)
        vb14.DumpCurrMethod()
        uiLat.Text = args.Location.Position.Latitude
        uiLon.Text = args.Location.Position.Longitude
        MapkaFocus(False)
    End Sub

    Private Sub uiMapka_DTapped(sender As Maps.MapControl, args As Maps.MapInputEventArgs)
        vb14.DumpCurrMethod()
        uiLat.Text = args.Location.Position.Latitude
        uiLon.Text = args.Location.Position.Longitude
        MapkaFocus(False)
    End Sub

    Private Sub MapkaFocus(bFocus As Boolean)
        vb14.DumpCurrMethod()
        For Each oItem As FrameworkElement In uiGridFields.Children
            If Not oItem.Name.StartsWith("uiNazwa") Then
                oItem.Visibility = If(bFocus, Visibility.Collapsed, Visibility.Visible)
            End If
        Next
    End Sub


    Private Sub uiMapka_FocusEngaged(sender As Control, args As FocusEngagedEventArgs) Handles uiMapka.FocusEngaged
        vb14.DumpCurrMethod()
        MapkaFocus(True)
    End Sub

    Private Sub uiMapka_Tapped(sender As Control, e As TappedRoutedEventArgs) Handles uiMapka.Tapped
        vb14.DumpCurrMethod()
        MapkaFocus(True)
        ' TappedRoutedEventArgs
        ' Maps.MapInputEventArgs
    End Sub

    Private Sub uiMapka_MapTapped(sender As Maps.MapControl, args As Maps.MapInputEventArgs) Handles uiMapka.MapTapped
        vb14.DumpCurrMethod()
        MapkaFocus(True)
    End Sub

    Private Sub POIclear()
        vb14.DumpCurrMethod()
        uiPOIlist.Items.Clear()
    End Sub

    Private Sub POIend()
        vb14.DumpCurrMethod()
        Dim oSep As New MenuFlyoutSeparator
        uiPOIlist.Items.Add(oSep)
        Dim oNew As New MenuFlyoutItem
        oNew.Text = "Find"
        AddHandler oNew.Click, AddressOf uiFind_Click
        uiPOIlist.Items.Add(oNew)
    End Sub

    Private Sub uiPOI_Click(sender As Object, e As RoutedEventArgs)
        vb14.DumpCurrMethod()
        ' kliknięto na konkretnym POI
        Dim oFE As FrameworkElement = sender
        If oFE Is Nothing Then Return

        Dim oPkt As Windows.Devices.Geolocation.BasicGeoposition = oFE.DataContext
        TrySetMapCenter(oPkt)

    End Sub

    Private Async Sub uiFind_Click(sender As Object, e As RoutedEventArgs)
        vb14.DumpCurrMethod()

        If Not NetIsIPavailable(True) Then Return

        'Me.Frame.Navigate(GetType(FindPOI))
        Dim sTxt As String = Await vb14.DialogBoxInputDirectAsync("Co mam szukać?")
        If sTxt = "" Then Return

        ' wyszukanie korzystając z OpenStreetMaps
        MapkaFocus(True)

        POIclear()
        Await POIfill(sTxt)
        POIend()    ' czyli separator oraz 'search'

    End Sub

    Private Async Function POIfill(sSearchQuery As String) As Task
        ' https://operations.osmfoundation.org/policies/nominatim/ (1 search/sec, UserAgent na serio)
        ' zrob listę, do każdego jako DataContext dodaj Windows.Devices.Geolocation.BasicGeoposition ze wspolrzednymi

        Dim sUrl As String = "https://nominatim.openstreetmap.org/search?format=jsonv2&q=" & System.Net.WebUtility.UrlEncode(sSearchQuery)

        Dim moHttp As New Windows.Web.Http.HttpClient
        moHttp.DefaultRequestHeaders.UserAgent.TryParseAdd("PrzypomnijTu " & GetAppVers())
        moHttp.DefaultRequestHeaders.Accept.Add(New Windows.Web.Http.Headers.HttpMediaTypeWithQualityHeaderValue("application/json"))

        Dim sError = ""
        Dim oResp As Windows.Web.Http.HttpResponseMessage = Nothing

        Try
            oResp = Await moHttp.GetAsync(New Uri(sUrl))
        Catch ex As Exception
            sError = ex.Message
        End Try
        If sError <> "" Then
            sError = "error " & sError & ": chyba app nie ma uprawnień do Internet"
            vb14.DialogBox(sError)
            Return
        End If

        If Not oResp.IsSuccessStatusCode Then
            vb14.DialogBox("ERROR: cannot get answer from nominatim.openstreetmap.org")
            Return
        End If

        Dim sResp As String = ""
        Try
            sResp = Await oResp.Content.ReadAsStringAsync
        Catch ex As Exception
            sError = ex.Message
        End Try

        If sError <> "" Then
            sError = "error " & sError & " at ReadAsStringAsync"
            vb14.DialogBox(sError)
            Return
        End If

        Dim oList As List(Of OSMnominatim)
        oList = Newtonsoft.Json.JsonConvert.DeserializeObject(sResp, GetType(List(Of OSMnominatim)))

        Dim iCnt As Integer = 10
        For Each oItem As OSMnominatim In oList
            Dim oNew As New MenuFlyoutItem
            oNew.Text = oItem.display_name
            Dim oGeo As New Windows.Devices.Geolocation.BasicGeoposition
            oGeo.Latitude = oItem.lat
            oGeo.Longitude = oItem.lon
            oNew.DataContext = oGeo
            AddHandler oNew.Click, AddressOf uiPOI_Click
            uiPOIlist.Items.Add(oNew)
            iCnt -= 1
            If iCnt < 0 Then
                Exit For
            End If
        Next
        If oList.Count > 10 Then
            Dim oNew As New MenuFlyoutItem
            oNew.Text = "..."
            uiPOIlist.Items.Add(oNew)
        End If

    End Function

End Class

Public Class OSMnominatim
    Public Property lat As String
    Public Property lon As String
    Public Property display_name As String

    '{
    '    "place_id": "100149",
    '    "licence": "Data © OpenStreetMap contributors, ODbL 1.0. https://osm.org/copyright",
    '    "osm_type": "node",
    '    "osm_id": "107775",
    '    "boundingbox": ["51.3473219", "51.6673219", "-0.2876474", "0.0323526"],
    '    "lat": "51.5073219",
    '    "lon": "-0.1276474",
    '    "display_name": "London, Greater London, England, SW1A 2DU, United Kingdom",
    '    "class": "place",
    '    "type": "city",
    '    "importance": 0.9654895765402,
    '    "icon": "https://nominatim.openstreetmap.org/images/mapicons/poi_place_city.p.20.png",
    '    "address": {
    '      "city": "London",
    '      "state_district": "Greater London",
    '      "state": "England",
    '      "postcode": "SW1A 2DU",
    '      "country": "United Kingdom",
    '      "country_code": "gb"
    '    },
    '    "extratags": {
    '      "capital": "yes",
    '      "website": "http://www.london.gov.uk",
    '      "wikidata": "Q84",
    '      "wikipedia": "en:London",
    '      "population": "8416535"
    '    }
    '    }
End Class

