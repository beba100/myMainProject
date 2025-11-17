# PowerShell script to convert markdown to Excel
$mdFilePath = "c:\BEBA_HOTEL\My API Project\‏‏Master Application Hotel Fixed\CustomerApi\HOTEL_CODES_LIST.md"
$excelFilePath = "c:\BEBA_HOTEL\My API Project\‏‏Master Application Hotel Fixed\CustomerApi\HOTEL_CODES_LIST.xlsx"

# Read markdown file
$content = Get-Content -Path $mdFilePath -Encoding UTF8

# Parse table data
$tableData = @()
$inTable = $false
$headerFound = $false

foreach ($line in $content) {
    $line = $line.Trim()
    if ($line -match '^\|\s*#\s*\|' -and $line -match 'Hotel Code') {
        $headerFound = $true
        continue
    }
    if ($headerFound -and $line -match '^\|[-:]+\|') {
        $inTable = $true
        continue
    }
    if ($inTable -and $line -match '^\|' -and -not ($line -match '^\|[-:]+\|')) {
        # Parse data row
        $cells = $line -split '\|' | Where-Object { $_.Trim() -ne '' }
        if ($cells.Count -ge 5) {
            $rowData = @()
            foreach ($cell in $cells) {
                $cell = $cell.Trim()
                # Remove backticks
                $cell = $cell -replace '`([^`]+)`', '$1'
                $rowData += $cell
            }
            $tableData += ,$rowData
        }
    }
    if ($inTable -and -not ($line -match '^\|')) {
        break
    }
}

# Create Excel application
$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false

# Create workbook
$workbook = $excel.Workbooks.Add()
$worksheet = $workbook.Worksheets.Item(1)
$worksheet.Name = "Hotel Codes List"

# Set headers
$headers = @("#", "Hotel Code", "Hotel Name", "Status", "Notes")
for ($col = 1; $col -le $headers.Count; $col++) {
    $cell = $worksheet.Cells.Item(1, $col)
    $cell.Value2 = $headers[$col - 1]
    $cell.Font.Bold = $true
    $cell.Font.Color = [System.Drawing.Color]::White
    $cell.Interior.Color = [System.Drawing.ColorTranslator]::FromHtml("#366092")
    $cell.HorizontalAlignment = -4108  # xlCenter
    $cell.VerticalAlignment = -4107    # xlCenter
    $cell.Font.Size = 12
}

# Add data rows
for ($row = 0; $row -lt $tableData.Count; $row++) {
    for ($col = 0; $col -lt $tableData[$row].Count; $col++) {
        $cell = $worksheet.Cells.Item($row + 2, $col + 1)
        $cell.Value2 = $tableData[$row][$col]
        $cell.HorizontalAlignment = -4108  # xlCenter
        $cell.VerticalAlignment = -4107    # xlCenter
        $cell.Font.Size = 11
        # Alternate row colors
        if ($row % 2 -eq 0) {
            $cell.Interior.Color = [System.Drawing.ColorTranslator]::FromHtml("#F2F2F2")
        }
    }
}

# Set column widths
$worksheet.Columns.Item(1).ColumnWidth = 8   # #
$worksheet.Columns.Item(2).ColumnWidth = 15  # Hotel Code
$worksheet.Columns.Item(3).ColumnWidth = 25  # Hotel Name
$worksheet.Columns.Item(4).ColumnWidth = 12  # Status
$worksheet.Columns.Item(5).ColumnWidth = 20  # Notes

# Set row height for header
$worksheet.Rows.Item(1).RowHeight = 25

# Freeze header row
$worksheet.Application.ActiveWindow.SplitRow = 1
$worksheet.Application.ActiveWindow.FreezePanes = $true

# Save and close
$workbook.SaveAs($excelFilePath, 51)  # 51 = xlOpenXMLWorkbook (.xlsx)
$workbook.Close()
$excel.Quit()

# Release COM objects
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($worksheet) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($workbook) | Out-Null
[System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
[System.GC]::Collect()
[System.GC]::WaitForPendingFinalizers()

Write-Host "Excel file created successfully: $excelFilePath"
Write-Host "Total hotels: $($tableData.Count)"





