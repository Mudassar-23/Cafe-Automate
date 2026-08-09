/* ============================================================
   Cafe Automate — API fetch wrapper + SignalR helper
   ============================================================ */
// Local dev (python http.server on :5500) → hit the API directly.
// Docker / production (nginx on :80) → use relative path; nginx proxies to backend.
const _devMode = window.location.port === '5500' || window.location.port === '3000';
const API_BASE = _devMode ? 'http://localhost:5112/api'      : '/api';
const HUB_URL  = _devMode ? 'http://localhost:5112/hubs/orders' : '/hubs/orders';

function getToken() { return localStorage.getItem('ca_token'); }
function getUser()  { try { return JSON.parse(localStorage.getItem('ca_user')); } catch { return null; } }

function logout() {
  localStorage.removeItem('ca_token');
  localStorage.removeItem('ca_user');
  sessionStorage.removeItem('ca_cart');
  window.location.href = 'login.html';
}

async function apiFetch(path, options = {}) {
  const token = getToken();
  const headers = { 'Content-Type': 'application/json', ...(options.headers || {}) };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers });

  if (res.status === 401) { logout(); return; }

  if (res.status === 204) return null;

  const text = await res.text();
  if (!text) return null;

  const data = JSON.parse(text);
  if (!res.ok) throw new Error(data.error || `HTTP ${res.status}`);

  return data;
}

// ── SignalR connection helper ──────────────────────────────────
let _hubConnection = null;

async function getHubConnection() {
  if (_hubConnection && _hubConnection.state === 'Connected') return _hubConnection;

  if (typeof signalR === 'undefined') {
    console.warn('SignalR script not loaded.');
    return null;
  }

  const token = getToken();
  _hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(HUB_URL, token ? { accessTokenFactory: () => token } : {})
    .withAutomaticReconnect()
    .build();

  try {
    await _hubConnection.start();
  } catch (err) {
    console.warn('SignalR connect failed:', err);
    return null;
  }

  return _hubConnection;
}

// ── Toast util (shared by all pages) ─────────────────────────
function showToast(msg, type = 'neutral') {
  let t = document.getElementById('globalToast');
  if (!t) {
    t = document.createElement('div');
    t.id = 'globalToast';
    t.style.cssText = 'position:fixed;bottom:26px;left:50%;transform:translateX(-50%) translateY(20px);padding:13px 26px;border-radius:999px;font-size:.88rem;font-weight:600;opacity:0;pointer-events:none;transition:all .3s ease;z-index:9999;box-shadow:0 14px 34px -14px rgba(34,22,12,.38);font-family:Outfit,sans-serif';
    document.body.appendChild(t);
  }
  const colors = { success: '#6f8f5c', error: '#c0392b', neutral: '#221a15' };
  t.style.background = colors[type] || colors.neutral;
  t.style.color = '#fff';
  t.textContent = msg;
  t.style.opacity = '1';
  t.style.transform = 'translateX(-50%) translateY(0)';
  clearTimeout(t._tid);
  t._tid = setTimeout(() => {
    t.style.opacity = '0';
    t.style.transform = 'translateX(-50%) translateY(20px)';
  }, 2800);
}
