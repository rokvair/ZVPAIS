import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function ProtectedRoute({ children, requireSpecialist = false }) {
  const { isAuthenticated, isSpecialist } = useAuth();

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (requireSpecialist && !isSpecialist) return <Navigate to="/" replace />;

  return children;
}
