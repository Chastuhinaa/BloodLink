"use strict";

const TOKEN_KEY = "bloodlink.token";
const $ = (sel) => document.querySelector(sel);

const TYPE_NAMES = { WholeBlood: "Цільна кров", Plasma: "Плазма", Platelets: "Тромбоцити" };

async function api(path, { method = "GET", body } = {}) {
  const headers = { "Content-Type": "application/json" };
  const token = localStorage.getItem(TOKEN_KEY);
  if (token) headers.Authorization = `Bearer ${token}`;

  const resp = await fetch(path, { method, headers, body: body ? JSON.stringify(body) : undefined });
  if (resp.status === 401 && token) {
    logout();
    throw new Error("Сесія завершилась, увійдіть знову.");
  }
  if (resp.status === 204) return null;

  const data = await resp.json().catch(() => null);
  if (!resp.ok) throw new Error(problemText(data) || `Помилка ${resp.status}`);
  return data;
}

function problemText(p) {
  if (!p) return "";
  if (Array.isArray(p.errors)) return p.errors.join(" ");
  if (p.errors && typeof p.errors === "object") return Object.values(p.errors).flat().join(" ");
  return p.title || "";
}

const formatDate = (iso) => new Date(iso + "T00:00:00").toLocaleDateString("uk-UA");
const todayIso = () => new Date().toISOString().slice(0, 10);
const escapeHtml = (s) => String(s ?? "").replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));

function show(screen) {
  const loggedIn = !!localStorage.getItem(TOKEN_KEY);
  if (!loggedIn) screen = "auth";
  else if (screen === "auth" || !screen) screen = "profile";

  document.querySelectorAll(".screen").forEach((s) => s.classList.add("hidden"));
  $(`#screen-${screen}`).classList.remove("hidden");
  $("#tabbar").classList.toggle("hidden", !loggedIn);
  $("#logout").classList.toggle("hidden", !loggedIn);
  document.querySelectorAll(".tabbar a").forEach((a) => a.classList.toggle("active", a.dataset.screen === screen));

  if (screen === "profile") loadProfile();
  if (screen === "centers") loadCenters();
  if (screen === "history") loadHistory();
  if (screen === "requests") loadRequests();
}

window.addEventListener("hashchange", () => show(location.hash.slice(1)));

function logout() {
  localStorage.removeItem(TOKEN_KEY);
  location.hash = "";
  show("auth");
}
$("#logout").addEventListener("click", logout);

let authMode = "login";
document.querySelectorAll(".tab").forEach((tab) =>
  tab.addEventListener("click", () => {
    authMode = tab.dataset.mode;
    document.querySelectorAll(".tab").forEach((t) => t.classList.toggle("active", t === tab));
    document.querySelectorAll(".register-only").forEach((el) => el.classList.toggle("hidden", authMode !== "register"));
    $("#auth-submit").textContent = authMode === "login" ? "Увійти" : "Зареєструватися";
    $("#auth-error").textContent = "";
  }));

$("#auth-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const f = new FormData(e.target);
  $("#auth-error").textContent = "";
  try {
    const body = { email: f.get("email"), password: f.get("password") };
    if (authMode === "register") body.fullName = f.get("fullName");
    const res = await api(`/api/auth/${authMode}`, { method: "POST", body });
    localStorage.setItem(TOKEN_KEY, res.token);
    e.target.reset();
    location.hash = "#profile";
    show("profile");
  } catch (err) {
    $("#auth-error").textContent = err.message;
  }
});

let profile = null;

async function loadProfile() {
  try {
    profile = await api("/api/profile");
    const form = $("#profile-form");
    for (const key of ["fullName", "bloodGroup", "rhFactor", "sex", "birthDate", "city", "latitude", "longitude"]) {
      form.elements[key].value = profile[key] ?? "";
    }
    await loadEligibility();
  } catch (err) {
    $("#profile-error").textContent = err.message;
  }
}

async function loadEligibility() {
  const box = $("#eligibility");
  const e = await api("/api/eligibility?type=WholeBlood");
  box.classList.toggle("wait", !e.canDonateNow);
  box.innerHTML = e.canDonateNow
    ? `<p class="big">Можна здавати вже сьогодні</p><p class="muted">${escapeHtml(e.reason)}</p>`
    : `<p class="big">Через ${e.daysLeft} дн. — з ${formatDate(e.nextDate)}</p><p class="muted">${escapeHtml(e.reason)}</p>`;
}

$("#profile-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const f = new FormData(e.target);
  const val = (k) => (f.get(k) === "" ? null : f.get(k));
  const num = (k) => (f.get(k) === "" ? null : Number(f.get(k)));
  $("#profile-error").textContent = "";
  $("#profile-ok").textContent = "";
  try {
    profile = await api("/api/profile", {
      method: "PUT",
      body: {
        fullName: f.get("fullName"),
        bloodGroup: val("bloodGroup"),
        rhFactor: val("rhFactor"),
        sex: val("sex"),
        birthDate: val("birthDate"),
        city: val("city"),
        latitude: num("latitude"),
        longitude: num("longitude"),
      },
    });
    $("#profile-ok").textContent = `Збережено. Група крові: ${profile.bloodType}.`;
    await loadEligibility();
  } catch (err) {
    $("#profile-error").textContent = err.message;
  }
});

$("#use-location").addEventListener("click", () => {
  if (!navigator.geolocation) return;
  navigator.geolocation.getCurrentPosition((pos) => {
    const form = $("#profile-form");
    form.elements.latitude.value = pos.coords.latitude.toFixed(5);
    form.elements.longitude.value = pos.coords.longitude.toFixed(5);
  }, () => { $("#profile-error").textContent = "Не вдалося визначити місцезнаходження."; });
});

async function loadCenters() {
  const list = $("#centers-list");
  try {
    if (!profile) profile = await api("/api/profile");
    const hasCoords = profile.latitude != null && profile.longitude != null;
    $("#centers-hint").textContent = hasCoords
      ? "Від найближчого до найдальшого від точки в профілі."
      : "Додайте координати в профілі, щоб побачити відстань.";
    const query = hasCoords ? `?lat=${profile.latitude}&lng=${profile.longitude}` : "";
    const centers = await api(`/api/centers${query}`);
    list.innerHTML = centers.map((c) => `
      <li>
        <div>
          <div class="title">${escapeHtml(c.name)}</div>
          <div class="sub">${escapeHtml(c.address)}, ${escapeHtml(c.city)}</div>
          <div class="sub">${escapeHtml(c.workingHours || "")} ${c.phone ? "· " + escapeHtml(c.phone) : ""}</div>
        </div>
        ${c.distanceKm != null ? `<span class="dist">${c.distanceKm.toFixed(1)} км</span>` : ""}
      </li>`).join("");
  } catch (err) {
    list.innerHTML = `<li class="error">${escapeHtml(err.message)}</li>`;
  }
}

async function loadHistory() {
  const form = $("#donation-form");
  form.elements.date.max = todayIso();
  if (!form.elements.date.value) form.elements.date.value = todayIso();

  const select = $("#donation-center");
  if (select.options.length <= 1) {
    const centers = await api("/api/centers");
    for (const c of centers) select.add(new Option(c.name, c.id));
  }

  const list = $("#history-list");
  const items = await api("/api/donations");
  list.innerHTML = items.length === 0
    ? `<li class="muted">Поки що немає жодної донації.</li>`
    : items.map((d) => `
      <li>
        <div>
          <div class="title">${formatDate(d.date)} · ${TYPE_NAMES[d.type] || d.type}</div>
          <div class="sub">${escapeHtml(d.bloodCenterName || "Центр не вказано")}${d.notes ? " · " + escapeHtml(d.notes) : ""}</div>
        </div>
        <button type="button" class="link del" data-id="${d.id}" aria-label="Видалити запис">✕</button>
      </li>`).join("");
}

$("#history-list").addEventListener("click", async (e) => {
  const id = e.target.dataset?.id;
  if (!id || !confirm("Видалити цей запис?")) return;
  await api(`/api/donations/${id}`, { method: "DELETE" });
  loadHistory();
});

$("#donation-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const f = new FormData(e.target);
  $("#donation-error").textContent = "";
  try {
    await api("/api/donations", {
      method: "POST",
      body: {
        date: f.get("date"),
        type: f.get("type"),
        bloodCenterId: f.get("bloodCenterId") ? Number(f.get("bloodCenterId")) : null,
        notes: f.get("notes") || null,
      },
    });
    e.target.elements.notes.value = "";
    loadHistory();
  } catch (err) {
    $("#donation-error").textContent = err.message;
  }
});

const URGENCY_NAMES = { Normal: "Звичайна", Urgent: "Терміново", Critical: "Критично" };
const STATUS_NAMES = { Open: "Відкритий", Fulfilled: "Кров знайдено", Cancelled: "Скасовано" };

async function fillCenterSelect(select) {
  if (select.options.length > 0) return;
  const centers = await api("/api/centers");
  for (const c of centers) select.add(new Option(c.name, c.id));
}

function requestMeta(r) {
  const parts = [
    `${TYPE_NAMES[r.type] || r.type} · потрібно донорів: ${r.unitsNeeded}`,
    `${escapeHtml(r.bloodCenterName)}${r.distanceKm != null ? ` · ${r.distanceKm.toFixed(1)} км` : ""}`,
  ];
  if (r.hospital || r.patientName) {
    parts.push(`Для: ${[r.hospital, r.patientName].filter(Boolean).map(escapeHtml).join(", ")}`);
  }
  if (r.neededBy) parts.push(`Потрібно до ${formatDate(r.neededBy)}`);
  if (r.description) parts.push(escapeHtml(r.description));
  return parts.map((p) => `<div class="sub">${p}</div>`).join("");
}

function requestBadges(r) {
  const urgency = r.urgency !== "Normal" ? `<span class="badge ${r.urgency}">${URGENCY_NAMES[r.urgency]}</span>` : "";
  const status = r.status !== "Open" ? `<span class="badge closed">${STATUS_NAMES[r.status]}</span>` : "";
  return `<div class="top"><span class="badge blood">${escapeHtml(r.bloodType)}</span>${urgency}${status}
    <span class="sub">відгукнулося: ${r.responsesCount}</span></div>`;
}

function otherRequestHtml(r) {
  let action;
  if (r.iResponded) {
    action = `<div class="actions"><span class="help">Ви відгукнулися. Зателефонуйте:</span>
      <a class="tel" href="tel:${escapeHtml(r.contactPhone)}">${escapeHtml(r.contactPhone)}</a>
      <button type="button" class="link del" data-cancel="${r.id}">Скасувати відгук</button></div>`;
  } else if (r.canHelp) {
    action = `<div class="actions"><span class="help">Ваша група підходить</span>
      <button type="button" class="small-btn" data-respond="${r.id}">Я допоможу</button></div>`;
  } else {
    action = `<div class="nohelp">${escapeHtml(r.cannotHelpReason || "")}</div>`;
  }
  return `<li class="req">${requestBadges(r)}${requestMeta(r)}${action}</li>`;
}

function myRequestHtml(r) {
  const responders = r.responders.length
    ? `<div class="sub">Готові допомогти:</div><div class="responders">${r.responders.map((d) =>
        `<div>${escapeHtml(d.fullName)} · ${escapeHtml(d.bloodType)} · ${escapeHtml(d.email)}</div>`).join("")}</div>`
    : `<div class="nohelp">Поки ніхто не відгукнувся.</div>`;
  const actions = r.status === "Open"
    ? `<div class="actions"><button type="button" class="link" data-status="Fulfilled" data-id="${r.id}">Кров знайдено</button>
        <button type="button" class="link del" data-status="Cancelled" data-id="${r.id}">Скасувати запит</button></div>`
    : "";
  return `<li class="req">${requestBadges(r)}${requestMeta(r)}${responders}${actions}</li>`;
}

async function loadRequests() {
  const list = $("#requests-list");
  try {
    await fillCenterSelect($("#request-center"));
    const only = $("#only-compatible").checked;
    const [items, mine] = await Promise.all([
      api(`/api/requests${only ? "?onlyCompatible=true" : ""}`),
      api("/api/requests/mine"),
    ]);

    const others = items.filter((r) => !r.isMine);
    list.innerHTML = others.length === 0
      ? `<li class="muted">Зараз немає відкритих запитів${only ? ", де ви можете допомогти" : ""}.</li>`
      : others.map(otherRequestHtml).join("");

    $("#my-requests-block").classList.toggle("hidden", mine.length === 0);
    $("#my-requests").innerHTML = mine.map(myRequestHtml).join("");
  } catch (err) {
    list.innerHTML = `<li class="error">${escapeHtml(err.message)}</li>`;
  }
}

$("#only-compatible").addEventListener("change", loadRequests);
$("#toggle-request-form").addEventListener("click", () => $("#request-form").classList.toggle("hidden"));

$("#request-form").addEventListener("submit", async (e) => {
  e.preventDefault();
  const f = new FormData(e.target);
  const val = (k) => (f.get(k) === "" ? null : f.get(k));
  $("#request-error").textContent = "";
  try {
    await api("/api/requests", {
      method: "POST",
      body: {
        bloodGroup: f.get("bloodGroup"),
        rhFactor: f.get("rhFactor"),
        type: f.get("type"),
        unitsNeeded: Number(f.get("unitsNeeded") || 1),
        urgency: f.get("urgency"),
        bloodCenterId: Number(f.get("bloodCenterId")),
        hospital: val("hospital"),
        patientName: val("patientName"),
        neededBy: val("neededBy"),
        contactPhone: f.get("contactPhone"),
        description: val("description"),
      },
    });
    e.target.reset();
    e.target.classList.add("hidden");
    loadRequests();
  } catch (err) {
    $("#request-error").textContent = err.message;
  }
});

$("#screen-requests").addEventListener("click", async (e) => {
  const t = e.target;
  try {
    if (t.dataset.respond) {
      await api(`/api/requests/${t.dataset.respond}/responses`, { method: "POST" });
    } else if (t.dataset.cancel) {
      await api(`/api/requests/${t.dataset.cancel}/responses`, { method: "DELETE" });
    } else if (t.dataset.status) {
      const question = t.dataset.status === "Fulfilled" ? "Позначити, що кров знайдено?" : "Скасувати запит?";
      if (!confirm(question)) return;
      await api(`/api/requests/${t.dataset.id}/status`, { method: "PATCH", body: { status: t.dataset.status } });
    } else {
      return;
    }
    loadRequests();
  } catch (err) {
    alert(err.message);
  }
});

show(location.hash.slice(1));
