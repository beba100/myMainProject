using Microsoft.AspNetCore.Mvc;
using zaaerIntegration.DTOs.Zaaer;
using zaaerIntegration.Services.Zaaer;
using zaaerIntegration.Services.PartnerQueueing;
using System.Text.Json;

namespace zaaerIntegration.Controllers.Zaaer
{
    /// <summary>
    /// Controller for Zaaer Customer integration endpoints
    /// </summary>
    [ApiController]
    [Route("api/zaaer/[controller]")]
    [Produces("application/json")]
    public class CustomerController : ControllerBase
    {
        private readonly IZaaerCustomerService _zaaerCustomerService;
        private readonly IPartnerQueueService _queueService;
        private readonly IQueueSettingsProvider _queueSettings;

        public CustomerController(IZaaerCustomerService zaaerCustomerService, IPartnerQueueService queueService, IQueueSettingsProvider queueSettings)
        {
            _zaaerCustomerService = zaaerCustomerService;
            _queueService = queueService;
            _queueSettings = queueSettings;
        }

        /// <summary>
        /// Create a new customer via Zaaer integration
        /// </summary>
        /// <param name="createCustomerDto">Customer data</param>
        /// <returns>Created customer</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ZaaerCustomerResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ZaaerCustomerResponseDto>> CreateCustomer([FromBody] ZaaerCreateCustomerDto createCustomerDto)
        {
            try
            {
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = "/api/zaaer/Customer",
                        OperationKey = "Zaaer.Customer.Create",
                        PayloadType = nameof(ZaaerCreateCustomerDto),
                        PayloadJson = JsonSerializer.Serialize(createCustomerDto),
                        HotelId = createCustomerDto.HotelId
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var result = await _zaaerCustomerService.CreateCustomerAsync(createCustomerDto);
                return CreatedAtAction(nameof(GetCustomerById), new { customerId = result.CustomerId }, result);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner: {ex.InnerException.Message}";
                }
                return BadRequest($"Error creating customer: {errorMessage}");
            }
        }

        /// <summary>
        /// Update an existing customer via Zaaer integration
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="updateCustomerDto">Updated customer data</param>
        /// <returns>Updated customer</returns>
        [HttpPut("{customerId}")]
        [ProducesResponseType(typeof(ZaaerCustomerResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ZaaerCustomerResponseDto>> UpdateCustomer(int customerId, [FromBody] ZaaerUpdateCustomerDto updateCustomerDto)
        {
            try
            {
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = $"/api/zaaer/Customer/{customerId}",
                        OperationKey = "Zaaer.Customer.UpdateById",
                        TargetId = customerId,
                        PayloadType = nameof(ZaaerUpdateCustomerDto),
                        PayloadJson = JsonSerializer.Serialize(updateCustomerDto),
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                // Try by ZaaerId first, then fallback to internal ID
                ZaaerCustomerResponseDto result;
                try
                {
                    result = await _zaaerCustomerService.UpdateCustomerByZaaerIdAsync(customerId, updateCustomerDto);
                }
                catch (ArgumentException)
                {
                    result = await _zaaerCustomerService.UpdateCustomerAsync(customerId, updateCustomerDto);
                }
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing customer via customer number
        /// </summary>
        /// <param name="customerNo">Customer Number</param>
        /// <param name="updateCustomerDto">Updated customer data</param>
        /// <returns>Updated customer</returns>
        [HttpPut("number/{customerNo}")]
        [ProducesResponseType(typeof(ZaaerCustomerResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ZaaerCustomerResponseDto>> UpdateCustomerByNumber(string customerNo, [FromBody] ZaaerUpdateCustomerDto updateCustomerDto)
        {
            try
            {
                var queueSettings = _queueSettings.GetSettings();
                if (queueSettings.EnableQueueMode)
                {
                    var dtoQ = new EnqueuePartnerRequestDto
                    {
                        Partner = queueSettings.DefaultPartner,
                        Operation = $"/api/zaaer/Customer/number/{customerNo}",
                        OperationKey = "Zaaer.Customer.UpdateByNumber",
                        PayloadType = nameof(ZaaerUpdateCustomerDto),
                        PayloadJson = JsonSerializer.Serialize(updateCustomerDto)
                    };
                    await _queueService.EnqueueAsync(dtoQ);
                    return Accepted(new { queued = true, requestRef = dtoQ.RequestRef });
                }
                var result = await _zaaerCustomerService.UpdateCustomerByNumberAsync(customerNo, updateCustomerDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating customer by number: {ex.Message}");
            }
        }

        /// <summary>
        /// Get customer by ID via Zaaer integration
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Customer data</returns>
        [HttpGet("{customerId}")]
        [ProducesResponseType(typeof(ZaaerCustomerResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ZaaerCustomerResponseDto>> GetCustomerById(int customerId)
        {
            try
            {
                var result = await _zaaerCustomerService.GetCustomerByIdAsync(customerId);
                if (result == null)
                {
                    return NotFound($"Customer with ID {customerId} not found.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving customer: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all customers for a hotel via Zaaer integration
        /// </summary>
        /// <param name="hotelId">Hotel ID</param>
        /// <returns>List of customers</returns>
        [HttpGet("hotel/{hotelId}")]
        [ProducesResponseType(typeof(IEnumerable<ZaaerCustomerResponseDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<ZaaerCustomerResponseDto>>> GetCustomersByHotel(int hotelId)
        {
            try
            {
                var result = await _zaaerCustomerService.GetAllCustomersAsync(hotelId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving customers: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete customer via Zaaer integration
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{customerId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteCustomer(int customerId)
        {
            try
            {
                var result = await _zaaerCustomerService.DeleteCustomerAsync(customerId);
                if (!result)
                {
                    return NotFound($"Customer with ID {customerId} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting customer: {ex.Message}");
            }
        }
    }
}
