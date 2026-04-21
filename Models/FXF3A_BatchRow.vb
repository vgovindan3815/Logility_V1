Option Strict On
Option Explicit On

Imports FedEx.PABST.SS.SSLib
Imports FedEx.PABST.SS.Screens

Namespace Models

    ''' <summary>
    ''' One row in the FXF3A batch DataGrid.
    ''' Maps directly to FXF3A.itemClass.
    ''' Discount table is flattened to 3 inline rows (Disc1..Disc3).
    ''' </summary>
    Public Class FXF3A_BatchRow
        Inherits BatchRowBase

        ' ── Discount row 1 ───────────────────────────────────────────
        Private _disc1 As String = "" : Public Property Disc1 As String
            Get
                Return _disc1
            End Get
            Set(v As String)
                SetField(_disc1, v)
            End Set
        End Property
        Private _minChg1 As String = "" : Public Property MinChg1 As String
            Get
                Return _minChg1
            End Get
            Set(v As String)
                SetField(_minChg1, v)
            End Set
        End Property
        Private _maxWgt1 As String = "" : Public Property MaxWgt1 As String
            Get
                Return _maxWgt1
            End Get
            Set(v As String)
                SetField(_maxWgt1, v)
            End Set
        End Property
        Private _floorMin1 As String = "" : Public Property FloorMin1 As String
            Get
                Return _floorMin1
            End Get
            Set(v As String)
                SetField(_floorMin1, v)
            End Set
        End Property
        Private _effDate1 As String = "" : Public Property EffDate1 As String
            Get
                Return _effDate1
            End Get
            Set(v As String)
                SetField(_effDate1, v)
            End Set
        End Property
        Private _canDate1 As String = "" : Public Property CanDate1 As String
            Get
                Return _canDate1
            End Get
            Set(v As String)
                SetField(_canDate1, v)
            End Set
        End Property

        ' ── Discount row 2 ───────────────────────────────────────────
        Private _disc2 As String = "" : Public Property Disc2 As String
            Get
                Return _disc2
            End Get
            Set(v As String)
                SetField(_disc2, v)
            End Set
        End Property
        Private _minChg2 As String = "" : Public Property MinChg2 As String
            Get
                Return _minChg2
            End Get
            Set(v As String)
                SetField(_minChg2, v)
            End Set
        End Property
        Private _maxWgt2 As String = "" : Public Property MaxWgt2 As String
            Get
                Return _maxWgt2
            End Get
            Set(v As String)
                SetField(_maxWgt2, v)
            End Set
        End Property
        Private _floorMin2 As String = "" : Public Property FloorMin2 As String
            Get
                Return _floorMin2
            End Get
            Set(v As String)
                SetField(_floorMin2, v)
            End Set
        End Property
        Private _effDate2 As String = "" : Public Property EffDate2 As String
            Get
                Return _effDate2
            End Get
            Set(v As String)
                SetField(_effDate2, v)
            End Set
        End Property
        Private _canDate2 As String = "" : Public Property CanDate2 As String
            Get
                Return _canDate2
            End Get
            Set(v As String)
                SetField(_canDate2, v)
            End Set
        End Property

        ' ── Discount row 3 ───────────────────────────────────────────
        Private _disc3 As String = "" : Public Property Disc3 As String
            Get
                Return _disc3
            End Get
            Set(v As String)
                SetField(_disc3, v)
            End Set
        End Property
        Private _minChg3 As String = "" : Public Property MinChg3 As String
            Get
                Return _minChg3
            End Get
            Set(v As String)
                SetField(_minChg3, v)
            End Set
        End Property
        Private _maxWgt3 As String = "" : Public Property MaxWgt3 As String
            Get
                Return _maxWgt3
            End Get
            Set(v As String)
                SetField(_maxWgt3, v)
            End Set
        End Property
        Private _floorMin3 As String = "" : Public Property FloorMin3 As String
            Get
                Return _floorMin3
            End Get
            Set(v As String)
                SetField(_floorMin3, v)
            End Set
        End Property
        Private _effDate3 As String = "" : Public Property EffDate3 As String
            Get
                Return _effDate3
            End Get
            Set(v As String)
                SetField(_effDate3, v)
            End Set
        End Property
        Private _canDate3 As String = "" : Public Property CanDate3 As String
            Get
                Return _canDate3
            End Get
            Set(v As String)
                SetField(_canDate3, v)
            End Set
        End Property

        ' ── Item attributes ──────────────────────────────────────────
        Private _currency As String = "USD" : Public Property Currency As String
            Get
                Return _currency
            End Get
            Set(v As String)
                SetField(_currency, v)
            End Set
        End Property
        Private _fsAuth As String = "" : Public Property FsAuth As String
            Get
                Return _fsAuth
            End Get
            Set(v As String)
                SetField(_fsAuth, v)
            End Set
        End Property
        Private _fsNum As String = "" : Public Property FsNum As String
            Get
                Return _fsNum
            End Get
            Set(v As String)
                SetField(_fsNum, v)
            End Set
        End Property
        Private _fsItem As String = "" : Public Property FsItem As String
            Get
                Return _fsItem
            End Get
            Set(v As String)
                SetField(_fsItem, v)
            End Set
        End Property
        Private _prepdIn As String = "N" : Public Property PrepdIn As String
            Get
                Return _prepdIn
            End Get
            Set(v As String)
                SetField(_prepdIn, v)
            End Set
        End Property
        Private _prepdOut As String = "N" : Public Property PrepdOut As String
            Get
                Return _prepdOut
            End Get
            Set(v As String)
                SetField(_prepdOut, v)
            End Set
        End Property
        Private _collIn As String = "N" : Public Property CollIn As String
            Get
                Return _collIn
            End Get
            Set(v As String)
                SetField(_collIn, v)
            End Set
        End Property
        Private _collOut As String = "N" : Public Property CollOut As String
            Get
                Return _collOut
            End Get
            Set(v As String)
                SetField(_collOut, v)
            End Set
        End Property
        Private _thirdParty As String = "N" : Public Property ThirdParty As String
            Get
                Return _thirdParty
            End Get
            Set(v As String)
                SetField(_thirdParty, v)
            End Set
        End Property
        Private _inter As String = "NA" : Public Property Inter As String
            Get
                Return _inter
            End Get
            Set(v As String)
                SetField(_inter, v)
            End Set
        End Property
        Private _typeHaul As String = "NA" : Public Property TypeHaul As String
            Get
                Return _typeHaul
            End Get
            Set(v As String)
                SetField(_typeHaul, v)
            End Set
        End Property
        Private _country As String = "" : Public Property Country As String
            Get
                Return _country
            End Get
            Set(v As String)
                SetField(_country, v)
            End Set
        End Property
        Private _matrix As String = "" : Public Property Matrix As String
            Get
                Return _matrix
            End Get
            Set(v As String)
                SetField(_matrix, v)
            End Set
        End Property
        Private _geoDir1 As String = "NA" : Public Property GeoDir1 As String
            Get
                Return _geoDir1
            End Get
            Set(v As String)
                SetField(_geoDir1, v)
            End Set
        End Property
        Private _geoType1 As String = "NA" : Public Property GeoType1 As String
            Get
                Return _geoType1
            End Get
            Set(v As String)
                SetField(_geoType1, v)
            End Set
        End Property
        Private _geoName1 As String = "" : Public Property GeoName1 As String
            Get
                Return _geoName1
            End Get
            Set(v As String)
                SetField(_geoName1, v)
            End Set
        End Property
        Private _geoSt1 As String = "" : Public Property GeoSt1 As String
            Get
                Return _geoSt1
            End Get
            Set(v As String)
                SetField(_geoSt1, v)
            End Set
        End Property
        Private _geoCty1 As String = "" : Public Property GeoCty1 As String
            Get
                Return _geoCty1
            End Get
            Set(v As String)
                SetField(_geoCty1, v)
            End Set
        End Property
        Private _geoDir2 As String = "NA" : Public Property GeoDir2 As String
            Get
                Return _geoDir2
            End Get
            Set(v As String)
                SetField(_geoDir2, v)
            End Set
        End Property
        Private _geoType2 As String = "NA" : Public Property GeoType2 As String
            Get
                Return _geoType2
            End Get
            Set(v As String)
                SetField(_geoType2, v)
            End Set
        End Property
        Private _geoName2 As String = "" : Public Property GeoName2 As String
            Get
                Return _geoName2
            End Get
            Set(v As String)
                SetField(_geoName2, v)
            End Set
        End Property
        Private _geoSt2 As String = "" : Public Property GeoSt2 As String
            Get
                Return _geoSt2
            End Get
            Set(v As String)
                SetField(_geoSt2, v)
            End Set
        End Property
        Private _geoCty2 As String = "" : Public Property GeoCty2 As String
            Get
                Return _geoCty2
            End Get
            Set(v As String)
                SetField(_geoCty2, v)
            End Set
        End Property
        Private _ratesEff As String = "" : Public Property RatesEff As String
            Get
                Return _ratesEff
            End Get
            Set(v As String)
                SetField(_ratesEff, v)
            End Set
        End Property
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
        Private _clsZip As String = "NA" : Public Property ClsZip As String
            Get
                Return _clsZip
            End Get
            Set(v As String)
                SetField(_clsZip, v)
            End Set
        End Property
        Private _czAuth As String = "" : Public Property CzAuth As String
            Get
                Return _czAuth
            End Get
            Set(v As String)
                SetField(_czAuth, v)
            End Set
        End Property
        Private _czNum As String = "" : Public Property CzNum As String
            Get
                Return _czNum
            End Get
            Set(v As String)
                SetField(_czNum, v)
            End Set
        End Property
        Private _czSec As String = "" : Public Property CzSec As String
            Get
                Return _czSec
            End Get
            Set(v As String)
                SetField(_czSec, v)
            End Set
        End Property
        Private _applyArbs As String = "N" : Public Property ApplyArbs As String
            Get
                Return _applyArbs
            End Get
            Set(v As String)
                SetField(_applyArbs, v)
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
        Private _genGeoA As String = "NA" : Public Property GenGeoA As String
            Get
                Return _genGeoA
            End Get
            Set(v As String)
                SetField(_genGeoA, v)
            End Set
        End Property
        Private _minWgt As String = "" : Public Property MinWgt As String
            Get
                Return _minWgt
            End Get
            Set(v As String)
                SetField(_minWgt, v)
            End Set
        End Property
        Private _maxWgt As String = "" : Public Property MaxWgt As String
            Get
                Return _maxWgt
            End Get
            Set(v As String)
                SetField(_maxWgt, v)
            End Set
        End Property
        Private _incExempt As String = "N" : Public Property IncExempt As String
            Get
                Return _incExempt
            End Get
            Set(v As String)
                SetField(_incExempt, v)
            End Set
        End Property
        Private _fak As String = "N" : Public Property Fak As String
            Get
                Return _fak
            End Get
            Set(v As String)
                SetField(_fak, v)
            End Set
        End Property
        Private _edAgg As String = "" : Public Property EdAgg As String
            Get
                Return _edAgg
            End Get
            Set(v As String)
                SetField(_edAgg, v)
            End Set
        End Property

        ' ── NMFC class flags ─────────────────────────────────────────
        Private _n50  As String = "N" : Public Property N50  As String
            Get
                Return _n50 
            End Get
            Set(v As String)
                SetField(_n50, v) 
            End Set
        End Property
        Private _n55  As String = "N" : Public Property N55  As String
            Get
                Return _n55 
            End Get
            Set(v As String)
                SetField(_n55, v) 
            End Set
        End Property
        Private _n60  As String = "N" : Public Property N60  As String
            Get
                Return _n60 
            End Get
            Set(v As String)
                SetField(_n60, v) 
            End Set
        End Property
        Private _n65  As String = "N" : Public Property N65  As String
            Get
                Return _n65 
            End Get
            Set(v As String)
                SetField(_n65, v) 
            End Set
        End Property
        Private _n70  As String = "N" : Public Property N70  As String
            Get
                Return _n70 
            End Get
            Set(v As String)
                SetField(_n70, v) 
            End Set
        End Property
        Private _n77  As String = "N" : Public Property N77_5 As String
            Get
                Return _n77 
            End Get
            Set(v As String)
                SetField(_n77, v) 
            End Set
        End Property
        Private _n85  As String = "N" : Public Property N85  As String
            Get
                Return _n85 
            End Get
            Set(v As String)
                SetField(_n85, v) 
            End Set
        End Property
        Private _n92  As String = "N" : Public Property N92_5 As String
            Get
                Return _n92 
            End Get
            Set(v As String)
                SetField(_n92, v) 
            End Set
        End Property
        Private _n100 As String = "N" : Public Property N100 As String
            Get
                Return _n100
            End Get
            Set(v As String)
                SetField(_n100, v)
            End Set
        End Property
        Private _n110 As String = "N" : Public Property N110 As String
            Get
                Return _n110
            End Get
            Set(v As String)
                SetField(_n110, v)
            End Set
        End Property
        Private _n125 As String = "N" : Public Property N125 As String
            Get
                Return _n125
            End Get
            Set(v As String)
                SetField(_n125, v)
            End Set
        End Property
        Private _n150 As String = "N" : Public Property N150 As String
            Get
                Return _n150
            End Get
            Set(v As String)
                SetField(_n150, v)
            End Set
        End Property
        Private _n175 As String = "N" : Public Property N175 As String
            Get
                Return _n175
            End Get
            Set(v As String)
                SetField(_n175, v)
            End Set
        End Property
        Private _n200 As String = "N" : Public Property N200 As String
            Get
                Return _n200
            End Get
            Set(v As String)
                SetField(_n200, v)
            End Set
        End Property
        Private _n250 As String = "N" : Public Property N250 As String
            Get
                Return _n250
            End Get
            Set(v As String)
                SetField(_n250, v)
            End Set
        End Property
        Private _n300 As String = "N" : Public Property N300 As String
            Get
                Return _n300
            End Get
            Set(v As String)
                SetField(_n300, v)
            End Set
        End Property
        Private _n400 As String = "N" : Public Property N400 As String
            Get
                Return _n400
            End Get
            Set(v As String)
                SetField(_n400, v)
            End Set
        End Property
        Private _n500 As String = "N" : Public Property N500 As String
            Get
                Return _n500
            End Get
            Set(v As String)
                SetField(_n500, v)
            End Set
        End Property
        Private _payRule1 As String = "" : Public Property PayRule1 As String
            Get
                Return _payRule1
            End Get
            Set(v As String)
                SetField(_payRule1, v)
            End Set
        End Property
        Private _payRule2 As String = "" : Public Property PayRule2 As String
            Get
                Return _payRule2
            End Get
            Set(v As String)
                SetField(_payRule2, v)
            End Set
        End Property

        ' ── Read-only from screen (populated by GET) ─────────────────
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

        ' ── Build FXF3A.itemClass from this row ──────────────────────
        Public Function ToItemClass() As FXF3A.itemClass
            Dim it As New FXF3A.itemClass
            it.itemHeader.auhority  = Authority   ' note: typo in source — must match
            it.itemHeader.number    = Number
            it.itemHeader.item      = Item
            it.itemHeader.discTable = BuildDiscTable()

            it.currency          = _currency
            it.fsAuthority       = _fsAuth
            it.fsNumber          = _fsNum
            it.fsItem            = _fsItem
            it.prepaidInbound    = (_prepdIn    = "Y")
            it.prepaidOutbound   = (_prepdOut   = "Y")
            it.collectInbound    = (_collIn     = "Y")
            it.collectOutbound   = (_collOut    = "Y")
            it.thirdParty        = (_thirdParty = "Y")
            it.inter             = ParseEnum(Of ScreenScraping.fxfInterEnum)(_inter, "NA")
            it.typeHaul          = ParseEnum(Of ScreenScraping.fxfTypeHaulEnum)(_typeHaul, "NA")
            it.country           = _country
            it.matrix            = _matrix
            it.geoDir1           = ParseEnum(Of ScreenScraping.fxfGeoDirEnum)(_geoDir1, "NA")
            it.geoType1          = ParseEnum(Of ScreenScraping.fxfGeoTypeEnum)(_geoType1, "NA")
            it.geoName1          = _geoName1
            it.geoState1         = _geoSt1
            it.geoCountry1       = _geoCty1
            it.geoDir2           = ParseEnum(Of ScreenScraping.fxfGeoDirEnum)(_geoDir2, "NA")
            it.geoType2          = ParseEnum(Of ScreenScraping.fxfGeoTypeEnum)(_geoType2, "NA")
            it.geoName2          = _geoName2
            it.geoState2         = _geoSt2
            it.geoCountry2       = _geoCty2
            it.ratesEffective    = ParseDate(_ratesEff)
            it.servDaysLow       = _srvDaysLo
            it.servDaysHigh      = _srvDaysHi
            it.classZip          = ParseEnum(Of ScreenScraping.fxfClassZipEnum)(_clsZip, "NA")
            it.classZipAuthority = _czAuth
            it.classZipNumber    = _czNum
            it.classZipSection   = _czSec
            it.applyArbs         = (_applyArbs = "Y")
            it.excClass          = _excCls
            it.excClassMaxWgt    = _excMaxW
            it.genGeoAlt         = ParseEnum(Of ScreenScraping.fxfGenGeoAltrEnum)(_genGeoA, "NA")
            it.minWgt            = _minWgt
            it.maxWgt            = _maxWgt
            it.incExempt         = (_incExempt = "Y")
            it.fak               = (_fak       = "Y")
            it.edAgg             = _edAgg
            it.nmfc50   = (_n50  = "Y") : it.nmfc55  = (_n55  = "Y") : it.nmfc60  = (_n60  = "Y")
            it.nmfc65   = (_n65  = "Y") : it.nmfc70  = (_n70  = "Y") : it.nmfc77_5= (_n77  = "Y")
            it.nmfc85   = (_n85  = "Y") : it.nmfc92_5= (_n92  = "Y") : it.nmfc100 = (_n100 = "Y")
            it.nmfc110  = (_n110 = "Y") : it.nmfc125 = (_n125 = "Y") : it.nmfc150 = (_n150 = "Y")
            it.nmfc175  = (_n175 = "Y") : it.nmfc200 = (_n200 = "Y") : it.nmfc250 = (_n250 = "Y")
            it.nmfc300  = (_n300 = "Y") : it.nmfc400 = (_n400 = "Y") : it.nmfc500 = (_n500 = "Y")
            it.payRule1 = _payRule1
            it.payRule2 = _payRule2
            Return it
        End Function

        ' ── Populate this row from a FXF3A.itemClass result ──────────
        Public Sub FromItemClass(it As FXF3A.itemClass)
            Authority    = it.itemHeader.auhority
            Number       = it.itemHeader.number
            Item         = it.itemHeader.item
            If it.itemHeader.discTable IsNot Nothing AndAlso it.itemHeader.discTable.Count > 0 Then
                Dim r0 As FXF3A.discountTable = DirectCast(it.itemHeader.discTable(0), FXF3A.discountTable)
                _disc1 = r0.disc.ToString() : _minChg1 = r0.minChargeDisc.ToString()
                _maxWgt1 = r0.maxWgt.ToString() : _floorMin1 = r0.floorMin.ToString()
                _effDate1 = FormatDate(r0.effectiveDate)
                _canDate1 = FormatDate(r0.cancelDate)
            End If
            If it.itemHeader.discTable IsNot Nothing AndAlso it.itemHeader.discTable.Count > 1 Then
                Dim r1 As FXF3A.discountTable = DirectCast(it.itemHeader.discTable(1), FXF3A.discountTable)
                _disc2 = r1.disc.ToString() : _minChg2 = r1.minChargeDisc.ToString()
                _maxWgt2 = r1.maxWgt.ToString() : _floorMin2 = r1.floorMin.ToString()
                _effDate2 = FormatDate(r1.effectiveDate)
                _canDate2 = FormatDate(r1.cancelDate)
            End If
            If it.itemHeader.discTable IsNot Nothing AndAlso it.itemHeader.discTable.Count > 2 Then
                Dim r2 As FXF3A.discountTable = DirectCast(it.itemHeader.discTable(2), FXF3A.discountTable)
                _disc3 = r2.disc.ToString() : _minChg3 = r2.minChargeDisc.ToString()
                _maxWgt3 = r2.maxWgt.ToString() : _floorMin3 = r2.floorMin.ToString()
                _effDate3 = FormatDate(r2.effectiveDate)
                _canDate3 = FormatDate(r2.cancelDate)
            End If
            _currency = it.currency : _fsAuth = it.fsAuthority
            _fsNum = it.fsNumber : _fsItem = it.fsItem
            _prepdIn    = If(it.prepaidInbound,  "Y", "N") : _prepdOut = If(it.prepaidOutbound, "Y", "N")
            _collIn     = If(it.collectInbound,  "Y", "N") : _collOut  = If(it.collectOutbound, "Y", "N")
            _thirdParty = If(it.thirdParty,      "Y", "N")
            _inter    = [Enum].GetName(GetType(ScreenScraping.fxfInterEnum), it.inter)
            _typeHaul = [Enum].GetName(GetType(ScreenScraping.fxfTypeHaulEnum), it.typeHaul)
            _country = it.country : _matrix = it.matrix
            _ratesEff = FormatDate(it.ratesEffective)
            _srvDaysLo = it.servDaysLow : _srvDaysHi = it.servDaysHigh
            _lastMaintDate = FormatDate(it.lastMaintenanceDate)
            _operatorId = it.operatorId : _revision = it.revision
            Status = OperationStatus.Success
        End Sub

        ' ── Private helpers ──────────────────────────────────────────
        Private Function BuildDiscTable() As FXF3A.DiscCollection
            Dim dtc As New FXF3A.DiscCollection
            For Each tpl As Tuple(Of String, String, String, String, String, String) In New Tuple(Of String, String, String, String, String, String)() {
                    Tuple.Create(_disc1, _minChg1, _maxWgt1, _floorMin1, _effDate1, _canDate1),
                    Tuple.Create(_disc2, _minChg2, _maxWgt2, _floorMin2, _effDate2, _canDate2),
                    Tuple.Create(_disc3, _minChg3, _maxWgt3, _floorMin3, _effDate3, _canDate3)}
                Dim disc As String = tpl.Item1
                Dim eff As String = tpl.Item5
                ' Skip row if both disc and eff are empty, OR if eff date is missing
                ' (screen requires a valid effective date on every discount row)
                If String.IsNullOrWhiteSpace(disc) AndAlso String.IsNullOrWhiteSpace(eff) Then
                    Continue For
                End If
                If String.IsNullOrWhiteSpace(eff) Then
                    Continue For
                End If
                Dim row As New FXF3A.discountTable
                Dim dv As Single
                If Single.TryParse(disc, dv) Then row.disc = dv
                Dim mcv As Single
                If Single.TryParse(tpl.Item2, mcv) Then row.minChargeDisc = mcv
                Dim mwv As Integer
                If Integer.TryParse(tpl.Item3, mwv) Then row.maxWgt = mwv
                Dim fmv As Single
                If Single.TryParse(tpl.Item4, fmv) Then row.floorMin = fmv
                row.effectiveDate = ParseDate(eff)
                row.cancelDate    = ParseDate(tpl.Item6)
                dtc.Add(row)
            Next
            Return dtc
        End Function

        Private Shared Function ParseDate(s As String) As Date
            If String.IsNullOrWhiteSpace(s) Then Return ScreenScraping.NULL_DATE
            Dim d As Date
            Return If(Date.TryParse(s, d), d, ScreenScraping.NULL_DATE)
        End Function

        Private Shared Function FormatDate(d As Date) As String
            If d = ScreenScraping.NULL_DATE OrElse d = Date.MinValue Then Return ""
            Return d.ToString("MM/dd/yy")
        End Function

        Private Shared Function ParseEnum(Of T As Structure)(s As String,
                defaultVal As String) As T
            Dim target = If(String.IsNullOrWhiteSpace(s), defaultVal, s)
            Return DirectCast([Enum].Parse(GetType(T), target, True), T)
        End Function

    End Class

End Namespace
