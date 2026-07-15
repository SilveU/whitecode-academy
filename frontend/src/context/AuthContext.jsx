import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { getToken, setToken, removeToken, getUser, setUser, removeUser, authApi } from '../services/api';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUserState] = useState(() => getUser());
  const [token, setTokenState] = useState(() => getToken());
  const [loading, setLoading] = useState(false);
  const [initializing, setInitializing] = useState(true);

  const isAuthenticated = !!token;

  // Derive roles from user object
  const roles = user?.roles || [];
  const isAdmin = roles.includes('Admin');
  const isInstructor = roles.includes('Instructor');
  const isStudent = roles.includes('User'); // Backend uses "User" role for students

  // Force logout (clear everything)
  const forceLogout = useCallback(() => {
    removeToken();
    removeUser();
    setTokenState(null);
    setUserState(null);
  }, []);

  // Validate stored token on app startup
  useEffect(() => {
    async function validateToken() {
      const storedToken = getToken();
      if (!storedToken) {
        setInitializing(false);
        return;
      }

      try {
        // Try to refresh the token to verify it's still valid
        const res = await authApi.refresh();
        if (res.ok && res.data?.token) {
          setToken(res.data.token);
          setTokenState(res.data.token);
        }
        // If refresh fails with 401, the token is expired but we keep the
        // user logged in — the next API call will fail and show an error.
        // We only force logout on network errors or if there's no token.
      } catch {
        // Network error — keep user logged in, they might be offline
      } finally {
        setInitializing(false);
      }
    }
    validateToken();
  }, []);

  const login = useCallback(async (identity, password) => {
    setLoading(true);
    try {
      const res = await authApi.login(identity, password);
      if (res.ok && res.data?.isAuthenticated) {
        const userData = {
          id: res.data.id,
          email: res.data.email,
          userName: res.data.userName,
          firstName: res.data.firstName,
          lastName: res.data.lastName,
          roles: res.data.roles || [],
          message: res.data.message,
        };
        setToken(res.data.token);
        setUser(userData);
        setTokenState(res.data.token);
        setUserState(userData);
        return { success: true, user: userData };
      }
      return { success: false, message: res.data?.message || res.message || 'فشل تسجيل الدخول' };
    } catch {
      return { success: false, message: 'حدث خطأ في الاتصال بالخادم' };
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } catch {
      // ignore
    }
    forceLogout();
  }, [forceLogout]);

  const refreshAuthToken = useCallback(async () => {
    try {
      const res = await authApi.refresh();
      if (res.ok && res.data?.token) {
        setToken(res.data.token);
        setTokenState(res.data.token);
        return true;
      }
    } catch {
      // ignore
    }
    return false;
  }, []);

  // Handle 401 responses globally — auto-logout on unauthorized
  const handleUnauthorized = useCallback(() => {
    forceLogout();
  }, [forceLogout]);

  const value = {
    user,
    token,
    loading,
    initializing,
    isAuthenticated,
    isAdmin,
    isInstructor,
    isStudent,
    roles,
    login,
    logout,
    refreshAuthToken,
    handleUnauthorized,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}

