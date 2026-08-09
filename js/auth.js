/* ============================================================
   Cafe Automate — Auth JS (login.html)
   ============================================================ */
const API = 'http://localhost:5112/api';

// ── Tab switching ─────────────────────────────────────────────
const tabLogin  = document.getElementById('tabLogin');
const tabSignup = document.getElementById('tabSignup');
const slider    = document.getElementById('tabSlider');
const formLogin  = document.getElementById('loginForm');
const formSignup = document.getElementById('signupForm');

function switchTab(to) {
  if (to === 'login') {
    tabLogin.classList.add('active');
    tabSignup.classList.remove('active');
    slider.classList.remove('signup');
    formLogin.classList.remove('hidden');
    formSignup.classList.add('hidden');
  } else {
    tabSignup.classList.add('active');
    tabLogin.classList.remove('active');
    slider.classList.add('signup');
    formSignup.classList.remove('hidden');
    formLogin.classList.add('hidden');
  }
  clearErrors();
}

tabLogin.addEventListener('click',  () => switchTab('login'));
tabSignup.addEventListener('click', () => switchTab('signup'));
document.getElementById('switchToSignup')?.addEventListener('click', () => switchTab('signup'));
document.getElementById('switchToLogin')?.addEventListener('click',  () => switchTab('login'));

// ── Password visibility toggle ────────────────────────────────
document.querySelectorAll('.pw-toggle').forEach(btn => {
  btn.addEventListener('click', () => {
    const input = btn.previousElementSibling;
    const isText = input.type === 'text';
    input.type = isText ? 'password' : 'text';
    btn.textContent = isText ? '👁' : '🙈';
  });
});

// ── Error helpers ─────────────────────────────────────────────
function showError(formId, msg) {
  const el = document.getElementById(formId + 'Error');
  if (!el) return;
  el.textContent = msg;
  el.classList.add('show');
  const btn = document.getElementById(formId + 'Btn');
  if (btn) { btn.classList.add('shake'); setTimeout(() => btn.classList.remove('shake'), 500); }
}

function clearErrors() {
  document.querySelectorAll('.auth-error').forEach(el => el.classList.remove('show'));
}

// ── Role-based redirect ───────────────────────────────────────
function redirectByRole(role) {
  const map = { 1: 'dashboard-website-admin.html', 2: 'dashboard-cafe-admin.html', 3: 'index.html' };
  window.location.href = map[role] || 'index.html';
}

// ── Login ─────────────────────────────────────────────────────
document.getElementById('loginForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  clearErrors();
  const btn = document.getElementById('loginBtn');
  btn.classList.add('loading');

  const email    = document.getElementById('loginEmail').value.trim();
  const password = document.getElementById('loginPassword').value;

  try {
    const res  = await fetch(`${API}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password })
    });
    const data = await res.json();

    if (!res.ok) { showError('login', data.error || 'Login failed.'); return; }

    localStorage.setItem('ca_token', data.token);
    localStorage.setItem('ca_user', JSON.stringify({ id: data.userId, name: data.fullName, email: data.email, role: data.role }));
    showToast('Welcome back, ' + data.fullName + '!');
    setTimeout(() => redirectByRole(data.role), 800);
  } catch {
    showError('login', 'Cannot reach the server. Please try again.');
  } finally {
    btn.classList.remove('loading');
  }
});

// ── Signup ────────────────────────────────────────────────────
document.getElementById('signupForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  clearErrors();

  const fullName  = document.getElementById('signupName').value.trim();
  const email     = document.getElementById('signupEmail').value.trim();
  const password  = document.getElementById('signupPassword').value;
  const confirm   = document.getElementById('signupConfirm').value;

  if (!email.toLowerCase().endsWith('@stewart.com')) { showError('signup', 'Only @stewart.com email addresses can register.'); return; }
  if (password !== confirm) { showError('signup', 'Passwords do not match.'); return; }
  if (password.length < 6)  { showError('signup', 'Password must be at least 6 characters.'); return; }

  const btn = document.getElementById('signupBtn');
  btn.classList.add('loading');

  try {
    const res  = await fetch(`${API}/auth/signup`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ fullName, email, password })
    });
    const data = await res.json();

    if (!res.ok) { showError('signup', data.error || 'Signup failed.'); return; }

    localStorage.setItem('ca_token', data.token);
    localStorage.setItem('ca_user', JSON.stringify({ id: data.userId, name: data.fullName, email: data.email, role: data.role }));
    showToast('Account created! Redirecting…');
    setTimeout(() => redirectByRole(data.role), 900);
  } catch {
    showError('signup', 'Cannot reach the server. Please try again.');
  } finally {
    btn.classList.remove('loading');
  }
});

// ── Toast ─────────────────────────────────────────────────────
function showToast(msg) {
  const t = document.getElementById('authToast');
  if (!t) return;
  t.textContent = msg;
  t.classList.add('show');
  setTimeout(() => t.classList.remove('show'), 2800);
}

// ── Auto-redirect if already logged in ────────────────────────
(function checkExisting() {
  const token = localStorage.getItem('ca_token');
  const user  = localStorage.getItem('ca_user');
  if (token && user) {
    try { redirectByRole(JSON.parse(user).role); } catch { /* ignore */ }
  }
})();
