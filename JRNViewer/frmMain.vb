Imports JRNViewer
Imports System.Data.Odbc

Public Class frmMain
    Dim JrnRcvrs As New List(Of JournalObjects.JournalReceiver)
    Dim myToolTip As New ToolTip


    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()
        'btnRefresh
        myToolTip.SetToolTip(btnRefresh, "Allows you to change your library and files to search for")
        'btnLogin
        myToolTip.SetToolTip(btnLogin, "Allows you to switch to another AS/400")
        'btnExport
        myToolTip.SetToolTip(btnExport, "Click this to generate an Excel Version of the Journal Records")
        'tabDisplayJournalEntries
        'tabAvailableJournals
        myToolTip.SetToolTip(TabControl1, "Display Journal Entries" + vbCrLf +
                                                       "   This displays the journal entries, plus the formatted data in a grid view" + vbCrLf +
                                                       "Journal Information" + vbCrLf +
                                                       "   This displays the journal information associated with each table.")
        'cbAvailableJournals
        myToolTip.SetToolTip(cbAvailableJournals, "Select here for a list of available journaled tables")

        LoadForm()

    End Sub

    Public Sub LoadForm()
        ' Add any initialization after the InitializeComponent() call.
        Me.Show()
        Me.ToolStripStatus.Text = "Status: Requesting Login Details"
        LoginForm1.ShowDialog()
        If LoginForm1.Success Then
            Me.ToolStripStatus.Text = "Status: Select Libraries to search for Journaled Tables"
            With Me.ToolStripProgressBar.ProgressBar
                .Visible = True
                .Value = 75
            End With
            Me.Refresh()
            frmLibList.ShowDialog()
        Else
            End
        End If

        FillAvailableJournals()

    End Sub

    Private Sub UpdateComboList()
        Dim myRow As Integer
        cbAvailableJournals.SelectedText = ""
        cbAvailableJournals.Items.Clear()

        For myRow = 0 To DataGridView1.RowCount - 1
            'DataGridView1.Rows(myRow).Cells(0)
            cbAvailableJournals.Items.Add(DataGridView1.Rows(myRow).Cells(0).Value.ToString.TrimEnd + "/" + _
                                          DataGridView1.Rows(myRow).Cells(1).Value.ToString.TrimEnd)
        Next
    End Sub

    Public Sub FillAvailableJournals()
        With Me.ToolStripProgressBar.ProgressBar
            .Visible = True
        End With
        'Verify that we have libraries selected...
        Me.ToolStripStatus.Text = "Status: Retreiving Available Journaled Tables... please wait..."
        With Me.ToolStripProgressBar.ProgressBar
            .Visible = True
            .Value = 50
        End With
        Me.Refresh()
        Dim JrnObj As New JournalObjects.DB2
        If My.Settings.LibraryList.Length > 0 Then
            DataGridView1.DataSource = JrnObj.GetJournaledTablesForLibList(LoginForm1.cbSystemID.Text, _
                                                                           LoginForm1.txtUsername.Text, _
                                                                           LoginForm1.txtPassword.Text, _
                                                                           My.Settings.LibraryList)
            With Me.ToolStripProgressBar.ProgressBar
                .Visible = True
                .Value = 100
            End With
            Me.Refresh()
            UpdateComboList()
        Else
            Me.ToolStripStatus.Text = "Status: No Libraries Selected... Go To Config to Select Search Libraries"
        End If

        With Me.ToolStripProgressBar.ProgressBar
            .Visible = False
        End With


        Me.ToolStripStatus.Text = "Status: Ready"

    End Sub

    Private Sub frmMain_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles btnRefresh.Click
        Me.ToolStripStatus.Text = "Status: Select Libraries to search for Journaled Tables"
        Me.Refresh()
        frmLibList.ShowDialog()
        FillAvailableJournals()
    End Sub

    Private Sub DataGridView1_CellContentDoubleClick(sender As Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DataGridView1.CellContentDoubleClick
        If e.RowIndex = -1 Then
            Return
        End If

        MsgBox(DataGridView1.Rows(e.RowIndex).Cells(0).Value.ToString.TrimEnd + "/" + _
               DataGridView1.Rows(e.RowIndex).Cells(1).Value.ToString.TrimEnd)

    End Sub

    Private Sub btnLogin_Click(sender As System.Object, e As System.EventArgs) Handles btnLogin.Click
        LoadForm()
    End Sub

    Private Sub cbAvailableJournals_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles cbAvailableJournals.SelectedIndexChanged
        'MsgBox("Item " + cbAvailableJournals.SelectedIndex.ToString + " " + cbAvailableJournals.SelectedItem.ToString)
        Dim JrnObj As New JournalObjects.DB2
        Dim myJrnRcvr As New JournalObjects.JournalReceiver
        Dim EarlyDate As Date, LastDate As New Date
        Dim JrnSchma As String, JrnName As String

        JrnSchma = DataGridView1.Rows(cbAvailableJournals.SelectedIndex.ToString).Cells(2).Value.ToString.TrimEnd
        JrnName = DataGridView1.Rows(cbAvailableJournals.SelectedIndex.ToString).Cells(3).Value.ToString.TrimEnd

        'TODO - Need to put a "try" here"...
        JrnRcvrs = JrnObj.getReceiversForJournal(LoginForm1.cbSystemID.Text, LoginForm1.txtUsername.Text, LoginForm1.txtPassword.Text, _
                                               JrnSchma, JrnName)
        EarlyDate = Date.Now
        For Each JrnRcvr In JrnRcvrs
            'Get Earliest Journal Date'
            If JrnRcvr.ReceiverStartDate < EarlyDate Then
                EarlyDate = JrnRcvr.ReceiverStartDate
            End If

            'Get Oldest Journal Date'
            If JrnRcvr.ReceiverEndDate > LastDate Then
                LastDate = JrnRcvr.ReceiverEndDate
            End If
            Debug.Print(JrnRcvr.ReceiverName + ":" + JrnRcvr.ReceiverStartDate.ToString + " " + JrnRcvr.ReceiverEndDate.ToString)
        Next

        dtFromDate.Value = EarlyDate
        dtToDate.Value = LastDate

    End Sub

    Private Sub btnRetrieveData_Click(sender As System.Object, e As System.EventArgs) Handles btnRetrieveData.Click

        Dim MyCommand As New OdbcCommand()
        Dim da As New OdbcDataAdapter
        Dim JrnObj As New JournalObjects.DB2
        Dim TbleNamePath As String, JrnNamePath As String, JrnRcvrStartPath As String,
            JrnRcvrEndPath As String, SearchStart As Date, SearchEnd As Date
        Dim JrnRcvrStart As New JournalObjects.JournalReceiver
        Dim JrnRcvrEnd As New JournalObjects.JournalReceiver

        Me.ToolStripStatus.Text = "Status: Retreiving Appropriate Journal Receivers for given dates"
        With Me.ToolStripProgressBar.ProgressBar
            .Visible = True
            .Value = 25
        End With
        Me.Refresh()

        Try
            JrnRcvrStart = getJrnRcvrForDate(dtFromDate.Value)
            JrnRcvrEnd = getJrnRcvrForDate(dtToDate.Value)
        Catch ex As Exception
            Me.ToolStripStatus.Text = "Error: " + ex.Message
            With Me.ToolStripProgressBar.ProgressBar
                .Visible = True
                .Value = 100
            End With

            MsgBox(ex.Message, MsgBoxStyle.Exclamation)

            Me.ToolStripStatus.Text = "Status: System Ready"
            With Me.ToolStripProgressBar.ProgressBar
                .Visible = False
                .Value = 100
            End With
            Exit Sub
        End Try
        JrnRcvrStart = getJrnRcvrForDate(dtFromDate.Value)
        JrnRcvrEnd = getJrnRcvrForDate(dtToDate.Value)

        TbleNamePath = cbAvailableJournals.SelectedItem.ToString
        JrnNamePath = JrnRcvrStart.JournalSchema.TrimEnd + "/" + JrnRcvrStart.JournalName.TrimEnd
        JrnRcvrStartPath = JrnRcvrStart.ReceiverSchema.TrimEnd + "/" + JrnRcvrStart.ReceiverName.TrimEnd()
        JrnRcvrEndPath = JrnRcvrEnd.ReceiverSchema.TrimEnd + "/" + JrnRcvrEnd.ReceiverName.TrimEnd()
        SearchStart = dtFromDate.Value
        SearchEnd = dtToDate.Value

        Me.ToolStripStatus.Text = "Status: Search Receivers for " + cbAvailableJournals.Text + " .  Please be patient... this might take some time."
        With Me.ToolStripProgressBar.ProgressBar
            .Visible = True
            .Value = 75
        End With
        Me.Refresh()

        Try
            DataGridView2.DataSource = Nothing
            DataGridView2.DataSource = JrnObj.getJournalData(LoginForm1.cbSystemID.Text, LoginForm1.txtUsername.Text, LoginForm1.txtPassword.Text, _
                                          TbleNamePath, JrnNamePath, JrnRcvrStartPath, JrnRcvrEndPath, _
                                          SearchStart, SearchEnd)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Exclamation)
            Exit Sub
        End Try

        Me.ToolStripStatus.Text = "Status: System Ready"
        With Me.ToolStripProgressBar.ProgressBar
            .Visible = False
            .Value = 75
        End With
        Me.Refresh()
    End Sub

    Private Function getJrnRcvrForDate(SearchDate As Date)
        Dim myJrnRcvr As New JournalObjects.JournalReceiver

        Try
            For Each JrnRcvr In JrnRcvrs
                'Return JrnRcvr for search date
                If JrnRcvr.ReceiverStartDate <= SearchDate And JrnRcvr.ReceiverEndDate >= SearchDate Then
                    Debug.Print(JrnRcvr.ReceiverStartDate.ToString + " : " + SearchDate + " : " + JrnRcvr.ReceiverEndDate.ToString)
                    Return JrnRcvr
                End If
            Next
            Throw New InvalidProgramException("No Journal Receiver found for Search Date")
        Catch ex As Exception
            Throw ex
        End Try

    End Function

    Private Sub btnExit_Click(sender As System.Object, e As System.EventArgs) Handles btnExit.Click
        End
    End Sub

    Private Sub btnExport_Click(sender As System.Object, e As System.EventArgs) Handles btnExport.Click
        If cbAvailableJournals.Text <> "" Then

            Dim ExportName As String, FromDate As String, ToDate As String

            FromDate = Format(dtFromDate.Value.Date, "yyyyMMdd") + Format(dtFromDate.Value.TimeOfDay.ToString.Replace(":", ""))
            ToDate = Format(dtToDate.Value.Date, "yyyyMMdd") + Format(dtToDate.Value.TimeOfDay.ToString.Replace(":", ""))
            ExportName = My.Application.Info.DirectoryPath + "\" + cbAvailableJournals.Text.Replace("/", "_") + "_" + FromDate + " To " + ToDate + ".xls"

            Dim fs As New IO.StreamWriter(ExportName, False)
            fs.WriteLine("<?xml version=""1.0""?>")
            fs.WriteLine("<?mso-application progid=""Excel.Sheet""?>")
            fs.WriteLine("<ss:Workbook xmlns:ss=""urn:schemas-microsoft-com:office:spreadsheet"">")
            fs.WriteLine("    <ss:Styles>")
            fs.WriteLine("        <ss:Style ss:ID=""1"">")
            fs.WriteLine("           <ss:Font ss:Bold=""1""/>")
            fs.WriteLine("        </ss:Style>")
            fs.WriteLine("    </ss:Styles>")
            fs.WriteLine("    <ss:Worksheet ss:Name=""Sheet1"">")
            fs.WriteLine("        <ss:Table>")
            For x As Integer = 0 To DataGridView2.Columns.Count - 1
                fs.WriteLine("            <ss:Column ss:Width=""{0}""/>",
                             DataGridView2.Columns.Item(x).Width)
            Next
            fs.WriteLine("            <ss:Row ss:StyleID=""1"">")
            For i As Integer = 0 To DataGridView2.Columns.Count - 1
                fs.WriteLine("                <ss:Cell>")
                fs.WriteLine(String.Format(
                             "                   <ss:Data ss:Type=""String"">{0}</ss:Data>",
                                           DataGridView2.Columns.Item(i).HeaderText))
                fs.WriteLine("                </ss:Cell>")
            Next
            fs.WriteLine("            </ss:Row>")
            For intRow As Integer = 0 To DataGridView2.RowCount - 2
                fs.WriteLine(String.Format("            <ss:Row ss:Height =""{0}"">",
                                           DataGridView2.Rows(intRow).Height))
                For intCol As Integer = 0 To DataGridView2.Columns.Count - 1
                    fs.WriteLine("                <ss:Cell>")
                    fs.WriteLine(String.Format(
                                 "                   <ss:Data ss:Type=""String"">{0}</ss:Data>",
                                               DataGridView2.Item(intCol, intRow).Value.ToString))
                    fs.WriteLine("                </ss:Cell>")
                Next
                fs.WriteLine("            </ss:Row>")
            Next
            fs.WriteLine("        </ss:Table>")
            fs.WriteLine("    </ss:Worksheet>")
            fs.WriteLine("</ss:Workbook>")
            fs.Close()

            Try
                Dim LaunchExcel As New Process
                With LaunchExcel
                    .StartInfo.FileName = ExportName
                    .StartInfo.WindowStyle = ProcessWindowStyle.Maximized
                    .Start()
                End With

            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical, "Error Opening As Excel File")
            End Try

        Else
            MsgBox("No Table To Export... Select a Table and Run a Search to generate Excel Output", MsgBoxStyle.Exclamation)
        End If
    End Sub
End Class
