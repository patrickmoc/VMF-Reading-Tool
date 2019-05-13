'Represents an info_teleport_destination, storing it's position, angle and name
Public Class TpDest

#Region "Properties"
    'angles of the teleport destination
    Private _angles As String

    Public Property angles() As String
        Get
            Return _angles
        End Get
        Set(value As String)
            _angles = value
        End Set
    End Property

    'name of the teleport destination
    Private _name As String

    Public Property name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
        End Set
    End Property

    'origin of teleport destination
    Private _origin As String

    Public Property origin As String
        Get
            Return _origin
        End Get
        Set(value As String)
            _origin = value
        End Set
    End Property

    'type of teleport destination:
    '0: Unused
    '1: Stage teleport
    '2: Bonus teleport
    Private _tpType As Integer

    Public Property tpType As Integer
        Get
            Return _tpType
        End Get
        Set(value As Integer)
            If (value <= 2 And value >= 0) Then
                _tpType = value
            Else
                Throw (New ArgumentOutOfRangeException("Invalid value for tpType """ & value & """"))
            End If
        End Set
    End Property

    'Stage or Bonus number, used for order of stages and bonuses
    Private _stOrBonusNum As Integer

    Public Property stOrBonusNum As Integer
        Get
            Return _stOrBonusNum
        End Get
        Set(value As Integer)
            _stOrBonusNum = value
        End Set
    End Property

#End Region

#Region "Constructors"
    Public Sub New(Optional anglesIn As String = Nothing,
                   Optional nameIn As String = Nothing,
                   Optional originIn As String = Nothing)

        If anglesIn IsNot Nothing Then
            angles = anglesIn
        End If

        If nameIn IsNot Nothing Then
            name = nameIn
        End If

        If originIn IsNot Nothing Then
            origin = originIn
        End If

        tpType = 0
        stOrBonusNum = -1
    End Sub
#End Region

#Region "Overrides"
    'TODO add tptype and storbonusnum to string...
    Public Overrides Function ToString() As String
        Dim strReturn As String = "Name:" & vbTab & name & vbNewLine &
                                  "Origin:" & vbTab & origin & vbNewLine &
                                  "Angles:" & vbTab & angles & vbNewLine
        Return strReturn
    End Function
#End Region
End Class
