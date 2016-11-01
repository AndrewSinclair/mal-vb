Imports mal_vb.Types

Module Module1

    Public Function Read(ByVal inputLine As String) As MalType
        Return Reader.ReadStr(inputLine)
    End Function

    Public Function Eval(ByVal inputLine As MalType) As MalType
        Return inputLine
    End Function

    Public Function Print(ByVal inputLine As String) As String
        Return inputLine
    End Function

    Public Function Rep(ByVal inputLine As String) As String
        Dim form As MalType = Read(inputLine)
        Dim result As MalType = Eval(form)
        Dim outputLine As String = Printer.PrStr(result)

        Return outputLine
    End Function

    Public Function Prompt() As String
        Return Prompt(Nothing)
    End Function

    Public Function Prompt(ByVal msg As String) As String
        If msg Is Nothing Then
            Console.Write("user> ")
        Else
            Console.Write(msg)
        End If

        Return Console.ReadLine()
    End Function

    Public Function IsExitCommand(ByVal command As String) As Boolean
        Return _
            command Is Nothing OrElse
            command.ToLower.Trim = "exit" OrElse
            command.ToLower.Trim = "quit"
    End Function

    Sub Main()
        Dim inputLine As String = Prompt()

        While Not IsExitCommand(inputLine)
            Dim outputLine As String = Rep(inputLine)

            Console.WriteLine(outputLine)

            inputLine = Prompt()
        End While

        Console.WriteLine(vbCrLf & "Thanks for playing!")
    End Sub

End Module
