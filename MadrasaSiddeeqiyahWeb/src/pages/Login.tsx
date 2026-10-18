import React, { useEffect, useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authService } from '../services/authService';
import { Lock, User, LogIn, KeyRound, ArrowLeft, ShieldCheck } from 'lucide-react';
import { motion } from 'framer-motion';

const Login: React.FC = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    if (authService.isAuthenticated()) {
      navigate('/admin');
    }
  }, [navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      await authService.login({ username, password });
      navigate('/admin');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Invalid username or password');
    } finally {
      setLoading(false);
    }
  };

  const handleQuickFill = () => {
    setUsername('admin');
    setPassword('Madrasa@2026');
    setError('');
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-radial from-white via-gray-50 to-primary-50/30 px-4 py-12">
      <motion.div 
        initial={{ opacity: 0, y: 25, scale: 0.98 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        transition={{ duration: 0.5, ease: [0.16, 1, 0.3, 1] }}
        className="max-w-md w-full"
      >
        <div className="mb-6">
          <Link 
            to="/" 
            className="inline-flex items-center gap-2 text-sm font-medium text-gray-600 hover:text-primary-600 transition"
          >
            <ArrowLeft className="w-4 h-4" /> Back to Admission Form
          </Link>
        </div>

        <div className="text-center mb-8">
          <motion.div 
            initial={{ scale: 0.5, rotate: -10 }}
            animate={{ scale: 1, rotate: 0 }}
            transition={{ type: 'spring', stiffness: 300, damping: 20 }}
            className="inline-flex p-3.5 bg-primary-600 text-white rounded-3xl shadow-xl shadow-primary-200 mb-4"
          >
            <ShieldCheck className="w-8 h-8" />
          </motion.div>
          <h2 className="text-3xl font-extrabold text-gray-900 tracking-tight">Admin Portal</h2>
          <p className="mt-2 text-sm text-gray-600">Secure management for Madrasa Siddeeqiyah</p>
        </div>

        <div className="bg-white p-8 rounded-3xl shadow-2xl border border-gray-100 backdrop-blur-xs">
          {/* Admin Credentials Helper Card */}
          <div className="mb-6 p-4 rounded-2xl bg-amber-50/80 border border-amber-200 text-amber-900 text-xs">
            <div className="flex items-center justify-between mb-2">
              <span className="font-bold flex items-center gap-1.5 text-amber-800">
                <KeyRound className="w-3.5 h-3.5 text-amber-600" /> Default Admin Credentials
              </span>
              <button
                type="button"
                onClick={handleQuickFill}
                className="px-2.5 py-1 bg-amber-200 hover:bg-amber-300 text-amber-900 font-bold rounded-lg transition active:scale-95"
              >
                1-Click Autofill
              </button>
            </div>
            <div className="font-mono text-gray-700 space-y-0.5">
              <div>Username: <span className="font-bold text-gray-900">admin</span></div>
              <div>Password: <span className="font-bold text-gray-900">Madrasa@2026</span></div>
            </div>
          </div>

          <form onSubmit={handleSubmit} className="space-y-5">
            {error && (
              <motion.div 
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                className="bg-red-50 border-l-4 border-red-500 p-3.5 rounded-xl"
              >
                <p className="text-sm font-medium text-red-700">{error}</p>
              </motion.div>
            )}

            <div className="space-y-1.5">
              <label className="text-xs font-bold uppercase tracking-wider text-gray-600 flex items-center gap-2">
                <User className="w-4 h-4 text-primary-500" /> Username
              </label>
              <input
                required
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                className="w-full px-4 py-3 rounded-xl border border-gray-200 focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none transition text-sm bg-gray-50/50 focus:bg-white"
                placeholder="admin"
              />
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-bold uppercase tracking-wider text-gray-600 flex items-center gap-2">
                <Lock className="w-4 h-4 text-primary-500" /> Password
              </label>
              <input
                required
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                className="w-full px-4 py-3 rounded-xl border border-gray-200 focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none transition text-sm bg-gray-50/50 focus:bg-white"
                placeholder="••••••••"
              />
            </div>

            <motion.button
              whileHover={{ scale: 1.01 }}
              whileTap={{ scale: 0.98 }}
              disabled={loading}
              type="submit"
              className={`w-full flex items-center justify-center gap-2 py-3.5 px-6 rounded-2xl shadow-lg text-base font-bold text-white transition-all ${
                loading 
                ? 'bg-gray-400 cursor-not-allowed' 
                : 'bg-primary-600 hover:bg-primary-700 hover:shadow-primary-300'
              }`}
            >
              <LogIn className="w-5 h-5" />
              {loading ? 'Verifying access...' : 'Sign In as Administrator'}
            </motion.button>
          </form>
        </div>
      </motion.div>
    </div>
  );
};

export default Login;
