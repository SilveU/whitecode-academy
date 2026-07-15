// ─── Centralized API Service ───
const API_BASE = '/api';

const DEFAULT_HEADERS = {
  'ngrok-skip-browser-warning': 'true',
};

// ─── Token helpers ───
export function getToken() {
  return localStorage.getItem('wca_token');
}

export function setToken(token) {
  localStorage.setItem('wca_token', token);
}

export function removeToken() {
  localStorage.removeItem('wca_token');
}

export function getUser() {
  try {
    const raw = localStorage.getItem('wca_user');
    return raw ? JSON.parse(raw) : null;
  } catch {
    removeUser();
    return null;
  }
}

export function setUser(user) {
  localStorage.setItem('wca_user', JSON.stringify(user));
}

export function removeUser() {
  localStorage.removeItem('wca_user');
}

// ─── Build headers ───
function authHeaders(extra = {}) {
  const token = getToken();
  const headers = { ...DEFAULT_HEADERS, ...extra };
  if (token) headers['Authorization'] = `Bearer ${token}`;
  return headers;
}

// ─── Core fetch wrapper ───
async function request(method, path, { body, isForm = false, params } = {}) {
  let url = `${API_BASE}${path}`;

  if (params) {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== '') qs.append(k, v);
    });
    const qsStr = qs.toString();
    if (qsStr) url += `?${qsStr}`;
  }

  const opts = { method, headers: authHeaders() };

  if (body) {
    if (isForm) {
      // FormData — browser sets Content-Type with boundary automatically
      opts.body = body;
    } else {
      opts.headers['Content-Type'] = 'application/json';
      opts.body = JSON.stringify(body);
    }
  }

  const res = await fetch(url, opts);

  // 204 No Content
  if (res.status === 204) return { ok: true, data: null, status: 204 };

  const data = await res.json().catch(() => null);

  if (!res.ok) {
    let message = '';
    if (data?.errors) {
      message = Object.values(data.errors).flat().join(' | ');
    } else if (data?.message) {
      message = data.message;
    } else {
      message = `Error ${res.status}`;
    }
    return { ok: false, data, status: res.status, message };
  }

  return { ok: true, data, status: res.status };
}

// ─── Convenience methods ───
export const api = {
  get:      (path, params)        => request('GET', path, { params }),
  post:     (path, body)          => request('POST', path, { body }),
  put:      (path, body)          => request('PUT', path, { body }),
  del:      (path)                => request('DELETE', path),
  delQuery: (path, params)        => request('DELETE', path, { params }),
  postForm: (path, formData)      => request('POST', path, { body: formData, isForm: true }),
  putForm:  (path, formData)      => request('PUT', path, { body: formData, isForm: true }),
};

// ─── Auth endpoints ───
export const authApi = {
  login: (identity, password) => api.post('/authentication/login', { identity, password }),
  register: (data) => api.post('/authentication/register', data),
  refresh: () => api.post('/authentication/refresh'),
  logout: () => api.post('/authentication/logout'),
  logoutAll: () => api.post('/authentication/logout-all'),
  resendConfirmation: (email) => api.post(`/authentication/resend-email-confirmation?Email=${encodeURIComponent(email)}`),
};

// ─── Course endpoints ───
export const courseApi = {
  list:   (params) => api.get('/course', params),
  getById: (id)    => api.get(`/course/${id}`),
  create: (data)   => api.post('/course', data),
  update: (id, data) => api.put(`/course/${id}`, data),
  delete: (id)     => api.del(`/course/${id}`),
};

// ─── Department endpoints ───
export const departmentApi = {
  list:   (params)    => api.get('/department', params),
  getById: (id)       => api.get(`/department/${id}`),
  create: (formData)  => api.postForm('/department', formData),
  update: (id, formData) => api.putForm(`/department/${id}`, formData),
  delete: (id)        => api.del(`/department/${id}`),
};

// ─── Section endpoints ───
export const sectionApi = {
  listByCourse: (courseId) => api.get(`/section/by-course/${courseId}`),
  create: (formData)       => api.postForm('/section', formData),
  update: (id, formData)   => api.putForm(`/section/${id}`, formData),
  delete: (id)             => api.del(`/section/${id}`),
};

// ─── Enrollment endpoints ───
export const enrollmentApi = {
  byCourse:  (courseId)  => api.get(`/enrollment/by-course/${courseId}`),
  byStudent: (studentId) => api.get(`/enrollment/by-student/${studentId}`),
  enroll:    (courseId)  => api.post('/enrollment', { courseId }),
  unenroll:  (studentId, courseId) => api.delQuery('/enrollment', { studentId, courseId }),
};

// ─── Instructor endpoints ───
export const instructorApi = {
  list:   (params) => api.get('/instructor', params),
  getById: (id)    => api.get(`/instructor/${id}`),
  assign: (data)   => api.post('/instructor', data),
  update: (id, data) => api.put(`/instructor/${id}`, data),
  delete: (id)     => api.del(`/instructor/${id}`),
};

// ─── Student endpoints ───
export const studentApi = {
  register: () => api.post('/student'),
  delete: (id) => api.del(`/student/${id}`),
};
