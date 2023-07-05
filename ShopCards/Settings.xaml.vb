Imports vb14 = Vblib.pkarlibmodule14
'Imports Vblib.Extensions

Public NotInheritable Class Settings
    Inherits Page

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiVersion.ShowAppVers()
        uiLastSync.Text = "Last sync date: " & vb14.GetSettingsDate("lastSync").ToString("yyyy.MM.dd HH:mm")
        uiWalkSpeed.GetSettingsInt()
        uiGPSPrec.GetSettingsInt()
        uiShowSyncSummary.GetSettingsBool()
    End Sub

    Private Sub uiSave_Click(sender As Object, e As RoutedEventArgs)
        uiShowSyncSummary.SetSettingsBool()
        uiWalkSpeed.SetSettingsInt()
        uiGPSPrec.SetSettingsInt()

        Me.GoBack
    End Sub
End Class
