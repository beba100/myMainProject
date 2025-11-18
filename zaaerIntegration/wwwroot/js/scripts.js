// Utility Functions
const Utils = {
    getElement(id) {
        return document.getElementById(id) || null;
    },
    getFloatValue(id) {
        const el = this.getElement(id);
        return el ? parseFloat(el.value) || 0 : 0;
    },
    setValue(id, value) {
        const el = this.getElement(id);
        if (el) el.value = value;
        else console.warn(`Element ${id} not found`);
    },
    isChecked(id) {
        const el = this.getElement(id);
        return el ? el.checked : false;
    },
    setChecked(id, state) {
        const el = this.getElement(id);
        if (el) el.checked = state;
    },
    roundValue(value) {
        return parseFloat(value).toFixed(2);
    },
    attachEventListener(id, event, handler) {
        const el = this.getElement(id);
        if (el) el.addEventListener(event, handler);
        else console.warn(`Cannot attach event to ${id} (not found)`);
    },
    formatDate(date, format = 'en-CA') {
        return date.toLocaleDateString(format) + ' ' + date.toLocaleTimeString('en-US', { hour12: true });
    },
    toHijriDate(date) {
        return new Intl.DateTimeFormat('ar-SA-u-ca-islamic', { year: 'numeric', month: '2-digit', day: '2-digit' })
            .format(date)
            .replace(/-/g, '/');
    }
};

// Dropdown Handler
const DropdownHandler = {
    async fetchDropdownData(url, elementId, selectIndex = 0, callback = null) {
        try {
            const response = await fetch(url, { method: 'GET', headers: { 'Accept': 'application/json' } });
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

            const data = await response.json();
            const dropdown = Utils.getElement(elementId);

            if (dropdown && Array.isArray(data)) {
                dropdown.innerHTML = '';
                data.forEach(item => {
                    const option = document.createElement('option');
                    option.value = item.bS_Id || item.expcat_id || item.mode_ID || '';
                    option.textContent = item.bs_name_ar || item.expcat_name || item.mode_Name || 'Unknown';
                    dropdown.appendChild(option);
                });

                if (data.length > selectIndex) {
                    dropdown.selectedIndex = selectIndex;
                }

                if (elementId === 'BookingSourceComboBox') {
                    Utils.attachEventListener('BookingSourceComboBox', 'change', applyBookingSourceLogic);
                }

                if (callback) callback();
            }
        } catch (error) {
            console.error(`Error fetching ${elementId}:`, error);
        }
    }
};

// Booking Source Logic
function applyBookingSourceLogic() {
    const bookingSourceDropdown = Utils.getElement('BookingSourceComboBox');
    const bookingNoContainer = Utils.getElement('bookingNoContainer');
    const bookingSourceContainer = Utils.getElement('bookingSourceContainer');
    const txtBookingNo = Utils.getElement('txtBookingNo');

    if (!bookingSourceDropdown || !bookingNoContainer || !bookingSourceContainer) return;

    let bookingSourceValue = parseInt(bookingSourceDropdown.value, 10) || 0;

    if (bookingSourceValue > 1) {
        bookingNoContainer.style.display = 'block';
        bookingSourceContainer.classList.replace('col-12', 'col-6');
    } else {
        bookingNoContainer.style.display = 'none';
        txtBookingNo.value = '';
        bookingSourceContainer.classList.replace('col-6', 'col-12');
    }
}

// UI Initializer
const UIInitializer = {
    bookingId: null,
    customerName: null,
    roomName: null,

    async init() {
        const params = new URLSearchParams(window.location.search);
        this.bookingId = params.get('bookingId');
        this.customerName = params.get('customerName') ? decodeURIComponent(params.get('customerName')) : "Unknown Customer";
        this.roomName = params.get('roomName') ? decodeURIComponent(params.get('roomName')) : "Unknown Room";

        if (!this.bookingId || this.bookingId === "0") {
            console.error("Invalid Booking ID detected.");
            showError("Invalid Booking ID.", 5);
            return;
        }

        const apiUrl = `/api/Booking/${this.bookingId}`;
        try {
            const response = await fetch(apiUrl, {
                method: "GET",
                mode: "cors",
                headers: { "Accept": "application/json", "Content-Type": "application/json" }
            });
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

            const data = await response.json();
            this.customerName = data.ch_name || this.customerName;
            this.roomName = data.a_name || this.roomName;

            Utils.setValue("ReceivedFromTextBox", this.customerName);
            const roomNameLabel = Utils.getElement("roomNameLabel");
            if (roomNameLabel) roomNameLabel.textContent = this.roomName;

            const now = new Date();
            Utils.setValue("txttime_stamp", Utils.formatDate(now));
            Utils.setValue("CMBHijri", Utils.toHijriDate(now));

            if (data.gFromDate) Utils.setValue("FromDate_DTP", data.gFromDate.split("T")[0]);
            if (data.gToDate) Utils.setValue("ToDate_DTP", data.gToDate.split("T")[0]);

            await Promise.all([
                DropdownHandler.fetchDropdownData('/api/BookingSource', 'BookingSourceComboBox', 0),
                DropdownHandler.fetchDropdownData('/api/Expcat?filter=ch_rcpt=1', 'ChRcpt_CMB', 0),
                DropdownHandler.fetchDropdownData('/api/PaymentMode', 'Mode_CMB', 0, VisibilityLogic.applyVisibilityLogic),
                DropdownHandler.fetchDropdownData('/api/Expcat?filter=ch_id_bank=1', 'Bank_CMB', 1)
            ]);

            Utils.attachEventListener('Mode_CMB', 'change', VisibilityLogic.applyVisibilityLogic);
            Utils.attachEventListener('ReceiptType_CMB', 'change', VisibilityLogic.applyVisibilityLogic);

            // Update Booking Modal
            document.getElementById("customerNameHeader").textContent = data.ch_name || this.customerName;
            document.getElementById("roomNameHeader").textContent = "Room: " + (data.a_name || this.roomName);
            document.getElementById("b_no_display").textContent = data.b_No || "-";

            Utils.setValue("rentalAmount", data.rentalAmount || "");
            Utils.setValue("amountPaid", data.amountPaid || "");
            Utils.setValue("balance", data.balance || "");
            Utils.setValue("comments", data.comments || "");

            if (data.gToDate) Utils.setValue("GToDate", data.gToDate.split("T")[0]);
            if (data.gFromDate) Utils.setValue("GFromDate", data.gFromDate.split("T")[0]);

            const rentalType = (data.rental || "").toLowerCase();
            document.getElementById("rentalDaily").checked = (rentalType === "d");
            document.getElementById("rentalMonthly").checked = (rentalType === "m");
            document.getElementById("rentalYearly").checked = (rentalType === "y");

            const bookingModal = new bootstrap.Modal(document.getElementById("bookingModal"));
            bookingModal.show();

            document.getElementById("bookingModal").addEventListener("hidden.bs.modal", () => {
                window.location.href = window.location.origin + "/rooms.php";
            });
        } catch (error) {
            console.error("Error fetching booking data:", error);
            showError(`Error fetching booking data: ${error.message}`);
        }
    }
};

// Visibility Logic
const VisibilityLogic = {
    applyVisibilityLogic() {
        // Placeholder for visibility logic (e.g., showing/hiding fields based on dropdowns)
    }
};

// Tax Calculator
const TaxCalculator = {
    async init() {
        // Placeholder for tax calculation logic
    }
};

// Datepicker Initialization
function initializeDatepickers() {
    $.datepicker.setDefaults({
        dateFormat: "yy-mm-dd",
        regional: "en"
    });

    const modalIds = ["receiptModal", "bookingModal", "customerModal"];
    modalIds.forEach(modalId => {
        $(`#${modalId}`).on("shown.bs.modal", function() {
            const $fromDateInput = $(this).find("#FromDate_DTP, #GFromDate");
            const $toDateInput = $(this).find("#ToDate_DTP, #GToDate");

            const formatDate = (dateString) => {
                if (!dateString) return null;
                let date;
                if (/^\d{4}-\d{2}-\d{2}$/.test(dateString)) {
                    const [year, month, day] = dateString.split("-").map(Number);
                    date = new Date(year, month - 1, day);
                } else if (/^\d{2}-\d{2}-\d{4}$/.test(dateString)) {
                    const [day, month, year] = dateString.split("-").map(Number);
                    date = new Date(year, month - 1, day);
                } else {
                    date = new Date(dateString);
                }
                if (!date || isNaN(date.getTime())) return null;
                const year = date.getFullYear();
                const month = String(date.getMonth() + 1).padStart(2, "0");
                const day = String(date.getDate()).padStart(2, "0");
                return `${year}-${month}-${day}`;
            };

            if ($fromDateInput.length) {
                const initialFromDate = $fromDateInput.val();
                const formattedFromDate = formatDate(initialFromDate);
                if (formattedFromDate) $fromDateInput.val(formattedFromDate);
            }

            if ($toDateInput.length) {
                const initialToDate = $toDateInput.val();
                const formattedToDate = formatDate(initialToDate);
                if (formattedToDate) $toDateInput.val(formattedToDate);
            }

            if ($fromDateInput.length && !$fromDateInput.hasClass("hasDatepicker")) {
                $fromDateInput.datepicker({
                    dateFormat: "yy-mm-dd",
                    changeMonth: true,
                    changeYear: true,
                    showAnim: "fadeIn",
                    showButtonPanel: true,
                    showOn: "focus",
                    beforeShow: (input, inst) => {
                        setTimeout(() => inst.dpDiv.css({ zIndex: 10000 }), 0);
                    },
                    onSelect: function(dateText) {
                        const selectedDate = $(this).datepicker("getDate");
                        const formattedDate = formatDate(selectedDate);
                        if (formattedDate) $(this).val(formattedDate);
                    }
                });
            }

            if ($toDateInput.length && !$toDateInput.hasClass("hasDatepicker")) {
                $toDateInput.datepicker({
                    dateFormat: "yy-mm-dd",
                    changeMonth: true,
                    changeYear: true,
                    showAnim: "fadeIn",
                    showButtonPanel: true,
                    showOn: "focus",
                    beforeShow: (input, inst) => {
                        setTimeout(() => inst.dpDiv.css({ zIndex: 10000 }), 0);
                    },
                    onSelect: function(dateText) {
                        const selectedDate = $(this).datepicker("getDate");
                        const formattedDate = formatDate(selectedDate);
                        if (formattedDate) $(this).val(formattedDate);
                    }
                });
            }
        });
    });
}

// Booking Functions
async function updateBooking() {
    const bookingId = Utils.getElement("bookingId")?.value?.trim();
    if (!bookingId || isNaN(bookingId)) {
        alert("Booking ID is missing or invalid!");
        return;
    }

    const getValue = (id) => {
        const element = Utils.getElement(id);
        return element ? element.value.trim() || null : null;
    };

    const getRentalType = () => {
        if (document.getElementById("rentalDaily")?.checked) return "d";
        if (document.getElementById("rentalMonthly")?.checked) return "m";
        if (document.getElementById("rentalYearly")?.checked) return "y";
        return null;
    };

    const updatedData = {
        B_Id: parseInt(bookingId),
        RentalAmount: getValue("rentalAmount") ? parseFloat(getValue("rentalAmount")) : null,
        AmountPaid: getValue("amountPaid") ? parseFloat(getValue("amountPaid")) : null,
        Balance: getValue("balance") ? parseFloat(getValue("balance")) : null,
        Comments: getValue("comments") || "",
        Rental: getRentalType(),
        GFromDate: getValue("GFromDate") ? new Date(getValue("GFromDate")).toISOString() : null,
        GToDate: getValue("GToDate") ? new Date(getValue("GToDate")).toISOString() : null
    };

    const apiUrl = `/api/BookingDTO/${bookingId}`;
    try {
        const response = await fetch(apiUrl, {
            method: "PUT",
            headers: { "Content-Type": "application/json", "Accept": "application/json" },
            mode: "cors",
            body: JSON.stringify(updatedData)
        });

        if (!response.ok) {
            const errorMessage = await response.text();
            throw new Error(`Server Error: ${errorMessage}`);
        }

        showSuccess("Booking updated successfully!", 5);
        window.location.href = `${window.location.origin}/rooms.php`;
    } catch (error) {
        console.error("Error updating booking:", error);
        alert(`Error updating booking: ${error.message}`);
    }
}

function checkoutBooking() {
    const bookingId = Utils.getElement("bookingId")?.value?.trim();
    const username = GlobalSettings.getUserSetting("FullName") || "Guest";
    const balance = parseFloat(Utils.getElement("balance")?.value) || 0;

    if (!username) {
        alert("Error: Username not found! Please log in again.");
        return;
    }

    if (balance > 0) {
        alert("You cannot check out! The balance must be 0.");
        return;
    }

    if (!bookingId || isNaN(bookingId)) {
        alert("Invalid Booking ID!");
        return;
    }

    if (!confirm(`Are you sure you want to check out this room?\n\nClick 'OK' to proceed, or 'Cancel' to abort.`)) {
        return;
    }

    const apiUrl = `/api/Checkout/CloseBooking/${bookingId}`;
    fetch(apiUrl, {
        method: "PUT",
        headers: { "Content-Type": "application/json", "Accept": "application/json" },
        body: JSON.stringify({ Username: username })
    })
    .then(response => {
        if (!response.ok) {
            return response.json().then(err => { throw new Error(err.message); });
        }
        return response.json();
    })
    .then(() => {
        showSuccess("Booking closed successfully!", 5);
        window.location.href = `${window.location.origin}/rooms.php`;
    })
    .catch(error => {
        console.error("Error during check-out:", error);
        alert(`Error during check-out: ${error.message}`);
    });
}

// Customer Functions
function handleCustomerIconClick() {
    const bookingId = Utils.getElement("bookingId")?.value?.trim();
    if (!bookingId) return;
    loadCustomerDetails(bookingId);
}

async function loadCustomerDetails(bookingId) {
    if (!bookingId) return;

    try {
        const response = await fetch(`/api/Customer/GetCustomerByBooking/${bookingId}`);
        if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

        const customer = await response.json();
        if (!customer || Object.keys(customer).length === 0) return;

        Utils.setValue("customerId", customer.ch_id);

        setTimeout(() => {
            Utils.setValue("FirstName_TXT", customer.ch_name || "");
            Utils.setValue("Mobile_TXT", customer.ch_mobileno || "");
            Utils.setValue("txtBirthday", customer.ch_birthday || "");
            Utils.setValue("CommentsTextBox", customer.ch_comments || "");
            Utils.setValue("GuestType_CMB", customer.gtype_id || "");
            Utils.setValue("IDType_CMB", customer.id_type || "");
            Utils.setValue("Nationality_CMB", customer.n_id || "");
            Utils.setValue("ch_visano", customer.ch_visano_p || "");
            Utils.setValue("ch_issueplace", customer.ch_issueplace_id || "");
            Utils.setValue("ch_expdate", customer.ch_expdate_id ? new Date(customer.ch_expdate_id).toISOString().split('T')[0] : null);

            const genderDropdown = Utils.getElement("Gender_CMP");
            if (genderDropdown) {
                genderDropdown.value = customer.ch_gender === "1" ? "ذكر" : customer.ch_gender === "2" ? "أنثى" : "غير ذلك";
            }

            updateUniversalFields(customer);

            const customerModal = new bootstrap.Modal(document.getElementById("customerModal"));
            customerModal.show();
        }, 100);
    } catch (error) {
        console.error("Error fetching customer details:", error);
    }
}

async function loadGuestTypes() {
    try {
        const response = await fetch("/api/GuestType/GetGuestTypes");
        const data = await response.json();
        const guestTypeDropdown = Utils.getElement("GuestType_CMB");

        guestTypeDropdown.innerHTML = "";
        data.forEach(item => {
            const option = document.createElement("option");
            option.value = item.gtype_id;
            option.textContent = item.gtype_name_ar || item.gtype_name;
            guestTypeDropdown.appendChild(option);
        });

        guestTypeDropdown.addEventListener("change", function() {
            applyGuestTypeLogic(this.value);
        });

        if (data.length > 0) {
            guestTypeDropdown.value = data[0].gtype_id;
            applyGuestTypeLogic(guestTypeDropdown.value);
        }
    } catch (error) {
        console.error("Error loading Guest Types:", error);
    }
}

function applyGuestTypeLogic(selectedGtypeID) {
    const visaField = Utils.getElement("visaField");
    const idTypeDropdown = Utils.getElement("IDType_CMB");

    if (visaField) {
        visaField.style.display = selectedGtypeID == "4" ? "block" : "none";
    }

    const filterCondition = {
        1: [1, 3, 6],
        2: [4, 6],
        3: [5, 6],
        4: [6]
    }[parseInt(selectedGtypeID)] || [];

    filterIDTypes(filterCondition);
}

function filterIDTypes(validIDs) {
    const idTypeDropdown = Utils.getElement("IDType_CMB");
    let firstValidOption = null;

    for (let option of idTypeDropdown.options) {
        const optionValue = parseInt(option.value);
        if (validIDs.includes(optionValue)) {
            option.style.display = "block";
            if (!firstValidOption) firstValidOption = option.value;
        } else {
            option.style.display = "none";
        }
    }

    if (!validIDs.includes(parseInt(idTypeDropdown.value))) {
        idTypeDropdown.value = firstValidOption || "";
    }

    updateUniversalFields();
}

async function loadIdTypes() {
    try {
        const response = await fetch("/api/IdType/GetIdTypes");
        const data = await response.json();
        const idTypeDropdown = Utils.getElement("IDType_CMB");

        idTypeDropdown.innerHTML = "";
        data.forEach(item => {
            const option = document.createElement("option");
            option.value = item.iT_ID;
            option.textContent = item.it_name2 || `ID ${item.iT_ID}`;
            idTypeDropdown.appendChild(option);
        });

        idTypeDropdown.addEventListener("change", updateUniversalFields);
    } catch (error) {
        console.error("Error fetching ID Types:", error);
    }
}

async function loadNationalities() {
    try {
        const response = await fetch("/api/Nationality/GetNationalities");
        const data = await response.json();
        const nationalityDropdown = Utils.getElement("Nationality_CMB");

        nationalityDropdown.innerHTML = "";
        const defaultOption = document.createElement("option");
        defaultOption.value = "";
        defaultOption.textContent = "اختر الجنسية...";
        defaultOption.disabled = true;
        defaultOption.selected = true;
        nationalityDropdown.appendChild(defaultOption);

        data.forEach(item => {
            const option = document.createElement("option");
            option.value = item.n_id;
            option.textContent = item.n_name_ar && item.n_name ? `${item.n_name_ar} - ${item.n_name}` : item.n_name_ar || item.n_name;
            nationalityDropdown.appendChild(option);
        });
    } catch (error) {
        console.error("Error loading nationalities:", error);
    }
}

function updateUniversalFields(customer = null) {
    const idTypeDropdown = Utils.getElement("IDType_CMB");
    const idType = parseInt(idTypeDropdown.value) || 0;

    const idFields = {
        nationalId: Utils.getElement("UniversalId_TXT"),
        nationalSerial: Utils.getElement("UniversalSerial_TXT"),
        familyId: Utils.getElement("FamilyId_TXT"),
        iqamaId: Utils.getElement("IqamaId_TXT"),
        iqamaSerial: Utils.getElement("IqamaSerial_TXT"),
        gccId: Utils.getElement("GCCId_TXT"),
        passportId: Utils.getElement("PassportId_TXT"),
        visaNo: Utils.getElement("ch_visano"),
    };

    Object.values(idFields).forEach(field => {
        field.parentElement.style.display = "none";
        field.value = "";
    });

    switch (idType) {
        case 1:
            idFields.nationalId.parentElement.style.display = "block";
            idFields.nationalSerial.parentElement.style.display = "block";
            idFields.nationalId.value = customer?.ch_docnum_id || "";
            idFields.nationalSerial.value = customer?.ch_idserial_id || "";
            break;
        case 3:
            idFields.familyId.parentElement.style.display = "block";
            idFields.familyId.value = customer?.ch_docnum_f || "";
            break;
        case 4:
            idFields.iqamaId.parentElement.style.display = "block";
            idFields.iqamaSerial.parentElement.style.display = "block";
            idFields.iqamaId.value = customer?.ch_docnum_i || "";
            idFields.iqamaSerial.value = customer?.ch_idserial_i || "";
            break;
        case 5:
            idFields.gccId.parentElement.style.display = "block";
            idFields.gccId.value = customer?.ch_docnum_g || "";
            break;
        case 6:
            idFields.passportId.parentElement.style.display = "block";
            idFields.visaNo.parentElement.style.display = "block";
            idFields.passportId.value = customer?.ch_docnum_p || "";
            idFields.visaNo.value = customer?.ch_visano_p || "";
            break;
    }
}

async function updateCustomer() {
    const customerId = Utils.getElement("customerId")?.value;
    if (!customerId) return;

    const customerData = {
        ch_name: Utils.getElement("FirstName_TXT")?.value || null,
        ch_birthday: Utils.getElement("txtBirthday")?.value || null,
        gtype_id: Utils.getElement("GuestType_CMB")?.value ? parseInt(Utils.getElement("GuestType_CMB").value) : null,
        n_id: Utils.getElement("Nationality_CMB")?.value ? parseInt(Utils.getElement("Nationality_CMB").value) : null,
        id_type: Utils.getElement("IDType_CMB")?.value ? parseInt(Utils.getElement("IDType_CMB").value) : null,
        ch_issueplace_id: Utils.getElement("ch_issueplace")?.value || null,
        ch_expdate_id: Utils.getElement("ch_expdate")?.value ? new Date(Utils.getElement("ch_expdate").value).toISOString() : null,
        ch_visano_p: Utils.getElement("ch_visano")?.value || null,
        ch_mobileno: Utils.getElement("Mobile_TXT")?.value || null,
        ch_comments: Utils.getElement("CommentsTextBox")?.value || null,
        ch_gender: Utils.getElement("Gender_CMP")?.value === "ذكر" ? "1" : Utils.getElement("Gender_CMP")?.value === "أنثى" ? "2" : "3"
    };

    let idFields = {};
    if (customerData.id_type === 1) {
        idFields.ch_docnum_id = Utils.getElement("UniversalId_TXT")?.value || null;
        idFields.ch_idserial_id = Utils.getElement("UniversalSerial_TXT")?.value || null;
    }
    if (customerData.id_type === 3) {
        idFields.ch_docnum_f = Utils.getElement("FamilyId_TXT")?.value || null;
    }
    if (customerData.id_type === 4) {
        idFields.ch_docnum_i = Utils.getElement("IqamaId_TXT")?.value || null;
        idFields.ch_idserial_i = Utils.getElement("IqamaSerial_TXT")?.value || null;
    }
    if (customerData.id_type === 5) {
        idFields.ch_docnum_g = Utils.getElement("GCCId_TXT")?.value || null;
    }
    if (customerData.id_type === 6) {
        idFields.ch_docnum_p = Utils.getElement("PassportId_TXT")?.value || null;
    }

    Object.assign(customerData, idFields);

    try {
        const response = await fetch(`/api/Customer/${customerId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(customerData)
        });

        if (!response.ok) {
            const result = await response.json();
            throw new Error(result.message || "Failed to update customer.");
        }

        alert("Customer updated successfully!");
    } catch (error) {
        console.error("Error updating customer:", error);
        alert("Failed to update customer.");
    }
}

// Receipt Functions
function hideUpdateButtonAndShowModal() {
    const updateBtn = Utils.getElement("updateReceiptBtn");
    if (updateBtn) updateBtn.style.display = "none";
    showReceiptModal();
}

// Payment Records
function showRecords() {
    const bookingId = Utils.getElement("bookingId")?.value;
    if (!bookingId || isNaN(bookingId) || bookingId <= 0) {
        alert("Invalid Booking ID");
        return;
    }

    fetch(`/api/Receipt/GetPaymentsByBooking/${bookingId}`)
        .then(response => {
            if (!response.ok) throw new Error(`API Error: ${response.status} - ${response.statusText}`);
            return response.json();
        })
        .then(data => {
            if (!Array.isArray(data)) {
                alert("Unexpected response format from server.");
                return;
            }

            const tableBody = document.querySelector("#receiptsTable tbody");
            tableBody.innerHTML = "";

            data.forEach((payment, index) => {
                const row = document.createElement("tr");
                if (payment.receipt_Type.includes("صرف")) {
                    row.style.backgroundColor = "#FF4040";
                }

                row.innerHTML = `
                    <td>${payment.receipt_No || "—"}</td>
                    <td>${formatDate(payment.receipt_Date)}</td>
                    <td>${formatDate(payment.receipt_FromDate)}</td>
                    <td>${formatDate(payment.receipt_ToDate)}</td>
                    <td>${payment.receipt_Mode || "—"}</td>
                    <td>${payment.receipt_Type || "—"}</td>
                    <td>${payment.total_Received ? payment.total_Received.toFixed(2) : "0.00"}</td>
                    <td>
                        <button class="btn btn-info btn-sm" onclick="viewPayment(${payment.r_Id}, '${payment.receipt_Type}')">عرض</button>
                        <button class="btn btn-success btn-sm" onclick="printInvoice(${payment.r_Id})">طباعة</button>
                    </td>
                `;

                tableBody.appendChild(row);
            });

            const modalElement = document.getElementById("receiptDetailsModal");
            new bootstrap.Modal(modalElement).show();
        })
        .catch(error => {
            console.error("Error loading payments:", error);
            alert(`Failed to load payments: ${error.message}. Please try again.`);
        });
}

function formatDate(date) {
    if (!date || typeof date !== "string" || date.trim() === "") return "";
    const d = new Date(date);
    if (isNaN(d.getTime())) return "";
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, "0");
    const day = String(d.getDate()).padStart(2, "0");
    return `${year}-${month}-${day}`;
}

function openCustomerListPopup() {
    const popup = window.open('customer_list.html', 'customerListPopup', 'width=800,height=600,scrollbars=yes');
    if (!popup) {
        alert('Please allow popups for this site to select a customer.');
    }
}

// Tax Breakdown Toggle
function initializeTaxBreakdownToggle() {
    const toggleTaxBtn = Utils.getElement("toggleTaxBtn");
    const taxBreakdownCollapse = Utils.getElement("taxBreakdownCollapse");

    toggleTaxBtn.addEventListener("click", () => {
        const isCollapsed = taxBreakdownCollapse.classList.contains("show");
        if (isCollapsed) {
            taxBreakdownCollapse.classList.remove("show");
            toggleTaxBtn.innerHTML = '<i class="fas fa-plus me-1"></i> إظهار تفاصيل الضرائب';
        } else {
            taxBreakdownCollapse.classList.add("show");
            toggleTaxBtn.innerHTML = '<i class="fas fa-minus me-1"></i> إخفاء تفاصيل الضرائب';
        }
    });
}

// Main Initialization
document.addEventListener("DOMContentLoaded", async () => {
    try {
        await Promise.all([
            UIInitializer.init(),
            TaxCalculator.init(),
            loadGuestTypes(),
            loadNationalities(),
            loadIdTypes()
        ]);
        initializeDatepickers();
        initializeTaxBreakdownToggle();
    } catch (error) {
        console.error("Error initializing UI:", error);
    }
});