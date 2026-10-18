import React, { useEffect, useState } from 'react';
import { studentService } from '../services/studentService';
import { authService } from '../services/authService';
import { useNavigate } from 'react-router-dom';
import type { Student, StudentSummary } from '../types/student';
import {
  LogOut,
  Search,
  CheckCircle,
  XCircle,
  Clock,
  Users,
  Calendar,
  Phone,
  BookOpen,
  Home,
  Download,
  Trash2,
  Edit3,
  ShieldCheck,
} from 'lucide-react';

const AdminDashboard: React.FC = () => {
  const [students, setStudents] = useState<Student[]>([]);
  const [summary, setSummary] = useState<StudentSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('All');
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(0);
  const [currentUser, setCurrentUser] = useState<{ username: string; role: string } | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    setCurrentUser(authService.getCurrentUser());
    loadDashboard();
  }, []);

  useEffect(() => {
    loadDashboard();
  }, [searchTerm, statusFilter, pageNumber, pageSize]);

  const loadDashboard = async () => {
    setLoading(true);
    try {
      const [summaryData, pagedData] = await Promise.all([
        studentService.getSummary(),
        studentService.getAll({ searchTerm, status: statusFilter, pageNumber, pageSize }),
      ]);

      setSummary(summaryData);
      setStudents(pagedData.items);
      setTotalPages(pagedData.totalPages);
    } catch (err) {
      console.error('Failed to load dashboard', err);
      setStudents([]);
      setSummary(null);
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    authService.logout();
    navigate('/login');
  };

  const handleStatusUpdate = async (id: number, status: string) => {
    try {
      await studentService.updateStatus(id, { status });
      loadDashboard();
    } catch (err) {
      alert('Failed to update status');
      console.error(err);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Delete this application? This cannot be undone.')) return;

    try {
      await studentService.delete(id);
      loadDashboard();
    } catch (err) {
      alert('Failed to delete application');
      console.error(err);
    }
  };

  const handleExport = async () => {
    try {
      const blob = await studentService.exportCsv({ searchTerm, status: statusFilter });
      const downloadUrl = window.URL.createObjectURL(blob);
      const anchor = document.createElement('a');
      anchor.href = downloadUrl;
      anchor.download = 'student-applications.csv';
      anchor.click();
      window.URL.revokeObjectURL(downloadUrl);
    } catch (err) {
      alert('Unable to export applications at this time.');
      console.error(err);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Approved':
        return 'bg-green-100 text-green-700 border-green-200';
      case 'Rejected':
        return 'bg-red-100 text-red-700 border-red-200';
      default:
        return 'bg-yellow-100 text-yellow-700 border-yellow-200';
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 flex">
      <div className="w-64 bg-white border-r border-gray-200 flex flex-col hidden md:flex">
        <div className="p-6 border-b border-gray-100">
          <h1 className="text-xl font-bold text-primary-600 flex items-center gap-2">
            <Users className="w-6 h-6" /> Madrasa Admin
          </h1>
          <p className="mt-2 text-sm text-gray-500">{currentUser?.username} • {currentUser?.role}</p>
        </div>
        <nav className="flex-1 p-4 space-y-2">
          <button
            onClick={() => navigate('/admin')}
            className="w-full flex items-center gap-3 px-4 py-3 bg-primary-50 text-primary-700 rounded-xl font-medium transition-all"
          >
            <Users className="w-5 h-5" /> Applications
          </button>
          <button
            onClick={() => navigate('/admin/users')}
            className="w-full flex items-center gap-3 px-4 py-3 text-gray-700 rounded-xl hover:bg-gray-100 transition-all"
          >
            <ShieldCheck className="w-5 h-5" /> User Accounts
          </button>
        </nav>
        <div className="p-4 border-t border-gray-100">
          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-4 py-3 text-gray-600 hover:bg-red-50 hover:text-red-600 rounded-xl font-medium transition-all"
          >
            <LogOut className="w-5 h-5" /> Logout
          </button>
        </div>
      </div>

      <div className="flex-1 flex flex-col">
        <header className="h-20 bg-white border-b border-gray-200 px-8 flex flex-col justify-center gap-4 md:flex-row md:items-center md:justify-between">
          <div>
            <h2 className="text-2xl font-bold text-gray-800">Student Applications</h2>
            <p className="text-sm text-gray-500">Review submissions, export reports, and manage statuses.</p>
          </div>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
            <div className="relative w-full sm:w-72">
              <Search className="w-5 h-5 absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                placeholder="Search name or phone..."
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setPageNumber(1);
                }}
                className="pl-10 pr-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-primary-500 outline-none w-full"
              />
            </div>
            <select
              value={statusFilter}
              onChange={(e) => {
                setStatusFilter(e.target.value);
                setPageNumber(1);
              }}
              className="px-4 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-primary-500 outline-none"
            >
              <option value="All">All Status</option>
              <option value="Pending">Pending</option>
              <option value="Approved">Approved</option>
              <option value="Rejected">Rejected</option>
            </select>
            <button
              onClick={handleExport}
              className="inline-flex items-center gap-2 px-4 py-2 bg-primary-600 text-white rounded-xl shadow-lg hover:bg-primary-700 transition-all"
            >
              <Download className="w-4 h-4" /> Export CSV
            </button>
          </div>
        </header>

        <main className="p-8 flex-1 overflow-auto">
          <div className="grid gap-5 mb-8 md:grid-cols-4">
            <div className="bg-white rounded-3xl border border-gray-200 p-6 shadow-sm">
              <p className="text-sm text-gray-500">Total Applications</p>
              <p className="mt-4 text-3xl font-bold text-gray-900">{summary?.totalApplications ?? 0}</p>
            </div>
            <div className="bg-white rounded-3xl border border-gray-200 p-6 shadow-sm">
              <p className="text-sm text-gray-500">Pending</p>
              <p className="mt-4 text-3xl font-bold text-yellow-700">{summary?.pending ?? 0}</p>
            </div>
            <div className="bg-white rounded-3xl border border-gray-200 p-6 shadow-sm">
              <p className="text-sm text-gray-500">Approved</p>
              <p className="mt-4 text-3xl font-bold text-green-700">{summary?.approved ?? 0}</p>
            </div>
            <div className="bg-white rounded-3xl border border-gray-200 p-6 shadow-sm">
              <p className="text-sm text-gray-500">Rejected</p>
              <p className="mt-4 text-3xl font-bold text-red-700">{summary?.rejected ?? 0}</p>
            </div>
          </div>

          <div className="bg-white rounded-2xl shadow-sm border border-gray-200 overflow-hidden">
            <div className="px-8 py-6 border-b border-gray-200 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div>
                <h3 className="text-lg font-semibold text-gray-900">Applications</h3>
                <p className="text-sm text-gray-500">Showing page {pageNumber} of {totalPages || 1}</p>
              </div>
              <div className="flex flex-wrap items-center gap-3">
                <span className="text-sm text-gray-500">Rows per page:</span>
                <select
                  value={pageSize}
                  onChange={(e) => {
                    setPageSize(Number(e.target.value));
                    setPageNumber(1);
                  }}
                  className="px-3 py-2 border border-gray-200 rounded-xl focus:ring-2 focus:ring-primary-500 outline-none"
                >
                  <option value={10}>10</option>
                  <option value={20}>20</option>
                  <option value={50}>50</option>
                </select>
              </div>
            </div>

            {loading ? (
              <div className="p-16 flex items-center justify-center">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="min-w-full text-left">
                  <thead className="bg-gray-50 border-b border-gray-200">
                    <tr>
                      <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase">Student</th>
                      <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase">Contact</th>
                      <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase">Course</th>
                      <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase">Applied</th>
                      <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase">Status</th>
                      <th className="px-6 py-4 text-xs font-bold text-gray-500 uppercase">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {students.length === 0 ? (
                      <tr>
                        <td colSpan={6} className="px-6 py-16 text-center text-sm text-gray-500">
                          No applications found. Adjust the filters or wait for new submissions.
                        </td>
                      </tr>
                    ) : students.map((student) => (
                      <tr key={student.id} className="hover:bg-gray-50 transition-all">
                        <td className="px-6 py-5">
                          <div className="text-sm font-semibold text-gray-900">{student.fullName}</div>
                          <div className="text-xs text-gray-500">Father: {student.fatherName}</div>
                          {student.aadhaarNumber && (
                            <div className="mt-1.5 inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-lg bg-emerald-50 text-emerald-800 text-[11px] font-mono border border-emerald-200">
                              <ShieldCheck className="w-3 h-3 text-emerald-600" />
                              <span>{student.aadhaarNumber}</span>
                              {student.isAadhaarVerified && (
                                <span className="text-[9px] bg-emerald-200 text-emerald-900 px-1 py-0.2 rounded font-sans font-bold">
                                  UIDAI
                                </span>
                              )}
                            </div>
                          )}
                        </td>
                        <td className="px-6 py-5 text-sm text-gray-700">
                          <div className="flex items-center gap-2"><Phone className="w-4 h-4" /> {student.phone}</div>
                        </td>
                        <td className="px-6 py-5 text-sm text-gray-700">
                          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-slate-100 text-slate-600">
                            <BookOpen className="w-4 h-4" /> {student.course}
                          </div>
                          {student.hostelRequired && (
                            <div className="mt-2 inline-flex items-center gap-2 px-3 py-1 rounded-full bg-indigo-50 text-indigo-700 text-xs">
                              <Home className="w-3 h-3" /> Hostel needed
                            </div>
                          )}
                        </td>
                        <td className="px-6 py-5 text-sm text-gray-500 flex items-center gap-2">
                          <Calendar className="w-4 h-4" />
                          {new Date(student.applicationDate).toLocaleDateString()}
                        </td>
                        <td className="px-6 py-5">
                          <span className={`inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold border ${getStatusColor(student.status)}`}>
                            {student.status}
                          </span>
                        </td>
                        <td className="px-6 py-5 flex flex-wrap items-center gap-2">
                          {student.status === 'Pending' && (
                            <>
                              <button
                                onClick={() => handleStatusUpdate(student.id, 'Approved')}
                                className="inline-flex items-center justify-center w-10 h-10 rounded-xl bg-green-50 text-green-600 hover:bg-green-100 transition-all"
                                title="Approve"
                              >
                                <CheckCircle className="w-5 h-5" />
                              </button>
                              <button
                                onClick={() => handleStatusUpdate(student.id, 'Rejected')}
                                className="inline-flex items-center justify-center w-10 h-10 rounded-xl bg-red-50 text-red-600 hover:bg-red-100 transition-all"
                                title="Reject"
                              >
                                <XCircle className="w-5 h-5" />
                              </button>
                            </>
                          )}
                          <button
                            onClick={() => navigate(`/admin/students/${student.id}/edit`)}
                            className="inline-flex items-center justify-center w-10 h-10 rounded-xl bg-blue-50 text-blue-600 hover:bg-blue-100 transition-all"
                            title="Edit"
                          >
                            <Edit3 className="w-5 h-5" />
                          </button>
                          <button
                            onClick={() => handleDelete(student.id)}
                            className="inline-flex items-center justify-center w-10 h-10 rounded-xl bg-gray-50 text-gray-500 hover:bg-gray-100 transition-all"
                            title="Delete"
                          >
                            <Trash2 className="w-5 h-5" />
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          {!loading && students.length === 0 && (
            <div className="p-12 text-center">
              <Clock className="w-12 h-12 text-gray-300 mx-auto mb-4" />
              <p className="text-gray-500 font-medium">No student applications found matching your filters.</p>
            </div>
          )}

          <div className="mt-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div className="text-sm text-gray-600">
              Showing page {pageNumber} of {totalPages || 1}
            </div>
            <div className="flex items-center gap-3">
              <button
                disabled={pageNumber <= 1}
                onClick={() => setPageNumber((prev) => Math.max(prev - 1, 1))}
                className="px-4 py-2 rounded-xl border border-gray-200 bg-white text-sm font-medium text-gray-700 disabled:opacity-50"
              >
                Previous
              </button>
              <button
                disabled={pageNumber >= totalPages}
                onClick={() => setPageNumber((prev) => Math.min(prev + 1, totalPages))}
                className="px-4 py-2 rounded-xl border border-gray-200 bg-white text-sm font-medium text-gray-700 disabled:opacity-50"
              >
                Next
              </button>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
};

export default AdminDashboard;
