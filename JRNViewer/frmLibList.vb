Imports System.Text

Public Class frmLibList
    Dim myLibraryList As New Hashtable
    Dim JrnObj As New JournalObjects.DB2
    Dim MyLibList As String
    Dim MyLibListArray() As String
    Dim MyLib As String
    Dim MyToolTip As New ToolTip

    Public Property LibraryList() As Hashtable
        Get
            Return myLibraryList
        End Get

        Set(value As Hashtable)
            myLibraryList = value
        End Set
    End Property

    Private Sub frmLibList_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        'txtFilter
        MyToolTip.SetToolTip(txtFilter, "Filters the available Libraries based on what you type.")
        'txtTableList
        MyToolTip.SetToolTip(txtTableList, "This is a comma delimited list of tables to search for available journals.  (Ex:  CONTROL,SUBINF,SEREQP")
        'btnRefresh
        MyToolTip.SetToolTip(btnRefresh, "Retreives the available libraries on the system.  You may need to do this if a new library has been added since you pulled up this form")
        'btnSave
        MyToolTip.SetToolTip(btnSave, "Takes the search criteria selected here and locates the journals attached to the tables in the Table List.")
        'CheckedListBox1
        MyToolTip.SetToolTip(CheckedListBox1, "Check the libraries you wish to include in the Library/Table Journal Search.")

        FillLibList()
        loadLibList()
        UpdateLibList()
    End Sub
    Private Sub FillLibList()
        Try
            MyLibList = JrnObj.GetAvailableLibList(LoginForm1.cbSystemID.Text, LoginForm1.txtUsername.Text, LoginForm1.txtPassword.Text)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "Error Retreiving Libraries")
            MsgBox("Due to the error being returned for " + LoginForm1.cbSystemID.Text + _
                   " this program will now end.  Restart once AS/400 problem has been resolved, " + _
                   "or choose another AS/400", MsgBoxStyle.Critical, "Program Ending")
            End
        End Try

        MyLibListArray = Split(MyLibList, vbTab)
    End Sub

    Private Sub UpdateLibList()

        CheckedListBox1.Items.Clear()
        For Each MyLib In MyLibListArray
            If MyLib.Length > 0 Then
                If txtFilter.TextLength > 0 Then
                    If UCase(MyLib.Substring(0, txtFilter.TextLength)) = UCase(txtFilter.Text) Then
                        CheckedListBox1.Items.Add(MyLib)
                        If Me.LibraryList(MyLib) Then
                            CheckedListBox1.SetItemChecked(CheckedListBox1.Items.Count - 1, True)
                        End If
                    End If
                Else
                    CheckedListBox1.Items.Add(MyLib)
                    If Me.LibraryList(MyLib) Then
                        CheckedListBox1.SetItemChecked(CheckedListBox1.Items.Count - 1, True)
                    End If
                End If
            End If
        Next
    End Sub


    Private Sub btnRefresh_Click(sender As System.Object, e As System.EventArgs) Handles btnRefresh.Click
        FillLibList()
        UpdateLibList()
    End Sub

    Private Sub btnSave_Click(sender As System.Object, e As System.EventArgs) Handles btnSave.Click
        Dim i As Integer, LibList As New StringBuilder

        myLibraryList.Clear()

        For i = 0 To CheckedListBox1.CheckedItems.Count - 1
            LibList.Append(CheckedListBox1.CheckedItems(i))
            LibList.Append(vbTab)
            Me.LibraryList.Add(CheckedListBox1.CheckedItems(i), i + 1)
        Next

        My.Settings.LibraryList = LibList.ToString
        My.Settings.TableList = txtTableList.Text
        My.Settings.LibraryFilter = txtFilter.Text
        My.Settings.Save()
        Me.Close()
    End Sub

    Private Sub loadLibList()
        Dim MyLibList As String
        Dim MyLibListArray() As String
        Dim MyLib As String
        Dim i As Integer

        txtTableList.Text = My.Settings.TableList
        MyLibList = My.Settings.LibraryList
        MyLibListArray = Split(MyLibList, vbTab)
        txtFilter.Text = My.Settings.LibraryFilter
        Try
            Me.LibraryList.Clear()
        Catch ex As Exception

        End Try

        For Each MyLib In MyLibListArray
            i = i + 1
            Me.LibraryList.Add(MyLib, i)
        Next

    End Sub

    Private Sub TextBox1_TextChanged(sender As System.Object, e As System.EventArgs) Handles txtFilter.TextChanged
        UpdateLibList()
    End Sub
End Class