
Imports pkar.DotNetExtensions

#Region "lista sklepów"

' pierwotna wersja: plik ROAMING "int_sklepy.xml", XmlType("sklep"), z XML attr "Nazwa", 
' nowsza wersja: plik ROAMING/OneDrive "int_sklepy.txt", jedna linijka na jedną nazwę

' teraz import jest via XmlDoc, żeby można było jakby co to przerzucić to do vblib

Public Class JedenSklep
    Inherits pkar.BaseStruct

    Public Property Nazwa As String
End Class


Public Class BazaSklepy
    Private ReadOnly _lSklepy As New ObjectModel.ObservableCollection(Of JedenSklep)
    'Private _bDirty As String = False
    Private Const FILENAME_BASE As String = "int_sklepy"
    Private Const FILENAME_XML = FILENAME_BASE & ".xml"
    Protected Const FILENAME_TXT = FILENAME_BASE & ".txt"
    Private ReadOnly _sRoamingRootPath As String
    Protected _bUseOneDrive As Boolean = False

    Public Sub Add(sName As String)

        If MyInit(sName) Then Return    ' dodaj moje defaultowe, jeśli to jest moje dodawanie :)

        Dim oNew As New JedenSklep
        oNew.Nazwa = sName
        _lSklepy.Add(oNew)
        '_bDirty = True
    End Sub
    Public Sub Delete(sName As String)
        '_bDirty = True
        For Each oDel As JedenSklep In _lSklepy
            If oDel.Nazwa = sName Then
                _lSklepy.Remove(oDel)
                Exit Sub
            End If
        Next
    End Sub

    Private Async Function ImportXML(sContent As String) As Task

        ' pozbywamy się Byte-Order-Mark, który psuje LoadXml
        Dim iInd As Integer = sContent.IndexOf("<")
        sContent = sContent.Substring(iInd)
        Dim bError As Boolean = False
        Dim oItemsXml As New System.Xml.XmlDocument
        Try
            oItemsXml.LoadXml(sContent)
        Catch ex As Exception
            bError = True
        End Try
        If bError Then
            Await DialogBoxResAsync("errCannotImportXML")
            Return
        End If

        Dim oNodes As Xml.XmlNodeList = oItemsXml.DocumentElement.GetElementsByTagName("sklep")
        For Each oNode As Xml.XmlNode In oNodes
            Dim sNazwa As String = oNode.Attributes?.Item(0)?.InnerText
            If sNazwa IsNot Nothing Then Add(sNazwa)
        Next

    End Function

    Private Sub ImportTxt(sFileContent As String)

        _lSklepy.Clear()

        Dim aSklepy As String() = sFileContent.Split(vbCrLf)
        For Each sSklep As String In aSklepy
            Dim sNazwa = sSklep.Trim
            If sNazwa.Length > 2 Then Add(sNazwa)
        Next
    End Sub

    'Private Function ExportTxt() As String
    '    Dim sRet As String = ""
    '    For Each oSklep As JedenSklep In _lSklepy
    '        sRet = sRet & oSklep.Nazwa & vbCrLf
    '    Next

    '    Return sRet.Trim
    'End Function

    'Protected Overrides Async Sub Finalize()
    '    If _bDirty Then
    '        Await SaveAsync()
    '        _bDirty = False
    '    End If
    '    _lSklepy.Clear()
    'End Sub

    Public Function GetList() As ObjectModel.ObservableCollection(Of JedenSklep)
        Return _lSklepy
    End Function

    Public Sub New(sRoamingRootPath As String)
        _sRoamingRootPath = sRoamingRootPath
    End Sub

    ''' <summary>
    ''' wczytaj dane; zwraca TRUE gdy powinno się zapisać (bo było z XML)
    ''' </summary>
    ''' <param name="sODcontent">zawartość pliku z OneDrive, bądź "" gdy pliku nie ma</param>
    ''' <param name="dODdate">data tego pliku (nieistotna gdy pliku nie było)</param>
    Protected Async Function LoadAsync(sODcontent As String, dODdate As DateTimeOffset) As Task(Of Boolean)
        Dim bFromXml As Boolean = False
        Dim bShouldSave As Boolean = False

        Dim sFilename As String = IO.Path.Combine(_sRoamingRootPath, FILENAME_TXT)
        If Not IO.File.Exists(sFilename) Then
            sFilename = IO.Path.Combine(_sRoamingRootPath, FILENAME_XML)
            If Not IO.File.Exists(sFilename) Then Return False   ' nie ma żadnego pliku do wczytania - sytuacja której nie obsługujemy
            bFromXml = True
        End If

        Dim sContent As String = IO.File.ReadAllText(sFilename)
        Dim dLocalDate As DateTimeOffset = IO.File.GetLastWriteTime(sFilename)

        If _bUseOneDrive AndAlso sODcontent <> "" Then
            ' porownaj daty, z tolerancją 20 sekund
            Select Case Await SelectOneContentChoose(dODdate, dLocalDate, GetSettingsBool("sklepyLastSaveNoOD"))
                Case 0
                    ' oba takie same, przyjmuję bez zmian
                Case 1
                    ' użyj OneDrive
                    sContent = sODcontent
                    bFromXml = False
                Case 2
                    ' użyj lokalne
                    bShouldSave = True
            End Select

        End If

        If bFromXml Then
            Await ImportXML(sContent)
        Else
            ImportTxt(sContent)
        End If

        Return bFromXml Or bShouldSave

    End Function


    ''' <summary>
    ''' zwraca tekst do zapisania do OneDrive, po zapisaniu do Roaming
    ''' </summary>
    ''' <returns></returns>
    Protected Function Save() As String

        Dim sContent As String = String.Join(vbCrLf, _lSklepy.Select(Function(x) x.Nazwa))

        Dim sFilename As String = IO.Path.Combine(_sRoamingRootPath, FILENAME_TXT)
        IO.File.WriteAllText(sFilename, sContent)

        Return sContent

    End Function

End Class

#End Region

#Region "itemy w sklepie"
Public Class JedenItem
    Public Property Nazwa As String = ""
    Public Property Zalatwione As Boolean = False
    Public Property Info As String = ""
    Public Property Cena As String = ""
    Public Property Miejsce As String = ""
End Class

Public Class BazaItemySklepu
    Private _Itemy As ObjectModel.ObservableCollection(Of JedenItem)
    Protected _bDirty = False
    Protected ReadOnly _mFileNameBase As String = ""
    Public ReadOnly NazwaSklepu As String
    Private Shared _sRoamingRootPath As String
    Protected _bUseOneDrive As Boolean = False

    Public Sub New(sNazwaSklepu As String, sRoamingRootPath As String)
        NazwaSklepu = sNazwaSklepu
        _Itemy = New ObjectModel.ObservableCollection(Of JedenItem)
        _mFileNameBase = sNazwaSklepu.ToValidPath
        _sRoamingRootPath = sRoamingRootPath
    End Sub

    Public Sub Add(sName As String, sMiejsce As String, sInfo As String, sCena As String)
        Dim oNew As New JedenItem With {
            .Nazwa = sName,
            .Cena = sCena,
            .Info = sInfo,
            .Miejsce = sMiejsce,
            .Zalatwione = False
        }
        Add(oNew)
    End Sub

    Public Sub Add(oNew As JedenItem)
        _Itemy.Add(oNew)
        _bDirty = True
    End Sub

    Public Sub Delete(sName As String)
        _bDirty = True
        For Each oDel As JedenItem In _Itemy
            If oDel.Nazwa = sName Then
                _Itemy.Remove(oDel)
                Exit Sub
            End If
        Next
    End Sub


    Public Function Count() As Integer
        Return _Itemy.Count
    End Function

    Public Function GetList() As ICollection(Of JedenItem)
        Return _Itemy
    End Function

    Public Sub Clear()
        _Itemy.Clear()
    End Sub

    Public Function GetOrderedList() As IOrderedEnumerable(Of JedenItem)
        Dim groups As IOrderedEnumerable(Of JedenItem) = From c In _Itemy Order By c.Zalatwione, c.Miejsce
        Return groups
    End Function

    Public Function GetGroupedList() As IOrderedEnumerable(Of JedenItem)
        Dim groups = From c In _Itemy Order By c.Zalatwione, c.Miejsce Group By c.Miejsce Into Group
        Return groups
    End Function

    Private Shared Function ImportSmartShopping(sNewName As String, sContent As String) As BazaItemySklepu

        Dim listaItems As New BazaItemySklepu(sNewName, _sRoamingRootPath)

        Dim bError As Boolean = False
        '<ShoppingList>
        '<Store>lidl</Store>
        '<Items>
        '<ShoppingListItem>
        '<BaseItem><Name>czekolada</Name><Category>01 pieczywo</Category><UnitOfMeasure>Pack</UnitOfMeasure><ImageUri>ms-appx:/UniAssets/Icons/shopping-cart.png</ImageUri></BaseItem>
        '<Price>1.49</Price><Quantity>1</Quantity><Note>nadziewana</Note><IsCompleted>true</IsCompleted><AisleNum>-1</AisleNum></ShoppingListItem>

        Dim oItemsXml As New System.Xml.XmlDocument
        Try
            oItemsXml.LoadXml(sContent)
        Catch ex As Exception
            bError = True
        End Try
        If bError Then
            DialogBoxRes("errCannotImportXML")
            Return Nothing
        End If

        Dim oNodes As Xml.XmlNodeList = oItemsXml.DocumentElement.GetElementsByTagName("ShoppingListItem")
        For Each oNode As Xml.XmlNode In oNodes
            Dim sPrice As String = oNode.SelectSingleNode("Price").InnerText
            If sPrice = "-1" Then sPrice = ""
            Dim sInfo As String = oNode.SelectSingleNode("Note").InnerText
            ' Dim bDone As boolean = oNode.SelectSingleNode("IsCompleted").InnerText.ToString
            Dim oNodeBase As Xml.XmlNode = oNode.SelectSingleNode("BaseItem")
            Dim sNazwa As String = oNodeBase.SelectSingleNode("Name").InnerText
            Dim sCat As String = oNodeBase.SelectSingleNode("Category").InnerText
            listaItems.Add(sNazwa, sCat, sInfo, sPrice)
        Next

        Return listaItems

    End Function

    Private Shared Function ImportCSV(sNewName As String, sContent As String, sSeparator As String) As BazaItemySklepu

        Dim aLines As String() = sContent.Split(vbCrLf)
        If aLines.Length < 1 Then
            DialogBoxRes("errCSVnoLine")
            Return Nothing
        End If

        Dim listaItems As New BazaItemySklepu(sNewName, _sRoamingRootPath)

        Dim iIgnored As Integer = 0
        Dim iImported As Integer = 0

        For Each sLine As String In aLines
            Dim aFld As String() = sLine.Split(sSeparator)
            If aFld.Length <> 4 Then
                iIgnored += 1
            Else
                listaItems.Add(aFld(0), aFld(1), aFld(2), aFld(3))
                iImported += 1
            End If
        Next

        If iImported = 0 Then
            DialogBoxRes("errCSVempty")
            Return Nothing
        End If

        If iIgnored <> 0 Then
            DialogBox($"Warning: CSV import, {iImported.ToString} lines imported, {iIgnored.ToString} lines ignored")
        End If

        Return listaItems

    End Function

    Protected Async Function ImportXML(sContent As String) As Task
        ' pozbywamy się Byte-Order-Mark, który psuje LoadXml
        Dim iInd As Integer = sContent.IndexOf("<")
        sContent = sContent.Substring(iInd)
        Dim bError As Boolean = False
        Dim oItemsXml As New System.Xml.XmlDocument
        Try
            oItemsXml.LoadXml(sContent)
        Catch ex As Exception
            bError = True
        End Try
        If bError Then
            Await DialogBoxResAsync("errCannotImportXML")
            Return
        End If

        _bDirty = True  ' wczytaliśmy z XML, to warto zapisać

        Dim oNodes As Xml.XmlNodeList = oItemsXml.DocumentElement.GetElementsByTagName("item")
        For Each oNode As Xml.XmlNode In oNodes
            Dim oNew As New Vblib.JedenItem
            oNew.Nazwa = oNode.Attributes?.ItemOf("Nazwa")?.InnerText
            oNew.Info = oNode.Attributes?.ItemOf("Info")?.InnerText
            oNew.Miejsce = oNode.Attributes?.ItemOf("Miejsce")?.InnerText
            oNew.Cena = oNode.Attributes?.ItemOf("Cena")?.InnerText
            Dim sTmp As String = oNode.Attributes?.ItemOf("Zalatwione")?.InnerText
            If sTmp IsNot Nothing And sTmp.ToLower = "true" Then oNew.Zalatwione = True

            Add(oNew)
        Next

    End Function

    Protected Sub LoadJSON(sContent As String)
        _Itemy = Newtonsoft.Json.JsonConvert.DeserializeObject(sContent, GetType(ObjectModel.ObservableCollection(Of JedenItem)))
    End Sub

    Protected Function ExportJSON() As String
        Return Newtonsoft.Json.JsonConvert.SerializeObject(_Itemy, Newtonsoft.Json.Formatting.Indented)
    End Function

    Public Shared Function TryImport(sNewName As String, sContent As String) As BazaItemySklepu
        If sContent.IndexOf("<ShoppingListItem><BaseItem>") > 3 Then
            Return ImportSmartShopping(sNewName, sContent)
        ElseIf sContent.IndexOf("|") > 1 Then
            Return ImportCSV(sNewName, sContent, "|")
        ElseIf sContent.IndexOf(vbTab) > 1 Then
            Return ImportCSV(sNewName, sContent, vbTab)
        End If

        DialogBoxRes("errUnknownFormat")
        Return Nothing
    End Function

    Public Function Export() As String
        Dim sTxt As String = ""
        For Each oItem As JedenItem In _Itemy
            sTxt = sTxt & $"{oItem.Nazwa}|{oItem.Miejsce}|{oItem.Info}|{oItem.Cena}" & vbCrLf
        Next

        Return sTxt
    End Function

    ''' <summary>
    ''' zwraca tekst do zapisania do OneDrive, po zapisaniu do Roaming
    ''' </summary>
    ''' <returns></returns>
    Protected Function Save() As String

        Dim sContent As String = ExportJSON()

        Dim sFilename As String = IO.Path.Combine(_sRoamingRootPath, _mFileNameBase & ".json")
        IO.File.WriteAllText(sFilename, sContent)

        Return sContent

    End Function

    ''' <summary>
    ''' wczytaj dane; zwraca TRUE gdy powinno się zapisać (bo było z XML)
    ''' </summary>
    ''' <param name="sODcontent">zawartość pliku z OneDrive, bądź "" gdy pliku nie ma</param>
    ''' <param name="dODdate">data tego pliku (nieistotna gdy pliku nie było)</param>
    Protected Async Function LoadAsync(sODcontent As String, dODdate As DateTimeOffset) As Task(Of Boolean)
        Dim bFromXml As Boolean = False
        Dim bShouldSave As Boolean = False

        Dim sFilename As String = IO.Path.Combine(_sRoamingRootPath, _mFileNameBase & ".json")
        If Not IO.File.Exists(sFilename) Then
            sFilename = IO.Path.Combine(_sRoamingRootPath, _mFileNameBase & ".xml")
            If Not IO.File.Exists(sFilename) Then Return False   ' nie ma żadnego pliku do wczytania - sytuacja której nie obsługujemy
            bFromXml = True
        End If

        Dim sContent As String = IO.File.ReadAllText(sFilename)
        Dim dLocalDate As DateTimeOffset = IO.File.GetLastWriteTime(sFilename)

        If _bUseOneDrive AndAlso sODcontent <> "" Then
            ' porownaj daty, z tolerancją 20 sekund
            Select Case Await SelectOneContentChoose(dODdate, dLocalDate, GetSettingsBool("LastSaveNoOD_" & _mFileNameBase))
                Case 0
                    ' oba takie same, przyjmuję bez zmian
                Case 1
                    ' użyj OneDrive
                    sContent = sODcontent
                    bFromXml = False
                Case 2
                    ' użyj lokalne
                    bShouldSave = True
            End Select

        End If

        If bFromXml Then
            Await ImportXML(sContent)
        Else
            LoadJSON(sContent)
        End If

        Return bFromXml Or bShouldSave

    End Function



End Class

#End Region
