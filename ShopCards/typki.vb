Imports vb14 = Vblib.pkarlibmodule14
Imports Vblib.Extensions


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
        ' zostało poprawione z poprzedniej wersji pliku

        FillIconFileName(GetFolderIkonki.Path)

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

    ''' <summary>
    ''' Dla podanego sklepu ściąga ikonkę z WWW i daje do Cache
    ''' </summary>
    ''' <param name="oSklep"></param>
    ''' <returns></returns>
    Public Async Function SciagnijIkonke(oSklep As VBlib_Karty.JedenSklep) As Task(Of Boolean)

        Dim bChanged As Boolean = False

        If IO.File.Exists(oSklep.sIconPathname) Then Return bChanged

        ' aktualizacja ze starej wersji pliku - sprawdzam pole które jest nowe
        Dim sFilename As String = oSklep.sName.ToValidPath
        sFilename &= ".icon"
        ' operacje na nazwach
        If oSklep.sIconUri.ToLowerInvariant.Contains(".png") Then
            sFilename &= ".png"
        ElseIf oSklep.sIconUri.ToLowerInvariant.Contains(".ico") Then
            sFilename &= ".ico"
        Else
            ' biedronka nie ma ico/png, ale to jest .png - zresztą nazwa jest i tak chyba nieważna
            sFilename &= ".png"
        End If

        Dim sPath As String = GetFolderIkonki.Path
        oSklep.sIconFilename = Await SaveUriAsFileAsync(oSklep.sIconUri, sPath, sFilename)

        Return True

    End Function

    ''' <summary>
    '''  ściąga z sUri do pliku sPath \ sFileName, zwraca nazwę pliku lub "", gdy się nie udało
    ''' </summary>
    ''' <param name="sUri"></param>
    ''' <param name="sPath"></param>
    ''' <param name="sFileName"></param>
    ''' <returns>nazwa pliku z ikonką (nie path!) lub "", gdy się nie udało</returns>
    Private Async Function SaveUriAsFileAsync(sUri As String, sPath As String, sFileName As String) As Task(Of String)
        ' ściągnięcie ikonki, wedle oSklep.sIconUri, aktualizacja sIconFilename
        ' zwraca nazwę pliku lub "", gdy się nie udało

        Dim oFold As Windows.Storage.StorageFolder = Await Windows.Storage.StorageFolder.GetFolderFromPathAsync(sPath)
        Dim oFile As Windows.Storage.StorageFile

        If IO.File.Exists(IO.Path.Combine(sPath, sFileName)) Then
            oFile = Await oFold.TryGetItemAsync(sFileName)
        Else
            oFile = Await oFold.CreateFileAsync(sFileName)
        End If

        Dim oEngine As New Windows.Networking.BackgroundTransfer.BackgroundDownloader

        Dim oDown = oEngine.CreateDownload(New Uri(sUri), oFile)
        Await oDown.StartAsync
        Return sFileName ' oFile.Path
    End Function

    ''' <summary>
    ''' ściąga wszystkie ikonki których jeszcze ne ma
    ''' </summary>
    ''' <returns></returns>
    Public Async Function DownloadMissingIcons() As Task
        Dim bChanged As Boolean = False
        Dim sPath As String = GetFolderIkonki.Path

        For Each oSklep As VBlib_Karty.JedenSklep In App.moSklepy.GetList
            bChanged = bChanged Or Await SciagnijIkonke(oSklep)
        Next

        If bChanged Then Await App.moSklepy.SaveAsync

    End Function

    Private Function GetFolderIkonki() As Windows.Storage.StorageFolder
        Return Windows.Storage.ApplicationData.Current.LocalCacheFolder
    End Function


End Class
