let currentFilter = '';
let currentSortBy = 'name';
let currentSortDir = 'asc';
let currentContacts = [];
let editingId = null;
let currentPage = 1;
let pageSize = 5;

function updateFileName(input) {
    const display = document.getElementById('fileNameDisplay');
    display.textContent = input.files.length > 0 ? input.files[0].name : 'Choose CSV file';
}

async function loadContacts() {
    const tbody = document.getElementById('contactsTableBody');
    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center; padding:30px; color:#999;">Loading...</td></tr>';

    const params = new URLSearchParams({
        sortBy: currentSortBy,
        sortDir: currentSortDir,
        filter: currentFilter,
        page: currentPage,
        pageSize: pageSize
    });
    const response = await fetch('/Contacts/GetContacts?' + params.toString());
    const result = await response.json();
    currentContacts = result.data;
    renderTable();
    renderPagination(result.total);
    updateStats(result.total);
}

let deletedCount = 0;
function updateStats(total) {
    document.getElementById('statTotal').textContent = total;
    document.getElementById('statImported').textContent = total;
    document.getElementById('statUpdated').textContent = new Date().toLocaleDateString();
    document.getElementById('statDeleted').textContent = deletedCount;
}

function renderTable() {
    const tbody = document.getElementById('contactsTableBody');
    tbody.innerHTML = '';

    currentContacts.forEach(contact => {
        const row = document.createElement('tr');

        if (editingId === contact.id) {
            row.innerHTML = `
                <td><input type="text" id="edit-name-${contact.id}" value="${contact.name}" /></td>
                <td><input type="date" id="edit-dob-${contact.id}" value="${contact.dateOfBirth.substring(0, 10)}" min="1900-01-01" max="2026-12-31" /></td>
                <td>
                    <label class="switch">
                        <input type="checkbox" id="edit-married-${contact.id}" ${contact.married ? 'checked' : ''} />
                        <span class="slider"></span>
                    </label>
                </td>
                <td><input type="text" id="edit-phone-${contact.id}" value="${contact.phone}" /></td>
                <td><input type="number" step="0.01" id="edit-salary-${contact.id}" value="${contact.salary}" /></td>
                <td>
                    <button class="action-btn save" onclick="saveContact(${contact.id})" title="Save">✓</button>
                    <button class="action-btn cancel" onclick="cancelEdit()" title="Cancel">✕</button>
                </td>
            `;
        } else {
            row.innerHTML = `
                <td>${contact.name}</td>
                <td>${new Date(contact.dateOfBirth).toLocaleDateString()}</td>
                <td>
                    <span class="badge ${contact.married ? 'badge-married' : 'badge-single'}">
                        ${contact.married ? 'Married' : 'Single'}
                    </span>
                </td>
                <td>${contact.phone}</td>
                <td>${contact.salary.toFixed(2)}</td>
                <td>
                    <button class="action-btn edit" onclick="editContact(${contact.id})" title="Edit">✎</button>
                    <button class="action-btn delete" onclick="deleteContact(${contact.id})" title="Delete">🗑</button>
                </td>
            `;
        }
        tbody.appendChild(row);
    });
}

function editContact(id) {
    editingId = id;
    renderTable();
}

function cancelEdit() {
    editingId = null;
    renderTable();
}

async function saveContact(id) {
    const updated = {
        id: id,
        name: document.getElementById(`edit-name-${id}`).value,
        dateOfBirth: document.getElementById(`edit-dob-${id}`).value,
        married: document.getElementById(`edit-married-${id}`).checked,
        phone: document.getElementById(`edit-phone-${id}`).value,
        salary: parseFloat(document.getElementById(`edit-salary-${id}`).value)
    };

    const response = await fetch('/Contacts/UpdateContact', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(updated)
    });

    if (!response.ok) {
        const error = await response.json();
        alert('Error: ' + error.message);
        return;
    }

    editingId = null;
    loadContacts();
}

function renderPagination(total) {
    const container = document.getElementById('pagination');
    const totalPages = Math.max(1, Math.ceil(total / pageSize));

    let pageButtons = '';
    for (let i = 1; i <= totalPages; i++) {
        pageButtons += `<button class="${i === currentPage ? 'active-page' : ''}" onclick="goToPage(${i})">${i}</button>`;
    }

    container.innerHTML = `
        <button onclick="goToPage(${currentPage - 1})" ${currentPage <= 1 ? 'disabled' : ''}>Prev</button>
        ${pageButtons}
        <button onclick="goToPage(${currentPage + 1})" ${currentPage >= totalPages ? 'disabled' : ''}>Next</button>
    `;
}

function goToPage(page) {
    currentPage = page;
    loadContacts();
}

document.querySelectorAll('th[data-sort]').forEach(th => {
    th.addEventListener('click', () => {
        const column = th.getAttribute('data-sort');
        if (currentSortBy === column) {
            currentSortDir = currentSortDir === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortBy = column;
            currentSortDir = 'asc';
        }
        loadContacts();
    });
});

let filterTimer = null;
document.getElementById('filterInput').addEventListener('input', (e) => {
    clearTimeout(filterTimer);
    filterTimer = setTimeout(() => {
        currentFilter = e.target.value;
        currentPage = 1;
        loadContacts();
    }, 300);
});

async function deleteContact(id) {
    const confirmed = confirm('Are you sure you want to delete this contact?');
    if (!confirmed) return;

    const formData = new URLSearchParams();
    formData.append('id', id);

    await fetch('/Contacts/DeleteContact', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData.toString()
    });
    deletedCount++;
    loadContacts();
}

document.getElementById('sortSelect').addEventListener('change', (e) => {
    const [column, dir] = e.target.value.split('-');
    currentSortBy = column;
    currentSortDir = dir;
    currentPage = 1;
    loadContacts();
});

loadContacts();

document.addEventListener('wheel', (e) => {
    if (document.activeElement.tagName === 'INPUT' &&
        (document.activeElement.type === 'number' || document.activeElement.type === 'date')) {
        document.activeElement.blur();
    }
});