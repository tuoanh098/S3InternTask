const CART_KEY = 'mini_shop_cart_v1';

function getCartItems() {
  try { return JSON.parse(localStorage.getItem(CART_KEY)) || []; }
  catch { return []; }
}
function money(n) { return (n || 0).toLocaleString('vi-VN') + '₫'; }

function renderSummary() {
  const items = getCartItems();
  const wrap = document.getElementById('sum-items');
  const btn = document.getElementById('btn-submit');

  if (!items.length) {
    wrap.innerHTML = `<p>Giỏ hàng trống. <a class="btn" href="index.html">Quay lại mua sắm</a></p>`;
    document.getElementById('sum-subtotal').textContent = money(0);
    document.getElementById('sum-total').textContent = money(0);
    if (btn) btn.disabled = true;
    return;
  }

  if (btn) btn.disabled = false;

  wrap.innerHTML = items.map(it => `
    <div class="sum-row">
      <div>
        <div class="name">${it.name}</div>
        <div class="meta">${it.qty} × ${money(it.price)}</div>
      </div>
      <strong>${money(it.price * it.qty)}</strong>
    </div>
  `).join('');

  const subtotal = items.reduce((s, it) => s + it.price * it.qty, 0);
  document.getElementById('sum-subtotal').textContent = money(subtotal);
  document.getElementById('sum-total').textContent = money(subtotal);
}

function setInvalid(fieldEl, msg) {
  const field = fieldEl.closest('.field');
  if (!field) return;
  field.dataset.invalid = msg ? 'true' : 'false';
  const err = field.querySelector('.error');
  if (err) err.textContent = msg || '';
}

function validateForm(form) {
  let ok = true;
  const requiredIds = ['full_name', 'email', 'phone', 'address', 'city', 'district'];
  requiredIds.forEach(id => {
    const el = form.querySelector('#' + id);
    if (!el) return;
    let msg = '';
    if (!el.value.trim()) msg = 'Trường này là bắt buộc.';
    else if (el.type === 'email' && !el.checkValidity()) msg = 'Email không hợp lệ.';
    else if (el.id === 'phone' && !el.checkValidity()) msg = 'Số điện thoại không hợp lệ.';
    setInvalid(el, msg);
    if (msg && ok) el.focus();
    ok = ok && !msg;
  });

  const agree = document.getElementById('agree');
  if (agree && !agree.checked) {
    agree.focus();
    ok = false;
    alert('Vui lòng đồng ý điều khoản mua hàng.');
  }
  return ok;
}

function handleSubmit(e) {
  e.preventDefault();
  const form = e.currentTarget;
  if (!validateForm(form)) return;

  const items = getCartItems();
  if (!items.length) return alert('Giỏ hàng trống.');

  const subtotal = items.reduce((s, it) => s + it.price * it.qty, 0);
  const order = {
    id: Date.now(),
    created_at: new Date().toISOString(),
    items,
    amount: { subtotal, shipping: 0, total: subtotal },
    customer: {
      full_name: form.full_name.value.trim(),
      email: form.email.value.trim(),
      phone: form.phone.value.trim(),
      address: form.address.value.trim(),
      city: form.city.value.trim(),
      district: form.district.value.trim(),
      note: form.note.value.trim(),
    },
    payment: form.querySelector('input[name="payment"]:checked')?.value || 'cod',
    status: 'created'
  };

  sessionStorage.setItem('mini_shop_last_order', JSON.stringify(order));
  localStorage.setItem(CART_KEY, JSON.stringify([]));
  window.location.href = 'thanks.html';
}

document.addEventListener('DOMContentLoaded', () => {
  renderSummary();
  const form = document.getElementById('checkout-form');
  if (form) form.addEventListener('submit', handleSubmit);
});
