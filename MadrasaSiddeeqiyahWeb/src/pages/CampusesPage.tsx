import React, { useEffect, useState } from 'react';
import { financeService } from '../services/financeService';
import type { CampusItem } from '../types/finance';
import { Navbar } from '../components/Navbar';
import {
  MapPin,
  Users,
  Phone,
  User,
  Building2,
  IndianRupee,
  TrendingUp,
  TrendingDown,
  Target,
  Star,
  ChevronDown,
  ChevronUp,
  ExternalLink,
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

const fmt = (n: number) =>
  new Intl.NumberFormat('en-IN', { style: 'currency', currency: 'INR', maximumFractionDigits: 0 }).format(n);

const pct = (spent: number, budget: number) =>
  budget > 0 ? Math.min(100, Math.round((spent / budget) * 100)) : 0;

const capPct = (current: number, capacity: number) =>
  capacity > 0 ? Math.min(100, Math.round((current / capacity) * 100)) : 0;

const ProgressBar: React.FC<{ value: number; color: string; label: string }> = ({ value, color, label }) => (
  <div className="mb-2">
    <div className="flex justify-between text-xs font-semibold text-gray-600 mb-1">
      <span>{label}</span>
      <span>{value}%</span>
    </div>
    <div className="h-2.5 bg-gray-100 rounded-full overflow-hidden">
      <motion.div
        className={`h-full rounded-full ${color}`}
        initial={{ width: 0 }}
        animate={{ width: `${value}%` }}
        transition={{ duration: 1.2, ease: 'easeOut' }}
      />
    </div>
  </div>
);

const StatBadge: React.FC<{ icon: React.ElementType; label: string; value: string; sub?: string; color: string }> = ({
  icon: Icon, label, value, sub, color,
}) => (
  <div className={`flex items-center gap-3 p-3 rounded-2xl ${color}`}>
    <div className="p-2 rounded-xl bg-white/60">
      <Icon className="w-4 h-4" />
    </div>
    <div>
      <p className="text-[10px] font-bold uppercase tracking-wider opacity-70">{label}</p>
      <p className="text-base font-extrabold leading-tight">{value}</p>
      {sub && <p className="text-[10px] opacity-60 font-medium">{sub}</p>}
    </div>
  </div>
);

const CampusCard: React.FC<{ campus: CampusItem; index: number }> = ({ campus, index }) => {
  const [expanded, setExpanded] = useState(false);
  const budgetUsed = pct(campus.totalSpent, campus.annualBudget);
  const occupancy = capPct(campus.currentStudents, campus.studentCapacity);
  const netBalance = campus.totalReceived - campus.totalSpent;

  const mapSrc = campus.googleMapEmbedUrl ||
    `https://maps.google.com/maps?q=${campus.latitude},${campus.longitude}&z=15&output=embed`;

  return (
    <motion.div
      initial={{ opacity: 0, y: 40 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.55, delay: index * 0.15, ease: [0.22, 1, 0.36, 1] }}
      layout
      className="bg-white rounded-3xl border border-gray-100 shadow-lg shadow-gray-100 overflow-hidden"
    >
      {/* Header gradient */}
      <div className={`px-6 pt-6 pb-4 ${index === 0
        ? 'bg-gradient-to-br from-primary-600 to-teal-600'
        : index === 1
        ? 'bg-gradient-to-br from-violet-600 to-purple-700'
        : 'bg-gradient-to-br from-amber-500 to-orange-600'
      } text-white`}>
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-2 mb-1">
              <span className="text-[11px] font-bold bg-white/20 px-2.5 py-0.5 rounded-full uppercase tracking-wider">
                {campus.branchCode}
              </span>
              <Star className="w-3.5 h-3.5 text-yellow-300 fill-yellow-300" />
            </div>
            <h2 className="text-xl font-extrabold tracking-tight">{campus.name}</h2>
            <p className="text-sm opacity-80 mt-0.5">{campus.city}, {campus.state}</p>
          </div>
          <Building2 className="w-10 h-10 opacity-20" />
        </div>

        <div className="flex flex-wrap gap-3 mt-4">
          <div className="flex items-center gap-1.5 bg-white/15 rounded-xl px-3 py-1.5 text-xs font-bold">
            <MapPin className="w-3.5 h-3.5" />
            {campus.address}
          </div>
          <div className="flex items-center gap-1.5 bg-white/15 rounded-xl px-3 py-1.5 text-xs font-bold">
            <Phone className="w-3.5 h-3.5" />
            {campus.contactPhone}
          </div>
          <div className="flex items-center gap-1.5 bg-white/15 rounded-xl px-3 py-1.5 text-xs font-bold">
            <User className="w-3.5 h-3.5" />
            {campus.inchargePerson}
          </div>
        </div>
      </div>

      {/* Stats grid */}
      <div className="p-5 grid grid-cols-2 sm:grid-cols-4 gap-3">
        <StatBadge
          icon={Users}
          label="Students"
          value={`${campus.currentStudents} / ${campus.studentCapacity}`}
          sub={`${occupancy}% capacity`}
          color="bg-blue-50 text-blue-700"
        />
        <StatBadge
          icon={IndianRupee}
          label="Annual Budget"
          value={fmt(campus.annualBudget)}
          color="bg-emerald-50 text-emerald-700"
        />
        <StatBadge
          icon={TrendingDown}
          label="Total Spent"
          value={fmt(campus.totalSpent)}
          color="bg-red-50 text-red-600"
        />
        <StatBadge
          icon={netBalance >= 0 ? TrendingUp : TrendingDown}
          label="Net Balance"
          value={fmt(Math.abs(netBalance))}
          sub={netBalance >= 0 ? 'Surplus' : 'Deficit'}
          color={netBalance >= 0 ? 'bg-green-50 text-green-700' : 'bg-rose-50 text-rose-700'}
        />
      </div>

      {/* Progress bars */}
      <div className="px-5 pb-4">
        <ProgressBar
          value={budgetUsed}
          color={budgetUsed > 90 ? 'bg-red-500' : budgetUsed > 70 ? 'bg-amber-500' : 'bg-emerald-500'}
          label="Budget Utilised"
        />
        <ProgressBar
          value={occupancy}
          color={occupancy > 90 ? 'bg-rose-500' : occupancy > 70 ? 'bg-violet-500' : 'bg-blue-500'}
          label="Campus Occupancy"
        />
      </div>

      {/* Toggle map */}
      <div className="px-5 pb-5">
        <motion.button
          whileTap={{ scale: 0.97 }}
          type="button"
          onClick={() => setExpanded(!expanded)}
          className="w-full flex items-center justify-center gap-2 py-3 rounded-2xl border-2 border-dashed border-gray-200 hover:border-primary-300 hover:bg-primary-50/40 text-sm font-bold text-gray-500 hover:text-primary-700 transition-all"
        >
          <MapPin className="w-4 h-4" />
          {expanded ? 'Hide Map' : 'View Campus on Google Maps'}
          {expanded ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
        </motion.button>

        <AnimatePresence>
          {expanded && (
            <motion.div
              initial={{ height: 0, opacity: 0 }}
              animate={{ height: 340, opacity: 1 }}
              exit={{ height: 0, opacity: 0 }}
              transition={{ duration: 0.5, ease: [0.22, 1, 0.36, 1] }}
              className="overflow-hidden mt-4 rounded-2xl border border-gray-200 shadow-inner"
            >
              <iframe
                title={`Map of ${campus.name}`}
                src={mapSrc}
                width="100%"
                height="340"
                loading="lazy"
                className="w-full h-[340px] border-0"
                allowFullScreen
              />
            </motion.div>
          )}
        </AnimatePresence>

        {campus.latitude && campus.longitude && (
          <a
            href={`https://www.google.com/maps?q=${campus.latitude},${campus.longitude}`}
            target="_blank"
            rel="noopener noreferrer"
            className="mt-2 flex items-center justify-center gap-1.5 text-[12px] font-bold text-primary-600 hover:text-primary-800 transition"
          >
            <ExternalLink className="w-3.5 h-3.5" />
            Open in Google Maps
          </a>
        )}
      </div>
    </motion.div>
  );
};

const CampusesPage: React.FC = () => {
  const [campuses, setCampuses] = useState<CampusItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    financeService.getCampuses()
      .then(setCampuses)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const totalStudents = campuses.reduce((s, c) => s + c.currentStudents, 0);
  const totalCapacity = campuses.reduce((s, c) => s + c.studentCapacity, 0);
  const totalBudget = campuses.reduce((s, c) => s + c.annualBudget, 0);
  const totalSpent = campuses.reduce((s, c) => s + c.totalSpent, 0);
  const totalReceived = campuses.reduce((s, c) => s + c.totalReceived, 0);

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50/30 to-teal-50/40">
      <Navbar />

      {/* Hero banner */}
      <div className="bg-gradient-to-r from-teal-700 via-primary-700 to-primary-600 text-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-14">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <div className="inline-flex items-center gap-2 bg-white/15 text-white text-xs font-bold px-3 py-1.5 rounded-full mb-4">
              <MapPin className="w-3.5 h-3.5 text-teal-200" />
              {campuses.length} Campuses • Live Data
            </div>
            <h1 className="text-3xl sm:text-4xl font-extrabold tracking-tight mb-2">
              Our Campuses &amp; Locations
            </h1>
            <p className="text-teal-100 max-w-xl text-sm font-medium">
              Every campus, its students, budget, and location — fully transparent and publicly available.
            </p>
          </motion.div>

          {/* Network-wide summary strip */}
          {!loading && (
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3 }}
              className="mt-8 grid grid-cols-2 sm:grid-cols-5 gap-3"
            >
              {[
                { label: 'Total Campuses', value: campuses.length.toString(), icon: Building2 },
                { label: 'Total Students', value: totalStudents.toString(), icon: Users },
                { label: 'Total Capacity', value: totalCapacity.toString(), icon: Target },
                { label: 'Network Budget', value: fmt(totalBudget), icon: IndianRupee },
                { label: 'Total Received', value: fmt(totalReceived), icon: TrendingUp },
              ].map((item, i) => (
                <motion.div
                  key={item.label}
                  initial={{ opacity: 0, scale: 0.85 }}
                  animate={{ opacity: 1, scale: 1 }}
                  transition={{ delay: 0.35 + i * 0.07 }}
                  className="bg-white/10 backdrop-blur rounded-2xl px-4 py-3 flex items-center gap-3"
                >
                  <item.icon className="w-5 h-5 text-teal-200 shrink-0" />
                  <div>
                    <p className="text-[10px] uppercase font-bold opacity-70 tracking-wider">{item.label}</p>
                    <p className="text-lg font-extrabold leading-tight">{item.value}</p>
                  </div>
                </motion.div>
              ))}
            </motion.div>
          )}
        </div>
      </div>

      {/* Campus cards */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        {loading ? (
          <div className="flex flex-col items-center justify-center py-28 gap-4">
            <motion.div
              animate={{ rotate: 360 }}
              transition={{ duration: 1.2, repeat: Infinity, ease: 'linear' }}
              className="w-12 h-12 rounded-full border-4 border-primary-200 border-t-primary-600"
            />
            <p className="text-gray-500 font-semibold">Loading campuses…</p>
          </div>
        ) : campuses.length === 0 ? (
          <div className="text-center py-20 text-gray-400 font-semibold">No campuses found.</div>
        ) : (
          <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
            {campuses.map((campus, i) => (
              <CampusCard key={campus.id} campus={campus} index={i} />
            ))}
          </div>
        )}

        {/* Overall spending summary */}
        {!loading && campuses.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.6 }}
            className="mt-10 bg-white rounded-3xl border border-gray-100 shadow-lg shadow-gray-100 p-6"
          >
            <h3 className="text-lg font-extrabold text-gray-800 mb-4 flex items-center gap-2">
              <Target className="w-5 h-5 text-primary-600" />
              Network-Wide Financial Overview
            </h3>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
              <div className="p-4 rounded-2xl bg-emerald-50 border border-emerald-100">
                <p className="text-xs font-bold text-emerald-600 uppercase tracking-wider">Total Received</p>
                <p className="text-2xl font-extrabold text-emerald-700 mt-1">{fmt(totalReceived)}</p>
              </div>
              <div className="p-4 rounded-2xl bg-red-50 border border-red-100">
                <p className="text-xs font-bold text-red-600 uppercase tracking-wider">Total Spent</p>
                <p className="text-2xl font-extrabold text-red-700 mt-1">{fmt(totalSpent)}</p>
              </div>
              <div className={`p-4 rounded-2xl border ${totalReceived - totalSpent >= 0 ? 'bg-green-50 border-green-100' : 'bg-rose-50 border-rose-100'}`}>
                <p className={`text-xs font-bold uppercase tracking-wider ${totalReceived - totalSpent >= 0 ? 'text-green-600' : 'text-rose-600'}`}>
                  Net Balance
                </p>
                <p className={`text-2xl font-extrabold mt-1 ${totalReceived - totalSpent >= 0 ? 'text-green-700' : 'text-rose-700'}`}>
                  {fmt(Math.abs(totalReceived - totalSpent))}
                  <span className="text-sm font-bold ml-1">{totalReceived - totalSpent >= 0 ? '▲ Surplus' : '▼ Deficit'}</span>
                </p>
              </div>
            </div>
          </motion.div>
        )}
      </div>
    </div>
  );
};

export default CampusesPage;
