Imports System
Imports System.Collections.Generic

Public Enum MsgBoxStyleType
    Info
    Success
    Warning
    [Error]
    Question
End Enum

Public Enum MsgBoxButtonsType
    OK
    OKCancel
    YesNo
    YesNoCancel
End Enum

Public Enum MsgBoxResultType
    OK
    Cancel
    Yes
    No
End Enum

Public Module ConsoleMessageBox

    Public Function Show(message As String,
                         Optional title As String = "Notification",
                         Optional style As MsgBoxStyleType = MsgBoxStyleType.Info,
                         Optional buttons As MsgBoxButtonsType = MsgBoxButtonsType.OK) As MsgBoxResultType

        ' 1. Configure colors and badge based on style
        Dim borderColor As ConsoleColor
        Dim badge As String

        Select Case style
            Case MsgBoxStyleType.Success
                borderColor = ConsoleColor.Green
                badge = "[✔] "
            Case MsgBoxStyleType.Warning
                borderColor = ConsoleColor.Yellow
                badge = "[!] "
            Case MsgBoxStyleType.Error
                borderColor = ConsoleColor.Red
                badge = "[✖] "
            Case MsgBoxStyleType.Question
                borderColor = ConsoleColor.Cyan
                badge = "[?] "
            Case Else ' Info
                borderColor = ConsoleColor.Blue
                badge = "[i] "
        End Select

        ' 2. Setup available buttons
        Dim buttonList As New List(Of MsgBoxResultType)
        Select Case buttons
            Case MsgBoxButtonsType.OK
                buttonList.Add(MsgBoxResultType.OK)
            Case MsgBoxButtonsType.OKCancel
                buttonList.AddRange({MsgBoxResultType.OK, MsgBoxResultType.Cancel})
            Case MsgBoxButtonsType.YesNo
                buttonList.AddRange({MsgBoxResultType.Yes, MsgBoxResultType.No})
            Case MsgBoxButtonsType.YesNoCancel
                buttonList.AddRange({MsgBoxResultType.Yes, MsgBoxResultType.No, MsgBoxResultType.Cancel})
        End Select

        ' 3. Calculate box dimensions and wrap text
        Dim maxWidth As Integer = Math.Min(64, Math.Max(36, Console.WindowWidth - 6))
        Dim innerWidth As Integer = maxWidth - 4
        Dim wrappedLines As List(Of String) = WrapText(message, innerWidth)
        Dim headerText As String = Truncate(badge & title, innerWidth)

        Dim origFg As ConsoleColor = Console.ForegroundColor
        Dim origBg As ConsoleColor = Console.BackgroundColor
        Dim origCursorVisible As Boolean = True

        Try
            origCursorVisible = Console.CursorVisible
            Console.CursorVisible = False
        Catch
            ' Ignore if redirected console doesn't support CursorVisible
        End Try

        Dim selectedIndex As Integer = 0

        Try
            Console.WriteLine()

            ' Draw Top Border
            WriteColored("╔" & New String("═"c, maxWidth - 2) & "╗" & Environment.NewLine, borderColor)

            ' Draw Title Bar
            WriteColored("║ ", borderColor)
            WriteColored(headerText.PadRight(innerWidth), borderColor)
            WriteColored(" ║" & Environment.NewLine, borderColor)

            ' Draw Divider
            WriteColored("╠" & New String("─"c, maxWidth - 2) & "╣" & Environment.NewLine, borderColor)

            ' Draw Message Body
            For Each line As String In wrappedLines
                WriteColored("║ ", borderColor)
                Console.ForegroundColor = ConsoleColor.White
                Console.Write(line.PadRight(innerWidth))
                WriteColored(" ║" & Environment.NewLine, borderColor)
            Next

            ' Empty padding line above buttons
            WriteColored("║ " & New String(" "c, innerWidth) & " ║" & Environment.NewLine, borderColor)

            ' Record cursor row for interactive button redraws
            Dim buttonRow As Integer = Console.CursorTop
            WriteColored("║ " & New String(" "c, innerWidth) & " ║" & Environment.NewLine, borderColor)

            ' Draw Bottom Border
            WriteColored("╚" & New String("═"c, maxWidth - 2) & "╝" & Environment.NewLine, borderColor)

            ' 4. Interactive Input Loop
            While True
                RenderButtons(buttonRow, innerWidth, borderColor, buttonList, selectedIndex)

                Dim keyInfo As ConsoleKeyInfo = Console.ReadKey(intercept:=True)

                Select Case keyInfo.Key
                    Case ConsoleKey.LeftArrow, ConsoleKey.UpArrow
                        selectedIndex = (selectedIndex - 1 + buttonList.Count) Mod buttonList.Count

                    Case ConsoleKey.RightArrow, ConsoleKey.DownArrow, ConsoleKey.Tab
                        selectedIndex = (selectedIndex + 1) Mod buttonList.Count

                    Case ConsoleKey.Enter, ConsoleKey.Spacebar
                        Return buttonList(selectedIndex)

                    Case ConsoleKey.Escape
                        If buttonList.Contains(MsgBoxResultType.Cancel) Then Return MsgBoxResultType.Cancel
                        If buttonList.Contains(MsgBoxResultType.No) Then Return MsgBoxResultType.No
                        Return buttonList(0)

                    Case ConsoleKey.Y
                        If buttonList.Contains(MsgBoxResultType.Yes) Then Return MsgBoxResultType.Yes

                    Case ConsoleKey.N
                        If buttonList.Contains(MsgBoxResultType.No) Then Return MsgBoxResultType.No

                    Case ConsoleKey.O
                        If buttonList.Contains(MsgBoxResultType.OK) Then Return MsgBoxResultType.OK

                    Case ConsoleKey.C
                        If buttonList.Contains(MsgBoxResultType.Cancel) Then Return MsgBoxResultType.Cancel
                End Select
            End While

        Finally
            Console.ForegroundColor = origFg
            Console.BackgroundColor = origBg
            Try
                Console.CursorVisible = origCursorVisible
            Catch
            End Try
            Console.WriteLine()
        End Try

        Return MsgBoxResultType.OK
    End Function

    Private Sub RenderButtons(row As Integer, innerWidth As Integer, borderColor As ConsoleColor,
                              buttons As List(Of MsgBoxResultType), selectedIndex As Integer)
        Console.SetCursorPosition(0, row)
        WriteColored("║ ", borderColor)

        Dim totalButtonsWidth As Integer = 0
        Dim labels As New List(Of String)

        For Each btn In buttons
            Dim lbl As String = $" [ {btn.ToString().ToUpper()} ] "
            labels.Add(lbl)
            totalButtonsWidth += lbl.Length + 1
        Next
        totalButtonsWidth -= 1 ' Remove trailing space

        Dim leftPad As Integer = Math.Max(0, (innerWidth - totalButtonsWidth) \ 2)
        Dim rightPad As Integer = Math.Max(0, innerWidth - totalButtonsWidth - leftPad)

        Console.Write(New String(" "c, leftPad))

        For i As Integer = 0 To buttons.Count - 1
            If i = selectedIndex Then
                Console.BackgroundColor = borderColor
                Console.ForegroundColor = ConsoleColor.Black
                Console.Write(labels(i))
                Console.ResetColor()
            Else
                Console.ForegroundColor = ConsoleColor.Gray
                Console.Write(labels(i))
            End If

            If i < buttons.Count - 1 Then
                Console.Write(" ")
            End If
        Next

        Console.Write(New String(" "c, rightPad))
        WriteColored(" ║", borderColor)
        Console.SetCursorPosition(0, row + 2)
    End Sub

    Private Sub WriteColored(text As String, color As ConsoleColor)
        Console.ForegroundColor = color
        Console.Write(text)
    End Sub

    Private Function WrapText(text As String, maxLineLength As Integer) As List(Of String)
        Dim result As New List(Of String)
        If String.IsNullOrEmpty(text) Then
            result.Add("")
            Return result
        End If

        For Each paragraph As String In text.Replace(vbCrLf, vbLf).Split(vbLf)
            Dim words As String() = paragraph.Split(" "c)
            Dim currentLine As String = ""

            For Each word As String In words
                If word.Length > maxLineLength Then
                    If currentLine.Length > 0 Then
                        result.Add(currentLine)
                        currentLine = ""
                    End If
                    For i As Integer = 0 To word.Length - 1 Step maxLineLength
                        result.Add(word.Substring(i, Math.Min(maxLineLength, word.Length - i)))
                    Next
                ElseIf currentLine.Length + word.Length + 1 > maxLineLength Then
                    result.Add(currentLine)
                    currentLine = word
                Else
                    currentLine = If(currentLine.Length = 0, word, currentLine & " " & word)
                End If
            Next
            result.Add(currentLine)
        Next

        Return result
    End Function

    Private Function Truncate(value As String, maxChars As Integer) As String
        If value.Length <= maxChars Then Return value
        Return value.Substring(0, maxChars - 3) & "..."
    End Function

End Module
