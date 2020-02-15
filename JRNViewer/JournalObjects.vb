Imports System
Imports System.IO
Imports System.Text
Imports System.Data.Odbc

Namespace JournalObjects

    Public Class JournaledTable

        Dim myTableSchema As String
        Dim myTableName As String
        Dim myJournalSchema As String
        Dim myJournalName As String
        Dim myJournalActive As Boolean
        Dim myJournalStartDate As DateTime
        Public Property TableSchema() As String
            Get
                Return myTableSchema
            End Get

            Set(value As String)
                myTableSchema = value
            End Set
        End Property
        Public Property TableName() As String
            Get
                Return myTableName
            End Get

            Set(value As String)
                myTableName = value
            End Set
        End Property
        Public Property JournalSchema() As String
            Get
                Return myJournalSchema
            End Get

            Set(value As String)
                myJournalSchema = value
            End Set
        End Property
        Public Property JournalName() As String
            Get
                Return myJournalName
            End Get

            Set(value As String)
                myJournalName = value
            End Set
        End Property
        Public Property JournalActive() As Boolean
            Get
                Return myJournalActive
            End Get

            Set(value As Boolean)
                myJournalActive = value
            End Set
        End Property
        Public Property JournalStartDate() As DateTime
            Get
                Return myJournalStartDate
            End Get

            Set(value As DateTime)
                myJournalStartDate = value
            End Set
        End Property

        Public Sub New()
            'Initialize Object
        End Sub
    End Class

    Public Class JournalReceiver
        Dim myJournalSchema As String
        Dim myJournalName As String
        Dim myReceiverSchema As String
        Dim myReceiverName As String
        Dim myReceiverStartDate As DateTime
        Dim myReceiverEndDate As DateTime
        Public Property JournalSchema() As String
            Get
                Return myJournalSchema
            End Get

            Set(value As String)
                myJournalSchema = value
            End Set
        End Property

        Public Property JournalName() As String
            Get
                Return myJournalName
            End Get

            Set(value As String)
                myJournalName = value
            End Set
        End Property

        Public Property ReceiverSchema() As String
            Get
                Return myReceiverSchema
            End Get

            Set(value As String)
                myReceiverSchema = value
            End Set
        End Property

        Public Property ReceiverName() As String
            Get
                Return myReceiverName
            End Get

            Set(value As String)
                myReceiverName = value
            End Set
        End Property

        Public Property ReceiverStartDate() As DateTime
            Get
                Return myReceiverStartDate
            End Get

            Set(value As DateTime)
                myReceiverStartDate = value
            End Set
        End Property

        Public Property ReceiverEndDate() As DateTime
            Get
                Return myReceiverEndDate
            End Get

            Set(value As DateTime)
                myReceiverEndDate = value
            End Set
        End Property

        Public Sub New()
            'Initialize Object
        End Sub

    End Class

    Public Class DB2
        Dim mySystemIP As String
        Dim myUserID As String
        Dim myPassword As String
        Public Property SystemIP() As String
            Get
                Return mySystemIP
            End Get

            Set(value As String)
                mySystemIP = value
            End Set
        End Property

        Public Property UserID() As String
            Get
                Return myUserID
            End Get

            Set(value As String)
                myUserID = value
            End Set
        End Property

        Public Property Password() As String
            Get
                Return myPassword
            End Get

            Set(value As String)
                myPassword = value
            End Set
        End Property


        Public Sub VerifyID(System_IP As String, UserName As String, Password As String)
            Me.UserID = UserName
            Me.Password = Password
            Me.SystemIP = System_IP
            Try
                ConnectToDB()
            Catch ex As Exception
                Throw ex
            End Try
        End Sub

        Public Function GetAvailableLibList(System_IP As String, UserName As String, Password As String)
            Dim MyDataReader As OdbcDataReader
            Dim MyCommand As New OdbcCommand()
            Dim MyAS400Command As String
            Dim LibList As New StringBuilder
            Me.UserID = UserName
            Me.Password = Password
            Me.SystemIP = System_IP
            Try
                MyCommand.Connection = ConnectToDB()
                MyAS400Command = "DSPOBJD OBJ(*ALL/*ALL) OBJTYPE(*LIB) OUTPUT(*OUTFILE) OUTFILE(QTEMP/ALLLIBS)"
                MyCommand.CommandText = "Call QSYS.QCMDEXC('" + MyAS400Command + "', " _
                                                              + Format(MyAS400Command.Length, "0000000000.00000") _
                                                       + ")"
                MyCommand.ExecuteNonQuery()
                MyCommand.CommandText = "SELECT ODLBNM, ODOBNM FROM qtemp.alllibs"
                MyDataReader = MyCommand.ExecuteReader()

                While MyDataReader.Read
                    LibList.Append(MyDataReader("ODOBNM"))
                    LibList.Append(vbTab)
                End While

                MyDataReader.Close()
                MyCommand.Dispose()

                Return LibList.ToString
            Catch ex As Exception
                Throw ex
            End Try


        End Function
        Public Function GetJournaledTablesForLibList(System_IP As String, UserName As String, Password As String, LibList As String)
            Dim MyDataReader As OdbcDataReader
            Dim MyCommand As New OdbcCommand()
            Dim MyAS400Command As String

            Dim MyTableList As String
            Dim MyTableListArray() As String
            Dim MyTable As String

            Dim MyLibList As String
            Dim MyLibListArray() As String
            Dim MyLib As String, OutMemberOpt As String
            Dim list = New List(Of JournaledTable)
            Me.UserID = UserName
            Me.Password = Password
            Me.SystemIP = System_IP

            MyLibList = My.Settings.LibraryList
            MyLibListArray = Split(MyLibList, vbTab)

            MyTableList = My.Settings.TableList

            If MyTableList.Trim = "" Then
                ReDim MyTableListArray(0)
                MyTableListArray(0) = "*ALL"
            Else
                MyTableListArray = Split(MyTableList, ",")
            End If

            OutMemberOpt = "OUTMBR(*FIRST *Replace)"

            Try
                MyCommand.Connection = ConnectToDB()
                For Each MyLib In MyLibListArray
                    If MyLib <> "" Then
                        For Each MyTable In MyTableListArray
                            If MyTable.Trim <> "" Then
                                MyAS400Command = "DSPOBJD OBJ(" + MyLib.TrimEnd + "/" + MyTable.Trim + ") " + _
                                                 "OBJTYPE(*FILE) " + _
                                                 "DETAIL(*FULL) " + _
                                                 "OUTPUT(*OUTFILE) " + _
                                                 "OUTFILE(QTEMP/JRNTBLS) " + _
                                                 OutMemberOpt

                                MyCommand.CommandText = "Call QSYS.QCMDEXC('" + MyAS400Command + "', " _
                                                                              + Format(MyAS400Command.Length, "0000000000.00000") _
                                                                       + ")"
                                MyCommand.ExecuteNonQuery()
                                OutMemberOpt = "OUTMBR(*FIRST *ADD)"
                            End If
                        Next
                    End If
                Next

                MyCommand.CommandText = "Select ODLBNM, ODOBNM, ODJRLB, ODJRNM, ODJRST,ODJRDT " +
                                        "FROM qtemp.JRNTBLS " + _
                                        "Where ODJRNM <>''"

                MyDataReader = MyCommand.ExecuteReader()

                While MyDataReader.Read

                    Dim JrnldTble As New JournaledTable
                    With JrnldTble
                        .TableSchema = MyDataReader("ODLBNM")
                        .TableName = MyDataReader("ODOBNM")
                        .JournalSchema = MyDataReader("ODJRLB")
                        .JournalName = MyDataReader("ODJRNM")
                        .JournalActive = MyDataReader("ODJRST")
                        .JournalStartDate = ConvertToDate(MyDataReader("ODJRDT"))
                    End With
                    list.Add(JrnldTble)
                End While

                MyDataReader.Close()
                MyCommand.Dispose()

                Return list

            Catch ex As Exception
                Throw ex
            End Try


        End Function

        Public Function getReceiversForJournal(System_IP As String, UserName As String, Password As String, _
                                               JrnSchema As String, JrnlName As String)
            Dim JrnlRcvrs As New List(Of JournalReceiver)
            Dim MyDataReader As OdbcDataReader
            Dim MyCommand As New OdbcCommand()
            Dim MyAS400Command As String

            Me.UserID = UserName
            Me.Password = Password
            Me.SystemIP = System_IP

            Try
                MyCommand.Connection = ConnectToDB()
                MyAS400Command = "DSPOBJD OBJ(" + JrnSchema.TrimEnd + "/" + "*All) " + _
                                         "OBJTYPE(*JRNRCV) " + _
                                         "DETAIL(*FULL) " + _
                                         "OUTPUT(*OUTFILE) " + _
                                         "OUTFILE(QTEMP/JRNRCVS) OUTMBR(*FIRST *Replace)"

                MyCommand.CommandText = "Call QSYS.QCMDEXC('" + MyAS400Command + "', " _
                                                              + Format(MyAS400Command.Length, "0000000000.00000") _
                                                       + ")"
                MyCommand.ExecuteNonQuery()

                MyCommand.CommandText = "Select ODJRLB, ODJRNM,ODLBNM, ODOBNM, ODCDAT, ODCTIM, ODLDAT, ODLTIM " +
                                        "FROM qtemp.JRNRCVS " + _
                                        "Where ODJRNM ='" + JrnlName.TrimEnd + "'"

                MyDataReader = MyCommand.ExecuteReader()

                While MyDataReader.Read

                    Dim JrnlRcvr As New JournalReceiver
                    With JrnlRcvr
                        .JournalSchema = MyDataReader("ODJRLB")
                        .JournalName = MyDataReader("ODJRNM")
                        .ReceiverSchema = MyDataReader("ODLBNM")
                        .ReceiverName = MyDataReader("ODOBNM")
                        .ReceiverStartDate = ConvertToDateTime(MyDataReader("ODCDAT"), MyDataReader("ODCTIM"))
                        .ReceiverEndDate = ConvertToDateTime(MyDataReader("ODLDAT"), MyDataReader("ODLTIM"))
                    End With
                    JrnlRcvrs.Add(JrnlRcvr)
                End While

                MyDataReader.Close()
                MyCommand.Dispose()

                Return JrnlRcvrs

            Catch ex As Exception
                Throw ex
            End Try



        End Function
        Public Function getJournalData(System_IP As String, UserName As String, Password As String, _
                                       TbleNamePath As String, JrnNamePath As String, _
                                       JrnRcvrStartPath As String, JrnRcvrEndPath As String, _
                                       SearchStart As Date, SearchEnd As Date)

            Dim MyCommand As New OdbcCommand()
            Dim MyAS400Command As String

            Me.UserID = UserName
            Me.Password = Password
            Me.SystemIP = System_IP

            Try

                MyCommand.Connection = ConnectToDB()
                '----------------------------------------------------------------------------
                'Drop Qtemp/JrnType3 and MyTemp just in case...
                Try
                    MyCommand.CommandText = "Drop Table QTEMP.JRNTYPE3"
                    MyCommand.ExecuteNonQuery()
                Catch ex As Exception
                    'Do Nothing... just here if no table is available to drop
                End Try

                Try
                    MyCommand.CommandText = "Drop Table qtemp.mytemp"
                    MyCommand.ExecuteNonQuery()
                Catch ex As Exception
                    'Do Nothing... just here if no table is available to drop
                End Try

                '----------------------------------------------------------------------------
                'Now get Journal Entries
                MyAS400Command = "DSPJRN JRN(" + JrnNamePath + ") " + _
                                 "       FILE((" + TbleNamePath + " *ALL)) " + _
                                 "       RCVRNG(" + JrnRcvrStartPath + " " + JrnRcvrEndPath + ") " + _
                                 "       FROMTIME(''" + Format(SearchStart.Date, "MM/dd/yyyy") + "'' ''" + SearchStart.TimeOfDay.ToString + "'')" + _
                                 "       TOTIME(''" + Format(SearchEnd.Date, "MM/dd/yyyy") + "'' ''" + SearchEnd.TimeOfDay.ToString + "'')" + _
                                 "       ENTTYP(*RCD) OUTPUT(*OUTFILE) OUTFILFMT(*TYPE3) " + _
                                 "       OUTFILE(QTEMP/JRNTYPE3) " + _
                                 "       ENTDTALEN(*VARLEN 1000 100) " + _
                                 "       NULLINDLEN(25)               "

                ExecAS400Cmd(MyAS400Command, MyCommand)
                '----------------------------------------------------------------------------
                'Next, create temp table to extract the journals with a join back to the journal
                MyCommand.CommandText = "Create Table qtemp.mytemp (" + _
                                        "  MYSEQN NUMERIC(10) default NULL , " + _
                                        "  MYRAW  CHAR(1000) default NULL) "
                MyCommand.ExecuteNonQuery()
                '----------------------------------------------------------------------------
                'Insert the Journal Seq and Raw Data into the temp table
                MyCommand.CommandText = "Insert into qtemp.mytemp         " + _
                                        "  SELECT JOSEQN, JOESD  FROM Qtemp.JRNTYPE3"
                MyCommand.ExecuteNonQuery()
                '----------------------------------------------------------------------------
                'Create a version of the table we're searching for with an additional
                'field to join it to the journal entry

                BuildTempTableSQL(TbleNamePath, MyCommand)

                '----------------------------------------------------------------------------
                'Take the qtemp/mytemp table and use the CPYF cmd to put it in the 
                'newly created version of it...

                MyAS400Command = "CPYF FROMFILE(QTEMP/MYTEMP) TOFILE(QTEMP/JrnViewer) " + _
                                 "   MBROPT(*REPLACE) FMTOPT(*NOCHK) ERRLVL(*NOMAX)"
                ExecAS400Cmd(MyAS400Command, MyCommand)

                '----------------------------------------------------------------------------
                'Join the Journal (minus the JOESD) to the newly created JrnViewer

                MyCommand.CommandText = "SELECT A.JOENTL, A.JOSEQN, A.JOCODE, A.JOENTT, A.JOTSTP," + _
                                        "       A.JOJOB,  A.JOUSER, A.JONBR,  A.JOPGM,  A.JOOBJ,   " + _
                                        "       A.JOLIB,  A.JOMBR, A.JOCTRR,  A.JOFLAG, A.JOCCID,  " + _
                                        "       A.JOUSPF, A.JOSYNM, A.JOINCDAT, A.JOMINESD,     " + _
                                        "       A.JORES, B.*  " + _
                                        "FROM   Qtemp.JRNTYPE3 a  " + _
                                        "Inner Join qtemp.JrnViewer b  " + _
                                        "   on a.JOSEQN = b.MYSEQN"
                Dim da As New OdbcDataAdapter(MyCommand.CommandText, MyCommand.Connection)
                Dim ds As New DataSet
                da.Fill(ds)
                Return ds.Tables(0)
            Catch ex As Exception
                Throw ex
            End Try

        End Function



        Private Function ConnectToDB()
            Dim MyDB2String As String = "DRIVER={Client Access ODBC Driver (32-bit)};" & _
                                        "System=" & Me.SystemIP & ";" & _
                                        "UID=" & Me.UserID & ";" & _
                                        "PASSWORD=" & Me.Password & ";"
            Try
                Dim MyDB2Connection As New OdbcConnection(MyDB2String)
                MyDB2Connection.Open()
                Return MyDB2Connection
            Catch ex As Exception
                Throw ex
            End Try
        End Function

        Private Function ConvertToDate(Numeric As Long)
            Dim DateString As String
            Dim mm As String, dd As String, yy As String
            Dim newDate As Date

            DateString = Numeric.ToString("000000")
            mm = Left(DateString, 2)
            yy = "20" + Right(DateString, 2)
            dd = Mid(DateString, 1, 2)

            Try
                newDate = Date.Parse(mm + "/" + dd + "/" + yy)
            Catch ex As Exception
                newDate = Date.Parse("08/01/1939")
            End Try

            Return newDate

        End Function

        Private Function ConvertToDateTime(MyDate As Long, MyTime As Long)
            Dim tempString As String
            Dim mm As String, dd As String, yy As String
            Dim hr As String, mn As String, ss As String
            Dim newDate As Date

            tempString = MyDate.ToString("000000")
            mm = Left(tempString, 2)
            dd = Mid(tempString, 3, 2)
            yy = "20" + Right(tempString, 2)

            tempString = MyTime.ToString("000000")
            hr = Left(tempString, 2)
            mn = Mid(tempString, 3, 2)
            ss = Right(tempString, 2)

            Try
                newDate = Date.Parse(mm + "/" + dd + "/" + yy + " " + hr + ":" + mn + ":" + ss)
            Catch ex As Exception
                newDate = DateTime.Now
            End Try

            Return newDate

        End Function

        Private Function ExecAS400Cmd(Cmd2Exe As String, CmdConnxn As OdbcCommand)
            Dim StrForLen As String

            StrForLen = Replace(Cmd2Exe, "''", "'")
            CmdConnxn.CommandText = "Call QSYS.QCMDEXC('" + Cmd2Exe + "', " _
                                                          + Format(StrForLen.Length, "0000000000.00000") _
                                                   + ")"
            CmdConnxn.ExecuteNonQuery()

        End Function

        Private Function BuildTempTableSQL(TbleNamePath As String, CmdConnxn As OdbcCommand)
            Dim MyDataReader As OdbcDataReader
            Dim Cmd2Exe As String, BuildTable As New StringBuilder
            Dim SplitPath() As String

            SplitPath = Split(TbleNamePath, "/")

            'Drop Table if it exists
            Try
                CmdConnxn.CommandText = "Drop Table Qtemp.JrnViewer"
                CmdConnxn.ExecuteNonQuery()
            Catch ex As Exception
                'Do Nothing
            End Try

            Cmd2Exe = "DSPFFD FILE(" + TbleNamePath + ") OUTPUT(*OUTFILE) OUTFILE(QTEMP/TBLLAYOUT)"
            ExecAS400Cmd(Cmd2Exe, CmdConnxn)

            CmdConnxn.CommandText = "Select WHFLDI , WHFLDT , WHFLDB ,WHFLDD, WHFLDP " + _
                                    "From Qtemp.TblLayout " + _
                                    "Where WHLIB = '" + SplitPath(0) + "' and WHFILE = '" + SplitPath(1) + "'"
            MyDataReader = CmdConnxn.ExecuteReader

            BuildTable.Append("Create Table Qtemp.JrnViewer (")
            BuildTable.Append("MYSEQN NUMERIC(10) default NULL ,")
            While MyDataReader.Read
                BuildTable.Append(BuildField(MyDataReader("WHFLDI"), _
                                             MyDataReader("WHFLDT"), _
                                             MyDataReader("WHFLDB"), _
                                             MyDataReader("WHFLDD"), _
                                             MyDataReader("WHFLDP")))
                BuildTable.Append(",")
            End While
            BuildTable.Append(")")
            MyDataReader.Close()

            CmdConnxn.CommandText = BuildTable.ToString.Replace(",)", ")")
            CmdConnxn.ExecuteNonQuery()

        End Function

        Private Function BuildField(FieldName As String, DataType As String, StringDataLength As Integer, _
                                    DigitLength As Integer, DecimalPos As Integer)
            Select Case DataType
                Case "Z"
                    Return FieldName + " " + "TimeStamp "
                Case "L"
                    Return FieldName + " " + "DATE" + " default NULL "
                Case "T"
                    Return FieldName + " " + "TIME" + " default NULL "
                Case "S"
                    Return FieldName + " " + "NUMERIC" + "(" + DigitLength.ToString + "," + DecimalPos.ToString + ") default NULL "
                Case "P"
                    Return FieldName + " " + "DECIMAL" + "(" + DigitLength.ToString + "," + DecimalPos.ToString + ") default NULL "
                Case Else
                    If StringDataLength > 21000 Then
                        Return "" + FieldName + " " + "TEXT" + "(" + StringDataLength.ToString + ") default NULL "
                    End If
                    If StringDataLength > 255 Then
                        Return FieldName + " " + "VARCHAR" + "(" + StringDataLength.ToString + ") default NULL "
                    Else
                        Return FieldName + " " + "CHAR" + "(" + StringDataLength.ToString + ") default NULL "
                    End If

            End Select
        End Function
    End Class

End Namespace
