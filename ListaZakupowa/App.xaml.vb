Imports System.Xml.Serialization
Imports Windows.Storage
Imports Windows.UI.Core
Imports Windows.UI.Popups
''' <summary>
''' Provides application-specific behavior to supplement the default Application class.
''' </summary>
<XmlType("item")>
Public Class BazaItem
    <XmlAttribute()>
    Public Property Nazwa As String
    <XmlAttribute()>
    Public Property Zalatwione As Boolean
    <XmlAttribute()>
    Public Property Info As String
    <XmlAttribute()>
    Public Property Cena As String
    <XmlAttribute()>
    Public Property Miejsce As String
    <XmlIgnore>
    Public Property ShowTBlock As Visibility = Visibility.Visible
    <XmlIgnore>
    Public Property ShowTBox As Visibility = Visibility.Collapsed

End Class

NotInheritable Class App
    Inherits Application

    Public Shared oItemek As BazaItem
    Public Shared msNazwaSklepu As String
    Public Shared mbReadFromApp As Boolean = False
    Public Shared msUpdatingItemName As String
    Private Shared msOldLang As String = ""

    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active

        If rootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = New Frame()

            AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed

            ' PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
            AddHandler rootFrame.Navigated, AddressOf OnNavigatedAddBackButton
            AddHandler SystemNavigationManager.GetForCurrentView().BackRequested, AddressOf OnBackButtonPressed

            If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
                ' TODO: Load state from previously suspended application
            End If
            ' Place the frame in the current Window
            Window.Current.Content = rootFrame
        End If

        If e.PrelaunchActivated = False Then
            If rootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                rootFrame.Navigate(GetType(MainPage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End If
    End Sub

    ''' <summary>
    ''' Invoked when Navigation to a certain page fails
    ''' </summary>
    ''' <param name="sender">The Frame which failed navigation</param>
    ''' <param name="e">Details about the navigation failure</param>
    Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
    End Sub

    ' PKAR added wedle https://stackoverflow.com/questions/39262926/uwp-hardware-back-press-work-correctly-in-mobile-but-error-with-pc
    Private Sub OnNavigatedAddBackButton(sender As Object, e As NavigationEventArgs)
        Dim oFrame As Frame = TryCast(sender, Frame)
        Dim oNavig As SystemNavigationManager = SystemNavigationManager.GetForCurrentView

        If oFrame.CanGoBack Then
            oNavig.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible
        Else
            oNavig.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed
        End If

    End Sub

    Private Sub OnBackButtonPressed(sender As Object, e As BackRequestedEventArgs)
        Try
            TryCast(Window.Current.Content, Frame).GoBack()
            e.Handled = True
        Catch ex As Exception
        End Try
    End Sub

    ''' <summary>
    ''' Invoked when application execution is being suspended.  Application state is saved
    ''' without knowing whether the application will be terminated or resumed with the contents
    ''' of memory still intact.
    ''' </summary>
    ''' <param name="sender">The source of the suspend request.</param>
    ''' <param name="e">Details about the suspend request.</param>
    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        ' TODO: Save application state and stop any background activity
        deferral.Complete()
    End Sub

    Public Shared Async Sub DialogBox(sMsg As String)
        Dim oMsg As MessageDialog = New MessageDialog(sMsg)
        Await oMsg.ShowAsync
    End Sub
    Public Shared Async Sub DialogBoxRes(sMsgResId As String)
        Dim oMsg As MessageDialog = New MessageDialog(
            Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView().GetString(sMsgResId))
        Await oMsg.ShowAsync
    End Sub

    Public Shared Async Sub DialogBoxError(iNr As Integer, sMsg As String)
        Dim sTxt As String = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView().GetString("errAnyError")
        sTxt = sTxt & " (" & iNr & ")" & vbCrLf & sMsg
        Dim oMsg As MessageDialog = New MessageDialog(sTxt)
        Await oMsg.ShowAsync
    End Sub

    Public Shared Async Function DialogBoxResYN(sMsgResId As String, Optional sYesResId As String = "resDlgYes", Optional sNoResId As String = "resDlgNo") As Task(Of Boolean)
        Dim sMsg, sYes, sNo As String

        With Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView()
            sMsg = .GetString(sMsgResId)
            sYes = .GetString(sYesResId)
            sNo = .GetString(sNoResId)
        End With

        If sMsg = "" Then sMsg = sMsgResId  ' zabezpieczenie na brak string w resource
        If sYes = "" Then sYes = sYesResId
        If sNo = "" Then sNo = sNoResId

        Dim oMsg As MessageDialog = New MessageDialog(sMsg)
        Dim oYes As UICommand = New UICommand(sYes)
        Dim oNo As UICommand = New UICommand(sNo)
        oMsg.Commands.Add(oYes)
        oMsg.Commands.Add(oNo)
        oMsg.DefaultCommandIndex = 1    ' default: No
        Dim oCmd As IUICommand = Await oMsg.ShowAsync
        If oCmd Is Nothing Then Return False
        If oCmd.Label = sYes Then Return True

        Return False

    End Function

    Public Shared Async Function GetRoamingFile(sName As String, bCreate As Boolean) As Task(Of StorageFile)
        Dim oFold As StorageFolder = Windows.Storage.ApplicationData.Current.RoamingFolder
        If oFold Is Nothing Then
            DialogBoxRes("errNoRoamFolder")
            Return Nothing
        End If

        Dim bErr As Boolean = False
        Dim oFile As StorageFile = Nothing
        Try
            If bCreate Then
                oFile = Await oFold.CreateFileAsync(sName, CreationCollisionOption.ReplaceExisting)
            Else
                oFile = Await oFold.GetFileAsync(sName)
            End If
        Catch ex As Exception
            bErr = True
        End Try
        If bErr Then
            Return Nothing
        End If

        Return oFile
    End Function

    Public Shared Async Function ReadRoamingFile(sName As String) As Task(Of String)
        Dim oFile As StorageFile = Await GetRoamingFile(sName, False)
        If oFile Is Nothing Then Return ""

        Dim sTxt As String = Await FileIO.ReadTextAsync(oFile)
        Return sTxt

    End Function

    Public Shared Async Function SaveRoamingFile(sName As String, sContens As String, sRoot As String) As Task

        Dim oFile As StorageFile = Await GetRoamingFile(sName, True)
        If oFile Is Nothing Then Exit Function

        If sRoot <> "" Then sContens = "<" & sRoot & ">" & sContens & "</" & sRoot & ">"
        Await FileIO.WriteTextAsync(oFile, sContens)

    End Function

    Public Shared Sub IgnoreLangOn()
        Dim sOldLang As String = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride
        If sOldLang <> "" Then msOldLang = sOldLang     ' zabezpieczenie przed podwojnym wywolaniem

        Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = ""
    End Sub

    Public Shared Sub IgnoreLangOff()
        If msOldLang <> "" Then Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = msOldLang
    End Sub

End Class
