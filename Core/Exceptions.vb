Option Strict On
Option Explicit On

' ================================================================
'  Exceptions.vb
'  Re-exports / documents the FedEx screen scraping exceptions
'  so all ViewModels can import Logility_Freight.Core and catch them
'  by short name without needing a direct reference to the
'  FedEx.PABST.SS.Exceptions namespace everywhere.
'
'  NOTE: The actual exception types live in the FedEx DLL.
'        This file exists so the project compiles with correct
'        Imports, and to document which exceptions to handle.
' ================================================================

' Import the FedEx exception namespace so ViewModels only need:
'   Imports Logility_Freight.Core
' and can catch: AccountNotFoundException, NoDiscountRecordsException,
'                NumericValueException, GenericScreenScraperException

' FedEx exceptions available from FedEx.PABST.SS.Exceptions:
'
'   AccountNotFoundException
'       Thrown when the account number is not found in the system.
'
'   NoDiscountRecordsException
'       Thrown when a getItem/getItems call finds no discount records.
'       Treat as a warning (not an error) — the item key is valid but empty.
'
'   NumericValueException
'       Thrown when a date or numeric field has an invalid value.
'       Most common cause: invalid cancel date format.
'
'   GenericScreenScraperException
'       Catch-all for CICS screen errors. Has a .ScreenDump property
'       containing a text dump of the terminal screen at failure time.
'       Always log .ScreenDump for diagnostics.

Namespace Core

    ''' <summary>
    ''' Marker enum for logging category of a screen scraping failure.
    ''' Used in the batch results export to classify rows by error type.
    ''' </summary>
    Public Enum ScreenErrorCategory
        None
        AccountNotFound
        NoRecords
        InvalidValue
        ScreenError
        Unknown
    End Enum

End Namespace
