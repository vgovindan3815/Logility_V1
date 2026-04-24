Option Strict On
Option Explicit On

Imports Logility_Freight.Core

Namespace Models

    ''' <summary>Operation state of a single batch row.</summary>
    Public Enum OperationStatus
        Pending     ' Not yet run
        Running     ' Currently executing
        Success     ' Completed OK
        Warning     ' No-records (not a hard error)
        [Error]     ' Failed
        Skipped     ' Blank action — skipped
    End Enum

    ''' <summary>
    ''' Allowed batch actions — maps to screen ActionInd values.
    ''' </summary>
    Public Enum BatchAction
        [GET]
        ADD
        CHANGE
        CANCEL
        DELETE
        RELEASE
    End Enum

    ''' <summary>
    ''' Base class for all per-screen batch row models.
    ''' Contains the key fields shared by all FXF3x screens.
    ''' </summary>
    Public MustInherit Class BatchRowBase
        Inherits BaseViewModel

        ' ── Action ───────────────────────────────────────────────────
        Private _action As String = ""
        Public Property Action As String
            Get
                Return _action
            End Get
            Set(v As String)
                SetField(_action, v)
            End Set
        End Property

        ' ── Carrier ──────────────────────────────────────────────────
        Private _carrier As String = "FXFM"
        Public Property Carrier As String
            Get
                Return _carrier
            End Get
            Set(v As String)
                SetField(_carrier, v)
            End Set
        End Property

        ' ── Customer type ────────────────────────────────────────────
        Private _custType As String = "CC"
        Public Property CustType As String
            Get
                Return _custType
            End Get
            Set(v As String)
                SetField(_custType, v)
            End Set
        End Property

        ' ── Account ──────────────────────────────────────────────────
        Private _account As String = ""
        Public Property Account As String
            Get
                Return _account
            End Get
            Set(v As String)
                SetField(_account, v)
            End Set
        End Property

        ' ── Authority / Number / Item ─────────────────────────────────
        Private _authority As String = ""
        Public Property Authority As String
            Get
                Return _authority
            End Get
            Set(v As String)
                SetField(_authority, v)
            End Set
        End Property

        Private _number As String = ""
        Public Property Number As String
            Get
                Return _number
            End Get
            Set(v As String)
                SetField(_number, v)
            End Set
        End Property

        Private _item As String = ""
        Public Property Item As String
            Get
                Return _item
            End Get
            Set(v As String)
                SetField(_item, v)
            End Set
        End Property

        ' ── Release flag ─────────────────────────────────────────────
        Private _release As String = "N"
        Public Property Release As String
            Get
                Return _release
            End Get
            Set(v As String)
                SetField(_release, v)
            End Set
        End Property

        ' ── Cancel date ──────────────────────────────────────────────
        Private _cancelDate As String = ""
        Public Property CancelDate As String
            Get
                Return _cancelDate
            End Get
            Set(v As String)
                SetField(_cancelDate, v)
            End Set
        End Property

        ' ── Status ───────────────────────────────────────────────────
        Private _status As OperationStatus = OperationStatus.Pending
        Public Property Status As OperationStatus
            Get
                Return _status
            End Get
            Set(v As OperationStatus)
                SetField(_status, v)
                NotifyPropertyChanged("StatusIcon")
                NotifyPropertyChanged("IsRunning")
            End Set
        End Property

        Private _statusMessage As String = ""
        Public Property StatusMessage As String
            Get
                Return _statusMessage
            End Get
            Set(v As String)
                SetField(_statusMessage, v)
            End Set
        End Property

        ' ── Computed display helpers ──────────────────────────────────
        Public ReadOnly Property StatusIcon As String
            Get
                Select Case _status
                    Case OperationStatus.Pending  : Return "—"
                    Case OperationStatus.Running  : Return "⏳"
                    Case OperationStatus.Success  : Return "✓"
                    Case OperationStatus.Warning  : Return "⚠"
                    Case OperationStatus.Error    : Return "✗"
                    Case OperationStatus.Skipped  : Return "○"
                    Case Else                     : Return "—"
                End Select
            End Get
        End Property

        Public ReadOnly Property IsRunning As Boolean
            Get
                Return _status = OperationStatus.Running
            End Get
        End Property

        ' ── Helper: parse carrier enum ───────────────────────────────
        Public Function GetCarrierEnum() As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum
            Return DirectCast(
                [Enum].Parse(GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum),
                             _carrier, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCarrierEnum)
        End Function

        ' ── Helper: parse custType enum ──────────────────────────────
        Public Function GetCustTypeEnum() As FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum
            Return DirectCast(
                [Enum].Parse(GetType(FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum),
                             _custType, True),
                FedEx.PABST.SS.SSLib.ScreenScraping.fxfCustTypeEnum)
        End Function

        ' ── Helper: parse cancel date ────────────────────────────────
        Public Function GetCancelDate() As Date
            If String.IsNullOrWhiteSpace(_cancelDate) Then
                Return FedEx.PABST.SS.SSLib.ScreenScraping.NULL_DATE
            End If
            Dim d As Date
            If Date.TryParse(_cancelDate, d) Then Return d
            Return FedEx.PABST.SS.SSLib.ScreenScraping.NULL_DATE
        End Function

        ' ── Helper: check if key is blank ────────────────────────────
        Public ReadOnly Property HasItemKey As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(_authority) AndAlso
                       Not String.IsNullOrWhiteSpace(_number) AndAlso
                       Not String.IsNullOrWhiteSpace(_item)
            End Get
        End Property

        ' ── Row selection (for selective mainframe push) ──────────────
        Private _isSelected As Boolean = False
        Public Property IsSelected As Boolean
            Get
                Return _isSelected
            End Get
            Set(value As Boolean)
                SetField(_isSelected, value)
            End Set
        End Property

        ' ── Detailed error information for debugging ─────────────────
        Private _errorCategory As ScreenErrorCategory = ScreenErrorCategory.None
        Public Property ErrorCategory As ScreenErrorCategory
            Get
                Return _errorCategory
            End Get
            Set(v As ScreenErrorCategory)
                SetField(_errorCategory, v)
            End Set
        End Property

        Private _screenDump As String = ""
        Public Property ScreenDump As String
            Get
                Return _screenDump
            End Get
            Set(v As String)
                SetField(_screenDump, v)
            End Set
        End Property

        Private _exceptionType As String = ""
        Public Property ExceptionType As String
            Get
                Return _exceptionType
            End Get
            Set(v As String)
                SetField(_exceptionType, v)
            End Set
        End Property

        Private _exceptionDetails As String = ""
        Public Property ExceptionDetails As String
            Get
                Return _exceptionDetails
            End Get
            Set(v As String)
                SetField(_exceptionDetails, v)
            End Set
        End Property

        Private _timestamp As DateTime = DateTime.Now
        Public Property ErrorTimestamp As DateTime
            Get
                Return _timestamp
            End Get
            Set(v As DateTime)
                SetField(_timestamp, v)
            End Set
        End Property

    End Class

    ''' <summary>
    ''' Extended base for FXF3B–G screens that have a Part key field.
    ''' </summary>
    Public MustInherit Class BatchRowWithPart
        Inherits BatchRowBase

        Private _part As String = ""
        Public Property Part As String
            Get
                Return _part
            End Get
            Set(v As String)
                SetField(_part, v)
            End Set
        End Property

        Public ReadOnly Property HasItemKeyWithPart As Boolean
            Get
                Return HasItemKey AndAlso Not String.IsNullOrWhiteSpace(_part)
            End Get
        End Property

    End Class

End Namespace
