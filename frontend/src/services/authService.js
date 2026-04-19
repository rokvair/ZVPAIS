import axios from 'axios';

const BASE_URL = window.location.hostname === 'localhost'
  ? 'http://localhost:5219/api/auth'
  : 'https://zvpis-backend-gmdudghme8gvb9er.uaenorth-01.azurewebsites.net/api/auth';

export async function login(email, password) {
  const response = await axios.post(`${BASE_URL}/login`, { email, password });
  const data = response.data;
  localStorage.setItem('token', data.token);
  localStorage.setItem('user', JSON.stringify({ email: data.email, role: data.role, userId: data.userId }));
  return data;
}

export async function register(payload) {
  const response = await axios.post(`${BASE_URL}/register`, payload);
  const data = response.data;
  localStorage.setItem('token', data.token);
  localStorage.setItem('user', JSON.stringify({ email: data.email, role: data.role, userId: data.userId }));
  return data;
}

export function logout() {
  localStorage.removeItem('token');
  localStorage.removeItem('user');
}

export function getStoredUser() {
  const raw = localStorage.getItem('user');
  return raw ? JSON.parse(raw) : null;
}

export function getToken() {
  return localStorage.getItem('token');
}
