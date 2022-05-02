'' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

'Imports System.Xml.Serialization
'Imports Windows.ApplicationModel.DataTransfer
'Imports Windows.Data.Xml.Dom
'Imports Windows.Web.Http
'''' <summary>
'''' An empty page that can be used on its own or navigated to within a Frame.
'''' </summary>
'Public NotInheritable Class ListaProduktow
'    Inherits Page

'    'Private bReadFromApp As Boolean = False
'    'Private msUpdatingItemName = ""

'    Public Class BazaItemySklepu
'        Private Itemy As ObservableCollection(Of BazaItem)
'        Private bDirty = False
'        Private mFileName = ""

'        Public Sub Add(sName As String, sMiejsce As String, sInfo As String, sCena As String)
'            Dim oNew As BazaItem = New BazaItem
'            oNew.Nazwa = sName
'            oNew.Cena = sCena
'            oNew.Info = sInfo
'            oNew.Miejsce = sMiejsce
'            oNew.Zalatwione = False
'            oNew.ShowTBox = Visibility.Collapsed
'            oNew.ShowTBlock = Visibility.Visible
'            Itemy.Add(oNew)
'            bDirty = True
'        End Sub
'        Public Sub Delete(sName As String)
'            bDirty = True
'            For Each oDel As BazaItem In Itemy
'                If oDel.Nazwa = sName Then
'                    Itemy.Remove(oDel)
'                    Exit Sub
'                End If
'            Next
'        End Sub

'        Public Async Function Load() As Task
'            Dim oFile As Windows.Storage.StorageFile = Await App.GetRoamingFile(mFileName, False)
'            If oFile Is Nothing Then Exit Function

'            Dim oSer As XmlSerializer = New XmlSerializer(GetType(ObservableCollection(Of BazaItem)))
'            Dim oStream As Stream = Await oFile.OpenStreamForReadAsync
'            Itemy = TryCast(oSer.Deserialize(oStream), ObservableCollection(Of BazaItem))

'            'For Each oItem In Itemy
'            '    oItem.ShowTBlock = Visibility.Visible
'            '    oItem.ShowTBox = Visibility.Collapsed
'            'Next
'        End Function

'        Public Async Function Save() As Task

'            Dim oFile As Windows.Storage.StorageFile = Await App.GetRoamingFile(mFileName, True)
'            If oFile Is Nothing Then Exit Function

'            Dim oSer As XmlSerializer = New XmlSerializer(GetType(ObservableCollection(Of BazaItem)))
'            Dim oStream As Stream = Await oFile.OpenStreamForWriteAsync
'            oSer.Serialize(oStream, Itemy)
'            oStream.Dispose()   ' == fclose

'        End Function

'        Public Sub New(sNazwaSklepu As String)
'            Itemy = New ObservableCollection(Of BazaItem)
'            mFileName = sNazwaSklepu & ".xml"

'        End Sub

'        Protected Overrides Async Sub Finalize()
'            'If bDirty Then
'            Await Save()
'            bDirty = False
'            'End If
'            Itemy.Clear()
'        End Sub

'        Public Function GetList() As ICollection(Of BazaItem)
'            Return Itemy
'        End Function

'        Public Function GetOrderedList() As IOrderedEnumerable(Of BazaItem)
'            Dim groups As IOrderedEnumerable(Of BazaItem) = From c In Itemy Order By c.Zalatwione, c.Miejsce
'            Return groups
'        End Function

'        Public Function GetGroupedList() As IOrderedEnumerable(Of BazaItem)
'            Dim groups = From c In Itemy Order By c.Zalatwione, c.Miejsce Group By c.Miejsce Into Group
'            Return groups
'        End Function
'    End Class


'    Public Class GroupInfoList
'        Inherits List(Of Object)
'        Public Property Miejsce As String
'        ' Public Property Lista As List(Of Object)
'    End Class

'    ' Dim msNazwaSklepu As String
'    Dim moBazaItems As BazaItemySklepu

'    'Protected Overrides Sub onNavigatedTo(e As NavigationEventArgs)
'    'End Sub

'    Private Sub uiRefresh_Click(sender As Object, e As RoutedEventArgs)
'        WyswietlPogrupowane()
'    End Sub

'    Private Function ImportSmartShopping(sTxt As String) As Boolean
'        Dim bError As Boolean = False
'        '<ShoppingList>
'        '<Store>lidl</Store>
'        '<Items>
'        '<ShoppingListItem>
'        '<BaseItem><Name>czekolada</Name><Category>01 pieczywo</Category><UnitOfMeasure>Pack</UnitOfMeasure><ImageUri>ms-appx:/UniAssets/Icons/shopping-cart.png</ImageUri></BaseItem>
'        '<Price>1.49</Price><Quantity>1</Quantity><Note>nadziewana</Note><IsCompleted>true</IsCompleted><AisleNum>-1</AisleNum></ShoppingListItem>

'        Dim oItemsXml As XmlDocument = New XmlDocument
'        Try
'            oItemsXml.LoadXml(sTxt)
'        Catch ex As Exception
'            bError = True
'        End Try
'        If bError Then
'            App.DialogBoxRes("errCannotImportXML")
'            Return False
'        End If

'        Dim oNodes As XmlNodeList = oItemsXml.DocumentElement.SelectNodes("//ShoppingListItem")
'        For Each oNode As IXmlNode In oNodes
'            Dim sPrice As String = oNode.SelectSingleNode("Price").InnerText.ToString
'            If sPrice = "-1" Then sPrice = ""
'            Dim sInfo As String = oNode.SelectSingleNode("Note").InnerText.ToString
'            ' Dim bDone As boolean = oNode.SelectSingleNode("IsCompleted").InnerText.ToString
'            Dim oNodeBase As IXmlNode = oNode.SelectSingleNode("BaseItem")
'            Dim sNazwa As String = oNodeBase.SelectSingleNode("Name").InnerText.ToString
'            Dim sCat As String = oNodeBase.SelectSingleNode("Category").InnerText.ToString
'            moBazaItems.Add(sNazwa, sCat, sInfo, sPrice)
'        Next
'        Return True
'    End Function

'    Private Function ImportCSV(sTxt As String, sSeparator As String) As Boolean
'        ImportCSV = False

'        Dim aLines As String() = sTxt.Split(vbCrLf)
'        If aLines.Count < 1 Then
'            App.DialogBoxRes("errCSVnoLine")
'            Return False
'        End If

'        Dim iIgnored As Integer = 0
'        Dim iImported As Integer = 0

'        For Each sLine As String In aLines
'            Dim aFld As String() = sLine.Split(sSeparator)
'            If aFld.Count <> 4 Then
'                iIgnored += 1
'            Else
'                moBazaItems.Add(aFld(0), aFld(1), aFld(2), aFld(3))
'                iImported += 1
'            End If
'        Next

'        If iImported = 0 Then
'            App.DialogBoxRes("errCSVempty")
'            Return False
'        End If

'        If iIgnored <> 0 Then
'            App.DialogBox("Warning: CSV import, " & iImported.ToString & " lines imported, " & iIgnored.ToString & " lines ignored")
'        End If

'        Return True

'    End Function

'    Private Async Sub uiImport_Click(sender As Object, e As RoutedEventArgs)
'        Dim sTxt As String = uiImportText.Text

'        uiImportFlyout.Hide()
'        uiImportText.Text = ""    ' zeby nie zostawala poprzednia nazwa

'        If sTxt.Length < 30 Then
'            App.DialogBoxRes("errTooShortImport")
'            Exit Sub
'        End If

'        Dim bError As Boolean = False
'        If sTxt.Substring(0, 4).ToLower = "http" Then
'            Dim oHttp As HttpClient = New HttpClient
'            Try
'                Dim sResp As String = Await oHttp.GetStringAsync(New Uri(sTxt))
'                sTxt = sResp
'            Catch ex As Exception
'                bError = True
'            End Try

'            If bError Then
'                App.DialogBoxRes("errHttpError")
'                Exit Sub
'            End If
'        End If

'        If sTxt.Length < 30 Then
'            App.DialogBoxRes("errTooShortImport")
'            Exit Sub
'        End If

'        Dim bRetOk As Boolean
'        If sTxt.IndexOf("<ShoppingListItem><BaseItem>") > 3 Then
'            bRetOk = ImportSmartShopping(sTxt)
'        ElseIf sTxt.IndexOf("|") > 1 Then
'            bRetOk = ImportCSV(sTxt, "|")
'        ElseIf sTxt.IndexOf(vbTab) > 1 Then
'            bRetOk = ImportCSV(sTxt, vbTab)
'        Else
'            App.DialogBoxRes("errUnknownFormat")
'            Exit Sub
'        End If

'        If Not bRetOk Then Exit Sub


'        moBazaItems.Save()
'        WyswietlPogrupowane()

'    End Sub

'    Private Sub CallEditItem()
'        ' lista grup do sTxt
'        Dim sTxt As String = ""
'        For Each sGrp As String In From c In moBazaItems.GetList Select c.Miejsce Distinct
'            sTxt = sTxt & sGrp & "|"
'        Next
'        Me.Frame.Navigate(GetType(EditItem), sTxt)
'    End Sub

'    Private Sub uiAddItem_Click(sender As Object, e As RoutedEventArgs)
'        App.oItemek = New BazaItem

'        App.msUpdatingItemName = ""

'        CallEditItem()
'    End Sub

'    Private Sub uiItemEdit_Click(sender As Object, e As RoutedEventArgs)
'        Dim oItem As BazaItem = TryCast(TryCast(sender, MenuFlyoutItem).DataContext, BazaItem)
'        'oItem.ShowTBlock = Visibility.Collapsed
'        'oItem.ShowTBox = Visibility.Visible
'        App.oItemek = oItem
'        App.msUpdatingItemName = oItem.Nazwa
'        CallEditItem()
'    End Sub

'    Private Sub uiItemRemove_Click(sender As Object, e As RoutedEventArgs)
'        moBazaItems.Delete(TryCast(TryCast(sender, MenuFlyoutItem).DataContext, BazaItem).Nazwa)
'        WyswietlPogrupowane()
'    End Sub

'    Private Async Sub uiGoMain_Click(sender As Object, e As RoutedEventArgs)
'        Await moBazaItems.Save()    ' na wszelki wypadek - jakby GoBack przerwalo zapisywanie...
'        Me.Frame.GoBack()
'    End Sub

'    Private Sub WyswietlPogrupowane()
'        'ListItemsSklepu.ItemsSource = moBazaItems.GetOrderedList ' moBazaItems.GetList
'        'ItemyGrp.Source = From c In moBazaItems.GetList Order By c.Zalatwione, c.Miejsce Group By c.Miejsce Into Group
'        ' ItemyGrp.Source = From c In moBazaItems.GetList Order By c.Zalatwione, c.Miejsce
'        ' ItemyGrp.Source = moBazaItems.GetOrderedList    ' dziala, gdy ItemyGrp nie ma IsGrouped=true

'        ' ItemyGrp.Source = From c In moBazaItems.GetList Group By c.Miejsce Into Kategoria = Group, Items = c
'        ' grupowanie wylatuje runtime z cannot convert type
'        ' https://github.com/Microsoft/Windows-universal-samples/blob/master/Samples/XamlListView/cs/Model/Contact.cs

'        Dim oGrupy As ObservableCollection(Of GroupInfoList) = New ObservableCollection(Of GroupInfoList)

'        Dim colItems As ICollection(Of BazaItem) = moBazaItems.GetList
'        Dim oQuery = From c In colItems Order By c.Miejsce Group By c.Miejsce Into Group
'        'ItemyGrp.Source = oQuery
'        For Each oGrp In oQuery
'            Dim info As GroupInfoList = New GroupInfoList
'            info.Miejsce = oGrp.Miejsce
'            'info.Lista = New List(Of Object)
'            Dim oWgrupie As IEnumerable(Of BazaItem) = From c In colItems Where c.Miejsce = info.Miejsce
'            For Each oItem As BazaItem In oWgrupie
'                info.Add(oItem)
'            Next
'            oGrupy.Add(info)
'        Next
'        ItemyGrp.Source = oGrupy
'        'System.Linq.GroupedEnumerable`4[ListaZakupowa.ListaProduktow+BazaItem,System.String,ListaZakupowa.ListaProduktow+BazaItem,VB$AnonymousType_0`2[System.String,System.Collections.Generic.IEnumerable`1[ListaZakupowa.ListaProduktow+BazaItem]]]' to type 
'        'System.Linq.IOrderedEnumerable`1[ListaZakupowa.ListaProduktow+BazaItem]'.

'    End Sub
'    Private Async Sub uiPage_Loaded(sender As Object, e As RoutedEventArgs)
'        moBazaItems = New BazaItemySklepu(App.msNazwaSklepu)

'        Await moBazaItems.Load()
'        Dim bError As Boolean = False
'        Try
'            If App.mbReadFromApp Then   ' byla edycja
'                If App.msUpdatingItemName <> "" Then moBazaItems.Delete(App.msUpdatingItemName) ' różnica między edit a add
'                moBazaItems.Add(App.oItemek.Nazwa, App.oItemek.Miejsce, App.oItemek.Info, App.oItemek.Cena)
'                moBazaItems.Save()
'            End If
'        Catch ex As Exception
'            bError = True
'        End Try

'        If bError Then
'            App.DialogBoxRes("errAnyError")
'            Exit Sub
'        End If

'        App.mbReadFromApp = False

'        WyswietlPogrupowane()
'        If ListItemsSklepu.ActualWidth > 400 Then uiExport.Visibility = Visibility.Visible
'    End Sub

'    Private Sub uiExport_Click(sender As Object, e As RoutedEventArgs) Handles uiExport.Click
'        Dim sTxt As String = ""
'        For Each oItem As BazaItem In moBazaItems.GetList
'            sTxt = sTxt & oItem.Nazwa & "|" & oItem.Miejsce & "|" & oItem.Info & "|" & oItem.Cena & vbCrLf
'        Next

'        Dim oClipCont As DataPackage = New DataPackage
'        oClipCont.RequestedOperation = DataPackageOperation.Copy
'        oClipCont.SetText(sTxt)
'        Clipboard.SetContent(oClipCont)

'        App.DialogBoxRes("msgExportClip")
'    End Sub
'End Class
