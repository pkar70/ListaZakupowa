' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

Imports System.Runtime.Serialization
Imports System.Xml.Serialization
Imports Windows.Storage

<XmlType("sklep")>
Public Class BazaSklep
    <XmlAttribute()>
    Public Property Nazwa As String
End Class

Public Class BazaSklepy
    Private Sklepy As ObservableCollection(Of BazaSklep)
    Private bDirty = False
    Private mFileName = "int_sklepy.xml"

    Public Sub Add(sName As String)
        Dim oNew As BazaSklep = New BazaSklep
        oNew.Nazwa = sName
        Sklepy.Add(oNew)
        bDirty = True
    End Sub
    Public Sub Delete(sName As String)
        'Dim oNew = New BazaSklep
        'oNew.Nazwa = sName
        'Sklepy.Remove(oNew)
        bDirty = True
        For Each oDel As BazaSklep In Sklepy
            If oDel.Nazwa = sName Then
                Sklepy.Remove(oDel)
                Exit Sub
            End If
        Next
    End Sub

    Public Async Function Load() As Task
        Dim oFile As StorageFile = Await App.GetRoamingFile(mFileName, False)
        If oFile Is Nothing Then Exit Function

        Dim oSer As XmlSerializer = New XmlSerializer(GetType(ObservableCollection(Of BazaSklep)))
        Dim oStream As Stream = Await oFile.OpenStreamForReadAsync
        Sklepy = TryCast(oSer.Deserialize(oStream), ObservableCollection(Of BazaSklep))
        bDirty = False
    End Function

    Public Async Function Save() As Task

        Dim oFile As StorageFile = Await App.GetRoamingFile(mFileName, True)
        If oFile Is Nothing Then Exit Function

        Dim oSer As XmlSerializer = New XmlSerializer(GetType(ObservableCollection(Of BazaSklep)))
        Dim oStream As Stream = Await oFile.OpenStreamForWriteAsync
        oSer.Serialize(oStream, Sklepy)
        oStream.Dispose()   ' == fclose

    End Function

    Public Sub New()
        Sklepy = New ObservableCollection(Of BazaSklep)

        'Add("Lidl")
        'Add("Iga")
        'Add("Obi")
        'Add("Sidzina")
        'Add("Kielce")
    End Sub

    Protected Overrides Async Sub Finalize()
        If bDirty Then
            Await Save()
            bDirty = False
        End If
        Sklepy.Clear()
    End Sub

    Public Function GetList() As ICollection(Of BazaSklep)
        Return Sklepy
    End Function

End Class
Public NotInheritable Class MainPage
    Inherits Page
    Dim moSklepy As BazaSklepy = New BazaSklepy

    Private Sub RefreshSklepy()
        ListItems.ItemsSource = moSklepy.GetList
    End Sub
    Private Async Sub UIpage_Loaded(sender As Object, e As RoutedEventArgs)
        Await moSklepy.Load()
        RefreshSklepy()
        AddHandler Windows.Storage.ApplicationData.Current.DataChanged, AddressOf DataChangeHandler
    End Sub

    Private Sub PokazListeSklepu(sName As String)
        App.msNazwaSklepu = sName
        Me.Frame.Navigate(GetType(ListaProduktow), sName)
    End Sub

    Private Sub uiShop_Tapped(sender As Object, e As TappedRoutedEventArgs)
        ' zmiana strony na liste zakupowa danego sklepu
        PokazListeSklepu(TryCast(sender, TextBlock).Text)
    End Sub

    'Private Sub uiShopEditLayout_Click(sender As Object, e As RoutedEventArgs)
    '    ' edycja layout - osobna baza (int_layout_[sklep].xml)
    'End Sub

    Private Sub uiShopRemove_Click(sender As Object, e As RoutedEventArgs)
        ' ask for confirm
        moSklepy.Delete(TryCast(TryCast(sender, MenuFlyoutItem).DataContext, BazaSklep).Nazwa)
        ' refresh
        moSklepy.Save()
    End Sub

    Private Async Sub uiAddSklep_Click(sender As Object, e As RoutedEventArgs)
        Dim sName As String = uiAddSklep.Text
        If sName.Length < 2 Then Exit Sub

        Dim bExistPrev As Boolean = False
        Dim oPlik As StorageFile = Await App.GetRoamingFile(sName & ".xml", False)
        If oPlik IsNot Nothing Then
            If Await App.DialogBoxResYN("resPrevShopExist", "resPrevShopDelete", "resPrevShopRetain") Then
                Await oPlik.DeleteAsync()
            End If
            bExistPrev = True
        End If
        moSklepy.Add(sName)
        moSklepy.Save()
        'RefreshSklepy()
        uiAddSklepFlyout.Hide()
        uiAddSklep.Text = ""    ' zeby nie zostawala poprzednia nazwa
    End Sub

    Private Sub uiShopContext_Click(sender As Object, e As RoutedEventArgs)
        PokazListeSklepu(TryCast(TryCast(sender, MenuFlyoutItem).DataContext, BazaSklep).Nazwa)
    End Sub

    Private Async Sub DataChangeHandler(ByVal appData As Windows.Storage.ApplicationData, ByVal o As Object)
        Await moSklepy.Load()
        RefreshSklepy()
    End Sub

    Private Async Sub uiRefresh_Click(sender As Object, e As RoutedEventArgs)
        Await moSklepy.Load()
        RefreshSklepy()
    End Sub


End Class
