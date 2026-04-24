# Screen Data Models — Exact Field Reference
# Derived from FedEx screen scraping source code

---

## Common pattern for all FXF3x screens

Every screen's `BatchRow` model has:
- The common key fields (Action, Carrier, CustType, Account, Authority, Number, Item)
- FXF3B–G also have `Part` as a key field
- Screen-specific item fields
- `Status` (OperationStatus enum), `StatusMessage` String, `IsRunning` Boolean

---

## FXF3A — Customer Discount Items

**Source class:** `FedEx.PABST.SS.Screens.FXF3A`
**Has getItems:** Yes — returns `ItemHeaderCollection`
**Has cancelItem:** Yes — sets cancel date on discount rows
**Has releaseItem:** Yes

### itemHeaderClass fields:
```
auhority As String          ← note: typo in source, must match exactly
number As String
item As String
discTable As DiscCollection
isReleased As Boolean
```

### discountTable fields (up to N rows, shown inline as rows 1-3):
```
disc As Single              ← discount percentage
minChargeDisc As Single     ← minimum charge discount
maxWgt As Integer           ← maximum weight
floorMin As Single          ← floor minimum
effectiveDate As Date       ← MM/dd/yy
cancelDate As Date          ← MM/dd/yy, NULL_DATE = open ended
```

### itemClass fields:
```
itemHeader As itemHeaderClass
currency As String
fsAuthority As String
fsNumber As String
fsItem As String
prepaidInbound As Boolean
prepaidOutbound As Boolean
collectInbound As Boolean
collectOutbound As Boolean
thirdParty As Boolean
inter As fxfInterEnum           ← NA/I/S/B
typeHaul As fxfTypeHaulEnum     ← NA/SingleLine/JointLineToCarrier/JointLineFromCarrier
country As String
matrix As String
geoDir1 As fxfGeoDirEnum        ← NA/F/T/B/A/X
geoType1 As fxfGeoTypeEnum      ← NA/AC/AL/CC/CN/CO/NC/NN/PC/ST/TE/ZR/GG
geoName1 As String
geoState1 As String
geoCountry1 As String
geoDir2 As fxfGeoDirEnum
geoType2 As fxfGeoTypeEnum
geoName2 As String
geoState2 As String
geoCountry2 As String
ratesEffective As Date
servDaysLow As String
servDaysHigh As String
classZip As fxfClassZipEnum     ← NA/B/C/D/N/S/X/C_3/C_5
classZipAuthority As String
classZipNumber As String
classZipSection As String
applyArbs As Boolean
excClass As String
excClassMaxWgt As String
genGeoAlt As fxfGenGeoAltrEnum  ← NA/Y/N/X/A/Q
minWgt As String
maxWgt As String
incExempt As Boolean
fak As Boolean
edAgg As String
nmfc50 As Boolean
nmfc55 As Boolean
nmfc60 As Boolean
nmfc65 As Boolean
nmfc70 As Boolean
nmfc77_5 As Boolean
nmfc85 As Boolean
nmfc92_5 As Boolean
nmfc100 As Boolean
nmfc110 As Boolean
nmfc125 As Boolean
nmfc150 As Boolean
nmfc175 As Boolean
nmfc200 As Boolean
nmfc250 As Boolean
nmfc300 As Boolean
nmfc400 As Boolean
nmfc500 As Boolean
payRule1 As String
payRule2 As String
lastMaintenanceDate As Date     ← read-only from screen
operatorId As String            ← read-only from screen
revision As String              ← read-only from screen
```

### CRUD method signatures:
```vb
' List — FXF3A only
Function getItems(pCarrier, pCustType, pAccountNumber,
    Optional bActiveItemsOnly = True,
    Optional pEffectiveDate = NULL_DATE) As ItemHeaderCollection

' Single item
Function getItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem) As itemClass

' Write operations
Sub addItem(pCarrier, pCustType, pAccountNumber,
    pItem As itemClass, Optional pRelease = False)

Sub changeItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pItemObj As itemClass,
    Optional pRelease = False)

Sub cancelItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem,
    Optional cancelDate = NULL_DATE,
    Optional pRelease = False)

Sub deleteItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem)

Sub releaseItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem)
```

---

## FXF3B — Discounts by State/Terminal

**Source class:** `FedEx.PABST.SS.Screens.FXF3B`
**Screen title:** CUST/NAT'L ACCNT DISCOUNTS (BY STATE/TERM ONLY)
**Has getItems:** No — getItem only
**Has cancelItem:** No
**Has releaseItem:** No
**Extra key field:** Part

### itemHeaderClass fields:
```
auhority As String
number As String
item As String
part As String          ← additional key field vs FXF3A
discTable As DiscCollection   ← same structure as FXF3A
isReleased As Boolean
```

### itemClass fields:
```
itemHeader As itemHeaderClass
fsAuthority As String
fsNumber As String
fsItem As String
prepaidInbound As Boolean
prepaidOutbound As Boolean
collectInbound As Boolean
collectOutbound As Boolean
geoTbl1 As geoTableCollection
geoTbl2 As geoTableCollection
rateEff As Date
classZip As fxfClassZipEnum
classZipAuthority As String
classZipNumber As String
classZipSection As String
excClass As String
excClassMaxWgt As String
genGeoAlt As fxfGenGeoAltrEnum
lastMaintenanceDate As Date     ← read-only
operatorId As String            ← read-only
revision As String              ← read-only
```

### geoTableCollection fields:
```
incExc As fxfPlusMinusEnum   ← Plus or Minus only
dir As fxfGeoDirEnum         ← F/T/B/A only
type As fxfGeoTypeEnum       ← ST or TE only
rows: List of geoTableRow:
  name As String             ← 2-char state abbr if type=ST; ≤4 chars if type=TE
  country As String          ← 2-char country abbr
```

### CRUD method signatures:
```vb
Function getItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart) As itemClass

Sub addItem(pCarrier, pCustType, pAccountNumber,
    pItem As itemClass, Optional pRelease = False)

Sub changeItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart,
    pItemObj As itemClass, Optional pRelease = False)

Sub deleteItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart)
```

---

## FXF3C — Customer Geography Discounts

**Source class:** `FedEx.PABST.SS.Screens.FXF3C`
**Screen title:** CUST/NAT'L ACCNT GEOGRAPHY
**Has getItems:** No
**Has cancelItem:** No
**Extra key field:** Part

### itemHeaderClass fields:
```
auhority As String
number As String
item As String
part As String
geoTable As geoTableCollection   ← differs from FXF3B — in header not item
isReleased As Boolean
```

### geoTableRow fields (FXF3C — more fields than FXF3B):
```
plusMinus As fxfPlusMinusEnum
dir As fxfGeoDirEnum
type As fxfGeoTypeEnum
name As String
state As String
country As String
```

### itemClass fields:
```
itemHeader As itemHeaderClass
servDaysLow As Integer
servDaysHigh As Integer
lastMaintenanceDate As Date    ← read-only
operatorId As String           ← read-only
revision As String             ← read-only
```

### CRUD method signatures:
```vb
Function getItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart) As itemClass

Sub addItem(pCarrier, pCustType, pAccountNumber,
    pItem As itemClass, Optional pRelease = False)

Sub changeItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart,
    pItemObj As itemClass, Optional pRelease = False)

Sub deleteItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart)
```

---

## FXF3D — Customer Product Discounts

**Source class:** `FedEx.PABST.SS.Screens.FXF3D`
**Screen title:** CUST/NAT'L ACCNT PRODUCTS
**Has getItems:** No
**Has cancelItem:** Yes
**Extra key field:** Part

### itemHeaderClass fields:
```
auhority As String
number As String
item As String
part As String
isReleased As Boolean
```

### prodTableRow fields:
```
type As accountProdType    ← accountProdType enum (defined in FXF3D class)
product1 As String
product2 As String
excCls As String
```

### itemClass fields:
```
itemHeader As itemHeaderClass
effectiveDate As Date
cancelDate As Date
excClass As String
excClassMaxWgt As String
prodTable As prodTableCollection
lastMaintenanceDate As Date    ← read-only
operatorId As String           ← read-only
revision As String             ← read-only
```

### CRUD method signatures:
```vb
Function getItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart) As itemClass

Sub addItem(pCarrier, pCustType, pAccountNumber,
    pItem As itemClass, Optional pRelease = False)

Sub changeItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart,
    pItemObj As itemClass, Optional pRelease = False)

Sub cancelItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart,
    Optional cancelDate = NULL_DATE, Optional pRelease = False)

Sub deleteItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart)
```

---

## FXF3E — Customer Rates

**Source class:** `FedEx.PABST.SS.Screens.FXF3E`
**Screen title:** CUST/NAT'L ACCNT RATES
**Has getItems:** No
**Has cancelItem:** Yes
**Extra key field:** Part

### itemHeaderClass fields:
```
auhority As String
number As String
item As String
part As String
' Note: isActive property stub exists but always returns False (commented out body)
```

### rateTableRow fields:
```
weight As Integer
type As fxfRateTypeEnum     ← NA/R/M/F/D/DF/ED/EF
amount As Single
```

### itemClass fields:
```
itemHeader As itemHeaderClass
condition As String
prepaidOrCollect As fxfPrepaidOrCollectEnum   ← NA/C/P/T
effectiveDate As Date
cancelDate As Date
comments As String
alternation As fxfAlternationEnum             ← NA/K/L/N
classRates As fxfClassZipEnum
rateManually As Boolean
clsTrfAuthority As String
clsTrfNumber As String
clsTrfSection As String
rateEffDate As Date
rateTable As rateTableCollection
lastMaintenanceDate As Date    ← read-only
operatorId As String           ← read-only
revision As String             ← read-only
```

### CRUD method signatures:
```vb
Function getItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart) As itemClass

Sub addItem(pCarrier, pCustType, pAccountNumber,
    pItem As itemClass, Optional pRelease = False)

Sub changeItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart,
    pItemObj As itemClass, Optional pRelease = False)

Sub cancelItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart,
    Optional cancelDate = NULL_DATE, Optional pRelease = False)

Sub deleteItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart)
```

---

## FXF3F — Customer Discounts/Adjustments

**Source class:** `FedEx.PABST.SS.Screens.FXF3F`
**Screen title:** CUST/NAT'L ACCNT DISCOUNTS/ADJUSTMENTS
**Has getItems:** No
**Has cancelItem:** Yes
**Extra key field:** Part

### itemHeaderClass fields:
```
auhority As String
number As String
item As String
part As String
adjType As adjTypeType      ← adjTypeType enum defined in FXF3F
isReleased As Boolean
```

### rateTableRow fields (FXF3F — more complex than FXF3E):
```
weight As Integer
discAdjDir As discAdjDirType      ← blank, I, or D
discAdjUnits As discAdjUnitsType  ← %, $, P, or D  (% == P, $ == D)
discAdjType As discAdjTypeType    ← C, H, M, F, or R
amount As Single
```

### itemClass fields:
```
itemHeader As itemHeaderClass
condition As String
prepaidOrCollect As fxfPrepaidOrCollectEnum
effectiveDate As Date
cancelDate As Date
comments As String
appRule As fxfAppRuleEnum         ← NA/A/B/C/M/C_3/C_5/U
rateTable As rateTableCollection
lastMaintenanceDate As Date    ← read-only
operatorId As String           ← read-only
revision As String             ← read-only
```

### CRUD method signatures:
```vb
Function getItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart) As itemClass

Sub addItem(pCarrier, pCustType, pAccountNumber,
    pItem As itemClass, Optional pRelease = False)

Sub changeItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart,
    pItemObj As itemClass, Optional pRelease = False)

Sub cancelItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart,
    Optional cancelDate = NULL_DATE, Optional pRelease = False)

Sub deleteItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart)
```

---

## FXF3G — Customer Charges/Allowances

**Source class:** `FedEx.PABST.SS.Screens.FXF3G`
**Screen title:** CUST/NAT'L ACCNT CHARGES/ALLOWANCES
**Has getItems:** No
**Has cancelItem:** Yes
**Extra key field:** Part

### itemHeaderClass fields:
```
auhority As String
number As String
item As String
part As String
isReleased As Boolean
```

### schgTableRow fields:
```
cond As String
desc As String
minWgt As Integer
maxWgt As Integer
type As String          ← should be enum per source comment, but is String
amount As Double
minAmt As Double
maxAmt As Double
app As String           ← only M or blank
cond_id As String       ← % off current 100 rates
```

### itemClass fields:
```
itemHeader As itemHeaderClass
prepaidOrCollect As fxfPrepaidOrCollectEnum
effectiveDate As Date
cancelDate As Date
comments As String
schgTable As schgTableCollection
lastMaintenanceDate As Date    ← read-only
operatorId As String           ← read-only
revision As String             ← read-only
```

### CRUD method signatures:
```vb
Function getItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart) As itemClass

Sub addItem(pCarrier, pCustType, pAccountNumber,
    pItem As itemClass, Optional pRelease = False)

Sub changeItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart,
    pItemObj As itemClass, Optional pRelease = False)

Sub cancelItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart,
    Optional cancelDate = NULL_DATE, Optional pRelease = False)

Sub deleteItem(pCarrier, pCustType, pAccountNumber,
    pAuthority, pNumber, pItem, pPart)
```

---

## Shared OperationStatus enum (used in all BatchRow models)

```vb
Public Enum OperationStatus
    Pending     ' blank / not yet run
    Running     ' currently executing
    Success     ' completed OK
    Warning     ' completed with no-records (not an error)
    Error       ' failed
    Skipped     ' blank action row
End Enum
```
