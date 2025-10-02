const KEY = 'mini_shop_cart_v1';
const state = { items: load() };

function save(){ localStorage.setItem(KEY, JSON.stringify(state.items)); render(); }
function load(){ try{ return JSON.parse(localStorage.getItem(KEY)) || []; }catch{ return []; } }

function add(p){
  const found = state.items.find(i => i.id === p.id);
  if (found) found.qty += 1; else state.items.push({ id:p.id, name:p.name, price:p.price, image:p.image, qty:1 });
  save();
}
function remove(id){ const i = state.items.findIndex(x=>x.id===id); if(i>-1){ state.items.splice(i,1); save(); } }
function setQty(id, qty){ const it=state.items.find(x=>x.id===id); if(!it) return; it.qty=Math.max(1, qty|0); save(); }
function total(){ return state.items.reduce((s,i)=> s + i.price*i.qty, 0); }

function render(){
  const cnt = document.getElementById('cart-count'); if (cnt) cnt.textContent = state.items.reduce((s,i)=>s+i.qty,0);
  const list = document.getElementById('cart-items'); if (!list) return;
  list.innerHTML = state.items.map(i => `
    <div class="row">
      <img src="${i.image}" alt="${i.name}" />
      <div class="grow">
        <strong>${i.name}</strong>
        <div class="muted">${i.price.toLocaleString('vi-VN')}₫</div>
        <label>SL <input type="number" min="1" value="${i.qty}" data-id="${i.id}" class="qty"></label>
      </div>
      <button class="rm" data-id="${i.id}">X</button>
    </div>
  `).join('');
  const t = document.getElementById('cart-total'); if (t) t.textContent = total().toLocaleString('vi-VN') + '₫';
  list.onclick = (e) => { if (e.target.classList.contains('rm')) remove(e.target.dataset.id); };
  list.oninput = (e) => { if (e.target.classList.contains('qty')) setQty(e.target.dataset.id, e.target.value); };
}
window.Cart = { add, remove, setQty, total, render };
window.addEventListener('DOMContentLoaded', render);
