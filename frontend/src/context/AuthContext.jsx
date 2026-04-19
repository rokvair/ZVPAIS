import React, { createContext, useContext, useState } from 'react';
import { login as apiLogin, register as apiRegister, logout as apiLogout, getStoredUser } from '../services/authService';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(getStoredUser);

  async function login(email, password) {
    const data = await apiLogin(email, password);
    setUser({ email: data.email, role: data.role, userId: data.userId });
    return data;
  }

  async function register(payload) {
    const data = await apiRegister(payload);
    setUser({ email: data.email, role: data.role, userId: data.userId });
    return data;
  }

  function logout() {
    apiLogout();
    setUser(null);
  }

  const isAuthenticated = !!user;
  const isSpecialist = user?.role === 'Specialist';

  return (
    <AuthContext.Provider value={{ user, login, register, logout, isAuthenticated, isSpecialist }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
