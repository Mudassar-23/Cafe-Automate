/* ============================================================
   Cafe Automate — Public menu rendering (index.html)
   ============================================================ */

async function loadDailyMenu() {
  const grid = document.getElementById('dailyGrid');
  if (!grid) return;

  try {
    const items = await apiFetch('/daily-menu');
    if (!items || items.length === 0) {
      grid.innerHTML = `<div class="daily-empty"><p>No daily specials today. Check back soon!</p></div>`;
      return;
    }
    grid.innerHTML = items.map(renderDailyCard).join('');
  } catch {
    grid.innerHTML = `<div class="daily-empty"><p>Could not load today's menu.</p></div>`;
  }
}

function renderDailyCard(item) {
  const soldOut = item.status === 'SoldOut';
  return `
    <div class="daily-card ${soldOut ? 'sold-out' : ''}">
      ${soldOut ? `<div class="sold-out-stamp">SOLD OUT</div>` : ''}
      <div class="daily-body">
        <div class="daily-tag">Today</div>
        <h3>${escHtml(item.name)}</h3>
        <p>Fresh today · ${item.quantity} left</p>
        <div class="daily-foot">
          <span class="price">Rs ${Number(item.price).toFixed(0)}</span>
          <button class="add-btn daily-add"
            ${soldOut ? 'disabled' : ''}
            onclick="${soldOut ? 'shakeBtn(this)' : `addToCart({sourceType:'DailyMenu',menuItemId:${item.id},itemName:'${escJs(item.name)}',unitPrice:${item.price},emoji:'${item.emoji}'})`}"
            aria-label="Add ${item.name} to cart">
            +
          </button>
        </div>
      </div>
    </div>`;
}

async function loadAllMenu() {
  const grid = document.getElementById('menuGrid');
  if (!grid) return;

  try {
    const items = await apiFetch('/all-menu');
    if (!items || items.length === 0) {
      grid.innerHTML = `<div class="menu-empty"><p>Menu coming soon!</p></div>`;
      return;
    }

    window._allMenuItems = items;
    renderMenuGrid('all');

    // Wire category tabs
    document.querySelectorAll('.tab').forEach(btn => {
      btn.addEventListener('click', () => {
        document.querySelectorAll('.tab').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        renderMenuGrid(btn.dataset.cat);
      });
    });
  } catch {
    grid.innerHTML = `<div class="menu-empty"><p>Could not load the menu.</p></div>`;
  }
}

function renderMenuGrid(cat) {
  const grid  = document.getElementById('menuGrid');
  const items = window._allMenuItems || [];
  const filtered = cat === 'all' ? items : items.filter(i => i.category?.toLowerCase() === cat);

  if (filtered.length === 0) {
    grid.innerHTML = `<div class="menu-empty"><p>No items in this category yet.</p></div>`;
    return;
  }

  grid.innerHTML = filtered.map(renderMenuCard).join('');
}

function renderMenuCard(item) {
  const soldOut = !item.isAvailable;

  return `
    <div class="menu-card ${soldOut ? 'sold-out' : ''}">
      <div class="menu-visual">
        <span style="font-size:2.8rem">${escHtml(item.emoji || '☕')}</span>
        <div class="badge">${item.category || 'Menu'}</div>
        ${soldOut ? `<div class="sold-out-stamp">SOLD OUT</div>` : ''}
      </div>
      <div class="menu-info">
        <h3>${escHtml(item.name)}</h3>
        <p>${escHtml(item.description || '')}</p>
      </div>
      <div class="menu-foot">
        <span class="price">Rs ${Number(item.price).toFixed(0)}</span>
        <button class="add-btn"
          ${soldOut ? 'disabled' : ''}
          onclick="${soldOut ? 'shakeBtn(this)' : `addToCart({sourceType:'AllMenu',menuItemId:${item.id},itemName:'${escJs(item.name)}',unitPrice:${item.price},emoji:'${escJs(item.emoji || '☕')}'})`}"
          aria-label="Add ${escHtml(item.name)} to cart">
          +
        </button>
      </div>
    </div>`;
}

function shakeBtn(btn) {
  btn.classList.add('shake');
  setTimeout(() => btn.classList.remove('shake'), 400);
}

function escHtml(s = '') {
  const d = document.createElement('div');
  d.textContent = String(s);
  return d.innerHTML;
}

function escJs(s = '') {
  return String(s).replace(/'/g, "\\'").replace(/"/g, '\\"');
}

document.addEventListener('DOMContentLoaded', () => {
  loadDailyMenu();
  loadAllMenu();
});
