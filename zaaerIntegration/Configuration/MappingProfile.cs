using AutoMapper;
using FinanceLedgerAPI.Models;
using zaaerIntegration.DTOs.Request;
using zaaerIntegration.DTOs.Response;
using zaaerIntegration.DTOs.Zaaer;

namespace zaaerIntegration.Configuration
{
    /// <summary>
    /// AutoMapper Profile
    /// ��� AutoMapper
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Customer mappings
            CreateMap<Customer, CustomerResponseDto>()
                .ForMember(dest => dest.GuestTypeName, opt => opt.MapFrom(src => src.GuestType != null ? src.GuestType.GtypeName : null))
                .ForMember(dest => dest.NationalityName, opt => opt.MapFrom(src => src.Nationality != null ? src.Nationality.NName : null))
                .ForMember(dest => dest.GuestCategoryName, opt => opt.MapFrom(src => src.GuestCategory != null ? src.GuestCategory.GcName : null));

            CreateMap<CreateCustomerDto, Customer>()
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.EnteredBy, opt => opt.Ignore())
                .ForMember(dest => dest.EnteredAt, opt => opt.Ignore())
                .ForMember(dest => dest.GuestType, opt => opt.Ignore())
                .ForMember(dest => dest.Nationality, opt => opt.Ignore())
                .ForMember(dest => dest.GuestCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Identifications, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerAccounts, opt => opt.Ignore());

            CreateMap<UpdateCustomerDto, Customer>()
                .ForMember(dest => dest.EnteredBy, opt => opt.Ignore())
                .ForMember(dest => dest.EnteredAt, opt => opt.Ignore())
                .ForMember(dest => dest.GuestType, opt => opt.Ignore())
                .ForMember(dest => dest.Nationality, opt => opt.Ignore())
                .ForMember(dest => dest.GuestCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Identifications, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerAccounts, opt => opt.Ignore());

            // HotelSettings Settings mappings
            CreateMap<HotelSettings, HotelResponseDto>();
            CreateMap<CreateHotelDto, HotelSettings>()
                .ForMember(dest => dest.HotelId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Buildings, opt => opt.Ignore())
                .ForMember(dest => dest.Customers, opt => opt.Ignore())
                .ForMember(dest => dest.Reservations, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.RoomTypes, opt => opt.Ignore());

            CreateMap<UpdateHotelDto, HotelSettings>()
                .ForMember(dest => dest.HotelId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Buildings, opt => opt.Ignore())
                .ForMember(dest => dest.Customers, opt => opt.Ignore())
                .ForMember(dest => dest.Reservations, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.RoomTypes, opt => opt.Ignore());

            // Reservation mappings
            CreateMap<Reservation, ReservationResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusEnum))
                .ForMember(dest => dest.StatusWord, opt => opt.MapFrom(src => src.StatusWord))
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.HotelSettings != null ? src.HotelSettings.HotelName : null))
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore()) // Will be populated by service
                .ForMember(dest => dest.CorporateName, opt => opt.MapFrom(src => src.CorporateCustomer != null ? src.CorporateCustomer.CorporateName : null))
                .ForMember(dest => dest.VisitPurposeName, opt => opt.MapFrom(src => src.VisitPurpose != null ? src.VisitPurpose.VpName : null));

            CreateMap<CreateReservationDto, Reservation>()
                .ForMember(dest => dest.ReservationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.CorporateCustomer, opt => opt.Ignore())
                .ForMember(dest => dest.VisitPurpose, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnits, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore());

            CreateMap<UpdateReservationDto, Reservation>()
                .ForMember(dest => dest.ReservationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.CorporateCustomer, opt => opt.Ignore())
                .ForMember(dest => dest.VisitPurpose, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnits, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore());

            // Invoice mappings
            CreateMap<Invoice, InvoiceResponseDto>()
                .ForMember(dest => dest.HotelName, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore());

            CreateMap<CreateInvoiceDto, Invoice>()
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnit, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore());

            CreateMap<UpdateInvoiceDto, Invoice>()
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.InvoiceDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnit, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore());

            // Payment Receipt mappings
            CreateMap<PaymentReceipt, PaymentReceiptResponseDto>()
                .ForMember(dest => dest.HotelName, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethodName, opt => opt.Ignore())
                .ForMember(dest => dest.BankName, opt => opt.Ignore());

            CreateMap<CreatePaymentReceiptDto, PaymentReceipt>()
                .ForMember(dest => dest.ReceiptId, opt => opt.Ignore())
                .ForMember(dest => dest.ReceiptDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnit, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethodNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.BankNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore());

            CreateMap<UpdatePaymentReceiptDto, PaymentReceipt>()
                .ForMember(dest => dest.ReceiptId, opt => opt.Ignore())
                .ForMember(dest => dest.ReceiptDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnit, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethodNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.BankNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore());

            // Corporate Customer mappings
            CreateMap<CorporateCustomer, CorporateCustomerResponseDto>()
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.HotelSettings != null ? src.HotelSettings.HotelName : null));

            CreateMap<CreateCorporateCustomerDto, CorporateCustomer>()
                .ForMember(dest => dest.CorporateId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservations, opt => opt.Ignore());

            CreateMap<UpdateCorporateCustomerDto, CorporateCustomer>()
                .ForMember(dest => dest.CorporateId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservations, opt => opt.Ignore());

            // Reservation Unit mappings
            CreateMap<ReservationUnit, ReservationUnitResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusEnum))
                .ForMember(dest => dest.StatusWord, opt => opt.MapFrom(src => src.StatusWord))
                .ForMember(dest => dest.ReservationNo, opt => opt.MapFrom(src => src.Reservation != null ? src.Reservation.ReservationNo : null))
                .ForMember(dest => dest.ApartmentName, opt => opt.MapFrom(src => src.Apartment != null ? src.Apartment.ApartmentName : null))
                .ForMember(dest => dest.ApartmentCode, opt => opt.MapFrom(src => src.Apartment != null ? src.Apartment.ApartmentCode : null))
                .ForMember(dest => dest.BuildingName, opt => opt.MapFrom(src => src.Apartment != null && src.Apartment.Building != null ? src.Apartment.Building.BuildingName : null))
                .ForMember(dest => dest.FloorName, opt => opt.MapFrom(src => src.Apartment != null && src.Apartment.Floor != null ? src.Apartment.Floor.FloorName : null))
                .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.Apartment != null && src.Apartment.RoomType != null ? src.Apartment.RoomType.RoomTypeName : null))
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Reservation != null && src.Reservation.HotelSettings != null ? src.Reservation.HotelSettings.HotelName : null));

            CreateMap<CreateReservationUnitDto, ReservationUnit>()
                .ForMember(dest => dest.UnitId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.Apartment, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore());

            CreateMap<UpdateReservationUnitDto, ReservationUnit>()
                .ForMember(dest => dest.UnitId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.Apartment, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore());

            // Apartment mappings
            CreateMap<Apartment, ApartmentResponseDto>()
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.HotelSettings != null ? src.HotelSettings.HotelName : null))
                .ForMember(dest => dest.BuildingName, opt => opt.MapFrom(src => src.Building != null ? src.Building.BuildingName : null))
                .ForMember(dest => dest.FloorName, opt => opt.MapFrom(src => src.Floor != null ? src.Floor.FloorName : null))
                .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.RoomType != null ? src.RoomType.RoomTypeName : null))
                .ForMember(dest => dest.TotalReservations, opt => opt.MapFrom(src => src.ReservationUnits != null ? src.ReservationUnits.Count : 0))
                .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.ReservationUnits != null ? src.ReservationUnits.Sum(ru => ru.TotalAmount) : 0))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Status == "available"));

            CreateMap<CreateApartmentDto, Apartment>()
                .ForMember(dest => dest.ApartmentId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Building, opt => opt.Ignore())
                .ForMember(dest => dest.Floor, opt => opt.Ignore())
                .ForMember(dest => dest.RoomType, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnits, opt => opt.Ignore());

            CreateMap<UpdateApartmentDto, Apartment>()
                .ForMember(dest => dest.ApartmentId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Building, opt => opt.Ignore())
                .ForMember(dest => dest.Floor, opt => opt.Ignore())
                .ForMember(dest => dest.RoomType, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnits, opt => opt.Ignore());

            // Building mappings
            CreateMap<Building, BuildingResponseDto>()
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.HotelSettings != null ? src.HotelSettings.HotelName : null))
                .ForMember(dest => dest.TotalFloors, opt => opt.MapFrom(src => src.Floors != null ? src.Floors.Count : 0))
                .ForMember(dest => dest.TotalApartments, opt => opt.MapFrom(src => src.Apartments != null ? src.Apartments.Count : 0))
                .ForMember(dest => dest.TotalReservations, opt => opt.MapFrom(src => src.Apartments != null ? src.Apartments.Sum(a => a.ReservationUnits != null ? a.ReservationUnits.Count : 0) : 0))
                .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.Apartments != null ? src.Apartments.Sum(a => a.ReservationUnits != null ? a.ReservationUnits.Sum(ru => ru.TotalAmount) : 0) : 0));

            CreateMap<CreateBuildingDto, Building>()
                .ForMember(dest => dest.BuildingId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Floors, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore());

            CreateMap<UpdateBuildingDto, Building>()
                .ForMember(dest => dest.BuildingId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Floors, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore());

            // Floor mappings
            CreateMap<Floor, FloorResponseDto>()
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.HotelSettings != null ? src.HotelSettings.HotelName : null))
                .ForMember(dest => dest.TotalApartments, opt => opt.MapFrom(src => src.Apartments != null ? src.Apartments.Count : 0))
                .ForMember(dest => dest.TotalReservations, opt => opt.MapFrom(src => src.Apartments != null ? src.Apartments.Sum(a => a.ReservationUnits != null ? a.ReservationUnits.Count : 0) : 0))
                .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.Apartments != null ? src.Apartments.Sum(a => a.ReservationUnits != null ? a.ReservationUnits.Sum(ru => ru.TotalAmount) : 0) : 0));

            CreateMap<CreateFloorDto, Floor>()
                .ForMember(dest => dest.FloorId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore());

            CreateMap<UpdateFloorDto, Floor>()
                .ForMember(dest => dest.FloorId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore());

            CreateMap<CreateFloorItemDto, Floor>()
                .ForMember(dest => dest.FloorId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore());

            CreateMap<UpdateFloorItemDto, Floor>()
                .ForMember(dest => dest.FloorId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore());

            // RoomType mappings
            CreateMap<RoomType, RoomTypeResponseDto>()
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.HotelSettings != null ? src.HotelSettings.HotelName : null))
                .ForMember(dest => dest.TotalApartments, opt => opt.MapFrom(src => src.Apartments != null ? src.Apartments.Count : 0))
                .ForMember(dest => dest.TotalReservations, opt => opt.MapFrom(src => src.Apartments != null ? src.Apartments.Sum(a => a.ReservationUnits != null ? a.ReservationUnits.Count : 0) : 0))
                .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => src.Apartments != null ? src.Apartments.Sum(a => a.ReservationUnits != null ? a.ReservationUnits.Sum(ru => ru.TotalAmount) : 0) : 0))
                .ForMember(dest => dest.AverageRevenue, opt => opt.MapFrom(src => src.Apartments != null && src.Apartments.Any() ? src.Apartments.Sum(a => a.ReservationUnits != null ? a.ReservationUnits.Sum(ru => ru.TotalAmount) : 0) / src.Apartments.Count : 0))
                .ForMember(dest => dest.OccupancyRate, opt => opt.MapFrom(src => 0)) // Will be calculated separately
                .ForMember(dest => dest.AverageStayDuration, opt => opt.MapFrom(src => 0)); // Will be calculated separately

            CreateMap<CreateRoomTypeDto, RoomType>()
                .ForMember(dest => dest.RoomTypeId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore());

            CreateMap<UpdateRoomTypeDto, RoomType>()
                .ForMember(dest => dest.RoomTypeId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore());

            // Zaaer Customer mappings
            CreateMap<Customer, ZaaerCustomerResponseDto>()
                .ForMember(dest => dest.Identifications, opt => opt.MapFrom(src => src.Identifications));

            CreateMap<ZaaerCreateCustomerDto, Customer>()
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.GuestType, opt => opt.Ignore())
                .ForMember(dest => dest.Nationality, opt => opt.Ignore())
                .ForMember(dest => dest.GuestCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Identifications, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.Reservations, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore());

            CreateMap<ZaaerUpdateCustomerDto, Customer>()
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.GuestType, opt => opt.Ignore())
                .ForMember(dest => dest.Nationality, opt => opt.Ignore())
                .ForMember(dest => dest.GuestCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Identifications, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.Reservations, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore());

            // Zaaer Customer Identification mappings
            CreateMap<CustomerIdentification, ZaaerCustomerIdentificationResponseDto>();
            CreateMap<ZaaerCustomerIdentificationDto, CustomerIdentification>()
                .ForMember(dest => dest.IdentificationId, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore()) // Ignore CustomerId - set programmatically
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.IdType, opt => opt.Ignore());

            // Zaaer Reservation mappings
            CreateMap<Reservation, ZaaerReservationResponseDto>()
                .ForMember(dest => dest.ReservationUnits, opt => opt.MapFrom(src => src.ReservationUnits));

            CreateMap<ZaaerCreateReservationDto, Reservation>()
                .ForMember(dest => dest.ReservationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RentalType, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.RentalType) ? FinanceLedgerAPI.Enums.RentalType.Daily.ToString() : src.RentalType))
                .ForMember(dest => dest.IsAutoExtend, opt => opt.MapFrom(src => src.IsAutoExtend)) // Explicit mapping for IsAutoExtend
                .ForMember(dest => dest.PriceTypeId, opt => opt.MapFrom(src => src.PriceTypeId)) // Explicit mapping for PriceTypeId
                .ForMember(dest => dest.ReservationUnits, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.VisitPurpose, opt => opt.Ignore())
                .ForMember(dest => dest.CorporateCustomer, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore());

            CreateMap<ZaaerUpdateReservationDto, Reservation>()
                .ForMember(dest => dest.ReservationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RentalType, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.RentalType)))
                .ForMember(dest => dest.RentalType, opt => opt.MapFrom(src => src.RentalType))
                .ForMember(dest => dest.ReservationType, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.ReservationType)))
                .ForMember(dest => dest.ReservationType, opt => opt.MapFrom(src => src.ReservationType))
                .ForMember(dest => dest.Status, opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.Status)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                // Map IsAutoExtend only if value is provided (HasValue check in Condition, then map the value)
                .ForMember(dest => dest.IsAutoExtend, opt => 
                {
                    opt.Condition(src => src.IsAutoExtend.HasValue);
                    opt.MapFrom(src => src.IsAutoExtend!.Value);
                })
                // Map PriceTypeId only if value is provided (HasValue check in Condition, then map the value)
                .ForMember(dest => dest.PriceTypeId, opt => 
                {
                    opt.Condition(src => src.PriceTypeId.HasValue);
                    opt.MapFrom(src => src.PriceTypeId!.Value);
                })
                .ForMember(dest => dest.ReservationUnits, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.VisitPurpose, opt => opt.Ignore())
                .ForMember(dest => dest.CorporateCustomer, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore());

            // Zaaer Reservation Unit mappings
            CreateMap<ReservationUnit, ZaaerReservationUnitResponseDto>();

            // Legacy DTO support (if exists)
            CreateMap<ZaaerReservationUnitDto, ReservationUnit>()
                .ForMember(dest => dest.UnitId, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.Apartment, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore());

            // Remove additional minimal DTO mappings to avoid conflicts; keep existing mappings only

            // Zaaer Payment Receipt mappings
            CreateMap<PaymentReceipt, ZaaerPaymentReceiptResponseDto>();
            CreateMap<ZaaerCreatePaymentReceiptDto, PaymentReceipt>()
                .ForMember(dest => dest.ReceiptId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnit, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethodNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.BankNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.BankId, opt => opt.MapFrom(src => src.BankId))
                .ForMember(dest => dest.PaymentMethodId, opt => opt.MapFrom(src => src.PaymentMethodId))
                .ForMember(dest => dest.InvoiceId, opt => opt.MapFrom(src => src.InvoiceId));

            CreateMap<ZaaerUpdatePaymentReceiptDto, PaymentReceipt>()
                .ForMember(dest => dest.ReceiptId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnit, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethodNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.BankNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore())
                .ForMember(dest => dest.BankId, opt => opt.MapFrom(src => src.BankId))
                .ForMember(dest => dest.PaymentMethodId, opt => opt.MapFrom(src => src.PaymentMethodId))
                .ForMember(dest => dest.InvoiceId, opt => opt.MapFrom(src => src.InvoiceId));

            // Zaaer Invoice mappings
            CreateMap<Invoice, ZaaerInvoiceResponseDto>()
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CreditNotes, opt => opt.Ignore());
            CreateMap<ZaaerCreateInvoiceDto, Invoice>()
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnit, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CreditNotes, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore());

            CreateMap<ZaaerUpdateInvoiceDto, Invoice>()
                .ForMember(dest => dest.InvoiceId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnit, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CreditNotes, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore());

            // Zaaer Refund mappings
            CreateMap<Refund, ZaaerRefundResponseDto>();
            CreateMap<ZaaerCreateRefundDto, Refund>()
                .ForMember(dest => dest.RefundId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnit, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethodNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.BankNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore());

            CreateMap<ZaaerUpdateRefundDto, Refund>()
                .ForMember(dest => dest.RefundId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnit, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentMethodNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.BankNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore());

            // Credit Note Mappings
            CreateMap<ZaaerCreateCreditNoteDto, CreditNote>()
                .ForMember(dest => dest.CreditNoteId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Invoice, opt => opt.Ignore())
                .ForMember(dest => dest.Reservation, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerTransactions, opt => opt.Ignore());

            CreateMap<CreditNote, ZaaerCreditNoteResponseDto>();

            // Room Type Mappings
            CreateMap<ZaaerCreateRoomTypeDto, RoomType>()
                .ForMember(dest => dest.RoomTypeId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore());

            CreateMap<ZaaerUpdateRoomTypeDto, RoomType>()
                .ForMember(dest => dest.RoomTypeId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore());

            CreateMap<RoomType, ZaaerRoomTypeResponseDto>();

            // Zaaer Floor mappings
            CreateMap<Floor, ZaaerFloorResponseDto>();
            CreateMap<ZaaerCreateFloorDto, Floor>();
            CreateMap<ZaaerUpdateFloorDto, Floor>();

            // Seasonal Rate mappings
            CreateMap<ZaaerCreateSeasonalRateDto, SeasonalRate>()
                .ForMember(dest => dest.SeasonId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<ZaaerSeasonalRateItemDto, SeasonalRateItem>()
                .ForMember(dest => dest.ItemId, opt => opt.Ignore())
                .ForMember(dest => dest.SeasonId, opt => opt.Ignore());
            CreateMap<SeasonalRate, ZaaerSeasonalRateResponseDto>();
            CreateMap<SeasonalRateItem, ZaaerSeasonalRateItemResponseDto>();

            // Maintenance mappings
            CreateMap<Maintenance, ZaaerMaintenanceResponseDto>();
            CreateMap<ZaaerCreateMaintenanceDto, Maintenance>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Apartment, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
            CreateMap<ZaaerUpdateMaintenanceDto, Maintenance>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Apartment, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Zaaer Tax mappings
            CreateMap<Tax, ZaaerTaxResponseDto>();
            CreateMap<ZaaerCreateTaxDto, Tax>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Ignore())
                .ForMember(dest => dest.TaxCategory, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore());
            CreateMap<ZaaerUpdateTaxDto, Tax>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Ignore())
                .ForMember(dest => dest.TaxCategory, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // RateType mappings
            CreateMap<ZaaerCreateRateTypeDto, RateType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UnitItems, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore());
            CreateMap<ZaaerRateTypeUnitItemDto, RateTypeUnitItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.RateTypeId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RateType, opt => opt.Ignore());
            CreateMap<RateType, ZaaerRateTypeResponseDto>();
            CreateMap<RateTypeUnitItem, ZaaerRateTypeUnitItemResponseDto>();

            // ZATCA details mappings
            CreateMap<ZaaerCreateZatcaDetailsDto, ZatcaDetails>()
                .ForMember(dest => dest.DetailsId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<ZatcaDetails, ZaaerZatcaDetailsResponseDto>();

            // NTMP details mappings
            CreateMap<ZaaerCreateNtmpDetailsDto, NtmpDetails>()
                .ForMember(dest => dest.DetailsId, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<NtmpDetails, ZaaerNtmpDetailsResponseDto>()
                .ForMember(dest => dest.PasswordMask, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.PasswordHash) ? null : "******"));

            // Shomoos details mappings
            CreateMap<ZaaerCreateShomoosDetailsDto, ShomoosDetails>()
                .ForMember(dest => dest.DetailsId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<ShomoosDetails, ZaaerShomoosDetailsResponseDto>()
                .ForMember(dest => dest.BranchSecretMask, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.BranchSecret) ? null : "******"));

            // Zaaer Apartment mappings
            CreateMap<Apartment, ZaaerApartmentResponseDto>();
            CreateMap<ZaaerCreateApartmentDto, Apartment>()
                .ForMember(dest => dest.ApartmentId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Building, opt => opt.Ignore())
                .ForMember(dest => dest.Floor, opt => opt.Ignore())
                .ForMember(dest => dest.RoomType, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnits, opt => opt.Ignore());
            CreateMap<ZaaerUpdateApartmentDto, Apartment>()
                .ForMember(dest => dest.ApartmentId, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Building, opt => opt.Ignore())
                .ForMember(dest => dest.Floor, opt => opt.Ignore())
                .ForMember(dest => dest.RoomType, opt => opt.Ignore())
                // Do NOT allow Zaaer to change operational room status via this endpoint
                // This ensures payloads that include "status" won't override current occupancy state
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ReservationUnits, opt => opt.Ignore());


            // Hotel Settings mappings
            CreateMap<HotelSettings, ZaaerHotelSettingsResponseDto>();
            CreateMap<ZaaerCreateHotelSettingsDto, HotelSettings>()
                .ForMember(dest => dest.HotelId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Customers, opt => opt.Ignore())
                .ForMember(dest => dest.Reservations, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.RoomTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore())
                .ForMember(dest => dest.Buildings, opt => opt.Ignore())
                .ForMember(dest => dest.Floors, opt => opt.Ignore())
                .ForMember(dest => dest.CorporateCustomers, opt => opt.Ignore())
                .ForMember(dest => dest.CreditNotes, opt => opt.Ignore());
            CreateMap<ZaaerUpdateHotelSettingsDto, HotelSettings>()
                .ForMember(dest => dest.HotelId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Customers, opt => opt.Ignore())
                .ForMember(dest => dest.Reservations, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.PaymentReceipts, opt => opt.Ignore())
                .ForMember(dest => dest.Refunds, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerAccounts, opt => opt.Ignore())
                .ForMember(dest => dest.RoomTypes, opt => opt.Ignore())
                .ForMember(dest => dest.Apartments, opt => opt.Ignore())
                .ForMember(dest => dest.Buildings, opt => opt.Ignore())
                .ForMember(dest => dest.Floors, opt => opt.Ignore())
                .ForMember(dest => dest.CorporateCustomers, opt => opt.Ignore())
                .ForMember(dest => dest.CreditNotes, opt => opt.Ignore())
                // Explicitly map fields - LogoUrl null handling is done in the service layer (converts null to empty string)
                .ForMember(dest => dest.LogoUrl, opt => opt.MapFrom(src => src.LogoUrl))
                .ForMember(dest => dest.HotelCode, opt => opt.MapFrom(src => src.HotelCode))
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.HotelName))
                .ForMember(dest => dest.DefaultCurrency, opt => opt.MapFrom(src => src.DefaultCurrency))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName));

            // Zaaer Bank mappings
            CreateMap<FinanceLedgerAPI.Models.Bank, ZaaerBankResponseDto>();
            // Zaaer Room Type Rate mappings
            CreateMap<RoomTypeRate, ZaaerRoomTypeRateResponseDto>()
                .ForMember(dest => dest.RoomTypeName, opt => opt.Ignore()); // Will be set in service

            CreateMap<ZaaerCreateRoomTypeRateDto, RoomTypeRate>()
                .ForMember(dest => dest.RateId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RoomType, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore());

            CreateMap<ZaaerUpdateRoomTypeRateDto, RoomTypeRate>()
                .ForMember(dest => dest.RateId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RoomType, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Expense
            CreateMap<ZaaerCreateExpenseDto, Expense>()
                .ForMember(dest => dest.ExpenseId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<ZaaerUpdateExpenseDto, Expense>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<FinanceLedgerAPI.Models.Expense, ZaaerExpenseResponseDto>();

            // Zaaer User mappings
            CreateMap<User, ZaaerUserResponseDto>()
                .ForMember(dest => dest.RoleName, opt => opt.Ignore()); // Will be set in service
            CreateMap<ZaaerCreateUserDto, User>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Password is handled separately in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore());
            CreateMap<ZaaerUpdateUserDto, User>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Password is handled separately in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.LastLogin, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.HotelSettings, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Only update non-null fields
        }
    }
}
