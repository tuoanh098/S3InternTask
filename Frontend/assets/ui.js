(() => {
  // Nav menu mobile
  const navBtn = document.getElementById('btn-menu');
  const navLinks = document.getElementById('nav-links');
  if (navBtn && navLinks) {
    navBtn.addEventListener('click', () => {
      const open = !navLinks.hasAttribute('hidden');
      if (open) navLinks.setAttribute('hidden', '');
      else navLinks.removeAttribute('hidden');
      navBtn.setAttribute('aria-expanded', String(!open));
    });
  }

  // Tạo card sản phẩm (global)
  window.renderProductCard = function renderProductCard(p) {
    const card = document.createElement('article');
    card.className = 'card';
    card.innerHTML = `
      <img src="${p.image}" alt="${p.name}" loading="lazy">
      <h3>${p.name}</h3>
      <p class="price">${(p.price || 0).toLocaleString('vi-VN')}₫</p>
      <button data-id="${p.id}" class="view">Xem</button>
      <button data-id="${p.id}" class="add">Thêm vào giỏ</button>
    `;

    card.addEventListener('click', (e) => {
      const id = e.target?.dataset?.id;
      if (!id) return;
      if (e.target.classList.contains('view')) openProductModal(p);
      if (e.target.classList.contains('add')) window.Cart?.add?.(p);
    });

    return card;
  };

  // Product modal open/close
  function openProductModal(p) {
    const dlg = document.getElementById('product-modal');
    if (!dlg) return;
    dlg.querySelector('#pm-title').textContent = p.name;
    dlg.querySelector('#pm-img').src = p.image;
    dlg.querySelector('#pm-img').alt = p.name;
    dlg.querySelector('#pm-price').textContent = (p.price || 0).toLocaleString('vi-VN') + '₫';
    dlg.querySelector('#pm-desc').textContent = p.description || '';
    dlg.showModal();
    dlg.querySelector('#pm-add').onclick = () => window.Cart?.add?.(p);
    dlg.querySelector('#pm-close').onclick = () => dlg.close();
    dlg.addEventListener('click', (ev) => { if (ev.target === dlg) dlg.close(); });
    dlg.addEventListener('cancel', (ev) => { ev.preventDefault(); dlg.close(); });
  }

  // === Cart sidebar open/close ===
  const btnCart = document.getElementById('btn-cart');
  const dlgCart  = document.getElementById('cart');
  const btnClose = document.getElementById('cart-close');

  if (btnCart && dlgCart) {
    btnCart.addEventListener('click', () => {
      if (window.Cart && typeof window.Cart.render === 'function') window.Cart.render();
      try {
        if (typeof dlgCart.showModal === 'function') dlgCart.showModal();
        else dlgCart.setAttribute('open', '');
      } catch {
        dlgCart.setAttribute('open', '');
      }
    });
  }

  if (btnClose && dlgCart) {
    btnClose.addEventListener('click', () => {
      if (typeof dlgCart.close === 'function') dlgCart.close();
      else dlgCart.removeAttribute('open');
    });
  }

  if (dlgCart) {
    dlgCart.addEventListener('cancel', (e) => {
      e.preventDefault();
      if (typeof dlgCart.close === 'function') dlgCart.close();
      else dlgCart.removeAttribute('open');
    });
    dlgCart.addEventListener('click', (e) => {
      if (e.target === dlgCart) {
        if (typeof dlgCart.close === 'function') dlgCart.close();
        else dlgCart.removeAttribute('open');
      }
    });
  }
})();
