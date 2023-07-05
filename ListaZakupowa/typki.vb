
Imports vb14 = Vblib.pkarlibmodule14
'Imports Vblib.Extensions


#Region "lista sklepów"

' pierwotna wersja: plik ROAMING "int_sklepy.xml", XmlType("sklep"), z XML attr "Nazwa", 
' nowsza wersja: plik ROAMING/OneDrive "int_sklepy.txt", jedna linijka na jedną nazwę

' teraz import jest via XmlDoc, żeby można było jakby co to przerzucić to do vblib


Partial Public Class BazaSklepy
    Inherits Vblib.BazaSklepy

    Public Sub New()
        MyBase.New(Windows.Storage.ApplicationData.Current.RoamingFolder.Path)
    End Sub

    Public Async Function LoadAsync(bUseOneDrive As Boolean) As Task

        _bUseOneDrive = bUseOneDrive

        Dim sODcontent As String = ""
        Dim dODdate As DateTimeOffset

        ' jeśli jest OneDrive, i jest tam plik, to go ściągnij
        If bUseOneDrive Then
            If App.mODroot IsNot Nothing Then
                Dim oFile As ODfile = Await App.mODroot.GetFileAsync(FILENAME_TXT)
                If oFile IsNot Nothing Then
                    sODcontent = Await oFile.ReadContentAsync()
                    dODdate = oFile.GetLastModDate
                End If
            End If
        End If

        If Await MyBase.LoadAsync(sODcontent, dODdate) Then Await SaveAsync()

    End Function

    Public Async Function SaveAsync(Optional bForceOneDrive As Boolean = False) As Task
        ' If Not (_bDirty Or bForceOneDrive) Then Return

        ' zapisujemy zawsze do Roaming, a do OneDrive jak jest zgoda
        Dim sTxt As String = Save()
        vb14.SetSettingsBool("sklepyLastSaveNoOD", False)

        If _bUseOneDrive Or bForceOneDrive Then
            If App.mODroot Is Nothing OrElse Not Await App.mODroot.FileWriteStringAsync(FILENAME_TXT, sTxt) Then
                vb14.SetSettingsBool("sklepyLastSaveNoOD", True)
            End If
        End If

    End Function

End Class

#End Region

#Region "itemy w sklepie"

Public Class BazaItemySklepu
    Inherits Vblib.BazaItemySklepu

    'Private _Itemy As ObservableCollection(Of JedenItem)
    'Private _bDirty = False
    'Private ReadOnly _mFileNameBase = ""
    'Public ReadOnly NazwaSklepu As String

    Public Sub New(sNazwaSklepu As String)
        MyBase.New(sNazwaSklepu, Windows.Storage.ApplicationData.Current.RoamingFolder.Path)
    End Sub

    Public Async Function LoadAsync(bUseOneDrive As Boolean) As Task
        _bUseOneDrive = bUseOneDrive

        Dim sODcontent As String = ""
        Dim dODdate As DateTimeOffset

        ' jeśli jest OneDrive, i jest tam plik, to go ściągnij
        If bUseOneDrive Then
            If App.mODroot IsNot Nothing Then
                Dim oFile As ODfile = Await App.mODroot.GetFileAsync(_mFileNameBase & ".json")
                If oFile IsNot Nothing Then
                    sODcontent = Await oFile.ReadContentAsync()
                    dODdate = oFile.GetLastModDate
                End If
            End If
        End If

        If Await MyBase.LoadAsync(sODcontent, dODdate) Then Await SaveAsync()

    End Function

    Public Async Function SaveAsync(Optional bForceOneDrive As Boolean = False) As Task

        ' If Not (_bDirty Or bForceOneDrive) Then Return
        ' bo nie ma informacji o zmianie!

        ' zapisujemy zawsze do Roaming, a do OneDrive jak jest zgoda
        Dim sTxt As String = Save()

        vb14.SetSettingsBool("LastSaveNoOD_" & _mFileNameBase, False)

        If _bUseOneDrive Or bForceOneDrive Then
            If App.mODroot Is Nothing OrElse Not Await App.mODroot.FileWriteStringAsync(_mFileNameBase & ".json", sTxt) Then
                vb14.SetSettingsBool("LastSaveNoOD_" & _mFileNameBase, True)
            End If
        End If

    End Function

    Protected Overrides Async Sub Finalize()
        'If bDirty Then
        Await SaveAsync()
        _bDirty = False
        'End If
        Clear()
    End Sub

End Class

#End Region

Public Module RoamingFile
    Public Async Function GetRoamingFileAsync(sName As String, bCreate As Boolean) As Task(Of Windows.Storage.StorageFile)
        Dim oFold As Windows.Storage.StorageFolder = Windows.Storage.ApplicationData.Current.RoamingFolder
        If oFold Is Nothing Then
            vb14.DialogBoxRes("errNoRoamFolder")
            Return Nothing
        End If

        Dim bErr As Boolean = False
        Dim oFile As Windows.Storage.StorageFile = Nothing
        Try
            If bCreate Then
                oFile = Await oFold.CreateFileAsync(sName, Windows.Storage.CreationCollisionOption.ReplaceExisting)
            Else
                oFile = Await oFold.TryGetItemAsync(sName)
            End If
        Catch ex As Exception
            bErr = True
        End Try
        If bErr Then
            Return Nothing
        End If

        Return oFile
    End Function


End Module