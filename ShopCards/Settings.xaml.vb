Imports vb14 = Vblib.pkarlibmodule14
'Imports Vblib.Extensions
Imports pkar.UI.Extensions
Imports pkar.UI.Configs

Public NotInheritable Class Settings
    Inherits Page

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        Me.InitDialogs

#If DEBUG Then
        uiVersion.ShowAppVers(True)
#Else
        uiVersion.ShowAppVers(false)
#End If

        uiLastSync.Text = "Last sync date: " & vblib.GetSettingsDate("lastSync").ToString("yyyy.MM.dd HH:mm")
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
