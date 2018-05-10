' The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class EditItem
    Inherits Page

    Dim bComboFilled As Boolean = False

    Protected Overrides Sub onNavigatedTo(e As NavigationEventArgs)
        uiSklep.Text = App.msNazwaSklepu
        Dim sTxt As String = e.Parameter.ToString ' uiMiejsca

        uiMiejsca.Items.Clear()

        Dim oCBItem As ComboBoxItem
        Dim aMsc As String() = sTxt.Split("|")
        For Each sMsc As String In aMsc
            If sMsc.Length > 1 Then
                oCBItem = New ComboBoxItem
                oCBItem.Content = sMsc
                uiMiejsca.Items.Add(oCBItem)
            End If
        Next

        bComboFilled = True
        'oCBItem = New ComboBoxItem
        'oCBItem.Content = "--add new"
        'uiMiejsca.Items.Add(oCBItem)

    End Sub

    Private Sub uiOK_Click(sender As Object, e As RoutedEventArgs)
        If uiNazwa.Text.Length < 2 Then
            App.DialogBoxRes("errNameTooShort")
            Exit Sub
        End If

        Dim bError As Boolean = False
        Dim sError As String = ""

        Try
            App.oItemek.Nazwa = uiNazwa.Text
            ' App.oItemek.Miejsce = uiMiejsce.Text
            App.oItemek.Info = uiInfo.Text
            App.oItemek.Cena = uiCena.Text
        Catch ex As Exception
            App.DialogBoxError(1, ex.Message)
            Exit Sub
        End Try

        'If bError Then
        '    App.DialogBoxError(sError)
        '    Exit Sub
        'End If

        Try
            Dim iTmp As Integer = uiMiejsca.SelectedIndex
            If iTmp < 0 Then
                App.oItemek.Miejsce = ""
            Else
                ' Dim sTxt As String = uiMiejsca.Items.ElementAt(iTmp).Content
                ' App.oItemek.Miejsce = sTxt
                App.oItemek.Miejsce = uiMiejsca.SelectionBoxItem
            End If
        Catch ex As Exception
            App.DialogBoxError(2, ex.Message)
            Exit Sub
            'sError = ex.Message
            'bError = True
        End Try

        'If bError Then
        '    App.DialogBoxError(sError)
        '    Exit Sub
        'End If

        App.mbReadFromApp = True
        Me.Frame.GoBack()

    End Sub

    Private Async Sub uiPage_Loaded(sender As Object, e As RoutedEventArgs)
        uiSklep.Text = App.msNazwaSklepu
        uiNazwa.Text = If(App.oItemek.Nazwa Is Nothing, "", App.oItemek.Nazwa)
        uiInfo.Text = If(App.oItemek.Info, "")  ' to jest to samo co wyzej :)
        uiCena.Text = If(App.oItemek.Cena, "")
        ' uiMiejsce.Text = If(App.oItemek.Miejsce Is Nothing, "", App.oItemek.Miejsce)

        Dim sTxt As String = ""
        If App.oItemek.Miejsce IsNot Nothing Then
            sTxt = App.oItemek.Miejsce
        End If

        ' moze to spowoduje ze nie bedzie crash przy Edit - a tak sie zdarzylo testerom Microsoft
        ' zas w Internet mozna znalezc ze jest problem przy zmianie Combo gdy sie go jednoczesnie laduje
        Dim iGuard As Integer = 10
        While iGuard < 10 And Not bComboFilled
            iGuard += 1
            Await Task.Delay(10)
        End While


        If sTxt <> "" Then
            Dim i As Integer
            Try
                Dim iMax As Integer = uiMiejsca.Items.Count
                ' Debug.WriteLine("W comboboxie jest itemow " & iMax.ToString)

                App.IgnoreLangOn()

                For i = 0 To uiMiejsca.Items.Count - 1      ' Dim As jest wyzej
                    ' Debug.WriteLine(uiMiejsca.Items.ElementAt(i).Content)
                    If uiMiejsca.Items.ElementAt(i).Content = sTxt Then
                        uiMiejsca.SelectedIndex = i
                        Exit For
                    End If
                Next
                App.IgnoreLangOff()

            Catch ex As Exception
                App.DialogBoxError(3, ex.Message)
            End Try
        End If

    End Sub

    Private Sub uiMiejsca_Changed(sender As Object, e As SelectionChangedEventArgs) Handles uiMiejsca.SelectionChanged
        Dim iInd As Integer = uiMiejsca.SelectedIndex
        ' 20180602: mi też error(4) przy edycji takiego bez kategorii?
        If iInd < 0 Then Exit Sub

        Try
            App.IgnoreLangOn()
            App.oItemek.Miejsce = uiMiejsca.Items.ElementAt(iInd).Content
            App.IgnoreLangOff()
        Catch ex As Exception
            App.DialogBoxError(4, ex.Message)
        End Try
    End Sub

    Private Sub uiCancel_Click(sender As Object, e As RoutedEventArgs)
        App.mbReadFromApp = False
        Me.Frame.GoBack()
    End Sub

    Private Sub uiAddCat_Click(sender As Object, e As RoutedEventArgs)
        Dim sName As String = uiAddCat.Text
        If sName.Length < 2 Then Exit Sub

        Dim oCBItem As ComboBoxItem = New ComboBoxItem
        oCBItem.Content = sName
        uiMiejsca.Items.Add(oCBItem)

        uiAddCatFlyout.Hide()
        uiAddCat.Text = ""    ' zeby nie zostawala poprzednia nazwa
    End Sub
End Class
