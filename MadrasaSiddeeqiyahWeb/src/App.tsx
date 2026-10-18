import type { ReactNode } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import AdmissionForm from './pages/AdmissionForm';
import AdminDashboard from './pages/AdminDashboard';
import Login from './pages/Login';
import StudentEdit from './pages/StudentEdit';
import UserManagement from './pages/UserManagement';
import PublicTransparency from './pages/PublicTransparency';
import CampusesPage from './pages/CampusesPage';
import { authService } from './services/authService';

const AdminRoute = ({ children }: { children: ReactNode }) => {
  return authService.isAdmin() ? children : <Navigate to="/login" replace />;
};

function App() {
  return (
    <Router>
      <div className="min-h-screen bg-gray-50">
        <Routes>
          {/* Public pages */}
          <Route path="/" element={<AdmissionForm />} />
          <Route path="/login" element={<Login />} />
          <Route path="/transparency" element={<PublicTransparency />} />
          <Route path="/campuses" element={<CampusesPage />} />

          {/* Admin-protected pages */}
          <Route path="/admin" element={<AdminRoute><AdminDashboard /></AdminRoute>} />
          <Route path="/admin/users" element={<AdminRoute><UserManagement /></AdminRoute>} />
          <Route path="/admin/students/:id/edit" element={<AdminRoute><StudentEdit /></AdminRoute>} />

          {/* Fallback */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
