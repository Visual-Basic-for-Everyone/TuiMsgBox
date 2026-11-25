Imports System
Imports System.Text

Module Program
    Sub Main(args As String())
        Console.OutputEncoding = Encoding.UTF8

        ' 1. Simple Info Box
        ConsoleMessageBox.Show("Configuration loaded successfully from appsettings.json.",
                               "System Ready",
                               MsgBoxStyleType.Success)

        ' 2. Interactive Yes/No Confirmation
        Dim choice As MsgBoxResultType = ConsoleMessageBox.Show(
            "This action will overwrite existing database records. Do you want to continue?",
            "Confirm Migration",
            MsgBoxStyleType.Warning,
            MsgBoxButtonsType.YesNo)

        If choice = MsgBoxResultType.Yes Then
            ConsoleMessageBox.Show("Database migration completed.", "Done", MsgBoxStyleType.Info)
        Else
            ConsoleMessageBox.Show("Operation aborted by user.", "Cancelled", MsgBoxStyleType.Error)
        End If
    End Sub
End Module
