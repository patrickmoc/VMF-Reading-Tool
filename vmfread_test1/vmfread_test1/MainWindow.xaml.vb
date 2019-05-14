
Imports System.Text.RegularExpressions
Imports System.IO
Imports Microsoft.Win32

Class MainWindow

#Region "Declarations"
    Const MAX As Integer = 50
    'Array of Teleport Destinations read from vmf
    Private teleportLocations(MAX) As TpDest
    Private tpSize As Integer
#End Region

#Region "File Load / Save"

    'Load vmf from file browser
    Private Sub BtnChooseFile_Click(sender As Object, e As RoutedEventArgs) Handles btnChooseFile.Click


        'StreamReader reads the vmf from the path specified
        Dim sReader As StreamReader

        'File browser
        Dim fileDialog As OpenFileDialog
        fileDialog = New OpenFileDialog With {
            .CheckFileExists = True,
            .Title = "Select a VMF",
            .Filter = "Valve Map Format (*.vmf)|*.vmf|All Files (*.*)|*.*"
        }
        'TODO: Add handling to ensure file exists
        If (Not fileDialog.ShowDialog()) Then
            Return
        End If

        Try
            'A new file has been opened so the array size must be reset
            tpSize = 0

            'Display file choice
            txtFilePath.Text = fileDialog.FileName
            sReader = File.OpenText(fileDialog.FileName)
            Dim strTemp As String
            'Temp string to contain lines
            strTemp = sReader.ReadLine
            'Parse until EOF
            While (Not sReader.EndOfStream)

                '****TODO**** Use proper parser instead of regex / string methods 

                'Only search for keywords if the object is a teleport destination
                If (strTemp.Contains("""classname"" ""info_teleport_destination""")) Then

                    Dim tempTp As New TpDest()

                    'Parse until closing bracket
                    While (Not strTemp.Contains("}"))

                        'Only accepts and appends if verified
                        SubVerifyLine(strTemp, tempTp)
                        'Move reader to next line
                        strTemp = sReader.ReadLine

                    End While

                    If tpSize < MAX Then
                        teleportLocations(tpSize) = tempTp
                        tpSize += 1
                    Else
                        MessageBox.Show("ERROR: Teleport location count exceeds the limit of " & MAX)
                        Return
                    End If

                Else
                    'Move reader to next line
                    strTemp = sReader.ReadLine
                End If

            End While

            lstTpList.Items.Clear()

            sReader.Close()

        Catch ex As Exception
            MessageBox.Show("An error occured while parsing vmf: " & ex.ToString)
        End Try

        InitControls()

    End Sub

    Private Sub SaveScript(Script As String)
        If (Script IsNot Nothing) Then
            Dim save As SaveFileDialog = New SaveFileDialog With {
            .Title = "Save File",
            .Filter = "Config File (*.cfg)|*.cfg|Text File (*.txt)|*.txt|All Files (*.*)|*.*"
            }
            If (save.ShowDialog()) Then
                System.IO.File.WriteAllText(save.FileName, Script)
            End If
        End If
    End Sub
#End Region

#Region "Helpers"
    Private Sub InitControls()

        'Make controls accessible
        txtName.IsEnabled = True
        txtOrigin.IsEnabled = True
        txtAngles.IsEnabled = True
        txtStNum.IsEnabled = True
        txtStageorcpsLabel.IsEnabled = True
        txtStCount.IsEnabled = True
        txtBCount.IsEnabled = True
        txtMapper.IsEnabled = True
        cmbTpType.IsEnabled = True
        cmbMapTypes.IsEnabled = True
        cmbTier.IsEnabled = True
        chkBakedTriggers.IsEnabled = True
        btnApply.IsEnabled = True
        btnCancel.IsEnabled = True
        btnMakeScript.IsEnabled = True


        'Init Comboboxes -------
        Dim items As Array
        items = System.Enum.GetValues(GetType(TpTypes))

        'Load TP types into combobox
        cmbTpType.Items.Clear()

        For Each item In items
            cmbTpType.Items.Add(item)
        Next

        items = System.Enum.GetValues(GetType(mapTypes))

        'Load map types into combobox
        cmbMapTypes.Items.Clear()

        For Each item In items
            cmbMapTypes.Items.Add(item)
        Next

        cmbMapTypes.SelectedIndex = 0

        'Load Tiers into combobox
        cmbTier.Items.Clear()

        For I As Integer = 1 To 6
            cmbTier.Items.Add(I)
        Next

        cmbTier.SelectedIndex = 0


        'Init listview and textbox -------
        lstTpList.IsEnabled = "True"
        For index As Integer = 0 To tpSize - 1
            lstTpList.Items.Add(teleportLocations(index).name)
        Next
        'Needed to refresh the data
        lstTpList.SelectedIndex = 0

    End Sub
    'Generates the string for the first line of the output script
    '<Map Insertion Guide>
    'Arg 1 - Tier(1 - 6)
    'Arg 2 - Checkpoints
    'Arg 3 - Map Type ( 0 - Staged, 1 - Linear )
    'Arg 4 - Map Author ( In quotes )
    'Arg 5 - Bonuses
    'Arg 6 - Baked Triggers ( 0 - Disabled, 1 - Enabled )

    'Example sm_insertmap 4 4 1 "Author" 2 1
    Private Function InsertMap() As String
        Dim ReturnString As String = "sm_insertmap "
        If (cmbTier.SelectedIndex >= 0) Then
            ReturnString &= (cmbTier.SelectedIndex + 1) & " "
        Else
            Return "//Error: Missing Tier in map info!" & vbNewLine
        End If

        If (txtStCount.Text <> "") Then
            ReturnString &= txtStCount.Text & " "
        Else
            Return "//Error: Missing stage count in map info!" & vbNewLine
        End If

        If (cmbMapTypes.SelectedIndex >= 0) Then
            ReturnString &= cmbMapTypes.SelectedIndex & " "
        Else
            Return "//Error: Missing map type in map info!" & vbNewLine
        End If

        If (txtMapper.Text <> "") Then
            ReturnString &= """" & txtMapper.Text & """" & " "
        Else
            Return "//Error: Missing mapper name in map info!" & vbNewLine
        End If

        If (txtBCount.Text <> "") Then
            ReturnString &= txtBCount.Text & " "
        Else
            Return "//Error: Missing bonus count in map info!" & vbNewLine
        End If

        If (chkBakedTriggers.IsChecked) Then
            ReturnString &= 1 & ";" & vbNewLine
        Else
            ReturnString &= 0 & ";" & vbNewLine
        End If

        Return ReturnString

    End Function

    'Accepts a line from the parser and a TpDest object to store info in
    'TODO Make a generalized regex instead of this garbage
    Private Sub SubVerifyLine(strIn As String, ByRef tpDest As TpDest)
        'Line is the name of the teleport
        If (strIn.Contains("targetname")) Then

            'Remove "targetname" "xxxx" from string to get only value 
            Dim rgx As Regex = New Regex(vbTab & """targetname"" """)
            strIn = rgx.Replace(strIn, "")
            strIn = strIn.Replace(ControlChars.Quote, "")
            tpDest.name = strIn

        ElseIf (strIn.Contains("angles")) Then
            'Remove "angles" "X X X" from string to get only value
            Dim rgx As Regex = New Regex(vbTab & """angles"" """)
            strIn = rgx.Replace(strIn, "")
            strIn = strIn.Replace(ControlChars.Quote, "")
            tpDest.angles = strIn

        ElseIf (strIn.Contains("origin")) Then
            'Remove "origin" "X X X" from string to get only value
            Dim rgx As Regex = New Regex(vbTab & """origin"" """)
            strIn = rgx.Replace(strIn, "")
            strIn = strIn.Replace(ControlChars.Quote, "")
            tpDest.origin = strIn

        End If

    End Sub
#End Region

#Region "Event Handlers"
    Private Sub ListBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim i As Integer = lstTpList.SelectedIndex
        If i < tpSize And i > -1 Then
            Dim tp As TpDest = teleportLocations(i)
            txtOrigin.Text = tp.origin
            txtAngles.Text = tp.angles
            txtName.Text = tp.name
            txtStNum.Text = tp.stOrBonusNum
            cmbTpType.SelectedIndex = tp.tpType
        End If
    End Sub

    Private Sub Txt_LostFocus(sender As Object, e As System.EventArgs) Handles txtStNum.LostFocus, txtStCount.LostFocus, txtBCount.LostFocus
        If (Not Integer.TryParse(sender.Text, 0)) Then
            'Input is not an integer!
            sender.Clear()
            sender.BorderBrush = Brushes.Red
        Else
            'Input is valid!
            sender.BorderBrush = Brushes.Gray
        End If

    End Sub

    'Applies the current teleport settings to the selected destination
    Private Sub BtnApply_Click(sender As Object, e As RoutedEventArgs) Handles btnApply.Click
        If (Integer.TryParse(txtStNum.Text, 0) And tpSize > 0) Then
            teleportLocations(lstTpList.SelectedIndex).stOrBonusNum = Integer.Parse(txtStNum.Text)
            teleportLocations(lstTpList.SelectedIndex).tpType = cmbTpType.SelectedIndex
            MessageBox.Show("Changed successfuly.")
        Else
            If (tpSize <= 0) Then
                MessageBox.Show("Could not find any destinations!")
            Else
                MessageBox.Show("Please enter a valid stage or bonus value!")
            End If
        End If
    End Sub

    'resets the selected teleport destination settings to what they were before
    Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        Dim i As Integer = lstTpList.SelectedIndex
        If (i >= 0) Then
            Dim tp As TpDest = teleportLocations(i)
            txtStNum.Text = tp.stOrBonusNum
            cmbTpType.SelectedIndex = tp.tpType
        End If
    End Sub

    'Generates the script to be used
    Private Sub BtnMakeScript_Click(sender As Object, e As RoutedEventArgs) Handles btnMakeScript.Click
        'List of all destinations that aren't unused
        Dim TempDest As List(Of TpDest) = New List(Of TpDest)
        'String containing the formatted script
        Dim printString As String = ""
        Dim insertmapFailed = False

        printString &= InsertMap()

        'filter out unused destinations
        For I As Integer = 0 To tpSize - 1
            If (teleportLocations(I).tpType <> 0) Then
                TempDest.Add(teleportLocations(I))
            End If
        Next

        'Sort new array of destinations using TpDest compareTo()
        TempDest.Sort()

        'format alongside map information

        For Each Item As TpDest In TempDest

            printString &= "//" & Item.name & vbNewLine
            printString &= "setpos " & Item.origin & "; setang " & Item.angles & ";" & vbNewLine
            If (Item.tpType = 1) Then
                'Print stage format
                printString &= "sm_setspawn " & Item.stOrBonusNum & ";" & vbNewLine
            ElseIf (Item.tpType = 2) Then
                'Print Bonus
                printString &= "sm_setbspawn " & Item.stOrBonusNum & ";" & vbNewLine
            End If

        Next

        SaveScript(printString)

    End Sub
#End Region
End Class

