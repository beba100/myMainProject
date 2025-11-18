const rooms = [
    { id: 1, number: "201", type: "Deluxe", status: "occupied", guest: "„Õ„œ √Õ„œ", checkoutTime: "11:00 AM" },
    { id: 2, number: "202", type: "Standard", status: "available", guest: null, checkoutTime: null },
    { id: 3, number: "203", type: "Suite", status: "cleaning", guest: null, checkoutTime: null },
    { id: 4, number: "204", type: "Deluxe", status: "maintenance", guest: null, checkoutTime: null },
    { id: 5, number: "205", type: "Standard", status: "dirty", guest: null, checkoutTime: null },
];

function getStatusClass(status) {
    switch (status) {
        case 'available': return 'available';
        case 'occupied': return 'occupied';
        case 'cleaning': return 'cleaning';
        case 'maintenance': return 'maintenance';
        case 'dirty': return 'dirty';
        default: return 'reserved';
    }
}

function renderRooms() {
    const grid = document.getElementById('roomGrid');
    grid.innerHTML = '';
    rooms.forEach(room => {
        const card = document.createElement('div');
        card.className = 'room-card';
        card.setAttribute("data-id", room.id);
        card.innerHTML = `
      <strong>€—›… ${room.number}</strong><br/>
      <span class="status-badge ${getStatusClass(room.status)}"></span>${room.status}<br/>
      ${room.guest ? `«·÷Ì›: ${room.guest}` : ''}<br/>
      ${room.checkoutTime ? `„Ê⁄œ «·Œ—ÊÃ: ${room.checkoutTime}` : ''}
      <hr/>
      <button class="btn-checkout">Check-out</button>
      <button class="btn-clean">Clean</button>
      <button class="btn-maintain">Maintenance</button>
      <button class="btn-assign">Assign</button>
    `;
        card.addEventListener('click', (e) => {
            if (e.target.classList.contains('btn-checkout')) handleAction('checkout', room);
            else if (e.target.classList.contains('btn-clean')) handleAction('clean', room);
            else if (e.target.classList.contains('btn-maintain')) handleAction('maintenance', room);
            else if (e.target.classList.contains('btn-assign')) handleAction('assign', room);
        });
        grid.appendChild(card);
    });
}

function handleAction(action, room) {
    console.log(`Performing ${action} on room ${room.number}`);
    setTimeout(() => {
        alert(`${action.charAt(0).toUpperCase() + action.slice(1)} performed for Room ${room.number}`);
        if (action === 'clean') room.status = 'available';
        else if (action === 'checkout') room.status = 'dirty';
        else if (action === 'maintenance') room.status = 'maintenance';
        else if (action === 'assign') {
            const guestName = prompt("Enter guest name:");
            if (guestName) {
                room.status = 'occupied';
                room.guest = guestName;
                room.checkoutTime = "11:00 AM";
            }
        }
        renderRooms();
    }, 300);
}

$(document).ready(function () {
    // Devextreme SelectBox for filters
    $('#floor').dxSelectBox({
        items: ['«·ﬂ·', '«·ÿ«»ﬁ 1', '«·ÿ«»ﬁ 2'],
        value: '«·ﬂ·'
    });

    $('#status').dxSelectBox({
        items: ['«·ﬂ·', '„ «Õ', '„‘€Ê·', ' ‰ŸÌ›', '’Ì«‰…'],
        value: '«·ﬂ·',
        onValueChanged: function (e) {
            filterRooms(e.value);
        }
    });

    $('#type').dxSelectBox({
        items: ['«·ﬂ·', '⁄«œÌ', 'œÌ·Êﬂ”', 'Ã‰«Õ'],
        value: '«·ﬂ·'
    });

    renderRooms();
});

function filterRooms(status) {
    if (status === '«·ﬂ·') {
        renderRooms();
        return;
    }
    const filtered = rooms.filter(r => r.status === status.toLowerCase());
    const grid = document.getElementById('roomGrid');
    grid.innerHTML = '';
    filtered.forEach(room => {
        const card = document.createElement('div');
        card.className = 'room-card';
        card.innerHTML = `
      <strong>€—›… ${room.number}</strong><br/>
      <span class="status-badge ${getStatusClass(room.status)}"></span>${room.status}<br/>
      ${room.guest ? `«·÷Ì›: ${room.guest}` : ''}<br/>
      ${room.checkoutTime ? `„Ê⁄œ «·Œ—ÊÃ: ${room.checkoutTime}` : ''}
    `;
        grid.appendChild(card);
    });
}