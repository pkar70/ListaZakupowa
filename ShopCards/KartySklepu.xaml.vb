
Public NotInheritable Class KartySklepu
    Inherits Page

    Private moItem As VBlib_Karty.JedenSklep
    Private moCard As VBlib_Karty.JednaKarta

    Protected Overrides Sub onNavigatedTo(e As NavigationEventArgs)

        Dim sName As String = e.Parameter.ToString
        If String.IsNullOrEmpty(sName) Then
            moItem = New VBlib_Karty.JedenSklep
        Else
            Dim iInd As Integer = sName.IndexOf("|")
            If iInd > 0 Then
                ' wywołanie z kartą i sklepem
                Dim sKarta As String = sName.Substring(iInd + 1)
                sName = sName.Substring(0, iInd)
                moItem = If(App.moSklepy.GetItem(sName), New VBlib_Karty.JedenSklep)
                For Each oKarta As VBlib_Karty.JednaKarta In moItem.lKarty
                    If oKarta.sCzyja = sKarta Then
                        moCard = oKarta
                        Exit For
                    End If
                Next
            Else
                ' wywołanie tylko ze sklepem
                moItem = If(App.moSklepy.GetItem(sName), New VBlib_Karty.JedenSklep)
                If moItem.lKarty.Count > 0 Then
                    moCard = moItem.lKarty(0)
                Else
                    moCard = Nothing
                End If
            End If

        End If

    End Sub

    Private Sub PokazKarte(oKarta As VBlib_Karty.JednaKarta)

        uiCzyja.Content = oKarta.sCzyja
        If oKarta.sCzyja = "" Then uiCzyja.Content = "(mine)"

        If oKarta.sPicFilename <> "" Then
            uiPicKarta.Source = New BitmapImage(New Uri(App.GetPickiFolder.Path & "\" & oKarta.sPicFilename))
            uiCardNo.Visibility = Visibility.Collapsed
        Else
            ' generujemy kartę

            ' https://www.codeproject.com/articles/1086849/generate-barcode-and-qr-code-in-windows-universal
            Dim oBarcode As New ZXing.Mobile.BarcodeWriter
            oBarcode.Format = oKarta.iCodeType
            ' ograniczenie na 300 dobre dla kodów paskowych
            oBarcode.Options = New ZXing.Common.EncodingOptions With
                {
                    .Height = Math.Min(uiScroll.ActualHeight, 300),
                    .Width = uiScroll.ActualWidth
                    }

            Dim oRes = oBarcode.Write(oKarta.sNumber)
            If oRes Is Nothing Then Return

            uiPicKarta.Source = oRes

            uiCardNo.Text = oKarta.sNumber
            uiCardNo.Visibility = Visibility.Visible

        End If
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Me.InitDialogs

        If moItem.lKarty.Count = 0 Then
            uiTytul.Text = "brak kart"
            uiTytul.Visibility = Visibility.Visible
            uiSklep.Visibility = Visibility.Collapsed
            uiCzyja.Visibility = Visibility.Collapsed
            Return
        End If

        uiTytul.Text = moItem.sName
        uiSklep.Text = moItem.sName

        PokazKarte(moCard)

        If moItem.lKarty.Count = 1 Then
            ' pokaż tą jedną kartę
            uiTytul.Visibility = Visibility.Visible
            uiSklep.Visibility = Visibility.Collapsed
            uiCzyja.Visibility = Visibility.Collapsed
        Else
            ' pozwól wybrać
            uiTytul.Visibility = Visibility.Collapsed
            uiSklep.Visibility = Visibility.Visible
            uiCzyja.Visibility = Visibility.Visible

            uiMenuKart.Items.Clear()
            For Each oKarta As VBlib_Karty.JednaKarta In moItem.lKarty
                Dim oNew As New MenuFlyoutItem
                oNew.DataContext = oKarta
                oNew.Text = oKarta.sCzyja
                If oNew.Text = "" Then oNew.Text = "(mine)"
                AddHandler oNew.Click, AddressOf WyborKarty
                uiMenuKart.Items.Add(oNew)
            Next

        End If

    End Sub

    Private Sub WyborKarty(sender As Object, e As RoutedEventArgs)
        ' tu wejdzie tylko wtedy, gdy jest więcej niż jedna karta aktywna
        Dim oMFI As MenuFlyoutItem = TryCast(sender, MenuFlyoutItem)
        If oMFI Is Nothing Then Return

        PokazKarte(oMFI.DataContext)

    End Sub
End Class
