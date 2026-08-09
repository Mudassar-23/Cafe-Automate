/* ============================================================
   Cafe Automate — Cart (sessionStorage, mixed menu sources)
   ============================================================ */

const CART_KEY = 'ca_cart';

function loadCart() {
  try { return JSON.parse(sessionStorage.getItem(CART_KEY)) || []; } catch { return []; }
}

function saveCart(items) {
  sessionStorage.setItem(CART_KEY, JSON.stringify(items));
}

function getCart() { return loadCart(); }

function addToCart(item) {
  // item: { sourceType, menuItemId, itemName, unitPrice, emoji }
  const cart = loadCart();
  const existing = cart.find(c => c.sourceType === item.sourceType && c.menuItemId === item.menuItemId);
  if (existing) {
    existing.quantity += 1;
  } else {
    cart.push({ ...item, quantity: 1 });
  }
  saveCart(cart);
  updateCartUI();
  animateCartBadge();
}

function removeFromCart(sourceType, menuItemId) {
  const cart = loadCart().filter(c => !(c.sourceType === sourceType && c.menuItemId === menuItemId));
  saveCart(cart);
  updateCartUI();
}

function setQuantity(sourceType, menuItemId, qty) {
  const cart = loadCart();
  const item = cart.find(c => c.sourceType === sourceType && c.menuItemId === menuItemId);
  if (!item) return;
  if (qty <= 0) { removeFromCart(sourceType, menuItemId); return; }
  item.quantity = qty;
  saveCart(cart);
  updateCartUI();
}

function clearCart() {
  sessionStorage.removeItem(CART_KEY);
  updateCartUI();
}

function cartTotal() {
  return loadCart().reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);
}

function cartCount() {
  return loadCart().reduce((sum, i) => sum + i.quantity, 0);
}

// ── UI update ─────────────────────────────────────────────────
function updateCartUI() {
  const count = cartCount();
  document.querySelectorAll('.cart-count').forEach(el => { el.textContent = count; });

  const cartItems  = document.getElementById('cartItems');
  const cartTotal_ = document.getElementById('cartTotalAmt');
  const checkoutBtn = document.getElementById('checkoutBtn');

  if (!cartItems) return;

  const cart = loadCart();
  if (cart.length === 0) {
    cartItems.innerHTML = `<div class="empty-cart"><div class="emoji">🧺</div><p>Your cart is empty.<br>Add something delicious!</p></div>`;
    if (checkoutBtn) checkoutBtn.disabled = true;
    if (cartTotal_) cartTotal_.textContent = '0';
    return;
  }

  cartItems.innerHTML = cart.map(item => `
    <div class="cart-item" data-src="${item.sourceType}" data-id="${item.menuItemId}">
      <div class="thumb">${item.emoji || '☕'}</div>
      <div class="info">
        <h4>${escHtml(item.itemName)}</h4>
        <div class="price">Rs ${(item.unitPrice * item.quantity).toFixed(0)}</div>
        <span class="item-source-tag">${item.sourceType === 'DailyMenu' ? 'Today\'s pick' : 'All Menu'}</span>
      </div>
      <div>
        <div class="qty-control">
          <button onclick="setQuantity('${item.sourceType}',${item.menuItemId},${item.quantity - 1})">−</button>
          <span>${item.quantity}</span>
          <button onclick="setQuantity('${item.sourceType}',${item.menuItemId},${item.quantity + 1})">+</button>
        </div>
        <button class="remove-btn" onclick="removeFromCart('${item.sourceType}',${item.menuItemId})">Remove</button>
      </div>
    </div>
  `).join('');

  if (cartTotal_) cartTotal_.textContent = cartTotal().toFixed(0);
  if (checkoutBtn) checkoutBtn.disabled = false;
}

function animateCartBadge() {
  document.querySelectorAll('.cart-count').forEach(el => {
    el.classList.remove('bounce');
    void el.offsetWidth;
    el.classList.add('bounce');
    setTimeout(() => el.classList.remove('bounce'), 300);
  });
}

function escHtml(s) {
  const d = document.createElement('div');
  d.textContent = s;
  return d.innerHTML;
}

// ── Init on load ──────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', updateCartUI);
