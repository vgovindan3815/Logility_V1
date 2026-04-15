Option Strict On
Option Explicit On

Imports FedEx.PABST.SS.SSLib
Imports FedEx.PABST.SS.Screens

' ================================================================
'  FXF3J, FXF3K, FXF3M, FXF3N, FXF4M BatchRow models
'  FXF3J  — Copy Account (inherits BatchRowBase, no Part)
'  FXF3K  — State Matrix (inherits BatchRowBase, no Part)
'  FXF3M  — H-Unit Allowance (inherits BatchRowWithPart)
'  FXF3N  — Unit Rates (inherits BatchRowWithPart)
'  FXF4M  — Pay Rule / Earned Discount (inherits BatchRowWithPart)
' ================================================================

Namespace Models

    ' ──────────────────────────────────────────────────────────────
    '  FXF3J — Copy Account
    '  Inherits BatchRowBase (no Part — copy has no Part concept).
    '  FromType/ToType default "CC", ToCarrier defaults "ARFW".
    '  The ViewModel handles the COPY API call directly; ToItemClass
    '  returns Nothing.
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3J_BatchRow
        Inherits BatchRowBase

        ' ── From side ────────────────────────────────────────────────
        Private _fromType As String = "CC" : Public Property FromType As String
            Get
                Return _fromType
            End Get
            Set(v As String)
                SetField(_fromType, v)
            End Set
        End Property
        Private _fromAuth As String = ""   : Public Property FromAuth As String
            Get
                Return _fromAuth
            End Get
            Set(v As String)
                SetField(_fromAuth, v)
            End Set
        End Property
        Private _fromName As String = ""   : Public Property FromName As String
            Get
                Return _fromName
            End Get
            Set(v As String)
                SetField(_fromName, v)
            End Set
        End Property
        Private _fromNbr As String = ""    : Public Property FromNbr As String
            Get
                Return _fromNbr
            End Get
            Set(v As String)
                SetField(_fromNbr, v)
            End Set
        End Property
        Private _fromItem As String = ""   : Public Property FromItem As String
            Get
                Return _fromItem
            End Get
            Set(v As String)
                SetField(_fromItem, v)
            End Set
        End Property
        Private _fromPart As String = ""   : Public Property FromPart As String
            Get
                Return _fromPart
            End Get
            Set(v As String)
                SetField(_fromPart, v)
            End Set
        End Property

        ' ── To side ──────────────────────────────────────────────────
        Private _toType As String = "CC"   : Public Property ToType As String
            Get
                Return _toType
            End Get
            Set(v As String)
                SetField(_toType, v)
            End Set
        End Property
        Private _toAuth As String = ""     : Public Property ToAuth As String
            Get
                Return _toAuth
            End Get
            Set(v As String)
                SetField(_toAuth, v)
            End Set
        End Property
        Private _toName As String = ""     : Public Property ToName As String
            Get
                Return _toName
            End Get
            Set(v As String)
                SetField(_toName, v)
            End Set
        End Property
        Private _toNbr As String = ""      : Public Property ToNbr As String
            Get
                Return _toNbr
            End Get
            Set(v As String)
                SetField(_toNbr, v)
            End Set
        End Property
        Private _toItem As String = ""     : Public Property ToItem As String
            Get
                Return _toItem
            End Get
            Set(v As String)
                SetField(_toItem, v)
            End Set
        End Property
        Private _toPart As String = ""     : Public Property ToPart As String
            Get
                Return _toPart
            End Get
            Set(v As String)
                SetField(_toPart, v)
            End Set
        End Property

        ' ── Copy options ──────────────────────────────────────────────
        Private _toCarrier As String = "ARFW" : Public Property ToCarrier As String
            Get
                Return _toCarrier
            End Get
            Set(v As String)
                SetField(_toCarrier, v)
            End Set
        End Property
        Private _toRelease As String = "N" : Public Property ToRelease As String
            Get
                Return _toRelease
            End Get
            Set(v As String)
                SetField(_toRelease, v)
            End Set
        End Property
        Private _copyEffDate As String = "" : Public Property CopyEffDate As String
            Get
                Return _copyEffDate
            End Get
            Set(v As String)
                SetField(_copyEffDate, v)
            End Set
        End Property

        ' ── ToItemClass — not applicable for COPY ────────────────────
        ' Copy uses a different API handled entirely in ViewModel.
        Public Function ToItemClass() As Object
            Return Nothing
        End Function

        ' ── Private helpers ──────────────────────────────────────────
        Private Shared Function ParseDate(s As String) As Date
            If String.IsNullOrWhiteSpace(s) Then Return ScreenScraping.NULL_DATE
            Dim d As Date
            Return If(Date.TryParse(s, d), d, ScreenScraping.NULL_DATE)
        End Function

        Private Shared Function ParseEnum(Of T As Structure)(s As String, defaultVal As String) As T
            Dim target = If(String.IsNullOrWhiteSpace(s), defaultVal, s)
            Return DirectCast([Enum].Parse(GetType(T), target, True), T)
        End Function

        Private Shared Function GetEnumName(Of T As Structure)(value As T) As String
            Return [Enum].GetName(GetType(T), value)
        End Function

    End Class

    ' ──────────────────────────────────────────────────────────────
    '  FXF3K — State Matrix
    '  Inherits BatchRowBase (no Part).
    '  Actions: GET, DELETE, CANCEL.
    '  No ADD/CHANGE — a 101x101 matrix cannot be batch-entered.
    '  No ToItemClass or FromItemClass — matrix data is complex and
    '  handled entirely in ViewModel.
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3K_BatchRow
        Inherits BatchRowBase

        Private _matrixName As String = ""       : Public Property MatrixName As String
            Get
                Return _matrixName
            End Get
            Set(v As String)
                SetField(_matrixName, v)
            End Set
        End Property
        Private _matrixEffDate As String = ""    : Public Property MatrixEffDate As String
            Get
                Return _matrixEffDate
            End Get
            Set(v As String)
                SetField(_matrixEffDate, v)
            End Set
        End Property
        Private _matrixCancelDate As String = "" : Public Property MatrixCancelDate As String
            Get
                Return _matrixCancelDate
            End Get
            Set(v As String)
                SetField(_matrixCancelDate, v)
            End Set
        End Property

        ' ── Private helpers ──────────────────────────────────────────
        Private Shared Function ParseDate(s As String) As Date
            If String.IsNullOrWhiteSpace(s) Then Return ScreenScraping.NULL_DATE
            Dim d As Date
            Return If(Date.TryParse(s, d), d, ScreenScraping.NULL_DATE)
        End Function

    End Class

    ' ──────────────────────────────────────────────────────────────
    '  FXF3M — H-Unit Allowance
    '  Unique: hUnit (CalcRule, allowance sub-object, EWR sub-object)
    '          fsAuthority/fsNumber/fsItem, rateManual, huType,
    '          condition, prepaidOrCollect, effectiveDate, cancelDate,
    '          comments, lastMaintenanceDate, operatorId, revision
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3M_BatchRow
        Inherits BatchRowWithPart

        Private _fsAuth As String = ""        : Public Property FsAuth As String
            Get
                Return _fsAuth
            End Get
            Set(v As String)
                SetField(_fsAuth, v)
            End Set
        End Property
        Private _fsNum As String = ""         : Public Property FsNum As String
            Get
                Return _fsNum
            End Get
            Set(v As String)
                SetField(_fsNum, v)
            End Set
        End Property
        Private _fsItem As String = ""        : Public Property FsItem As String
            Get
                Return _fsItem
            End Get
            Set(v As String)
                SetField(_fsItem, v)
            End Set
        End Property
        Private _rateManual As String        : Public Property RateManual As String
            Get
                Return _rateManual
            End Get
            Set(v As String)
                SetField(_rateManual, v)
            End Set
        End Property
        Private _huType As String = ""        : Public Property HuType As String
            Get
                Return _huType
            End Get
            Set(v As String)
                SetField(_huType, v)
            End Set
        End Property
        Private _condition As String = ""     : Public Property Condition As String
            Get
                Return _condition
            End Get
            Set(v As String)
                SetField(_condition, v)
            End Set
        End Property
        Private _prepdColl As String = "NA"   : Public Property PrepdOrCollect As String
            Get
                Return _prepdColl
            End Get
            Set(v As String)
                SetField(_prepdColl, v)
            End Set
        End Property
        Private _effDate As String = ""       : Public Property EffDate As String
            Get
                Return _effDate
            End Get
            Set(v As String)
                SetField(_effDate, v)
            End Set
        End Property
        Private _canDateItem As String = ""   : Public Property CanDateItem As String
            Get
                Return _canDateItem
            End Get
            Set(v As String)
                SetField(_canDateItem, v)
            End Set
        End Property
        Private _comments As String = ""      : Public Property Comments As String
            Get
                Return _comments
            End Get
            Set(v As String)
                SetField(_comments, v)
            End Set
        End Property

        ' hUnit fields
        Private _calcRule As String = ""      : Public Property CalcRule As String
            Get
                Return _calcRule
            End Get
            Set(v As String)
                SetField(_calcRule, v)
            End Set
        End Property

        ' hUnit.allowance fields
        Private _allowMaxNum As String = ""   : Public Property AllowMaxNum As String
            Get
                Return _allowMaxNum
            End Get
            Set(v As String)
                SetField(_allowMaxNum, v)
            End Set
        End Property
        Private _allowMaxTotWgt As String = "" : Public Property AllowMaxTotWgt As String
            Get
                Return _allowMaxTotWgt
            End Get
            Set(v As String)
                SetField(_allowMaxTotWgt, v)
            End Set
        End Property
        Private _allowMaxPerWgt As String = "" : Public Property AllowMaxPerWgt As String
            Get
                Return _allowMaxPerWgt
            End Get
            Set(v As String)
                SetField(_allowMaxPerWgt, v)
            End Set
        End Property

        ' hUnit.EWR fields
        Private _ewrCls As String           : Public Property EwrCls As String
            Get
                Return _ewrCls
            End Get
            Set(v As String)
                SetField(_ewrCls, v)
            End Set
        End Property
        Private _ewrClsNum As String = ""    : Public Property EwrClsNum As String
            Get
                Return _ewrClsNum
            End Get
            Set(v As String)
                SetField(_ewrClsNum, v)
            End Set
        End Property
        Private _ewrLowRate As String       : Public Property EwrLowRate As String
            Get
                Return _ewrLowRate
            End Get
            Set(v As String)
                SetField(_ewrLowRate, v)
            End Set
        End Property
        Private _ewrHighRate As String      : Public Property EwrHighRate As String
            Get
                Return _ewrHighRate
            End Get
            Set(v As String)
                SetField(_ewrHighRate, v)
            End Set
        End Property
        Private _ewrHighestVolByWgt As String = "N" : Public Property EwrHighestVolByWgt As String
            Get
                Return _ewrHighestVolByWgt
            End Get
            Set(v As String)
                SetField(_ewrHighestVolByWgt, v)
            End Set
        End Property

        ' Audit fields
        Private _lastMaintDate As String = "" : Public Property LastMaintDate As String
            Get
                Return _lastMaintDate
            End Get
            Set(v As String)
                SetField(_lastMaintDate, v)
            End Set
        End Property
        Private _operatorId As String = ""   : Public Property OperatorId As String
            Get
                Return _operatorId
            End Get
            Set(v As String)
                SetField(_operatorId, v)
            End Set
        End Property
        Private _revision As String = ""     : Public Property Revision As String
            Get
                Return _revision
            End Get
            Set(v As String)
                SetField(_revision, v)
            End Set
        End Property

        ' ── Build FXF3M.itemClass from this row ──────────────────────
        Public Function ToItemClass() As FXF3M.itemClass
            Dim it As New FXF3M.itemClass
            it.itemHeader.auhority = Authority
            it.itemHeader.number   = Number
            it.itemHeader.item     = Item
            it.itemHeader.part     = Part

            it.fsAuthority      = FsAuth
            it.fsNumber         = FsNum
            it.fsItem           = FsItem
            it.rateManual       = (RateManual = "Y")
            it.huType           = HuType
            it.condition        = Condition
            it.prepaidOrCollect = ParseEnum(Of ScreenScraping.fxfPrepaidOrCollectEnum)(PrepdOrCollect, "NA")
            it.effectiveDate    = ParseDate(EffDate)
            it.cancelDate       = ParseDate(CanDateItem)
            it.comments         = Comments

            Dim hu As New FXF3M.hUnitClass()
            hu.CalcRule = CalcRule

            Dim allow As New FXF3M.hUnitAllowanceClass()
            allow.MaxNum    = AllowMaxNum
            allow.MaxTotWgt = AllowMaxTotWgt
            allow.MaxPerWgt = AllowMaxPerWgt
            hu.allowance = allow

            Dim ewr As New FXF3M.hUnitExcessWeightRuleClass()
            ewr.Cls             = (EwrCls             = "Y")
            ewr.ClsNum          = EwrClsNum
            ewr.LowRate         = (EwrLowRate         = "Y")
            ewr.HighRate        = (EwrHighRate        = "Y")
            ewr.HighestVolByWgt = (EwrHighestVolByWgt = "Y")
            hu.EWR = ewr

            it.hUnit = hu
            Return it
        End Function

        ' ── Populate this row from a FXF3M.itemClass result ──────────
        Public Sub FromItemClass(it As FXF3M.itemClass)
            Authority = it.itemHeader.auhority
            Number    = it.itemHeader.number
            Item      = it.itemHeader.item
            Part      = it.itemHeader.part

            FsAuth         = it.fsAuthority
            FsNum          = it.fsNumber
            FsItem         = it.fsItem
            RateManual     = If(it.rateManual, "Y", "N")
            HuType         = it.huType
            Condition      = it.condition
            PrepdOrCollect = GetEnumName(Of ScreenScraping.fxfPrepaidOrCollectEnum)(it.prepaidOrCollect)
            EffDate        = FormatDate(it.effectiveDate)
            CanDateItem    = FormatDate(it.cancelDate)
            Comments       = it.comments

            If it.hUnit IsNot Nothing Then
                CalcRule = it.hUnit.CalcRule
                If it.hUnit.allowance IsNot Nothing Then
                    AllowMaxNum    = it.hUnit.allowance.MaxNum
                    AllowMaxTotWgt = it.hUnit.allowance.MaxTotWgt
                    AllowMaxPerWgt = it.hUnit.allowance.MaxPerWgt
                End If
                If it.hUnit.EWR IsNot Nothing Then
                    EwrCls             = If(it.hUnit.EWR.Cls,             "Y", "N")
                    EwrClsNum          = it.hUnit.EWR.ClsNum
                    EwrLowRate         = If(it.hUnit.EWR.LowRate,         "Y", "N")
                    EwrHighRate        = If(it.hUnit.EWR.HighRate,        "Y", "N")
                    EwrHighestVolByWgt = If(it.hUnit.EWR.HighestVolByWgt, "Y", "N")
                End If
            End If

            LastMaintDate = FormatDate(it.lastMaintenanceDate)
            OperatorId    = it.operatorId
            Revision      = it.revision
            Status        = OperationStatus.Success
        End Sub

        ' ── Private helpers ──────────────────────────────────────────
        Private Shared Function ParseDate(s As String) As Date
            If String.IsNullOrWhiteSpace(s) Then Return ScreenScraping.NULL_DATE
            Dim d As Date
            Return If(Date.TryParse(s, d), d, ScreenScraping.NULL_DATE)
        End Function

        Private Shared Function FormatDate(d As Date) As String
            If d = ScreenScraping.NULL_DATE OrElse d = Date.MinValue Then Return ""
            Return d.ToString("MM/dd/yy")
        End Function

        Private Shared Function ParseEnum(Of T As Structure)(s As String, defaultVal As String) As T
            Dim target = If(String.IsNullOrWhiteSpace(s), defaultVal, s)
            Return DirectCast([Enum].Parse(GetType(T), target, True), T)
        End Function

        Private Shared Function GetEnumName(Of T As Structure)(value As T) As String
            Return [Enum].GetName(GetType(T), value)
        End Function

    End Class

    ' ──────────────────────────────────────────────────────────────
    '  FXF3N — Unit Rates
    '  Unique: condition, prepaidOrCollect, alternation, classRates,
    '          clsTrf*, rateEffDate, huType (fxfHUType enum),
    '          mileage auth/num/range, rateTable rows (up to 10).
    '  rateTableRow.rateType is ScreenScraping.fxfRateType (NOT
    '  fxfRateTypeEnum — this is a different type from SSLib).
    '  rateTable columns per row: MinHunits, MaxHunits,
    '          AvgMinHunit, AvgMaxHunit (integers), rateType, amount.
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3N_BatchRow
        Inherits BatchRowWithPart

        Private _condition As String = ""     : Public Property Condition As String
            Get
                Return _condition
            End Get
            Set(v As String)
                SetField(_condition, v)
            End Set
        End Property
        Private _prepdColl As String = "NA"   : Public Property PrepdOrCollect As String
            Get
                Return _prepdColl
            End Get
            Set(v As String)
                SetField(_prepdColl, v)
            End Set
        End Property
        Private _effDate As String = ""       : Public Property EffDate As String
            Get
                Return _effDate
            End Get
            Set(v As String)
                SetField(_effDate, v)
            End Set
        End Property
        Private _canDateItem As String = ""   : Public Property CanDateItem As String
            Get
                Return _canDateItem
            End Get
            Set(v As String)
                SetField(_canDateItem, v)
            End Set
        End Property
        Private _alternation As String = "NA" : Public Property Alternation As String
            Get
                Return _alternation
            End Get
            Set(v As String)
                SetField(_alternation, v)
            End Set
        End Property
        Private _classRates As String = "NA"  : Public Property ClassRates As String
            Get
                Return _classRates
            End Get
            Set(v As String)
                SetField(_classRates, v)
            End Set
        End Property
        Private _clsTrfAuth As String = ""    : Public Property ClsTrfAuth As String
            Get
                Return _clsTrfAuth
            End Get
            Set(v As String)
                SetField(_clsTrfAuth, v)
            End Set
        End Property
        Private _clsTrfNum As String = ""     : Public Property ClsTrfNum As String
            Get
                Return _clsTrfNum
            End Get
            Set(v As String)
                SetField(_clsTrfNum, v)
            End Set
        End Property
        Private _clsTrfSec As String = ""     : Public Property ClsTrfSec As String
            Get
                Return _clsTrfSec
            End Get
            Set(v As String)
                SetField(_clsTrfSec, v)
            End Set
        End Property
        Private _rateEffDate As String = ""   : Public Property RateEffDate As String
            Get
                Return _rateEffDate
            End Get
            Set(v As String)
                SetField(_rateEffDate, v)
            End Set
        End Property
        Private _huType As String = "NA"      : Public Property HuType As String
            Get
                Return _huType
            End Get
            Set(v As String)
                SetField(_huType, v)
            End Set
        End Property
        Private _mileageAuth As String = ""   : Public Property MileageAuth As String
            Get
                Return _mileageAuth
            End Get
            Set(v As String)
                SetField(_mileageAuth, v)
            End Set
        End Property
        Private _mileageNum As String = ""    : Public Property MileageNum As String
            Get
                Return _mileageNum
            End Get
            Set(v As String)
                SetField(_mileageNum, v)
            End Set
        End Property
        Private _mileageRangeLow As String = ""  : Public Property MileageRangeLow As String
            Get
                Return _mileageRangeLow
            End Get
            Set(v As String)
                SetField(_mileageRangeLow, v)
            End Set
        End Property
        Private _mileageRangeHigh As String = "" : Public Property MileageRangeHigh As String
            Get
                Return _mileageRangeHigh
            End Get
            Set(v As String)
                SetField(_mileageRangeHigh, v)
            End Set
        End Property
        Private _comments As String = ""      : Public Property Comments As String
            Get
                Return _comments
            End Get
            Set(v As String)
                SetField(_comments, v)
            End Set
        End Property

        ' rateTable rows (up to 10): MinHunits, MaxHunits, AvgMinHunit, AvgMaxHunit (int as string), RateType, Amount
        ' Row 1
        Private _rt1MinH As String = ""    : Public Property RT1MinH As String
            Get
                Return _rt1MinH
            End Get
            Set(v As String)
                SetField(_rt1MinH, v)
            End Set
        End Property
        Private _rt1MaxH As String = ""    : Public Property RT1MaxH As String
            Get
                Return _rt1MaxH
            End Get
            Set(v As String)
                SetField(_rt1MaxH, v)
            End Set
        End Property
        Private _rt1AvgMin As String = ""  : Public Property RT1AvgMin As String
            Get
                Return _rt1AvgMin
            End Get
            Set(v As String)
                SetField(_rt1AvgMin, v)
            End Set
        End Property
        Private _rt1AvgMax As String = ""  : Public Property RT1AvgMax As String
            Get
                Return _rt1AvgMax
            End Get
            Set(v As String)
                SetField(_rt1AvgMax, v)
            End Set
        End Property
        Private _rt1RateType As String = "NA" : Public Property RT1RateType As String
            Get
                Return _rt1RateType
            End Get
            Set(v As String)
                SetField(_rt1RateType, v)
            End Set
        End Property
        Private _rt1Amt As String = ""     : Public Property RT1Amt As String
            Get
                Return _rt1Amt
            End Get
            Set(v As String)
                SetField(_rt1Amt, v)
            End Set
        End Property
        ' Row 2
        Private _rt2MinH As String = ""    : Public Property RT2MinH As String
            Get
                Return _rt2MinH
            End Get
            Set(v As String)
                SetField(_rt2MinH, v)
            End Set
        End Property
        Private _rt2MaxH As String = ""    : Public Property RT2MaxH As String
            Get
                Return _rt2MaxH
            End Get
            Set(v As String)
                SetField(_rt2MaxH, v)
            End Set
        End Property
        Private _rt2AvgMin As String = ""  : Public Property RT2AvgMin As String
            Get
                Return _rt2AvgMin
            End Get
            Set(v As String)
                SetField(_rt2AvgMin, v)
            End Set
        End Property
        Private _rt2AvgMax As String = ""  : Public Property RT2AvgMax As String
            Get
                Return _rt2AvgMax
            End Get
            Set(v As String)
                SetField(_rt2AvgMax, v)
            End Set
        End Property
        Private _rt2RateType As String = "NA" : Public Property RT2RateType As String
            Get
                Return _rt2RateType
            End Get
            Set(v As String)
                SetField(_rt2RateType, v)
            End Set
        End Property
        Private _rt2Amt As String = ""     : Public Property RT2Amt As String
            Get
                Return _rt2Amt
            End Get
            Set(v As String)
                SetField(_rt2Amt, v)
            End Set
        End Property
        ' Row 3
        Private _rt3MinH As String = ""    : Public Property RT3MinH As String
            Get
                Return _rt3MinH
            End Get
            Set(v As String)
                SetField(_rt3MinH, v)
            End Set
        End Property
        Private _rt3MaxH As String = ""    : Public Property RT3MaxH As String
            Get
                Return _rt3MaxH
            End Get
            Set(v As String)
                SetField(_rt3MaxH, v)
            End Set
        End Property
        Private _rt3AvgMin As String = ""  : Public Property RT3AvgMin As String
            Get
                Return _rt3AvgMin
            End Get
            Set(v As String)
                SetField(_rt3AvgMin, v)
            End Set
        End Property
        Private _rt3AvgMax As String = ""  : Public Property RT3AvgMax As String
            Get
                Return _rt3AvgMax
            End Get
            Set(v As String)
                SetField(_rt3AvgMax, v)
            End Set
        End Property
        Private _rt3RateType As String = "NA" : Public Property RT3RateType As String
            Get
                Return _rt3RateType
            End Get
            Set(v As String)
                SetField(_rt3RateType, v)
            End Set
        End Property
        Private _rt3Amt As String = ""     : Public Property RT3Amt As String
            Get
                Return _rt3Amt
            End Get
            Set(v As String)
                SetField(_rt3Amt, v)
            End Set
        End Property
        ' Row 4
        Private _rt4MinH As String = ""    : Public Property RT4MinH As String
            Get
                Return _rt4MinH
            End Get
            Set(v As String)
                SetField(_rt4MinH, v)
            End Set
        End Property
        Private _rt4MaxH As String = ""    : Public Property RT4MaxH As String
            Get
                Return _rt4MaxH
            End Get
            Set(v As String)
                SetField(_rt4MaxH, v)
            End Set
        End Property
        Private _rt4AvgMin As String = ""  : Public Property RT4AvgMin As String
            Get
                Return _rt4AvgMin
            End Get
            Set(v As String)
                SetField(_rt4AvgMin, v)
            End Set
        End Property
        Private _rt4AvgMax As String = ""  : Public Property RT4AvgMax As String
            Get
                Return _rt4AvgMax
            End Get
            Set(v As String)
                SetField(_rt4AvgMax, v)
            End Set
        End Property
        Private _rt4RateType As String = "NA" : Public Property RT4RateType As String
            Get
                Return _rt4RateType
            End Get
            Set(v As String)
                SetField(_rt4RateType, v)
            End Set
        End Property
        Private _rt4Amt As String = ""     : Public Property RT4Amt As String
            Get
                Return _rt4Amt
            End Get
            Set(v As String)
                SetField(_rt4Amt, v)
            End Set
        End Property
        ' Row 5
        Private _rt5MinH As String = ""    : Public Property RT5MinH As String
            Get
                Return _rt5MinH
            End Get
            Set(v As String)
                SetField(_rt5MinH, v)
            End Set
        End Property
        Private _rt5MaxH As String = ""    : Public Property RT5MaxH As String
            Get
                Return _rt5MaxH
            End Get
            Set(v As String)
                SetField(_rt5MaxH, v)
            End Set
        End Property
        Private _rt5AvgMin As String = ""  : Public Property RT5AvgMin As String
            Get
                Return _rt5AvgMin
            End Get
            Set(v As String)
                SetField(_rt5AvgMin, v)
            End Set
        End Property
        Private _rt5AvgMax As String = ""  : Public Property RT5AvgMax As String
            Get
                Return _rt5AvgMax
            End Get
            Set(v As String)
                SetField(_rt5AvgMax, v)
            End Set
        End Property
        Private _rt5RateType As String = "NA" : Public Property RT5RateType As String
            Get
                Return _rt5RateType
            End Get
            Set(v As String)
                SetField(_rt5RateType, v)
            End Set
        End Property
        Private _rt5Amt As String = ""     : Public Property RT5Amt As String
            Get
                Return _rt5Amt
            End Get
            Set(v As String)
                SetField(_rt5Amt, v)
            End Set
        End Property
        ' Row 6
        Private _rt6MinH As String = ""    : Public Property RT6MinH As String
            Get
                Return _rt6MinH
            End Get
            Set(v As String)
                SetField(_rt6MinH, v)
            End Set
        End Property
        Private _rt6MaxH As String = ""    : Public Property RT6MaxH As String
            Get
                Return _rt6MaxH
            End Get
            Set(v As String)
                SetField(_rt6MaxH, v)
            End Set
        End Property
        Private _rt6AvgMin As String = ""  : Public Property RT6AvgMin As String
            Get
                Return _rt6AvgMin
            End Get
            Set(v As String)
                SetField(_rt6AvgMin, v)
            End Set
        End Property
        Private _rt6AvgMax As String = ""  : Public Property RT6AvgMax As String
            Get
                Return _rt6AvgMax
            End Get
            Set(v As String)
                SetField(_rt6AvgMax, v)
            End Set
        End Property
        Private _rt6RateType As String = "NA" : Public Property RT6RateType As String
            Get
                Return _rt6RateType
            End Get
            Set(v As String)
                SetField(_rt6RateType, v)
            End Set
        End Property
        Private _rt6Amt As String = ""     : Public Property RT6Amt As String
            Get
                Return _rt6Amt
            End Get
            Set(v As String)
                SetField(_rt6Amt, v)
            End Set
        End Property
        ' Row 7
        Private _rt7MinH As String = ""    : Public Property RT7MinH As String
            Get
                Return _rt7MinH
            End Get
            Set(v As String)
                SetField(_rt7MinH, v)
            End Set
        End Property
        Private _rt7MaxH As String = ""    : Public Property RT7MaxH As String
            Get
                Return _rt7MaxH
            End Get
            Set(v As String)
                SetField(_rt7MaxH, v)
            End Set
        End Property
        Private _rt7AvgMin As String = ""  : Public Property RT7AvgMin As String
            Get
                Return _rt7AvgMin
            End Get
            Set(v As String)
                SetField(_rt7AvgMin, v)
            End Set
        End Property
        Private _rt7AvgMax As String = ""  : Public Property RT7AvgMax As String
            Get
                Return _rt7AvgMax
            End Get
            Set(v As String)
                SetField(_rt7AvgMax, v)
            End Set
        End Property
        Private _rt7RateType As String = "NA" : Public Property RT7RateType As String
            Get
                Return _rt7RateType
            End Get
            Set(v As String)
                SetField(_rt7RateType, v)
            End Set
        End Property
        Private _rt7Amt As String = ""     : Public Property RT7Amt As String
            Get
                Return _rt7Amt
            End Get
            Set(v As String)
                SetField(_rt7Amt, v)
            End Set
        End Property
        ' Row 8
        Private _rt8MinH As String = ""    : Public Property RT8MinH As String
            Get
                Return _rt8MinH
            End Get
            Set(v As String)
                SetField(_rt8MinH, v)
            End Set
        End Property
        Private _rt8MaxH As String = ""    : Public Property RT8MaxH As String
            Get
                Return _rt8MaxH
            End Get
            Set(v As String)
                SetField(_rt8MaxH, v)
            End Set
        End Property
        Private _rt8AvgMin As String = ""  : Public Property RT8AvgMin As String
            Get
                Return _rt8AvgMin
            End Get
            Set(v As String)
                SetField(_rt8AvgMin, v)
            End Set
        End Property
        Private _rt8AvgMax As String = ""  : Public Property RT8AvgMax As String
            Get
                Return _rt8AvgMax
            End Get
            Set(v As String)
                SetField(_rt8AvgMax, v)
            End Set
        End Property
        Private _rt8RateType As String = "NA" : Public Property RT8RateType As String
            Get
                Return _rt8RateType
            End Get
            Set(v As String)
                SetField(_rt8RateType, v)
            End Set
        End Property
        Private _rt8Amt As String = ""     : Public Property RT8Amt As String
            Get
                Return _rt8Amt
            End Get
            Set(v As String)
                SetField(_rt8Amt, v)
            End Set
        End Property
        ' Row 9
        Private _rt9MinH As String = ""    : Public Property RT9MinH As String
            Get
                Return _rt9MinH
            End Get
            Set(v As String)
                SetField(_rt9MinH, v)
            End Set
        End Property
        Private _rt9MaxH As String = ""    : Public Property RT9MaxH As String
            Get
                Return _rt9MaxH
            End Get
            Set(v As String)
                SetField(_rt9MaxH, v)
            End Set
        End Property
        Private _rt9AvgMin As String = ""  : Public Property RT9AvgMin As String
            Get
                Return _rt9AvgMin
            End Get
            Set(v As String)
                SetField(_rt9AvgMin, v)
            End Set
        End Property
        Private _rt9AvgMax As String = ""  : Public Property RT9AvgMax As String
            Get
                Return _rt9AvgMax
            End Get
            Set(v As String)
                SetField(_rt9AvgMax, v)
            End Set
        End Property
        Private _rt9RateType As String = "NA" : Public Property RT9RateType As String
            Get
                Return _rt9RateType
            End Get
            Set(v As String)
                SetField(_rt9RateType, v)
            End Set
        End Property
        Private _rt9Amt As String = ""     : Public Property RT9Amt As String
            Get
                Return _rt9Amt
            End Get
            Set(v As String)
                SetField(_rt9Amt, v)
            End Set
        End Property
        ' Row 10
        Private _rt10MinH As String = ""   : Public Property RT10MinH As String
            Get
                Return _rt10MinH
            End Get
            Set(v As String)
                SetField(_rt10MinH, v)
            End Set
        End Property
        Private _rt10MaxH As String = ""   : Public Property RT10MaxH As String
            Get
                Return _rt10MaxH
            End Get
            Set(v As String)
                SetField(_rt10MaxH, v)
            End Set
        End Property
        Private _rt10AvgMin As String = "" : Public Property RT10AvgMin As String
            Get
                Return _rt10AvgMin
            End Get
            Set(v As String)
                SetField(_rt10AvgMin, v)
            End Set
        End Property
        Private _rt10AvgMax As String = "" : Public Property RT10AvgMax As String
            Get
                Return _rt10AvgMax
            End Get
            Set(v As String)
                SetField(_rt10AvgMax, v)
            End Set
        End Property
        Private _rt10RateType As String = "NA" : Public Property RT10RateType As String
            Get
                Return _rt10RateType
            End Get
            Set(v As String)
                SetField(_rt10RateType, v)
            End Set
        End Property
        Private _rt10Amt As String = ""    : Public Property RT10Amt As String
            Get
                Return _rt10Amt
            End Get
            Set(v As String)
                SetField(_rt10Amt, v)
            End Set
        End Property

        ' Audit fields
        Private _lastMaintDate As String = "" : Public Property LastMaintDate As String
            Get
                Return _lastMaintDate
            End Get
            Set(v As String)
                SetField(_lastMaintDate, v)
            End Set
        End Property
        Private _operatorId As String = ""   : Public Property OperatorId As String
            Get
                Return _operatorId
            End Get
            Set(v As String)
                SetField(_operatorId, v)
            End Set
        End Property
        Private _revision As String = ""     : Public Property Revision As String
            Get
                Return _revision
            End Get
            Set(v As String)
                SetField(_revision, v)
            End Set
        End Property

        ' ── Build FXF3N.itemClass from this row ──────────────────────
        Public Function ToItemClass() As FXF3N.itemClass
            Dim it As New FXF3N.itemClass
            it.itemHeader.auhority = Authority
            it.itemHeader.number   = Number
            it.itemHeader.item     = Item
            it.itemHeader.part     = Part

            it.condition        = Condition
            it.prepaidOrCollect = ParseEnum(Of ScreenScraping.fxfPrepaidOrCollectEnum)(PrepdOrCollect, "NA")
            it.effectiveDate    = ParseDate(EffDate)
            it.cancelDate       = ParseDate(CanDateItem)
            it.alternation      = ParseEnum(Of ScreenScraping.fxfAlternationEnum)(Alternation, "NA")
            it.classRates       = ParseEnum(Of ScreenScraping.fxfClassZipEnum)(ClassRates, "NA")
            it.clsTrfAuthority  = ClsTrfAuth
            it.clsTrfNumber     = ClsTrfNum
            it.clsTrfSection    = ClsTrfSec
            it.rateEffDate      = ParseDate(RateEffDate)
            it.huType           = ParseEnum(Of ScreenScraping.fxfHUType)(HuType, "NA")
            it.mileageAuthority = MileageAuth
            it.mileageNumber    = MileageNum
            it.comments         = Comments

            Dim loV As Integer
            it.mileageRangeLow  = If(Integer.TryParse(MileageRangeLow,  loV), loV, ScreenScraping.NULL_INT)
            Dim hiV As Integer
            it.mileageRangeHigh = If(Integer.TryParse(MileageRangeHigh, hiV), hiV, ScreenScraping.NULL_INT)

            Dim rt As New FXF3N.rateTableCollection()
            Dim addRate As Action(Of String, String, String, String, String, String) =
                Sub(minS As String, maxS As String, avgMinS As String, avgMaxS As String,
                    typeS As String, amtS As String)
                    Dim minV As Integer
                    If Not Integer.TryParse(minS, minV) Then minV = 0
                    ' Skip blank rows: minHunits = 0 AND rateType = "NA" AND amount empty
                    If minV = 0 AndAlso
                       (String.IsNullOrWhiteSpace(typeS) OrElse typeS.Equals("NA", StringComparison.OrdinalIgnoreCase)) AndAlso
                       String.IsNullOrWhiteSpace(amtS) Then Return
                    Dim row As New FXF3N.rateTableRow()
                    row.MinHunits = minV
                    Dim maxV As Integer
                    row.MaxHunits    = If(Integer.TryParse(maxS,   maxV), maxV, 0)
                    Dim avgMinV As Integer
                    row.AvgMinHunit  = If(Integer.TryParse(avgMinS, avgMinV), avgMinV, 0)
                    Dim avgMaxV As Integer
                    row.AvgMaxHunit  = If(Integer.TryParse(avgMaxS, avgMaxV), avgMaxV, 0)
                    Dim rtype As ScreenScraping.fxfRateType
                    If [Enum].TryParse(Of ScreenScraping.fxfRateType)(typeS, True, rtype) Then row.rateType = rtype
                    Dim av As Single
                    If Single.TryParse(amtS, av) Then row.amount = av
                    rt.Add(row)
                End Sub

            addRate(RT1MinH,  RT1MaxH,  RT1AvgMin,  RT1AvgMax,  RT1RateType,  RT1Amt)
            addRate(RT2MinH,  RT2MaxH,  RT2AvgMin,  RT2AvgMax,  RT2RateType,  RT2Amt)
            addRate(RT3MinH,  RT3MaxH,  RT3AvgMin,  RT3AvgMax,  RT3RateType,  RT3Amt)
            addRate(RT4MinH,  RT4MaxH,  RT4AvgMin,  RT4AvgMax,  RT4RateType,  RT4Amt)
            addRate(RT5MinH,  RT5MaxH,  RT5AvgMin,  RT5AvgMax,  RT5RateType,  RT5Amt)
            addRate(RT6MinH,  RT6MaxH,  RT6AvgMin,  RT6AvgMax,  RT6RateType,  RT6Amt)
            addRate(RT7MinH,  RT7MaxH,  RT7AvgMin,  RT7AvgMax,  RT7RateType,  RT7Amt)
            addRate(RT8MinH,  RT8MaxH,  RT8AvgMin,  RT8AvgMax,  RT8RateType,  RT8Amt)
            addRate(RT9MinH,  RT9MaxH,  RT9AvgMin,  RT9AvgMax,  RT9RateType,  RT9Amt)
            addRate(RT10MinH, RT10MaxH, RT10AvgMin, RT10AvgMax, RT10RateType, RT10Amt)
            it.rateTable = rt
            Return it
        End Function

        ' ── Populate this row from a FXF3N.itemClass result ──────────
        Public Sub FromItemClass(it As FXF3N.itemClass)
            Authority = it.itemHeader.auhority
            Number    = it.itemHeader.number
            Item      = it.itemHeader.item
            Part      = it.itemHeader.part

            Condition      = it.condition
            PrepdOrCollect = GetEnumName(Of ScreenScraping.fxfPrepaidOrCollectEnum)(it.prepaidOrCollect)
            EffDate        = FormatDate(it.effectiveDate)
            CanDateItem    = FormatDate(it.cancelDate)
            Alternation    = GetEnumName(Of ScreenScraping.fxfAlternationEnum)(it.alternation)
            ClassRates     = GetEnumName(Of ScreenScraping.fxfClassZipEnum)(it.classRates)
            ClsTrfAuth     = it.clsTrfAuthority
            ClsTrfNum      = it.clsTrfNumber
            ClsTrfSec      = it.clsTrfSection
            RateEffDate    = FormatDate(it.rateEffDate)
            HuType         = GetEnumName(Of ScreenScraping.fxfHUType)(it.huType)
            MileageAuth    = it.mileageAuthority
            MileageNum     = it.mileageNumber
            MileageRangeLow  = If(it.mileageRangeLow  = ScreenScraping.NULL_INT, "", it.mileageRangeLow.ToString())
            MileageRangeHigh = If(it.mileageRangeHigh = ScreenScraping.NULL_INT, "", it.mileageRangeHigh.ToString())
            Comments       = it.comments

            If it.rateTable IsNot Nothing Then
                Dim idx As Integer = 0
                For Each r As FXF3N.rateTableRow In it.rateTable
                    If idx >= 10 Then Exit For
                    Dim minh As String = r.MinHunits.ToString()
                    Dim maxh As String = r.MaxHunits.ToString()
                    Dim avgn As String = r.AvgMinHunit.ToString()
                    Dim avgx As String = r.AvgMaxHunit.ToString()
                    Dim rtyp As String = GetEnumName(Of ScreenScraping.fxfRateType)(r.rateType)
                    Dim amt  As String = r.amount.ToString()
                    Select Case idx
                        Case 0
                            RT1MinH = minh : RT1MaxH = maxh : RT1AvgMin = avgn : RT1AvgMax = avgx : RT1RateType = rtyp : RT1Amt = amt
                        Case 1
                            RT2MinH = minh : RT2MaxH = maxh : RT2AvgMin = avgn : RT2AvgMax = avgx : RT2RateType = rtyp : RT2Amt = amt
                        Case 2
                            RT3MinH = minh : RT3MaxH = maxh : RT3AvgMin = avgn : RT3AvgMax = avgx : RT3RateType = rtyp : RT3Amt = amt
                        Case 3
                            RT4MinH = minh : RT4MaxH = maxh : RT4AvgMin = avgn : RT4AvgMax = avgx : RT4RateType = rtyp : RT4Amt = amt
                        Case 4
                            RT5MinH = minh : RT5MaxH = maxh : RT5AvgMin = avgn : RT5AvgMax = avgx : RT5RateType = rtyp : RT5Amt = amt
                        Case 5
                            RT6MinH = minh : RT6MaxH = maxh : RT6AvgMin = avgn : RT6AvgMax = avgx : RT6RateType = rtyp : RT6Amt = amt
                        Case 6
                            RT7MinH = minh : RT7MaxH = maxh : RT7AvgMin = avgn : RT7AvgMax = avgx : RT7RateType = rtyp : RT7Amt = amt
                        Case 7
                            RT8MinH = minh : RT8MaxH = maxh : RT8AvgMin = avgn : RT8AvgMax = avgx : RT8RateType = rtyp : RT8Amt = amt
                        Case 8
                            RT9MinH = minh : RT9MaxH = maxh : RT9AvgMin = avgn : RT9AvgMax = avgx : RT9RateType = rtyp : RT9Amt = amt
                        Case 9
                            RT10MinH = minh : RT10MaxH = maxh : RT10AvgMin = avgn : RT10AvgMax = avgx : RT10RateType = rtyp : RT10Amt = amt
                    End Select
                    idx += 1
                Next
            End If

            LastMaintDate = FormatDate(it.lastMaintenanceDate)
            OperatorId    = it.operatorId
            Revision      = it.revision
            Status        = OperationStatus.Success
        End Sub

        ' ── Private helpers ──────────────────────────────────────────
        Private Shared Function ParseDate(s As String) As Date
            If String.IsNullOrWhiteSpace(s) Then Return ScreenScraping.NULL_DATE
            Dim d As Date
            Return If(Date.TryParse(s, d), d, ScreenScraping.NULL_DATE)
        End Function

        Private Shared Function FormatDate(d As Date) As String
            If d = ScreenScraping.NULL_DATE OrElse d = Date.MinValue Then Return ""
            Return d.ToString("MM/dd/yy")
        End Function

        Private Shared Function ParseEnum(Of T As Structure)(s As String, defaultVal As String) As T
            Dim target = If(String.IsNullOrWhiteSpace(s), defaultVal, s)
            Return DirectCast([Enum].Parse(GetType(T), target, True), T)
        End Function

        Private Shared Function GetEnumName(Of T As Structure)(value As T) As String
            Return [Enum].GetName(GetType(T), value)
        End Function

    End Class

    ' ──────────────────────────────────────────────────────────────
    '  FXF4M — Pay Rule / Earned Discount
    '  Inherits BatchRowWithPart — Part maps to payRule in API calls.
    '  FXF4M does NOT have addItem or changeItem; it has:
    '    getItem, getItems, cancelItem, setEdTable, deleteItem.
    '  Therefore ToItemClass is not meaningful (returns Nothing).
    '  FromItemClass populates from a FXF4M.itemClass result.
    '  NOTE: FXF4M.itemHeaderClass has payRule (not part) and uses
    '        the auhority spelling typo consistent with other screens.
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF4M_BatchRow
        Inherits BatchRowWithPart

        Private _effDate As String = ""      : Public Property EffDate As String
            Get
                Return _effDate
            End Get
            Set(v As String)
                SetField(_effDate, v)
            End Set
        End Property
        Private _expDate As String = ""      : Public Property ExpDate As String
            Get
                Return _expDate
            End Get
            Set(v As String)
                SetField(_expDate, v)
            End Set
        End Property
        ' Secondary pay rule reference from itemClass.payRule
        ' (distinct from the key payRule stored in Part / itemHeader.payRule)
        Private _payRule As String = ""      : Public Property PayRule As String
            Get
                Return _payRule
            End Get
            Set(v As String)
                SetField(_payRule, v)
            End Set
        End Property
        Private _fsAuth As String = ""       : Public Property FsAuth As String
            Get
                Return _fsAuth
            End Get
            Set(v As String)
                SetField(_fsAuth, v)
            End Set
        End Property
        Private _fsNum As String = ""        : Public Property FsNum As String
            Get
                Return _fsNum
            End Get
            Set(v As String)
                SetField(_fsNum, v)
            End Set
        End Property
        Private _fsItem As String = ""       : Public Property FsItem As String
            Get
                Return _fsItem
            End Get
            Set(v As String)
                SetField(_fsItem, v)
            End Set
        End Property
        Private _prepdInbound As String     : Public Property PrepaidInbound As String
            Get
                Return _prepdInbound
            End Get
            Set(v As String)
                SetField(_prepdInbound, v)
            End Set
        End Property
        Private _prepdOutbound As String    : Public Property PrepaidOutbound As String
            Get
                Return _prepdOutbound
            End Get
            Set(v As String)
                SetField(_prepdOutbound, v)
            End Set
        End Property
        Private _collInbound As String      : Public Property CollectInbound As String
            Get
                Return _collInbound
            End Get
            Set(v As String)
                SetField(_collInbound, v)
            End Set
        End Property
        Private _collOutbound As String     : Public Property CollectOutbound As String
            Get
                Return _collOutbound
            End Get
            Set(v As String)
                SetField(_collOutbound, v)
            End Set
        End Property
        Private _thirdParty As String       : Public Property ThirdParty As String
            Get
                Return _thirdParty
            End Get
            Set(v As String)
                SetField(_thirdParty, v)
            End Set
        End Property
        Private _inter As String = "NA"      : Public Property Inter As String
            Get
                Return _inter
            End Get
            Set(v As String)
                SetField(_inter, v)
            End Set
        End Property
        Private _typeHaul As String = "NA"   : Public Property TypeHaul As String
            Get
                Return _typeHaul
            End Get
            Set(v As String)
                SetField(_typeHaul, v)
            End Set
        End Property
        Private _country As String = ""      : Public Property Country As String
            Get
                Return _country
            End Get
            Set(v As String)
                SetField(_country, v)
            End Set
        End Property
        Private _hdrUpdDate As String = ""   : Public Property HdrUpdDate As String
            Get
                Return _hdrUpdDate
            End Get
            Set(v As String)
                SetField(_hdrUpdDate, v)
            End Set
        End Property
        Private _hdrUpdUserId As String = "" : Public Property HdrUpdUserID As String
            Get
                Return _hdrUpdUserId
            End Get
            Set(v As String)
                SetField(_hdrUpdUserId, v)
            End Set
        End Property

        ' Audit fields
        Private _lastMaintDate As String = "" : Public Property LastMaintDate As String
            Get
                Return _lastMaintDate
            End Get
            Set(v As String)
                SetField(_lastMaintDate, v)
            End Set
        End Property
        Private _operatorId As String = ""   : Public Property OperatorId As String
            Get
                Return _operatorId
            End Get
            Set(v As String)
                SetField(_operatorId, v)
            End Set
        End Property
        Private _revision As String = ""     : Public Property Revision As String
            Get
                Return _revision
            End Get
            Set(v As String)
                SetField(_revision, v)
            End Set
        End Property

        ' ── ToItemClass — FXF4M has no addItem/changeItem ────────────
        ' No write operations are supported via itemClass; handled in
        ' ViewModel via setEdTable / cancelItem / deleteItem APIs.
        Public Function ToItemClass() As Object
            Return Nothing
        End Function

        ' ── Populate this row from a FXF4M.itemClass result ──────────
        Public Sub FromItemClass(it As FXF4M.itemClass)
            ' FXF4M.itemHeaderClass uses payRule (not part) as the key field
            Authority = it.itemHeader.auhority
            Number    = it.itemHeader.number
            Item      = it.itemHeader.item
            Part      = it.itemHeader.payRule   ' payRule is the key stored in Part

            EffDate  = FormatDate(it.effectiveDate)
            ExpDate  = FormatDate(it.cancelDate)
            PayRule  = it.payRule

            FsAuth   = it.fsAuthority
            FsNum    = it.fsNumber
            FsItem   = it.fsItem

            PrepaidInbound  = If(it.prepaidInbound,  "Y", "N")
            PrepaidOutbound = If(it.prepaidOutbound, "Y", "N")
            CollectInbound  = If(it.collectInbound,  "Y", "N")
            CollectOutbound = If(it.collectOutbound, "Y", "N")
            ThirdParty      = If(it.thirdParty,      "Y", "N")

            Inter    = GetEnumName(Of ScreenScraping.fxfInterEnum)(it.inter)
            TypeHaul = GetEnumName(Of ScreenScraping.fxfTypeHaulEnum)(it.typeHaul)
            Country  = it.country

            HdrUpdDate   = FormatDate(it.hdrUpdDate)
            HdrUpdUserID = it.hdrUpdUserID

            LastMaintDate = FormatDate(it.hdrUpdDate)
            OperatorId    = it.hdrUpdUserID
            Revision      = ""
            Status        = OperationStatus.Success
        End Sub

        ' ── Private helpers ──────────────────────────────────────────
        Private Shared Function ParseDate(s As String) As Date
            If String.IsNullOrWhiteSpace(s) Then Return ScreenScraping.NULL_DATE
            Dim d As Date
            Return If(Date.TryParse(s, d), d, ScreenScraping.NULL_DATE)
        End Function

        Private Shared Function FormatDate(d As Date) As String
            If d = ScreenScraping.NULL_DATE OrElse d = Date.MinValue Then Return ""
            Return d.ToString("MM/dd/yy")
        End Function

        Private Shared Function ParseEnum(Of T As Structure)(s As String, defaultVal As String) As T
            Dim target = If(String.IsNullOrWhiteSpace(s), defaultVal, s)
            Return DirectCast([Enum].Parse(GetType(T), target, True), T)
        End Function

        Private Shared Function GetEnumName(Of T As Structure)(value As T) As String
            Return [Enum].GetName(GetType(T), value)
        End Function

    End Class

End Namespace
