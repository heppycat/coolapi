"use strict";
const $ = (s, r = document) => r.querySelector(s);
const esc = v => String(v ?? "").replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
const money = n => new Intl.NumberFormat(undefined, { style: "currency", currency: "USD" }).format(n || 0);
const COLORS = ["#1d9bf0", "#7856ff", "#00ba7c", "#f91880", "#ff7a00", "#ffd400"];
const color = k => COLORS[Math.abs(Number(k) || 0) % COLORS.length];
const initial = s => (s || "?").trim()[0].toUpperCase();
const avatar = (label, key) => `<div class="avatar" style="background:${color(key)}">${esc(initial(label))}</div>`;
const ago = d => {
  const s = (Date.now() - new Date(/Z|[+-]\d\d:\d\d$/.test(d) ? d : d + "Z")) / 1000;
  if (s < 60) return "just now";
  if (s < 3600) return Math.floor(s / 60) + "m";
  if (s < 86400) return Math.floor(s / 3600) + "h";
  return new Date(d).toLocaleDateString(undefined, { month: "short", day: "numeric" });
};
const ICON = {
  shop: '<path d="M3 9l1-5h16l1 5M3 9v11h18V9M3 9h18M9 20v-6h6v6"/>',
  cart: '<circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/><path d="M1 1h4l2.7 13.4a2 2 0 0 0 2 1.6h9.7a2 2 0 0 0 2-1.6L23 6H6"/>',
  orders: '<path d="M6 2 3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4zM3 6h18M16 10a4 4 0 0 1-8 0"/>',
  products: '<path d="M21 8 12 3 3 8v8l9 5 9-5zM3 8l9 5 9-5M12 13v8"/>',
  customers: '<path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8M23 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8"/>',
  users: '<path d="M12 2 4 5v6c0 5 3.4 9.7 8 11 4.6-1.3 8-6 8-11V5z"/>',
};

const loadCart = () => { try { return JSON.parse(localStorage.getItem("cart")) || {}; } catch { return {}; } };
const state = { token: localStorage.getItem("token"), me: null, data: {}, q: "", cart: loadCart() };
const saveCart = () => { try { localStorage.setItem("cart", JSON.stringify(state.cart)); } catch {} };
const cartCount = () => Object.values(state.cart).reduce((a, n) => a + n, 0);
const isAdmin = () => state.me && (state.me.role === 1 || state.me.role === "Admin");

function toast(msg, bad) {
  const t = document.createElement("div");
  t.className = "toast" + (bad ? " bad" : "");
  t.textContent = msg;
  $("#toasts").append(t);
  setTimeout(() => t.remove(), 3200);
}

async function api(method, path, body) {
  const res = await fetch("/api" + path, {
    method,
    headers: { ...(body ? { "Content-Type": "application/json" } : {}), ...(state.token ? { Authorization: "Bearer " + state.token } : {}) },
    body: body ? JSON.stringify(body) : undefined,
  });
  if (res.status === 401 && state.token) { logout(); throw new Error("Session expired"); }
  if (!res.ok) {
    let msg = res.statusText || "Request failed";
    try {
      const j = await res.json();
      const errs = j.errors && Object.values(j.errors).flat();
      msg = (errs && errs.join(" ")) || j.detail || j.title || j.message || msg;
    } catch {}
    if (res.status === 403) msg = "Admins only";
    throw new Error(msg);
  }
  return res.status === 204 ? null : res.json().catch(() => null);
}

function logout() {
  localStorage.removeItem("token");
  state.token = null; state.me = null; state.data = {};
  render();
}

/* ---------- auth ---------- */
function renderAuth(mode = "login") {
  $("#app").innerHTML = `
  <div class="auth"><form class="card" id="af">
    <div class="logo" style="margin:0">🛍</div>
    <h1>${mode === "login" ? "Sign in to coolShop" : "Create your account"}</h1>
    <input class="field" name="email" type="email" placeholder="Email" required autocomplete="username">
    <input class="field" name="password" type="password" placeholder="Password" required autocomplete="${mode === "login" ? "current-password" : "new-password"}">
    <div class="err" id="err"></div>
    <button class="btn" style="height:48px">${mode === "login" ? "Log in" : "Sign up"}</button>
    <div class="muted">${mode === "login" ? `No account? <a href="#" id="sw">Sign up</a>` : `Have an account? <a href="#" id="sw">Log in</a>`}</div>
  </form></div>`;
  $("#sw").onclick = e => { e.preventDefault(); renderAuth(mode === "login" ? "register" : "login"); };
  $("#af").onsubmit = async e => {
    e.preventDefault();
    const f = Object.fromEntries(new FormData(e.target));
    const btn = $("#af .btn"); btn.disabled = true;
    try {
      const r = await api("POST", "/auth/" + mode, f);
      state.token = r.token; localStorage.setItem("token", r.token);
      state.me = r.user;
      render();
    } catch (x) { $("#err").textContent = x.message; btn.disabled = false; }
  };
}

/* ---------- shell ---------- */
const NAMES = { shop: "Shop", cart: "Cart", orders: "Orders", customers: "Customers", users: "Users" };
const routes = () => ["shop", "cart", "orders", ...(isAdmin() ? ["customers", "users"] : [])];
const route = () => { const r = location.hash.slice(2); return routes().includes(r) ? r : "shop"; };

async function render() {
  if (!state.token) return renderAuth();
  if (!state.me) {
    try { state.me = await api("GET", "/users/me"); } catch { return renderAuth(); }
  }
  const r = route();
  $("#app").innerHTML = `
  <div class="shell">
    <header class="left">
      <div class="logo" onclick="location.hash='#/shop'">🛍</div>
      <nav class="nav">${routes().map(k => `<a href="#/${k}" class="${k === r ? "on" : ""}"><svg viewBox="0 0 24 24">${ICON[k]}</svg><span>${NAMES[k]}</span>${k === "cart" ? `<i class="badge" id="badge"></i>` : ""}</a>`).join("")}</nav>
      <div class="me" id="me" title="Click to log out">
        ${avatar(state.me.email, state.me.id)}
        <div class="who"><b>${esc(state.me.email.split("@")[0])}</b><span>${isAdmin() ? "Admin" : "Customer"} · log out</span></div>
      </div>
    </header>
    <main class="main"><div class="head"><h1>${NAMES[r]}</h1><span class="muted" id="count"></span></div><div id="view"><div class="skeleton"></div><div class="skeleton"></div><div class="skeleton"></div></div></main>
    <aside class="right">
      <div class="search"><svg viewBox="0 0 24 24"><circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/></svg><input id="q" placeholder="Search" value="${esc(state.q)}"></div>
      <div class="box" id="side"></div>
    </aside>
  </div>`;
  $("#me").onclick = () => confirm("Log out?") && logout();
  $("#q").oninput = e => { state.q = e.target.value; draw(); };
  load();
}

async function load() {
  try {
    const [products, customers, orders, users] = await Promise.all([
      api("GET", "/products"), api("GET", "/customers"), api("GET", "/orders"),
      isAdmin() ? api("GET", "/users") : Promise.resolve([]),
    ]);
    state.data = { products, customers, orders, users };
    // drop cart entries for deleted products
    for (const id of Object.keys(state.cart)) if (!products.some(p => p.id === +id)) delete state.cart[id];
    saveCart();
    draw();
  } catch (e) { toast(e.message, true); $("#view").innerHTML = `<div class="empty"><h3>Something went wrong</h3>${esc(e.message)}</div>`; }
}

const match = (...f) => !state.q || f.join(" ").toLowerCase().includes(state.q.toLowerCase());
const productOf = id => state.data.products.find(p => p.id === id);
const productName = id => productOf(id)?.name ?? "Product #" + id;
const customerName = id => state.data.customers.find(c => c.id === id)?.name ?? "Customer #" + id;
const empty = (t, s) => `<div class="empty"><h3>${t}</h3>${s}</div>`;
const cartLines = () => Object.entries(state.cart).map(([id, qty]) => ({ p: productOf(+id), qty })).filter(l => l.p);
const cartTotal = () => cartLines().reduce((a, l) => a + l.p.price * l.qty, 0);

function draw() {
  const d = state.data; if (!d.orders) return;
  const view = { shop: viewShop, cart: viewCart, orders: viewOrders, customers: viewCustomers, users: viewUsers }[route()]();
  $("#view").innerHTML = view.html;
  $("#count").textContent = view.count;
  const n = cartCount(), b = $("#badge");
  if (b) { b.textContent = n; b.style.display = n ? "grid" : "none"; }
  drawSide();
  view.bind?.();
}

function drawSide() {
  const d = state.data;
  if (isAdmin()) {
    const revenue = d.orders.filter(o => o.status !== "Cancelled").reduce((a, o) => a + o.total, 0);
    $("#side").innerHTML = `<h2>Store overview</h2>` + [["Revenue", money(revenue)], ["Orders", d.orders.length], ["Pending", d.orders.filter(o => o.status === "Pending").length], ["Products", d.products.length], ["Customers", d.customers.length]]
      .map(([k, v]) => `<div class="stat"><span class="muted">${k}</span><b>${v}</b></div>`).join("");
  } else {
    const l = cartLines();
    $("#side").innerHTML = `<h2>Your cart</h2>` + (l.length
      ? l.map(x => `<div class="stat"><span>${x.qty} × ${esc(x.p.name)}</span><span class="muted">${money(x.p.price * x.qty)}</span></div>`).join("") + `<div class="stat"><b>Total</b><b>${money(cartTotal())}</b></div><div style="padding:0 16px 16px"><a class="btn" style="display:grid;place-items:center;height:40px" href="#/cart">Checkout</a></div>`
      : `<div class="stat muted">Your cart is empty</div>`);
  }
}

async function act(fn, ok) {
  try { await fn(); if (ok) toast(ok); await load(); } catch (e) { toast(e.message, true); }
}

/* ---------- views ---------- */
function setQty(id, n) {
  if (n <= 0) delete state.cart[id]; else state.cart[id] = Math.min(n, 99);
  saveCart(); draw();
}

function viewShop() {
  const d = state.data;
  const list = d.products.filter(p => match(p.name));
  const tile = p => {
    const q = state.cart[p.id] || 0;
    return `<div class="product">
      <div class="thumb" style="background:linear-gradient(135deg,${color(p.id)},${color(p.id + 2)})"><span>${esc(initial(p.name))}</span></div>
      <div class="info"><div class="pname">${esc(p.name)}</div><div class="price">${money(p.price)}</div>
      ${q ? `<div class="stepper"><button data-dec="${p.id}">−</button><b>${q}</b><button data-inc="${p.id}">+</button></div>`
          : `<button class="btn sm" data-add="${p.id}">Add to cart</button>`}
      ${isAdmin() ? `<div class="bar"><button class="btn ghost sm" data-edit="${p.id}">Edit</button><button class="btn danger sm" data-del="${p.id}">Delete</button></div>` : ""}
      </div></div>`;
  };
  const html = `
  ${isAdmin() ? `<form class="composer" id="pf"><div class="body">
    <div class="muted">Add a product</div>
    <div class="row"><input class="field" name="name" placeholder="Product name" required><input class="field qty" style="flex:0 0 120px" name="price" type="number" step="0.01" min="0" placeholder="Price" required><button class="btn">Add</button></div></div></form>` : ""}
  ${list.length ? `<div class="grid">${list.map(tile).join("")}</div>` : empty("No products", "Nothing matches.")}`;
  return {
    html, count: list.length + " items",
    bind() {
      const on = (attr, fn) => document.querySelectorAll(`[data-${attr}]`).forEach(b => b.onclick = () => fn(+b.dataset[attr]));
      on("add", id => { setQty(id, 1); toast("Added to cart"); });
      on("inc", id => setQty(id, state.cart[id] + 1));
      on("dec", id => setQty(id, state.cart[id] - 1));
      $("#pf")?.addEventListener("submit", e => {
        e.preventDefault();
        act(() => api("POST", "/products", { name: e.target.name.value, price: +e.target.price.value }), "Product added");
      });
      on("edit", id => {
        const p = productOf(id);
        const name = prompt("Name", p.name); if (name === null) return;
        const price = prompt("Price", p.price); if (price === null) return;
        act(() => api("PUT", "/products/" + id, { name, price: +price }), "Product updated");
      });
      on("del", id => confirm("Delete product?") && act(() => api("DELETE", "/products/" + id), "Product deleted"));
    },
  };
}

function viewCart() {
  const d = state.data, lines = cartLines();
  const linked = !isAdmin() && d.customers.some(c => c.id === state.me.customerId);
  const html = lines.length ? `
  ${lines.map(l => `<div class="line">
    <div class="thumb sm" style="background:linear-gradient(135deg,${color(l.p.id)},${color(l.p.id + 2)})"><span>${esc(initial(l.p.name))}</span></div>
    <div class="grow"><b>${esc(l.p.name)}</b><div class="muted">${money(l.p.price)} each</div></div>
    <div class="stepper"><button data-dec="${l.p.id}">−</button><b>${l.qty}</b><button data-inc="${l.p.id}">+</button></div>
    <b class="lt">${money(l.p.price * l.qty)}</b></div>`).join("")}
  <form class="checkout" id="co">
    ${linked ? "" : `<select class="field" name="customer" required><option value="">${isAdmin() ? "Order for customer…" : "Select your customer profile…"}</option>${d.customers.map(c => `<option value="${c.id}" ${c.id === state.me.customerId ? "selected" : ""}>${esc(c.name)}</option>`).join("")}</select>`}
    <div class="sum"><span class="muted">Total</span><span class="price">${money(cartTotal())}</span></div>
    <button class="btn big2">Place order</button>
    <button type="button" class="btn ghost" id="clear">Clear cart</button>
  </form>` : empty("Your cart is empty", `Browse the <a href="#/shop">shop</a> to add something.`);
  return {
    html, count: cartCount() + " items",
    bind() {
      document.querySelectorAll("[data-inc]").forEach(b => b.onclick = () => setQty(+b.dataset.inc, state.cart[b.dataset.inc] + 1));
      document.querySelectorAll("[data-dec]").forEach(b => b.onclick = () => setQty(+b.dataset.dec, state.cart[b.dataset.dec] - 1));
      $("#clear")?.addEventListener("click", () => { state.cart = {}; saveCart(); draw(); });
      $("#co")?.addEventListener("submit", e => {
        e.preventDefault();
        const customerId = linked ? state.me.customerId : +e.target.customer.value;
        const items = lines.map(l => ({ productId: l.p.id, quantity: l.qty }));
        act(async () => {
          await api("POST", "/orders", { customerId, items });
          state.cart = {}; saveCart(); location.hash = "#/orders";
        }, "Order placed 🎉");
      });
    },
  };
}

function viewOrders() {
  const d = state.data;
  const steps = ["Pending", "Paid", "Shipped"];
  const list = d.orders.filter(o => match(o.id, o.status, customerName(o.customerId), o.items.map(i => productName(i.productId)).join(" "))).sort((a, b) => b.id - a.id);
  const track = o => o.status === "Cancelled" ? `<span class="pill Cancelled">Cancelled</span>` :
    `<div class="track">${steps.map((s, i) => `<div class="${i <= steps.indexOf(o.status) ? "done" : ""}"><i></i><span>${s}</span></div>`).join("")}</div>`;
  const html = list.length ? list.map(o => `
  <article class="order">
    <div class="otop"><div><b>Order #${o.id}</b>${isAdmin() ? ` <span class="muted">· ${esc(customerName(o.customerId))}</span>` : ""}<div class="muted">${new Date(o.orderDate + (/Z|[+-]\d\d:\d\d$/.test(o.orderDate) ? "" : "Z")).toLocaleString()}</div></div><div class="price">${money(o.total)}</div></div>
    ${track(o)}
    <div class="lines">${o.items.map(i => `<div><span>${i.quantity} × ${esc(productName(i.productId))}</span><span class="muted">${money(i.lineTotal)}</span></div>`).join("") || `<div class="muted">No items</div>`}</div>
    ${isAdmin() ? `<div class="bar">
      <select class="field" data-status="${o.id}">${["Pending", "Paid", "Shipped", "Cancelled"].map(s => `<option ${s === o.status ? "selected" : ""}>${s}</option>`).join("")}</select>
      <button class="btn danger sm" data-del-order="${o.id}">Delete</button></div>` : ""}
  </article>`).join("") : empty("No orders yet", `Find something in the <a href="#/shop">shop</a>.`);
  return {
    html, count: list.length + " orders",
    bind() {
      document.querySelectorAll("[data-status]").forEach(s => s.onchange = () => act(() => api("PUT", "/orders/" + s.dataset.status, { status: s.value }), "Status updated"));
      document.querySelectorAll("[data-del-order]").forEach(b => b.onclick = () => confirm("Delete this order?") && act(() => api("DELETE", "/orders/" + b.dataset.delOrder), "Order deleted"));
    },
  };
}

function viewCustomers() {
  const d = state.data;
  const list = d.customers.filter(c => match(c.name, c.email));
  const html = `
  ${isAdmin() ? `<form class="composer" id="cf">${avatar(state.me.email, state.me.id)}<div class="body">
    <input class="field" name="name" placeholder="Name" required>
    <div class="actions"><input class="field" name="email" type="email" placeholder="Email" required style="margin-right:8px"><button class="btn">Add</button></div></div></form>` : ""}
  ${list.length ? list.map(c => `
  <article class="post">${avatar(c.name, c.id)}<div class="body">
    <div class="top"><b>${esc(c.name)}</b><span class="muted">${esc(c.email)}</span></div>
    <div class="muted">${d.orders.filter(o => o.customerId === c.id).length} orders</div>
    ${isAdmin() ? `<div class="bar"><button class="btn ghost sm" data-edit="${c.id}">Edit</button><button class="btn danger sm" data-del="${c.id}">Delete</button></div>` : ""}
  </div></article>`).join("") : empty("No customers", "Nothing to show.")}`;
  return {
    html, count: d.customers.length,
    bind() {
      $("#cf")?.addEventListener("submit", e => {
        e.preventDefault();
        act(() => api("POST", "/customers", { name: e.target.name.value, email: e.target.email.value }), "Customer added");
      });
      document.querySelectorAll("[data-edit]").forEach(b => b.onclick = () => {
        const c = d.customers.find(x => x.id === +b.dataset.edit);
        const name = prompt("Name", c.name); if (name === null) return;
        const email = prompt("Email", c.email); if (email === null) return;
        act(() => api("PUT", "/customers/" + c.id, { name, email }), "Customer updated");
      });
      document.querySelectorAll("[data-del]").forEach(b => b.onclick = () => confirm("Delete customer?") && act(() => api("DELETE", "/customers/" + b.dataset.del), "Customer deleted"));
    },
  };
}

function viewUsers() {
  const d = state.data;
  const list = d.users.filter(u => match(u.email));
  const roleName = r => (r === 1 || r === "Admin") ? "Admin" : "User";
  const html = list.length ? list.map(u => `
  <article class="post">${avatar(u.email, u.id)}<div class="body">
    <div class="top"><b>${esc(u.email)}</b><span class="pill ${roleName(u.role)}">${roleName(u.role)}</span></div>
    <div class="muted">${u.customerId ? "Linked to " + esc(customerName(u.customerId)) : "No customer linked"}</div>
    ${u.id === state.me.id ? "" : `<div class="bar"><button class="btn ghost sm" data-role="${u.id}" data-to="${roleName(u.role) === "Admin" ? 0 : 1}">Make ${roleName(u.role) === "Admin" ? "User" : "Admin"}</button></div>`}
  </div></article>`).join("") : empty("No users", "Nothing to show.");
  return {
    html, count: d.users.length,
    bind() {
      document.querySelectorAll("[data-role]").forEach(b => b.onclick = () => act(() => api("PUT", `/users/${b.dataset.role}/role`, { role: +b.dataset.to }), "Role updated"));
    },
  };
}

addEventListener("hashchange", render);
render();
