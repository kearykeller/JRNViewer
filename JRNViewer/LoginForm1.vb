Imports JRNViewer.JournalObjects

Public Class LoginForm1
    Dim mySystemIP As String
    Dim myUserID As String
    Dim myPassword As String
    Dim mySuccess As Boolean
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
    Public Property Success() As Boolean
        Get
            Return mySuccess
        End Get

        Set(value As Boolean)
            mySuccess = value
        End Set
    End Property

    Private Sub OK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK.Click
        Dim TestUser As New DB2
        Dim ErrMsg As String
        Dim mySystems As New System.Collections.Specialized.StringCollection

        Try
            TestUser.VerifyID(cbSystemID.Text, txtUsername.Text, txtPassword.Text)
            Me.Success = True
            If IsNothing(My.Settings.AS400s) Then
                'Add System to List
                mySystems.Add(UCase(cbSystemID.Text))
                My.Settings.AS400s = mySystems
                My.Settings.Save()
            End If
            mySystems = My.Settings.AS400s
            If mySystems.Contains(UCase(cbSystemID.Text)) = False Then
                'Add System to List
                mySystems.Add(UCase(cbSystemID.Text))
                My.Settings.AS400s = mySystems
                My.Settings.Save()
            End If
            Me.Close()
        Catch ex As Exception
            ErrMsg = "Login Unsuccessful.  Please confirm system and credentials"
            MsgBox(ErrMsg, MsgBoxStyle.Critical, "Login Error")
            Me.Success = False
        End Try
    End Sub

    Private Sub Cancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel.Click
        Me.Close()
        Me.Success = False
    End Sub

    Private Sub LoginForm1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Dim mySystems As System.Collections.Specialized.StringCollection
        Dim sys As String

        Try
            cbSystemID.Items.Clear()
            mySystems = My.Settings.AS400s

            For Each sys In mySystems
                cbSystemID.Items.Add(sys)
            Next
        Catch ex As Exception

        End Try


    End Sub
End Class
