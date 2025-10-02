(function () {
  const STORAGE_KEY = 'mini_shop_last_order';
  const money = n => (n || 0).toLocaleString('vi-VN') + '₫';
  const el = id => document.getElementById(id);

  function showEmpty() {
    el('empty').hidden = false;
    el('order-wrap').hidden = true;
  }

  function render(order) {
    el('order-wrap').hidden = false;
    el('empty').hidden = true;

    el('order-id').textContent = '#' + String(order.id);
    el('order-date').textContent = new Date(order.created_at).toLocaleString('vi-VN');
    el('order-payment').textContent = order.payment === 'cod' ? 'Thanh toán khi nhận hàng (COD)' : order.payment;
    el('order-status').textContent = order.status || 'created';

    const c = order.customer || {};
    el('order-name').textContent = c.full_name || '';
    el('order-address').textContent = `${c.address || ''}, ${c.district || ''}, ${c.city || ''}`.replaceAll(' ,', ',').replace(/^,\\s*/,'');
    el('order-contact').textContent = `${c.phone || ''}${c.email ? ' · ' + c.email : ''}`;

    const items = order.items || [];
    const wrap = el('sum-items');
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
    el('sum-subtotal').textContent = money(subtotal);
    el('sum-total').textContent = money(subtotal);
  }

  document.addEventListener('DOMContentLoaded', () => {
    const raw = sessionStorage.getItem(STORAGE_KEY);
    if (!raw) return showEmpty();

    let order;
    try { order = JSON.parse(raw); } catch { return showEmpty(); }
    if (!order || !Array.isArray(order.items) || !order.items.length) return showEmpty();

    render(order);

    const btnPrint = el('btn-print');
    if (btnPrint) btnPrint.addEventListener('click', () => window.print());
  });
})();
