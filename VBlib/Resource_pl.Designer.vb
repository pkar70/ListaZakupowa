﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System
Imports System.Reflection

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Friend Class Resource_PL
        
        Private Shared resourceMan As Global.System.Resources.ResourceManager
        
        Private Shared resourceCulture As Global.System.Globalization.CultureInfo
        
        <Global.System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>  _
        Friend Sub New()
            MyBase.New
        End Sub
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("Vblib.Resource_PL", GetType(Resource_PL).GetTypeInfo.Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to PL.
        '''</summary>
        Friend Shared ReadOnly Property _lang() As String
            Get
                Return ResourceManager.GetString("_lang", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Wystąpił błąd.
        '''</summary>
        Friend Shared ReadOnly Property errAnyError() As String
            Get
                Return ResourceManager.GetString("errAnyError", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to BŁĄD konwersji XML.
        '''</summary>
        Friend Shared ReadOnly Property errCannotImportXML() As String
            Get
                Return ResourceManager.GetString("errCannotImportXML", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to BŁAD: nie wczytano żadnej linii z pliku CSV.
        '''</summary>
        Friend Shared ReadOnly Property errCSVempty() As String
            Get
                Return ResourceManager.GetString("errCSVempty", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to BŁAD: brak zmiany linii w pliku CSV.
        '''</summary>
        Friend Shared ReadOnly Property errCSVnoLine() As String
            Get
                Return ResourceManager.GetString("errCSVnoLine", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to BŁAD ściągania danych z http.
        '''</summary>
        Friend Shared ReadOnly Property errHttpError() As String
            Get
                Return ResourceManager.GetString("errHttpError", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to BŁAD: nazwa za krótka.
        '''</summary>
        Friend Shared ReadOnly Property errNameTooShort() As String
            Get
                Return ResourceManager.GetString("errNameTooShort", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nie udało się zapisać listy zakupów w sklepie.
        '''</summary>
        Friend Shared ReadOnly Property errNieudanySaveListyItemow() As String
            Get
                Return ResourceManager.GetString("errNieudanySaveListyItemow", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nie udało się zapisać listy sklepów.
        '''</summary>
        Friend Shared ReadOnly Property errNieudanySaveListySklepow() As String
            Get
                Return ResourceManager.GetString("errNieudanySaveListySklepow", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Błąd dostępu do katalogu Roaming.
        '''</summary>
        Friend Shared ReadOnly Property errNoRoamFolder() As String
            Get
                Return ResourceManager.GetString("errNoRoamFolder", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to BŁĄD: nie ma folderu roaming?.
        '''</summary>
        Friend Shared ReadOnly Property errNoRoamFolder1() As String
            Get
                Return ResourceManager.GetString("errNoRoamFolder1", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to BŁĄD: za krótki tekst na import.
        '''</summary>
        Friend Shared ReadOnly Property errTooShortImport() As String
            Get
                Return ResourceManager.GetString("errTooShortImport", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to BŁĄD: nieznany format dla importu.
        '''</summary>
        Friend Shared ReadOnly Property errUnknownFormat() As String
            Get
                Return ResourceManager.GetString("errUnknownFormat", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Listy zakupowe.
        '''</summary>
        Friend Shared ReadOnly Property manifestAppName() As String
            Get
                Return ResourceManager.GetString("manifestAppName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to dodaj nowy.
        '''</summary>
        Friend Shared ReadOnly Property msgAddNewGroip() As String
            Get
                Return ResourceManager.GetString("msgAddNewGroip", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Jednak nie....
        '''</summary>
        Friend Shared ReadOnly Property msgCancelSklep() As String
            Get
                Return ResourceManager.GetString("msgCancelSklep", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Dane zostały zmienione zarówno lokalnie jak i w Cloud. Które powinienem użyć?.
        '''</summary>
        Friend Shared ReadOnly Property msgConflictModifiedODandLocal() As String
            Get
                Return ResourceManager.GetString("msgConflictModifiedODandLocal", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Lokalne.
        '''</summary>
        Friend Shared ReadOnly Property msgConflictUseLocal() As String
            Get
                Return ResourceManager.GetString("msgConflictUseLocal", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Cloud.
        '''</summary>
        Friend Shared ReadOnly Property msgConflictUseOD() As String
            Get
                Return ResourceManager.GetString("msgConflictUseOD", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Skasować dane z OneDrive? Jeśli tego nie zrobisz, to jak później ponownie włączysz OneDrive to dane z niego nadpiszą dane lokalne..
        '''</summary>
        Friend Shared ReadOnly Property msgDeleteFromOneDrive() As String
            Get
                Return ResourceManager.GetString("msgDeleteFromOneDrive", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Dodaj.
        '''</summary>
        Friend Shared ReadOnly Property msgDodajSklep() As String
            Get
                Return ResourceManager.GetString("msgDodajSklep", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Podaj nazwę dla importowanej listy:.
        '''</summary>
        Friend Shared ReadOnly Property msgEnterName() As String
            Get
                Return ResourceManager.GetString("msgEnterName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Wyeksportowano Listę Zakupową do Schowka.
        '''</summary>
        Friend Shared ReadOnly Property msgExportClip() As String
            Get
                Return ResourceManager.GetString("msgExportClip", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Przesłać teraz dane do OneDrive?.
        '''</summary>
        Friend Shared ReadOnly Property msgExportRoamToOneDrive() As String
            Get
                Return ResourceManager.GetString("msgExportRoamToOneDrive", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Podaj nazwę sklepu:.
        '''</summary>
        Friend Shared ReadOnly Property msgNazwaSklepu() As String
            Get
                Return ResourceManager.GetString("msgNazwaSklepu", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nie można włączyć OneDrive gdy nie ma dostępnej sieci .
        '''</summary>
        Friend Shared ReadOnly Property msgNoNetworkNoOneDrive() As String
            Get
                Return ResourceManager.GetString("msgNoNetworkNoOneDrive", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Przejść do wczytanej listy?.
        '''</summary>
        Friend Shared ReadOnly Property msgPrzejscDoNowego() As String
            Get
                Return ResourceManager.GetString("msgPrzejscDoNowego", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Poniechaj.
        '''</summary>
        Friend Shared ReadOnly Property resDlgCancel() As String
            Get
                Return ResourceManager.GetString("resDlgCancel", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Kontynuuj.
        '''</summary>
        Friend Shared ReadOnly Property resDlgContinue() As String
            Get
                Return ResourceManager.GetString("resDlgContinue", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Nie.
        '''</summary>
        Friend Shared ReadOnly Property resDlgNo() As String
            Get
                Return ResourceManager.GetString("resDlgNo", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Tak.
        '''</summary>
        Friend Shared ReadOnly Property resDlgYes() As String
            Get
                Return ResourceManager.GetString("resDlgYes", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Usuń.
        '''</summary>
        Friend Shared ReadOnly Property resPrevShopDelete() As String
            Get
                Return ResourceManager.GetString("resPrevShopDelete", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Taka lista kiedyś istniała. Co z nią zrobić?.
        '''</summary>
        Friend Shared ReadOnly Property resPrevShopExist() As String
            Get
                Return ResourceManager.GetString("resPrevShopExist", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to Przywróc.
        '''</summary>
        Friend Shared ReadOnly Property resPrevShopRetain() As String
            Get
                Return ResourceManager.GetString("resPrevShopRetain", resourceCulture)
            End Get
        End Property
    End Class
End Namespace
