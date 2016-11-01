Imports mal_vb.Types

Public Class Reader
    Const TokenRegexp As String = "[\s, ]*(~@|[\[\]{}()'`~^@]|""(?:\\.|[^\\""])*""|;.*|[^\s\[\]{}('""`,;)]*)"

    Private Property Tokens As List(Of Token)
    Private Property Position As Integer

    Public Sub New(ByVal tokens As List(Of Token))
        Me.Tokens = tokens
    End Sub

    Private Function Peek() As Token
        If Position >= Tokens.Count Then Return Nothing
        If Tokens.Count = 0 Then Return Nothing

        Return Tokens(Position)
    End Function

    Private Function [Next]() As Token
        If Position >= Tokens.Count Then Return Nothing
        If Tokens.Count = 0 Then Return Nothing

        Dim currToken As Token = Tokens(Position)

        Position += 1

        Return currToken
    End Function

    Private Function ReadList() As MalType
        Dim malTypes As New List(Of MalType)

        Dim currToken As Token = [Next]()

        While currToken IsNot Nothing AndAlso currToken.Value <> ")"
            Dim form As MalType = ReadForm()

            If Peek().Value = ")" Then Exit While

            malTypes.Add(form)

            currToken = [Next]()
        End While

        Return New MalList With {.Value = malTypes}
    End Function

    Private Function ReadAtom() As MalType
        Dim currToken As Token = Peek()
        Dim valStr As String = currToken.Value
        Dim valDbl As Double
        Dim valInt As Integer
        Dim valBool As Boolean

        If Integer.TryParse(valStr, valInt) Then
            Return New MalInt With {.Value = valInt}
        ElseIf Double.TryParse(valStr, valDbl) Then
            Return New MalDbl With {.Value = valDbl}
        ElseIf Boolean.TryParse(valStr, valBool) Then
            Return New MalBool With {.Value = valBool}
        ElseIf valStr.StartsWith("""") AndAlso valStr.EndsWith("""") Then
            Return New MalStr With {.Value = valStr}
        ElseIf valStr = "nil" Then
            Return MalNil.Instance
        Else
            Return New MalSymbol With {.Value = valStr}
        End If
    End Function

    Private Function ReadForm() As MalType
        Dim currToken As Token = Peek()

        If currToken.Value = "(" Then
            Return ReadList()
        Else
            Return ReadAtom()
        End If
    End Function

    Private Shared Function Tokenizer(ByVal inputLine As String) As List(Of Token)
        Return _
            System.Text.RegularExpressions.Regex.Split(inputLine, TokenRegexp).Except({""}).Select(Function(t) New Token(t)).ToList
    End Function

    Public Shared Function ReadStr(ByVal inputLine As String) As MalType
        Dim reader As New Reader(Tokenizer(inputLine))

        Return reader.ReadForm()
    End Function
End Class

Public Class Token
    Public Property Name As String
    Public Property Value As String

    Sub New(ByVal value As String)
        Me.Value = value
    End Sub
End Class