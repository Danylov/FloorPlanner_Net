'use strict';

// This file is the client code: it runs in the browser, not on the server.
//
// - The server (the ASP.NET Core application) only sends index.html, style.css and app.js as ordinary static files
//   (UseDefaultFiles, UseStaticFiles); it does not execute this JavaScript.
// - The browser loads these files and runs app.js.
// - app.js calls the API of the same server with fetch (GET /api/floorplans, POST /api/rooms/{roomId}/furniture, and so on)
//   and shows the answers on the page. The client and the server talk only over HTTP and exchange JSON.
//   The same API can have other clients too, for example Swagger UI or curl.
//
// An important consequence: the checks made here (for example, the search for a free place for the armchair) are only
// for convenience. The real protection is on the server: PlacementValidator and the validation attributes are applied
// to every request, wherever it comes from.

// The page only calls the API. Everything that comes from the API is put into the page through
// textContent and createElement (HTML strings are never inserted into the page), so the data cannot inject HTML or scripts.

// $ is just a short name of our own helper: $('plan-select') returns the element of the page with the id "plan-select".
// It is not jQuery (the page uses no libraries); "$" is an ordinary variable name in JavaScript.
const $ = (id) => document.getElementById(id);

// ---------- common functions ----------

// Calls the API and returns { ok, status, data, elapsedMs }; data is the parsed JSON or the text of the response.
// Every call also shows itself in the "Last request" block of the page.
async function api(method, path, body) {
    // Remember the start time, to measure how long the request takes (milliseconds, measured in the browser).
    const started = performance.now();
    // The result that is returned to the caller; it is filled below, both for success and for a failure.
    let result;

    try {
        // The settings of the request: the HTTP method (GET, POST, DELETE) and the headers (empty for now).
        const options = { method, headers: {} };

        // A body is added only when the caller passes data. POST and PUT (also PATCH) usually carry a body, GET and DELETE do not.
        // The page sends data only with POST; the API also accepts PUT with a body, but the page does not use it.
        if (body !== undefined) {
            // Tell the server that the body is JSON...
            options.headers['Content-Type'] = 'application/json';
            // ...and turn the object into a JSON string.
            options.body = JSON.stringify(body);
        }

        // What the answer of this API contains:
        // - headers: the format of the body (Content-Type: application/json, or application/problem+json for errors) and,
        //   for a created item (201), Location with the address of the new item (the page does not read the headers);
        // - body: the requested or created data, or, for an error, ProblemDetails (title, status, detail; for 409 also
        //   failure and conflictingObjectId; for invalid input data, 400, the list of errors by field).
        //
        // Send the request to the server and wait for the answer (the status and the headers). The page is not frozen meanwhile.
        const response = await fetch(path, options);
        // Wait for the body of the answer and read it as text (it may be empty, for example for 204).
        const text = await response.text();
        // The data of the answer: the parsed JSON, or the text if it is not JSON.
        let data;

        try {
            // Turn the JSON text into objects; an empty body gives null.
            data = text ? JSON.parse(text) : null;
        } catch {
            // The body is not JSON: keep it as plain text.
            data = text;
        }

        // The server answered (any status, including 4xx and 5xx): ok is true only for the statuses 2xx.
        result = { ok: response.ok, status: response.status, data, elapsedMs: Math.round(performance.now() - started) };
    } catch {
        // fetch throws only when there is no answer at all (the server is not running, no network): status 0.
        result = { ok: false, status: 0, data: { title: 'Network error' }, elapsedMs: Math.round(performance.now() - started) };
    }

    // The block "Last request": the method, the address, the status and the time, for example "GET /api/floorplans -> 200 (35 ms)".
    $('status').textContent = `${method} ${path} -> ${result.status} (${result.elapsedMs} ms)`;
    // The block "Last request": the raw answer. Nothing for an empty answer, text as it is, objects as formatted JSON (indent 2).
    $('raw-json').textContent = result.data === null || result.data === undefined
        ? ''
        : (typeof result.data === 'string' ? result.data : JSON.stringify(result.data, null, 2));

    // The caller checks result.ok and result.status and uses result.data; api() does not throw for error answers.
    return result;
}

// Creates an element: props are assigned to it, children are strings (they become text nodes) or other nodes.
function el(tag, props, ...children) {
    const node = document.createElement(tag);

    Object.assign(node, props);

    for (const child of children) {
        node.append(child instanceof Node ? child : document.createTextNode(String(child)));
    }

    return node;
}

// Draws a table: columns is [{ header, value: (row) => string | Node }].
function renderTable(container, columns, rows) {
    const head = el('tr', {}, ...columns.map((column) => el('th', {}, column.header)));
    const body = el('tbody', {}, ...rows.map((row) =>
        el('tr', {}, ...columns.map((column) => el('td', {}, column.value(row))))));

    container.replaceChildren(el('table', {}, el('thead', {}, head), body));
}

// Draws pairs "label: value" as a description list: pairs is [[label, value], ...].
function renderKeyValues(container, pairs) {
    const list = el('dl');

    for (const [label, value] of pairs) {
        list.append(el('dt', {}, label), el('dd', {}, value));
    }

    container.append(list);
}

// Shows an error block: the title, the detail and the list of validation errors (data.errors).
function showProblem(container, result) {
    const data = result.data;
    const block = el('div', { className: 'error' });

    if (typeof data === 'string') {
        block.append(el('strong', {}, data || `Request failed (${result.status})`));
    } else {
        block.append(el('strong', {}, (data && data.title) || `Request failed (${result.status})`));

        if (data && data.detail) {
            block.append(el('p', {}, data.detail));
        }

        if (data && data.errors) {
            const list = el('ul');

            for (const [field, messages] of Object.entries(data.errors)) {
                for (const message of [].concat(messages)) {
                    list.append(el('li', {}, `${field}: ${message}`));
                }
            }

            block.append(list);
        }
    }

    container.replaceChildren(block);

    return block;
}

// Fills a <select>: valueOf and labelOf take an item and return the value and the text of its option.
function fillSelect(select, items, valueOf, labelOf) {
    select.replaceChildren(...items.map((item) => el('option', { value: String(valueOf(item)) }, labelOf(item))));
}

// Shows a gray note instead of the content (for example, "No furniture found").
function showMuted(container, text) {
    container.replaceChildren(el('p', { className: 'muted' }, text));
}

// The lists "Existing categories" always show all the categories of the selected plan, whatever is typed in the text field
// next to them. (The list of suggestions of a text field, <datalist>, shows only the items that match the typed text,
// so it cannot be always full.) The text fields stay free: a new category can still be typed.
function fillCategorySelects(categories) {
    for (const select of [categorySelect, searchCategorySelect]) {
        select.replaceChildren(
            el('option', { value: '' }, 'Choose an existing category'),
            ...categories.map((c) => el('option', { value: c.category }, c.category)));
    }
}

// Choosing a category copies it into the text field and returns the list to its first item, so the full list is ready again.
function bindCategorySelect(select, input) {
    select.addEventListener('change', () => {
        if (select.value) {
            input.value = select.value;
            select.value = '';
        }
    });
}

// The size of a furniture item as the text "width x depth".
const size = (item) => `${item.width}×${item.depth}`;

// Rounds to 2 decimals, to remove the floating point noise. It is used for the area in the search results (the search returns the raw
// product width * depth, for example 0.16000000000000003) and for the coordinates that the examples put into the form.
const roundTo2 = (value) => Math.round(value * 100) / 100;

// An unexpected error of the page itself (the errors of the API are shown by api() and showProblem()) goes to the console.
const reportError = (error) => console.error(error);

// ---------- elements of the page ----------

const planSelect = $('plan-select');
const roomSelect = $('room-select');
const furnitureForm = $('furniture-form');
const searchForm = $('search-form');
const furnitureResult = $('furniture-result');
const summaryResult = $('summary-result');
const categorySelect = $('category-select');
const searchCategorySelect = $('search-category-select');

// The rooms of the selected plan (with their corners) and the furniture of the selected room: the examples use them.
let currentRooms = [];
let currentRoomFurniture = [];

// ---------- plan: summary and furniture ----------

// Loads the summary of the selected plan and draws it: the totals, the statistics by category and the five largest items.
// It also refreshes the lists "Existing categories".
async function loadSummary() {
    const container = summaryResult;
    const result = await api('GET', `/api/floorplans/${planSelect.value}/summary`);

    if (!result.ok) {
        showProblem(container, result);
        return;
    }

    const summary = result.data;

    fillCategorySelects(summary.categories);

    container.replaceChildren();
    renderKeyValues(container, [
        ['Plan', summary.name],
        ['Rooms', summary.roomCount],
        ['Total room area (m²)', summary.totalRoomArea],
        ['Furniture items', summary.furnitureCount],
        ['Has empty rooms', summary.hasEmptyRooms ? 'Yes' : 'No'],
    ]);

    container.append(el('h3', {}, 'By category'));
    const categories = el('div');
    renderTable(categories, [
        { header: 'Category', value: (c) => c.category },
        { header: 'Items', value: (c) => c.count },
        { header: 'Footprint (m²)', value: (c) => c.totalFootprintArea },
    ], summary.categories);
    container.append(categories);

    container.append(el('h3', {}, 'Five largest items'));
    const largest = el('div');
    renderTable(largest, [
        { header: 'Name', value: (i) => i.name },
        { header: 'Category', value: (i) => i.category },
        { header: 'Footprint (m²)', value: (i) => i.footprintArea },
    ], summary.top5Largest);
    container.append(largest);
}

// Loads all the furniture of the selected plan and draws the table "Furniture in plan" (with the room of every item).
async function loadPlanFurniture() {
    const container = $('plan-furniture');
    const result = await api('GET', `/api/floorplans/${planSelect.value}/furniture`);

    if (!result.ok) {
        showProblem(container, result);
        return;
    }

    if (result.data.length === 0) {
        showMuted(container, 'No furniture in this plan');
        return;
    }

    renderTable(container, [
        { header: 'Name', value: (i) => i.name },
        { header: 'Category', value: (i) => i.category },
        { header: 'Size (W×D)', value: size },
        { header: 'Room', value: (i) => i.roomName },
    ], result.data);
}

// ---------- room: add and list furniture ----------

// Loads the rooms of the selected plan, fills the list "Room" and shows the furniture of the first room.
async function loadRooms() {
    const result = await api('GET', `/api/floorplans/${planSelect.value}/rooms`);

    if (!result.ok) {
        showProblem(furnitureResult, result);
        return;
    }

    currentRooms = result.data;
    fillSelect(roomSelect, result.data, (r) => r.id, (r) => `${r.name} (${r.area} m²)`);
    await loadRoomFurniture();
}

// Loads the furniture of the selected room, remembers it for the examples and draws the table with the "Delete" buttons.
async function loadRoomFurniture() {
    const container = $('furniture-list');

    if (!roomSelect.value) {
        currentRoomFurniture = [];
        showMuted(container, 'This plan has no rooms');
        return;
    }

    const result = await api('GET', `/api/rooms/${roomSelect.value}/furniture`);

    if (!result.ok) {
        showProblem(container, result);
        return;
    }

    currentRoomFurniture = result.data;

    if (result.data.length === 0) {
        showMuted(container, 'No furniture in this room');
        return;
    }

    renderTable(container, [
        { header: 'Id', value: (i) => i.id },
        { header: 'Name', value: (i) => i.name },
        { header: 'Category', value: (i) => i.category },
        { header: 'X', value: (i) => i.x },
        { header: 'Z', value: (i) => i.z },
        { header: 'Size (W×D)', value: size },
        { header: '', value: (i) => deleteButton(i.id) },
    ], result.data);
}

// The "Delete" button of a row of the table; a click deletes the furniture item with this id.
function deleteButton(id) {
    const button = el('button', { type: 'button' }, 'Delete');

    button.addEventListener('click', () => deleteFurniture(id));

    return button;
}

// Deletes the furniture item from the selected room and reloads the tables; on an error shows it above the tables.
async function deleteFurniture(id) {
    const result = await api('DELETE', `/api/rooms/${roomSelect.value}/furniture/${id}`);

    if (!result.ok) {
        showProblem(furnitureResult, result);
        return;
    }

    furnitureResult.replaceChildren();
    await refreshPlanData();
}

// Reloads everything that depends on the furniture: the summary, the furniture of the plan and the furniture of the room.
async function refreshPlanData() {
    await loadSummary();
    await loadPlanFurniture();
    await loadRoomFurniture();
}

// Loads everything about the selected plan: the summary, the furniture of the plan and the rooms with their furniture.
// It is called when the page is opened and when another plan is chosen.
async function loadPlan() {
    await loadSummary();
    await loadPlanFurniture();
    await loadRooms();
}

// The "Add furniture" button: sends the values of the form to the server (POST), then shows "Created" or the error
// (for 409 with the reason: outside the room or overlaps another item) and reloads the tables after a success.
furnitureForm.addEventListener('submit', async (event) => {
    event.preventDefault();

    const container = furnitureResult;
    const field = (name) => furnitureForm.elements.namedItem(name).value;
    const number = (name) => Number(field(name));

    const result = await api('POST', `/api/rooms/${roomSelect.value}/furniture`, {
        name: field('name'),
        category: field('category'),
        x: number('x'),
        z: number('z'),
        width: number('width'),
        depth: number('depth'),
        height: number('height'),
    });

    if (result.status === 201) {
        container.replaceChildren(el('div', { className: 'ok' }, `Created: ${result.data.name}, id ${result.data.id}`));

        await refreshPlanData();
        return;
    }

    const block = showProblem(container, result);

    if (result.status === 409 && result.data && result.data.failure) {
        block.append(el('p', {}, `Reason: ${result.data.failure}`));
    }
});

// The three buttons work in the room that is selected in the list "Room" and fill the form with an armchair 0.8 x 0.8 x 0.9:
// - "Fill valid example" looks for a free place in the room: the armchair must be inside the walls and must not overlap the furniture;
// - "Fill overlapping example" puts the armchair exactly on the first piece of furniture of the room;
// - "Fill outside example" puts the armchair outside the room, to the left of its walls.
// The position and the sizes are always replaced; the name and the category are filled only if they are empty,
// so a name that the user has already typed is kept.
const ARMCHAIR = { width: 0.8, depth: 0.8, height: 0.9 };
const GRID_STEP = 0.05; // the same step as in the number fields of the form
const GAP = 0.01; // the free place keeps a gap of 1 cm to the walls and to the furniture, so that rounding cannot make it overlap

// The room that is chosen in the list "Room" (with its corners), or undefined if there is none.
function selectedRoom() {
    return currentRooms.find((room) => String(room.id) === roomSelect.value);
}

// The smallest rectangle around the room: the minimum and maximum X and Z of its corners.
function roomBounds(room) {
    const xs = room.corners.map((c) => c.x);
    const zs = room.corners.map((c) => c.z);

    return { minX: Math.min(...xs), maxX: Math.max(...xs), minZ: Math.min(...zs), maxZ: Math.max(...zs) };
}

// The point is inside the polygon of the room (the ray casting method, the same idea as on the server).
function isInside(corners, x, z) {
    let inside = false;

    for (let i = 0, j = corners.length - 1; i < corners.length; j = i++) {
        const a = corners[i];
        const b = corners[j];

        if ((a.z > z) !== (b.z > z) && x < ((b.x - a.x) * (z - a.z)) / (b.z - a.z) + a.x) {
            inside = !inside;
        }
    }

    return inside;
}

// The rectangle is inside the room when all its 4 corners are inside (this is the rule of the server).
function rectInside(corners, rect) {
    return [[rect.minX, rect.minZ], [rect.maxX, rect.minZ], [rect.maxX, rect.maxZ], [rect.minX, rect.maxZ]]
        .every(([x, z]) => isInside(corners, x, z));
}

// Rectangles that only touch each other do not overlap (the same as on the server).
function overlaps(a, b) {
    return a.minX < b.maxX && a.maxX > b.minX && a.minZ < b.maxZ && a.maxZ > b.minZ;
}

// The rectangle that a furniture item occupies on the floor (it is described by its center and its sizes).
function footprint(item) {
    return { minX: item.x - item.width / 2, maxX: item.x + item.width / 2, minZ: item.z - item.depth / 2, maxZ: item.z + item.depth / 2 };
}

// How the free place and the server work together (the server first CHECKS and then SAVES, not the other way round):
// 1. Here, in the client, a free place is only looked for, and x and z are put into the fields of the form.
//    Nothing is sent to the server at this moment (apart from reading the current furniture of the room).
// 2. When the user presses "Add furniture", the client sends POST /api/rooms/{roomId}/furniture with the position and sizes.
// 3. The server (FurnitureService.CreateAsync) creates the item and asks PlacementValidator BEFORE saving it:
//    is it inside the room and does it not overlap other furniture?
//    - if the check passes, the item is added to the room and saved in the database (201);
//    - if not, PlacementException is thrown, NOTHING gets into the database, and the answer is 409 with the reason.
// So the search here is only a hint: if the place has been taken meanwhile, or the user has changed the coordinates,
// the server checks everything again and rejects the item.
//
// Looks for a free place for the armchair: the centers are tried on a grid (row by row), the first suitable one is taken.
function findFreePlace(room, furniture) {
    const { minX, maxX, minZ, maxZ } = roomBounds(room);
    // The armchair together with the gap around it.
    const width = ARMCHAIR.width + 2 * GAP;
    const depth = ARMCHAIR.depth + 2 * GAP;
    const occupied = furniture.map(footprint);

    for (let row = 0; minZ + row * GRID_STEP <= maxZ; row++) {
        for (let column = 0; minX + column * GRID_STEP <= maxX; column++) {
            const x = roundTo2(minX + column * GRID_STEP);
            const z = roundTo2(minZ + row * GRID_STEP);
            const rect = footprint({ x, z, width, depth });

            if (rectInside(room.corners, rect) && !occupied.some((other) => overlaps(rect, other))) {
                return { x, z };
            }
        }
    }

    return null;
}

// Puts the example into the form: the position and the sizes are replaced, the name and the category are filled only if empty.
function fillForm(x, z) {
    furnitureResult.replaceChildren();

    const values = { x, z, ...ARMCHAIR };

    for (const [name, value] of Object.entries(values)) {
        furnitureForm.elements.namedItem(name).value = value;
    }

    for (const [name, value] of Object.entries({ name: 'Armchair', category: 'Chair' })) {
        const input = furnitureForm.elements.namedItem(name);

        if (!input.value.trim()) {
            input.value = value;
        }
    }
}

// Reads the current furniture of the selected room and returns the room (or null if there is no room to work in).
async function prepareExample() {
    const room = selectedRoom();

    if (!room) {
        showProblem(furnitureResult, { data: { title: 'No room selected', detail: 'This plan has no rooms.' } });
        return null;
    }

    await loadRoomFurniture();

    return room;
}

// "Fill valid example": finds a free place in the selected room and puts it into the form.
$('fill-valid').addEventListener('click', async () => {
    const room = await prepareExample();

    if (!room) {
        return;
    }

    const place = findFreePlace(room, currentRoomFurniture);

    if (!place) {
        const detail = `There is no free place for an armchair 0.8 x 0.8 m in the room "${room.name}".`;
        showProblem(furnitureResult, { data: { title: 'No free place', detail } });
        return;
    }

    fillForm(place.x, place.z);
});

// "Fill overlapping example": puts the armchair on the first furniture item of the room, so the server rejects it.
$('fill-overlapping').addEventListener('click', async () => {
    const room = await prepareExample();

    if (!room) {
        return;
    }

    if (currentRoomFurniture.length === 0) {
        const detail = `The room "${room.name}" has no furniture to overlap with.`;
        showProblem(furnitureResult, { data: { title: 'Nothing to overlap', detail } });
        return;
    }

    fillForm(currentRoomFurniture[0].x, currentRoomFurniture[0].z);
});

// "Fill outside example": puts the armchair to the left of the walls of the room, so the server rejects it.
$('fill-outside').addEventListener('click', async () => {
    const room = await prepareExample();

    if (!room) {
        return;
    }

    const { minX, minZ, maxZ } = roomBounds(room);

    fillForm(roundTo2(minX - 1), roundTo2((minZ + maxZ) / 2));
});

// ---------- search ----------

// Searches the furniture of all the plans by category (all categories if the field is empty) and draws the table
// with the room and the plan of every found item.
async function search() {
    const container = $('search-result');
    const category = searchForm.elements.namedItem('category').value.trim();
    const query = category ? `?category=${encodeURIComponent(category)}` : '';
    const result = await api('GET', `/api/furniture${query}`);

    if (!result.ok) {
        showProblem(container, result);
        return;
    }

    if (result.data.items.length === 0) {
        showMuted(container, 'No furniture found');
        return;
    }

    renderTable(container, [
        { header: 'Name', value: (i) => i.name },
        { header: 'Category', value: (i) => i.category },
        { header: 'Size (W×D)', value: size },
        { header: 'Area (m²)', value: (i) => roundTo2(i.area) },
        { header: 'Room', value: (i) => i.roomName },
        { header: 'Plan', value: (i) => i.planName },
    ], result.data.items);
}

// The "Search" button: runs the search instead of reloading the page, as a form would do by default.
searchForm.addEventListener('submit', (event) => {
    event.preventDefault();
    search().catch(reportError);
});

// ---------- start ----------

// Connects the lists "Existing categories" with the text fields of the two forms.
bindCategorySelect(categorySelect, furnitureForm.elements.namedItem('category'));
bindCategorySelect(searchCategorySelect, searchForm.elements.namedItem('category'));

// Choosing another plan reloads its data; choosing another room shows the furniture of that room.
planSelect.addEventListener('change', loadPlan);
roomSelect.addEventListener('change', loadRoomFurniture);

// Runs when the page is opened: loads the list of plans, selects the first one, shows its data and runs the first search.
async function start() {
    const result = await api('GET', '/api/floorplans');

    if (!result.ok) {
        showProblem(summaryResult, result);
        return;
    }

    if (result.data.length === 0) {
        showMuted(summaryResult, 'There are no floor plans yet');
        return;
    }

    fillSelect(planSelect, result.data, (p) => p.id, (p) => p.name);
    planSelect.value = String(result.data[0].id);

    await loadPlan();
    await search();
}

start().catch(reportError);
