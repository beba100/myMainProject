using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Customer Controller
    ///  Õﬂ„ «·⁄„·«¡
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Get all customers with pagination
        /// «·Õ’Ê· ⁄·Ï Ã„Ì⁄ «·⁄„·«¡ „⁄ «· ’›Õ
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (customers, totalCount) = await _customerService.GetAllCustomersAsync(pageNumber, pageSize, searchTerm);
                
                return Ok(new
                {
                    Customers = customers,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return StatusCode(500, "An error occurred while retrieving customers.");
            }
        }

        /// <summary>
        /// Get customer by ID
        /// «·Õ’Ê· ⁄·Ï «·⁄„Ì· »«·„⁄—›
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> GetCustomer(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(new ApiResponse<CustomerResponseDto>
                    {
                        Success = false,
                        Message = "Customer not found"
                    });
                }

                return Ok(new ApiResponse<CustomerResponseDto>
                {
                    Success = true,
                    Data = customer,
                    Message = "Customer retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with ID {CustomerId}", id);
                return StatusCode(500, new ApiResponse<CustomerResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the customer"
                });
            }
        }

        /// <summary>
        /// Get customer by customer number
        /// «·Õ’Ê· ⁄·Ï «·⁄„Ì· »—ﬁ„ «·⁄„Ì·
        /// </summary>
        [HttpGet("by-number/{customerNo}")]
        public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> GetCustomerByNo(string customerNo)
        {
            try
            {
                var customer = await _customerService.GetCustomerByNoAsync(customerNo);
                if (customer == null)
                {
                    return NotFound(new ApiResponse<CustomerResponseDto>
                    {
                        Success = false,
                        Message = "Customer not found"
                    });
                }

                return Ok(new ApiResponse<CustomerResponseDto>
                {
                    Success = true,
                    Data = customer,
                    Message = "Customer retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with number {CustomerNo}", customerNo);
                return StatusCode(500, new ApiResponse<CustomerResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the customer"
                });
            }
        }

        /// <summary>
        /// Create new customer
        /// ≈‰‘«¡ ⁄„Ì· ÃœÌœ
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> CreateCustomer([FromBody] CreateCustomerDto createCustomerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<CustomerResponseDto>
                    {
                        Success = false,
                        Message = "Invalid model state",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var customer = await _customerService.CreateCustomerAsync(createCustomerDto);

                return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, new ApiResponse<CustomerResponseDto>
                {
                    Success = true,
                    Data = customer,
                    Message = "Customer created successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CustomerResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, new ApiResponse<CustomerResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the customer"
                });
            }
        }

        /// <summary>
        /// Update existing customer
        ///  ÕœÌÀ «·⁄„Ì· «·„ÊÃÊœ
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CustomerResponseDto>>> UpdateCustomer(int id, [FromBody] UpdateCustomerDto updateCustomerDto)
        {
            try
            {
                if (id != updateCustomerDto.CustomerId)
                {
                    return BadRequest(new ApiResponse<CustomerResponseDto>
                    {
                        Success = false,
                        Message = "Customer ID mismatch"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<CustomerResponseDto>
                    {
                        Success = false,
                        Message = "Invalid model state",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var customer = await _customerService.UpdateCustomerAsync(updateCustomerDto);
                if (customer == null)
                {
                    return NotFound(new ApiResponse<CustomerResponseDto>
                    {
                        Success = false,
                        Message = "Customer not found"
                    });
                }

                return Ok(new ApiResponse<CustomerResponseDto>
                {
                    Success = true,
                    Data = customer,
                    Message = "Customer updated successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<CustomerResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with ID {CustomerId}", id);
                return StatusCode(500, new ApiResponse<CustomerResponseDto>
                {
                    Success = false,
                    Message = "An error occurred while updating the customer"
                });
            }
        }

        /// <summary>
        /// Delete customer
        /// Õ–› «·⁄„Ì·
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCustomer(int id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Customer not found"
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Customer deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with ID {CustomerId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the customer"
                });
            }
        }

        /// <summary>
        /// Search customers by name
        /// «·»ÕÀ ⁄‰ «·⁄„·«¡ »«·«”„
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CustomerResponseDto>>>> SearchCustomers([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new ApiResponse<IEnumerable<CustomerResponseDto>>
                    {
                        Success = false,
                        Message = "Search term is required"
                    });
                }

                var customers = await _customerService.SearchCustomersAsync(searchTerm);

                return Ok(new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = true,
                    Data = customers,
                    Message = "Search completed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term {SearchTerm}", searchTerm);
                return StatusCode(500, new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = false,
                    Message = "An error occurred while searching customers"
                });
            }
        }

        /// <summary>
        /// Get customers by nationality
        /// «·Õ’Ê· ⁄·Ï «·⁄„·«¡ »«·Ã‰”Ì…
        /// </summary>
        [HttpGet("by-nationality/{nationalityId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CustomerResponseDto>>>> GetCustomersByNationality(int nationalityId)
        {
            try
            {
                var customers = await _customerService.GetCustomersByNationalityAsync(nationalityId);

                return Ok(new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = true,
                    Data = customers,
                    Message = "Customers retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers by nationality {NationalityId}", nationalityId);
                return StatusCode(500, new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving customers"
                });
            }
        }

        /// <summary>
        /// Get customers by guest type
        /// «·Õ’Ê· ⁄·Ï «·⁄„·«¡ »‰Ê⁄ «·÷Ì›
        /// </summary>
        [HttpGet("by-guest-type/{guestTypeId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CustomerResponseDto>>>> GetCustomersByGuestType(int guestTypeId)
        {
            try
            {
                var customers = await _customerService.GetCustomersByGuestTypeAsync(guestTypeId);

                return Ok(new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = true,
                    Data = customers,
                    Message = "Customers retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers by guest type {GuestTypeId}", guestTypeId);
                return StatusCode(500, new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving customers"
                });
            }
        }

        /// <summary>
        /// Get customers by guest category
        /// «·Õ’Ê· ⁄·Ï «·⁄„·«¡ »›∆… «·÷Ì›
        /// </summary>
        [HttpGet("by-guest-category/{guestCategoryId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CustomerResponseDto>>>> GetCustomersByGuestCategory(int guestCategoryId)
        {
            try
            {
                var customers = await _customerService.GetCustomersByGuestCategoryAsync(guestCategoryId);

                return Ok(new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = true,
                    Data = customers,
                    Message = "Customers retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers by guest category {GuestCategoryId}", guestCategoryId);
                return StatusCode(500, new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving customers"
                });
            }
        }

        /// <summary>
        /// Get customers by date range
        /// «·Õ’Ê· ⁄·Ï «·⁄„·«¡ »‰ÿ«ﬁ «· «—ÌŒ
        /// </summary>
        [HttpGet("by-date-range")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CustomerResponseDto>>>> GetCustomersByDateRange(
            [FromQuery] DateTime fromDate, 
            [FromQuery] DateTime toDate)
        {
            try
            {
                var customers = await _customerService.GetCustomersByDateRangeAsync(fromDate, toDate);

                return Ok(new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = true,
                    Data = customers,
                    Message = "Customers retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers by date range {FromDate} to {ToDate}", fromDate, toDate);
                return StatusCode(500, new ApiResponse<IEnumerable<CustomerResponseDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving customers"
                });
            }
        }

        /// <summary>
        /// Get customer statistics
        /// «·Õ’Ê· ⁄·Ï ≈Õ’«∆Ì«  «·⁄„·«¡
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<object>>> GetCustomerStatistics()
        {
            try
            {
                var statistics = await _customerService.GetCustomerStatisticsAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = statistics,
                    Message = "Statistics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer statistics");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving statistics"
                });
            }
        }
    }

    /// <summary>
    /// API Response Wrapper
    /// €·«› «” Ã«»… API
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? TotalCount { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
