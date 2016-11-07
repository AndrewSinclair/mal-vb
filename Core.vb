Public Class Core
    Public Shared Property Ns As Dictionary(Of MalSymbol, MalFunction)

    Sub New()
        Ns.Add(New MalSymbol("+"), New MalFunction(MalIntAggregate(Function(a, b) a + b)))
        Ns.Add(New MalSymbol("-"), New MalFunction(MalIntAggregate(Function(a, b) a - b)))
        Ns.Add(New MalSymbol("*"), New MalFunction(MalIntAggregate(Function(a, b) a * b)))
        Ns.Add(New MalSymbol("/"), New MalFunction(MalIntAggregate(Function(a, b) a / b)))
        Ns.Add(New MalSymbol("<"), New MalFunction(Function(inputs)
                                                       If DirectCast(inputs(0), MalInt).Value < DirectCast(inputs(1), MalInt).Value Then
                                                           Return MalBool.True
                                                       Else
                                                           Return MalBool.False
                                                       End If
                                                   End Function))

        Ns.Add(New MalSymbol("<="), New MalFunction(Function(inputs)
                                                        If DirectCast(inputs(0), MalInt).Value <= DirectCast(inputs(1), MalInt).Value Then
                                                            Return MalBool.True
                                                        Else
                                                            Return MalBool.False
                                                        End If
                                                    End Function))

        Ns.Add(New MalSymbol(">"), New MalFunction(Function(inputs)
                                                       If DirectCast(inputs(0), MalInt).Value > DirectCast(inputs(1), MalInt).Value Then
                                                           Return MalBool.True
                                                       Else
                                                           Return MalBool.False
                                                       End If
                                                   End Function))

        Ns.Add(New MalSymbol(">="), New MalFunction(Function(inputs)
                                                        If DirectCast(inputs(0), MalInt).Value >= DirectCast(inputs(1), MalInt).Value Then
                                                            Return MalBool.True
                                                        Else
                                                            Return MalBool.False
                                                        End If
                                                    End Function))


        Ns.Add(New MalSymbol("list"), New MalFunction(Function(inputs) New MalList(inputs)))
        Ns.Add(New MalSymbol("list?"), New MalFunction(Function(inputs) If(TypeOf inputs(0) Is MalList, MalBool.True, MalBool.False)))
        Ns.Add(New MalSymbol("empty?"), New MalFunction(Function(inputs) If(DirectCast(inputs(0), MalList).Value.Count = 0, MalBool.True, MalBool.False)))
        Ns.Add(New MalSymbol("count"), New MalFunction(Function(inputs) New MalInt(DirectCast(inputs(0), MalList).Value.Count)))
        Ns.Add(New MalSymbol("="), New MalFunction(Function(inputs) If(RecurEqual(inputs), MalBool.True, MalBool.False)))


        Ns.Add(New MalSymbol("pr-str"), New MalFunction(Function(inputs)
                                                            Dim strs As List(Of String) = inputs.Select(Function(t) Printer.PrStr(t, True)).ToList

                                                            Return New MalStr(String.Join(" ", strs))
                                                        End Function))

        Ns.Add(New MalSymbol("str"), New MalFunction(Function(inputs)
                                                         Dim strs As List(Of String) = inputs.Select(Function(t) Printer.PrStr(t, False)).ToList

                                                         Return New MalStr(String.Join("", strs))
                                                     End Function))

        Ns.Add(New MalSymbol("prn"), New MalFunction(Function(inputs)
                                                         Dim strs As List(Of String) = inputs.Select(Function(t) Printer.PrStr(t, True)).ToList
                                                         Dim output As String = String.Join(" ", strs)

                                                         Console.Write(output)

                                                         Return MalNil.Instance
                                                     End Function))

        Ns.Add(New MalSymbol("println"), New MalFunction(Function(inputs)
                                                             Dim strs As List(Of String) = inputs.Select(Function(t) Printer.PrStr(t, False)).ToList
                                                             Dim output As String = String.Join(" ", strs)

                                                             Console.WriteLine(output)

                                                             Return MalNil.Instance
                                                         End Function))

        Ns.Add(New MalSymbol("read-string"), New MalFunction(Function(inputs) Reader.ReadStr(DirectCast(inputs(0), MalStr).Value)))

        Ns.Add(New MalSymbol("slurp"), New MalFunction(Function(inputs) New MalStr(System.IO.File.ReadAllText(DirectCast(inputs(0), MalStr).Value))))


        Ns.Add(New MalSymbol("atom"), New MalFunction(Function(inputs) New MalAtom(inputs(0))))

        Ns.Add(New MalSymbol("atom?"), New MalFunction(Function(inputs) If(TypeOf inputs(0) Is MalAtom, MalBool.True, MalBool.False)))

        Ns.Add(New MalSymbol("deref"), New MalFunction(Function(inputs) DirectCast(inputs(0), MalAtom).Value))

        Ns.Add(New MalSymbol("reset!"), New MalFunction(Function(inputs)
                                                            DirectCast(inputs(0), MalAtom).Value = inputs(1)
                                                            Return inputs(0)
                                                        End Function))

        Ns.Add(New MalSymbol("swap!"), New MalFunction(Function(inputs)
                                                           Dim atom As MalAtom = DirectCast(inputs(0), MalAtom)
                                                           Dim f As MalFunction = DirectCast(inputs(1), MalFunction)
                                                           Dim args As List(Of MalType) = inputs.GetRange(2, inputs.Count - 2)
                                                           Dim prevValue As MalType = atom.Value

                                                           atom.Value = f.Invoke({prevValue}.ToList.Union(args))
                                                           Return atom.Value
                                                       End Function))

        Ns.Add(New MalSymbol("cons"), New MalFunction(Function(inputs)
                                                          Dim x As MalType = inputs(0)
                                                          Dim xs As List(Of MalType)
                                                          If TypeOf inputs(1) Is MalList Then
                                                              xs = DirectCast(inputs(1), MalList).Value
                                                          ElseIf TypeOf inputs(1) Is MalVector Then
                                                              xs = DirectCast(inputs(1), MalVector).Value
                                                          Else
                                                              Throw New EvaluateException("Expected either list or vector to cons operator.")
                                                          End If

                                                          Dim immutableXs(xs.Count) As MalType
                                                          xs.CopyTo(immutableXs)
                                                          immutableXs.ToList.Insert(0, x)

                                                          Return New MalList(immutableXs.ToList)
                                                      End Function))

        Ns.Add(New MalSymbol("concat"), New MalFunction(Function(inputs)
                                                            Dim lists As List(Of List(Of MalType)) = (From input As MalType In inputs Select If(TypeOf input Is MalList, DirectCast(input, MalList).Value, DirectCast(input, MalVector).Value))

                                                            Return New MalList(lists.Aggregate(Function(xss, xs) xss.Concat(xs)))
                                                        End Function))

        Ns.Add(New MalSymbol("nth"), New MalFunction(Function(inputs)
                                                         Dim list As List(Of MalType)
                                                         If TypeOf inputs(0) Is MalList Then
                                                             list = DirectCast(inputs(0), MalList).Value
                                                         Else
                                                             list = DirectCast(inputs(0), MalVector).Value
                                                         End If

                                                         Dim index As MalInt = inputs(1)

                                                         If index.Value < 0 OrElse index.Value > list.Count Then Throw New EvaluateException("The index was not in the list")

                                                         Return inputs(index.Value)
                                                     End Function))

        Ns.Add(New MalSymbol("first"), New MalFunction(Function(inputs)
                                                           Dim list As List(Of MalType)
                                                           If TypeOf inputs(0) Is MalNil Then
                                                               Return MalNil.Instance
                                                           ElseIf TypeOf inputs(0) Is MalList Then
                                                               list = DirectCast(inputs(0), MalList).Value
                                                           Else
                                                               list = DirectCast(inputs(0), MalVector).Value
                                                           End If

                                                           If list.Count = 0 Then Return MalNil.Instance

                                                           Return list(0)
                                                       End Function))

        Ns.Add(New MalSymbol("rest"), New MalFunction(Function(inputs)
                                                          Dim list As List(Of MalType)
                                                          If TypeOf inputs(0) Is MalNil Then
                                                              Return MalNil.Instance
                                                          ElseIf TypeOf inputs(0) Is MalList Then
                                                              list = DirectCast(inputs(0), MalList).Value
                                                          Else
                                                              list = DirectCast(inputs(0), MalVector).Value
                                                          End If

                                                          Dim count As Integer = list.Count

                                                          If count <= 1 Then Return MalNil.Instance

                                                          Return New MalList(list.GetRange(1, count - 1))
                                                      End Function))
    End Sub

    Private Shared Function RecurEqual(ByVal inputs As List(Of MalType)) As Boolean
        Dim first As MalType = inputs(0)
        Dim second As MalType = inputs(1)

        If TypeOf first Is MalList AndAlso TypeOf second Is MalList Then
            If DirectCast(first, MalList).Value.Count = DirectCast(second, MalList).Value.Count Then
                Dim firstList As List(Of MalType) = DirectCast(first, MalList).Value
                Dim secondList As List(Of MalType) = DirectCast(second, MalList).Value
                Dim isEqual As Boolean = True
                Dim length As Integer = firstList.Count
                Dim index As Integer = 0

                While index < length AndAlso isEqual
                    isEqual = RecurEqual(New List(Of MalType)({firstList(index), secondList(index)}))

                    index += 1
                End While

                Return isEqual
            Else
                Return False
            End If
        End If

        Return first.GetType.Equals(second.GetType) AndAlso first.Equals(second)
    End Function

End Class
