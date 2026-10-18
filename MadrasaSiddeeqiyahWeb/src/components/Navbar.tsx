import React from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { GraduationCap, Wallet, MapPin, LogIn, Sparkles } from 'lucide-react';
import { motion } from 'framer-motion';

export const Navbar: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();

  const navLinks = [
    { name: 'Admissions & Aadhaar', path: '/', icon: GraduationCap },
    { name: 'Accounts & Expenses', path: '/transparency', icon: Wallet },
    { name: 'Campuses & Map', path: '/campuses', icon: MapPin },
  ];

  return (
    <header className="bg-white/90 backdrop-blur-md border-b border-gray-200 sticky top-0 z-50 shadow-xs">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-18 flex justify-between items-center">
        {/* Brand */}
        <Link to="/" className="flex items-center gap-3 group">
          <motion.div 
            whileHover={{ rotate: 12, scale: 1.05 }}
            className="p-2.5 bg-gradient-to-br from-primary-600 via-primary-700 to-teal-700 text-white rounded-2xl shadow-md shadow-primary-200 flex items-center justify-center transition"
          >
            <GraduationCap className="w-6 h-6" />
          </motion.div>
          <div>
            <div className="flex items-center gap-2">
              <span className="font-black text-gray-900 tracking-tight text-lg sm:text-xl group-hover:text-primary-700 transition">
                Madrasa Siddeeqiyah
              </span>
              <span className="hidden md:inline-flex items-center gap-1 text-[11px] font-bold bg-primary-50 text-primary-700 px-2.5 py-0.5 rounded-full border border-primary-200/60">
                <Sparkles className="w-3 h-3 text-primary-500" /> Public Portal
              </span>
            </div>
            <p className="text-[11px] text-gray-500 font-medium">Islamic Education & Public Welfare Transparency</p>
          </div>
        </Link>

        {/* Center Navigation Links */}
        <nav className="hidden md:flex items-center gap-1 bg-gray-100/80 p-1.5 rounded-2xl border border-gray-200/60">
          {navLinks.map((link) => {
            const Icon = link.icon;
            const isActive = location.pathname === link.path;
            return (
              <Link
                key={link.path}
                to={link.path}
                className={`relative px-4 py-2 rounded-xl text-xs sm:text-sm font-bold flex items-center gap-2 transition-all ${
                  isActive
                    ? 'text-primary-700 font-extrabold'
                    : 'text-gray-600 hover:text-gray-900 hover:bg-white/60'
                }`}
              >
                {isActive && (
                  <motion.div
                    layoutId="active-pill"
                    className="absolute inset-0 bg-white rounded-xl shadow-xs border border-gray-200/50 -z-10"
                    transition={{ type: 'spring', stiffness: 400, damping: 30 }}
                  />
                )}
                <Icon className={`w-4 h-4 ${isActive ? 'text-primary-600' : 'text-gray-400'}`} />
                <span>{link.name}</span>
              </Link>
            );
          })}
        </nav>

        {/* Admin Login Button */}
        <div className="flex items-center gap-2">
          <motion.button
            whileHover={{ scale: 1.03 }}
            whileTap={{ scale: 0.97 }}
            type="button"
            onClick={() => navigate('/login')}
            className="inline-flex items-center gap-2 rounded-2xl border border-primary-200 bg-primary-50/70 hover:bg-primary-100/80 px-4 py-2 text-xs sm:text-sm font-bold text-primary-700 shadow-xs transition"
          >
            <LogIn className="w-4 h-4" /> Admin Portal
          </motion.button>
        </div>
      </div>

      {/* Mobile Nav strip */}
      <div className="md:hidden flex items-center justify-around bg-gray-50/90 border-t border-gray-200 px-2 py-2">
        {navLinks.map((link) => {
          const Icon = link.icon;
          const isActive = location.pathname === link.path;
          return (
            <Link
              key={link.path}
              to={link.path}
              className={`flex flex-col items-center gap-1 py-1 px-3 rounded-xl text-[11px] font-bold ${
                isActive ? 'text-primary-600 bg-white shadow-xs' : 'text-gray-500'
              }`}
            >
              <Icon className="w-4 h-4" />
              <span>{link.name.split(' ')[0]}</span>
            </Link>
          );
        })}
      </div>
    </header>
  );
};
