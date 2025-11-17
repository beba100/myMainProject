using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for Invoice operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILogger<InvoiceController> _logger;

        /// <summary>
        /// Initializes a new instance of the InvoiceController class
        /// </summary>
        /// <param name="invoiceService">Invoice service</param>
        /// <param name="logger">Logger</param>
        public InvoiceController(IInvoiceService invoiceService, ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all invoices with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for invoice number, external ref, or notes</param>
        /// <returns>List of invoices with total count</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllInvoices(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (invoices, totalCount) = await _invoiceService.GetAllInvoicesAsync(pageNumber, pageSize, searchTerm);
                
                return Ok(new
                {
                    Invoices = invoices,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices");
                return StatusCode(500, "An error occurred while retrieving invoices.");
            }
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>Invoice details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
                if (invoice == null)
                {
                    return NotFound($"Invoice with ID {id} not found.");
                }

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice with ID {InvoiceId}", id);
                return StatusCode(500, "An error occurred while retrieving the invoice.");
            }
        }

        /// <summary>
        /// Get invoice by invoice number
        /// </summary>
        /// <param name="invoiceNo">Invoice number</param>
        /// <returns>Invoice details</returns>
        [HttpGet("number/{invoiceNo}")]
        public async Task<IActionResult> GetInvoiceByNo(string invoiceNo)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByNoAsync(invoiceNo);
                if (invoice == null)
                {
                    return NotFound($"Invoice with number '{invoiceNo}' not found.");
                }

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice with number {InvoiceNo}", invoiceNo);
                return StatusCode(500, "An error occurred while retrieving the invoice.");
            }
        }

        /// <summary>
        /// Create new invoice
        /// </summary>
        /// <param name="createInvoiceDto">Invoice creation data</param>
        /// <returns>Created invoice</returns>
        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceDto createInvoiceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var invoice = await _invoiceService.CreateInvoiceAsync(createInvoiceDto);
                return CreatedAtAction(nameof(GetInvoiceById), new { id = invoice.InvoiceId }, invoice);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(500, "An error occurred while creating the invoice.");
            }
        }

        /// <summary>
        /// Update existing invoice
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <param name="updateInvoiceDto">Invoice update data</param>
        /// <returns>Updated invoice</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] UpdateInvoiceDto updateInvoiceDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var invoice = await _invoiceService.UpdateInvoiceAsync(id, updateInvoiceDto);
                if (invoice == null)
                {
                    return NotFound($"Invoice with ID {id} not found.");
                }

                return Ok(invoice);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice with ID {InvoiceId}", id);
                return StatusCode(500, "An error occurred while updating the invoice.");
            }
        }

        /// <summary>
        /// Delete invoice
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            try
            {
                var result = await _invoiceService.DeleteInvoiceAsync(id);
                if (!result)
                {
                    return NotFound($"Invoice with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice with ID {InvoiceId}", id);
                return StatusCode(500, "An error occurred while deleting the invoice.");
            }
        }

        /// <summary>
        /// Get invoices by customer ID
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of customer invoices</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetInvoicesByCustomerId(int customerId)
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesByCustomerIdAsync(customerId);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while retrieving customer invoices.");
            }
        }

        /// <summary>
        /// Get invoices by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of hotel invoices</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetInvoicesByHotelId(int hotelId)
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesByHotelIdAsync(hotelId);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving hotel invoices.");
            }
        }

        /// <summary>
        /// Get invoices by reservation ID
        /// </summary>
        /// <param name="reservationId">Reservation ID</param>
        /// <returns>List of reservation invoices</returns>
        [HttpGet("reservation/{reservationId}")]
        public async Task<IActionResult> GetInvoicesByReservationId(int reservationId)
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesByReservationIdAsync(reservationId);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices for reservation {ReservationId}", reservationId);
                return StatusCode(500, "An error occurred while retrieving reservation invoices.");
            }
        }

        /// <summary>
        /// Get invoices by payment status
        /// </summary>
        /// <param name="paymentStatus">Payment status</param>
        /// <returns>List of invoices with specified payment status</returns>
        [HttpGet("payment-status/{paymentStatus}")]
        public async Task<IActionResult> GetInvoicesByPaymentStatus(string paymentStatus)
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesByPaymentStatusAsync(paymentStatus);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices with payment status {PaymentStatus}", paymentStatus);
                return StatusCode(500, "An error occurred while retrieving invoices by payment status.");
            }
        }

        /// <summary>
        /// Get invoices by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of invoices in date range</returns>
        [HttpGet("date-range")]
        public async Task<IActionResult> GetInvoicesByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesByDateRangeAsync(startDate, endDate);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices by date range");
                return StatusCode(500, "An error occurred while retrieving invoices by date range.");
            }
        }

        /// <summary>
        /// Get invoices by invoice type
        /// </summary>
        /// <param name="invoiceType">Invoice type</param>
        /// <returns>List of invoices with specified type</returns>
        [HttpGet("type/{invoiceType}")]
        public async Task<IActionResult> GetInvoicesByType(string invoiceType)
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesByTypeAsync(invoiceType);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices by type {InvoiceType}", invoiceType);
                return StatusCode(500, "An error occurred while retrieving invoices by type.");
            }
        }

        /// <summary>
        /// Search invoices by customer name
        /// </summary>
        /// <param name="customerName">Customer name to search for</param>
        /// <returns>List of matching invoices</returns>
        [HttpGet("search/customer")]
        public async Task<IActionResult> SearchInvoicesByCustomerName([FromQuery] string customerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerName))
                {
                    return BadRequest("Customer name cannot be empty.");
                }

                var invoices = await _invoiceService.SearchInvoicesByCustomerNameAsync(customerName);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching invoices by customer name {CustomerName}", customerName);
                return StatusCode(500, "An error occurred while searching invoices by customer name.");
            }
        }

        /// <summary>
        /// Search invoices by hotel name
        /// </summary>
        /// <param name="hotelName">Hotel name to search for</param>
        /// <returns>List of matching invoices</returns>
        [HttpGet("search/hotel")]
        public async Task<IActionResult> SearchInvoicesByHotelName([FromQuery] string hotelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hotelName))
                {
                    return BadRequest("Hotel name cannot be empty.");
                }

                var invoices = await _invoiceService.SearchInvoicesByHotelNameAsync(hotelName);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching invoices by hotel name {HotelName}", hotelName);
                return StatusCode(500, "An error occurred while searching invoices by hotel name.");
            }
        }

        /// <summary>
        /// Search invoices by invoice number
        /// </summary>
        /// <param name="invoiceNo">Invoice number to search for</param>
        /// <returns>List of matching invoices</returns>
        [HttpGet("search/number")]
        public async Task<IActionResult> SearchInvoicesByNo([FromQuery] string invoiceNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invoiceNo))
                {
                    return BadRequest("Invoice number cannot be empty.");
                }

                var invoices = await _invoiceService.SearchInvoicesByNoAsync(invoiceNo);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching invoices by number {InvoiceNo}", invoiceNo);
                return StatusCode(500, "An error occurred while searching invoices by number.");
            }
        }

        /// <summary>
        /// Get invoice statistics
        /// </summary>
        /// <returns>Invoice statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetInvoiceStatistics()
        {
            try
            {
                var statistics = await _invoiceService.GetInvoiceStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoice statistics");
                return StatusCode(500, "An error occurred while retrieving invoice statistics.");
            }
        }

        /// <summary>
        /// Check if invoice number exists
        /// </summary>
        /// <param name="invoiceNo">Invoice number to check</param>
        /// <param name="excludeId">Invoice ID to exclude from check (for updates)</param>
        /// <returns>True if number exists, false otherwise</returns>
        [HttpGet("check-number")]
        public async Task<IActionResult> CheckInvoiceNumber([FromQuery] string invoiceNo, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invoiceNo))
                {
                    return BadRequest("Invoice number cannot be empty.");
                }

                var exists = await _invoiceService.InvoiceNoExistsAsync(invoiceNo, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking invoice number {InvoiceNo}", invoiceNo);
                return StatusCode(500, "An error occurred while checking invoice number.");
            }
        }

        /// <summary>
        /// Get unpaid invoices
        /// </summary>
        /// <returns>List of unpaid invoices</returns>
        [HttpGet("unpaid")]
        public async Task<IActionResult> GetUnpaidInvoices()
        {
            try
            {
                var invoices = await _invoiceService.GetUnpaidInvoicesAsync();
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unpaid invoices");
                return StatusCode(500, "An error occurred while retrieving unpaid invoices.");
            }
        }

        /// <summary>
        /// Get overdue invoices
        /// </summary>
        /// <returns>List of overdue invoices</returns>
        [HttpGet("overdue")]
        public async Task<IActionResult> GetOverdueInvoices()
        {
            try
            {
                var invoices = await _invoiceService.GetOverdueInvoicesAsync();
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue invoices");
                return StatusCode(500, "An error occurred while retrieving overdue invoices.");
            }
        }

        /// <summary>
        /// Get invoices by ZATCA status
        /// </summary>
        /// <param name="isSentZatca">ZATCA sent status</param>
        /// <returns>List of invoices with specified ZATCA status</returns>
        [HttpGet("zatca-status")]
        public async Task<IActionResult> GetInvoicesByZatcaStatus([FromQuery] bool isSentZatca)
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesByZatcaStatusAsync(isSentZatca);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices by ZATCA status {IsSentZatca}", isSentZatca);
                return StatusCode(500, "An error occurred while retrieving invoices by ZATCA status.");
            }
        }

        /// <summary>
        /// Get invoices by period range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of invoices in period range</returns>
        [HttpGet("period-range")]
        public async Task<IActionResult> GetInvoicesByPeriodRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesByPeriodRangeAsync(startDate, endDate);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invoices by period range");
                return StatusCode(500, "An error occurred while retrieving invoices by period range.");
            }
        }

        /// <summary>
        /// Update payment status
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <param name="paymentStatus">New payment status</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/payment-status")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] string paymentStatus)
        {
            try
            {
                var result = await _invoiceService.UpdatePaymentStatusAsync(id, paymentStatus);
                if (!result)
                {
                    return NotFound($"Invoice with ID {id} not found.");
                }

                return Ok(new { Message = "Payment status updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for ID {InvoiceId}", id);
                return StatusCode(500, "An error occurred while updating payment status.");
            }
        }

        /// <summary>
        /// Update payment amount
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <param name="amountPaid">Amount paid</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/payment-amount")]
        public async Task<IActionResult> UpdatePaymentAmount(int id, [FromBody] decimal amountPaid)
        {
            try
            {
                var result = await _invoiceService.UpdatePaymentAmountAsync(id, amountPaid);
                if (!result)
                {
                    return NotFound($"Invoice with ID {id} not found.");
                }

                return Ok(new { Message = "Payment amount updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment amount for ID {InvoiceId}", id);
                return StatusCode(500, "An error occurred while updating payment amount.");
            }
        }

        /// <summary>
        /// Mark invoice as sent to ZATCA
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <param name="zatcaUuid">ZATCA UUID</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/mark-zatca")]
        public async Task<IActionResult> MarkAsSentToZatca(int id, [FromBody] string zatcaUuid)
        {
            try
            {
                var result = await _invoiceService.MarkAsSentToZatcaAsync(id, zatcaUuid);
                if (!result)
                {
                    return NotFound($"Invoice with ID {id} not found.");
                }

                return Ok(new { Message = "Invoice marked as sent to ZATCA successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking invoice as sent to ZATCA for ID {InvoiceId}", id);
                return StatusCode(500, "An error occurred while marking invoice as sent to ZATCA.");
            }
        }

        /// <summary>
        /// Calculate invoice totals
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/calculate-totals")]
        public async Task<IActionResult> CalculateInvoiceTotals(int id)
        {
            try
            {
                var result = await _invoiceService.CalculateInvoiceTotalsAsync(id);
                if (!result)
                {
                    return NotFound($"Invoice with ID {id} not found.");
                }

                return Ok(new { Message = "Invoice totals calculated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating invoice totals for ID {InvoiceId}", id);
                return StatusCode(500, "An error occurred while calculating invoice totals.");
            }
        }
    }
}
