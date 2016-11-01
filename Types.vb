Namespace Types

    Public MustInherit Class MalType
    End Class

    Public Class MalInt
        Inherits MalType

        Public Property Value As Integer
    End Class

    Public Class MalStr
        Inherits MalType

        Public Property Value As String
    End Class

    Public Class MalDbl
        Inherits MalType

        Public Property Value As Double
    End Class

    Public Class MalBool
        Inherits MalType

        Public Property Value As Boolean
    End Class

    Public Class MalSymbol
        Inherits MalType

        Public Property Value As String
    End Class

    Public Class MalList
        Inherits MalType

        Public Property Value As New List(Of MalType)
    End Class

    Public Class MalNil
        Inherits MalType

        Private Shared Property _instance As MalNil

        Public Shared ReadOnly Property Instance As MalNil
            Get
                Return _instance
            End Get
        End Property

        Sub New()
            _instance = New MalNil
        End Sub
    End Class
End Namespace
