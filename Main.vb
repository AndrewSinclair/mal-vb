Module Main
    Public Property ReplEnv As New MalEnvironment

    Sub New()
        ReplEnv.Env.Add("+", Function(a, b) a + b)
        ReplEnv.Env.Add("-", Function(a, b) a - b)
        ReplEnv.Env.Add("*", Function(a, b) a * b)
        ReplEnv.Env.Add("/", Function(a, b) a / b)
    End Sub

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
        Dim evaler As New Eval

        Dim ast As MalType = Reader.ReadStr(inputLine)
        Dim evalResult As MalType = evaler.Eval(ast, replEnv)
        Dim printOutput = Printer.PrStr(evalResult)

        Return printOutput
    End Function

    Sub Main()
        Dim inputLine As String = Prompt()

        While Not ReadExit(inputLine)
            Try
                Dim outputLine As String = Rep(inputLine)

                Console.WriteLine(outputLine)
            Catch eex As EvaluateException
                Console.WriteLine("Error has occurred: " & eex.Message)
            Catch ex As Exception
                'comment or blank line
            End Try

            inputLine = Prompt()
        End While

        Console.WriteLine("Thanks for playing!")
    End Sub

End Module