Module Main
    Public Function Prompt() As String
        Return Prompt(Nothing)
    End Function

    Public Function Prompt(ByVal promptText As String) As String
        If promptText IsNot Nothing Then
            Console.Write(promptText)
        Else
            Console.Write("user> ")
        End If

        Return Console.ReadLine()
    End Function

    Public Function ReadExit(ByVal inputLine As String) As Boolean
        Return _
            inputLine Is Nothing OrElse
            inputLine.ToLower.Trim = "exit" OrElse
            inputLine.ToLower.Trim = "quit"
    End Function

    Public Function Rep(ByVal inputLine As String) As String
        Dim ast As MalType = Reader.ReadStr(inputLine)
        Dim evalResult As MalType = Eval.Eval(ast, ReplEnv)
        Dim printOutput = Printer.PrStr(evalResult, True)

        Return printOutput
    End Function

    Sub Main(ByVal args As String())
        Dim inputLine As String
        Dim isRunOnce As Boolean = False
        Dim isExitEncountered As Boolean = False

        If args IsNot Nothing AndAlso args.Count > 0 Then
            inputLine = String.Format("(load-file {0})", args(0))

            ReplEnv.Set(New MalSymbol("*ARGV*"), New MalList(args.ToList.GetRange(1, args.Count - 1).Select(Function(t) Reader.ReadStr(t))))

            isRunOnce = True
        Else
            inputLine = Prompt()
        End If

        While Not isExitEncountered
            Try
                Dim outputLine As String = Rep(inputLine)

                Console.WriteLine(outputLine)
            Catch eex As EvaluateException
                Console.WriteLine("Error has occurred: " & eex.Message)
            Catch nrex As NoReadableCodeException
                'comment or blank line
            Catch ex As Exception
                Console.WriteLine("Something unexpected has occured: " & ex.Message)
            End Try

            If Not isRunOnce Then
                inputLine = Prompt()
                isExitEncountered = ReadExit(inputLine)
            Else
                isExitEncountered = True
            End If
        End While

        Console.WriteLine("Thanks for playing!")
    End Sub

    Class NoReadableCodeException
        Inherits Exception

        Sub New(ByVal msg As String)
            MyBase.New(msg)
        End Sub
    End Class
End Module