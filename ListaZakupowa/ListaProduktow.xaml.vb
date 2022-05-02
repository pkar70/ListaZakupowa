Imports vb14 = Vblib.pkarlibmodule14

Public NotInheritable Class ListaProduktow
    Inherits Page

    Public Class GroupInfoList
        Inherits List(Of Object)
        Public Property Miejsce As String
        ' Public Property Lista As List(Of Object)
    End Class

    Private Sub uiRefresh_Click(sender As Object, e As RoutedEventArgs)
        WyswietlPogrupowane()
    End Sub

    Private Sub uiAddItem_Click(sender As Object, e As RoutedEventArgs)
        App.moEditingItem = New Vblib.JedenItem
        Me.Frame.Navigate(GetType(EditItem))

    End Sub

    Private Sub uiItemEdit_Click(sender As Object, e As RoutedEventArgs)
        Dim oItem As Vblib.JedenItem = TryCast(TryCast(sender, MenuFlyoutItem)?.DataContext, Vblib.JedenItem)
        If oItem Is Nothing Then Return

        App.moEditingItem = oItem
        Me.Frame.Navigate(GetType(EditItem))

    End Sub

    Private Sub uiItemRemove_Click(sender As Object, e As RoutedEventArgs)
        App.moBazaSklepu.Delete(TryCast(TryCast(sender, MenuFlyoutItem).DataContext, Vblib.JedenItem).Nazwa)
        WyswietlPogrupowane()
    End Sub

    Private Sub uiGoMain_Click(sender As Object, e As RoutedEventArgs)
        'zapisywanie jest w MainPage, bo wrócić można też przez BACK
        Me.Frame.GoBack()
    End Sub

    Private Sub WyswietlPogrupowane()
        'ListItemsSklepu.ItemsSource = moBazaItems.GetOrderedList ' moBazaItems.GetList
        'ItemyGrp.Source = From c In moBazaItems.GetList Order By c.Zalatwione, c.Miejsce Group By c.Miejsce Into Group
        ' ItemyGrp.Source = From c In moBazaItems.GetList Order By c.Zalatwione, c.Miejsce
        ' ItemyGrp.Source = moBazaItems.GetOrderedList    ' dziala, gdy ItemyGrp nie ma IsGrouped=true

        ' ItemyGrp.Source = From c In moBazaItems.GetList Group By c.Miejsce Into Kategoria = Group, Items = c
        ' grupowanie wylatuje runtime z cannot convert type
        ' https://github.com/Microsoft/Windows-universal-samples/blob/master/Samples/XamlListView/cs/Model/Contact.cs

        Dim oGrupy As New ObservableCollection(Of GroupInfoList)

        Dim colItems As ICollection(Of Vblib.JedenItem) = App.moBazaSklepu.GetList
        Dim oQuery = From c In colItems Order By c.Miejsce Group By c.Miejsce Into Group
        'ItemyGrp.Source = oQuery
        For Each oGrp In oQuery
            Dim info As New GroupInfoList
            info.Miejsce = oGrp.Miejsce
            'info.Lista = New List(Of Object)
            Dim oWgrupie As IEnumerable(Of Vblib.JedenItem) = From c In colItems Where c.Miejsce = info.Miejsce
            For Each oItem As Vblib.JedenItem In oWgrupie
                info.Add(oItem)
            Next
            oGrupy.Add(info)
        Next
        ItemyGrp.Source = oGrupy
        'System.Linq.GroupedEnumerable`4[ListaZakupowa.ListaProduktow+BazaItem,System.String,ListaZakupowa.ListaProduktow+BazaItem,VB$AnonymousType_0`2[System.String,System.Collections.Generic.IEnumerable`1[ListaZakupowa.ListaProduktow+BazaItem]]]' to type 
        'System.Linq.IOrderedEnumerable`1[ListaZakupowa.ListaProduktow+BazaItem]'.

    End Sub
    Private Async Sub uiPage_Loaded(sender As Object, e As RoutedEventArgs)
        Await App.moBazaSklepu.LoadAsync(vb14.GetSettingsBool("uiOneDrive"))
        WyswietlPogrupowane()
    End Sub

    Private Sub uiExport_Click(sender As Object, e As RoutedEventArgs) Handles uiExport.Click
        Dim sTxt As String = App.moBazaSklepu.Export
        vb14.ClipPut(sTxt)
        vb14.DialogBoxRes("msgExportClip")
    End Sub
End Class
