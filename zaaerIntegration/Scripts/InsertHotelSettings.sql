-- SQL INSERT Statement for hotel_settings table
-- Based on the data from dbo.hotel_settings table
-- Date: 2025-11-17

INSERT INTO hotel_settings (
    hotel_id,
    zaaer_id,
    hotel_code,
    hotel_name,
    default_currency,
    company_name,
    logo_url,
    address,
    phone,
    email,
    created_at,
    tax_number,
    cr_number,
    country_code,
    city,
    contact_person,
    latitude,
    longitude,
    enabled,
    total_rooms,
    property_type
) VALUES (
    11,                                                              -- hotel_id
    2,                                                               -- zaaer_id
    'Dammam2',                                                       -- hotel_code
    N'المدينة ١٤',                                                   -- hotel_name (Arabic text - using N prefix for Unicode)
    'SAR',                                                           -- default_currency
    N'المدينة ١٤',                                                   -- company_name (Arabic text)
    'https://stagingpms.qualitycode.sa/storage/hotels/YHfVs3Fhe6DIjZb0ftrhlSPUq.jpeg',  -- logo_url
    N'المدينة المنورة',                                             -- address (Arabic text)
    '548245840',                                                     -- phone
    'beba.mohamed@gmail.com',                                        -- email
    '2025-10-27 13:55:51',                                          -- created_at (converted from 10/27/2025 1:55:51 PM)
    '300581805400003',                                               -- tax_number
    '2050082780',                                                    -- cr_number
    'SA',                                                            -- country_code
    N'المدينة المنورة',                                             -- city (Arabic text)
    'MUHAMMEDD',                                                     -- contact_person
    NULL,                                                            -- latitude (empty in image)
    NULL,                                                            -- longitude (empty in image)
    1,                                                               -- enabled
    0,                                                               -- total_rooms
    'Hotel'                                                          -- property_type
);

-- Alternative INSERT with explicit NULL handling for empty fields
-- If latitude and longitude columns don't accept NULL, you may need to use default values or remove them from INSERT

-- Note: 
-- 1. Arabic text fields use N prefix for Unicode support (NVARCHAR)
-- 2. Date format converted from MM/DD/YYYY to YYYY-MM-DD HH:MM:SS
-- 3. Empty latitude/longitude fields are set to NULL
-- 4. Adjust column names if your actual table structure differs
-- 5. If hotel_id is an IDENTITY column, remove it from INSERT and let SQL Server auto-generate it

