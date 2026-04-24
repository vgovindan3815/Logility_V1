Option Strict On
Option Explicit On

' ================================================================
'  FXF3B–G BatchRow models
'  All inherit BatchRowWithPart (adds Part key field).
'  Each has screen-specific table row fields flattened inline.
' ================================================================

Namespace Models

    ' ──────────────────────────────────────────────────────────────
    '  FXF3B — Discounts by State/Terminal
    '  Unique fields: geoTbl1, geoTbl2 (2 geo table collections)
    '  Each table has: incExc, dir, type, and up to 5 rows of name+country
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3B_BatchRow
        Inherits BatchRowWithPart

        ' geoTbl1 collection attributes
        Private _gt1IncExc As String = "NA" : Public Property GT1IncExc As String
            Get
                Return _gt1IncExc
            End Get
            Set(v As String)
                SetField(_gt1IncExc, v)
            End Set
        End Property
        Private _gt1Dir As String = "NA" : Public Property GT1Dir As String
            Get
                Return _gt1Dir
            End Get
            Set(v As String)
                SetField(_gt1Dir, v)
            End Set
        End Property
        Private _gt1Type As String = "NA" : Public Property GT1Type As String
            Get
                Return _gt1Type
            End Get
            Set(v As String)
                SetField(_gt1Type, v)
            End Set
        End Property
        ' geoTbl1 rows (up to 5)
        Private _gt1R1Name As String = "" : Public Property GT1R1Name As String
            Get
                Return _gt1R1Name
            End Get
            Set(v As String)
                SetField(_gt1R1Name, v)
            End Set
        End Property
        Private _gt1R1Cty As String = "" : Public Property GT1R1Country As String
            Get
                Return _gt1R1Cty
            End Get
            Set(v As String)
                SetField(_gt1R1Cty, v)
            End Set
        End Property
        ' (pattern repeats for R2..R5 — Claude should expand these)

        ' geoTbl2 collection attributes
        Private _gt2IncExc As String = "NA" : Public Property GT2IncExc As String
            Get
                Return _gt2IncExc
            End Get
            Set(v As String)
                SetField(_gt2IncExc, v)
            End Set
        End Property
        Private _gt2Dir As String = "NA" : Public Property GT2Dir As String
            Get
                Return _gt2Dir
            End Get
            Set(v As String)
                SetField(_gt2Dir, v)
            End Set
        End Property
        Private _gt2Type As String = "NA" : Public Property GT2Type As String
            Get
                Return _gt2Type
            End Get
            Set(v As String)
                SetField(_gt2Type, v)
            End Set
        End Property

        ' Item attributes
        Private _fsAuth As String = ""   : Public Property FsAuth As String
            Get
                Return _fsAuth
            End Get
            Set(v As String)
                SetField(_fsAuth, v)
            End Set
        End Property
        Private _fsNum As String = ""    : Public Property FsNum As String
            Get
                Return _fsNum
            End Get
            Set(v As String)
                SetField(_fsNum, v)
            End Set
        End Property
        Private _fsItem As String = ""   : Public Property FsItem As String
            Get
                Return _fsItem
            End Get
            Set(v As String)
                SetField(_fsItem, v)
            End Set
        End Property
        Private _prepdIn As Boolean  : Public Property PrepdIn As Boolean
            Get
                Return _prepdIn
            End Get
            Set(v As Boolean)
                SetField(_prepdIn, v)
            End Set
        End Property
        Private _prepdOut As Boolean : Public Property PrepdOut As Boolean
            Get
                Return _prepdOut
            End Get
            Set(v As Boolean)
                SetField(_prepdOut, v)
            End Set
        End Property
        Private _collIn As Boolean   : Public Property CollIn As Boolean
            Get
                Return _collIn
            End Get
            Set(v As Boolean)
                SetField(_collIn, v)
            End Set
        End Property
        Private _collOut As Boolean  : Public Property CollOut As Boolean
            Get
                Return _collOut
            End Get
            Set(v As Boolean)
                SetField(_collOut, v)
            End Set
        End Property
        Private _rateEff As String = "" : Public Property RateEff As String
            Get
                Return _rateEff
            End Get
            Set(v As String)
                SetField(_rateEff, v)
            End Set
        End Property
        Private _clsZip As String = "NA" : Public Property ClsZip As String
            Get
                Return _clsZip
            End Get
            Set(v As String)
                SetField(_clsZip, v)
            End Set
        End Property
        Private _genGeoA As String = "NA" : Public Property GenGeoA As String
            Get
                Return _genGeoA
            End Get
            Set(v As String)
                SetField(_genGeoA, v)
            End Set
        End Property
        Private _lastMaintDate As String = "" : Public Property LastMaintDate As String
            Get
                Return _lastMaintDate
            End Get
            Set(v As String)
                SetField(_lastMaintDate, v)
            End Set
        End Property
        Private _operatorId As String = "" : Public Property OperatorId As String
            Get
                Return _operatorId
            End Get
            Set(v As String)
                SetField(_operatorId, v)
            End Set
        End Property
        Private _revision As String = "" : Public Property Revision As String
            Get
                Return _revision
            End Get
            Set(v As String)
                SetField(_revision, v)
            End Set
        End Property

    End Class

    ' ──────────────────────────────────────────────────────────────
    '  FXF3C — Customer Geography Discounts
    '  Unique fields: geoTable in itemHeader (6 fields per row)
    '  servDaysLow, servDaysHigh in itemClass
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3C_BatchRow
        Inherits BatchRowWithPart

        ' geoTable rows (up to 5 — each has plusMinus, dir, type, name, state, country)
        ' Row 1
        Private _r1Pm As String = "NA"   : Public Property R1PlusMinus As String
            Get
                Return _r1Pm
            End Get
            Set(v As String)
                SetField(_r1Pm, v)
            End Set
        End Property
        Private _r1Dir As String = "NA"  : Public Property R1Dir As String
            Get
                Return _r1Dir
            End Get
            Set(v As String)
                SetField(_r1Dir, v)
            End Set
        End Property
        Private _r1Type As String = "NA" : Public Property R1Type As String
            Get
                Return _r1Type
            End Get
            Set(v As String)
                SetField(_r1Type, v)
            End Set
        End Property
        Private _r1Name As String = ""   : Public Property R1Name As String
            Get
                Return _r1Name
            End Get
            Set(v As String)
                SetField(_r1Name, v)
            End Set
        End Property
        Private _r1State As String = ""  : Public Property R1State As String
            Get
                Return _r1State
            End Get
            Set(v As String)
                SetField(_r1State, v)
            End Set
        End Property
        Private _r1Cty As String = ""    : Public Property R1Country As String
            Get
                Return _r1Cty
            End Get
            Set(v As String)
                SetField(_r1Cty, v)
            End Set
        End Property
        ' (Claude should expand R2..R5 with same pattern)

        Private _srvDaysLo As String = "" : Public Property SrvDaysLo As String
            Get
                Return _srvDaysLo
            End Get
            Set(v As String)
                SetField(_srvDaysLo, v)
            End Set
        End Property
        Private _srvDaysHi As String = "" : Public Property SrvDaysHi As String
            Get
                Return _srvDaysHi
            End Get
            Set(v As String)
                SetField(_srvDaysHi, v)
            End Set
        End Property
        Private _lastMaintDate As String = "" : Public Property LastMaintDate As String
            Get
                Return _lastMaintDate
            End Get
            Set(v As String)
                SetField(_lastMaintDate, v)
            End Set
        End Property
        Private _operatorId As String = "" : Public Property OperatorId As String
            Get
                Return _operatorId
            End Get
            Set(v As String)
                SetField(_operatorId, v)
            End Set
        End Property
        Private _revision As String = "" : Public Property Revision As String
            Get
                Return _revision
            End Get
            Set(v As String)
                SetField(_revision, v)
            End Set
        End Property

    End Class

    ' ──────────────────────────────────────────────────────────────
    '  FXF3D — Customer Product Discounts
    '  Unique: effectiveDate, cancelDate, prodTable rows
    '  prodTableRow: type (accountProdType), product1, product2, excCls
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3D_BatchRow
        Inherits BatchRowWithPart

        Private _effDate As String = "" : Public Property EffDate As String
            Get
                Return _effDate
            End Get
            Set(v As String)
                SetField(_effDate, v)
            End Set
        End Property
        Private _canDateItem As String = "" : Public Property CanDateItem As String
            Get
                Return _canDateItem
            End Get
            Set(v As String)
                SetField(_canDateItem, v)
            End Set
        End Property
        Private _excCls As String = "" : Public Property ExcCls As String
            Get
                Return _excCls
            End Get
            Set(v As String)
                SetField(_excCls, v)
            End Set
        End Property
        Private _excMaxW As String = "" : Public Property ExcMaxW As String
            Get
                Return _excMaxW
            End Get
            Set(v As String)
                SetField(_excMaxW, v)
            End Set
        End Property

        ' prodTable rows (up to 5)
        ' Row 1: type (accountProdType enum), product1, product2, excCls
        Private _p1Type As String = "NA" : Public Property P1Type As String
            Get
                Return _p1Type
            End Get
            Set(v As String)
                SetField(_p1Type, v)
            End Set
        End Property
        Private _p1Prod1 As String = "" : Public Property P1Prod1 As String
            Get
                Return _p1Prod1
            End Get
            Set(v As String)
                SetField(_p1Prod1, v)
            End Set
        End Property
        Private _p1Prod2 As String = "" : Public Property P1Prod2 As String
            Get
                Return _p1Prod2
            End Get
            Set(v As String)
                SetField(_p1Prod2, v)
            End Set
        End Property
        Private _p1ExcCls As String = "" : Public Property P1ExcCls As String
            Get
                Return _p1ExcCls
            End Get
            Set(v As String)
                SetField(_p1ExcCls, v)
            End Set
        End Property
        ' (Claude should expand P2..P5 with same pattern)

        Private _lastMaintDate As String = "" : Public Property LastMaintDate As String
            Get
                Return _lastMaintDate
            End Get
            Set(v As String)
                SetField(_lastMaintDate, v)
            End Set
        End Property
        Private _operatorId As String = "" : Public Property OperatorId As String
            Get
                Return _operatorId
            End Get
            Set(v As String)
                SetField(_operatorId, v)
            End Set
        End Property
        Private _revision As String = "" : Public Property Revision As String
            Get
                Return _revision
            End Get
            Set(v As String)
                SetField(_revision, v)
            End Set
        End Property

    End Class

    ' ──────────────────────────────────────────────────────────────
    '  FXF3E — Customer Rates
    '  Unique: condition, prepaidOrCollect, alternation, classRates,
    '          rateManually, clsTrf*, rateEffDate
    '          rateTable rows: weight, type (FXF3E.fxfRateTypeEnum), amount
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3E_BatchRow
        Inherits BatchRowWithPart

        Private _condition As String = "" : Public Property Condition As String
            Get
                Return _condition
            End Get
            Set(v As String)
                SetField(_condition, v)
            End Set
        End Property
        Private _prepdColl As String = "NA" : Public Property PrepdOrCollect As String
            Get
                Return _prepdColl
            End Get
            Set(v As String)
                SetField(_prepdColl, v)
            End Set
        End Property
        Private _effDate As String = "" : Public Property EffDate As String
            Get
                Return _effDate
            End Get
            Set(v As String)
                SetField(_effDate, v)
            End Set
        End Property
        Private _canDateItem As String = "" : Public Property CanDateItem As String
            Get
                Return _canDateItem
            End Get
            Set(v As String)
                SetField(_canDateItem, v)
            End Set
        End Property
        Private _comments As String = "" : Public Property Comments As String
            Get
                Return _comments
            End Get
            Set(v As String)
                SetField(_comments, v)
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
        Private _classRates As String = "NA" : Public Property ClassRates As String
            Get
                Return _classRates
            End Get
            Set(v As String)
                SetField(_classRates, v)
            End Set
        End Property
        Private _rateManually As Boolean : Public Property RateManually As Boolean
            Get
                Return _rateManually
            End Get
            Set(v As Boolean)
                SetField(_rateManually, v)
            End Set
        End Property
        Private _clsTrfAuth As String = "" : Public Property ClsTrfAuth As String
            Get
                Return _clsTrfAuth
            End Get
            Set(v As String)
                SetField(_clsTrfAuth, v)
            End Set
        End Property
        Private _clsTrfNum As String = "" : Public Property ClsTrfNum As String
            Get
                Return _clsTrfNum
            End Get
            Set(v As String)
                SetField(_clsTrfNum, v)
            End Set
        End Property
        Private _clsTrfSec As String = "" : Public Property ClsTrfSec As String
            Get
                Return _clsTrfSec
            End Get
            Set(v As String)
                SetField(_clsTrfSec, v)
            End Set
        End Property
        Private _rateEffDate As String = "" : Public Property RateEffDate As String
            Get
                Return _rateEffDate
            End Get
            Set(v As String)
                SetField(_rateEffDate, v)
            End Set
        End Property

        ' rateTable row 1: weight, type (fxfRateTypeEnum), amount
        Private _rt1Wgt As String = ""  : Public Property RT1Wgt As String
            Get
                Return _rt1Wgt
            End Get
            Set(v As String)
                SetField(_rt1Wgt, v)
            End Set
        End Property
        Private _rt1Type As String = "NA" : Public Property RT1Type As String
            Get
                Return _rt1Type
            End Get
            Set(v As String)
                SetField(_rt1Type, v)
            End Set
        End Property
        Private _rt1Amt As String = "" : Public Property RT1Amt As String
            Get
                Return _rt1Amt
            End Get
            Set(v As String)
                SetField(_rt1Amt, v)
            End Set
        End Property
        ' (Claude should expand RT2..RT10 with same pattern)

        Private _lastMaintDate As String = "" : Public Property LastMaintDate As String
            Get
                Return _lastMaintDate
            End Get
            Set(v As String)
                SetField(_lastMaintDate, v)
            End Set
        End Property
        Private _operatorId As String = "" : Public Property OperatorId As String
            Get
                Return _operatorId
            End Get
            Set(v As String)
                SetField(_operatorId, v)
            End Set
        End Property
        Private _revision As String = "" : Public Property Revision As String
            Get
                Return _revision
            End Get
            Set(v As String)
                SetField(_revision, v)
            End Set
        End Property

    End Class

    ' ──────────────────────────────────────────────────────────────
    '  FXF3F — Customer Discounts/Adjustments
    '  Unique: appRule, adjType in header
    '          rateTable: weight, discAdjDir, discAdjUnits, discAdjType, amount
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3F_BatchRow
        Inherits BatchRowWithPart

        Private _adjType As String = "NA" : Public Property AdjType As String
            Get
                Return _adjType
            End Get
            Set(v As String)
                SetField(_adjType, v)
            End Set
        End Property
        Private _condition As String = "" : Public Property Condition As String
            Get
                Return _condition
            End Get
            Set(v As String)
                SetField(_condition, v)
            End Set
        End Property
        Private _prepdColl As String = "NA" : Public Property PrepdOrCollect As String
            Get
                Return _prepdColl
            End Get
            Set(v As String)
                SetField(_prepdColl, v)
            End Set
        End Property
        Private _effDate As String = "" : Public Property EffDate As String
            Get
                Return _effDate
            End Get
            Set(v As String)
                SetField(_effDate, v)
            End Set
        End Property
        Private _canDateItem As String = "" : Public Property CanDateItem As String
            Get
                Return _canDateItem
            End Get
            Set(v As String)
                SetField(_canDateItem, v)
            End Set
        End Property
        Private _comments As String = "" : Public Property Comments As String
            Get
                Return _comments
            End Get
            Set(v As String)
                SetField(_comments, v)
            End Set
        End Property
        Private _appRule As String = "NA" : Public Property AppRule As String
            Get
                Return _appRule
            End Get
            Set(v As String)
                SetField(_appRule, v)
            End Set
        End Property

        ' rateTable row 1: weight, discAdjDir, discAdjUnits, discAdjType, amount
        Private _rt1Wgt As String = ""    : Public Property RT1Wgt As String
            Get
                Return _rt1Wgt
            End Get
            Set(v As String)
                SetField(_rt1Wgt, v)
            End Set
        End Property
        Private _rt1Dir As String = "NA"  : Public Property RT1DiscAdjDir As String
            Get
                Return _rt1Dir
            End Get
            Set(v As String)
                SetField(_rt1Dir, v)
            End Set
        End Property
        Private _rt1Units As String = "NA" : Public Property RT1DiscAdjUnits As String
            Get
                Return _rt1Units
            End Get
            Set(v As String)
                SetField(_rt1Units, v)
            End Set
        End Property
        Private _rt1Type As String = "NA" : Public Property RT1DiscAdjType As String
            Get
                Return _rt1Type
            End Get
            Set(v As String)
                SetField(_rt1Type, v)
            End Set
        End Property
        Private _rt1Amt As String = ""    : Public Property RT1Amt As String
            Get
                Return _rt1Amt
            End Get
            Set(v As String)
                SetField(_rt1Amt, v)
            End Set
        End Property
        ' (Claude should expand RT2..RT10)

        Private _lastMaintDate As String = "" : Public Property LastMaintDate As String
            Get
                Return _lastMaintDate
            End Get
            Set(v As String)
                SetField(_lastMaintDate, v)
            End Set
        End Property
        Private _operatorId As String = "" : Public Property OperatorId As String
            Get
                Return _operatorId
            End Get
            Set(v As String)
                SetField(_operatorId, v)
            End Set
        End Property
        Private _revision As String = "" : Public Property Revision As String
            Get
                Return _revision
            End Get
            Set(v As String)
                SetField(_revision, v)
            End Set
        End Property

    End Class

    ' ──────────────────────────────────────────────────────────────
    '  FXF3G — Customer Charges/Allowances
    '  Unique: schgTable rows: cond, desc, minWgt, maxWgt, type(String),
    '                          amount, minAmt, maxAmt, app, cond_id
    ' ──────────────────────────────────────────────────────────────
    Public Class FXF3G_BatchRow
        Inherits BatchRowWithPart

        Private _prepdColl As String = "NA" : Public Property PrepdOrCollect As String
            Get
                Return _prepdColl
            End Get
            Set(v As String)
                SetField(_prepdColl, v)
            End Set
        End Property
        Private _effDate As String = "" : Public Property EffDate As String
            Get
                Return _effDate
            End Get
            Set(v As String)
                SetField(_effDate, v)
            End Set
        End Property
        Private _canDateItem As String = "" : Public Property CanDateItem As String
            Get
                Return _canDateItem
            End Get
            Set(v As String)
                SetField(_canDateItem, v)
            End Set
        End Property
        Private _comments As String = "" : Public Property Comments As String
            Get
                Return _comments
            End Get
            Set(v As String)
                SetField(_comments, v)
            End Set
        End Property

        ' schgTable row 1
        Private _s1Cond As String = ""    : Public Property S1Cond As String
            Get
                Return _s1Cond
            End Get
            Set(v As String)
                SetField(_s1Cond, v)
            End Set
        End Property
        Private _s1Desc As String = ""    : Public Property S1Desc As String
            Get
                Return _s1Desc
            End Get
            Set(v As String)
                SetField(_s1Desc, v)
            End Set
        End Property
        Private _s1MinWgt As String = ""  : Public Property S1MinWgt As String
            Get
                Return _s1MinWgt
            End Get
            Set(v As String)
                SetField(_s1MinWgt, v)
            End Set
        End Property
        Private _s1MaxWgt As String = ""  : Public Property S1MaxWgt As String
            Get
                Return _s1MaxWgt
            End Get
            Set(v As String)
                SetField(_s1MaxWgt, v)
            End Set
        End Property
        Private _s1Type As String = ""    : Public Property S1Type As String
            Get
                Return _s1Type
            End Get
            Set(v As String)
                SetField(_s1Type, v)
            End Set
        End Property
        Private _s1Amt As String = ""     : Public Property S1Amount As String
            Get
                Return _s1Amt
            End Get
            Set(v As String)
                SetField(_s1Amt, v)
            End Set
        End Property
        Private _s1MinAmt As String = ""  : Public Property S1MinAmt As String
            Get
                Return _s1MinAmt
            End Get
            Set(v As String)
                SetField(_s1MinAmt, v)
            End Set
        End Property
        Private _s1MaxAmt As String = ""  : Public Property S1MaxAmt As String
            Get
                Return _s1MaxAmt
            End Get
            Set(v As String)
                SetField(_s1MaxAmt, v)
            End Set
        End Property
        Private _s1App As String = ""     : Public Property S1App As String
            Get
                Return _s1App
            End Get
            Set(v As String)
                SetField(_s1App, v)
            End Set
        End Property
        Private _s1CondId As String = ""  : Public Property S1CondId As String
            Get
                Return _s1CondId
            End Get
            Set(v As String)
                SetField(_s1CondId, v)
            End Set
        End Property
        ' (Claude should expand S2..S10)

        Private _lastMaintDate As String = "" : Public Property LastMaintDate As String
            Get
                Return _lastMaintDate
            End Get
            Set(v As String)
                SetField(_lastMaintDate, v)
            End Set
        End Property
        Private _operatorId As String = "" : Public Property OperatorId As String
            Get
                Return _operatorId
            End Get
            Set(v As String)
                SetField(_operatorId, v)
            End Set
        End Property
        Private _revision As String = "" : Public Property Revision As String
            Get
                Return _revision
            End Get
            Set(v As String)
                SetField(_revision, v)
            End Set
        End Property

    End Class

End Namespace
