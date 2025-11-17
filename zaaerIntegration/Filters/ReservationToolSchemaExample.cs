using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Filters
{
    /// <summary>
    /// Provides Swagger examples for Reservation Tools DTOs to match the frontend payloads
    /// </summary>
    public class ReservationToolSchemaExample : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(ZaaerCreateReservationDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["zaaerId"] = new OpenApiNull(),
                    ["reservationNo"] = new OpenApiString("RES-002"),
                    ["hotelId"] = new OpenApiInteger(1),
                    ["customerId"] = new OpenApiInteger(1),
                    ["reservationDate"] = new OpenApiString("2025-01-15T10:00:00Z"),
                    ["rentalType"] = new OpenApiString("daily"),
                    ["numberOfMonths"] = new OpenApiNull(),
                    ["totalPenalties"] = new OpenApiInteger(0),
                    ["totalDiscounts"] = new OpenApiInteger(0),
                    ["subtotal"] = new OpenApiDouble(169.68),
                    ["totalTaxAmount"] = new OpenApiDouble(30.32),
                    ["totalAmount"] = new OpenApiDouble(200.00),
                    ["reservationType"] = new OpenApiString("individual"),
                    ["visitPurposeId"] = new OpenApiInteger(1),
                    ["corporateId"] = new OpenApiNull(),
                    ["isAutoExtend"] = new OpenApiBoolean(false),
                    ["priceTypeId"] = new OpenApiInteger(1),
                    ["reservationUnits"] = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["reservationId"] = new OpenApiInteger(0),
                            ["apartmentId"] = new OpenApiInteger(3105),
                            ["checkInDate"] = new OpenApiString("2025-01-15T14:00:00Z"),
                            ["checkOutDate"] = new OpenApiString("2025-01-16T11:00:00Z"),
                            ["rentAmount"] = new OpenApiDouble(84.84),
                            ["dayRates"] = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["nightDate"] = new OpenApiString("2025-01-15T00:00:00Z"),
                                    ["grossRate"] = new OpenApiDouble(100.00),
                                    ["vatAmount"] = new OpenApiDouble(13.04),
                                    ["netAmount"] = new OpenApiDouble(84.84)
                                }
                            }
                        },
                        new OpenApiObject
                        {
                            ["reservationId"] = new OpenApiInteger(0),
                            ["apartmentId"] = new OpenApiInteger(3102),
                            ["checkInDate"] = new OpenApiString("2025-01-15T14:00:00Z"),
                            ["checkOutDate"] = new OpenApiString("2025-01-16T11:00:00Z"),
                            ["rentAmount"] = new OpenApiDouble(84.84),
                            ["dayRates"] = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["nightDate"] = new OpenApiString("2025-01-15T00:00:00Z"),
                                    ["grossRate"] = new OpenApiDouble(100.00),
                                    ["vatAmount"] = new OpenApiDouble(13.04),
                                    ["netAmount"] = new OpenApiDouble(84.84)
                                }
                            }
                        }
                    }
                };
            }

            if (context.Type == typeof(ZaaerCreateReservationToolDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["reservationNo"] = new OpenApiString("RES-TOOLS-001"),
                    ["hotelId"] = new OpenApiInteger(1),
                    ["customerId"] = new OpenApiInteger(1),
                    ["reservationDate"] = new OpenApiString("2025-01-15T10:00:00Z"),
                    ["rentalType"] = new OpenApiString("daily"),
                    ["numberOfMonths"] = new OpenApiNull(),
                    ["totalPenalties"] = new OpenApiInteger(0),
                    ["totalDiscounts"] = new OpenApiInteger(0),
                    ["corporateId"] = new OpenApiNull(),
                    ["reservationUnits"] = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["reservationId"] = new OpenApiInteger(0),
                            ["apartmentId"] = new OpenApiInteger(3101),
                            ["checkInDate"] = new OpenApiString("2025-01-15T14:00:00Z"),
                            ["checkOutDate"] = new OpenApiString("2025-01-18T11:00:00Z"),
                            ["rentAmount"] = new OpenApiDouble(1500.00)
                        },
                        new OpenApiObject
                        {
                            ["reservationId"] = new OpenApiInteger(0),
                            ["apartmentId"] = new OpenApiInteger(3102),
                            ["checkInDate"] = new OpenApiString("2025-01-15T14:00:00Z"),
                            ["checkOutDate"] = new OpenApiString("2025-01-18T11:00:00Z"),
                            ["rentAmount"] = new OpenApiDouble(1200.00)
                        }
                    }
                };
            }

            if (context.Type == typeof(ZaaerUpdateReservationDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["zaaerId"] = new OpenApiNull(),
                    ["reservationNo"] = new OpenApiString("RES003-UPDATED"),
                    ["hotelId"] = new OpenApiInteger(1),
                    ["customerId"] = new OpenApiInteger(1),
                    ["reservationDate"] = new OpenApiString("2025-01-15T10:00:00Z"),
                    ["rentalType"] = new OpenApiString("daily"),
                    ["numberOfMonths"] = new OpenApiNull(),
                    ["totalPenalties"] = new OpenApiInteger(10),
                    ["totalDiscounts"] = new OpenApiInteger(150),
                    ["subtotal"] = new OpenApiDouble(169.68),
                    ["totalTaxAmount"] = new OpenApiDouble(30.32),
                    ["totalAmount"] = new OpenApiDouble(200.00),
                    ["reservationType"] = new OpenApiString("individual"),
                    ["visitPurposeId"] = new OpenApiInteger(1),
                    ["corporateId"] = new OpenApiNull(),
                    ["isAutoExtend"] = new OpenApiBoolean(false),
                    ["priceTypeId"] = new OpenApiInteger(1),
                    ["reservationUnits"] = new OpenApiArray
                    {
                        new OpenApiObject
                        {
                            ["reservationId"] = new OpenApiInteger(0),
                            ["apartmentId"] = new OpenApiInteger(3105),
                            ["checkInDate"] = new OpenApiString("2025-01-15T14:00:00Z"),
                            ["checkOutDate"] = new OpenApiString("2025-01-16T11:00:00Z"),
                            ["rentAmount"] = new OpenApiDouble(84.84),
                            ["dayRates"] = new OpenApiArray
                            {
                                new OpenApiObject
                                {
                                    ["nightDate"] = new OpenApiString("2025-01-15T00:00:00Z"),
                                    ["grossRate"] = new OpenApiDouble(100.00),
                                    ["vatAmount"] = new OpenApiDouble(13.04),
                                    ["netAmount"] = new OpenApiDouble(84.84)
                                }
                            }
                        }
                    }
                };
            }

            if (context.Type == typeof(ZaaerReservationUnitToolDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["reservationId"] = new OpenApiInteger(0),
                    ["apartmentId"] = new OpenApiInteger(3105),
                    ["checkInDate"] = new OpenApiString("2025-01-15T14:00:00Z"),
                    ["checkOutDate"] = new OpenApiString("2025-01-18T11:00:00Z"),
                    ["rentAmount"] = new OpenApiDouble(1500.00)
                };
            }
        }
    }
}


