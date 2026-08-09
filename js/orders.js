/* ============================================================
   Cafe Automate — Checkout + order creation (index.html)
   ============================================================ */

document.addEventListener('DOMContentLoaded', () => {
  const btn = document.getElementById('checkoutBtn');
  if (btn) btn.addEventListener('click', handleCheckout);
});

async function handleCheckout() {
  const user = getUser();
  if (!user) { window.location.href = 'login.html'; return; }
  if (user.role !== 3) { showToast('Only customers can place orders.', 'error'); return; }

  const cart = getCart();
  if (!cart.length) { showToast('Your cart is empty.', 'error'); return; }

  const btn = document.getElementById('checkoutBtn');
  btn.disabled = true;
  btn.textContent = 'Placing order…';

  try {
    const items = cart.map(i => ({
      sourceType: i.sourceType,
      menuItemId: i.menuItemId,
      itemName:   i.itemName,
      unitPrice:  i.unitPrice,
      quantity:   i.quantity
    }));

    const order   = await apiFetch('/orders', { method: 'POST', body: JSON.stringify({ items }) });
    const payment = await apiFetch('/cafe-payment-details').catch(() => null);

    clearCart();
    closeDrawer();
    showOrderConfirmation(order, payment);
  } catch (err) {
    showToast(err.message || 'Order failed. Try again.', 'error');
    btn.disabled = false;
    btn.textContent = 'Checkout';
  }
}

function showOrderConfirmation(order, payment) {
  const overlay = document.getElementById('confirmModal');
  if (!overlay) return;

  document.getElementById('confirmOrderId').textContent = `#${String(order.id).padStart(4, '0')}`;
  document.getElementById('confirmTotal').textContent   = Number(order.totalAmount).toFixed(0);

  const payBlock = document.getElementById('confirmPayBlock');
  if (payBlock && payment) {
    payBlock.innerHTML = `
      <div class="pay-details-block">
        <p><strong>Bank:</strong> ${payment.bankName}</p>
        <p><strong>Account holder:</strong> ${payment.accountHolderName}</p>
        <p><strong>Account #:</strong> ${payment.accountNumber}</p>
        <p><strong>IBAN / Card:</strong> ${payment.iBANOrCardNumber}</p>
        <p style="margin-top:8px;color:var(--ink-soft);font-size:.84rem">${payment.instructions}</p>
      </div>`;
  }

  overlay.classList.add('open');
}

function closeConfirmModal() {
  document.getElementById('confirmModal')?.classList.remove('open');
}
