using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.Services.Interfaces;

namespace zaaerIntegration.Controllers
{
    /// <summary>
    /// Controller for CorporateCustomer operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CorporateCustomerController : ControllerBase
    {
        private readonly ICorporateCustomerService _corporateCustomerService;
        private readonly ILogger<CorporateCustomerController> _logger;

        /// <summary>
        /// Initializes a new instance of the CorporateCustomerController class
        /// </summary>
        /// <param name="corporateCustomerService">CorporateCustomer service</param>
        /// <param name="logger">Logger</param>
        public CorporateCustomerController(ICorporateCustomerService corporateCustomerService, ILogger<CorporateCustomerController> logger)
        {
            _corporateCustomerService = corporateCustomerService;
            _logger = logger;
        }

        /// <summary>
        /// Get all corporate customers with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for corporate name, email, contact person, or notes</param>
        /// <returns>List of corporate customers with total count</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCorporateCustomers(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10, 
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var (corporateCustomers, totalCount) = await _corporateCustomerService.GetAllCorporateCustomersAsync(pageNumber, pageSize, searchTerm);
                
                return Ok(new
                {
                    CorporateCustomers = corporateCustomers,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers");
                return StatusCode(500, "An error occurred while retrieving corporate customers.");
            }
        }

        /// <summary>
        /// Get corporate customer by ID
        /// </summary>
        /// <param name="id">Corporate customer ID</param>
        /// <returns>Corporate customer details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCorporateCustomerById(int id)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerService.GetCorporateCustomerByIdAsync(id);
                if (corporateCustomer == null)
                {
                    return NotFound($"Corporate customer with ID {id} not found.");
                }

                return Ok(corporateCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customer with ID {CorporateCustomerId}", id);
                return StatusCode(500, "An error occurred while retrieving the corporate customer.");
            }
        }

        /// <summary>
        /// Get corporate customer by corporate name
        /// </summary>
        /// <param name="corporateName">Corporate name</param>
        /// <returns>Corporate customer details</returns>
        [HttpGet("name/{corporateName}")]
        public async Task<IActionResult> GetCorporateCustomerByName(string corporateName)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerService.GetCorporateCustomerByNameAsync(corporateName);
                if (corporateCustomer == null)
                {
                    return NotFound($"Corporate customer with name '{corporateName}' not found.");
                }

                return Ok(corporateCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customer with name {CorporateName}", corporateName);
                return StatusCode(500, "An error occurred while retrieving the corporate customer.");
            }
        }

        /// <summary>
        /// Create new corporate customer
        /// </summary>
        /// <param name="createCorporateCustomerDto">Corporate customer creation data</param>
        /// <returns>Created corporate customer</returns>
        [HttpPost]
        public async Task<IActionResult> CreateCorporateCustomer([FromBody] CreateCorporateCustomerDto createCorporateCustomerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var corporateCustomer = await _corporateCustomerService.CreateCorporateCustomerAsync(createCorporateCustomerDto);
                return CreatedAtAction(nameof(GetCorporateCustomerById), new { id = corporateCustomer.CorporateId }, corporateCustomer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating corporate customer");
                return StatusCode(500, "An error occurred while creating the corporate customer.");
            }
        }

        /// <summary>
        /// Update existing corporate customer
        /// </summary>
        /// <param name="id">Corporate customer ID</param>
        /// <param name="updateCorporateCustomerDto">Corporate customer update data</param>
        /// <returns>Updated corporate customer</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCorporateCustomer(int id, [FromBody] UpdateCorporateCustomerDto updateCorporateCustomerDto)
        {
            try
            {
                if (id != updateCorporateCustomerDto.CorporateId)
                {
                    return BadRequest("Corporate customer ID in URL does not match ID in body.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var corporateCustomer = await _corporateCustomerService.UpdateCorporateCustomerAsync(id, updateCorporateCustomerDto);
                if (corporateCustomer == null)
                {
                    return NotFound($"Corporate customer with ID {id} not found.");
                }

                return Ok(corporateCustomer);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating corporate customer with ID {CorporateCustomerId}", id);
                return StatusCode(500, "An error occurred while updating the corporate customer.");
            }
        }

        /// <summary>
        /// Delete corporate customer
        /// </summary>
        /// <param name="id">Corporate customer ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCorporateCustomer(int id)
        {
            try
            {
                var result = await _corporateCustomerService.DeleteCorporateCustomerAsync(id);
                if (!result)
                {
                    return NotFound($"Corporate customer with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting corporate customer with ID {CorporateCustomerId}", id);
                return StatusCode(500, "An error occurred while deleting the corporate customer.");
            }
        }

        /// <summary>
        /// Get corporate customers by hotel ID
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of hotel corporate customers</returns>
        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetCorporateCustomersByHotelId(int hotelId)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByHotelIdAsync(hotelId);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers for hotel {HotelId}", hotelId);
                return StatusCode(500, "An error occurred while retrieving hotel corporate customers.");
            }
        }

        /// <summary>
        /// Get corporate customers by country
        /// </summary>
        /// <param name="country">Country</param>
        /// <returns>List of corporate customers in the specified country</returns>
        [HttpGet("country/{country}")]
        public async Task<IActionResult> GetCorporateCustomersByCountry(string country)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByCountryAsync(country);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by country {Country}", country);
                return StatusCode(500, "An error occurred while retrieving corporate customers by country.");
            }
        }

        /// <summary>
        /// Get corporate customers by city
        /// </summary>
        /// <param name="city">City</param>
        /// <returns>List of corporate customers in the specified city</returns>
        [HttpGet("city/{city}")]
        public async Task<IActionResult> GetCorporateCustomersByCity(string city)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByCityAsync(city);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by city {City}", city);
                return StatusCode(500, "An error occurred while retrieving corporate customers by city.");
            }
        }

        /// <summary>
        /// Get corporate customer by VAT registration number
        /// </summary>
        /// <param name="vatRegistrationNo">VAT registration number</param>
        /// <returns>Corporate customer details</returns>
        [HttpGet("vat/{vatRegistrationNo}")]
        public async Task<IActionResult> GetCorporateCustomerByVatRegistrationNo(string vatRegistrationNo)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerService.GetCorporateCustomerByVatRegistrationNoAsync(vatRegistrationNo);
                if (corporateCustomer == null)
                {
                    return NotFound($"Corporate customer with VAT registration number '{vatRegistrationNo}' not found.");
                }

                return Ok(corporateCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customer by VAT registration number {VatRegistrationNo}", vatRegistrationNo);
                return StatusCode(500, "An error occurred while retrieving the corporate customer.");
            }
        }

        /// <summary>
        /// Get corporate customer by commercial registration number
        /// </summary>
        /// <param name="commercialRegistrationNo">Commercial registration number</param>
        /// <returns>Corporate customer details</returns>
        [HttpGet("commercial/{commercialRegistrationNo}")]
        public async Task<IActionResult> GetCorporateCustomerByCommercialRegistrationNo(string commercialRegistrationNo)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerService.GetCorporateCustomerByCommercialRegistrationNoAsync(commercialRegistrationNo);
                if (corporateCustomer == null)
                {
                    return NotFound($"Corporate customer with commercial registration number '{commercialRegistrationNo}' not found.");
                }

                return Ok(corporateCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customer by commercial registration number {CommercialRegistrationNo}", commercialRegistrationNo);
                return StatusCode(500, "An error occurred while retrieving the corporate customer.");
            }
        }

        /// <summary>
        /// Get corporate customers by contact person name
        /// </summary>
        /// <param name="contactPersonName">Contact person name</param>
        /// <returns>List of corporate customers with the specified contact person</returns>
        [HttpGet("contact-person/{contactPersonName}")]
        public async Task<IActionResult> GetCorporateCustomersByContactPersonName(string contactPersonName)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByContactPersonNameAsync(contactPersonName);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by contact person name {ContactPersonName}", contactPersonName);
                return StatusCode(500, "An error occurred while retrieving corporate customers by contact person name.");
            }
        }

        /// <summary>
        /// Get corporate customer by email
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>Corporate customer details</returns>
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetCorporateCustomerByEmail(string email)
        {
            try
            {
                var corporateCustomer = await _corporateCustomerService.GetCorporateCustomerByEmailAsync(email);
                if (corporateCustomer == null)
                {
                    return NotFound($"Corporate customer with email '{email}' not found.");
                }

                return Ok(corporateCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customer by email {Email}", email);
                return StatusCode(500, "An error occurred while retrieving the corporate customer.");
            }
        }

        /// <summary>
        /// Get corporate customers by phone
        /// </summary>
        /// <param name="phone">Phone number</param>
        /// <returns>List of corporate customers with the specified phone number</returns>
        [HttpGet("phone/{phone}")]
        public async Task<IActionResult> GetCorporateCustomersByPhone(string phone)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByPhoneAsync(phone);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by phone {Phone}", phone);
                return StatusCode(500, "An error occurred while retrieving corporate customers by phone.");
            }
        }

        /// <summary>
        /// Get active corporate customers
        /// </summary>
        /// <returns>List of active corporate customers</returns>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCorporateCustomers()
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetActiveCorporateCustomersAsync();
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active corporate customers");
                return StatusCode(500, "An error occurred while retrieving active corporate customers.");
            }
        }

        /// <summary>
        /// Get inactive corporate customers
        /// </summary>
        /// <returns>List of inactive corporate customers</returns>
        [HttpGet("inactive")]
        public async Task<IActionResult> GetInactiveCorporateCustomers()
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetInactiveCorporateCustomersAsync();
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inactive corporate customers");
                return StatusCode(500, "An error occurred while retrieving inactive corporate customers.");
            }
        }

        /// <summary>
        /// Search corporate customers by name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>List of matching corporate customers</returns>
        [HttpGet("search/name")]
        public async Task<IActionResult> SearchCorporateCustomersByName([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Name cannot be empty.");
                }

                var corporateCustomers = await _corporateCustomerService.SearchCorporateCustomersByNameAsync(name);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching corporate customers by name {Name}", name);
                return StatusCode(500, "An error occurred while searching corporate customers by name.");
            }
        }

        /// <summary>
        /// Search corporate customers by Arabic name
        /// </summary>
        /// <param name="nameAr">Arabic name to search for</param>
        /// <returns>List of matching corporate customers</returns>
        [HttpGet("search/name-ar")]
        public async Task<IActionResult> SearchCorporateCustomersByNameAr([FromQuery] string nameAr)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nameAr))
                {
                    return BadRequest("Arabic name cannot be empty.");
                }

                var corporateCustomers = await _corporateCustomerService.SearchCorporateCustomersByNameArAsync(nameAr);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching corporate customers by Arabic name {NameAr}", nameAr);
                return StatusCode(500, "An error occurred while searching corporate customers by Arabic name.");
            }
        }

        /// <summary>
        /// Get corporate customers with discount
        /// </summary>
        /// <returns>List of corporate customers with discount</returns>
        [HttpGet("with-discount")]
        public async Task<IActionResult> GetCorporateCustomersWithDiscount()
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersWithDiscountAsync();
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers with discount");
                return StatusCode(500, "An error occurred while retrieving corporate customers with discount.");
            }
        }

        /// <summary>
        /// Get corporate customers by discount method
        /// </summary>
        /// <param name="discountMethod">Discount method</param>
        /// <returns>List of corporate customers with the specified discount method</returns>
        [HttpGet("discount-method/{discountMethod}")]
        public async Task<IActionResult> GetCorporateCustomersByDiscountMethod(string discountMethod)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByDiscountMethodAsync(discountMethod);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by discount method {DiscountMethod}", discountMethod);
                return StatusCode(500, "An error occurred while retrieving corporate customers by discount method.");
            }
        }

        /// <summary>
        /// Get corporate customers by discount value range
        /// </summary>
        /// <param name="minValue">Minimum discount value</param>
        /// <param name="maxValue">Maximum discount value</param>
        /// <returns>List of corporate customers in the specified discount value range</returns>
        [HttpGet("discount-range")]
        public async Task<IActionResult> GetCorporateCustomersByDiscountValueRange([FromQuery] decimal minValue, [FromQuery] decimal maxValue)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByDiscountValueRangeAsync(minValue, maxValue);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by discount value range");
                return StatusCode(500, "An error occurred while retrieving corporate customers by discount value range.");
            }
        }

        /// <summary>
        /// Get corporate customer statistics
        /// </summary>
        /// <returns>Corporate customer statistics</returns>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetCorporateCustomerStatistics()
        {
            try
            {
                var statistics = await _corporateCustomerService.GetCorporateCustomerStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customer statistics");
                return StatusCode(500, "An error occurred while retrieving corporate customer statistics.");
            }
        }

        /// <summary>
        /// Check if corporate name exists
        /// </summary>
        /// <param name="corporateName">Corporate name to check</param>
        /// <param name="excludeId">Corporate customer ID to exclude from check (for updates)</param>
        /// <returns>True if name exists, false otherwise</returns>
        [HttpGet("check-name")]
        public async Task<IActionResult> CheckCorporateName([FromQuery] string corporateName, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(corporateName))
                {
                    return BadRequest("Corporate name cannot be empty.");
                }

                var exists = await _corporateCustomerService.CorporateNameExistsAsync(corporateName, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking corporate name {CorporateName}", corporateName);
                return StatusCode(500, "An error occurred while checking corporate name.");
            }
        }

        /// <summary>
        /// Check if VAT registration number exists
        /// </summary>
        /// <param name="vatRegistrationNo">VAT registration number to check</param>
        /// <param name="excludeId">Corporate customer ID to exclude from check (for updates)</param>
        /// <returns>True if number exists, false otherwise</returns>
        [HttpGet("check-vat")]
        public async Task<IActionResult> CheckVatRegistrationNo([FromQuery] string vatRegistrationNo, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(vatRegistrationNo))
                {
                    return BadRequest("VAT registration number cannot be empty.");
                }

                var exists = await _corporateCustomerService.VatRegistrationNoExistsAsync(vatRegistrationNo, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking VAT registration number {VatRegistrationNo}", vatRegistrationNo);
                return StatusCode(500, "An error occurred while checking VAT registration number.");
            }
        }

        /// <summary>
        /// Check if commercial registration number exists
        /// </summary>
        /// <param name="commercialRegistrationNo">Commercial registration number to check</param>
        /// <param name="excludeId">Corporate customer ID to exclude from check (for updates)</param>
        /// <returns>True if number exists, false otherwise</returns>
        [HttpGet("check-commercial")]
        public async Task<IActionResult> CheckCommercialRegistrationNo([FromQuery] string commercialRegistrationNo, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(commercialRegistrationNo))
                {
                    return BadRequest("Commercial registration number cannot be empty.");
                }

                var exists = await _corporateCustomerService.CommercialRegistrationNoExistsAsync(commercialRegistrationNo, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking commercial registration number {CommercialRegistrationNo}", commercialRegistrationNo);
                return StatusCode(500, "An error occurred while checking commercial registration number.");
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <param name="excludeId">Corporate customer ID to exclude from check (for updates)</param>
        /// <returns>True if email exists, false otherwise</returns>
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest("Email cannot be empty.");
                }

                var exists = await _corporateCustomerService.EmailExistsAsync(email, excludeId);
                return Ok(new { Exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email {Email}", email);
                return StatusCode(500, "An error occurred while checking email.");
            }
        }

        /// <summary>
        /// Get corporate customers by date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>List of corporate customers created in the specified date range</returns>
        [HttpGet("date-range")]
        public async Task<IActionResult> GetCorporateCustomersByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByDateRangeAsync(startDate, endDate);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by date range");
                return StatusCode(500, "An error occurred while retrieving corporate customers by date range.");
            }
        }

        /// <summary>
        /// Get corporate customers by created date
        /// </summary>
        /// <param name="createdDate">Created date</param>
        /// <returns>List of corporate customers created on the specified date</returns>
        [HttpGet("created-date")]
        public async Task<IActionResult> GetCorporateCustomersByCreatedDate([FromQuery] DateTime createdDate)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByCreatedDateAsync(createdDate);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by created date");
                return StatusCode(500, "An error occurred while retrieving corporate customers by created date.");
            }
        }

        /// <summary>
        /// Get corporate customers with reservations
        /// </summary>
        /// <returns>List of corporate customers that have reservations</returns>
        [HttpGet("with-reservations")]
        public async Task<IActionResult> GetCorporateCustomersWithReservations()
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersWithReservationsAsync();
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers with reservations");
                return StatusCode(500, "An error occurred while retrieving corporate customers with reservations.");
            }
        }

        /// <summary>
        /// Get corporate customers by reservation count range
        /// </summary>
        /// <param name="minCount">Minimum reservation count</param>
        /// <param name="maxCount">Maximum reservation count</param>
        /// <returns>List of corporate customers with reservation count in the specified range</returns>
        [HttpGet("reservation-count-range")]
        public async Task<IActionResult> GetCorporateCustomersByReservationCountRange([FromQuery] int minCount, [FromQuery] int maxCount)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByReservationCountRangeAsync(minCount, maxCount);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by reservation count range");
                return StatusCode(500, "An error occurred while retrieving corporate customers by reservation count range.");
            }
        }

        /// <summary>
        /// Get top corporate customers by reservation count
        /// </summary>
        /// <param name="topCount">Number of top corporate customers to return (default: 10)</param>
        /// <returns>List of top corporate customers by reservation count</returns>
        [HttpGet("top-by-reservations")]
        public async Task<IActionResult> GetTopCorporateCustomersByReservationCount([FromQuery] int topCount = 10)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetTopCorporateCustomersByReservationCountAsync(topCount);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top corporate customers by reservation count");
                return StatusCode(500, "An error occurred while retrieving top corporate customers by reservation count.");
            }
        }

        /// <summary>
        /// Get corporate customers by postal code
        /// </summary>
        /// <param name="postalCode">Postal code</param>
        /// <returns>List of corporate customers with the specified postal code</returns>
        [HttpGet("postal-code/{postalCode}")]
        public async Task<IActionResult> GetCorporateCustomersByPostalCode(string postalCode)
        {
            try
            {
                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByPostalCodeAsync(postalCode);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by postal code {PostalCode}", postalCode);
                return StatusCode(500, "An error occurred while retrieving corporate customers by postal code.");
            }
        }

        /// <summary>
        /// Get corporate customers by address
        /// </summary>
        /// <param name="address">Address to search for</param>
        /// <returns>List of corporate customers with addresses containing the specified text</returns>
        [HttpGet("address")]
        public async Task<IActionResult> GetCorporateCustomersByAddress([FromQuery] string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    return BadRequest("Address cannot be empty.");
                }

                var corporateCustomers = await _corporateCustomerService.GetCorporateCustomersByAddressAsync(address);
                return Ok(corporateCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving corporate customers by address {Address}", address);
                return StatusCode(500, "An error occurred while retrieving corporate customers by address.");
            }
        }

        /// <summary>
        /// Update corporate customer status
        /// </summary>
        /// <param name="id">Corporate customer ID</param>
        /// <param name="isActive">New active status</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateCorporateCustomerStatus(int id, [FromBody] bool isActive)
        {
            try
            {
                var result = await _corporateCustomerService.UpdateCorporateCustomerStatusAsync(id, isActive);
                if (!result)
                {
                    return NotFound($"Corporate customer with ID {id} not found.");
                }

                return Ok(new { Message = "Corporate customer status updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating corporate customer status for ID {CorporateCustomerId}", id);
                return StatusCode(500, "An error occurred while updating corporate customer status.");
            }
        }

        /// <summary>
        /// Update corporate customer discount
        /// </summary>
        /// <param name="id">Corporate customer ID</param>
        /// <param name="discountMethod">New discount method</param>
        /// <param name="discountValue">New discount value</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/discount")]
        public async Task<IActionResult> UpdateCorporateCustomerDiscount(int id, [FromQuery] string? discountMethod, [FromQuery] decimal? discountValue)
        {
            try
            {
                var result = await _corporateCustomerService.UpdateCorporateCustomerDiscountAsync(id, discountMethod, discountValue);
                if (!result)
                {
                    return NotFound($"Corporate customer with ID {id} not found.");
                }

                return Ok(new { Message = "Corporate customer discount updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating corporate customer discount for ID {CorporateCustomerId}", id);
                return StatusCode(500, "An error occurred while updating corporate customer discount.");
            }
        }

        /// <summary>
        /// Update corporate customer contact information
        /// </summary>
        /// <param name="id">Corporate customer ID</param>
        /// <param name="contactPersonName">New contact person name</param>
        /// <param name="contactPersonPhone">New contact person phone</param>
        /// <param name="email">New email</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id}/contact")]
        public async Task<IActionResult> UpdateCorporateCustomerContact(int id, [FromQuery] string? contactPersonName, [FromQuery] string? contactPersonPhone, [FromQuery] string? email)
        {
            try
            {
                var result = await _corporateCustomerService.UpdateCorporateCustomerContactAsync(id, contactPersonName, contactPersonPhone, email);
                if (!result)
                {
                    return NotFound($"Corporate customer with ID {id} not found.");
                }

                return Ok(new { Message = "Corporate customer contact information updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating corporate customer contact information for ID {CorporateCustomerId}", id);
                return StatusCode(500, "An error occurred while updating corporate customer contact information.");
            }
        }
    }
}
