
Imports vb14 = Vblib.pkarlibmodule14


Public NotInheritable Class MainPage
    Inherits Page

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Me.ProgRingInit(True, False)

        If Me.Width < 500 Then  ' Lumia: 480×800
            uiAppBarSeparat.Visibility = Visibility.Collapsed
        Else
            uiAppBarSeparat.Visibility = Visibility.Visible
        End If

        If vb14.GetSettingsBool("uiOneDrive") Then
            Me.ProgRingShow(True)
            If App.mODroot Is Nothing Then App.mODroot = Await ODclient.GetRootAsync()
            Me.ProgRingShow(False)
        End If

        If App.moBazaSklepu IsNot Nothing Then
            ' wróciliśmy tutaj z wizyty w sklepie, może coś się zmieniło
            ' tam mógł być nie OK, ale Back - a i tak trzeba zapisać
            Await App.moBazaSklepu.SaveAsync
        End If

        App.moSklepy = New BazaSklepy
        uiRefresh_Click(Nothing, Nothing)
        AddHandler Windows.Storage.ApplicationData.Current.DataChanged, AddressOf DataChangeHandler
    End Sub

    Private Sub PokazListeSklepu(sName As String)
        App.moBazaSklepu = New BazaItemySklepu(sName)
        If App.moBazaSklepu Is Nothing Then
            vb14.DialogBoxRes("errAnyError")
            Return
        End If
        Me.Frame.Navigate(GetType(ListaProduktow))
    End Sub

    Private Sub PokazListe()
        If vb14.GetSettingsBool("uiSklepySort") Then
            ' true: wedle nazwy
            ListItems.ItemsSource = From c In App.moSklepy.GetList Order By c.Nazwa
        Else
            ListItems.ItemsSource = App.moSklepy.GetList
        End If
    End Sub

#Disable Warning BC42356 ' This async method lacks 'Await' operators and so will run synchronously
    Private Async Sub DataChangeHandler(ByVal appData As Windows.Storage.ApplicationData, ByVal o As Object)
#Enable Warning BC42356 ' This async method lacks 'Await' operators and so will run synchronously
        uiRefresh_Click(Nothing, Nothing)
    End Sub

#Region "UI"

#Region "contextMenu Sklepu"
    Private Async Sub uiShopRemove_Click(sender As Object, e As RoutedEventArgs)
        App.moSklepy.Delete(TryCast(TryCast(sender, MenuFlyoutItem).DataContext, Vblib.JedenSklep).Nazwa)
        Await App.moSklepy.SaveAsync()
        PokazListe()
    End Sub


    Private Sub uiShopContext_Click(sender As Object, e As RoutedEventArgs)
        PokazListeSklepu(TryCast(TryCast(sender, MenuFlyoutItem).DataContext, Vblib.JedenSklep).Nazwa)
    End Sub

#End Region
    Private Sub uiShop_Tapped(sender As Object, e As TappedRoutedEventArgs)
        ' zmiana strony na liste zakupowa danego sklepu
        PokazListeSklepu(TryCast(sender, TextBlock).Text)
    End Sub

#Region "CommandBar"

    Private Async Sub uiRefresh_Click(sender As Object, e As RoutedEventArgs)
        Me.ProgRingShow(True)
        Await App.moSklepy.LoadAsync(vb14.GetSettingsBool("uiOneDrive"))
        Me.ProgRingShow(False)

        PokazListe()
    End Sub

    Private Async Sub uiAddSklep_Click(sender As Object, e As RoutedEventArgs)

        Dim sName As String = Await vb14.DialogBoxResYNAsync("msgNazwaSklepu", "msgDodajSklep", "msgCancelSklep")
        If sName.Length < 2 Then Exit Sub

        Dim oTemp As New BazaItemySklepu(sName)
        Await oTemp.LoadAsync(vb14.GetSettingsBool("uiOneDrive"))
        If oTemp.Count > 0 Then
            If Await vb14.DialogBoxResYNAsync("resPrevShopExist", "resPrevShopDelete", "resPrevShopRetain") Then
                oTemp.Clear()
                Await oTemp.SaveAsync
            End If
        End If

        App.moSklepy.Add(sName)
        Await App.moSklepy.SaveAsync()

        PokazListe()

    End Sub

    Private Async Sub uiImport_Click(sender As Object, e As RoutedEventArgs)

        Dim sTxt As String = uiImportText.Text

        uiImportFlyout.Hide()
        uiImportText.Text = ""    ' zeby nie zostawala poprzednia nazwa

        If sTxt.Length < 30 Then
            vb14.DialogBoxRes("errTooShortImport")
            Return
        End If

        If sTxt.Substring(0, 4).ToLower = "http" Then
            sTxt = Await vb14.HttpPageAsync(New Uri(sTxt))
            If sTxt = "" Then vb14.DialogBoxRes("errHttpError")
            Return
        End If

        If sTxt.Length < 30 Then
            vb14.DialogBoxRes("errTooShortImport")
            Return
        End If

        Dim sNewName As String = Date.Now.ToString("yy-MM-dd")
        sNewName = Await vb14.DialogBoxInputDirectAsync(vb14.GetLangString("msgEnterName"), sNewName)
        If sNewName = "" Then Return

        Dim oItems As BazaItemySklepu = BazaItemySklepu.TryImport(sNewName, sTxt)
        If oItems Is Nothing Then Return

        Await oItems.SaveAsync(vb14.GetSettingsBool("uiOneDrive"))

        App.moSklepy.Add(sNewName)
        Await App.moSklepy.SaveAsync()


        If Not Await vb14.DialogBoxResYNAsync("msgPrzejscDoNowego") Then
            PokazListe()
            Return
        Else
            PokazListeSklepu(sNewName)
        End If


    End Sub

    Private Sub uiGoSettings_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.Navigate(GetType(setup))
    End Sub
#End Region
#End Region
End Class
