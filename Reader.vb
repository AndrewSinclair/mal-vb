Public Class Reader
    Public Const keywordPrefix As String = ChrW(&H29E)

    Private Property Tokens As List(Of Token)
    Private Property Position As Integer

    Sub New(ByVal tokens As List(Of Token))
        Me.Tokens = tokens
        Position = 0
    End Sub

    Public Function [Next]() As Token
        If Tokens.Count = 0 Then Return Nothing
        If Position >= Tokens.Count Then Return Nothing

        Dim obj As Token = Tokens(Position)

        Position += 1

        Return obj
    End Function

    Public Function Peek() As Token
        If Tokens.Count = 0 Then Return Nothing
        If Position >= Tokens.Count Then Return Nothing

        Return Tokens(Position)
    End Function

    Public Function ReadList(ByVal delimiterType As String) As MalType
        Dim results As New List(Of MalType)
        [Next]()

        While Peek() IsNot Nothing AndAlso Peek().Value <> delimiterType
            Dim form As MalType = ReadForm()
            results.Add(form)

            [Next]()
        End While

        If Peek() Is Nothing Then Throw New EvaluateException("There was an unexpected EOF while reading a list")

        If delimiterType = ")" Then
            Return New MalList(results)
        ElseIf delimiterType = "]" Then
            Return New MalVector(results)
        ElseIf delimiterType = "}" Then
            If results.Count Mod 2 <> 0 Then Throw New EvaluateException("There was an uneven number of forms in the reading of a hashmap")

            Return New MalHashMap(results)
        Else
            Throw New EvaluateException("Delimiter Type was not correct!")
        End If
    End Function

    Private Shared Function StringFix(ByVal str As String) As String
        Dim fixed As String = str.Replace("\""", """")
        fixed = fixed.Replace("\n", vbCrLf)
        fixed = fixed.Replace("\\", "\")
        Return fixed
    End Function

    Private Function ReadAtom(ByVal atom As String) As MalType
        Return New Reader(New List(Of Token)({New Token(atom)})).ReadAtom
    End Function

    Private Function ReadAtom() As MalType
        Dim currToken As Token = Peek()
        Dim valueStr As String = currToken.Value
        Dim valueInt As Integer
        Dim valueBool As Boolean
        Dim valueDbl As Double

        If valueStr.StartsWith("""") AndAlso Not valueStr.EndsWith("""") Then Throw New EvaluateException("Expected "", got EOF.")

        If valueStr.StartsWith("""") AndAlso valueStr.EndsWith("""") Then
            Return New MalStr(StringFix(valueStr.Substring(1, valueStr.Length - 2)))
        ElseIf valueStr.StartsWith(":") Then
            Return New MalKeyword(valueStr.Replace(":", keywordPrefix))
        ElseIf Integer.TryParse(valueStr, valueInt) Then
            Return New MalInt(valueInt)
        ElseIf Boolean.TryParse(valueStr, valueBool) Then
            Return If(valueBool, MalBool.True, MalBool.False)
        ElseIf Double.TryParse(valueStr, valueDbl) Then
            Return New MalDbl(valueDbl)
        ElseIf valueStr.Equals("nil") Then
            Return MalNil.Instance
        Else
            Return New MalSymbol(valueStr)
        End If
    End Function

    Public Function ReadForm() As MalType
        Dim first As Token = Peek()

        If first.Value = "'" Then
            Return New MalList(New List(Of MalType)({
                New MalSymbol("quote"),
                ReadForm()}))
        ElseIf first.Value = "`" Then
            Return New MalList(New List(Of MalType)({
                New MalSymbol("quasiquote"),
                ReadForm()}))
        ElseIf first.Value = "~" Then
            Return New MalList(New List(Of MalType)({
                New MalSymbol("unquote"),
                ReadForm()}))
        ElseIf first.Value = "~@" Then
            Return New MalList(New List(Of MalType)({
                New MalSymbol("splice-unquote"),
                ReadForm()}))
        ElseIf first.Value = "@" Then
            Return New MalList(New List(Of MalType)({
                New MalSymbol("deref"),
                ReadForm()}))
        ElseIf first.Value = "(" Then
            Return ReadList(")")
        ElseIf first.Value = "[" Then
            Return ReadList("]")
        ElseIf first.Value = "{" Then
            Return ReadList("}")
        Else
            Return ReadAtom()
        End If
    End Function

    Public Shared Function Tokenizer(ByVal inputLine As String) As List(Of Token)
        Dim tokenList As New List(Of Token)
        Const pattern As String = "[\s,]*(~@|[\[\]{}()'`~@]|""(?:[\\].|[^\\""])*""|;.*|[^\s \[\]{}()'""`~@,;]*)"
        Dim regex As New Text.RegularExpressions.Regex(pattern)

        For Each match As Text.RegularExpressions.Match In regex.Matches(inputLine)
            Dim strMatch As String = (match.Groups(1).Value)
            If strMatch IsNot Nothing _
               AndAlso strMatch <> "" _
               AndAlso Not strMatch.StartsWith(";") Then

                tokenList.Add(New Token(strMatch))
            End If
        Next

        Return tokenList
    End Function

    Public Shared Function ReadStr(ByVal inputLine As String) As MalType
        Dim tokens As List(Of Token) = Tokenizer(inputLine)

        If tokens.Count = 0 Then Throw New NoReadableCodeException("Comment or blankline")

        Dim reader As New Reader(tokens)

        Return reader.ReadForm()
    End Function
End Class

Public Class Token
    Public Property Type As String
    Public Property Value As String

    Sub New(ByVal val As String)
        Value = val
        Type = "string"
    End Sub
End Class
