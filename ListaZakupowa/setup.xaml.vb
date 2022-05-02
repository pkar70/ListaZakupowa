Imports vb14 = Vblib.pkarlibmodule14
Public NotInheritable Class setup
    Inherits Page

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiVersion.ShowAppVers()
        uiOneDrive.GetSettingsBool()
        uiSklepySort.GetSettingsBool()
        Me.ProgRingInit(True, True)

        ' nie można przełączać skoro nie ma sieci
        If Not NetIsIPavailable() AndAlso Not vb14.GetSettingsBool("uiOneDrive") Then
            uiOneDrive.IsEnabled = False
            ToolTipService.SetToolTip(uiOneDrive, vb14.GetLangString("msgNoNetworkNoOneDrive"))
        End If
    End Sub

    Private Async Function ExportToOneDrive() As Task
        '' niezależnie od tego co jest w App - bo wczytujemy z False
        'Dim moSklepy As New BazaSklepy
        'Await moSklepy.LoadAsync(False)

        Await App.moSklepy.SaveAsync(True)

        Dim oLista As ObservableCollection(Of Vblib.JedenSklep) = App.moSklepy.GetList
        Me.ProgRingMaxVal(oLista.Count)

        For Each oSklep As Vblib.JedenSklep In oLista
            Dim oItemySklepu As New BazaItemySklepu(oSklep.Nazwa)
            Await oItemySklepu.LoadAsync(False)
            Await oItemySklepu.SaveAsync(True)
            Me.ProgRingInc
        Next

    End Function

    Private Async Sub uiOk_Click(sender As Object, e As RoutedEventArgs)
        uiSklepySort.SetSettingsBool()

        ' jeśli zaszła zmiana używania OneDrive...
        If uiOneDrive.IsOn <> vb14.GetSettingsBool("uiOneDrive") Then
            If uiOneDrive.IsOn Then
                If App.mODroot Is Nothing Then App.mODroot = Await ODclient.GetRootAsync()  ' BARDZO WAZNE: włącza OneDrive!
                If App.mODroot Is Nothing Then
                    ' nie powinno się zdarzyć, bo gdy nie ma otwieralnego OneDrive to IsEnable wyłączamy na możliwości włączenia OneDrive
                    vb14.DialogBox("Cannot open OneDrive")
                    Return
                End If
                If Await vb14.DialogBoxResYNAsync("msgExportRoamToOneDrive") Then
                    Me.ProgRingShow(True)
                    Await ExportToOneDrive()
                    Me.ProgRingShow(False)
                End If
            Else
                If Await vb14.DialogBoxResYNAsync("msgDeleteFromOneDrive") Then
                    Dim oNames As New List(Of String)
                    oNames.Add("int_sklepy.txt")
                    For Each oSklep As Vblib.JedenSklep In App.moSklepy.GetList
                        oNames.Add(oSklep.Nazwa & ".json")
                    Next

                    Await App.mODroot.RemoveFilesAsync(oNames)

                End If
            End If
        End If

        uiOneDrive.SetSettingsBool()

        Me.GoBack
    End Sub
End Class
