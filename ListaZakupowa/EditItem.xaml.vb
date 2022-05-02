Imports vb14 = VBlib.pkarlibmodule14

Public NotInheritable Class EditItem
    Inherits Page

    Private _bAdding As Boolean = False ' true gdy dodajemy, false gdy edytujemy

    Private Sub FillComboMiejsc(sCurrent As String)

        uiMiejsca.Items.Clear()

        Dim oCBItem As New ComboBoxItem

        For Each sGrp As String In From c In App.moBazaSklepu.GetList Select c.Miejsce Distinct
            oCBItem = New ComboBoxItem
            oCBItem.Content = sGrp
            If sGrp = sCurrent Then oCBItem.IsSelected = True
            uiMiejsca.Items.Add(oCBItem)
        Next

        oCBItem = New ComboBoxItem
        oCBItem.Content = "--" & vb14.GetLangString("msgAddNewGroip")
        uiMiejsca.Items.Add(oCBItem)

    End Sub

    Private Async Sub uiOK_Click(sender As Object, e As RoutedEventArgs)
        If uiNazwa.Text.Length < 2 Then
            vb14.DialogBoxRes("errNameTooShort")
            Exit Sub
        End If

        Dim bChanged As Boolean = False

        If App.moEditingItem.Nazwa <> uiNazwa.Text Then bChanged = True
        If App.moEditingItem.Info <> uiInfo.Text Then bChanged = True
        If App.moEditingItem.Cena <> uiCena.Text Then bChanged = True

        App.moEditingItem.Nazwa = uiNazwa.Text
        App.moEditingItem.Info = uiInfo.Text
        App.moEditingItem.Cena = uiCena.Text

        Dim oCBI As ComboBoxItem = TryCast(uiMiejsca.SelectedItem, ComboBoxItem)
        Dim sTmp As String = If(oCBI?.Content?.ToString, "")
        If App.moEditingItem.Miejsce <> sTmp Then bChanged = True
        App.moEditingItem.Miejsce = sTmp

        ' gdy edytujemy, to przecież edytujemy obiekt, powinno być od razu widać zmianę w App.moBazaSklepu
        ' więc nie trzeba żadnych podmian na liście

        If _bAdding Then App.moBazaSklepu.Add(App.moEditingItem)

        If _bAdding OrElse bChanged Then Await App.moBazaSklepu.SaveAsync()

        Me.Frame.GoBack()
    End Sub

    Private Sub uiPage_Loaded(sender As Object, e As RoutedEventArgs)
        uiSklep.Text = App.moBazaSklepu.NazwaSklepu

        uiNazwa.Text = App.moEditingItem.Nazwa
        uiInfo.Text = App.moEditingItem.Info
        uiCena.Text = App.moEditingItem.Cena

        _bAdding = (uiNazwa.Text = "")

        FillComboMiejsc(App.moEditingItem.Miejsce)

    End Sub

    Private Sub uiCancel_Click(sender As Object, e As RoutedEventArgs)
        Me.Frame.GoBack()
    End Sub

    Private Sub uiAddCat_Click(sender As Object, e As RoutedEventArgs)
        Dim sName As String = uiAddCat.Text
        If sName.Length < 2 Then Exit Sub

        Dim oCBItem As New ComboBoxItem
        oCBItem.Content = sName
        uiMiejsca.Items.Add(oCBItem)

        uiAddCatFlyout.Hide()
        uiAddCat.Text = ""    ' zeby nie zostawala poprzednia nazwa
    End Sub
End Class
