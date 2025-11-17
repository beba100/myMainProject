using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for PaymentReceipt operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentReceiptController : ControllerBase
    {
        private readonly IPaymentReceiptService _paymentReceiptService;
        private readonly ILogger<PaymentReceiptController> _logger;

        /// <summary>
        /// Initializes a new instance of the PaymentReceiptController class
        /// </summary>
        /// <param name="paymentReceiptService">PaymentReceipt service</param>
        /// <param name="logger">Logger</param>
        public PaymentReceiptController(IPaymentReceiptService paymentReceiptService, ILogger<PaymentReceiptController> logger)
        {
            _paymentReceiptService = paymentReceiptService;
            _logger = logger;
        }

        /// <summary>
        /// Get all payment receipts with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for receipt number, external ref, transaction no, or notes</param>
        /// <returns>List of payment receipts with total count</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllPaymentReceipts(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (paymentReceipts, totalCount) = await _paymentReceiptService.GetAllPaymentReceiptsAsync(pageNumber, pageSize, searchTerm);
                
                return Ok(new
                {
                    PaymentReceipts = paymentReceipts,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts");
                return StatusCode(500, "An error occurred while retrieving payment receipts.");
            }
        }

        /// <summary>
        /// Get payment receipt by ID
        /// </summary>
        /// <param name="id">Payment receipt ID</param>
        /// <returns>Payment receipt details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentReceiptById(int id)
        {
            try
            {
                var paymentReceipt = await _paymentReceiptService.GetPaymentReceiptByIdAsync(id);
                if (paymentReceipt == null)
                {
                    return NotFound($"Payment receipt with ID {id} not found.");
                }

                return Ok(paymentReceipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipt with ID {PaymentReceiptId}", id);
                return StatusCode(500, "An error occurred while retrieving the payment receipt.");
            }
        }

        /// <summary>
        /// Get payment receipt by receipt number
        /// </summary>
        /// <param name="receiptNo">Receipt number</param>
        /// <returns>Payment receipt details</returns>
        [HttpGet("number/{receiptNo}")]
        public async Task<IActionResult> GetPaymentReceiptByNo(string receiptNo)
        {
            try
            {
                var paymentReceipt = await _paymentReceiptService.GetPaymentReceiptByNoAsync(receiptNo);
                if (paymentReceipt == null)
                {
                    return NotFound($"Payment receipt with number '{receiptNo}' not found.");
                }

                return Ok(paymentReceipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipt with number {ReceiptNo}", receiptNo);
                return StatusCode(500, "An error occurred while retrieving the payment receipt.");
            }
        }

        /// <summary>
        /// Create new payment receipt
        /// </summary>
        /// <param name="createPaymentReceiptDto">Payment receipt creation data</param>
        /// <returns>Created payment receipt</returns>
        [HttpPost]
        public async Task<IActionResult> CreatePaymentReceipt([FromBody] CreatePaymentReceiptDto createPaymentReceiptDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var paymentReceipt = await _paymentReceiptService.CreatePaymentReceiptAsync(createPaymentReceiptDto);
                return CreatedAtAction(nameof(GetPaymentReceiptById), new { id = paymentReceipt.ReceiptId }, paymentReceipt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment receipt");
                return StatusCode(500, "An error occurred while creating the payment receipt.");
            }
        }

        /// <summary>
        /// Update existing payment receipt
        /// </summary>
        /// <param name="id">Payment receipt ID</param>
        /// <param name="updatePaymentReceiptDto">Payment receipt update data</param>
        /// <returns>Updated payment receipt</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePaymentReceipt(int id, [FromBody] UpdatePaymentReceiptDto updatePaymentReceiptDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var paymentReceipt = await _paymentReceiptService.UpdatePaymentReceiptAsync(id, updatePaymentReceiptDto);
                if (paymentReceipt == null)
                {
                    return NotFound($"Payment receipt with ID {id} not found.");
                }

                return Ok(paymentReceipt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment receipt with ID {PaymentReceiptId}", id);
                return StatusCode(500, "An error occurred while updating the payment receipt.");
            }
        }

        /// <summary>
        /// Delete payment receipt
        /// </summary>
        /// <param name="id">Payment receipt ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaymentReceipt(int id)
        {
            try
            {
                var result = await _paymentReceiptService.DeletePaymentReceiptAsync(id);
                if (!result)
                {
                    return NotFound($"Payment receipt with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment receipt with ID {PaymentReceiptId}", id);
                return StatusCode(500, "An error occurred while deleting the payment receipt.");
            }
        }

        /// <summary>
        /// Get payment receipts by customer ID
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of customer payment receipts</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetPaymentReceiptsByCustomerId(int customerId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByCustomerIdAsync(customerId);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while retrieving customer payment receipts.");
            }
        }

        /// <summary>
        /// Get payment receipts by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of hotel payment receipts</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetPaymentReceiptsByHotelId(int hotelId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByHotelIdAsync(hotelId);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving hotel payment receipts.");
            }
        }

        /// <summary>
        /// Get payment receipts by reservation ID
        /// </summary>
        /// <param name="reservationId">Reservation ID</param>
        /// <returns>List of reservation payment receipts</returns>
        [HttpGet("reservation/{reservationId}")]
        public async Task<IActionResult> GetPaymentReceiptsByReservationId(int reservationId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByReservationIdAsync(reservationId);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts for reservation {ReservationId}", reservationId);
                return StatusCode(500, "An error occurred while retrieving reservation payment receipts.");
            }
        }

        /// <summary>
        /// Get payment receipts by invoice ID
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <returns>List of invoice payment receipts</returns>
        [HttpGet("invoice/{invoiceId}")]
        public async Task<IActionResult> GetPaymentReceiptsByInvoiceId(int invoiceId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByInvoiceIdAsync(invoiceId);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts for invoice {InvoiceId}", invoiceId);
                return StatusCode(500, "An error occurred while retrieving invoice payment receipts.");
            }
        }

        /// <summary>
        /// Get payment receipts by receipt type
        /// </summary>
        /// <param name="receiptType">Receipt type</param>
        /// <returns>List of payment receipts with specified type</returns>
        [HttpGet("type/{receiptType}")]
        public async Task<IActionResult> GetPaymentReceiptsByType(string receiptType)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByTypeAsync(receiptType);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts by type {ReceiptType}", receiptType);
                return StatusCode(500, "An error occurred while retrieving payment receipts by type.");
            }
        }

        /// <summary>
        /// Get payment receipts by payment method
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        /// <returns>List of payment receipts with specified payment method</returns>
        [HttpGet("payment-method/{paymentMethod}")]
        public async Task<IActionResult> GetPaymentReceiptsByPaymentMethod(string paymentMethod)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByPaymentMethodAsync(paymentMethod);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts by payment method {PaymentMethod}", paymentMethod);
                return StatusCode(500, "An error occurred while retrieving payment receipts by payment method.");
            }
        }

        /// <summary>
        /// Get payment receipts by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of payment receipts in date range</returns>
        [HttpGet("date-range")]
        public async Task<IActionResult> GetPaymentReceiptsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByDateRangeAsync(startDate, endDate);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts by date range");
                return StatusCode(500, "An error occurred while retrieving payment receipts by date range.");
            }
        }

        /// <summary>
        /// Get payment receipts by amount range
        /// </summary>
        /// <param name="minAmount">Minimum amount</param>
        /// <param name="maxAmount">Maximum amount</param>
        /// <returns>List of payment receipts in amount range</returns>
        [HttpGet("amount-range")]
        public async Task<IActionResult> GetPaymentReceiptsByAmountRange([FromQuery] decimal minAmount, [FromQuery] decimal maxAmount)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByAmountRangeAsync(minAmount, maxAmount);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts by amount range");
                return StatusCode(500, "An error occurred while retrieving payment receipts by amount range.");
            }
        }

        /// <summary>
        /// Search payment receipts by customer name
        /// </summary>
        /// <param name="customerName">Customer name to search for</param>
        /// <returns>List of matching payment receipts</returns>
        [HttpGet("search/customer")]
        public async Task<IActionResult> SearchPaymentReceiptsByCustomerName([FromQuery] string customerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerName))
                {
                    return BadRequest("Customer name cannot be empty.");
                }

                var paymentReceipts = await _paymentReceiptService.SearchPaymentReceiptsByCustomerNameAsync(customerName);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching payment receipts by customer name {CustomerName}", customerName);
                return StatusCode(500, "An error occurred while searching payment receipts by customer name.");
            }
        }

        /// <summary>
        /// Search payment receipts by hotel name
        /// </summary>
        /// <param name="hotelName">Hotel name to search for</param>
        /// <returns>List of matching payment receipts</returns>
        [HttpGet("search/hotel")]
        public async Task<IActionResult> SearchPaymentReceiptsByHotelName([FromQuery] string hotelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hotelName))
                {
                    return BadRequest("Hotel name cannot be empty.");
                }

                var paymentReceipts = await _paymentReceiptService.SearchPaymentReceiptsByHotelNameAsync(hotelName);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching payment receipts by hotel name {HotelName}", hotelName);
                return StatusCode(500, "An error occurred while searching payment receipts by hotel name.");
            }
        }

        /// <summary>
        /// Search payment receipts by receipt number
        /// </summary>
        /// <param name="receiptNo">Receipt number to search for</param>
        /// <returns>List of matching payment receipts</returns>
        [HttpGet("search/number")]
        public async Task<IActionResult> SearchPaymentReceiptsByNo([FromQuery] string receiptNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(receiptNo))
                {
                    return BadRequest("Receipt number cannot be empty.");
                }

                var paymentReceipts = await _paymentReceiptService.SearchPaymentReceiptsByNoAsync(receiptNo);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching payment receipts by number {ReceiptNo}", receiptNo);
                return StatusCode(500, "An error occurred while searching payment receipts by number.");
            }
        }

        /// <summary>
        /// Search payment receipts by transaction number
        /// </summary>
        /// <param name="transactionNo">Transaction number to search for</param>
        /// <returns>List of matching payment receipts</returns>
        [HttpGet("search/transaction")]
        public async Task<IActionResult> SearchPaymentReceiptsByTransactionNo([FromQuery] string transactionNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transactionNo))
                {
                    return BadRequest("Transaction number cannot be empty.");
                }

                var paymentReceipts = await _paymentReceiptService.SearchPaymentReceiptsByTransactionNoAsync(transactionNo);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching payment receipts by transaction number {TransactionNo}", transactionNo);
                return StatusCode(500, "An error occurred while searching payment receipts by transaction number.");
            }
        }

        /// <summary>
        /// Get payment receipt statistics
        /// </summary>
        /// <returns>Payment receipt statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetPaymentReceiptStatistics()
        {
            try
            {
                var statistics = await _paymentReceiptService.GetPaymentReceiptStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipt statistics");
                return StatusCode(500, "An error occurred while retrieving payment receipt statistics.");
            }
        }

        /// <summary>
        /// Check if receipt number exists
        /// </summary>
        /// <param name="receiptNo">Receipt number to check</param>
        /// <param name="excludeId">Payment receipt ID to exclude from check (for updates)</param>
        /// <returns>True if number exists, false otherwise</returns>
        [HttpGet("check-number")]
        public async Task<IActionResult> CheckReceiptNumber([FromQuery] string receiptNo, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(receiptNo))
                {
                    return BadRequest("Receipt number cannot be empty.");
                }

                var exists = await _paymentReceiptService.ReceiptNoExistsAsync(receiptNo, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking receipt number {ReceiptNo}", receiptNo);
                return StatusCode(500, "An error occurred while checking receipt number.");
            }
        }

        /// <summary>
        /// Get payment receipts by bank ID
        /// </summary>
        /// <param name="bankId">Bank ID</param>
        /// <returns>List of payment receipts for the specified bank</returns>
        [HttpGet("bank/{bankId}")]
        public async Task<IActionResult> GetPaymentReceiptsByBankId(int bankId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByBankIdAsync(bankId);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts for bank {BankId}", bankId);
                return StatusCode(500, "An error occurred while retrieving bank payment receipts.");
            }
        }

        /// <summary>
        /// Get payment receipts by payment method ID
        /// </summary>
        /// <param name="paymentMethodId">Payment method ID</param>
        /// <returns>List of payment receipts for the specified payment method</returns>
        [HttpGet("payment-method-id/{paymentMethodId}")]
        public async Task<IActionResult> GetPaymentReceiptsByPaymentMethodId(int paymentMethodId)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByPaymentMethodIdAsync(paymentMethodId);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts for payment method {PaymentMethodId}", paymentMethodId);
                return StatusCode(500, "An error occurred while retrieving payment method payment receipts.");
            }
        }

        /// <summary>
        /// Get payment receipts by created by user
        /// </summary>
        /// <param name="createdBy">User ID who created the receipts</param>
        /// <returns>List of payment receipts created by the specified user</returns>
        [HttpGet("created-by/{createdBy}")]
        public async Task<IActionResult> GetPaymentReceiptsByCreatedBy(int createdBy)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByCreatedByAsync(createdBy);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts created by user {CreatedBy}", createdBy);
                return StatusCode(500, "An error occurred while retrieving payment receipts by creator.");
            }
        }


        /// <summary>
        /// Get total amount paid by customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Total amount paid by the customer</returns>
        [HttpGet("customer/{customerId}/total-amount")]
        public async Task<IActionResult> GetTotalAmountByCustomer(int customerId)
        {
            try
            {
                var totalAmount = await _paymentReceiptService.GetTotalAmountByCustomerAsync(customerId);
                return Ok(new { CustomerId = customerId, TotalAmount = totalAmount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total amount for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while retrieving total amount for customer.");
            }
        }

        /// <summary>
        /// Get total amount paid by hotel
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>Total amount paid to the hotel</returns>
        [HttpGet("hotel/{hotelId}/total-amount")]
        public async Task<IActionResult> GetTotalAmountByHotel(int hotelId)
        {
            try
            {
                var totalAmount = await _paymentReceiptService.GetTotalAmountByHotelAsync(hotelId);
                return Ok(new { HotelId = hotelId, TotalAmount = totalAmount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total amount for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving total amount for hotel.");
            }
        }

        /// <summary>
        /// Get payment receipts by period range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of payment receipts in period range</returns>
        [HttpGet("period-range")]
        public async Task<IActionResult> GetPaymentReceiptsByPeriodRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var paymentReceipts = await _paymentReceiptService.GetPaymentReceiptsByPeriodRangeAsync(startDate, endDate);
                return Ok(paymentReceipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment receipts by period range");
                return StatusCode(500, "An error occurred while retrieving payment receipts by period range.");
            }
        }

        /// <summary>
        /// Update payment receipt amount
        /// </summary>
        /// <param name="id">Payment receipt ID</param>
        /// <param name="amountPaid">New amount paid</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/amount")]
        public async Task<IActionResult> UpdatePaymentAmount(int id, [FromBody] decimal amountPaid)
        {
            try
            {
                var result = await _paymentReceiptService.UpdatePaymentAmountAsync(id, amountPaid);
                if (!result)
                {
                    return NotFound($"Payment receipt with ID {id} not found.");
                }

                return Ok(new { Message = "Payment amount updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment amount for ID {PaymentReceiptId}", id);
                return StatusCode(500, "An error occurred while updating payment amount.");
            }
        }

        /// <summary>
        /// Update payment receipt transaction number
        /// </summary>
        /// <param name="id">Payment receipt ID</param>
        /// <param name="transactionNo">New transaction number</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/transaction-number")]
        public async Task<IActionResult> UpdateTransactionNumber(int id, [FromBody] string transactionNo)
        {
            try
            {
                var result = await _paymentReceiptService.UpdateTransactionNumberAsync(id, transactionNo);
                if (!result)
                {
                    return NotFound($"Payment receipt with ID {id} not found.");
                }

                return Ok(new { Message = "Transaction number updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction number for ID {PaymentReceiptId}", id);
                return StatusCode(500, "An error occurred while updating transaction number.");
            }
        }

        /// <summary>
        /// Update payment receipt notes
        /// </summary>
        /// <param name="id">Payment receipt ID</param>
        /// <param name="notes">New notes</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/notes")]
        public async Task<IActionResult> UpdateNotes(int id, [FromBody] string notes)
        {
            try
            {
                var result = await _paymentReceiptService.UpdateNotesAsync(id, notes);
                if (!result)
                {
                    return NotFound($"Payment receipt with ID {id} not found.");
                }

                return Ok(new { Message = "Notes updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notes for ID {PaymentReceiptId}", id);
                return StatusCode(500, "An error occurred while updating notes.");
            }
        }
    }
}
