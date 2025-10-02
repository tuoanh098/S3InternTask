let PRODUCTS = [];
let FILTERED = [];

document.addEventListener('DOMContentLoaded', () => {
  importData();
  bindSearchSort();
});

async function importData() {
  const wrap = document.getElementById('catalog');
  try {
    if (wrap) wrap.textContent = 'Đang tải...';

    const res = await fetch('assets/products.json', { cache: 'no-store' });
    if (!res.ok) {
      const txt = await res.text();
      throw new Error(`Fetch products.json failed: HTTP ${res.status}\n${txt.slice(0,200)}`);
    }
    const data = await res.json();
    PRODUCTS = Array.isArray(data) ? data : [];
    FILTERED = PRODUCTS.slice();
    renderList(FILTERED);
  } catch (e) {
    console.error(e);
    if (wrap) wrap.textContent = 'Không tải được dữ liệu.';
  }
}

function renderList(list) {
  const wrap = document.getElementById('catalog');
  if (!wrap) return;
  wrap.innerHTML = '';
  list.forEach(p => wrap.appendChild(window.renderProductCard(p)));
}

// Search + Sort
function bindSearchSort() {
  const q = document.getElementById('q');
  const sort = document.getElementById('sort');
  const apply = () => {
    const term = (q?.value || '').toLowerCase().trim();
    FILTERED = PRODUCTS.filter(p =>
      p.name.toLowerCase().includes(term) ||
      (p.description || '').toLowerCase().includes(term) ||
      (p.category || '').toLowerCase().includes(term)
    );
    const s = sort?.value || '';
    if (s === 'price-asc') FILTERED.sort((a,b) => a.price - b.price);
    if (s === 'price-desc') FILTERED.sort((a,b) => b.price - a.price);
    if (s === 'rating-desc') FILTERED.sort((a,b) => (b.rating||0) - (a.rating||0));
    renderList(FILTERED);
  };
  let timer;
  q?.addEventListener('input', () => { clearTimeout(timer); timer = setTimeout(apply, 250); });
  sort?.addEventListener('change', apply);
}
