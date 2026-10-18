import React, { useEffect, useState } from 'react';
import { financeService } from '../services/financeService';
import type { FinanceSummary, ExpenseItem, DonationItem } from '../types/finance';
import { Navbar } from '../components/Navbar';
import { 
  Wallet, 
  TrendingUp, 
  TrendingDown, 
  Receipt, 
  Search, 
  HeartHandshake, 
  ShieldCheck, 
  PieChart, 
  Calendar,
  Building2,
  CheckCircle2,
  AlertCircle,
  FileText
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

const PublicTransparency: React.FC = () => {
  const [summary, setSummary] = useState<FinanceSummary | null>(null);
  const [expenses, setExpenses] = useState<ExpenseItem[]>([]);
  const [donations, setDonations] = useState<DonationItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState('All');
  const [searchTerm, setSearchTerm] = useState('');
  const [activeTab, setActiveTab] = useState<'expenses' | 'donations'>('expenses');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [sumRes, expRes, donRes] = await Promise.all([
        financeService.getSummary(),
        financeService.getExpenses(),
        financeService.getDonations()
      ]);
      setSummary(sumRes);
      setExpenses(expRes);
      setDonations(donRes);
    } catch (err) {
      console.error('Failed to load financial transparency data', err);
    } finally {
      setLoading(false);
    }
  };

  const filteredExpenses = expenses.filter(exp => {
    const matchesCategory = selectedCategory === 'All' || exp.category === selectedCategory;
    const matchesSearch = searchTerm === '' || 
      exp.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      exp.receiptNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      exp.spentBy.toLowerCase().includes(searchTerm.toLowerCase()) ||
      exp.campusName.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesCategory && matchesSearch;
  });

  const categories = ['All', ...new Set(expenses.map(e => e.category))];

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-50 via-white to-primary-50/20 flex flex-col antialiased">
      <Navbar />

      <main className="flex-1 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10 sm:py-14 w-full">
        {/* Page Hero */}
        <motion.div 
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="text-center mb-12"
        >
          <div className="inline-flex items-center gap-2 bg-emerald-100/80 text-emerald-800 text-xs font-black uppercase tracking-widest px-4 py-1.5 rounded-full mb-3 border border-emerald-200">
            <ShieldCheck className="w-4 h-4 text-emerald-600" /> 100% Public Financial Accountability
          </div>
          <h1 className="text-3xl sm:text-5xl font-black text-gray-900 tracking-tight">
            Accounts & Expenditure Ledger
          </h1>
          <p className="mt-3 text-sm sm:text-base text-gray-600 max-w-2xl mx-auto leading-relaxed">
            Every rupee entrusted to Madrasa Siddeeqiyah is recorded openly. Inspect all purchases, operational expenses, donations received, and current financial balance.
          </p>
        </motion.div>

        {/* 4 Core Financial KPI Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-12">
          {/* Total Received */}
          <motion.div 
            whileHover={{ y: -4 }}
            className="bg-white p-6 rounded-3xl shadow-lg shadow-emerald-100/50 border border-emerald-100 relative overflow-hidden"
          >
            <div className="flex items-center justify-between mb-4">
              <span className="text-xs font-extrabold uppercase tracking-wider text-emerald-700">Total Funds Collected</span>
              <div className="p-2.5 bg-emerald-100 text-emerald-700 rounded-2xl">
                <TrendingUp className="w-5 h-5" />
              </div>
            </div>
            <div className="text-3xl font-black text-gray-900 font-mono tracking-tight">
              ₹{(summary?.totalDonationsReceived ?? 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}
            </div>
            <p className="text-xs text-gray-500 mt-2 flex items-center gap-1 font-medium">
              <CheckCircle2 className="w-3.5 h-3.5 text-emerald-600" />
              From {donations.length} public donations & zakat funds
            </p>
          </motion.div>

          {/* Total Spent */}
          <motion.div 
            whileHover={{ y: -4 }}
            className="bg-white p-6 rounded-3xl shadow-lg shadow-rose-100/50 border border-rose-100 relative overflow-hidden"
          >
            <div className="flex items-center justify-between mb-4">
              <span className="text-xs font-extrabold uppercase tracking-wider text-rose-700">Total Money Spent</span>
              <div className="p-2.5 bg-rose-100 text-rose-700 rounded-2xl">
                <TrendingDown className="w-5 h-5" />
              </div>
            </div>
            <div className="text-3xl font-black text-gray-900 font-mono tracking-tight">
              ₹{(summary?.totalExpensesSpent ?? 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}
            </div>
            <p className="text-xs text-gray-500 mt-2 flex items-center gap-1 font-medium">
              <Receipt className="w-3.5 h-3.5 text-rose-600" />
              Across {expenses.length} itemized verified purchases
            </p>
          </motion.div>

          {/* Current Balance */}
          <motion.div 
            whileHover={{ y: -4 }}
            className="bg-white p-6 rounded-3xl shadow-lg shadow-primary-100/50 border border-primary-100 relative overflow-hidden"
          >
            <div className="flex items-center justify-between mb-4">
              <span className="text-xs font-extrabold uppercase tracking-wider text-primary-700">Cash Balance On Hand</span>
              <div className="p-2.5 bg-primary-100 text-primary-700 rounded-2xl">
                <Wallet className="w-5 h-5" />
              </div>
            </div>
            <div className="text-3xl font-black text-primary-900 font-mono tracking-tight">
              ₹{(summary?.netBalance ?? 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}
            </div>
            <p className="text-xs text-gray-500 mt-2 font-medium">
              Available liquid funds for student rations & upkeep
            </p>
          </motion.div>

          {/* Annual Target / Budget Need */}
          <motion.div 
            whileHover={{ y: -4 }}
            className="bg-white p-6 rounded-3xl shadow-lg shadow-amber-100/50 border border-amber-100 relative overflow-hidden"
          >
            <div className="flex items-center justify-between mb-4">
              <span className="text-xs font-extrabold uppercase tracking-wider text-amber-700">Budget Needed</span>
              <div className="p-2.5 bg-amber-100 text-amber-700 rounded-2xl">
                <AlertCircle className="w-5 h-5" />
              </div>
            </div>
            <div className="text-3xl font-black text-amber-900 font-mono tracking-tight">
              ₹{(summary?.remainingBudgetNeeded ?? 0).toLocaleString('en-IN', { minimumFractionDigits: 2 })}
            </div>
            <p className="text-xs text-gray-500 mt-2 font-medium">
              Remaining target to fulfill ₹{(summary?.annualBudgetGoal ?? 0).toLocaleString('en-IN')} annual budget
            </p>
          </motion.div>
        </div>

        {/* Category Spending Breakdown Visuals */}
        <div className="bg-white rounded-3xl p-6 sm:p-8 shadow-xl shadow-slate-200/50 border border-gray-100 mb-12">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6 pb-4 border-b border-gray-100">
            <div>
              <h2 className="text-xl font-bold text-gray-900 flex items-center gap-2">
                <PieChart className="w-5 h-5 text-primary-600" /> Expenditure Categories & Percentage
              </h2>
              <p className="text-xs text-gray-500 mt-0.5">Where every single rupee has been allocated this session</p>
            </div>
            <div className="text-xs font-bold text-gray-600 bg-gray-50 px-3.5 py-1.5 rounded-xl border border-gray-200">
              Session 2026 - 2027
            </div>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {summary?.categoryBreakdown.map((cat, idx) => (
              <motion.div 
                key={cat.category}
                initial={{ opacity: 0, x: -10 }}
                animate={{ opacity: 1, x: 0 }}
                transition={{ delay: idx * 0.05 }}
                className="space-y-2 p-3.5 rounded-2xl bg-slate-50/70 border border-slate-100"
              >
                <div className="flex justify-between items-center text-xs font-bold">
                  <span className="text-gray-800">{cat.category}</span>
                  <span className="font-mono text-gray-900">
                    ₹{cat.amount.toLocaleString('en-IN')} ({cat.percentage}%)
                  </span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-3 overflow-hidden">
                  <motion.div 
                    initial={{ width: 0 }}
                    animate={{ width: `${Math.min(100, cat.percentage)}%` }}
                    transition={{ duration: 0.8, delay: 0.2 + idx * 0.05 }}
                    className="h-full bg-gradient-to-r from-primary-600 to-teal-500 rounded-full"
                  />
                </div>
              </motion.div>
            ))}
          </div>
        </div>

        {/* Switcher & Search Section */}
        <div className="bg-white rounded-3xl p-6 sm:p-8 shadow-xl shadow-slate-200/50 border border-gray-100">
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
            {/* Tabs */}
            <div className="inline-flex bg-gray-100 p-1.5 rounded-2xl gap-1">
              <button
                onClick={() => setActiveTab('expenses')}
                className={`px-5 py-2 rounded-xl text-xs sm:text-sm font-bold flex items-center gap-2 transition ${
                  activeTab === 'expenses'
                    ? 'bg-white text-gray-900 shadow-xs'
                    : 'text-gray-500 hover:text-gray-800'
                }`}
              >
                <Receipt className="w-4 h-4 text-rose-500" />
                Purchases & Expenses ({expenses.length})
              </button>
              <button
                onClick={() => setActiveTab('donations')}
                className={`px-5 py-2 rounded-xl text-xs sm:text-sm font-bold flex items-center gap-2 transition ${
                  activeTab === 'donations'
                    ? 'bg-white text-gray-900 shadow-xs'
                    : 'text-gray-500 hover:text-gray-800'
                }`}
              >
                <HeartHandshake className="w-4 h-4 text-emerald-500" />
                Funds Received & Donors ({donations.length})
              </button>
            </div>

            {/* Search Input */}
            <div className="relative w-full md:w-80">
              <Search className="w-4 h-4 text-gray-400 absolute left-3.5 top-1/2 -translate-y-1/2 pointer-events-none" />
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search description, receipt #, or campus..."
                className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 text-xs sm:text-sm focus:ring-2 focus:ring-primary-500 focus:border-transparent outline-none bg-gray-50/60 focus:bg-white"
              />
            </div>
          </div>

          {/* Category Filter Pills (For Expenses Tab) */}
          {activeTab === 'expenses' && (
            <div className="flex items-center gap-2 overflow-x-auto pb-3 mb-6 scrollbar-none">
              <span className="text-xs font-bold text-gray-400 uppercase tracking-wider shrink-0 mr-1">Category:</span>
              {categories.map((c) => (
                <button
                  key={c}
                  onClick={() => setSelectedCategory(c)}
                  className={`px-3.5 py-1.5 rounded-full text-xs font-bold transition shrink-0 ${
                    selectedCategory === c
                      ? 'bg-primary-600 text-white shadow-xs'
                      : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                  }`}
                >
                  {c}
                </button>
              ))}
            </div>
          )}

          {/* Content Tables */}
          {loading ? (
            <div className="p-16 flex items-center justify-center">
              <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-600"></div>
            </div>
          ) : activeTab === 'expenses' ? (
            /* Expenses Table */
            <div className="overflow-x-auto">
              <table className="w-full text-left">
                <thead className="bg-slate-50/80 border-b border-gray-100">
                  <tr>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase">Item / Purpose</th>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase">Category</th>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase">Campus</th>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase">Voucher #</th>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase">Date</th>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase text-right">Cost (Amount)</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 text-xs sm:text-sm">
                  {filteredExpenses.length === 0 ? (
                    <tr>
                      <td colSpan={6} className="px-5 py-12 text-center text-gray-400">
                        No expenses match your search query.
                      </td>
                    </tr>
                  ) : (
                    filteredExpenses.map((exp) => (
                      <tr key={exp.id} className="hover:bg-slate-50/70 transition">
                        <td className="px-5 py-4">
                          <div className="font-bold text-gray-900">{exp.title}</div>
                          <div className="text-xs text-gray-500 mt-0.5">{exp.description}</div>
                          <div className="text-[11px] text-gray-400 mt-0.5">Approved by: {exp.spentBy}</div>
                        </td>
                        <td className="px-5 py-4">
                          <span className="inline-block px-2.5 py-1 rounded-full text-xs font-bold bg-primary-50 text-primary-700 border border-primary-100">
                            {exp.category}
                          </span>
                        </td>
                        <td className="px-5 py-4 text-xs font-medium text-gray-600">
                          <div className="flex items-center gap-1.5">
                            <Building2 className="w-3.5 h-3.5 text-gray-400 shrink-0" />
                            {exp.campusName}
                          </div>
                        </td>
                        <td className="px-5 py-4 font-mono text-xs text-gray-500 font-bold">
                          {exp.receiptNumber}
                        </td>
                        <td className="px-5 py-4 text-xs text-gray-500">
                          {new Date(exp.date).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' })}
                        </td>
                        <td className="px-5 py-4 font-mono font-black text-rose-600 text-right text-base">
                          ₹{exp.amount.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          ) : (
            /* Donations Table */
            <div className="overflow-x-auto">
              <table className="w-full text-left">
                <thead className="bg-slate-50/80 border-b border-gray-100">
                  <tr>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase">Donor Name</th>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase">Donation Purpose</th>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase">Campus Target</th>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase">Receipt #</th>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase">Date</th>
                    <th className="px-5 py-3.5 text-xs font-bold text-gray-500 uppercase text-right">Amount Received</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 text-xs sm:text-sm">
                  {donations.map((d) => (
                    <tr key={d.id} className="hover:bg-slate-50/70 transition">
                      <td className="px-5 py-4">
                        <div className="font-bold text-gray-900">{d.donorName}</div>
                        <div className="text-[11px] text-emerald-600 font-semibold flex items-center gap-1">
                          <CheckCircle2 className="w-3 h-3" /> Official Receipt Issued
                        </div>
                      </td>
                      <td className="px-5 py-4">
                        <span className="inline-block px-2.5 py-1 rounded-full text-xs font-bold bg-emerald-50 text-emerald-800 border border-emerald-100">
                          {d.purpose}
                        </span>
                      </td>
                      <td className="px-5 py-4 text-xs font-medium text-gray-600">
                        {d.campusName}
                      </td>
                      <td className="px-5 py-4 font-mono text-xs text-gray-500 font-bold">
                        {d.receiptNumber}
                      </td>
                      <td className="px-5 py-4 text-xs text-gray-500">
                        {new Date(d.date).toLocaleDateString('en-IN', { day: 'numeric', month: 'short', year: 'numeric' })}
                      </td>
                      <td className="px-5 py-4 font-mono font-black text-emerald-600 text-right text-base">
                        +₹{d.amount.toLocaleString('en-IN', { minimumFractionDigits: 2 })}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </main>

      <footer className="mt-12 py-8 bg-white border-t border-gray-200 text-center text-xs text-gray-400">
        <p>&copy; 2026 Madrasa Siddeeqiyah. Public Financial Transparency Portal.</p>
        <p className="mt-1">All audit books are independently verified and available upon request.</p>
      </footer>
    </div>
  );
};

export default PublicTransparency;
