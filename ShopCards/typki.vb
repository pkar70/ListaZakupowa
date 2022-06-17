Imports vb14 = Vblib.pkarlibmodule14

Public Class ListaSklepow
    Inherits VBlib_Karty.ListaSklepow

    'Private _Itemy As ObservableCollection(Of JedenItem)
    'Private _bDirty = False
    'Private ReadOnly _mFileNameBase = ""
    'Public ReadOnly NazwaSklepu As String

    Public Sub New()
        MyBase.New(App.GetJsonFolder.Path)
    End Sub

    Public Async Function LoadAsync(bUseOneDrive As Boolean) As Task
        _bUseOneDrive = bUseOneDrive

        Dim sODcontent As String = ""
        Dim dODdate As DateTimeOffset

        ' jeśli jest OneDrive, i jest tam plik, to go ściągnij
        If bUseOneDrive Then
            If App.mODroot IsNot Nothing Then
                Dim oFile As ODfile = Await App.mODroot.GetFileAsync(_mFileNameBase)
                If oFile IsNot Nothing Then
                    sODcontent = Await oFile.ReadContentAsync()
                    dODdate = oFile.GetLastModDate
                End If
            End If
        End If

        If Await MyBase.LoadLibAsync(sODcontent, dODdate) Then Await SaveAsync()

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
