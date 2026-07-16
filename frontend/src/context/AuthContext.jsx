import { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { getToken, setToken, removeToken, getUser, setUser, removeUser, authApi } from '../services/api';

// --- Helper to parse JWT payload ---
function parseJwt(token) {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
        return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
    return JSON.parse(jsonPayload);
  } catch (e) {
    return null;
  }
}

// --- Helper to extract roles from ASP.NET Core JWT ---
function extractRolesFromToken(token) {
  const payload = parseJwt(token);
  if (!payload) return [];
  const roleClaim = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role || payload.roles || [];
  return Array.isArray(roleClaim) ? roleClaim : [roleClaim];
}

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
        if (res.ok && res.data?.accessToken) {
          const newToken = res.data.accessToken;
          const parsedRoles = extractRolesFromToken(newToken);
          
          setToken(newToken);
          setTokenState(newToken);
          
          // Optionally update user roles if they changed
          if (user) {
             const updatedUser = { ...user, roles: parsedRoles };
             setUser(updatedUser);
             setUserState(updatedUser);
          }
        }
      } catch {
        // Network error — keep user logged in, they might be offline
      } finally {
        setInitializing(false);
      }
    }
    validateToken();
  }, []); // user is omitted intentionally to avoid infinite loop on mount

  const login = useCallback(async (identity, password) => {
    setLoading(true);
    try {
      const res = await authApi.login(identity, password);
      if (res.ok && res.data?.isAuthenticated) {
        const accessToken = res.data.accessToken;
        const parsedRoles = extractRolesFromToken(accessToken);
        
        const userData = {
          id: res.data.id,
          email: res.data.email,
          userName: res.data.userName,
          roles: parsedRoles,
          message: res.data.message,
        };
        setToken(accessToken);
        setUser(userData);
        setTokenState(accessToken);
        setUserState(userData);
        return { success: true, user: userData };
      }
      return { success: false, message: res.data?.message || res.message || 'Login failed' };
    } catch {
      return { success: false, message: 'Server connection error' };
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
      if (res.ok && res.data?.accessToken) {
        setToken(res.data.accessToken);
        setTokenState(res.data.accessToken);
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

