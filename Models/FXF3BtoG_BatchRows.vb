Option Strict On
Option Explicit On
Option Infer On

Imports FedEx.PABST.SS.SSLib
Imports FedEx.PABST.SS.Screens

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
        Private _gt1R2Name As String = "" : Public Property GT1R2Name As String
            Get
                Return _gt1R2Name
            End Get
            Set(v As String)
                SetField(_gt1R2Name, v)
            End Set
        End Property
        Private _gt1R2Cty As String = "" : Public Property GT1R2Country As String
            Get
                Return _gt1R2Cty
            End Get
            Set(v As String)
                SetField(_gt1R2Cty, v)
            End Set
        End Property
        Private _gt1R3Name As String = "" : Public Property GT1R3Name As String
            Get
                Return _gt1R3Name
            End Get
            Set(v As String)
                SetField(_gt1R3Name, v)
            End Set
        End Property
        Private _gt1R3Cty As String = "" : Public Property GT1R3Country As String
            Get
                Return _gt1R3Cty
            End Get
            Set(v As String)
                SetField(_gt1R3Cty, v)
            End Set
        End Property
        Private _gt1R4Name As String = "" : Public Property GT1R4Name As String
            Get
                Return _gt1R4Name
            End Get
            Set(v As String)
                SetField(_gt1R4Name, v)
            End Set
        End Property
        Private _gt1R4Cty As String = "" : Public Property GT1R4Country As String
            Get
                Return _gt1R4Cty
            End Get
            Set(v As String)
                SetField(_gt1R4Cty, v)
            End Set
        End Property
        Private _gt1R5Name As String = "" : Public Property GT1R5Name As String
            Get
                Return _gt1R5Name
            End Get
            Set(v As String)
                SetField(_gt1R5Name, v)
            End Set
        End Property
        Private _gt1R5Cty As String = "" : Public Property GT1R5Country As String
            Get
                Return _gt1R5Cty
            End Get
            Set(v As String)
                SetField(_gt1R5Cty, v)
            End Set
        End Property

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
        ' geoTbl2 rows (up to 5)
        Private _gt2R1Name As String = "" : Public Property GT2R1Name As String
            Get
                Return _gt2R1Name
            End Get
            Set(v As String)
                SetField(_gt2R1Name, v)
            End Set
        End Property
        Private _gt2R1Cty As String = "" : Public Property GT2R1Country As String
            Get
                Return _gt2R1Cty
            End Get
            Set(v As String)
                SetField(_gt2R1Cty, v)
            End Set
        End Property
        Private _gt2R2Name As String = "" : Public Property GT2R2Name As String
            Get
                Return _gt2R2Name
            End Get
            Set(v As String)
                SetField(_gt2R2Name, v)
            End Set
        End Property
        Private _gt2R2Cty As String = "" : Public Property GT2R2Country As String
            Get
                Return _gt2R2Cty
            End Get
            Set(v As String)
                SetField(_gt2R2Cty, v)
            End Set
        End Property
        Private _gt2R3Name As String = "" : Public Property GT2R3Name As String
            Get
                Return _gt2R3Name
            End Get
            Set(v As String)
                SetField(_gt2R3Name, v)
            End Set
        End Property
        Private _gt2R3Cty As String = "" : Public Property GT2R3Country As String
            Get
                Return _gt2R3Cty
            End Get
            Set(v As String)
                SetField(_gt2R3Cty, v)
            End Set
        End Property
        Private _gt2R4Name As String = "" : Public Property GT2R4Name As String
            Get
                Return _gt2R4Name
            End Get
            Set(v As String)
                SetField(_gt2R4Name, v)
            End Set
        End Property
        Private _gt2R4Cty As String = "" : Public Property GT2R4Country As String
            Get
                Return _gt2R4Cty
            End Get
            Set(v As String)
                SetField(_gt2R4Cty, v)
            End Set
        End Property
        Private _gt2R5Name As String = "" : Public Property GT2R5Name As String
            Get
                Return _gt2R5Name
            End Get
            Set(v As String)
                SetField(_gt2R5Name, v)
            End Set
        End Property
        Private _gt2R5Cty As String = "" : Public Property GT2R5Country As String
            Get
                Return _gt2R5Cty
            End Get
            Set(v As String)
                SetField(_gt2R5Cty, v)
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
        Private _prepdIn As String  : Public Property PrepdIn As String
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
        Private _collIn As String   : Public Property CollIn As String
            Get
                Return _collIn
            End Get
            Set(v As String)
                SetField(_collIn, v)
            End Set
        End Property
        Private _collOut As String  : Public Property CollOut As String
            Get
                Return _collOut
            End Get
            Set(v As String)
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

        ' Discount table rows (up to 5) — the core data on the 3B screen
        Private _dt1Disc As String = ""
        Public Property DT1Disc As String
            Get
                Return _dt1Disc
            End Get
            Set(v As String)
                SetField(_dt1Disc, v)
            End Set
        End Property
        Private _dt1MinChg As String = ""
        Public Property DT1MinChg As String
            Get
                Return _dt1MinChg
            End Get
            Set(v As String)
                SetField(_dt1MinChg, v)
            End Set
        End Property
        Private _dt1MaxWgt As String = ""
        Public Property DT1MaxWgt As String
            Get
                Return _dt1MaxWgt
            End Get
            Set(v As String)
                SetField(_dt1MaxWgt, v)
            End Set
        End Property
        Private _dt1FloorMin As String = ""
        Public Property DT1FloorMin As String
            Get
                Return _dt1FloorMin
            End Get
            Set(v As String)
                SetField(_dt1FloorMin, v)
            End Set
        End Property
        Private _dt1EffDate As String = ""
        Public Property DT1EffDate As String
            Get
                Return _dt1EffDate
            End Get
            Set(v As String)
                SetField(_dt1EffDate, v)
            End Set
        End Property
        Private _dt1CanDate As String = ""
        Public Property DT1CanDate As String
            Get
                Return _dt1CanDate
            End Get
            Set(v As String)
                SetField(_dt1CanDate, v)
            End Set
        End Property

        Private _dt2Disc As String = ""
        Public Property DT2Disc As String
            Get
                Return _dt2Disc
            End Get
            Set(v As String)
                SetField(_dt2Disc, v)
            End Set
        End Property
        Private _dt2MinChg As String = ""
        Public Property DT2MinChg As String
            Get
                Return _dt2MinChg
            End Get
            Set(v As String)
                SetField(_dt2MinChg, v)
            End Set
        End Property
        Private _dt2MaxWgt As String = ""
        Public Property DT2MaxWgt As String
            Get
                Return _dt2MaxWgt
            End Get
            Set(v As String)
                SetField(_dt2MaxWgt, v)
            End Set
        End Property
        Private _dt2FloorMin As String = ""
        Public Property DT2FloorMin As String
            Get
                Return _dt2FloorMin
            End Get
            Set(v As String)
                SetField(_dt2FloorMin, v)
            End Set
        End Property
        Private _dt2EffDate As String = ""
        Public Property DT2EffDate As String
            Get
                Return _dt2EffDate
            End Get
            Set(v As String)
                SetField(_dt2EffDate, v)
            End Set
        End Property
        Private _dt2CanDate As String = ""
        Public Property DT2CanDate As String
            Get
                Return _dt2CanDate
            End Get
            Set(v As String)
                SetField(_dt2CanDate, v)
            End Set
        End Property

        Private _dt3Disc As String = ""
        Public Property DT3Disc As String
            Get
                Return _dt3Disc
            End Get
            Set(v As String)
                SetField(_dt3Disc, v)
            End Set
        End Property
        Private _dt3MinChg As String = ""
        Public Property DT3MinChg As String
            Get
                Return _dt3MinChg
            End Get
            Set(v As String)
                SetField(_dt3MinChg, v)
            End Set
        End Property
        Private _dt3MaxWgt As String = ""
        Public Property DT3MaxWgt As String
            Get
                Return _dt3MaxWgt
            End Get
            Set(v As String)
                SetField(_dt3MaxWgt, v)
            End Set
        End Property
        Private _dt3FloorMin As String = ""
        Public Property DT3FloorMin As String
            Get
                Return _dt3FloorMin
            End Get
            Set(v As String)
                SetField(_dt3FloorMin, v)
            End Set
        End Property
        Private _dt3EffDate As String = ""
        Public Property DT3EffDate As String
            Get
                Return _dt3EffDate
            End Get
            Set(v As String)
                SetField(_dt3EffDate, v)
            End Set
        End Property
        Private _dt3CanDate As String = ""
        Public Property DT3CanDate As String
            Get
                Return _dt3CanDate
            End Get
            Set(v As String)
                SetField(_dt3CanDate, v)
            End Set
        End Property

        Private _dt4Disc As String = ""
        Public Property DT4Disc As String
            Get
                Return _dt4Disc
            End Get
            Set(v As String)
                SetField(_dt4Disc, v)
            End Set
        End Property
        Private _dt4MinChg As String = ""
        Public Property DT4MinChg As String
            Get
                Return _dt4MinChg
            End Get
            Set(v As String)
                SetField(_dt4MinChg, v)
            End Set
        End Property
        Private _dt4MaxWgt As String = ""
        Public Property DT4MaxWgt As String
            Get
                Return _dt4MaxWgt
            End Get
            Set(v As String)
                SetField(_dt4MaxWgt, v)
            End Set
        End Property
        Private _dt4FloorMin As String = ""
        Public Property DT4FloorMin As String
            Get
                Return _dt4FloorMin
            End Get
            Set(v As String)
                SetField(_dt4FloorMin, v)
            End Set
        End Property
        Private _dt4EffDate As String = ""
        Public Property DT4EffDate As String
            Get
                Return _dt4EffDate
            End Get
            Set(v As String)
                SetField(_dt4EffDate, v)
            End Set
        End Property
        Private _dt4CanDate As String = ""
        Public Property DT4CanDate As String
            Get
                Return _dt4CanDate
            End Get
            Set(v As String)
                SetField(_dt4CanDate, v)
            End Set
        End Property

        Private _dt5Disc As String = ""
        Public Property DT5Disc As String
            Get
                Return _dt5Disc
            End Get
            Set(v As String)
                SetField(_dt5Disc, v)
            End Set
        End Property
        Private _dt5MinChg As String = ""
        Public Property DT5MinChg As String
            Get
                Return _dt5MinChg
            End Get
            Set(v As String)
                SetField(_dt5MinChg, v)
            End Set
        End Property
        Private _dt5MaxWgt As String = ""
        Public Property DT5MaxWgt As String
            Get
                Return _dt5MaxWgt
            End Get
            Set(v As String)
                SetField(_dt5MaxWgt, v)
            End Set
        End Property
        Private _dt5FloorMin As String = ""
        Public Property DT5FloorMin As String
            Get
                Return _dt5FloorMin
            End Get
            Set(v As String)
                SetField(_dt5FloorMin, v)
            End Set
        End Property
        Private _dt5EffDate As String = ""
        Public Property DT5EffDate As String
            Get
                Return _dt5EffDate
            End Get
            Set(v As String)
                SetField(_dt5EffDate, v)
            End Set
        End Property
        Private _dt5CanDate As String = ""
        Public Property DT5CanDate As String
            Get
                Return _dt5CanDate
            End Get
            Set(v As String)
                SetField(_dt5CanDate, v)
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

        ' ── Build FXF3B.itemClass from this row ──────────────────────
        Public Function ToItemClass() As FXF3B.itemClass
            Dim it As New FXF3B.itemClass
            it.itemHeader.auhority = Authority
            it.itemHeader.number   = Number
            it.itemHeader.item     = Item
            it.itemHeader.part     = Part

            ' geoTbl1
            Dim gt1 As New FXF3B.geoTableCollection()
            gt1.incExc = ParseEnum(Of ScreenScraping.fxfPlusMinusEnum)(GT1IncExc, "NA")
            gt1.dir    = ParseEnum(Of ScreenScraping.fxfGeoDirEnum)(GT1Dir, "NA")
            gt1.type   = ParseEnum(Of ScreenScraping.fxfGeoTypeEnum)(GT1Type, "NA")
            If Not String.IsNullOrWhiteSpace(GT1R1Name) Then
                Dim r As New FXF3B.geoTableRow() : r.name = GT1R1Name : r.country = GT1R1Country : gt1.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(GT1R2Name) Then
                Dim r As New FXF3B.geoTableRow() : r.name = GT1R2Name : r.country = GT1R2Country : gt1.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(GT1R3Name) Then
                Dim r As New FXF3B.geoTableRow() : r.name = GT1R3Name : r.country = GT1R3Country : gt1.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(GT1R4Name) Then
                Dim r As New FXF3B.geoTableRow() : r.name = GT1R4Name : r.country = GT1R4Country : gt1.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(GT1R5Name) Then
                Dim r As New FXF3B.geoTableRow() : r.name = GT1R5Name : r.country = GT1R5Country : gt1.Add(r)
            End If
            it.geoTbl1 = gt1

            ' geoTbl2
            Dim gt2 As New FXF3B.geoTableCollection()
            gt2.incExc = ParseEnum(Of ScreenScraping.fxfPlusMinusEnum)(GT2IncExc, "NA")
            gt2.dir    = ParseEnum(Of ScreenScraping.fxfGeoDirEnum)(GT2Dir, "NA")
            gt2.type   = ParseEnum(Of ScreenScraping.fxfGeoTypeEnum)(GT2Type, "NA")
            If Not String.IsNullOrWhiteSpace(GT2R1Name) Then
                Dim r As New FXF3B.geoTableRow() : r.name = GT2R1Name : r.country = GT2R1Country : gt2.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(GT2R2Name) Then
                Dim r As New FXF3B.geoTableRow() : r.name = GT2R2Name : r.country = GT2R2Country : gt2.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(GT2R3Name) Then
                Dim r As New FXF3B.geoTableRow() : r.name = GT2R3Name : r.country = GT2R3Country : gt2.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(GT2R4Name) Then
                Dim r As New FXF3B.geoTableRow() : r.name = GT2R4Name : r.country = GT2R4Country : gt2.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(GT2R5Name) Then
                Dim r As New FXF3B.geoTableRow() : r.name = GT2R5Name : r.country = GT2R5Country : gt2.Add(r)
            End If
            it.geoTbl2 = gt2

            it.fsAuthority     = FsAuth
            it.fsNumber        = FsNum
            it.fsItem          = FsItem
            it.prepaidInbound  = (PrepdIn  = "Y")
            it.prepaidOutbound = (PrepdOut = "Y")
            it.collectInbound  = (CollIn   = "Y")
            it.collectOutbound = (CollOut  = "Y")
            it.rateEff         = ParseDate(RateEff)
            it.classZip        = ParseEnum(Of ScreenScraping.fxfClassZipEnum)(ClsZip, "NA")
            it.genGeoAlt       = ParseEnum(Of ScreenScraping.fxfGenGeoAltrEnum)(GenGeoA, "NA")

            ' Build discount table rows — DT1..DT5 (only include non-empty rows)
            Dim dc As New FXF3B.DiscCollection()
            Dim dtDiscs()  As String = {DT1Disc,  DT2Disc,  DT3Disc,  DT4Disc,  DT5Disc}
            Dim dtMinChgs() As String = {DT1MinChg, DT2MinChg, DT3MinChg, DT4MinChg, DT5MinChg}
            Dim dtMaxWgts() As String = {DT1MaxWgt, DT2MaxWgt, DT3MaxWgt, DT4MaxWgt, DT5MaxWgt}
            Dim dtFloors() As String = {DT1FloorMin, DT2FloorMin, DT3FloorMin, DT4FloorMin, DT5FloorMin}
            Dim dtEffDates() As String = {DT1EffDate, DT2EffDate, DT3EffDate, DT4EffDate, DT5EffDate}
            Dim dtCanDates() As String = {DT1CanDate, DT2CanDate, DT3CanDate, DT4CanDate, DT5CanDate}
            For idx As Integer = 0 To 4
                If String.IsNullOrWhiteSpace(dtDiscs(idx)) Then Continue For
                Dim dtRow As New FXF3B.discountTable()
                Dim discVal As Single
                If Single.TryParse(dtDiscs(idx), discVal) Then dtRow.disc = discVal
                Dim minChgVal As Single
                If Single.TryParse(dtMinChgs(idx), minChgVal) Then dtRow.minChargeDisc = minChgVal
                Dim maxWgtVal As Integer
                If Integer.TryParse(dtMaxWgts(idx), maxWgtVal) Then dtRow.maxWgt = maxWgtVal
                Dim floorVal As Single
                If Single.TryParse(dtFloors(idx), floorVal) Then dtRow.floorMin = floorVal
                dtRow.effectiveDate = ParseDate(dtEffDates(idx))
                dtRow.cancelDate    = ParseDate(dtCanDates(idx))
                dc.Add(dtRow)
            Next
            If dc.Count > 0 Then it.itemHeader.discTable = dc

            Return it
        End Function

        ' ── Populate this row from a FXF3B.itemClass result ──────────
        Public Sub FromItemClass(it As FXF3B.itemClass)
            Authority = it.itemHeader.auhority
            Number    = it.itemHeader.number
            Item      = it.itemHeader.item
            Part      = it.itemHeader.part

            If it.geoTbl1 IsNot Nothing Then
                GT1IncExc = GetEnumName(Of ScreenScraping.fxfPlusMinusEnum)(it.geoTbl1.incExc)
                GT1Dir    = GetEnumName(Of ScreenScraping.fxfGeoDirEnum)(it.geoTbl1.dir)
                GT1Type   = GetEnumName(Of ScreenScraping.fxfGeoTypeEnum)(it.geoTbl1.type)
                If it.geoTbl1.Count > 0 Then
                    Dim r0 = DirectCast(it.geoTbl1(0), FXF3B.geoTableRow) : GT1R1Name = r0.name : GT1R1Country = r0.country
                End If
                If it.geoTbl1.Count > 1 Then
                    Dim r1 = DirectCast(it.geoTbl1(1), FXF3B.geoTableRow) : GT1R2Name = r1.name : GT1R2Country = r1.country
                End If
                If it.geoTbl1.Count > 2 Then
                    Dim r2 = DirectCast(it.geoTbl1(2), FXF3B.geoTableRow) : GT1R3Name = r2.name : GT1R3Country = r2.country
                End If
                If it.geoTbl1.Count > 3 Then
                    Dim r3 = DirectCast(it.geoTbl1(3), FXF3B.geoTableRow) : GT1R4Name = r3.name : GT1R4Country = r3.country
                End If
                If it.geoTbl1.Count > 4 Then
                    Dim r4 = DirectCast(it.geoTbl1(4), FXF3B.geoTableRow) : GT1R5Name = r4.name : GT1R5Country = r4.country
                End If
            End If

            If it.geoTbl2 IsNot Nothing Then
                GT2IncExc = GetEnumName(Of ScreenScraping.fxfPlusMinusEnum)(it.geoTbl2.incExc)
                GT2Dir    = GetEnumName(Of ScreenScraping.fxfGeoDirEnum)(it.geoTbl2.dir)
                GT2Type   = GetEnumName(Of ScreenScraping.fxfGeoTypeEnum)(it.geoTbl2.type)
                If it.geoTbl2.Count > 0 Then
                    Dim r0 = DirectCast(it.geoTbl2(0), FXF3B.geoTableRow) : GT2R1Name = r0.name : GT2R1Country = r0.country
                End If
                If it.geoTbl2.Count > 1 Then
                    Dim r1 = DirectCast(it.geoTbl2(1), FXF3B.geoTableRow) : GT2R2Name = r1.name : GT2R2Country = r1.country
                End If
                If it.geoTbl2.Count > 2 Then
                    Dim r2 = DirectCast(it.geoTbl2(2), FXF3B.geoTableRow) : GT2R3Name = r2.name : GT2R3Country = r2.country
                End If
                If it.geoTbl2.Count > 3 Then
                    Dim r3 = DirectCast(it.geoTbl2(3), FXF3B.geoTableRow) : GT2R4Name = r3.name : GT2R4Country = r3.country
                End If
                If it.geoTbl2.Count > 4 Then
                    Dim r4 = DirectCast(it.geoTbl2(4), FXF3B.geoTableRow) : GT2R5Name = r4.name : GT2R5Country = r4.country
                End If
            End If

            FsAuth  = it.fsAuthority
            FsNum   = it.fsNumber
            FsItem  = it.fsItem
            PrepdIn  = If(it.prepaidInbound,  "Y", "N")
            PrepdOut = If(it.prepaidOutbound, "Y", "N")
            CollIn   = If(it.collectInbound,  "Y", "N")
            CollOut  = If(it.collectOutbound, "Y", "N")
            RateEff  = FormatDate(it.rateEff)
            ClsZip   = GetEnumName(Of ScreenScraping.fxfClassZipEnum)(it.classZip)
            GenGeoA  = GetEnumName(Of ScreenScraping.fxfGenGeoAltrEnum)(it.genGeoAlt)

            ' Read discount table rows
            If it.itemHeader.discTable IsNot Nothing Then
                For dtIdx As Integer = 0 To Math.Min(it.itemHeader.discTable.Count - 1, 4)
                    Dim dtRow As FXF3B.discountTable = it.itemHeader.discTable(dtIdx)
                    Select Case dtIdx
                        Case 0 : DT1Disc = dtRow.disc.ToString() : DT1MinChg = dtRow.minChargeDisc.ToString() : DT1MaxWgt = If(dtRow.maxWgt = ScreenScraping.NULL_INT, "", dtRow.maxWgt.ToString()) : DT1FloorMin = dtRow.floorMin.ToString() : DT1EffDate = FormatDate(dtRow.effectiveDate) : DT1CanDate = FormatDate(dtRow.cancelDate)
                        Case 1 : DT2Disc = dtRow.disc.ToString() : DT2MinChg = dtRow.minChargeDisc.ToString() : DT2MaxWgt = If(dtRow.maxWgt = ScreenScraping.NULL_INT, "", dtRow.maxWgt.ToString()) : DT2FloorMin = dtRow.floorMin.ToString() : DT2EffDate = FormatDate(dtRow.effectiveDate) : DT2CanDate = FormatDate(dtRow.cancelDate)
                        Case 2 : DT3Disc = dtRow.disc.ToString() : DT3MinChg = dtRow.minChargeDisc.ToString() : DT3MaxWgt = If(dtRow.maxWgt = ScreenScraping.NULL_INT, "", dtRow.maxWgt.ToString()) : DT3FloorMin = dtRow.floorMin.ToString() : DT3EffDate = FormatDate(dtRow.effectiveDate) : DT3CanDate = FormatDate(dtRow.cancelDate)
                        Case 3 : DT4Disc = dtRow.disc.ToString() : DT4MinChg = dtRow.minChargeDisc.ToString() : DT4MaxWgt = If(dtRow.maxWgt = ScreenScraping.NULL_INT, "", dtRow.maxWgt.ToString()) : DT4FloorMin = dtRow.floorMin.ToString() : DT4EffDate = FormatDate(dtRow.effectiveDate) : DT4CanDate = FormatDate(dtRow.cancelDate)
                        Case 4 : DT5Disc = dtRow.disc.ToString() : DT5MinChg = dtRow.minChargeDisc.ToString() : DT5MaxWgt = If(dtRow.maxWgt = ScreenScraping.NULL_INT, "", dtRow.maxWgt.ToString()) : DT5FloorMin = dtRow.floorMin.ToString() : DT5EffDate = FormatDate(dtRow.effectiveDate) : DT5CanDate = FormatDate(dtRow.cancelDate)
                    End Select
                Next
            End If

            LastMaintDate = FormatDate(it.lastMaintenanceDate)
            OperatorId = it.operatorId
            Revision   = it.revision
            Status     = OperationStatus.Success
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
        ' Row 2
        Private _r2Pm As String = "NA"   : Public Property R2PlusMinus As String
            Get
                Return _r2Pm
            End Get
            Set(v As String)
                SetField(_r2Pm, v)
            End Set
        End Property
        Private _r2Dir As String = "NA"  : Public Property R2Dir As String
            Get
                Return _r2Dir
            End Get
            Set(v As String)
                SetField(_r2Dir, v)
            End Set
        End Property
        Private _r2Type As String = "NA" : Public Property R2Type As String
            Get
                Return _r2Type
            End Get
            Set(v As String)
                SetField(_r2Type, v)
            End Set
        End Property
        Private _r2Name As String = ""   : Public Property R2Name As String
            Get
                Return _r2Name
            End Get
            Set(v As String)
                SetField(_r2Name, v)
            End Set
        End Property
        Private _r2State As String = ""  : Public Property R2State As String
            Get
                Return _r2State
            End Get
            Set(v As String)
                SetField(_r2State, v)
            End Set
        End Property
        Private _r2Cty As String = ""    : Public Property R2Country As String
            Get
                Return _r2Cty
            End Get
            Set(v As String)
                SetField(_r2Cty, v)
            End Set
        End Property
        ' Row 3
        Private _r3Pm As String = "NA"   : Public Property R3PlusMinus As String
            Get
                Return _r3Pm
            End Get
            Set(v As String)
                SetField(_r3Pm, v)
            End Set
        End Property
        Private _r3Dir As String = "NA"  : Public Property R3Dir As String
            Get
                Return _r3Dir
            End Get
            Set(v As String)
                SetField(_r3Dir, v)
            End Set
        End Property
        Private _r3Type As String = "NA" : Public Property R3Type As String
            Get
                Return _r3Type
            End Get
            Set(v As String)
                SetField(_r3Type, v)
            End Set
        End Property
        Private _r3Name As String = ""   : Public Property R3Name As String
            Get
                Return _r3Name
            End Get
            Set(v As String)
                SetField(_r3Name, v)
            End Set
        End Property
        Private _r3State As String = ""  : Public Property R3State As String
            Get
                Return _r3State
            End Get
            Set(v As String)
                SetField(_r3State, v)
            End Set
        End Property
        Private _r3Cty As String = ""    : Public Property R3Country As String
            Get
                Return _r3Cty
            End Get
            Set(v As String)
                SetField(_r3Cty, v)
            End Set
        End Property
        ' Row 4
        Private _r4Pm As String = "NA"   : Public Property R4PlusMinus As String
            Get
                Return _r4Pm
            End Get
            Set(v As String)
                SetField(_r4Pm, v)
            End Set
        End Property
        Private _r4Dir As String = "NA"  : Public Property R4Dir As String
            Get
                Return _r4Dir
            End Get
            Set(v As String)
                SetField(_r4Dir, v)
            End Set
        End Property
        Private _r4Type As String = "NA" : Public Property R4Type As String
            Get
                Return _r4Type
            End Get
            Set(v As String)
                SetField(_r4Type, v)
            End Set
        End Property
        Private _r4Name As String = ""   : Public Property R4Name As String
            Get
                Return _r4Name
            End Get
            Set(v As String)
                SetField(_r4Name, v)
            End Set
        End Property
        Private _r4State As String = ""  : Public Property R4State As String
            Get
                Return _r4State
            End Get
            Set(v As String)
                SetField(_r4State, v)
            End Set
        End Property
        Private _r4Cty As String = ""    : Public Property R4Country As String
            Get
                Return _r4Cty
            End Get
            Set(v As String)
                SetField(_r4Cty, v)
            End Set
        End Property
        ' Row 5
        Private _r5Pm As String = "NA"   : Public Property R5PlusMinus As String
            Get
                Return _r5Pm
            End Get
            Set(v As String)
                SetField(_r5Pm, v)
            End Set
        End Property
        Private _r5Dir As String = "NA"  : Public Property R5Dir As String
            Get
                Return _r5Dir
            End Get
            Set(v As String)
                SetField(_r5Dir, v)
            End Set
        End Property
        Private _r5Type As String = "NA" : Public Property R5Type As String
            Get
                Return _r5Type
            End Get
            Set(v As String)
                SetField(_r5Type, v)
            End Set
        End Property
        Private _r5Name As String = ""   : Public Property R5Name As String
            Get
                Return _r5Name
            End Get
            Set(v As String)
                SetField(_r5Name, v)
            End Set
        End Property
        Private _r5State As String = ""  : Public Property R5State As String
            Get
                Return _r5State
            End Get
            Set(v As String)
                SetField(_r5State, v)
            End Set
        End Property
        Private _r5Cty As String = ""    : Public Property R5Country As String
            Get
                Return _r5Cty
            End Get
            Set(v As String)
                SetField(_r5Cty, v)
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

        ' ── Build FXF3C.itemClass from this row ──────────────────────
        Public Function ToItemClass() As FXF3C.itemClass
            Dim it As New FXF3C.itemClass
            it.itemHeader.auhority = Authority
            it.itemHeader.number   = Number
            it.itemHeader.item     = Item
            it.itemHeader.part     = Part

            Dim gt As New FXF3C.geoTableCollection()
            If Not String.IsNullOrWhiteSpace(R1Name) Then
                Dim r As New FXF3C.geoTableRow()
                r.plusMinus = ParseEnum(Of ScreenScraping.fxfPlusMinusEnum)(R1PlusMinus, "NA")
                r.dir       = ParseEnum(Of ScreenScraping.fxfGeoDirEnum)(R1Dir, "NA")
                r.type      = ParseEnum(Of ScreenScraping.fxfGeoTypeEnum)(R1Type, "NA")
                r.name = R1Name : r.state = R1State : r.country = R1Country
                gt.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(R2Name) Then
                Dim r As New FXF3C.geoTableRow()
                r.plusMinus = ParseEnum(Of ScreenScraping.fxfPlusMinusEnum)(R2PlusMinus, "NA")
                r.dir       = ParseEnum(Of ScreenScraping.fxfGeoDirEnum)(R2Dir, "NA")
                r.type      = ParseEnum(Of ScreenScraping.fxfGeoTypeEnum)(R2Type, "NA")
                r.name = R2Name : r.state = R2State : r.country = R2Country
                gt.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(R3Name) Then
                Dim r As New FXF3C.geoTableRow()
                r.plusMinus = ParseEnum(Of ScreenScraping.fxfPlusMinusEnum)(R3PlusMinus, "NA")
                r.dir       = ParseEnum(Of ScreenScraping.fxfGeoDirEnum)(R3Dir, "NA")
                r.type      = ParseEnum(Of ScreenScraping.fxfGeoTypeEnum)(R3Type, "NA")
                r.name = R3Name : r.state = R3State : r.country = R3Country
                gt.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(R4Name) Then
                Dim r As New FXF3C.geoTableRow()
                r.plusMinus = ParseEnum(Of ScreenScraping.fxfPlusMinusEnum)(R4PlusMinus, "NA")
                r.dir       = ParseEnum(Of ScreenScraping.fxfGeoDirEnum)(R4Dir, "NA")
                r.type      = ParseEnum(Of ScreenScraping.fxfGeoTypeEnum)(R4Type, "NA")
                r.name = R4Name : r.state = R4State : r.country = R4Country
                gt.Add(r)
            End If
            If Not String.IsNullOrWhiteSpace(R5Name) Then
                Dim r As New FXF3C.geoTableRow()
                r.plusMinus = ParseEnum(Of ScreenScraping.fxfPlusMinusEnum)(R5PlusMinus, "NA")
                r.dir       = ParseEnum(Of ScreenScraping.fxfGeoDirEnum)(R5Dir, "NA")
                r.type      = ParseEnum(Of ScreenScraping.fxfGeoTypeEnum)(R5Type, "NA")
                r.name = R5Name : r.state = R5State : r.country = R5Country
                gt.Add(r)
            End If
            it.itemHeader.geoTable = gt

            Dim loV As Integer
            it.servDaysLow  = If(Integer.TryParse(SrvDaysLo, loV), loV, 0)
            Dim hiV As Integer
            it.servDaysHigh = If(Integer.TryParse(SrvDaysHi, hiV), hiV, 0)
            Return it
        End Function

        ' ── Populate this row from a FXF3C.itemClass result ──────────
        Public Sub FromItemClass(it As FXF3C.itemClass)
            Authority = it.itemHeader.auhority
            Number    = it.itemHeader.number
            Item      = it.itemHeader.item
            Part      = it.itemHeader.part

            If it.itemHeader.geoTable IsNot Nothing Then
                Dim idx As Integer = 0
                For Each row As FXF3C.geoTableRow In it.itemHeader.geoTable
                    Select Case idx
                        Case 0
                            R1PlusMinus = GetEnumName(Of ScreenScraping.fxfPlusMinusEnum)(row.plusMinus)
                            R1Dir       = GetEnumName(Of ScreenScraping.fxfGeoDirEnum)(row.dir)
                            R1Type      = GetEnumName(Of ScreenScraping.fxfGeoTypeEnum)(row.type)
                            R1Name = row.name : R1State = row.state : R1Country = row.country
                        Case 1
                            R2PlusMinus = GetEnumName(Of ScreenScraping.fxfPlusMinusEnum)(row.plusMinus)
                            R2Dir       = GetEnumName(Of ScreenScraping.fxfGeoDirEnum)(row.dir)
                            R2Type      = GetEnumName(Of ScreenScraping.fxfGeoTypeEnum)(row.type)
                            R2Name = row.name : R2State = row.state : R2Country = row.country
                        Case 2
                            R3PlusMinus = GetEnumName(Of ScreenScraping.fxfPlusMinusEnum)(row.plusMinus)
                            R3Dir       = GetEnumName(Of ScreenScraping.fxfGeoDirEnum)(row.dir)
                            R3Type      = GetEnumName(Of ScreenScraping.fxfGeoTypeEnum)(row.type)
                            R3Name = row.name : R3State = row.state : R3Country = row.country
                        Case 3
                            R4PlusMinus = GetEnumName(Of ScreenScraping.fxfPlusMinusEnum)(row.plusMinus)
                            R4Dir       = GetEnumName(Of ScreenScraping.fxfGeoDirEnum)(row.dir)
                            R4Type      = GetEnumName(Of ScreenScraping.fxfGeoTypeEnum)(row.type)
                            R4Name = row.name : R4State = row.state : R4Country = row.country
                        Case 4
                            R5PlusMinus = GetEnumName(Of ScreenScraping.fxfPlusMinusEnum)(row.plusMinus)
                            R5Dir       = GetEnumName(Of ScreenScraping.fxfGeoDirEnum)(row.dir)
                            R5Type      = GetEnumName(Of ScreenScraping.fxfGeoTypeEnum)(row.type)
                            R5Name = row.name : R5State = row.state : R5Country = row.country
                    End Select
                    idx += 1
                Next
            End If

            SrvDaysLo = it.servDaysLow.ToString()
            SrvDaysHi = it.servDaysHigh.ToString()
            LastMaintDate = FormatDate(it.lastMaintenanceDate)
            OperatorId = it.operatorId
            Revision   = it.revision
            Status     = OperationStatus.Success
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
        ' Row 1
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
        ' Row 2
        Private _p2Type As String = "NA" : Public Property P2Type As String
            Get
                Return _p2Type
            End Get
            Set(v As String)
                SetField(_p2Type, v)
            End Set
        End Property
        Private _p2Prod1 As String = "" : Public Property P2Prod1 As String
            Get
                Return _p2Prod1
            End Get
            Set(v As String)
                SetField(_p2Prod1, v)
            End Set
        End Property
        Private _p2Prod2 As String = "" : Public Property P2Prod2 As String
            Get
                Return _p2Prod2
            End Get
            Set(v As String)
                SetField(_p2Prod2, v)
            End Set
        End Property
        Private _p2ExcCls As String = "" : Public Property P2ExcCls As String
            Get
                Return _p2ExcCls
            End Get
            Set(v As String)
                SetField(_p2ExcCls, v)
            End Set
        End Property
        ' Row 3
        Private _p3Type As String = "NA" : Public Property P3Type As String
            Get
                Return _p3Type
            End Get
            Set(v As String)
                SetField(_p3Type, v)
            End Set
        End Property
        Private _p3Prod1 As String = "" : Public Property P3Prod1 As String
            Get
                Return _p3Prod1
            End Get
            Set(v As String)
                SetField(_p3Prod1, v)
            End Set
        End Property
        Private _p3Prod2 As String = "" : Public Property P3Prod2 As String
            Get
                Return _p3Prod2
            End Get
            Set(v As String)
                SetField(_p3Prod2, v)
            End Set
        End Property
        Private _p3ExcCls As String = "" : Public Property P3ExcCls As String
            Get
                Return _p3ExcCls
            End Get
            Set(v As String)
                SetField(_p3ExcCls, v)
            End Set
        End Property
        ' Row 4
        Private _p4Type As String = "NA" : Public Property P4Type As String
            Get
                Return _p4Type
            End Get
            Set(v As String)
                SetField(_p4Type, v)
            End Set
        End Property
        Private _p4Prod1 As String = "" : Public Property P4Prod1 As String
            Get
                Return _p4Prod1
            End Get
            Set(v As String)
                SetField(_p4Prod1, v)
            End Set
        End Property
        Private _p4Prod2 As String = "" : Public Property P4Prod2 As String
            Get
                Return _p4Prod2
            End Get
            Set(v As String)
                SetField(_p4Prod2, v)
            End Set
        End Property
        Private _p4ExcCls As String = "" : Public Property P4ExcCls As String
            Get
                Return _p4ExcCls
            End Get
            Set(v As String)
                SetField(_p4ExcCls, v)
            End Set
        End Property
        ' Row 5
        Private _p5Type As String = "NA" : Public Property P5Type As String
            Get
                Return _p5Type
            End Get
            Set(v As String)
                SetField(_p5Type, v)
            End Set
        End Property
        Private _p5Prod1 As String = "" : Public Property P5Prod1 As String
            Get
                Return _p5Prod1
            End Get
            Set(v As String)
                SetField(_p5Prod1, v)
            End Set
        End Property
        Private _p5Prod2 As String = "" : Public Property P5Prod2 As String
            Get
                Return _p5Prod2
            End Get
            Set(v As String)
                SetField(_p5Prod2, v)
            End Set
        End Property
        Private _p5ExcCls As String = "" : Public Property P5ExcCls As String
            Get
                Return _p5ExcCls
            End Get
            Set(v As String)
                SetField(_p5ExcCls, v)
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

        ' ── Build FXF3D.itemClass from this row ──────────────────────
        Public Function ToItemClass() As FXF3D.itemClass
            Dim it As New FXF3D.itemClass
            it.itemHeader.auhority = Authority
            it.itemHeader.number   = Number
            it.itemHeader.item     = Item
            it.itemHeader.part     = Part
            it.effectiveDate = ParseDate(EffDate)
            it.cancelDate    = ParseDate(CanDateItem)
            it.excClass      = ExcCls
            it.excClassMaxWgt = ExcMaxW

            Dim pt As New FXF3D.prodTableCollection()
            Dim addProd As Action(Of String, String, String, String) =
                Sub(t As String, p1 As String, p2 As String, ec As String)
                    If String.IsNullOrWhiteSpace(t) AndAlso String.IsNullOrWhiteSpace(p1) Then Return
                    Dim row As New FXF3D.prodTableRow()
                    Dim ptype As FXF3D.accountProdType
                    If [Enum].TryParse(Of FXF3D.accountProdType)(t, True, ptype) Then row.type = ptype
                    row.product1 = p1 : row.product2 = p2 : row.excCls = ec
                    pt.Add(row)
                End Sub
            addProd(P1Type, P1Prod1, P1Prod2, P1ExcCls)
            addProd(P2Type, P2Prod1, P2Prod2, P2ExcCls)
            addProd(P3Type, P3Prod1, P3Prod2, P3ExcCls)
            addProd(P4Type, P4Prod1, P4Prod2, P4ExcCls)
            addProd(P5Type, P5Prod1, P5Prod2, P5ExcCls)
            it.prodTable = pt
            Return it
        End Function

        ' ── Populate this row from a FXF3D.itemClass result ──────────
        Public Sub FromItemClass(it As FXF3D.itemClass)
            Authority = it.itemHeader.auhority
            Number    = it.itemHeader.number
            Item      = it.itemHeader.item
            Part      = it.itemHeader.part
            EffDate      = FormatDate(it.effectiveDate)
            CanDateItem  = FormatDate(it.cancelDate)
            ExcCls       = it.excClass
            ExcMaxW      = it.excClassMaxWgt

            If it.prodTable IsNot Nothing Then
                Dim idx As Integer = 0
                For Each r As FXF3D.prodTableRow In it.prodTable
                    Dim tName As String = [Enum].GetName(GetType(FXF3D.accountProdType), r.type)
                    Select Case idx
                        Case 0 : P1Type = tName : P1Prod1 = r.product1 : P1Prod2 = r.product2 : P1ExcCls = r.excCls
                        Case 1 : P2Type = tName : P2Prod1 = r.product1 : P2Prod2 = r.product2 : P2ExcCls = r.excCls
                        Case 2 : P3Type = tName : P3Prod1 = r.product1 : P3Prod2 = r.product2 : P3ExcCls = r.excCls
                        Case 3 : P4Type = tName : P4Prod1 = r.product1 : P4Prod2 = r.product2 : P4ExcCls = r.excCls
                        Case 4 : P5Type = tName : P5Prod1 = r.product1 : P5Prod2 = r.product2 : P5ExcCls = r.excCls
                    End Select
                    idx += 1
                Next
            End If

            LastMaintDate = FormatDate(it.lastMaintenanceDate)
            OperatorId = it.operatorId
            Revision   = it.revision
            Status     = OperationStatus.Success
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
        Private _rateManually As String = "N" : Public Property RateManually As String
            Get
                Return _rateManually
            End Get
            Set(v As String)
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

        ' rateTable rows (up to 10): weight, type (fxfRateTypeEnum), amount
        ' Row 1
        Private _rt1Wgt As String = ""    : Public Property RT1Wgt As String
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
        Private _rt1Amt As String = ""    : Public Property RT1Amt As String
            Get
                Return _rt1Amt
            End Get
            Set(v As String)
                SetField(_rt1Amt, v)
            End Set
        End Property
        ' Row 2
        Private _rt2Wgt As String = ""    : Public Property RT2Wgt As String
            Get
                Return _rt2Wgt
            End Get
            Set(v As String)
                SetField(_rt2Wgt, v)
            End Set
        End Property
        Private _rt2Type As String = "NA" : Public Property RT2Type As String
            Get
                Return _rt2Type
            End Get
            Set(v As String)
                SetField(_rt2Type, v)
            End Set
        End Property
        Private _rt2Amt As String = ""    : Public Property RT2Amt As String
            Get
                Return _rt2Amt
            End Get
            Set(v As String)
                SetField(_rt2Amt, v)
            End Set
        End Property
        ' Row 3
        Private _rt3Wgt As String = ""    : Public Property RT3Wgt As String
            Get
                Return _rt3Wgt
            End Get
            Set(v As String)
                SetField(_rt3Wgt, v)
            End Set
        End Property
        Private _rt3Type As String = "NA" : Public Property RT3Type As String
            Get
                Return _rt3Type
            End Get
            Set(v As String)
                SetField(_rt3Type, v)
            End Set
        End Property
        Private _rt3Amt As String = ""    : Public Property RT3Amt As String
            Get
                Return _rt3Amt
            End Get
            Set(v As String)
                SetField(_rt3Amt, v)
            End Set
        End Property
        ' Row 4
        Private _rt4Wgt As String = ""    : Public Property RT4Wgt As String
            Get
                Return _rt4Wgt
            End Get
            Set(v As String)
                SetField(_rt4Wgt, v)
            End Set
        End Property
        Private _rt4Type As String = "NA" : Public Property RT4Type As String
            Get
                Return _rt4Type
            End Get
            Set(v As String)
                SetField(_rt4Type, v)
            End Set
        End Property
        Private _rt4Amt As String = ""    : Public Property RT4Amt As String
            Get
                Return _rt4Amt
            End Get
            Set(v As String)
                SetField(_rt4Amt, v)
            End Set
        End Property
        ' Row 5
        Private _rt5Wgt As String = ""    : Public Property RT5Wgt As String
            Get
                Return _rt5Wgt
            End Get
            Set(v As String)
                SetField(_rt5Wgt, v)
            End Set
        End Property
        Private _rt5Type As String = "NA" : Public Property RT5Type As String
            Get
                Return _rt5Type
            End Get
            Set(v As String)
                SetField(_rt5Type, v)
            End Set
        End Property
        Private _rt5Amt As String = ""    : Public Property RT5Amt As String
            Get
                Return _rt5Amt
            End Get
            Set(v As String)
                SetField(_rt5Amt, v)
            End Set
        End Property
        ' Row 6
        Private _rt6Wgt As String = ""    : Public Property RT6Wgt As String
            Get
                Return _rt6Wgt
            End Get
            Set(v As String)
                SetField(_rt6Wgt, v)
            End Set
        End Property
        Private _rt6Type As String = "NA" : Public Property RT6Type As String
            Get
                Return _rt6Type
            End Get
            Set(v As String)
                SetField(_rt6Type, v)
            End Set
        End Property
        Private _rt6Amt As String = ""    : Public Property RT6Amt As String
            Get
                Return _rt6Amt
            End Get
            Set(v As String)
                SetField(_rt6Amt, v)
            End Set
        End Property
        ' Row 7
        Private _rt7Wgt As String = ""    : Public Property RT7Wgt As String
            Get
                Return _rt7Wgt
            End Get
            Set(v As String)
                SetField(_rt7Wgt, v)
            End Set
        End Property
        Private _rt7Type As String = "NA" : Public Property RT7Type As String
            Get
                Return _rt7Type
            End Get
            Set(v As String)
                SetField(_rt7Type, v)
            End Set
        End Property
        Private _rt7Amt As String = ""    : Public Property RT7Amt As String
            Get
                Return _rt7Amt
            End Get
            Set(v As String)
                SetField(_rt7Amt, v)
            End Set
        End Property
        ' Row 8
        Private _rt8Wgt As String = ""    : Public Property RT8Wgt As String
            Get
                Return _rt8Wgt
            End Get
            Set(v As String)
                SetField(_rt8Wgt, v)
            End Set
        End Property
        Private _rt8Type As String = "NA" : Public Property RT8Type As String
            Get
                Return _rt8Type
            End Get
            Set(v As String)
                SetField(_rt8Type, v)
            End Set
        End Property
        Private _rt8Amt As String = ""    : Public Property RT8Amt As String
            Get
                Return _rt8Amt
            End Get
            Set(v As String)
                SetField(_rt8Amt, v)
            End Set
        End Property
        ' Row 9
        Private _rt9Wgt As String = ""    : Public Property RT9Wgt As String
            Get
                Return _rt9Wgt
            End Get
            Set(v As String)
                SetField(_rt9Wgt, v)
            End Set
        End Property
        Private _rt9Type As String = "NA" : Public Property RT9Type As String
            Get
                Return _rt9Type
            End Get
            Set(v As String)
                SetField(_rt9Type, v)
            End Set
        End Property
        Private _rt9Amt As String = ""    : Public Property RT9Amt As String
            Get
                Return _rt9Amt
            End Get
            Set(v As String)
                SetField(_rt9Amt, v)
            End Set
        End Property
        ' Row 10
        Private _rt10Wgt As String = ""    : Public Property RT10Wgt As String
            Get
                Return _rt10Wgt
            End Get
            Set(v As String)
                SetField(_rt10Wgt, v)
            End Set
        End Property
        Private _rt10Type As String = "NA" : Public Property RT10Type As String
            Get
                Return _rt10Type
            End Get
            Set(v As String)
                SetField(_rt10Type, v)
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

        ' ── Build FXF3E.itemClass from this row ──────────────────────
        Public Function ToItemClass() As FXF3E.itemClass
            Dim it As New FXF3E.itemClass
            it.itemHeader.auhority = Authority
            it.itemHeader.number   = Number
            it.itemHeader.item     = Item
            it.itemHeader.part     = Part
            it.condition        = Condition
            it.prepaidOrCollect = ParseEnum(Of ScreenScraping.fxfPrepaidOrCollectEnum)(PrepdOrCollect, "NA")
            it.effectiveDate    = ParseDate(EffDate)
            it.cancelDate       = ParseDate(CanDateItem)
            it.comments         = Comments
            it.alternation      = ParseEnum(Of ScreenScraping.fxfAlternationEnum)(Alternation, "NA")
            it.classRates       = ParseEnum(Of ScreenScraping.fxfClassZipEnum)(ClassRates, "NA")
            it.rateManually     = (RateManually = "Y")
            it.clsTrfAuthority  = ClsTrfAuth
            it.clsTrfNumber     = ClsTrfNum
            it.clsTrfSection    = ClsTrfSec
            it.rateEffDate      = ParseDate(RateEffDate)

            Dim rt As New FXF3E.rateTableCollection()
            Dim addRate As Action(Of String, String, String) =
                Sub(wgtS As String, typeS As String, amtS As String)
                    Dim wv As Integer
                    If Not Integer.TryParse(wgtS, wv) OrElse wv <= 0 Then Return
                    Dim row As New FXF3E.rateTableRow()
                    row.weight = wv
                    Dim rtype As FXF3E.fxfRateTypeEnum
                    If [Enum].TryParse(Of FXF3E.fxfRateTypeEnum)(typeS, True, rtype) Then row.type = rtype
                    Dim av As Single
                    If Single.TryParse(amtS, av) Then row.amount = av
                    rt.Add(row)
                End Sub
            addRate(RT1Wgt, RT1Type, RT1Amt)
            addRate(RT2Wgt, RT2Type, RT2Amt)
            addRate(RT3Wgt, RT3Type, RT3Amt)
            addRate(RT4Wgt, RT4Type, RT4Amt)
            addRate(RT5Wgt, RT5Type, RT5Amt)
            addRate(RT6Wgt, RT6Type, RT6Amt)
            addRate(RT7Wgt, RT7Type, RT7Amt)
            addRate(RT8Wgt, RT8Type, RT8Amt)
            addRate(RT9Wgt, RT9Type, RT9Amt)
            addRate(RT10Wgt, RT10Type, RT10Amt)
            it.rateTable = rt
            Return it
        End Function

        ' ── Populate this row from a FXF3E.itemClass result ──────────
        Public Sub FromItemClass(it As FXF3E.itemClass)
            Authority = it.itemHeader.auhority
            Number    = it.itemHeader.number
            Item      = it.itemHeader.item
            Part      = it.itemHeader.part
            Condition      = it.condition
            PrepdOrCollect = GetEnumName(Of ScreenScraping.fxfPrepaidOrCollectEnum)(it.prepaidOrCollect)
            EffDate        = FormatDate(it.effectiveDate)
            CanDateItem    = FormatDate(it.cancelDate)
            Comments       = it.comments
            Alternation    = GetEnumName(Of ScreenScraping.fxfAlternationEnum)(it.alternation)
            ClassRates     = GetEnumName(Of ScreenScraping.fxfClassZipEnum)(it.classRates)
            RateManually   = If(it.rateManually, "Y", "N")
            ClsTrfAuth     = it.clsTrfAuthority
            ClsTrfNum      = it.clsTrfNumber
            ClsTrfSec      = it.clsTrfSection
            RateEffDate    = FormatDate(it.rateEffDate)

            If it.rateTable IsNot Nothing Then
                Dim idx As Integer = 0
                For Each r As FXF3E.rateTableRow In it.rateTable
                    If idx >= 10 Then Exit For
                    Dim wgt As String = r.weight.ToString()
                    Dim typ As String = GetEnumName(Of FXF3E.fxfRateTypeEnum)(r.type)
                    Dim amt As String = r.amount.ToString()
                    Select Case idx
                        Case 0
                            RT1Wgt = wgt : RT1Type = typ : RT1Amt = amt
                        Case 1
                            RT2Wgt = wgt : RT2Type = typ : RT2Amt = amt
                        Case 2
                            RT3Wgt = wgt : RT3Type = typ : RT3Amt = amt
                        Case 3
                            RT4Wgt = wgt : RT4Type = typ : RT4Amt = amt
                        Case 4
                            RT5Wgt = wgt : RT5Type = typ : RT5Amt = amt
                        Case 5
                            RT6Wgt = wgt : RT6Type = typ : RT6Amt = amt
                        Case 6
                            RT7Wgt = wgt : RT7Type = typ : RT7Amt = amt
                        Case 7
                            RT8Wgt = wgt : RT8Type = typ : RT8Amt = amt
                        Case 8
                            RT9Wgt = wgt : RT9Type = typ : RT9Amt = amt
                        Case 9
                            RT10Wgt = wgt : RT10Type = typ : RT10Amt = amt
                    End Select
                    idx += 1
                Next
            End If

            LastMaintDate = FormatDate(it.lastMaintenanceDate)
            OperatorId = it.operatorId
            Revision   = it.revision
            Status     = OperationStatus.Success
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

        ' rateTable rows (up to 10): weight, discAdjDir, discAdjUnits, discAdjType, amount
        ' Row 1
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
        ' Row 2
        Private _rt2Wgt As String = ""    : Public Property RT2Wgt As String
            Get
                Return _rt2Wgt
            End Get
            Set(v As String)
                SetField(_rt2Wgt, v)
            End Set
        End Property
        Private _rt2Dir As String = "NA"  : Public Property RT2DiscAdjDir As String
            Get
                Return _rt2Dir
            End Get
            Set(v As String)
                SetField(_rt2Dir, v)
            End Set
        End Property
        Private _rt2Units As String = "NA" : Public Property RT2DiscAdjUnits As String
            Get
                Return _rt2Units
            End Get
            Set(v As String)
                SetField(_rt2Units, v)
            End Set
        End Property
        Private _rt2Type As String = "NA" : Public Property RT2DiscAdjType As String
            Get
                Return _rt2Type
            End Get
            Set(v As String)
                SetField(_rt2Type, v)
            End Set
        End Property
        Private _rt2Amt As String = ""    : Public Property RT2Amt As String
            Get
                Return _rt2Amt
            End Get
            Set(v As String)
                SetField(_rt2Amt, v)
            End Set
        End Property
        ' Row 3
        Private _rt3Wgt As String = ""    : Public Property RT3Wgt As String
            Get
                Return _rt3Wgt
            End Get
            Set(v As String)
                SetField(_rt3Wgt, v)
            End Set
        End Property
        Private _rt3Dir As String = "NA"  : Public Property RT3DiscAdjDir As String
            Get
                Return _rt3Dir
            End Get
            Set(v As String)
                SetField(_rt3Dir, v)
            End Set
        End Property
        Private _rt3Units As String = "NA" : Public Property RT3DiscAdjUnits As String
            Get
                Return _rt3Units
            End Get
            Set(v As String)
                SetField(_rt3Units, v)
            End Set
        End Property
        Private _rt3Type As String = "NA" : Public Property RT3DiscAdjType As String
            Get
                Return _rt3Type
            End Get
            Set(v As String)
                SetField(_rt3Type, v)
            End Set
        End Property
        Private _rt3Amt As String = ""    : Public Property RT3Amt As String
            Get
                Return _rt3Amt
            End Get
            Set(v As String)
                SetField(_rt3Amt, v)
            End Set
        End Property
        ' Row 4
        Private _rt4Wgt As String = ""    : Public Property RT4Wgt As String
            Get
                Return _rt4Wgt
            End Get
            Set(v As String)
                SetField(_rt4Wgt, v)
            End Set
        End Property
        Private _rt4Dir As String = "NA"  : Public Property RT4DiscAdjDir As String
            Get
                Return _rt4Dir
            End Get
            Set(v As String)
                SetField(_rt4Dir, v)
            End Set
        End Property
        Private _rt4Units As String = "NA" : Public Property RT4DiscAdjUnits As String
            Get
                Return _rt4Units
            End Get
            Set(v As String)
                SetField(_rt4Units, v)
            End Set
        End Property
        Private _rt4Type As String = "NA" : Public Property RT4DiscAdjType As String
            Get
                Return _rt4Type
            End Get
            Set(v As String)
                SetField(_rt4Type, v)
            End Set
        End Property
        Private _rt4Amt As String = ""    : Public Property RT4Amt As String
            Get
                Return _rt4Amt
            End Get
            Set(v As String)
                SetField(_rt4Amt, v)
            End Set
        End Property
        ' Row 5
        Private _rt5Wgt As String = ""    : Public Property RT5Wgt As String
            Get
                Return _rt5Wgt
            End Get
            Set(v As String)
                SetField(_rt5Wgt, v)
            End Set
        End Property
        Private _rt5Dir As String = "NA"  : Public Property RT5DiscAdjDir As String
            Get
                Return _rt5Dir
            End Get
            Set(v As String)
                SetField(_rt5Dir, v)
            End Set
        End Property
        Private _rt5Units As String = "NA" : Public Property RT5DiscAdjUnits As String
            Get
                Return _rt5Units
            End Get
            Set(v As String)
                SetField(_rt5Units, v)
            End Set
        End Property
        Private _rt5Type As String = "NA" : Public Property RT5DiscAdjType As String
            Get
                Return _rt5Type
            End Get
            Set(v As String)
                SetField(_rt5Type, v)
            End Set
        End Property
        Private _rt5Amt As String = ""    : Public Property RT5Amt As String
            Get
                Return _rt5Amt
            End Get
            Set(v As String)
                SetField(_rt5Amt, v)
            End Set
        End Property
        ' Row 6
        Private _rt6Wgt As String = ""    : Public Property RT6Wgt As String
            Get
                Return _rt6Wgt
            End Get
            Set(v As String)
                SetField(_rt6Wgt, v)
            End Set
        End Property
        Private _rt6Dir As String = "NA"  : Public Property RT6DiscAdjDir As String
            Get
                Return _rt6Dir
            End Get
            Set(v As String)
                SetField(_rt6Dir, v)
            End Set
        End Property
        Private _rt6Units As String = "NA" : Public Property RT6DiscAdjUnits As String
            Get
                Return _rt6Units
            End Get
            Set(v As String)
                SetField(_rt6Units, v)
            End Set
        End Property
        Private _rt6Type As String = "NA" : Public Property RT6DiscAdjType As String
            Get
                Return _rt6Type
            End Get
            Set(v As String)
                SetField(_rt6Type, v)
            End Set
        End Property
        Private _rt6Amt As String = ""    : Public Property RT6Amt As String
            Get
                Return _rt6Amt
            End Get
            Set(v As String)
                SetField(_rt6Amt, v)
            End Set
        End Property
        ' Row 7
        Private _rt7Wgt As String = ""    : Public Property RT7Wgt As String
            Get
                Return _rt7Wgt
            End Get
            Set(v As String)
                SetField(_rt7Wgt, v)
            End Set
        End Property
        Private _rt7Dir As String = "NA"  : Public Property RT7DiscAdjDir As String
            Get
                Return _rt7Dir
            End Get
            Set(v As String)
                SetField(_rt7Dir, v)
            End Set
        End Property
        Private _rt7Units As String = "NA" : Public Property RT7DiscAdjUnits As String
            Get
                Return _rt7Units
            End Get
            Set(v As String)
                SetField(_rt7Units, v)
            End Set
        End Property
        Private _rt7Type As String = "NA" : Public Property RT7DiscAdjType As String
            Get
                Return _rt7Type
            End Get
            Set(v As String)
                SetField(_rt7Type, v)
            End Set
        End Property
        Private _rt7Amt As String = ""    : Public Property RT7Amt As String
            Get
                Return _rt7Amt
            End Get
            Set(v As String)
                SetField(_rt7Amt, v)
            End Set
        End Property
        ' Row 8
        Private _rt8Wgt As String = ""    : Public Property RT8Wgt As String
            Get
                Return _rt8Wgt
            End Get
            Set(v As String)
                SetField(_rt8Wgt, v)
            End Set
        End Property
        Private _rt8Dir As String = "NA"  : Public Property RT8DiscAdjDir As String
            Get
                Return _rt8Dir
            End Get
            Set(v As String)
                SetField(_rt8Dir, v)
            End Set
        End Property
        Private _rt8Units As String = "NA" : Public Property RT8DiscAdjUnits As String
            Get
                Return _rt8Units
            End Get
            Set(v As String)
                SetField(_rt8Units, v)
            End Set
        End Property
        Private _rt8Type As String = "NA" : Public Property RT8DiscAdjType As String
            Get
                Return _rt8Type
            End Get
            Set(v As String)
                SetField(_rt8Type, v)
            End Set
        End Property
        Private _rt8Amt As String = ""    : Public Property RT8Amt As String
            Get
                Return _rt8Amt
            End Get
            Set(v As String)
                SetField(_rt8Amt, v)
            End Set
        End Property
        ' Row 9
        Private _rt9Wgt As String = ""    : Public Property RT9Wgt As String
            Get
                Return _rt9Wgt
            End Get
            Set(v As String)
                SetField(_rt9Wgt, v)
            End Set
        End Property
        Private _rt9Dir As String = "NA"  : Public Property RT9DiscAdjDir As String
            Get
                Return _rt9Dir
            End Get
            Set(v As String)
                SetField(_rt9Dir, v)
            End Set
        End Property
        Private _rt9Units As String = "NA" : Public Property RT9DiscAdjUnits As String
            Get
                Return _rt9Units
            End Get
            Set(v As String)
                SetField(_rt9Units, v)
            End Set
        End Property
        Private _rt9Type As String = "NA" : Public Property RT9DiscAdjType As String
            Get
                Return _rt9Type
            End Get
            Set(v As String)
                SetField(_rt9Type, v)
            End Set
        End Property
        Private _rt9Amt As String = ""    : Public Property RT9Amt As String
            Get
                Return _rt9Amt
            End Get
            Set(v As String)
                SetField(_rt9Amt, v)
            End Set
        End Property
        ' Row 10
        Private _rt10Wgt As String = ""    : Public Property RT10Wgt As String
            Get
                Return _rt10Wgt
            End Get
            Set(v As String)
                SetField(_rt10Wgt, v)
            End Set
        End Property
        Private _rt10Dir As String = "NA"  : Public Property RT10DiscAdjDir As String
            Get
                Return _rt10Dir
            End Get
            Set(v As String)
                SetField(_rt10Dir, v)
            End Set
        End Property
        Private _rt10Units As String = "NA" : Public Property RT10DiscAdjUnits As String
            Get
                Return _rt10Units
            End Get
            Set(v As String)
                SetField(_rt10Units, v)
            End Set
        End Property
        Private _rt10Type As String = "NA" : Public Property RT10DiscAdjType As String
            Get
                Return _rt10Type
            End Get
            Set(v As String)
                SetField(_rt10Type, v)
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

        ' ── Build FXF3F.itemClass from this row ──────────────────────
        Public Function ToItemClass() As FXF3F.itemClass
            Dim it As New FXF3F.itemClass
            Dim adjT As FXF3F.adjTypeType
            If [Enum].TryParse(Of FXF3F.adjTypeType)(AdjType, True, adjT) Then it.itemHeader.adjType = adjT
            it.condition        = Condition
            it.prepaidOrCollect = ParseEnum(Of ScreenScraping.fxfPrepaidOrCollectEnum)(PrepdOrCollect, "NA")
            it.effectiveDate    = ParseDate(EffDate)
            it.cancelDate       = ParseDate(CanDateItem)
            it.comments         = Comments
            it.appRule          = ParseEnum(Of ScreenScraping.fxfAppRuleEnum)(AppRule, "NA")

            Dim rt As New FXF3F.rateTableCollection()
            Dim addRate As Action(Of String, String, String, String, String) =
                Sub(wgtS As String, dirS As String, unitsS As String, typeS As String, amtS As String)
                    Dim wv As Integer
                    If Not Integer.TryParse(wgtS, wv) OrElse wv <= 0 Then Return
                    Dim row As New FXF3F.rateTableRow()
                    row.weight = wv
                    Dim ddir As FXF3F.discAdjDirType
                    If [Enum].TryParse(Of FXF3F.discAdjDirType)(dirS, True, ddir) Then row.discAdjDir = ddir
                    Dim dunit As FXF3F.discAdjUnitsType
                    If [Enum].TryParse(Of FXF3F.discAdjUnitsType)(unitsS, True, dunit) Then row.discAdjUnits = dunit
                    Dim dtype As FXF3F.discAdjTypeType
                    If [Enum].TryParse(Of FXF3F.discAdjTypeType)(typeS, True, dtype) Then row.discAdjType = dtype
                    Dim av As Single
                    If Single.TryParse(amtS, av) Then row.amount = av
                    rt.Add(row)
                End Sub
            addRate(RT1Wgt, RT1DiscAdjDir, RT1DiscAdjUnits, RT1DiscAdjType, RT1Amt)
            addRate(RT2Wgt, RT2DiscAdjDir, RT2DiscAdjUnits, RT2DiscAdjType, RT2Amt)
            addRate(RT3Wgt, RT3DiscAdjDir, RT3DiscAdjUnits, RT3DiscAdjType, RT3Amt)
            addRate(RT4Wgt, RT4DiscAdjDir, RT4DiscAdjUnits, RT4DiscAdjType, RT4Amt)
            addRate(RT5Wgt, RT5DiscAdjDir, RT5DiscAdjUnits, RT5DiscAdjType, RT5Amt)
            addRate(RT6Wgt, RT6DiscAdjDir, RT6DiscAdjUnits, RT6DiscAdjType, RT6Amt)
            addRate(RT7Wgt, RT7DiscAdjDir, RT7DiscAdjUnits, RT7DiscAdjType, RT7Amt)
            addRate(RT8Wgt, RT8DiscAdjDir, RT8DiscAdjUnits, RT8DiscAdjType, RT8Amt)
            addRate(RT9Wgt, RT9DiscAdjDir, RT9DiscAdjUnits, RT9DiscAdjType, RT9Amt)
            addRate(RT10Wgt, RT10DiscAdjDir, RT10DiscAdjUnits, RT10DiscAdjType, RT10Amt)
            it.rateTable = rt
            Return it
        End Function

        ' ── Populate this row from a FXF3F.itemClass result ──────────
        Public Sub FromItemClass(it As FXF3F.itemClass)
            AdjType        = [Enum].GetName(GetType(FXF3F.adjTypeType), it.itemHeader.adjType)
            Condition      = it.condition
            PrepdOrCollect = GetEnumName(Of ScreenScraping.fxfPrepaidOrCollectEnum)(it.prepaidOrCollect)
            EffDate        = FormatDate(it.effectiveDate)
            CanDateItem    = FormatDate(it.cancelDate)
            Comments       = it.comments
            AppRule        = GetEnumName(Of ScreenScraping.fxfAppRuleEnum)(it.appRule)

            If it.rateTable IsNot Nothing Then
                Dim idx As Integer = 0
                For Each r As FXF3F.rateTableRow In it.rateTable
                    If idx >= 10 Then Exit For
                    Dim wgt As String = r.weight.ToString()
                    Dim dir As String = [Enum].GetName(GetType(FXF3F.discAdjDirType), r.discAdjDir)
                    Dim unt As String = [Enum].GetName(GetType(FXF3F.discAdjUnitsType), r.discAdjUnits)
                    Dim typ As String = [Enum].GetName(GetType(FXF3F.discAdjTypeType), r.discAdjType)
                    Dim amt As String = r.amount.ToString()
                    Select Case idx
                        Case 0
                            RT1Wgt = wgt : RT1DiscAdjDir = dir : RT1DiscAdjUnits = unt : RT1DiscAdjType = typ : RT1Amt = amt
                        Case 1
                            RT2Wgt = wgt : RT2DiscAdjDir = dir : RT2DiscAdjUnits = unt : RT2DiscAdjType = typ : RT2Amt = amt
                        Case 2
                            RT3Wgt = wgt : RT3DiscAdjDir = dir : RT3DiscAdjUnits = unt : RT3DiscAdjType = typ : RT3Amt = amt
                        Case 3
                            RT4Wgt = wgt : RT4DiscAdjDir = dir : RT4DiscAdjUnits = unt : RT4DiscAdjType = typ : RT4Amt = amt
                        Case 4
                            RT5Wgt = wgt : RT5DiscAdjDir = dir : RT5DiscAdjUnits = unt : RT5DiscAdjType = typ : RT5Amt = amt
                        Case 5
                            RT6Wgt = wgt : RT6DiscAdjDir = dir : RT6DiscAdjUnits = unt : RT6DiscAdjType = typ : RT6Amt = amt
                        Case 6
                            RT7Wgt = wgt : RT7DiscAdjDir = dir : RT7DiscAdjUnits = unt : RT7DiscAdjType = typ : RT7Amt = amt
                        Case 7
                            RT8Wgt = wgt : RT8DiscAdjDir = dir : RT8DiscAdjUnits = unt : RT8DiscAdjType = typ : RT8Amt = amt
                        Case 8
                            RT9Wgt = wgt : RT9DiscAdjDir = dir : RT9DiscAdjUnits = unt : RT9DiscAdjType = typ : RT9Amt = amt
                        Case 9
                            RT10Wgt = wgt : RT10DiscAdjDir = dir : RT10DiscAdjUnits = unt : RT10DiscAdjType = typ : RT10Amt = amt
                    End Select
                    idx += 1
                Next
            End If

            LastMaintDate = FormatDate(it.lastMaintenanceDate)
            OperatorId = it.operatorId
            Revision   = it.revision
            Status     = OperationStatus.Success
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

        ' schgTable rows (up to 10)
        ' Row 1
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
        ' Row 2
        Private _s2Cond As String = ""    : Public Property S2Cond As String
            Get
                Return _s2Cond
            End Get
            Set(v As String)
                SetField(_s2Cond, v)
            End Set
        End Property
        Private _s2Desc As String = ""    : Public Property S2Desc As String
            Get
                Return _s2Desc
            End Get
            Set(v As String)
                SetField(_s2Desc, v)
            End Set
        End Property
        Private _s2MinWgt As String = ""  : Public Property S2MinWgt As String
            Get
                Return _s2MinWgt
            End Get
            Set(v As String)
                SetField(_s2MinWgt, v)
            End Set
        End Property
        Private _s2MaxWgt As String = ""  : Public Property S2MaxWgt As String
            Get
                Return _s2MaxWgt
            End Get
            Set(v As String)
                SetField(_s2MaxWgt, v)
            End Set
        End Property
        Private _s2Type As String = ""    : Public Property S2Type As String
            Get
                Return _s2Type
            End Get
            Set(v As String)
                SetField(_s2Type, v)
            End Set
        End Property
        Private _s2Amt As String = ""     : Public Property S2Amount As String
            Get
                Return _s2Amt
            End Get
            Set(v As String)
                SetField(_s2Amt, v)
            End Set
        End Property
        Private _s2MinAmt As String = ""  : Public Property S2MinAmt As String
            Get
                Return _s2MinAmt
            End Get
            Set(v As String)
                SetField(_s2MinAmt, v)
            End Set
        End Property
        Private _s2MaxAmt As String = ""  : Public Property S2MaxAmt As String
            Get
                Return _s2MaxAmt
            End Get
            Set(v As String)
                SetField(_s2MaxAmt, v)
            End Set
        End Property
        Private _s2App As String = ""     : Public Property S2App As String
            Get
                Return _s2App
            End Get
            Set(v As String)
                SetField(_s2App, v)
            End Set
        End Property
        Private _s2CondId As String = ""  : Public Property S2CondId As String
            Get
                Return _s2CondId
            End Get
            Set(v As String)
                SetField(_s2CondId, v)
            End Set
        End Property
        ' Row 3
        Private _s3Cond As String = ""    : Public Property S3Cond As String
            Get
                Return _s3Cond
            End Get
            Set(v As String)
                SetField(_s3Cond, v)
            End Set
        End Property
        Private _s3Desc As String = ""    : Public Property S3Desc As String
            Get
                Return _s3Desc
            End Get
            Set(v As String)
                SetField(_s3Desc, v)
            End Set
        End Property
        Private _s3MinWgt As String = ""  : Public Property S3MinWgt As String
            Get
                Return _s3MinWgt
            End Get
            Set(v As String)
                SetField(_s3MinWgt, v)
            End Set
        End Property
        Private _s3MaxWgt As String = ""  : Public Property S3MaxWgt As String
            Get
                Return _s3MaxWgt
            End Get
            Set(v As String)
                SetField(_s3MaxWgt, v)
            End Set
        End Property
        Private _s3Type As String = ""    : Public Property S3Type As String
            Get
                Return _s3Type
            End Get
            Set(v As String)
                SetField(_s3Type, v)
            End Set
        End Property
        Private _s3Amt As String = ""     : Public Property S3Amount As String
            Get
                Return _s3Amt
            End Get
            Set(v As String)
                SetField(_s3Amt, v)
            End Set
        End Property
        Private _s3MinAmt As String = ""  : Public Property S3MinAmt As String
            Get
                Return _s3MinAmt
            End Get
            Set(v As String)
                SetField(_s3MinAmt, v)
            End Set
        End Property
        Private _s3MaxAmt As String = ""  : Public Property S3MaxAmt As String
            Get
                Return _s3MaxAmt
            End Get
            Set(v As String)
                SetField(_s3MaxAmt, v)
            End Set
        End Property
        Private _s3App As String = ""     : Public Property S3App As String
            Get
                Return _s3App
            End Get
            Set(v As String)
                SetField(_s3App, v)
            End Set
        End Property
        Private _s3CondId As String = ""  : Public Property S3CondId As String
            Get
                Return _s3CondId
            End Get
            Set(v As String)
                SetField(_s3CondId, v)
            End Set
        End Property
        ' Row 4
        Private _s4Cond As String = ""    : Public Property S4Cond As String
            Get
                Return _s4Cond
            End Get
            Set(v As String)
                SetField(_s4Cond, v)
            End Set
        End Property
        Private _s4Desc As String = ""    : Public Property S4Desc As String
            Get
                Return _s4Desc
            End Get
            Set(v As String)
                SetField(_s4Desc, v)
            End Set
        End Property
        Private _s4MinWgt As String = ""  : Public Property S4MinWgt As String
            Get
                Return _s4MinWgt
            End Get
            Set(v As String)
                SetField(_s4MinWgt, v)
            End Set
        End Property
        Private _s4MaxWgt As String = ""  : Public Property S4MaxWgt As String
            Get
                Return _s4MaxWgt
            End Get
            Set(v As String)
                SetField(_s4MaxWgt, v)
            End Set
        End Property
        Private _s4Type As String = ""    : Public Property S4Type As String
            Get
                Return _s4Type
            End Get
            Set(v As String)
                SetField(_s4Type, v)
            End Set
        End Property
        Private _s4Amt As String = ""     : Public Property S4Amount As String
            Get
                Return _s4Amt
            End Get
            Set(v As String)
                SetField(_s4Amt, v)
            End Set
        End Property
        Private _s4MinAmt As String = ""  : Public Property S4MinAmt As String
            Get
                Return _s4MinAmt
            End Get
            Set(v As String)
                SetField(_s4MinAmt, v)
            End Set
        End Property
        Private _s4MaxAmt As String = ""  : Public Property S4MaxAmt As String
            Get
                Return _s4MaxAmt
            End Get
            Set(v As String)
                SetField(_s4MaxAmt, v)
            End Set
        End Property
        Private _s4App As String = ""     : Public Property S4App As String
            Get
                Return _s4App
            End Get
            Set(v As String)
                SetField(_s4App, v)
            End Set
        End Property
        Private _s4CondId As String = ""  : Public Property S4CondId As String
            Get
                Return _s4CondId
            End Get
            Set(v As String)
                SetField(_s4CondId, v)
            End Set
        End Property
        ' Row 5
        Private _s5Cond As String = ""    : Public Property S5Cond As String
            Get
                Return _s5Cond
            End Get
            Set(v As String)
                SetField(_s5Cond, v)
            End Set
        End Property
        Private _s5Desc As String = ""    : Public Property S5Desc As String
            Get
                Return _s5Desc
            End Get
            Set(v As String)
                SetField(_s5Desc, v)
            End Set
        End Property
        Private _s5MinWgt As String = ""  : Public Property S5MinWgt As String
            Get
                Return _s5MinWgt
            End Get
            Set(v As String)
                SetField(_s5MinWgt, v)
            End Set
        End Property
        Private _s5MaxWgt As String = ""  : Public Property S5MaxWgt As String
            Get
                Return _s5MaxWgt
            End Get
            Set(v As String)
                SetField(_s5MaxWgt, v)
            End Set
        End Property
        Private _s5Type As String = ""    : Public Property S5Type As String
            Get
                Return _s5Type
            End Get
            Set(v As String)
                SetField(_s5Type, v)
            End Set
        End Property
        Private _s5Amt As String = ""     : Public Property S5Amount As String
            Get
                Return _s5Amt
            End Get
            Set(v As String)
                SetField(_s5Amt, v)
            End Set
        End Property
        Private _s5MinAmt As String = ""  : Public Property S5MinAmt As String
            Get
                Return _s5MinAmt
            End Get
            Set(v As String)
                SetField(_s5MinAmt, v)
            End Set
        End Property
        Private _s5MaxAmt As String = ""  : Public Property S5MaxAmt As String
            Get
                Return _s5MaxAmt
            End Get
            Set(v As String)
                SetField(_s5MaxAmt, v)
            End Set
        End Property
        Private _s5App As String = ""     : Public Property S5App As String
            Get
                Return _s5App
            End Get
            Set(v As String)
                SetField(_s5App, v)
            End Set
        End Property
        Private _s5CondId As String = ""  : Public Property S5CondId As String
            Get
                Return _s5CondId
            End Get
            Set(v As String)
                SetField(_s5CondId, v)
            End Set
        End Property
        ' Row 6
        Private _s6Cond As String = ""    : Public Property S6Cond As String
            Get
                Return _s6Cond
            End Get
            Set(v As String)
                SetField(_s6Cond, v)
            End Set
        End Property
        Private _s6Desc As String = ""    : Public Property S6Desc As String
            Get
                Return _s6Desc
            End Get
            Set(v As String)
                SetField(_s6Desc, v)
            End Set
        End Property
        Private _s6MinWgt As String = ""  : Public Property S6MinWgt As String
            Get
                Return _s6MinWgt
            End Get
            Set(v As String)
                SetField(_s6MinWgt, v)
            End Set
        End Property
        Private _s6MaxWgt As String = ""  : Public Property S6MaxWgt As String
            Get
                Return _s6MaxWgt
            End Get
            Set(v As String)
                SetField(_s6MaxWgt, v)
            End Set
        End Property
        Private _s6Type As String = ""    : Public Property S6Type As String
            Get
                Return _s6Type
            End Get
            Set(v As String)
                SetField(_s6Type, v)
            End Set
        End Property
        Private _s6Amt As String = ""     : Public Property S6Amount As String
            Get
                Return _s6Amt
            End Get
            Set(v As String)
                SetField(_s6Amt, v)
            End Set
        End Property
        Private _s6MinAmt As String = ""  : Public Property S6MinAmt As String
            Get
                Return _s6MinAmt
            End Get
            Set(v As String)
                SetField(_s6MinAmt, v)
            End Set
        End Property
        Private _s6MaxAmt As String = ""  : Public Property S6MaxAmt As String
            Get
                Return _s6MaxAmt
            End Get
            Set(v As String)
                SetField(_s6MaxAmt, v)
            End Set
        End Property
        Private _s6App As String = ""     : Public Property S6App As String
            Get
                Return _s6App
            End Get
            Set(v As String)
                SetField(_s6App, v)
            End Set
        End Property
        Private _s6CondId As String = ""  : Public Property S6CondId As String
            Get
                Return _s6CondId
            End Get
            Set(v As String)
                SetField(_s6CondId, v)
            End Set
        End Property
        ' Row 7
        Private _s7Cond As String = ""    : Public Property S7Cond As String
            Get
                Return _s7Cond
            End Get
            Set(v As String)
                SetField(_s7Cond, v)
            End Set
        End Property
        Private _s7Desc As String = ""    : Public Property S7Desc As String
            Get
                Return _s7Desc
            End Get
            Set(v As String)
                SetField(_s7Desc, v)
            End Set
        End Property
        Private _s7MinWgt As String = ""  : Public Property S7MinWgt As String
            Get
                Return _s7MinWgt
            End Get
            Set(v As String)
                SetField(_s7MinWgt, v)
            End Set
        End Property
        Private _s7MaxWgt As String = ""  : Public Property S7MaxWgt As String
            Get
                Return _s7MaxWgt
            End Get
            Set(v As String)
                SetField(_s7MaxWgt, v)
            End Set
        End Property
        Private _s7Type As String = ""    : Public Property S7Type As String
            Get
                Return _s7Type
            End Get
            Set(v As String)
                SetField(_s7Type, v)
            End Set
        End Property
        Private _s7Amt As String = ""     : Public Property S7Amount As String
            Get
                Return _s7Amt
            End Get
            Set(v As String)
                SetField(_s7Amt, v)
            End Set
        End Property
        Private _s7MinAmt As String = ""  : Public Property S7MinAmt As String
            Get
                Return _s7MinAmt
            End Get
            Set(v As String)
                SetField(_s7MinAmt, v)
            End Set
        End Property
        Private _s7MaxAmt As String = ""  : Public Property S7MaxAmt As String
            Get
                Return _s7MaxAmt
            End Get
            Set(v As String)
                SetField(_s7MaxAmt, v)
            End Set
        End Property
        Private _s7App As String = ""     : Public Property S7App As String
            Get
                Return _s7App
            End Get
            Set(v As String)
                SetField(_s7App, v)
            End Set
        End Property
        Private _s7CondId As String = ""  : Public Property S7CondId As String
            Get
                Return _s7CondId
            End Get
            Set(v As String)
                SetField(_s7CondId, v)
            End Set
        End Property
        ' Row 8
        Private _s8Cond As String = ""    : Public Property S8Cond As String
            Get
                Return _s8Cond
            End Get
            Set(v As String)
                SetField(_s8Cond, v)
            End Set
        End Property
        Private _s8Desc As String = ""    : Public Property S8Desc As String
            Get
                Return _s8Desc
            End Get
            Set(v As String)
                SetField(_s8Desc, v)
            End Set
        End Property
        Private _s8MinWgt As String = ""  : Public Property S8MinWgt As String
            Get
                Return _s8MinWgt
            End Get
            Set(v As String)
                SetField(_s8MinWgt, v)
            End Set
        End Property
        Private _s8MaxWgt As String = ""  : Public Property S8MaxWgt As String
            Get
                Return _s8MaxWgt
            End Get
            Set(v As String)
                SetField(_s8MaxWgt, v)
            End Set
        End Property
        Private _s8Type As String = ""    : Public Property S8Type As String
            Get
                Return _s8Type
            End Get
            Set(v As String)
                SetField(_s8Type, v)
            End Set
        End Property
        Private _s8Amt As String = ""     : Public Property S8Amount As String
            Get
                Return _s8Amt
            End Get
            Set(v As String)
                SetField(_s8Amt, v)
            End Set
        End Property
        Private _s8MinAmt As String = ""  : Public Property S8MinAmt As String
            Get
                Return _s8MinAmt
            End Get
            Set(v As String)
                SetField(_s8MinAmt, v)
            End Set
        End Property
        Private _s8MaxAmt As String = ""  : Public Property S8MaxAmt As String
            Get
                Return _s8MaxAmt
            End Get
            Set(v As String)
                SetField(_s8MaxAmt, v)
            End Set
        End Property
        Private _s8App As String = ""     : Public Property S8App As String
            Get
                Return _s8App
            End Get
            Set(v As String)
                SetField(_s8App, v)
            End Set
        End Property
        Private _s8CondId As String = ""  : Public Property S8CondId As String
            Get
                Return _s8CondId
            End Get
            Set(v As String)
                SetField(_s8CondId, v)
            End Set
        End Property
        ' Row 9
        Private _s9Cond As String = ""    : Public Property S9Cond As String
            Get
                Return _s9Cond
            End Get
            Set(v As String)
                SetField(_s9Cond, v)
            End Set
        End Property
        Private _s9Desc As String = ""    : Public Property S9Desc As String
            Get
                Return _s9Desc
            End Get
            Set(v As String)
                SetField(_s9Desc, v)
            End Set
        End Property
        Private _s9MinWgt As String = ""  : Public Property S9MinWgt As String
            Get
                Return _s9MinWgt
            End Get
            Set(v As String)
                SetField(_s9MinWgt, v)
            End Set
        End Property
        Private _s9MaxWgt As String = ""  : Public Property S9MaxWgt As String
            Get
                Return _s9MaxWgt
            End Get
            Set(v As String)
                SetField(_s9MaxWgt, v)
            End Set
        End Property
        Private _s9Type As String = ""    : Public Property S9Type As String
            Get
                Return _s9Type
            End Get
            Set(v As String)
                SetField(_s9Type, v)
            End Set
        End Property
        Private _s9Amt As String = ""     : Public Property S9Amount As String
            Get
                Return _s9Amt
            End Get
            Set(v As String)
                SetField(_s9Amt, v)
            End Set
        End Property
        Private _s9MinAmt As String = ""  : Public Property S9MinAmt As String
            Get
                Return _s9MinAmt
            End Get
            Set(v As String)
                SetField(_s9MinAmt, v)
            End Set
        End Property
        Private _s9MaxAmt As String = ""  : Public Property S9MaxAmt As String
            Get
                Return _s9MaxAmt
            End Get
            Set(v As String)
                SetField(_s9MaxAmt, v)
            End Set
        End Property
        Private _s9App As String = ""     : Public Property S9App As String
            Get
                Return _s9App
            End Get
            Set(v As String)
                SetField(_s9App, v)
            End Set
        End Property
        Private _s9CondId As String = ""  : Public Property S9CondId As String
            Get
                Return _s9CondId
            End Get
            Set(v As String)
                SetField(_s9CondId, v)
            End Set
        End Property
        ' Row 10
        Private _s10Cond As String = ""    : Public Property S10Cond As String
            Get
                Return _s10Cond
            End Get
            Set(v As String)
                SetField(_s10Cond, v)
            End Set
        End Property
        Private _s10Desc As String = ""    : Public Property S10Desc As String
            Get
                Return _s10Desc
            End Get
            Set(v As String)
                SetField(_s10Desc, v)
            End Set
        End Property
        Private _s10MinWgt As String = ""  : Public Property S10MinWgt As String
            Get
                Return _s10MinWgt
            End Get
            Set(v As String)
                SetField(_s10MinWgt, v)
            End Set
        End Property
        Private _s10MaxWgt As String = ""  : Public Property S10MaxWgt As String
            Get
                Return _s10MaxWgt
            End Get
            Set(v As String)
                SetField(_s10MaxWgt, v)
            End Set
        End Property
        Private _s10Type As String = ""    : Public Property S10Type As String
            Get
                Return _s10Type
            End Get
            Set(v As String)
                SetField(_s10Type, v)
            End Set
        End Property
        Private _s10Amt As String = ""     : Public Property S10Amount As String
            Get
                Return _s10Amt
            End Get
            Set(v As String)
                SetField(_s10Amt, v)
            End Set
        End Property
        Private _s10MinAmt As String = ""  : Public Property S10MinAmt As String
            Get
                Return _s10MinAmt
            End Get
            Set(v As String)
                SetField(_s10MinAmt, v)
            End Set
        End Property
        Private _s10MaxAmt As String = ""  : Public Property S10MaxAmt As String
            Get
                Return _s10MaxAmt
            End Get
            Set(v As String)
                SetField(_s10MaxAmt, v)
            End Set
        End Property
        Private _s10App As String = ""     : Public Property S10App As String
            Get
                Return _s10App
            End Get
            Set(v As String)
                SetField(_s10App, v)
            End Set
        End Property
        Private _s10CondId As String = ""  : Public Property S10CondId As String
            Get
                Return _s10CondId
            End Get
            Set(v As String)
                SetField(_s10CondId, v)
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

        ' ── Build FXF3G.itemClass from this row ──────────────────────
        Public Function ToItemClass() As FXF3G.itemClass
            Dim it As New FXF3G.itemClass
            it.itemHeader.auhority = Authority
            it.itemHeader.number   = Number
            it.itemHeader.item     = Item
            it.itemHeader.part     = Part
            it.prepaidOrCollect = ParseEnum(Of ScreenScraping.fxfPrepaidOrCollectEnum)(PrepdOrCollect, "NA")
            it.effectiveDate    = ParseDate(EffDate)
            it.cancelDate       = ParseDate(CanDateItem)
            it.comments         = Comments

            Dim st As New FXF3G.schgTableCollection()
            Dim addSchg As Action(Of String, String, String, String, String, String, String, String, String, String) =
                Sub(cond As String, desc As String, minW As String, maxW As String,
                    stype As String, amt As String, minA As String, maxA As String,
                    app As String, condId As String)
                    If String.IsNullOrWhiteSpace(cond) AndAlso String.IsNullOrWhiteSpace(desc) Then Return
                    Dim row As New FXF3G.schgTableRow()
                    row.cond = cond : row.desc = desc
                    Dim iv As Integer
                    If Integer.TryParse(minW, iv) Then row.minWgt = iv
                    If Integer.TryParse(maxW, iv) Then row.maxWgt = iv
                    row.type = stype
                    Dim dv As Double
                    If Double.TryParse(amt, dv) Then row.amount = dv
                    If Double.TryParse(minA, dv) Then row.minAmt = dv
                    If Double.TryParse(maxA, dv) Then row.maxAmt = dv
                    row.app = app : row.cond_id = condId
                    st.Add(row)
                End Sub
            addSchg(S1Cond, S1Desc, S1MinWgt, S1MaxWgt, S1Type, S1Amount, S1MinAmt, S1MaxAmt, S1App, S1CondId)
            addSchg(S2Cond, S2Desc, S2MinWgt, S2MaxWgt, S2Type, S2Amount, S2MinAmt, S2MaxAmt, S2App, S2CondId)
            addSchg(S3Cond, S3Desc, S3MinWgt, S3MaxWgt, S3Type, S3Amount, S3MinAmt, S3MaxAmt, S3App, S3CondId)
            addSchg(S4Cond, S4Desc, S4MinWgt, S4MaxWgt, S4Type, S4Amount, S4MinAmt, S4MaxAmt, S4App, S4CondId)
            addSchg(S5Cond, S5Desc, S5MinWgt, S5MaxWgt, S5Type, S5Amount, S5MinAmt, S5MaxAmt, S5App, S5CondId)
            addSchg(S6Cond, S6Desc, S6MinWgt, S6MaxWgt, S6Type, S6Amount, S6MinAmt, S6MaxAmt, S6App, S6CondId)
            addSchg(S7Cond, S7Desc, S7MinWgt, S7MaxWgt, S7Type, S7Amount, S7MinAmt, S7MaxAmt, S7App, S7CondId)
            addSchg(S8Cond, S8Desc, S8MinWgt, S8MaxWgt, S8Type, S8Amount, S8MinAmt, S8MaxAmt, S8App, S8CondId)
            addSchg(S9Cond, S9Desc, S9MinWgt, S9MaxWgt, S9Type, S9Amount, S9MinAmt, S9MaxAmt, S9App, S9CondId)
            addSchg(S10Cond, S10Desc, S10MinWgt, S10MaxWgt, S10Type, S10Amount, S10MinAmt, S10MaxAmt, S10App, S10CondId)
            it.schgTable = st
            Return it
        End Function

        ' ── Populate this row from a FXF3G.itemClass result ──────────
        Public Sub FromItemClass(it As FXF3G.itemClass)
            Authority = it.itemHeader.auhority
            Number    = it.itemHeader.number
            Item      = it.itemHeader.item
            Part      = it.itemHeader.part
            PrepdOrCollect = GetEnumName(Of ScreenScraping.fxfPrepaidOrCollectEnum)(it.prepaidOrCollect)
            EffDate        = FormatDate(it.effectiveDate)
            CanDateItem    = FormatDate(it.cancelDate)
            Comments       = it.comments

            If it.schgTable IsNot Nothing Then
                Dim idx As Integer = 0
                For Each r As FXF3G.schgTableRow In it.schgTable
                    If idx >= 10 Then Exit For
                    Select Case idx
                        Case 0
                            S1Cond = r.cond : S1Desc = r.desc : S1MinWgt = r.minWgt.ToString() : S1MaxWgt = r.maxWgt.ToString()
                            S1Type = r.type : S1Amount = r.amount.ToString() : S1MinAmt = r.minAmt.ToString() : S1MaxAmt = r.maxAmt.ToString() : S1App = r.app : S1CondId = r.cond_id
                        Case 1
                            S2Cond = r.cond : S2Desc = r.desc : S2MinWgt = r.minWgt.ToString() : S2MaxWgt = r.maxWgt.ToString()
                            S2Type = r.type : S2Amount = r.amount.ToString() : S2MinAmt = r.minAmt.ToString() : S2MaxAmt = r.maxAmt.ToString() : S2App = r.app : S2CondId = r.cond_id
                        Case 2
                            S3Cond = r.cond : S3Desc = r.desc : S3MinWgt = r.minWgt.ToString() : S3MaxWgt = r.maxWgt.ToString()
                            S3Type = r.type : S3Amount = r.amount.ToString() : S3MinAmt = r.minAmt.ToString() : S3MaxAmt = r.maxAmt.ToString() : S3App = r.app : S3CondId = r.cond_id
                        Case 3
                            S4Cond = r.cond : S4Desc = r.desc : S4MinWgt = r.minWgt.ToString() : S4MaxWgt = r.maxWgt.ToString()
                            S4Type = r.type : S4Amount = r.amount.ToString() : S4MinAmt = r.minAmt.ToString() : S4MaxAmt = r.maxAmt.ToString() : S4App = r.app : S4CondId = r.cond_id
                        Case 4
                            S5Cond = r.cond : S5Desc = r.desc : S5MinWgt = r.minWgt.ToString() : S5MaxWgt = r.maxWgt.ToString()
                            S5Type = r.type : S5Amount = r.amount.ToString() : S5MinAmt = r.minAmt.ToString() : S5MaxAmt = r.maxAmt.ToString() : S5App = r.app : S5CondId = r.cond_id
                        Case 5
                            S6Cond = r.cond : S6Desc = r.desc : S6MinWgt = r.minWgt.ToString() : S6MaxWgt = r.maxWgt.ToString()
                            S6Type = r.type : S6Amount = r.amount.ToString() : S6MinAmt = r.minAmt.ToString() : S6MaxAmt = r.maxAmt.ToString() : S6App = r.app : S6CondId = r.cond_id
                        Case 6
                            S7Cond = r.cond : S7Desc = r.desc : S7MinWgt = r.minWgt.ToString() : S7MaxWgt = r.maxWgt.ToString()
                            S7Type = r.type : S7Amount = r.amount.ToString() : S7MinAmt = r.minAmt.ToString() : S7MaxAmt = r.maxAmt.ToString() : S7App = r.app : S7CondId = r.cond_id
                        Case 7
                            S8Cond = r.cond : S8Desc = r.desc : S8MinWgt = r.minWgt.ToString() : S8MaxWgt = r.maxWgt.ToString()
                            S8Type = r.type : S8Amount = r.amount.ToString() : S8MinAmt = r.minAmt.ToString() : S8MaxAmt = r.maxAmt.ToString() : S8App = r.app : S8CondId = r.cond_id
                        Case 8
                            S9Cond = r.cond : S9Desc = r.desc : S9MinWgt = r.minWgt.ToString() : S9MaxWgt = r.maxWgt.ToString()
                            S9Type = r.type : S9Amount = r.amount.ToString() : S9MinAmt = r.minAmt.ToString() : S9MaxAmt = r.maxAmt.ToString() : S9App = r.app : S9CondId = r.cond_id
                        Case 9
                            S10Cond = r.cond : S10Desc = r.desc : S10MinWgt = r.minWgt.ToString() : S10MaxWgt = r.maxWgt.ToString()
                            S10Type = r.type : S10Amount = r.amount.ToString() : S10MinAmt = r.minAmt.ToString() : S10MaxAmt = r.maxAmt.ToString() : S10App = r.app : S10CondId = r.cond_id
                    End Select
                    idx += 1
                Next
            End If

            LastMaintDate = FormatDate(it.lastMaintenanceDate)
            OperatorId = it.operatorId
            Revision   = it.revision
            Status     = OperationStatus.Success
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
